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

namespace Talegen.Linq.QueryBuilder.Configuration
{
    using System;
    using Models;

    /// <summary>
    /// This interface defines a minimum implementation of a search configuration.
    /// </summary>
    public interface ISearchConfiguration
    {
        /// <summary>
        /// Gets or sets a value indicating whether the LINQ is being converted to an SQL provider.
        /// </summary>
        /// <value><c>true</c> if this instance is SQL provider; otherwise, <c>false</c>.</value>
        bool SupportSqlProviderSyntax { get; set; }

        /// <summary>
        /// Gets a the search field configuration definition.
        /// </summary>
        QueryConfigurationModel Configuration { get; }

        /// <summary>
        /// This method is used to return the value type of a specified filter identity.
        /// </summary>
        /// <param name="filterId">Contains the filter identity to find a value type for.</param>
        /// <returns>Returns the expected system Type of the specified filter value.</returns>
        Type FilterValueType(string filterId);
    }
}