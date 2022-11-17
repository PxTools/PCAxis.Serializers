using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Serializers.JsonStat2.Model
{
    public partial class Dataset
    {
        public Dataset()
        {
            Id = new List<string>();
            Size = new List<int>();
        }

        public void AddToTimeRole(string variableCode)
        {
            if (Role == null) Role = new DatasetRole();

            if (Role.Time == null)
            {
                Role.Time = new List<string>();
            }

            Role.Time.Add(variableCode);
        }

        public void AddToMetricRole(string variableCode)
        {
            if (Role == null) Role = new DatasetRole();

            if (Role.Metric == null)
            {
                Role.Metric = new List<string>();
            }

            Role.Metric.Add(variableCode);
        }

        public void AddToGeoRole(string variableCode)
        {
            if (Role == null) Role = new DatasetRole();

            if (Role.Geo == null)
            {
                Role.Geo = new List<string>();
            }

            Role.Geo.Add(variableCode);
        }

        public void AddContact(string name, string phone, string mail)
        {
            if (Extension == null) Extension = new ExtensionRoot();

            if (Extension.Contact == null) Extension.Contact = new List<Contact>();

            Extension.Contact.Add(new Contact(){Name = name, Phone = phone, Mail = mail});
        }

        public void AddContact(string name)
        {
            if (Extension == null) Extension = new ExtensionRoot();

            if (Extension.Contact == null) Extension.Contact = new List<Contact>();

            Extension.Contact.Add(new Contact() { Name = name});
        }

        public void AddTableNote(bool isMandatory, string text)
        {
            if (Extension == null) Extension = new ExtensionRoot();

            if (Extension.Note == null) Extension.Note = new List<Note>();

            Extension.Note.Add(new Note() { Mandatory = isMandatory, Text = text });
        }

        public void CreateExtensionRootPx()
        {
            if (Extension == null) Extension = new ExtensionRoot();

            if (Extension.Px == null) Extension.Px = new ExtensionRootPx();
        }

        public void AddInfoFile(string infoFile)
        {
            if (infoFile != null)
            {
                Extension.Px.Infofile = infoFile;
            }
        }

        public void AddTableId(string tableId)
        {
            if (tableId != null)
            {
                Extension.Px.Tableid = tableId;
            }
        }

        public void AddDecimals(int decimals)
        {
            if (decimals != -1)
            {
                Extension.Px.Decimals = decimals;
            }
        }

        public void AddLanguage(string language)
        {
            if (language != null)
            {
                Extension.Px.Language = language;
            }
        }

        public void AddOfficialStatistics(bool isOfficialStatistics)
        {
            Extension.Px.OfficialStatistics = isOfficialStatistics;
        }

        public void AddMatrix(string matrix)
        {
            if (matrix != null)
            {
                Extension.Px.Matrix = matrix;
            }
        }

        public void AddSubjectCode(string subjectCode)
        {
            if (subjectCode != null)
            {
                Extension.Px.SubjectCode = subjectCode;
            }
        }

        public void AddAggRegAllowed(bool isAggRegAllowed)
        {
            Extension.Px.Aggregallowed = isAggRegAllowed;
        }

        public void AddDescription(string description)
        {
            if (description != null)
            {
                Extension.Px.Description = description;
            }
        }

        public void AddSource(string source)
        {
            if (source != null)
            {
                Source = source;
            }
        }

        public void AddLabel(string label)
        {
            if (label != null)
            {
                Label = label;
            }
        }

        public void AddDimensionValue(string dimensionKey, string label, out DatasetDimensionValue dimensionValue)
        {
            if (Dimension == null) Dimension = new Dictionary<string, DatasetDimensionValue>();

            dimensionValue = new DatasetDimensionValue()
            {
                Label = label,
                Extension = new ExtensionDimension(),
                Category = new JsonstatCategory()
                {
                    Label = new Dictionary<string, string>(), Index = new Dictionary<string, int>()
                }
            };
            Dimension.Add(dimensionKey, dimensionValue);
        }

        public void AddNoteToDimension(DatasetDimensionValue dimensionValue, bool isMandatory, string text)
        {
            if (dimensionValue.Extension.Note == null) dimensionValue.Extension.Note = new List<Note>();

            dimensionValue.Extension.Note.Add(new Note() { Mandatory = isMandatory, Text = text });
        }

        public void AddValueNoteToDimension(DatasetDimensionValue dimensionValue, bool isMandatory, string text)
        {
            if (dimensionValue.Extension.Note == null) dimensionValue.Extension.Note = new List<Note>();

            dimensionValue.Extension.Note.Add(new Note() { Mandatory = isMandatory, Text = text });
        }

        public void AddUnitValue(JsonstatCategory category, int decimals, out JsonstatCategoryUnitValue unitValue)
        {
            if(category.Unit == null) category.Unit = new Dictionary<string, JsonstatCategoryUnitValue>();

            unitValue = new JsonstatCategoryUnitValue() { Decimals = decimals };
        }

        public void AddRefPeriod(DatasetDimensionValue dimensionValue, string valueCode, string refPeriod)
        {
            if(refPeriod == null) return;

            if (dimensionValue.Extension.Refperiod == null)
                dimensionValue.Extension.Refperiod = new Dictionary<string, string>();

            dimensionValue.Extension.Refperiod.Add(valueCode, refPeriod);
        }

        public void AddDimensionLink(DatasetDimensionValue dimensionValue, Dictionary<string, string> metaIds)
        {
            dimensionValue.Link = new JsonstatExtensionLink
            {
                Describedby = new List<DimensionExtension>() { new DimensionExtension() { Extension = metaIds } }
            };
        }

        public void SetUpdatedAsUtcString(DateTime datetime)
        {
            Updated = datetime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
        }
    }
}
