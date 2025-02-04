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
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace PCAxis.Serializers.JsonStat2.Model
{
    /// <summary>
    /// extension at dimension
    /// </summary>
    [DataContract]
    public class ExtensionDimension : IEquatable<ExtensionDimension>
    {
        /// <summary>
        /// Can dimension be elminated
        /// </summary>
        /// <value>Can dimension be elminated</value>
        [DataMember(Name = "elimination", EmitDefaultValue = true)]
        public bool? Elimination { get; set; }

        /// <summary>
        /// Elimination value code
        /// </summary>
        /// <value>Elimination value code</value>
        [DataMember(Name = "eliminationValueCode", EmitDefaultValue = false)]
        public string EliminationValueCode { get; set; }

        /// <summary>
        /// Describes if a note of a certain index is mandatory.
        /// </summary>
        /// <value>Describes if a note of a certain index is mandatory.</value>
        [DataMember(Name = "noteMandatory", EmitDefaultValue = false)]
        public Dictionary<string, bool> NoteMandatory { get; set; }

        /// <summary>
        /// Describes which value note are mandatory
        /// </summary>
        /// <value>Describes which value note are mandatory</value>
        [DataMember(Name = "categoryNoteMandatory", EmitDefaultValue = false)]
        public Dictionary<string, Dictionary<string, bool>> CategoryNoteMandatory { get; set; }

        /// <summary>
        /// Text with information on the exact period for the statistics
        /// </summary>
        /// <value>Text with information on the exact period for the statistics</value>
        [DataMember(Name = "refperiod", EmitDefaultValue = false)]
        public Dictionary<string, string> Refperiod { get; set; }

        /// <summary>
        /// Information about how variables are presented
        /// </summary>
        /// <value>Information about how variables are presented</value>
        [DataMember(Name = "show", EmitDefaultValue = false)]
        public string Show { get; set; }

        /// <summary>
        /// Available codelists for this dimension
        /// </summary>
        /// <value>Available codelists for this dimension</value>
        [DataMember(Name = "codeLists", EmitDefaultValue = false)]
        public List<CodeListInformation> CodeLists { get; set; }

        /// <summary>
        /// Earliest time period in table
        /// </summary>
        /// <value>Earliest time period in table</value>
        [DataMember(Name = "firstPeriod", EmitDefaultValue = false)]
        public string FirstPeriod { get; set; }

        /// <summary>
        /// Latest time period in table
        /// </summary>
        /// <value>Latest time period in table</value>
        [DataMember(Name = "lastPeriod", EmitDefaultValue = false)]
        public string LastPeriod { get; set; }

        /// <summary>
        /// Indicates if data is stock, flow or average.
        /// </summary>
        /// <value>Indicates if data is stock, flow or average.</value>
        [DataMember(Name = "measuringType", EmitDefaultValue = false)]
        public Dictionary<string, MeasuringType> MeasuringType { get; set; }

        /// <summary>
        /// Indicates if data is in current or fixed prices.
        /// </summary>
        /// <value>Indicates if data is in current or fixed prices.</value>
        [DataMember(Name = "priceType", EmitDefaultValue = false)]
        public Dictionary<string, PriceType> PriceType { get; set; }

        /// <summary>
        /// Describes adjustments made to the data
        /// </summary>
        /// <value>Describes adjustments made to the data</value>
        [DataMember(Name = "adjustment", EmitDefaultValue = false)]
        public Dictionary<string, Adjustment> Adjustment { get; set; }

        /// <summary>
        /// Base period for, for instance index series. Is shown with the footnote. If there is a contents variable the keyword is repeated for each value of the contents variable.
        /// </summary>
        /// <value>Base period for, for instance index series. Is shown with the footnote. If there is a contents variable the keyword is repeated for each value of the contents variable.</value>
        [DataMember(Name = "basePeriod", EmitDefaultValue = false)]
        public Dictionary<string, string> BasePeriod { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class ExtensionDimension {\n");
            sb.Append("  Elimination: ").Append(Elimination).Append("\n");
            sb.Append("  EliminationValueCode: ").Append(EliminationValueCode).Append("\n");
            sb.Append("  NoteMandatory: ").Append(NoteMandatory).Append("\n");
            sb.Append("  CategoryNoteMandatory: ").Append(CategoryNoteMandatory).Append("\n");
            sb.Append("  Refperiod: ").Append(Refperiod).Append("\n");
            sb.Append("  Show: ").Append(Show).Append("\n");
            sb.Append("  CodeLists: ").Append(CodeLists).Append("\n");
            sb.Append("  FirstPeriod: ").Append(FirstPeriod).Append("\n");
            sb.Append("  LastPeriod: ").Append(LastPeriod).Append("\n");
            sb.Append("  MeasuringType: ").Append(MeasuringType).Append("\n");
            sb.Append("  PriceType: ").Append(PriceType).Append("\n");
            sb.Append("  Adjustment: ").Append(Adjustment).Append("\n");
            sb.Append("  BasePeriod: ").Append(BasePeriod).Append("\n");
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
            return obj.GetType() == GetType() && Equals((ExtensionDimension)obj);
        }

        /// <summary>
        /// Returns true if ExtensionDimension instances are equal
        /// </summary>
        /// <param name="other">Instance of ExtensionDimension to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(ExtensionDimension other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            return
                (
                    Elimination == other.Elimination ||

                    Elimination.Equals(other.Elimination)
                ) &&
                (
                    EliminationValueCode == other.EliminationValueCode ||
                    EliminationValueCode != null &&
                    EliminationValueCode.Equals(other.EliminationValueCode)
                ) &&
                (
                    NoteMandatory == other.NoteMandatory ||
                    NoteMandatory != null &&
                    other.NoteMandatory != null &&
                    NoteMandatory.SequenceEqual(other.NoteMandatory)
                ) &&
                (
                    CategoryNoteMandatory == other.CategoryNoteMandatory ||
                    CategoryNoteMandatory != null &&
                    other.CategoryNoteMandatory != null &&
                    CategoryNoteMandatory.SequenceEqual(other.CategoryNoteMandatory)
                ) &&
                (
                    Refperiod == other.Refperiod ||
                    Refperiod != null &&
                    other.Refperiod != null &&
                    Refperiod.SequenceEqual(other.Refperiod)
                ) &&
                (
                    Show == other.Show ||
                    Show != null &&
                    Show.Equals(other.Show)
                ) &&
                (
                    CodeLists == other.CodeLists ||
                    CodeLists != null &&
                    other.CodeLists != null &&
                    CodeLists.SequenceEqual(other.CodeLists)
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
                    MeasuringType == other.MeasuringType ||
                    MeasuringType != null &&
                    other.MeasuringType != null &&
                    MeasuringType.SequenceEqual(other.MeasuringType)
                ) &&
                (
                    PriceType == other.PriceType ||
                    PriceType != null &&
                    other.PriceType != null &&
                    PriceType.SequenceEqual(other.PriceType)
                ) &&
                (
                    Adjustment == other.Adjustment ||
                    Adjustment != null &&
                    other.Adjustment != null &&
                    Adjustment.SequenceEqual(other.Adjustment)
                ) &&
                (
                    BasePeriod == other.BasePeriod ||
                    BasePeriod != null &&
                    other.BasePeriod != null &&
                    BasePeriod.SequenceEqual(other.BasePeriod)
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

                hashCode = hashCode * 59 + Elimination.GetHashCode();
                if (EliminationValueCode != null)
                    hashCode = hashCode * 59 + EliminationValueCode.GetHashCode();
                if (NoteMandatory != null)
                    hashCode = hashCode * 59 + NoteMandatory.GetHashCode();
                if (CategoryNoteMandatory != null)
                    hashCode = hashCode * 59 + CategoryNoteMandatory.GetHashCode();
                if (Refperiod != null)
                    hashCode = hashCode * 59 + Refperiod.GetHashCode();
                if (Show != null)
                    hashCode = hashCode * 59 + Show.GetHashCode();
                if (CodeLists != null)
                    hashCode = hashCode * 59 + CodeLists.GetHashCode();
                if (FirstPeriod != null)
                    hashCode = hashCode * 59 + FirstPeriod.GetHashCode();
                if (LastPeriod != null)
                    hashCode = hashCode * 59 + LastPeriod.GetHashCode();
                if (MeasuringType != null)
                    hashCode = hashCode * 59 + MeasuringType.GetHashCode();
                if (PriceType != null)
                    hashCode = hashCode * 59 + PriceType.GetHashCode();
                if (Adjustment != null)
                    hashCode = hashCode * 59 + Adjustment.GetHashCode();
                if (BasePeriod != null)
                    hashCode = hashCode * 59 + BasePeriod.GetHashCode();
                return hashCode;
            }
        }

        #region Operators
#pragma warning disable 1591

        public static bool operator ==(ExtensionDimension left, ExtensionDimension right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ExtensionDimension left, ExtensionDimension right)
        {
            return !Equals(left, right);
        }

#pragma warning restore 1591
        #endregion Operators
    }
}
