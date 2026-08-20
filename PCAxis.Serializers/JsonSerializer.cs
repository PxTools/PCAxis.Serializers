using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Newtonsoft.Json;

using PCAxis.Paxiom;
using PCAxis.Paxiom.Extensions;
using PCAxis.Paxiom.Operations;

namespace PCAxis.Serializers
{
    public class JsonSerializer : PCAxis.Paxiom.IPXModelStreamSerializer
    {
        public void Serialize(PXModel model, string path)
        {
            Serialize(model, new FileStream(path, FileMode.Create));
        }

        public void Serialize(PXModel model, Stream stream)
        {
            //using (var writer = new StreamWriter(stream, Encoding.UTF8))
            //{
            var writer = new StreamWriter(stream, new UTF8Encoding(false));
            var tableResponse = new TableResponse();
            PXModel pivotedModel = RearrangeValues(model);
            var formatter = new DataFormatter(pivotedModel);
            formatter.DecimalSeparator = ".";
            formatter.ThousandSeparator = "";

            string pxDateString;

            if (model.Meta.ContentVariable != null && model.Meta.ContentVariable.Values.Count > 0)
            {
                var lastUpdatedContentsVariable = model.Meta.ContentVariable.Values.OrderByDescending(x => x.ContentInfo.LastUpdated).FirstOrDefault();
                pxDateString = lastUpdatedContentsVariable.ContentInfo.LastUpdated;
            }
            else if (model.Meta.ContentInfo.LastUpdated != null)
            {
                pxDateString = model.Meta.ContentInfo.LastUpdated;
            }
            else
            {
                pxDateString = model.Meta.CreationDate;
            }

            // Add Metadata
            tableResponse.Metadata.Add(new TableResponseMetadata
            {
                Infofile = pivotedModel.Meta.InfoFile,
                Source = pivotedModel.Meta.Source,
                Label = pivotedModel.Meta.Title,
                Updated = pxDateString.PxDateStringToDateTime().ToString()
            });

            // Add stub
            tableResponse.Columns.AddRange(pivotedModel.Meta.Stub.Select(s => new TableResponseColumn
            {
                Code = s.Code,
                Text = s.Name,
                Type = s.IsTime ? "t" : "d",
                Comment = s.HasNotes() ? s.Notes.GetAllNotes() : null
            }));

            if (pivotedModel.Meta.ContentVariable != null)
            {
                // Add heading
                tableResponse.Columns.AddRange(pivotedModel.Meta.ContentVariable.Values.Select(val => new TableResponseColumn
                {
                    Code = val.Code,
                    Text = val.Text,
                    Type = "c",
                    Comment = val.HasNotes() ? val.Notes.GetAllNotes() : null
                }));
            }
            else
            {
                tableResponse.Columns.Add(new TableResponseColumn
                {
                    Code = pivotedModel.Meta.Contents,
                    Text = pivotedModel.Meta.Contents,
                    Type = "c"
                });
            }

            // Add comments
            foreach (var variable in pivotedModel.Meta.Stub)
            {
                foreach (var value in variable.Values)
                {
                    if (value.HasNotes())
                    {
                        tableResponse.Comments.Add(new TableResponseComment
                        {
                            Comment = value.Notes.GetAllNotes(),
                            Value = value.Code,
                            Variable = variable.Code
                        });
                    }
                }
            }

            int row = 0;
            Build(pivotedModel, formatter, 0, ref row, tableResponse, new List<string>());


            // Write to output stream
            writer.Write(tableResponse.ToJSON(false));
            writer.Flush();
            // } End using
        }

        /// <summary>
        /// Builds the table response data object recursively.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="formatter"></param>
        /// <param name="varIdx"></param>
        /// <param name="row"></param>
        /// <param name="response"></param>
        /// <param name="key"></param>
        private void Build(PXModel model, DataFormatter formatter, int varIdx, ref int row, TableResponse response, List<string> key)
        {
            foreach (var value in model.Meta.Stub[varIdx].Values)
            {
                if (varIdx + 1 < model.Meta.Stub.Count)
                {
                    // Continue building
                    Build(model, formatter, varIdx + 1, ref row, response, new List<string>(key) { value.Code });
                }
                else
                {
                    // No more variables. Output key and data
                    var data = new TableResponseData
                    {
                        Key = new List<string>(key) { value.Code },
                    };

                    for (int col = 0; col < model.Data.MatrixColumnCount; col++)
                    {
                        data.Values.Add(formatter.ReadElement(row, col));

                    }
                    response.Data.Add(data);
                    row++;
                }
            }
        }

        /// <summary>
        /// Pivots the data to fit the output format
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private PXModel RearrangeValues(PXModel model)
        {
            var nonContentVariables = model.Meta.Variables.Where(v => v.IsContentVariable == false);
            var pivotDescriptions = nonContentVariables.Select(cv => new PivotDescription(cv.Name, PlacementType.Stub)).ToList();
            if (model.Meta.ContentVariable != null)
            {
                pivotDescriptions.Add(new PivotDescription(model.Meta.ContentVariable.Name, PlacementType.Heading));
            }
            return new Pivot().Execute(model, pivotDescriptions.ToArray());
        }
        #region IWebSerializer Members


        //public void Serialize(PCAxis.Paxiom.PXModel model, ResponseBucket cacheResponse)
        //{
        //    cacheResponse.ContentType = "application/json; charset=" + System.Text.Encoding.UTF8.WebName;

        //    using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
        //    {
        //        Serialize(model, stream);
        //        stream.Flush();
        //        cacheResponse.ResponseData = stream.ToArray();
        //    }
        //}

        #endregion

    }


    public class TableResponse
    {


        public TableResponse()
        {

            Columns = new List<TableResponseColumn>();
            Comments = new List<TableResponseComment>();
            Data = new List<TableResponseData>();
            Metadata = new List<TableResponseMetadata>();
        }

        [JsonProperty("columns")]
        public List<TableResponseColumn> Columns { get; set; }

        [JsonProperty("comments")]
        public List<TableResponseComment> Comments { get; set; }

        [JsonProperty("data")]
        public List<TableResponseData> Data { get; set; }

        [JsonProperty("metadata")]
        public List<TableResponseMetadata> Metadata { get; set; }
    }

    public class TableResponseColumn
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("comment", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Comment { get; set; }

        [JsonProperty("type", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Type { get; set; }

        [JsonProperty("unit", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Unit { get; set; }
    }

    public class TableResponseComment
    {
        [JsonProperty("variable")]
        public string Variable { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("comment")]
        public string Comment { get; set; }
    }

    public class TableResponseData
    {
        public TableResponseData()
        {
            Values = new List<string>();
        }

        [JsonProperty("key")]
        public List<string> Key { get; set; }

        [JsonProperty("values")]
        public List<string> Values { get; set; }

        [JsonProperty("comments", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<string> Comments { get; set; }
    }

    public class TableResponseMetadata
    {
        private DateTime _updated;

        [JsonProperty("infofile")]
        public string Infofile { get; set; }

        [JsonProperty("updated")]
        public string Updated
        {
            get { return _updated.ToString("yyyy-MM-ddTHH:mm:ssZ"); }
            set { _updated = DateTime.Parse(value).ToUniversalTime(); }
        }

        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }


    }
    public static class JsonHelper
    {
        private static string SerializeJson(object obj, bool prettyPrint)
        {
            return JsonConvert.SerializeObject(obj, prettyPrint ? Formatting.Indented : Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }

        public static string ToJSON(this object obj, bool prettyPrint)
        {
            return SerializeJson(obj, prettyPrint);
        }

        public static object Deserialize<T>(string data)
        {
            return JsonConvert.DeserializeObject<T>(data);
        }
    }
}
