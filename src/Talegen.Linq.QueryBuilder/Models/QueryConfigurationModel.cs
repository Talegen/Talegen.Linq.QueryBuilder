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
    using Common.Core.Attributes;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Properties;

    /// <summary>
    /// Gets or sets an enumerated list of available operators
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum SearchLogicOperator
    {
        /// <summary>
        /// Join filters via AND operator
        /// </summary>
        [LocalizedDescription(ResourceKeys.LabelAndText)]
        And,

        /// <summary>
        /// Join filters via OR operator
        /// </summary>
        [LocalizedDescription(ResourceKeys.LabelOrText)]
        Or
    }

    /// <summary>
    /// This class contains a query configuration.
    /// </summary>
    public class QueryConfigurationModel
    {
        /// <summary>
        /// Gets or sets a list of valid field logic operators.
        /// </summary>
        public List<KeyValuePair<SearchLogicOperator, string>> FieldOperators { get; set; } = new List<KeyValuePair<SearchLogicOperator, string>>();

        /// <summary>
        /// Gets or sets a list of <see cref="FilterConfigurationModel" /> objects.
        /// </summary>
        public List<FilterConfigurationModel> FilterFields { get; set; } = new List<FilterConfigurationModel>();
    }
}