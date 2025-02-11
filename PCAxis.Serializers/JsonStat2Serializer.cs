using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

using Newtonsoft.Json;

using PCAxis.Metadata;
using PCAxis.Paxiom;
using PCAxis.Paxiom.Extensions;
using PCAxis.Serializers.JsonStat2.Model;

using PxWeb.Api2.Server.Models;

namespace PCAxis.Serializers
{
    public class JsonStat2Serializer : IPXModelStreamSerializer
    {
        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(typeof(JsonStat2Serializer));
        private readonly MetaLinkManager _metaLinkManager = new MetaLinkManager();

        private static Dictionary<double, string> BuildDataSymbolMap(PXMeta meta)
        {
            var dataSymbolMap = new Dictionary<double, string>();

            // Would have been handy to actually have this as a map in PAxiom.
            dataSymbolMap.Add(PXConstant.DATASYMBOL_1, string.IsNullOrEmpty(meta.DataSymbol1) ? PXConstant.DATASYMBOL_1_STRING : meta.DataSymbol1);
            dataSymbolMap.Add(PXConstant.DATASYMBOL_2, string.IsNullOrEmpty(meta.DataSymbol2) ? PXConstant.DATASYMBOL_2_STRING : meta.DataSymbol2);
            dataSymbolMap.Add(PXConstant.DATASYMBOL_3, string.IsNullOrEmpty(meta.DataSymbol3) ? PXConstant.DATASYMBOL_3_STRING : meta.DataSymbol3);
            dataSymbolMap.Add(PXConstant.DATASYMBOL_4, string.IsNullOrEmpty(meta.DataSymbol4) ? PXConstant.DATASYMBOL_4_STRING : meta.DataSymbol4);
            dataSymbolMap.Add(PXConstant.DATASYMBOL_5, string.IsNullOrEmpty(meta.DataSymbol5) ? PXConstant.DATASYMBOL_5_STRING : meta.DataSymbol5);
            dataSymbolMap.Add(PXConstant.DATASYMBOL_6, string.IsNullOrEmpty(meta.DataSymbol6) ? PXConstant.DATASYMBOL_6_STRING : meta.DataSymbol6);
            dataSymbolMap.Add(PXConstant.DATASYMBOL_7, string.IsNullOrEmpty(meta.DataSymbolSum) ? PXConstant.DATASYMBOL_7_STRING : meta.DataSymbolSum); //Strange code due to lagacy which has been addressed but not fixed
            dataSymbolMap.Add(PXConstant.DATASYMBOL_NIL, string.IsNullOrEmpty(meta.DataSymbolNIL) ? PXConstant.DATASYMBOL_NIL_STRING : meta.DataSymbolNIL);

            return dataSymbolMap;
        }

        public string BuildJsonStructure(PXModel model)
        {
            var dataset = new JsonStat2Dataset();

            //Updated
            AddUpdated(model, dataset);

            //Source
            dataset.AddSource(model.Meta.Source);

            //Label
            dataset.AddLabel(model.Meta.Title);

            //Extension PX
            AddPxToExtension(model, dataset);

            // Dimension
            //Handle Elminated content variable

            if (model.Meta.ContentVariable == null)
            {
                AddInfoForEliminatedContentVariable(model, dataset);
            }

            foreach (var variable in model.Meta.Variables)
            {
                //temporary collector storage
                var metaIdsHelper = new Dictionary<string, string>();

                dataset.AddDimensionValue(variable.Code, variable.Name, out var dimensionValue);

                var indexCounter = 0;

                foreach (var variableValue in variable.Values)
                {
                    dimensionValue.Category.Label.Add(variableValue.Code, variableValue.Value);
                    dimensionValue.Category.Index.Add(variableValue.Code, indexCounter++);

                    CollectMetaIdsForValue(variableValue, ref metaIdsHelper);

                    // ValueNote
                    AddValueNotes(variableValue, dimensionValue);

                    if (!variable.IsContentVariable) continue;

                    var unitDecimals = (variableValue.HasPrecision()) ? variableValue.Precision : model.Meta.ShowDecimals;
                    JsonStat2Dataset.AddUnitValue(dimensionValue.Category, out var unitValue);

                    if (variableValue.ContentInfo != null)
                    {
                        unitValue.Base = variableValue.ContentInfo.Units;
                        unitValue.Decimals = unitDecimals;

                        //refPeriod extension dimension
                        JsonStat2Dataset.AddRefPeriod(dimensionValue, variableValue.Code, variableValue.ContentInfo.RefPeriod);

                        //measuringType extension dimension
                        JsonStat2Dataset.AddMeasuringType(dimensionValue, variableValue.Code, GetMeasuringType(variableValue.ContentInfo.StockFa));

                        //priceType extension dimension
                        JsonStat2Dataset.AddPriceType(dimensionValue, variableValue.Code, GetPriceType(variableValue.ContentInfo.CFPrices));

                        //adjustment extension dimension
                        JsonStat2Dataset.AddAdjustment(dimensionValue, variableValue.Code, GetAdjustment(variableValue.ContentInfo.DayAdj, variableValue.ContentInfo.SeasAdj));

                        //basePeriod extension dimension
                        JsonStat2Dataset.AddBasePeriod(dimensionValue, variableValue.Code, variableValue.ContentInfo.Baseperiod);

                        // Contact
                        AddContact(dataset, variableValue.ContentInfo);
                    }
                    else
                    {
                        _logger.WarnFormat("Category {CategoryCode} lacks ContentInfo. Unit, refPeriod and contact not set", variableValue.Code);
                    }

                    dimensionValue.Category.Unit.Add(variableValue.Code, unitValue);
                }

                //elimination
                AddEliminationInfo(dimensionValue, variable);

                //Show
                AddShow(dimensionValue, variable);

                //Variable notes
                AddVariableNotes(variable, dimensionValue);

                //MetaID
                CollectMetaIdsForVariable(variable, ref metaIdsHelper);

                if (metaIdsHelper.Count > 0)
                {
                    JsonStat2Dataset.AddDimensionLink(dimensionValue, metaIdsHelper);
                }

                dataset.Size.Add(variable.Values.Count);
                dataset.Id.Add(variable.Code);

                //Role
                AddRoles(variable, dataset);

                //TimeUnit
                if (variable.IsTime || (variable.VariableType != null && variable.VariableType.Equals("T")))
                {
                    AddTimeUnit(dataset, variable);
                }
            }

            AddTableNotes(model, dataset);


            GetValueAndStatus(model, out var value, out var status);

            //Value
            dataset.Value = value;

            //Status
            dataset.Status = status.Count == 0 ? null : status;

            // override converter to stop adding ".0" after interger values.
            var result = JsonConvert.SerializeObject(dataset, new DecimalJsonConverter());

            return result;
        }

        private static void AddTimeUnit(JsonStat2Dataset dataset, Variable variable)
        {
            dataset.Extension.TimeUnit = GetTimeUnit(variable.TimeScale);
        }

        private static TimeUnit GetTimeUnit(TimeScaleType timeScaleType)
        {
            switch (timeScaleType)
            {
                case TimeScaleType.NotSet:
                    return TimeUnit.OtherEnum;
                case TimeScaleType.Annual:
                    return TimeUnit.AnnualEnum;
                case TimeScaleType.Halfyear:
                    return TimeUnit.HalfYearEnum;
                case TimeScaleType.Quartely:
                    return TimeUnit.QuarterlyEnum;
                case TimeScaleType.Monthly:
                    return TimeUnit.MonthlyEnum;
                case TimeScaleType.Weekly:
                    return TimeUnit.WeeklyEnum;
                default:
                    return TimeUnit.OtherEnum;
            }
        }

        private static PriceType GetPriceType(string cfprices)
        {
            string cfp = cfprices != null ? cfprices.ToUpper() : "";

            switch (cfp)
            {
                case "C":
                    return PriceType.CurrentEnum;
                case "F":
                    return PriceType.FixedEnum;
                default:
                    return PriceType.NotApplicableEnum;
            }
        }

        private static Adjustment GetAdjustment(string dayAdj, string seasAdj)
        {
            string dadj = dayAdj != null ? dayAdj.ToUpper() : "";
            string sadj = seasAdj != null ? seasAdj.ToUpper() : "";

            if (dadj.Equals("YES") && sadj.Equals("YES"))
            {
                return Adjustment.WorkAndSesEnum;
            }
            else if (sadj.Equals("YES"))
            {
                return Adjustment.SesOnlyEnum;
            }
            else if (dadj.Equals("YES"))
            {
                return Adjustment.WorkOnlyEnum;
            }
            else
            {
                return Adjustment.NoneEnum;
            }
        }

        private static MeasuringType GetMeasuringType(string stockfa)
        {
            if (stockfa == null)
            {
                return MeasuringType.OtherEnum;
            }
            switch (stockfa.ToUpper())
            {
                case "S":
                    return MeasuringType.StockEnum;
                case "F":
                    return MeasuringType.FlowEnum;
                case "A":
                    return MeasuringType.AverageEnum;
                default:
                    return MeasuringType.OtherEnum;
            }
        }


        private void AddInfoForEliminatedContentVariable(PXModel model, JsonStat2Dataset dataset)
        {
            dataset.AddDimensionValue("ContentsCode", "EliminatedContents", out var dimensionValue);
            var eliminatedValue = "EliminatedValue";
            dimensionValue.Category.Label.Add(eliminatedValue, model.Meta.Contents);
            dimensionValue.Category.Index.Add(eliminatedValue, 0);

            JsonStat2Dataset.AddUnitValue(dimensionValue.Category, out var unitValue);
            unitValue.Base = model.Meta.ContentInfo.Units;
            unitValue.Decimals = model.Meta.Decimals;

            dimensionValue.Category.Unit.Add(eliminatedValue, unitValue);

            dimensionValue.Extension.Elimination = true;

            //refPeriod extension dimension
            JsonStat2Dataset.AddRefPeriod(dimensionValue, eliminatedValue, model.Meta.ContentInfo.RefPeriod);

            //measuringType extension dimension
            JsonStat2Dataset.AddMeasuringType(dimensionValue, eliminatedValue, GetMeasuringType(model.Meta.ContentInfo.StockFa));

            //priceType extension dimension
            JsonStat2Dataset.AddPriceType(dimensionValue, eliminatedValue, GetPriceType(model.Meta.ContentInfo.CFPrices));

            //adjustment extension dimension
            JsonStat2Dataset.AddAdjustment(dimensionValue, eliminatedValue, GetAdjustment(model.Meta.ContentInfo.DayAdj, model.Meta.ContentInfo.SeasAdj));

            //basePeriod extension dimension
            JsonStat2Dataset.AddBasePeriod(dimensionValue, eliminatedValue, model.Meta.ContentInfo.Baseperiod);

            // Contact
            AddContact(dataset, model.Meta.ContentInfo);

            dataset.AddToMetricRole("ContentsCode");
            dataset.Size.Add(1);
            dataset.Id.Add("ContentsCode");
        }

        private static void AddUpdated(PXModel model, JsonStat2Dataset dataset)
        {
            DateTime tempDateTime;
            if (model.Meta.ContentVariable != null && model.Meta.ContentVariable.Values.Count > 0)
            {
                var lastUpdatedContentsVariable = model.Meta.ContentVariable.Values
                    .OrderByDescending(x => x.ContentInfo.LastUpdated)
                    .FirstOrDefault();

                if (lastUpdatedContentsVariable != null)
                {
                    tempDateTime = lastUpdatedContentsVariable.ContentInfo.LastUpdated.PxDateStringToDateTime();
                }
                else
                {
                    tempDateTime = model.Meta.CreationDate.PxDateStringToDateTime();
                }
            }
            else if (model.Meta.ContentInfo.LastUpdated != null)
            {
                tempDateTime = model.Meta.ContentInfo.LastUpdated.PxDateStringToDateTime();
            }
            else
            {
                tempDateTime = model.Meta.CreationDate.PxDateStringToDateTime();
            }

            dataset.Updated = DateTimeAsUtcString(tempDateTime);
        }

        public static string DateTimeAsUtcString(DateTime datetime)
        {
            return datetime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
        }

        private static void AddNextUpdate(Dataset dataset, string nextUpdate)
        {
            if (nextUpdate != null)
            {
                DateTime tempDatetime;
                tempDatetime = nextUpdate.PxDateStringToDateTime();

                dataset.Extension.Px.NextUpdate = DateTimeAsUtcString(tempDatetime);
            }
        }

        private static void AddPxToExtension(PXModel model, JsonStat2Dataset dataset)
        {
            var decimals = model.Meta.ShowDecimals < 0 ? model.Meta.Decimals : model.Meta.ShowDecimals;

            dataset.CreateExtensionRootPx();
            dataset.AddInfoFile(model.Meta.InfoFile);
            dataset.AddTableId(model.Meta.TableID);
            dataset.AddDecimals(decimals);
            dataset.AddContents(model.Meta.Contents);
            dataset.AddDescription(model.Meta.Description);
            dataset.AddDescriptiondefault(model.Meta.DescriptionDefault);
            dataset.AddStub(model.Meta.Stub.Select(v => v.Code).ToList());
            dataset.AddHeading(model.Meta.Heading.Select(v => v.Code).ToList());
            dataset.AddLanguage(model.Meta.Language);
            dataset.AddOfficialStatistics(model.Meta.OfficialStatistics);
            dataset.AddMatrix(model.Meta.Matrix);
            dataset.AddSubjectCode(model.Meta.SubjectCode);
            dataset.AddSubjectArea(model.Meta.SubjectArea);
            dataset.AddAggRegAllowed(model.Meta.AggregAllowed);
            dataset.AddSurvey(model.Meta.Survey);
            dataset.AddLink(model.Meta.Link);
            dataset.AddUpdateFrequency(model.Meta.UpdateFrequency);
            AddNextUpdate(dataset, model.Meta.NextUpdate);
        }

        private static void AddTableNotes(PXModel model, JsonStat2Dataset dataset)
        {
            var notes = model.Meta.Notes.Where(note => note.Type == NoteType.Table);

            var noteIndex = 0;
            foreach (var note in notes)
            {

                dataset.AddTableNote(note.Text);

                if (note.Mandantory)
                    dataset.AddIsMandatoryForTableNote(noteIndex.ToString());

                noteIndex++;

            }
        }

        private static void AddEliminationInfo(DatasetDimensionValue dimensionValue, Variable variable)
        {
            dimensionValue.Extension.Elimination = variable.Elimination;

            if (!variable.Elimination || variable.EliminationValue == null) return;

            if (string.IsNullOrEmpty(variable.EliminationValue.Code)) return;
            dimensionValue.Extension.EliminationValueCode = variable.EliminationValue.Code;
        }

        private static void AddShow(DatasetDimensionValue dimensionValue, Variable variable)
        {
            if (Enum.TryParse(variable.PresentationText.ToString(), out PresentationFormType presentationForm))
            {
                dimensionValue.Extension.Show = presentationForm.ToString().ToLower();
            }
        }

        private static void AddValueNotes(PCAxis.Paxiom.Value variableValue, DatasetDimensionValue dimensionValue)
        {
            if (variableValue.Notes == null) return;

            var index = 0;
            foreach (var note in variableValue.Notes)
            {
                JsonStat2Dataset.AddValueNoteToCategory(dimensionValue, variableValue.Code, note.Text);

                if (note.Mandantory)
                    JsonStat2Dataset.AddIsMandatoryForCategoryNote(dimensionValue, variableValue.Code, index.ToString());

                index++;
            }
        }

        private static void AddVariableNotes(Variable variable, DatasetDimensionValue dimensionValue)
        {
            if (variable.Notes == null) return;

            var noteIndex = 0;
            foreach (var note in variable.Notes)
            {
                JsonStat2Dataset.AddNoteToDimension(dimensionValue, note.Text);

                if (note.Mandantory)
                    JsonStat2Dataset.AddIsMandatoryForDimensionNote(dimensionValue, noteIndex.ToString());

                noteIndex++;
            }
        }

        private void AddContact(JsonStat2Dataset dataset, ContInfo contInfo)
        {
            if (contInfo.ContactInfo != null && contInfo.ContactInfo.Count > 0)
            {
                foreach (var contact in contInfo.ContactInfo)
                {
                    MapContact(dataset, contact, contInfo);
                }
            }
            else
            {
                MapContact(dataset, contInfo.Contact);
            }
        }

        private void MapContact(JsonStat2Dataset dataset, Paxiom.Contact contact, ContInfo contInfo)
        {

            if (dataset.Extension.Contact == null)
            {
                dataset.Extension.Contact = new List<PxWeb.Api2.Server.Models.Contact>();
            }

            StringBuilder sb = new StringBuilder();
            sb.Append(contact.Forname);
            sb.Append(' ');
            sb.Append(contact.Surname);

            PxWeb.Api2.Server.Models.Contact jsonContact = new PxWeb.Api2.Server.Models.Contact
            {
                Name = sb.ToString(),
                Mail = contact.Email,
                Phone = contact.PhoneNo
            };

            if (contInfo.Contact != null)
            {
                var contacts = contInfo.Contact.Split(new[] { "##" }, StringSplitOptions.RemoveEmptyEntries);
                var res = contacts.FirstOrDefault(x => x.Contains(contact.Forname) && x.Contains(contact.Surname) && x.Contains(contact.Email) && x.Contains(contact.PhoneNo));

                if (res != null)
                {
                    jsonContact.Raw = res;
                }
            }

            // Only display unique contact once
            if (!dataset.Extension.Contact.Exists(x => x.Mail.Equals(jsonContact.Mail) && x.Name.Equals(jsonContact.Name) && x.Phone.Equals(jsonContact.Phone)))
            {
                dataset.Extension.Contact.Add(jsonContact);
            }
        }

        private void MapContact(JsonStat2Dataset dataset, string contactString)
        {
            if (contactString != null)
            {
                if (dataset.Extension.Contact == null)
                {
                    dataset.Extension.Contact = new List<PxWeb.Api2.Server.Models.Contact>();
                }

                var contacts = contactString.Split(new[] { "##" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var contact in contacts)
                {
                    if (!dataset.Extension.Contact.Exists(x => x.Raw.Equals(contact)))
                    {
                        dataset.Extension.Contact.Add(new PxWeb.Api2.Server.Models.Contact
                        {
                            Raw = contact
                        });
                    }
                }
            }
        }

        private static void AddRoles(Variable variable, JsonStat2Dataset dataset)
        {
            if (variable.IsTime)
            {
                dataset.AddToTimeRole(variable.Code);
            }

            if (variable.IsContentVariable)
            {
                dataset.AddToMetricRole(variable.Code);
            }

            if (variable.VariableType == null) return;
            if (variable.VariableType.ToUpper() == "G" || (variable.Map != null))
            {
                dataset.AddToGeoRole(variable.Code);
            }
        }

        private void CollectMetaIdsForVariable(Variable variable, ref Dictionary<string, string> metaIds)
        {
            if (!string.IsNullOrWhiteSpace(variable.MetaId))
            {
                metaIds.Add(variable.Code, SerializeMetaIds(variable.MetaId));
            }
        }

        private void CollectMetaIdsForValue(PCAxis.Paxiom.Value value, ref Dictionary<string, string> metaIds)
        {
            if (!string.IsNullOrWhiteSpace(value.MetaId))
            {
                metaIds.Add(value.Code, SerializeMetaIds(value.MetaId));
            }
        }

        private string SerializeMetaIds(string metaId)
        {
            var metaIds = metaId.Split(_metaLinkManager.GetSystemSeparator(), StringSplitOptions.RemoveEmptyEntries);
            var metaIdsAsString = new List<string>();
            foreach (var meta in metaIds)
            {
                var metaLinks = meta.Split(_metaLinkManager.GetParamSeparator(), StringSplitOptions.RemoveEmptyEntries);
                if (metaLinks.Length > 0)
                {
                    metaIdsAsString.Add(meta);
                }
            }
            return (string.Join(" ", metaIdsAsString));
        }

        private static void GetValueAndStatus(PXModel model, out List<double?> value, out Dictionary<string, string> status)
        {
            value = new List<double?>();
            var buffer = new double[model.Data.MatrixColumnCount];
            var dataSymbolMap = BuildDataSymbolMap(model.Meta);
            var formatter = new DataFormatter(model);
            var note = string.Empty;
            var dataNote = string.Empty;
            status = new Dictionary<string, string>();
            var index = 0;
            var numberFormatInfo = new System.Globalization.NumberFormatInfo();
            for (var i = 0; i < model.Data.MatrixRowCount; i++)
            {
                model.Data.ReadLine(i, buffer);
                for (var j = 0; j < model.Data.MatrixColumnCount; j++)
                {
                    if (dataSymbolMap.TryGetValue(buffer[j], out var symbol))
                    {
                        value.Add(null);
                        status.Add(index.ToString(), symbol);
                    }
                    else
                    {
                        value.Add(Convert.ToDouble(formatter.ReadElement(i, j, ref note, ref dataNote, ref numberFormatInfo), numberFormatInfo));
                        if (!string.IsNullOrEmpty(dataNote))
                        {
                            status.Add(index.ToString(), dataNote);
                        }
                    }
                    index++;
                }
            }
        }

        public void Serialize(PXModel model, string path)
        {
            var result = BuildJsonStructure(model);
            var encoding = new UTF8Encoding();
            var fileName = model.Meta.MainTable + ".json";

            File.WriteAllText(path + fileName, result, encoding);
        }

        public void Serialize(PXModel model, Stream stream)
        {
            var result = BuildJsonStructure(model);

            byte[] jsonData = Encoding.UTF8.GetBytes(result);
            stream.Write(jsonData, 0, jsonData.Length);
        }
    }
}
