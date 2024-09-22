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
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using Talegen.Common.Core.Extensions;
    using Talegen.Linq.QueryBuilder;
    using Talegen.Linq.QueryBuilder.Configuration;
    using Talegen.Linq.QueryBuilder.Models;

    /// <summary>
    /// This class contains extensions for working with search query Expressions and the <see cref="SearchCondition{T}" /> class.
    /// </summary>
    public static class SearchExpressionExtensions
    {
        /// <summary>
        /// This method is used to convert a <see cref="SearchExpressionOperator" /> to a <see cref="ComparisonOperatorType" /> value.
        /// </summary>
        /// <param name="operatorValue">Contains the <see cref="SearchExpressionOperator" /> to convert.</param>
        /// <param name="useSqlLikeForContains">Contains a value indicating if a Like operator is used for Contains search operators instead of similar contains.</param>
        /// <returns>Returns the <see cref="ComparisonOperatorType" /> equivalent value.</returns>
        public static ComparisonOperatorType ToComparisonOperator(SearchExpressionOperator operatorValue, Boolean useSqlLikeForContains = false)
        {
            ComparisonOperatorType result = ComparisonOperatorType.Equal;

            switch (operatorValue)
            {
                case SearchExpressionOperator.Equals:
                    result = ComparisonOperatorType.Equal;
                    break;

                case SearchExpressionOperator.NotEquals:
                    result = ComparisonOperatorType.NotEqual;
                    break;

                case SearchExpressionOperator.GreaterThan:
                    result = ComparisonOperatorType.GreaterThan;
                    break;

                case SearchExpressionOperator.GreaterThanOrEqual:
                    result = ComparisonOperatorType.GreaterThanOrEqual;
                    break;

                case SearchExpressionOperator.LessThan:
                    result = ComparisonOperatorType.LessThan;
                    break;

                case SearchExpressionOperator.LessThanOrEqual:
                    result = ComparisonOperatorType.LessThanOrEqual;
                    break;

                case SearchExpressionOperator.Contains:
                    result = useSqlLikeForContains ? ComparisonOperatorType.Like : ComparisonOperatorType.Contains;
                    break;

                case SearchExpressionOperator.DoesNotContain:
                    result = useSqlLikeForContains ? ComparisonOperatorType.NotLike : ComparisonOperatorType.NotContains;
                    break;

                case SearchExpressionOperator.StartsWith:
                    result = ComparisonOperatorType.LikeStartsWith;
                    break;

                case SearchExpressionOperator.DoesNotStartWith:
                    result = ComparisonOperatorType.LikeNotStartsWith;
                    break;

                case SearchExpressionOperator.EndsWith:
                    result = ComparisonOperatorType.LikeEndsWith;
                    break;

                case SearchExpressionOperator.DoesNotEndWith:
                    result = ComparisonOperatorType.LikeNotEndsWith;
                    break;
            }

            return result;
        }

        /// <summary>
        /// This extension method filters an IQueryable according to the specified search condition.
        /// </summary>
        /// <typeparam name="T">Contains the element type the IQueryable is returning.</typeparam>
        /// <param name="source">Contains the IQueryable source.</param>
        /// <param name="condition">Contains the search condition to apply to the IQueryable.</param>
        /// <returns>Returns a new IQueryable with the search conditions applied.</returns>
        /// <exception cref="System.ArgumentNullException">Exception is thrown if source parameter is not specified.</exception>
        /// <exception cref="System.ArgumentNullException">Exception is thrown if condition parameter is not specified.</exception>
        public static IQueryable<T> Where<T>(this IQueryable<T> source, SearchCondition<T> condition)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (condition == null)
            {
                throw new ArgumentNullException(nameof(condition));
            }

            var callExpression = Expression.Call(typeof(Queryable), "Where", new[] { source.ElementType }, source.Expression, Expression.Quote(condition.InternalLambdaExpression));
            return (IQueryable<T>)source.Provider.CreateQuery(callExpression);
        }

        /// <summary>
        /// This extension method filters an IEnumerable according to the specified search condition.
        /// </summary>
        /// <typeparam name="T">Contains the element type the IEnumerable is returning.</typeparam>
        /// <param name="source">Contains the IEnumerable source.</param>
        /// <param name="condition">Contains the search condition to apply to the IEnumerable.</param>
        /// <returns>Returns a new IEnumerable with the search conditions applied.</returns>
        /// <exception cref="System.ArgumentNullException">Exception is thrown if source parameter is not specified.</exception>
        /// <exception cref="System.ArgumentNullException">Exception is thrown if condition parameter is not specified.</exception>
        public static IEnumerable<T> Where<T>(this IEnumerable<T> source, SearchCondition<T> condition)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (condition == null)
            {
                throw new ArgumentNullException(nameof(condition));
            }

            return source.Where(condition.LocalFunctionDelegate);
        }

        /// <summary>
        /// This method is used to convert a list of <see cref="QueryRequestFilterModel" /> models into a <see cref="SearchCondition{T}" /> tree.
        /// </summary>
        /// <typeparam name="T">Contains the element type the search condition shall be comparing.</typeparam>
        /// <param name="filterRows">Contains the query filter rows used to convert into search expressions.</param>
        /// <param name="configuration">Contains the <see cref="ISearchConfiguration" /> instance that holds filter name to property index.</param>
        /// <param name="groupedCondition">Contains the parent condition used in recursive SearchCondition grouping.</param>
        /// <returns>Returns a <see cref="SearchCondition{T}" /> object used for query filtering.</returns>
        /// <exception cref="System.ArgumentNullException">Exception is thrown if filterRows parameter is not specified.</exception>
        /// <exception cref="System.ArgumentNullException">Exception is thrown if configuration parameter is not specified.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S1643:Strings should not be concatenated using '+' in a loop", Justification = "Reviewed")]
        public static SearchCondition<T> BuildConditionsFromFilter<T>(this List<QueryRequestFilterModel> filterRows, ISearchConfiguration configuration, SearchCondition<T> groupedCondition = null)
        {
            if (filterRows == null)
            {
                throw new ArgumentNullException(nameof(filterRows));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            SearchCondition<T> result = groupedCondition;

            foreach (var filterRow in filterRows)
            {
                ComparisonOperatorType filterOperator = ToComparisonOperator(filterRow.Operator, configuration.SupportSqlProviderSyntax);
                Type valueType = configuration.FilterValueType(filterRow.FilterId);
                FilterConfigurationModel filterConfig = configuration.Configuration.FilterFields.FirstOrDefault(f => f.FieldId.Equals(filterRow.FilterId, StringComparison.OrdinalIgnoreCase));
                bool existsCompare = filterConfig?.CompareExists ?? false;

                // if the Provider is a SQL syntax query, let's adapt our filter values for SQL % wildcards.
                if (configuration.SupportSqlProviderSyntax)
                {
                    // this logic will add typical % markers found in LIKE command.
                    switch (filterOperator)
                    {
                        case ComparisonOperatorType.Like:
                        case ComparisonOperatorType.NotLike:

                            // for SQL LIKE command, prefix and/or suffix with % for "contains" operation.
                            if (!filterRow.Value.StartsWith('%'))
                            {
                                filterRow.Value = "%" + filterRow.Value;
                            }

                            if (!filterRow.Value.EndsWith('%'))
                            {
                                filterRow.Value += '%';
                            }

                            break;

                        case ComparisonOperatorType.LikeStartsWith:
                        case ComparisonOperatorType.LikeNotStartsWith:

                            // for SQL, suffix with % for "starts with" operation.
                            if (!filterRow.Value.EndsWith('%'))
                            {
                                filterRow.Value += '%';
                            }

                            break;

                        case ComparisonOperatorType.LikeEndsWith:
                        case ComparisonOperatorType.LikeNotEndsWith:

                            // for SQL, prefix with % for "ends with" operation.
                            if (!filterRow.Value.StartsWith('%'))
                            {
                                filterRow.Value = "%" + filterRow.Value;
                            }

                            break;
                    }
                }

                // if testing existence of navigation property, then if filter compare value = true, then compare != null, else compare == null
                SearchCondition<T> newCondition = existsCompare ?
                    filterRow.Value.ToBoolean() ?
                        new SearchCondition<T>(filterRow.FilterId, ComparisonOperatorType.NotEqual, null) :
                        new SearchCondition<T>(filterRow.FilterId, ComparisonOperatorType.Equal, null) :
                     new SearchCondition<T>(filterRow.FilterId, filterOperator, Convert.ChangeType(filterRow.Value, valueType, CultureInfo.InvariantCulture));

                if (filterRow.FilterId.Contains('.', StringComparison.Ordinal))
                {
                    // By convention, a period is only placed in filter id if a table name other than base is being queried. This means it is a navigation
                    // property and we need a query line to validate it is not null to avoid a Linq exception. We must split the string to get the navigation property.
                    string[] parameterParts = filterRow.FilterId.Split('.');

                    // Get the PropertyInfo instance for navigation property
                    ////var propertyInfo = typeof(T).GetProperty(parameterParts[0]);

                    // build new search condition and place it before the new condition that was just built
                    newCondition = new SearchCondition<T>(parameterParts[0], ComparisonOperatorType.NotEqual, null) & newCondition;
                }

                if (filterRow.GroupedFilters.Any())
                {
                    newCondition = filterRow.GroupedFilters.BuildConditionsFromFilter(configuration, newCondition);
                }

                if (result == null)
                {
                    result = newCondition;
                }
                else
                {
                    switch (filterRow.Logic)
                    {
                        case SearchLogicOperator.And:
                            result &= newCondition;
                            break;

                        case SearchLogicOperator.Or:
                            result |= newCondition;
                            break;
                    }
                }
            }

            return result;
        }
    }
}