/*
 * PxApi
 *
 * This api lets you do 2 things; Find a table(Navigation) and use a table (Table).  _Table below is added to show how tables can be described in yml._  **Table contains status code this API may return** | Status code    | Description      | Reason                      | | - -- -- --        | - -- -- -- -- --      | - -- -- -- -- -- -- -- -- -- --       | | 200            | Success          | The endpoint has delivered response for the request                      | | 400            | Bad request      | If the request is not valid | | 404            | Not found        | If the URL in request does not exist | | 429            | Too many request | Number of requests has surpassed the threshold                            | 
 *
 * The version of the OpenAPI document: 2.0
 * 
 * Generated by: https://openapi-generator.tech
 */

using System.ComponentModel;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Serializers.JsonStat2.Model.Converters;

namespace Serializers.JsonStat2.Model
{ 
        /// <summary>
        /// Type of codelist
        /// </summary>
        /// <value>Type of codelist</value>
        [TypeConverter(typeof(CustomEnumConverter<CodeListType>))]
        [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public enum CodeListType
        {
            
            /// <summary>
            /// Enum AggregationEnum for Aggregation
            /// </summary>
            [EnumMember(Value = "Aggregation")]
            AggregationEnum = 1,
            
            /// <summary>
            /// Enum ValuesetEnum for Valueset
            /// </summary>
            [EnumMember(Value = "Valueset")]
            ValuesetEnum = 2
        }
}