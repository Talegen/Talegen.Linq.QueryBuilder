﻿/*
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
    using System.Linq;
    using Models;

    /// <summary>
    /// This class implement Implements the <see cref="ISearchConfiguration" /> interface.
    /// </summary>
    /// <seealso cref="Talegen.Linq.QueryBuilder.Configuration.ISearchConfiguration" />
    public abstract class SearchConfigurationBase : ISearchConfiguration
    {
        /// <summary>
        /// Gets or sets a value indicating whether the LINQ is being converted to an SQL provider.
        /// </summary>
        /// <value><c>true</c> if this instance is SQL provider; otherwise, <c>false</c>.</value>
        public bool SupportSqlProviderSyntax { get; set; }

        /// <summary>
        /// Gets a the search field configuration definition.
        /// </summary>
        public QueryConfigurationModel Configuration { get; protected set; }

        /// <summary>
        /// This method is used to return the value type of a specified filter identity.
        /// </summary>
        /// <param name="filterId">Contains the filter identity to find a value type for.</param>
        /// <returns>Returns the expected system Type of the specified filter value.</returns>
        public virtual Type FilterValueType(string filterId)
        {
            Type result = typeof(string);

            var filter = this.Configuration.FilterFields.FirstOrDefault(c => c.FieldId.Equals(filterId, StringComparison.OrdinalIgnoreCase));

            if (filter != null)
            {
                switch (filter.DataType)
                {
                    case SearchConfigurationDataType.Long:
                        result = typeof(long);
                        break;

                    case SearchConfigurationDataType.SimplifiedLong:
                        result = typeof(long);
                        break;

                    case SearchConfigurationDataType.Int:
                        result = typeof(int);
                        break;

                    case SearchConfigurationDataType.SimplifiedInt:
                        result = typeof(int);
                        break;

                    case SearchConfigurationDataType.Boolean:
                        result = typeof(bool);
                        break;

                    case SearchConfigurationDataType.DateTime:
                        result = typeof(DateTime);
                        break;

                    case SearchConfigurationDataType.Xml:
                        result = typeof(string);
                        break;

                    case SearchConfigurationDataType.String:
                        result = typeof(string);
                        break;
                }
            }
            else
            {
                // error no search configuration found by that name
                throw new Exception($"No filter found by the name {filterId}");
            }

            return result;
        }

        /// <summary>
        /// An abstract method that is used to generate the query configuration.
        /// </summary>
        /// <returns>QueryConfigurationModel.</returns>
        protected abstract QueryConfigurationModel GenerateQueryConfiguration();
    }
}