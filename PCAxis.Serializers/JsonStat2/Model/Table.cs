/*
 * PxApi
 *
 * This api lets you do 2 things; Find a table(Navigation) and use a table (Table).  _Table below is added to show how tables can be described in yml._  **Table contains status code this API may return** | Status code    | Description      | Reason                      | | - -- -- --        | - -- -- -- -- --      | - -- -- -- -- -- -- -- -- -- --       | | 200            | Success          | The endpoint has delivered response for the request                      | | 400            | Bad request      | If the request is not valid | | 404            | Not found        | If the URL in request does not exist | | 429            | Too many request | Number of requests has surpassed the threshold                            | 
 *
 * The version of the OpenAPI document: 2.0
 * 
 * Generated by: https://openapi-generator.tech
 */

using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using JsonSubTypes;
using Swashbuckle.AspNetCore.Annotations;
using PCAxis.OpenAPILib.Converters;

namespace PCAxis.OpenAPILib.Models
{ 
    /// <summary>
    /// Table item
    /// </summary>
    [DataContract]
    [JsonConverter(typeof(JsonSubtypes), "ObjectType")]
    [SwaggerDiscriminator("ObjectType")]
    [JsonSubtypes.KnownSubType(typeof(FolderInformation), "folderinformation")]
    [SwaggerSubType(typeof(FolderInformation), DiscriminatorValue =  "folderinformation")]
    [JsonSubtypes.KnownSubType(typeof(Heading), "heading")]
    [SwaggerSubType(typeof(Heading), DiscriminatorValue =  "heading")]
    [JsonSubtypes.KnownSubType(typeof(Table), "table")]
    [SwaggerSubType(typeof(Table), DiscriminatorValue =  "table")]
    public partial class Table : FolderContentItem, IEquatable<Table>
    {
        /// <summary>
        /// Gets or Sets Tags
        /// </summary>
        [DataMember(Name="tags", EmitDefaultValue=true)]
        public List<string> Tags { get; set; }

        /// <summary>
        /// Date and time when the figures in the table was last updated, in UTC time.
        /// </summary>
        /// <value>Date and time when the figures in the table was last updated, in UTC time.</value>
        [Required]
        [RegularExpression("^((19|20)\\d\\d)\\-(0?[1-9]|1[012])\\-(0?[1-9]|[12][0-9]|3[01])$")]
        [DataMember(Name="updated", EmitDefaultValue=true)]
        public DateTime? Updated { get; set; }

        /// <summary>
        /// First period
        /// </summary>
        /// <value>First period</value>
        [Required]
        [DataMember(Name="firstPeriod", EmitDefaultValue=true)]
        public string? FirstPeriod { get; set; }

        /// <summary>
        /// Last period
        /// </summary>
        /// <value>Last period</value>
        [Required]
        [DataMember(Name="lastPeriod", EmitDefaultValue=true)]
        public string? LastPeriod { get; set; }


        /// <summary>
        /// Mostly for internal use. Which category table belongs to. internal, public, private or section.
        /// </summary>
        /// <value>Mostly for internal use. Which category table belongs to. internal, public, private or section.</value>
        [TypeConverter(typeof(CustomEnumConverter<CategoryEnum>))]
        [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public enum CategoryEnum
        {
            
            /// <summary>
            /// Enum InternalEnum for internal
            /// </summary>
            [EnumMember(Value = "internal")]
            InternalEnum = 1,
            
            /// <summary>
            /// Enum PublicEnum for public
            /// </summary>
            [EnumMember(Value = "public")]
            PublicEnum = 2,
            
            /// <summary>
            /// Enum PrivateEnum for private
            /// </summary>
            [EnumMember(Value = "private")]
            PrivateEnum = 3,
            
            /// <summary>
            /// Enum SectionEnum for section
            /// </summary>
            [EnumMember(Value = "section")]
            SectionEnum = 4
        }

        /// <summary>
        /// Mostly for internal use. Which category table belongs to. internal, public, private or section.
        /// </summary>
        /// <value>Mostly for internal use. Which category table belongs to. internal, public, private or section.</value>
        [DataMember(Name="category", EmitDefaultValue=true)]
        public CategoryEnum Category { get; set; } = CategoryEnum.PublicEnum;

        /// <summary>
        /// List of varibles name
        /// </summary>
        /// <value>List of varibles name</value>
        [Required]
        [DataMember(Name="variableNames", EmitDefaultValue=false)]
        public List<string> VariableNames { get; set; }

        /// <summary>
        /// Gets or Sets Discontinued
        /// </summary>
        [DataMember(Name="discontinued", EmitDefaultValue=true)]
        public bool? Discontinued { get; set; }

        /// <summary>
        /// Links to ...
        /// </summary>
        /// <value>Links to ...</value>
        [Required]
        [DataMember(Name="links", EmitDefaultValue=true)]
        public List<Link> Links { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class Table {\n");
            sb.Append("  Tags: ").Append(Tags).Append("\n");
            sb.Append("  Updated: ").Append(Updated).Append("\n");
            sb.Append("  FirstPeriod: ").Append(FirstPeriod).Append("\n");
            sb.Append("  LastPeriod: ").Append(LastPeriod).Append("\n");
            sb.Append("  Category: ").Append(Category).Append("\n");
            sb.Append("  VariableNames: ").Append(VariableNames).Append("\n");
            sb.Append("  Discontinued: ").Append(Discontinued).Append("\n");
            sb.Append("  Links: ").Append(Links).Append("\n");
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
            return obj.GetType() == GetType() && Equals((Table)obj);
        }

        /// <summary>
        /// Returns true if Table instances are equal
        /// </summary>
        /// <param name="other">Instance of Table to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(Table other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            return 
                (
                    Tags == other.Tags ||
                    Tags != null &&
                    other.Tags != null &&
                    Tags.SequenceEqual(other.Tags)
                ) && 
                (
                    Updated == other.Updated ||
                    Updated != null &&
                    Updated.Equals(other.Updated)
                ) && 
                (
                    FirstPeriod == other.FirstPeriod ||
                    FirstPeriod != null &&
                    FirstPeriod.Equals(other.FirstPeriod)
                ) && 
                (
                    LastPeriod == other.LastPeriod ||
                    LastPeriod != null &&
                    LastPeriod.Equals(other.LastPeriod)
                ) && 
                (
                    Category == other.Category ||
                    
                    Category.Equals(other.Category)
                ) && 
                (
                    VariableNames == other.VariableNames ||
                    VariableNames != null &&
                    other.VariableNames != null &&
                    VariableNames.SequenceEqual(other.VariableNames)
                ) && 
                (
                    Discontinued == other.Discontinued ||
                    Discontinued != null &&
                    Discontinued.Equals(other.Discontinued)
                ) && 
                (
                    Links == other.Links ||
                    Links != null &&
                    other.Links != null &&
                    Links.SequenceEqual(other.Links)
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
                    if (Tags != null)
                    hashCode = hashCode * 59 + Tags.GetHashCode();
                    if (Updated != null)
                    hashCode = hashCode * 59 + Updated.GetHashCode();
                    if (FirstPeriod != null)
                    hashCode = hashCode * 59 + FirstPeriod.GetHashCode();
                    if (LastPeriod != null)
                    hashCode = hashCode * 59 + LastPeriod.GetHashCode();
                    
                    hashCode = hashCode * 59 + Category.GetHashCode();
                    if (VariableNames != null)
                    hashCode = hashCode * 59 + VariableNames.GetHashCode();
                    if (Discontinued != null)
                    hashCode = hashCode * 59 + Discontinued.GetHashCode();
                    if (Links != null)
                    hashCode = hashCode * 59 + Links.GetHashCode();
                return hashCode;
            }
        }

        #region Operators
        #pragma warning disable 1591

        public static bool operator ==(Table left, Table right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Table left, Table right)
        {
            return !Equals(left, right);
        }

        #pragma warning restore 1591
        #endregion Operators
    }
}
