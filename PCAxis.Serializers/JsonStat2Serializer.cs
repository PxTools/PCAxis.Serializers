using System;
using Newtonsoft.Json;
using PCAxis.Paxiom;
using PCAxis.Metadata;
using PCAxis.Paxiom.Extensions;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using Serializers.JsonStat2.Model;
using Value = PCAxis.Paxiom.Value;

namespace PCAxis.Serializers
{
    public class JsonStat2Serializer : IPXModelStreamSerializer
    {
        private static log4net.ILog _logger = log4net.LogManager.GetLogger(typeof(JsonStat2Serializer));
        private MetaLinkManager _metaLinkManager = new MetaLinkManager();

        private Dictionary<double, string> BuildDataSymbolMap(PXMeta meta)
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
            var dataset = new Dataset();

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

            foreach (var variable in model.Meta.Variables.OrderByDescending(x => x.IsTime).ThenBy(x => x.IsContentVariable))
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
                    AddValueNotes(variableValue, dataset, dimensionValue);

                    if (!variable.IsContentVariable) continue;

                    var unitDecimals = (variableValue.HasPrecision()) ? variableValue.Precision : model.Meta.ShowDecimals;
                    dataset.AddUnitValue(dimensionValue.Category, out var unitValue);

                    if (variableValue.ContentInfo != null)
                    {
                        unitValue.Base = variableValue.ContentInfo.Units;
                        unitValue.Decimals = unitDecimals;

                        //refPeriod extension dimension
                        dataset.AddRefPeriod(dimensionValue, variableValue.Code, variableValue.ContentInfo.RefPeriod);

                        // Contact
                        AddContacts(dataset, variableValue.ContentInfo.Contact);
                    }
                    else
                    {
                        _logger.Warn("Category" + variableValue.Code + " lacks ContentInfo. Unit, refPeriod and contact not set");
                    }

                    dimensionValue.Category.Unit.Add(variableValue.Code, unitValue);
                }

                //elimination
                AddEliminationInfo(dimensionValue, variable);

                //Show
                AddShow(dimensionValue, variable);

                //Variable notes
                AddVariableNotes(variable, dataset, dimensionValue);

                CollectMetaIdsForVariable(variable, ref metaIdsHelper);

                if (metaIdsHelper.Count > 0)
                {
                    dataset.AddDimensionLink(dimensionValue, metaIdsHelper);
                }

                dataset.Size.Add(variable.Values.Count);
                dataset.Id.Add(variable.Code);

                //Role
                AddRoles(variable, dataset);
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

        private void AddInfoForEliminatedContentVariable(PXModel model, Dataset dataset)
        {
            dataset.AddDimensionValue("ContentsCode", "EliminatedContents", out var dimensionValue);
            dimensionValue.Category.Label.Add("EliminatedValue", model.Meta.Contents);
            dimensionValue.Category.Index.Add("EliminatedValue", 0);

            dataset.AddUnitValue(dimensionValue.Category, out var unitValue);
            unitValue.Base = model.Meta.ContentInfo.Units;
            unitValue.Decimals = model.Meta.Decimals;

            dimensionValue.Category.Unit.Add("EliminatedValue", unitValue);

            dimensionValue.Extension.Elimination = true;

            //refPeriod extension dimension
            dataset.AddRefPeriod(dimensionValue, "EliminatedValue", model.Meta.ContentInfo.RefPeriod);

            // Contact
            AddContacts(dataset, model.Meta.ContentInfo.Contact);
        }

        private void AddUpdated(PXModel model, Dataset dataset)
        {
            DateTime tempDateTime;
            if (model.Meta.ContentVariable != null && model.Meta.ContentVariable.Values.Count > 0)
            {
                var lastUpdatedContentsVariable = model.Meta.ContentVariable.Values
                    .OrderByDescending(x => x.ContentInfo.LastUpdated)
                    .FirstOrDefault();

                // ReSharper disable once PossibleNullReferenceException
                tempDateTime = lastUpdatedContentsVariable.ContentInfo.LastUpdated.PxDateStringToDateTime();
            }
            else if (model.Meta.ContentInfo.LastUpdated != null)
            {
                tempDateTime = model.Meta.ContentInfo.LastUpdated.PxDateStringToDateTime();
            }
            else
            {
                tempDateTime = model.Meta.CreationDate.PxDateStringToDateTime();
            }

            dataset.SetUpdatedAsUtcString(tempDateTime);
        }

        private void AddPxToExtension(PXModel model, Dataset dataset)
        {
            var decimals = model.Meta.ShowDecimals < 0 ? model.Meta.Decimals : model.Meta.ShowDecimals;

            dataset.CreateExtensionRootPx();
            dataset.AddInfoFile(model.Meta.InfoFile);
            dataset.AddTableId(model.Meta.TableID);
            dataset.AddDecimals(decimals);
            dataset.AddDescription(model.Meta.Description);
            dataset.AddLanguage(model.Meta.Language);
            dataset.AddOfficialStatistics(model.Meta.OfficialStatistics);
            dataset.AddMatrix(model.Meta.Matrix);
            dataset.AddSubjectCode(model.Meta.SubjectCode);
            dataset.AddAggRegAllowed(model.Meta.AggregAllowed);
        }

        private void AddTableNotes(PXModel model, Dataset dataset)
        {
            var notes = model.Meta.Notes.Where(note => note.Type == NoteType.Table);

            foreach (var note in notes)
            {
                dataset.AddTableNote(note.Mandantory, note.Text);
            }
        }

        private void AddEliminationInfo(DatasetDimensionValue dimensionValue, Variable variable)
        {
            dimensionValue.Extension.Elimination = variable.Elimination;

            if (!variable.Elimination || variable.EliminationValue == null) return;

            if (string.IsNullOrEmpty(variable.EliminationValue.Code)) return;
            dimensionValue.Extension.EliminationValueCode = variable.EliminationValue.Code;
        }

        private void AddShow(DatasetDimensionValue dimensionValue, Variable variable)
        {
            if (Enum.TryParse(variable.PresentationText.ToString(), out PresentationFormType presentationForm))
            {
                dimensionValue.Extension.Show = ConvertPresentationFormTypeToText(presentationForm);
            }
        }

        private string ConvertPresentationFormTypeToText(PresentationFormType enuFormType)
        {
            try
            {
                switch (enuFormType)
                {
                    case PresentationFormType.Code:
                        return "code";
                    case PresentationFormType.Value:
                        return "text";
                    case PresentationFormType.Code_Value:
                        return "code_text";
                    case PresentationFormType.Value_Code:
                        return "text_code";
                    default:
                        throw new ArgumentOutOfRangeException(nameof(enuFormType), enuFormType,
                            $"Presentation form value {enuFormType} is not a valid, defaulting to 1 (text)");
                }
            }
            catch (ArgumentOutOfRangeException e)
            {
                _logger.Warn(e.Message, e);
                return "text";
            }
        }

        private void AddValueNotes(Value variableValue, Dataset dataset, DatasetDimensionValue dimensionValue)
        {
            if (variableValue.Notes == null) return;
            foreach (var note in variableValue.Notes)
            {
                dataset.AddValueNoteToDimension(dimensionValue, note.Mandantory, note.Text);
            }
        }

        private void AddVariableNotes(Variable variable, Dataset dataset, DatasetDimensionValue dimensionValue)
        {
            if (variable.Notes == null) return;
            foreach (var note in variable.Notes)
            {
                dataset.AddNoteToDimension(dimensionValue, note.Mandantory, note.Text);
            }
        }

        private void AddContacts(Dataset dataset, string contactString)
        {
            if (dataset.Extension.Contact != null) return;
            var contacts = contactString.Split(new[] { "##" },
                StringSplitOptions.RemoveEmptyEntries);

            foreach (var contact in contacts)
            {
                var contactArray = contact.Split('#');

                //Temporary solution for contact information
                if (contactArray.Length <= 0) continue;

                if (contactArray.Length == 3)
                {
                    dataset.AddContact(contactArray[0], contactArray[1], contactArray[2], contact);
                }
                else
                {
                    dataset.AddContact(contact);
                }
            }
        }

        private void AddRoles(Variable variable, Dataset dataset)
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

        private void CollectMetaIdsForValue(Value value, ref Dictionary<string, string> metaIds)
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

        private void GetValueAndStatus(PXModel model, out List<double?> value, out Dictionary<string, string> status)
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
