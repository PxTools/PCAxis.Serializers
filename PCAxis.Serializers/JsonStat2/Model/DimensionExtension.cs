/*
 * PxApi
 *
 * This api lets you do 2 things; Find a table(Navigation) and use a table (Table).  _Table below is added to show how tables can be described in yml._  **Table contains status code this API may return** | Status code    | Description      | Reason                      | | - -- -- --        | - -- -- -- -- --      | - -- -- -- -- -- -- -- -- -- --       | | 200            | Success          | The endpoint has delivered response for the request                      | | 400            | Bad request      | If the request is not valid | | 403            | Forbidden        | number of cells exceed the API limit | | 404            | Not found        | If the URL in request does not exist | | 429            | Too many request | Requests exceed the API time limit. Large queries should be run in sequence | | 50X            | Internal Server Error | The service might be down | 
 *
 * The version of the OpenAPI document: 2.0
 * 
 * Generated by: https://openapi-generator.tech
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace PCAxis.Serializers.JsonStat2.Model
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class DimensionExtension : IEquatable<DimensionExtension>
    {
        /// <summary>
        /// A extension object
        /// </summary>
        /// <value>A extension object</value>
        [DataMember(Name = "extension", EmitDefaultValue = false)]
        public Dictionary<string, string> Extension { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class DimensionExtension {\n");
            sb.Append("  Extension: ").Append(Extension).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }

        /// <summary>
        /// Returns the JSON string presentation of the object
        /// </summary>
        /// <returns>JSON string presentation of the object</returns>
        public string ToJson()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
        }

        /// <summary>
        /// Returns true if objects are equal
        /// </summary>
        /// <param name="obj">Object to be compared</param>
        /// <returns>Boolean</returns>
        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((DimensionExtension)obj);
        }

        /// <summary>
        /// Returns true if DimensionExtension instances are equal
        /// </summary>
        /// <param name="other">Instance of DimensionExtension to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(DimensionExtension other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            return
                (
                    Extension == other.Extension ||
                    Extension != null &&
                    other.Extension != null &&
                    Extension.SequenceEqual(other.Extension)
                );
        }

        /// <summary>
        /// Gets the hash code
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                var hashCode = 41;
                // Suitable nullity checks etc, of course :)
                if (Extension != null)
                    hashCode = hashCode * 59 + Extension.GetHashCode();
                return hashCode;
            }
        }

        #region Operators
#pragma warning disable 1591

        public static bool operator ==(DimensionExtension left, DimensionExtension right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(DimensionExtension left, DimensionExtension right)
        {
            return !Equals(left, right);
        }

#pragma warning restore 1591
        #endregion Operators
    }
}
