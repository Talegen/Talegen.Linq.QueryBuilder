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

    /// <summary>
    /// This class is represents an query request filter sent from a query interface within an application.
    /// </summary>
    public class QueryRequestFilterModel
    {
        /// <summary>
        /// Gets or sets the filter row logic operator
        /// </summary>
        public SearchLogicOperator Logic { get; set; } = SearchLogicOperator.And;

        /// <summary>
        /// Gets or sets the filter name to add to the query expression.
        /// </summary>
        public string FilterId { get; set; }

        /// <summary>
        /// Gets or sets the filter operator
        /// </summary>
        public SearchExpressionOperator Operator { get; set; } = SearchExpressionOperator.Equals;

        /// <summary>
        /// Gets or sets the query filter value used to compare.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets a list of grouped filters for the given filter model.
        /// </summary>
        public List<QueryRequestFilterModel> GroupedFilters { get; set; } = new List<QueryRequestFilterModel>();
    }
}