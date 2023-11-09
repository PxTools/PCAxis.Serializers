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
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace PCAxis.Serializers.JsonStat2.Model
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public partial class Link : IEquatable<Link>
    {
        /// <summary>
        /// the link relation, see https://www.iana.org/assignments/link-relations/link-relations.xhtml
        /// </summary>
        /// <value>the link relation, see https://www.iana.org/assignments/link-relations/link-relations.xhtml</value>
        [Required]
        [DataMember(Name="rel", EmitDefaultValue=false)]
        public string Rel { get; set; }

        /// <summary>
        /// The language that is used for the link, see https://moz.com/learn/seo/hreflang-tag
        /// </summary>
        /// <value>The language that is used for the link, see https://moz.com/learn/seo/hreflang-tag</value>
        [Required]
        [DataMember(Name="hreflang", EmitDefaultValue=false)]
        public string Hreflang { get; set; }

        /// <summary>
        /// the link to the resource
        /// </summary>
        /// <value>the link to the resource</value>
        [Required]
        [DataMember(Name="href", EmitDefaultValue=false)]
        public string Href { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class Link {\n");
            sb.Append("  Rel: ").Append(Rel).Append("\n");
            sb.Append("  Hreflang: ").Append(Hreflang).Append("\n");
            sb.Append("  Href: ").Append(Href).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }

        /// <summary>
        /// Returns the JSON string presentation of the object
        /// </summary>
        /// <returns>JSON string presentation of the object</returns>
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
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
            return obj.GetType() == GetType() && Equals((Link)obj);
        }

        /// <summary>
        /// Returns true if Link instances are equal
        /// </summary>
        /// <param name="other">Instance of Link to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(Link other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            return 
                (
                    Rel == other.Rel ||
                    Rel != null &&
                    Rel.Equals(other.Rel)
                ) && 
                (
                    Hreflang == other.Hreflang ||
                    Hreflang != null &&
                    Hreflang.Equals(other.Hreflang)
                ) && 
                (
                    Href == other.Href ||
                    Href != null &&
                    Href.Equals(other.Href)
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
                    if (Rel != null)
                    hashCode = hashCode * 59 + Rel.GetHashCode();
                    if (Hreflang != null)
                    hashCode = hashCode * 59 + Hreflang.GetHashCode();
                    if (Href != null)
                    hashCode = hashCode * 59 + Href.GetHashCode();
                return hashCode;
            }
        }

        #region Operators
        #pragma warning disable 1591

        public static bool operator ==(Link left, Link right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Link left, Link right)
        {
            return !Equals(left, right);
        }

        #pragma warning restore 1591
        #endregion Operators
    }
}