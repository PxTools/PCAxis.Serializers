using System;
using System.Collections.Generic;
using System.Globalization;

namespace PCAxis.Serializers.JsonStat2.Model
{
    public partial class Dataset
    {
        public Dataset()
        {
            Id = new List<string>();
            Size = new List<int>();
            Class = ClassType.DatasetEnum;
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

        public void AddContents(string contents)
        {
            if (contents != null)
            {
                Extension.Px.Contents = contents;
            }
        }

        public void AddHeading(List<string> heading)
        {
            if (heading != null)
            {
                Extension.Px.Heading = heading;
            }
        }
        public void AddStub(List<string> stub)
        {
            if (stub != null)
            {
                Extension.Px.Stub = stub;
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

        public void AddSubjectArea(string subjectArea)
        {
            if (subjectArea != null)
            {
                Extension.Px.SubjectArea = subjectArea;
            }
        }

        public void AddAggRegAllowed(bool isAggRegAllowed)
        {
            Extension.Px.Aggregallowed = isAggRegAllowed;
        }

        public void AddSurvey(string survey)
        {
            if (survey != null)
            {
                Extension.Px.Survey = survey;
            }
        }

        public void AddLink(string link)
        {
            if (link != null)
            {
                Extension.Px.Link = link;
            }
        }

        public void AddDescription(string description)
        {
            if (description != null)
            {
                Extension.Px.Description = description;
            }
        }

        public void AddDescriptiondefault(bool isDescriptiondefault)
        {
            Extension.Px.Descriptiondefault = isDescriptiondefault;
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

        public void AddTableNote(string text)
        {
            if (text != null)
            {
                if (Note == null) Note = new List<string>();
                Note.Add(text);
            }
        }

        public void AddIsMandatoryForTableNote(string index)
        {
            if (Extension == null) Extension = new ExtensionRoot();
            if (Extension.NoteMandatory == null) Extension.NoteMandatory = new Dictionary<string, bool>();

            Extension.NoteMandatory.Add(index, true);
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
                    Label = new Dictionary<string, string>(),
                    Index = new Dictionary<string, int>()
                }
            };
            Dimension.Add(dimensionKey, dimensionValue);
        }

        public void AddNoteToDimension(DatasetDimensionValue dimensionValue, string text)
        {
            if (dimensionValue.Note == null) dimensionValue.Note = new List<string>();

            dimensionValue.Note.Add(text);
        }

        public void AddIsMandatoryForDimensionNote(DatasetDimensionValue dimensionValue, string index)
        {
            if (dimensionValue.Extension.NoteMandatory == null) dimensionValue.Extension.NoteMandatory = new Dictionary<string, bool>();

            dimensionValue.Extension.NoteMandatory.Add(index, true);
        }

        public void AddValueNoteToCategory(DatasetDimensionValue dimensionValue, string valueNoteKey, string text)
        {
            if (dimensionValue.Category.Note == null) dimensionValue.Category.Note = new Dictionary<string, List<string>>();

            if (dimensionValue.Category.Note.ContainsKey(valueNoteKey))
            {
                dimensionValue.Category.Note[valueNoteKey]
                    .Add(text);
            }
            else
            {
                dimensionValue.Category.Note.Add(valueNoteKey,
                    new List<string> { text });
            }
        }

        public void AddIsMandatoryForCategoryNote(DatasetDimensionValue dimensionValue, string valueNoteKey, string index)
        {
            if (dimensionValue.Extension.CategoryNoteMandatory == null) dimensionValue.Extension.CategoryNoteMandatory = new Dictionary<string, Dictionary<string, bool>>();

            if (dimensionValue.Extension.CategoryNoteMandatory.ContainsKey(valueNoteKey))
            {
                dimensionValue.Extension.CategoryNoteMandatory[valueNoteKey]
                    .Add(index, true);
            }
            else
            {
                dimensionValue.Extension.CategoryNoteMandatory.Add(valueNoteKey,
                    new Dictionary<string, bool> { { index, true } });
            }
        }

        public void AddUnitValue(JsonstatCategory category, out JsonstatCategoryUnitValue unitValue)
        {
            if (category.Unit == null) category.Unit = new Dictionary<string, JsonstatCategoryUnitValue>();

            unitValue = new JsonstatCategoryUnitValue();
        }

        public void AddRefPeriod(DatasetDimensionValue dimensionValue, string valueCode, string refPeriod)
        {
            if (refPeriod == null) return;

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

        public void AddMeasuringType(DatasetDimensionValue dimensionValue, string valueCode, MeasuringType measuringType)
        {
            if (dimensionValue.Extension.MeasuringType == null)
                dimensionValue.Extension.MeasuringType = new Dictionary<string, MeasuringType>();

            dimensionValue.Extension.MeasuringType.Add(valueCode, measuringType);
        }

        public void AddPriceType(DatasetDimensionValue dimensionValue, string valueCode, PriceType priceType)
        {
            if (dimensionValue.Extension.PriceType == null)
                dimensionValue.Extension.PriceType = new Dictionary<string, PriceType>();

            dimensionValue.Extension.PriceType.Add(valueCode, priceType);
        }

        public void AddAdjustment(DatasetDimensionValue dimensionValue, string valueCode, Adjustment adjustment)
        {
            if (dimensionValue.Extension.Adjustment == null)
                dimensionValue.Extension.Adjustment = new Dictionary<string, Adjustment>();

            dimensionValue.Extension.Adjustment.Add(valueCode, adjustment);
        }

        public void AddBasePeriod(DatasetDimensionValue dimensionValue, string valueCode, string basePeriod)
        {
            if (!string.IsNullOrEmpty(basePeriod))
            {
                if (dimensionValue.Extension.BasePeriod == null)
                    dimensionValue.Extension.BasePeriod = new Dictionary<string, string>();

                dimensionValue.Extension.BasePeriod.Add(valueCode, basePeriod);
            }
        }

        public void AddUpdateFrequency(string updateFrequency)
        {
            if (updateFrequency != null)
                Extension.Px.UpdateFrequency = updateFrequency;

        }
    }
}
