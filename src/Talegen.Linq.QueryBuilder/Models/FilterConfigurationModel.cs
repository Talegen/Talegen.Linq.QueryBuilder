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

namespace Talegen.Linq.QueryBuilder.Models
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Properties;
    using Talegen.Common.Core.Attributes;

    /// <summary>
    /// Contains an enumerated selection list of filter field data types.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum SearchConfigurationDataType
    {
        /// <summary>
        /// The data type is a string.
        /// </summary>
        String,

        /// <summary>
        /// The data type is a long.
        /// </summary>
        Long,

        /// <summary>
        /// The data type is a long.
        /// </summary>
        SimplifiedLong,

        /// <summary>
        /// The data type is an integer.
        /// </summary>
        Int,

        /// <summary>
        /// The data type is an integer.
        /// </summary>
        SimplifiedInt,

        /// <summary>
        /// The data type is a boolean.
        /// </summary>
        Boolean,

        /// <summary>
        /// The data type is a date time.
        /// </summary>
        DateTime,

        /// <summary>
        /// The data type is XML
        /// </summary>
        Xml
    }

    /// <summary>
    /// Gets or sets an enumerated list of available operators
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum SearchExpressionOperator
    {
        /// <summary>
        /// Does equal
        /// </summary>
        [Description("=")]
        Equals,

        /// <summary>
        /// Does not equal
        /// </summary>
        [Description("<>")]
        NotEquals,

        /// <summary>
        /// Greater than
        /// </summary>
        [Description(">")]
        GreaterThan,

        /// <summary>
        /// Less than
        /// </summary>
        [Description("<")]
        LessThan,

        /// <summary>
        /// Greater than or does equal
        /// </summary>
        [Description(">=")]
        GreaterThanOrEqual,

        /// <summary>
        /// Less than or does equal
        /// </summary>
        [Description("<=")]
        LessThanOrEqual,

        /// <summary>
        /// Contains a value
        /// </summary>
        [LocalizedDescription(ResourceKeys.LabelContainsText)]
        Contains,

        /// <summary>
        /// Does not contain a value
        /// </summary>
        [LocalizedDescription(ResourceKeys.LabelDoesNotContainText)]
        DoesNotContain,

        /// <summary>
        /// Starts with a value
        /// </summary>
        [LocalizedDescription(ResourceKeys.LabelStartsWithText)]
        StartsWith,

        /// <summary>
        /// Does not start with a value
        /// </summary>
        [LocalizedDescription(ResourceKeys.LabelDoesNotStartWithText)]
        DoesNotStartWith,

        /// <summary>
        /// Ends with a value
        /// </summary>
        [LocalizedDescription(ResourceKeys.LabelEndsWithText)]
        EndsWith,

        /// <summary>
        /// Does not end with a value
        /// </summary>
        [LocalizedDescription(ResourceKeys.LabelDoesNotEndWithText)]
        DoesNotEndWith
    }

    /// <summary>
    /// This class represents a query filter configuration model.
    /// </summary>
    public class FilterConfigurationModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FilterConfigurationModel" /> class.
        /// </summary>
        public FilterConfigurationModel()
        {
            this.Operators = new List<KeyValuePair<SearchExpressionOperator, string>>();
            this.ValidValues = new List<KeyValuePair<string, string>>();
            this.DataType = SearchConfigurationDataType.String;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FilterConfigurationModel" /> class.
        /// </summary>
        /// <param name="fieldId">The field identifier.</param>
        /// <param name="displayName">The display name.</param>
        /// <param name="type">The type.</param>
        /// <param name="validValues">The valid values.</param>
        /// <param name="compareExists">if set to <c>true</c> [compare exists].</param>
        /// <param name="overrideOperators">The override operators.</param>
        public FilterConfigurationModel(string fieldId, string displayName, SearchConfigurationDataType type = SearchConfigurationDataType.String, List<KeyValuePair<string, string>> validValues = null, bool compareExists = false, Dictionary<SearchExpressionOperator, string> overrideOperators = null)
        {
            this.FieldId = fieldId;
            this.DisplayName = displayName;
            this.DataType = type;
            this.CompareExists = compareExists;
            this.Operators = overrideOperators?.Select(d => new KeyValuePair<SearchExpressionOperator, string>(d.Key, d.Value)).ToList() ?? QueryConfigurationDefaults.TypeOperators(type);
            this.ValidValues = validValues ?? new List<KeyValuePair<string, string>>();
        }

        /// <summary>
        /// Gets or sets the field identity.
        /// </summary>
        public string FieldId { get; set; }

        /// <summary>
        /// Gets or sets the user interface display name.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this is an existence comparison (not null).
        /// </summary>
        [JsonIgnore]
        public bool CompareExists { get; set; }

        /// <summary>
        /// Gets or sets the data type.
        /// </summary>
        public SearchConfigurationDataType DataType { get; set; }

        /// <summary>
        /// Gets or sets a list of operator key value pairs.
        /// </summary>
        public List<KeyValuePair<SearchExpressionOperator, string>> Operators { get; set; }

        /// <summary>
        /// Gets or sets a list of valid value key value pairs.
        /// </summary>
        public List<KeyValuePair<string, string>> ValidValues { get; set; }
    }
}