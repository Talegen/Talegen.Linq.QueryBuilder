/*
 *
 * (c) Copyright Talegen, LLC.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/

namespace Talegen.Linq.QueryBuilder
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    /// <summary>
    /// Contains an enumerated list of all the different comparisons which can be performed with LINQ expressions.
    /// </summary>
    public enum ComparisonOperatorType
    {
        /// <summary>
        /// Or comparison operator
        /// </summary>
        Or = ExpressionType.Or,

        /// <summary>
        /// And comparison operator
        /// </summary>
        And = ExpressionType.And,

        /// <summary>
        /// XOr comparison operator
        /// </summary>
        Xor = ExpressionType.ExclusiveOr,

        /// <summary>
        /// Not comparison operator
        /// </summary>
        Not = ExpressionType.Not,

        /// <summary>
        /// Equal comparison operator
        /// </summary>
        Equal = ExpressionType.Equal,

        /// <summary>
        /// Contains comparison operator
        /// </summary>
        Contains = ExpressionType.TypeIs + 1,

        /// <summary>
        /// Doesn't Contain comparison operator
        /// </summary>
        NotContains = ExpressionType.TypeIs + 2,

        /// <summary>
        /// Starts with comparison operator
        /// </summary>
        StartsWith = ExpressionType.TypeIs + 3,

        /// <summary>
        /// Does not Start with comparison operator
        /// </summary>
        NotStartsWith = ExpressionType.TypeIs + 4,

        /// <summary>
        /// Ends with comparison operator
        /// </summary>
        EndsWith = ExpressionType.TypeIs + 5,

        /// <summary>
        /// Does not end with comparison operator
        /// </summary>
        NotEndsWith = ExpressionType.TypeIs + 6,

        /// <summary>
        /// Contains comparison operator
        /// </summary>
        Like = ExpressionType.TypeIs + 7,

        /// <summary>
        /// Doesn't Contain comparison operator
        /// </summary>
        NotLike = ExpressionType.TypeIs + 8,

        /// <summary>
        /// Contains comparison operator
        /// </summary>
        LikeStartsWith = ExpressionType.TypeIs + 9,

        /// <summary>
        /// Doesn't Contain comparison operator
        /// </summary>
        LikeNotStartsWith = ExpressionType.TypeIs + 10,

        /// <summary>
        /// Contains comparison operator
        /// </summary>
        LikeEndsWith = ExpressionType.TypeIs + 11,

        /// <summary>
        /// Doesn't Contain comparison operator
        /// </summary>
        LikeNotEndsWith = ExpressionType.TypeIs + 12,

        /// <summary>
        /// Not Equal comparison operator
        /// </summary>
        NotEqual = ExpressionType.NotEqual,

        /// <summary>
        /// Or Else comparison operator
        /// </summary>
        OrElse = ExpressionType.OrElse,

        /// <summary>
        /// And Also comparison operator
        /// </summary>
        AndAlso = ExpressionType.AndAlso,

        /// <summary>
        /// Less Than comparison operator
        /// </summary>
        LessThan = ExpressionType.LessThan,

        /// <summary>
        /// Greater Than comparison operator
        /// </summary>
        GreaterThan = ExpressionType.GreaterThan,

        /// <summary>
        /// Less than or Equal comparison operator
        /// </summary>
        LessThanOrEqual = ExpressionType.LessThanOrEqual,

        /// <summary>
        /// Greater than or Equal comparison operator
        /// </summary>
        GreaterThanOrEqual = ExpressionType.GreaterThanOrEqual
    }

    /// <summary>
    /// This abstract class contains the base methods and properties for building a search condition lambda expression tree.
    /// </summary>
    public abstract class SearchCondition
    {
        #region Private Fields

        /// <summary>
        /// Contains a dictionary of expressions used to ensure we get the same instance of a particular ParameterExpression across multiple queries.
        /// </summary>
        private static readonly Dictionary<string, ParameterExpression> ParameterTable = new Dictionary<string, ParameterExpression>();

        // TODO: need to export search conditions to extensible option, and build add-ons for EntityFrameworkCore
        ///// <summary>
        ///// Contains a default column Like method used for searching.
        ///// </summary>
        /////private static readonly MethodInfo LikeMethod = typeof(DbFunctionsExtensions).GetMethod("Like", new[] { typeof(DbFunctions), typeof(string), typeof(string) });

        /// <summary>
        /// Contains a default string contains method used for searching.
        /// </summary>
        private static readonly MethodInfo ContainsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });

        /// <summary>
        /// Contains a default string starts with method used for searching.
        /// </summary>
        private static readonly MethodInfo StartsWithMethod = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });

        /// <summary>
        /// Contains a default string ends with method used for searching.
        /// </summary>
        private static readonly MethodInfo EndsWithMethod = typeof(string).GetMethod("EndsWith", new[] { typeof(string) });

        #endregion Private Fields

        #region Protected Properties

        /// <summary>
        /// Gets or sets expression tree which will be passed to the LINQ runtime.
        /// </summary>
        protected internal LambdaExpression InternalLambdaExpression { get; set; }

        #endregion Protected Properties

        #region Public Static Methods

        /// <summary>
        /// Creates a combined search condition containing the two specified search conditions.
        /// </summary>
        /// <typeparam name="T">Contains the type of the element the condition will execute against.</typeparam>
        /// <param name="firstCondition">Contains the first search condition to combine with.</param>
        /// <param name="comparisonOperator">Contains the comparison operator.</param>
        /// <param name="secondCondition">Contains the second search condition to combine with the first.</param>
        /// <returns>Returns a new <see cref="SearchCondition{T}" /> combined search condition.</returns>
        public static SearchCondition<T> Combine<T>(SearchCondition<T> firstCondition, ComparisonOperatorType comparisonOperator, SearchCondition<T> secondCondition)
        {
            return SearchCondition<T>.Combine(firstCondition, comparisonOperator, secondCondition);
        }

        #endregion Public Static Methods

        #region Protected Methods

        /// <summary>
        /// Combines two Expressions according to the specified operator comparisonOperator.
        /// </summary>
        /// <param name="left">Contains the left expression to combine.</param>
        /// <param name="comparisonOperator">Contains the comparison operator.</param>
        /// <param name="right">Contains the right expression to combine with the left.</param>
        /// <returns>Returns a new combined expression.</returns>
        protected static Expression CombineExpression(Expression left, ComparisonOperatorType comparisonOperator, Expression right)
        {
            // TODO: in order to support LIKE in an extensible way, we may want to make this method virtual or defined via interface

            Expression result;

            // Join the Expressions based on the operator
            switch (comparisonOperator)
            {
                case ComparisonOperatorType.Or:
                    result = Expression.Or(left, right);
                    break;

                case ComparisonOperatorType.And:
                    result = Expression.And(left, right);
                    break;

                case ComparisonOperatorType.Xor:
                    result = Expression.ExclusiveOr(left, right);
                    break;

                case ComparisonOperatorType.Equal:
                    result = Expression.Equal(left, right);
                    break;

                case ComparisonOperatorType.OrElse:
                    result = Expression.OrElse(left, right);
                    break;

                case ComparisonOperatorType.AndAlso:
                    result = Expression.AndAlso(left, right);
                    break;

                case ComparisonOperatorType.NotEqual:
                    result = Expression.NotEqual(left, right);
                    break;

                case ComparisonOperatorType.LessThan:
                    result = Expression.LessThan(left, right);
                    break;

                case ComparisonOperatorType.GreaterThan:
                    result = Expression.GreaterThan(left, right);
                    break;

                case ComparisonOperatorType.LessThanOrEqual:
                    result = Expression.LessThanOrEqual(left, right);
                    break;

                case ComparisonOperatorType.GreaterThanOrEqual:
                    result = Expression.GreaterThanOrEqual(left, right);
                    break;

                ////case ComparisonOperatorType.Like:
                ////case ComparisonOperatorType.LikeStartsWith:
                ////case ComparisonOperatorType.LikeEndsWith:
                ////    result = Expression.Call(null, LikeMethod, Expression.Constant(EF.Functions), left, right);
                ////    break;

                ////case ComparisonOperatorType.NotLike:
                ////case ComparisonOperatorType.LikeNotStartsWith:
                ////case ComparisonOperatorType.LikeNotEndsWith:
                ////    result = Expression.Not(Expression.Call(null, LikeMethod, Expression.Constant(EF.Functions), left, right));
                ////    break;

                case ComparisonOperatorType.Contains:
                    result = Expression.Call(left, ContainsMethod, right);
                    break;

                case ComparisonOperatorType.NotContains:
                    result = Expression.Not(Expression.Call(left, ContainsMethod, right));
                    break;

                case ComparisonOperatorType.StartsWith:
                    result = Expression.Call(left, StartsWithMethod, right);
                    break;

                case ComparisonOperatorType.NotStartsWith:
                    result = Expression.Not(Expression.Call(left, StartsWithMethod, right));
                    break;

                case ComparisonOperatorType.EndsWith:
                    result = Expression.Call(left, EndsWithMethod, right);
                    break;

                case ComparisonOperatorType.NotEndsWith:
                    result = Expression.Not(Expression.Call(left, StartsWithMethod, right));
                    break;

                default:
                    throw new ArgumentException(nameof(comparisonOperator));
            }

            return result;
        }

        /// <summary>
        /// This method is used to combine lambda functions.
        /// </summary>
        /// <typeparam name="T">Contains the type condition that will be executed against.</typeparam>
        /// <param name="left">Contains the left lambda comparison function to combine with.</param>
        /// <param name="comparisonOperator">Contains the comparison operator.</param>
        /// <param name="right">Contains the right lambda comparison function to combine with the left.</param>
        /// <returns>Returns a new combined lambda comparison function.</returns>
        protected static Func<T, bool> CombineFunc<T>(Func<T, bool> left, ComparisonOperatorType comparisonOperator, Func<T, bool> right)
        {
            Func<T, bool> result;

            // Return a delegate which combines delegates d1 and d2
            switch (comparisonOperator)
            {
                case ComparisonOperatorType.Or:
                    result = (x) => left(x) | right(x);
                    break;

                case ComparisonOperatorType.And:
                    result = (x) => left(x) & right(x);
                    break;

                case ComparisonOperatorType.Xor:
                    result = (x) => left(x) ^ right(x);
                    break;

                case ComparisonOperatorType.Equal:
                    result = (x) => left(x) == right(x);
                    break;

                case ComparisonOperatorType.OrElse:
                    result = (x) => left(x) || right(x);
                    break;

                case ComparisonOperatorType.AndAlso:
                    result = (x) => left(x) && right(x);
                    break;

                case ComparisonOperatorType.NotEqual:
                    result = (x) => left(x) != right(x);
                    break;

                case ComparisonOperatorType.LessThan:
                    result = (x) => int.Parse(left(x).ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture) < int.Parse(right(x).ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
                    break;

                case ComparisonOperatorType.GreaterThan:
                    result = (x) => int.Parse(left(x).ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture) > int.Parse(right(x).ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
                    break;

                case ComparisonOperatorType.LessThanOrEqual:
                    result = (x) => int.Parse(left(x).ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture) <= int.Parse(right(x).ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
                    break;

                case ComparisonOperatorType.GreaterThanOrEqual:
                    result = (x) => int.Parse(left(x).ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture) >= int.Parse(right(x).ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
                    break;

                default:
                    throw new ArgumentException(nameof(comparisonOperator));
            }

            return result;
        }

        /// <summary>
        /// Guarantees that we get the same instance of a ParameterExpression for a given type.
        /// </summary>
        /// <param name="dataType">Contains the data type to get a parameter expression for.</param>
        /// <returns>Returns a new parameter expression for the specified type.</returns>
        protected static ParameterExpression GetParamInstance(Type dataType)
        {
            if (dataType == null)
            {
                throw new ArgumentNullException(nameof(dataType));
            }

            // Parameters are matched by reference, not by name, so we cache the instances in a Dictionary.
            if (!ParameterTable.ContainsKey(dataType.Name))
            {
                ParameterTable.Add(dataType.Name, Expression.Parameter(dataType, dataType.Name));
            }

            return ParameterTable[dataType.Name];
        }

        #endregion
    }

    /// <summary>
    /// This class represents the resulting output of an additional conditions call
    /// </summary>
    public sealed class AdditionalConditionsResult
    {
        /// <summary>
        /// Gets or sets the property identifier.
        /// </summary>
        /// <value>The property identifier.</value>
        /// <remarks>E.g., prop.TagId</remarks>
        public MemberExpression PropId { get; set; }

        /// <summary>
        /// Gets or sets the property value expression.
        /// </summary>
        /// <value>The property value expression.</value>
        /// <remarks>E.g., prop.TagId == 3</remarks>
        public BinaryExpression PropValueExpression { get; set; }

        /// <summary>
        /// Gets or sets the property lambda.
        /// </summary>
        /// <value>The property lambda.</value>
        /// <remarks>E.g., prop =&gt; prop.TagId == 3</remarks>
        public Expression PropLambda { get; set; }
    }

    /// <summary>
    /// This delegate defines additional list conditions event to be used within <see cref="SearchCondition{T}" /> class.
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    /// <param name="prop">The property.</param>
    /// <param name="value">The value.</param>
    /// <returns>Returns the results of any additional conditional list logic.</returns>
    public delegate AdditionalConditionsResult AdditionalListConditionsDelegate(string propertyName, ParameterExpression prop, object value);

    /// <summary>
    /// This class represents a search condition within the ad-hoc query building engine.
    /// </summary>
    /// <typeparam name="T">Contains the entity type of the search condition.</typeparam>
    public class SearchCondition<T> : SearchCondition
    {
        #region Internal Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchCondition{T}" /> class.
        /// </summary>
        internal SearchCondition()
        {
        }

        /// <summary>
        /// If implemented, will be called for additional condition checks for list properties to enhance queries for specific data relationships.
        /// </summary>
        public event AdditionalListConditionsDelegate OnAddListConditions;

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchCondition{T}" /> class.
        /// </summary>
        /// <param name="propertyName">Contains the property name being compared.</param>
        /// <param name="comparisonOperator">Contains the comparison operator.</param>
        /// <param name="value">Contains the value to compare.</param>
        internal SearchCondition(string propertyName, ComparisonOperatorType comparisonOperator, object value)
        {
            if (!string.IsNullOrEmpty(propertyName))
            {
                // Split the string to handle nested property access
                var parameterParts = propertyName.Split('.');

                // Get the PropertyInfo instance for propName
                var propertyInfo = typeof(T).GetProperty(parameterParts[0]);
                var parameterExpression = GetParamInstance(typeof(T));
                var isList = propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.Name.Contains("List", StringComparison.Ordinal);

                if (isList)
                {
                    // e.g., "objectName.ListPropertyName"
                    MemberExpression props = Expression.Property(parameterExpression, propertyName);

                    // special handling of list (tags, properties)
                    var propType = propertyInfo.PropertyType.GenericTypeArguments.FirstOrDefault();

                    ConstantExpression valueExpression = Expression.Constant(value, value.GetType());

                    // prop =>
                    ParameterExpression prop = Expression.Parameter(propType, "prop");
                    MemberExpression propId = null;
                    BinaryExpression propValueExpression = null;
                    Expression propLambda = null;

                    // Any method
                    var anyMethod = typeof(Queryable).GetMethods().FirstOrDefault(method => method.Name == "Any" && method.GetParameters().Length == 2).MakeGenericMethod(propType);

                    // e.g., objectName.ListPropertyName.AsQueryable()
                    var asQueryableMethod = typeof(Queryable).GetMethods().FirstOrDefault(method => method.Name == "AsQueryable").MakeGenericMethod(propType);
                    var asQueryableCallMethod = Expression.Call(asQueryableMethod, props);

                    ConstantExpression constantExpression;

                    if (this.OnAddListConditions != null)
                    {
                        var additionalConditions = this.OnAddListConditions.Invoke(propertyName, prop, value);

                        if (additionalConditions != null)
                        {
                            propId = additionalConditions.PropId;
                            propValueExpression = additionalConditions.PropValueExpression;
                            propLambda = additionalConditions.PropLambda;
                        }
                    }

                    this.InternalLambdaExpression = comparisonOperator == ComparisonOperatorType.Equal ?

                        // E.g., objectName.ListPropertyName.AsQueryable().!Any(prop => prop.TagId == 3)
                        Expression.Lambda<Func<T, bool>>(Expression.Call(null, anyMethod, asQueryableCallMethod, propLambda), parameterExpression) :

                        // E.g., objectName.ListPropertyName.AsQueryable().Any(prop => prop.TagId == 3)
                        Expression.Lambda<Func<T, bool>>(Expression.Not(Expression.Call(null, anyMethod, asQueryableCallMethod, propLambda)), parameterExpression);
                }
                else
                {
                    var callExpression = Expression.MakeMemberAccess(parameterExpression, propertyInfo);

                    // For each member specified, construct the additional MemberAccessExpression.
                    // For example, if the user says "myCustomer.Order.OrderID = 4" we need an  additional MemberAccessExpression for "Order.OrderID = 4"
                    for (var i = 1; i <= parameterParts.GetUpperBound(0); i++)
                    {
                        if (propertyInfo != null)
                        {
                            propertyInfo = propertyInfo.PropertyType.GetProperty(parameterParts[i]);

                            if (propertyInfo != null)
                            {
                                callExpression = Expression.MakeMemberAccess(callExpression, propertyInfo);
                            }
                        }
                    }

                    // ConstantExpression representing the value on the left side of the operator
                    if (propertyInfo != null)
                    {
                        // if we're querying a date, and using equal or not equal operators, we must strip off the time of the comparison data so we can get an
                        // exact match to the date specified in the query date picker.
                        if (propertyInfo.PropertyType == typeof(DateTime))
                        {
                            MemberExpression property = Expression.Property(parameterExpression, propertyName);
                            MemberExpression dateOnlyProperty = Expression.Property(property, "Date");
                            ConstantExpression valueExpression = Expression.Constant(value, propertyInfo.PropertyType);
                            var dateComboExpression = CombineExpression(dateOnlyProperty, comparisonOperator, valueExpression);

                            this.InternalLambdaExpression = Expression.Lambda<Func<T, bool>>(dateComboExpression, parameterExpression);
                        }
                        else if (propertyInfo.PropertyType == typeof(DateTime?))
                        {
                            MemberExpression property = Expression.Property(parameterExpression, propertyName);
                            MemberExpression valueOnlyProperty = Expression.Property(property, "Value");
                            MemberExpression dateOnlyProperty = Expression.Property(valueOnlyProperty, "Date");

                            ConstantExpression nullValue = Expression.Constant(null, propertyInfo.PropertyType);
                            ConstantExpression valueExpression = Expression.Constant(value, typeof(DateTime));

                            var dateComboExpression = CombineExpression(property, ComparisonOperatorType.NotEqual, nullValue);
                            var dateComboExpression2 = CombineExpression(dateOnlyProperty, comparisonOperator, valueExpression);

                            Expression andExpression = Expression.AndAlso(dateComboExpression, dateComboExpression2);
                            this.InternalLambdaExpression = Expression.Lambda<Func<T, bool>>(andExpression, parameterExpression);
                        }
                        else
                        {
                            // otherwise, do typical query expression build check for enums
                            if (propertyInfo.PropertyType.IsEnum)
                            {
                                value = Enum.Parse(propertyInfo.PropertyType, value.ToString());
                            }

                            var valueExpression = Expression.Constant(value, propertyInfo.PropertyType);
                            Expression comboExpression = CombineExpression(callExpression, comparisonOperator, valueExpression);
                            this.InternalLambdaExpression = Expression.Lambda<Func<T, bool>>(comboExpression, parameterExpression);
                        }
                    }
                }

                // Prevent crash for query on invalid properties
                if (this.InternalLambdaExpression != null)
                {
                    // Compile the lambda expression into a delegate
                    this.LocalFunctionDelegate = (Func<T, bool>)this.InternalLambdaExpression.Compile();
                }
            }
        }

        #endregion

        #region Internal Properties

        /// <summary>
        /// Gets or sets a delegate that holds a compiled expression tree which can be run locally.
        /// </summary>
        internal Func<T, bool> LocalFunctionDelegate { get; set; }

        #endregion

        #region Static Overloaded Operators

        /// <summary>
        /// Operator is used to combine search conditions using the or operator.
        /// </summary>
        /// <param name="firstCondition">Contains the first condition to allow or grouping operation.</param>
        /// <param name="secondCondition">Contains the second condition to allow or grouping operation.</param>
        /// <returns>Returns the search condition result of the combined operation.</returns>
        public static SearchCondition<T> operator |(SearchCondition<T> firstCondition, SearchCondition<T> secondCondition)
        {
            return SearchCondition.Combine(firstCondition, ComparisonOperatorType.OrElse, secondCondition);
        }

        /// <summary>
        /// Operator is used to combine search conditions using the And operator.
        /// </summary>
        /// <param name="firstCondition">Contains the first condition to allow and grouping operation.</param>
        /// <param name="secondCondition">Contains the second condition to allow and grouping operation.</param>
        /// <returns>Returns the search condition result of the combined operation.</returns>
        public static SearchCondition<T> operator &(SearchCondition<T> firstCondition, SearchCondition<T> secondCondition)
        {
            return SearchCondition.Combine(firstCondition, ComparisonOperatorType.AndAlso, secondCondition);
        }

        /// <summary>
        /// Operator is used to combine search conditions using the And operator.
        /// </summary>
        /// <param name="firstCondition">Contains the first condition to allow and grouping operation.</param>
        /// <param name="secondCondition">Contains the second condition to allow and grouping operation.</param>
        /// <returns>Returns the search condition result of the combined operation.</returns>
        [SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "Annoying.")]
        public static SearchCondition<T> BitwiseAnd(SearchCondition<T> firstCondition, SearchCondition<T> secondCondition)
        {
            return SearchCondition.Combine(firstCondition, ComparisonOperatorType.AndAlso, secondCondition);
        }

        /// <summary>
        /// Operator is used to combine search conditions using the or operator.
        /// </summary>
        /// <param name="firstCondition">Contains the first condition to allow or grouping operation.</param>
        /// <param name="secondCondition">Contains the second condition to allow or grouping operation.</param>
        /// <returns>Returns the search condition result of the combined operation.</returns>
        [SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "Annoying.")]
        public static SearchCondition<T> BitwiseOr(SearchCondition<T> firstCondition, SearchCondition<T> secondCondition)
        {
            return SearchCondition.Combine(firstCondition, ComparisonOperatorType.OrElse, secondCondition);
        }

        #endregion

        #region Internal Static Methods

        /// <summary>
        /// Combines two conditions according to the specified operator.
        /// </summary>
        /// <param name="firstCondition">Contains the first search condition to combine.</param>
        /// <param name="comparisonOperator">Contains the comparison operator to use to combine search conditions.</param>
        /// <param name="secondCondition">Contains the second search condition to combine with the first.</param>
        /// <returns>Returns a new combined <see cref="SearchCondition{T}" /> object.</returns>
        [SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "Annoying.")]
        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1126:PrefixCallsCorrectly", Justification = "Reviewed. False positive.")]
        public static SearchCondition<T> Combine(SearchCondition<T> firstCondition, ComparisonOperatorType comparisonOperator, SearchCondition<T> secondCondition)
        {
            if (firstCondition == null)
            {
                throw new ArgumentNullException(nameof(firstCondition));
            }

            if (secondCondition == null)
            {
                throw new ArgumentNullException(nameof(secondCondition));
            }

            SearchCondition<T> condition = new SearchCondition<T>();
            Expression combinedExpression = CombineExpression(firstCondition.InternalLambdaExpression.Body, comparisonOperator, secondCondition.InternalLambdaExpression.Body);
            var parameterExpressions = new[] { GetParamInstance(typeof(T)) };

            // Create the LambdaExpression and compile the delegate
            condition.InternalLambdaExpression = Expression.Lambda<Func<T, bool>>(combinedExpression, parameterExpressions);
            condition.LocalFunctionDelegate = CombineFunc(firstCondition.LocalFunctionDelegate, comparisonOperator, secondCondition.LocalFunctionDelegate);

            return condition;
        }

        /// <summary>
        /// Combines multiple conditions according to the specified operator.
        /// </summary>
        /// <param name="firstCondition">Contains the first search condition to combine.</param>
        /// <param name="comparisonOperator">Contains the comparison operator to use to combine search conditions.</param>
        /// <param name="conditions">Contains a parameter array of search conditions to combine with the first condition.</param>
        /// <returns>Returns a new combined <see cref="SearchCondition{T}" /> object.</returns>
        public static SearchCondition<T> Combine(SearchCondition<T> firstCondition, ComparisonOperatorType comparisonOperator, params SearchCondition<T>[] conditions)
        {
            if (conditions == null)
            {
                throw new ArgumentNullException(nameof(conditions));
            }

            var finalCondition = firstCondition;

            foreach (var c in conditions)
            {
                finalCondition = SearchCondition.Combine(finalCondition, comparisonOperator, c);
            }

            return finalCondition;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Run query locally instead of remotely.
        /// </summary>
        /// <param name="row">Contains a search row of the specified element type.</param>
        /// <returns>Returns a match result for the search criteria.</returns>
        public bool Matches(T row)
        {
            // Passes the row into the delegate to see if it's a match
            return this.LocalFunctionDelegate(row);
        }

        #endregion
    }

    /// <summary>
    /// This class represents a search condition within the ad-hoc query building engine.
    /// </summary>
    /// <typeparam name="T">Contains the entity type of the search condition.</typeparam>
    /// <typeparam name="TType">Contains the type of the comparison value.</typeparam>
    public class SearchCondition<T, TType> : SearchCondition<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SearchCondition{T, TType}" /> class.
        /// </summary>
        /// <param name="propertyName">Contains the property name being compared.</param>
        /// <param name="comparisonOperator">Contains the comparison operator.</param>
        /// <param name="value">Contains the value to compare.</param>
        internal SearchCondition(string propertyName, ComparisonOperatorType comparisonOperator, TType value)
            : base(propertyName, comparisonOperator, value)
        {
        }
    }
}