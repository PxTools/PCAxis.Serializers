using System;
using System.Collections.Generic;
using System.Linq;

using Parquet.Rows;
using Parquet.Schema;

using PCAxis.Paxiom;
using PCAxis.Paxiom.Operations;

namespace PCAxis.Serializers
{
    public class ParquetBuilder
    {
        private readonly PXModel model;
        private readonly Dictionary<string, DateTime> parseCache = new Dictionary<string, DateTime>();
        private readonly Dictionary<double, string> dataSymbolMap = new Dictionary<double, string>();


        /// <summary>
        /// Constructs a new instance of the ParquetBuilder class with the specified PXModel.
        /// </summary>
        /// <param name="model">The PXModel to be used for constructing the Parquet table.</param>
        public ParquetBuilder(PXModel model)
        {
            this.model = RearrangeValues(model);
            this.dataSymbolMap = BuildDataSymbolMap(model.Meta);
        }

        /// <summary>
        /// Builds a mapping between PX constants representing data symbols and their corresponding string representations in the PXMeta.
        /// </summary>
        /// <param name="meta">The PXMeta object containing metadata.</param>
        /// <returns>A dictionary where keys are PX constants for data symbols and values are their string representations.</returns>
        private Dictionary<double, string> BuildDataSymbolMap(PXMeta meta)
        {
            return new Dictionary<double, string>
        {
            { PXConstant.DATASYMBOL_1, string.IsNullOrEmpty(meta.DataSymbol1) ? PXConstant.DATASYMBOL_1_STRING : meta.DataSymbol1 },
            { PXConstant.DATASYMBOL_2, string.IsNullOrEmpty(meta.DataSymbol2) ? PXConstant.DATASYMBOL_2_STRING : meta.DataSymbol2 },
            { PXConstant.DATASYMBOL_3, string.IsNullOrEmpty(meta.DataSymbol3) ? PXConstant.DATASYMBOL_3_STRING : meta.DataSymbol3 },
            { PXConstant.DATASYMBOL_4, string.IsNullOrEmpty(meta.DataSymbol4) ? PXConstant.DATASYMBOL_4_STRING : meta.DataSymbol4 },
            { PXConstant.DATASYMBOL_5, string.IsNullOrEmpty(meta.DataSymbol5) ? PXConstant.DATASYMBOL_5_STRING : meta.DataSymbol5 },
            { PXConstant.DATASYMBOL_6, string.IsNullOrEmpty(meta.DataSymbol6) ? PXConstant.DATASYMBOL_6_STRING : meta.DataSymbol6 },
            { PXConstant.DATASYMBOL_7, string.IsNullOrEmpty(meta.DataSymbolSum) ? PXConstant.DATASYMBOL_7_STRING : meta.DataSymbolSum },
            { PXConstant.DATASYMBOL_NIL, string.IsNullOrEmpty(meta.DataSymbolNIL) ? PXConstant.DATASYMBOL_NIL_STRING : meta.DataSymbolNIL }
         };
        }

        /// <summary>
        /// Populates the Parquet table based on the PXModel data and metadata.
        /// </summary>
        /// <returns>The populated Parquet table.</returns>
        public Table PopulateTable()
        {
            int matrixSize = model.Data.MatrixColumnCount * model.Data.MatrixRowCount;
            double[] data = new double[matrixSize];
            int[] variableValueCounts = GetVariableValueCounts();
            var indices = GenerateDataPointIndices(variableValueCounts);

            for (int m = 0; m < matrixSize; m++)
            {
                data[m] = model.Data.ReadElement(m);
            }

            List<DataField> dataFields = CreateDataFields();

            var table = new Table(dataFields.ToArray());

            foreach (var index in indices)
            {
                var row = PopulateRow(index, dataFields, variableValueCounts, data);
                table.Add(row);
            }

            return table;
        }

        /// <summary>
        /// Creates the Parquet schema fields based on the PXModel metadata.
        /// </summary>
        /// <returns>The list of Parquet data fields.</returns>
        private List<DataField> CreateDataFields()
        {
            List<DataField> dataFields = new List<DataField>();
            int variableCount = model.Meta.Variables.Count;

            for (int i = 0; i < variableCount; i++)
            {
                var variable = model.Meta.Variables[i];
                if (variable.IsContentVariable && variable.Values.Count > 1)
                {
                    foreach (var value in variable.Values)
                    {
                        string columnName = $"{variable.Code}_{value.Code}";
                        dataFields.Add(new DataField(columnName, typeof(double)));
                        dataFields.Add(new DataField($"{columnName}_symbol", typeof(string))); // Add corresponding symbol column
                    }
                }
                else
                {
                    if (variable.IsContentVariable)
                    {
                        dataFields.Add(new DataField("value", typeof(double)));
                        dataFields.Add(new DataField("value_symbol", typeof(string))); // Add corresponding symbol column
                    }
                    else
                    {
                        if (variable.IsTime)
                        {
                            // Add both the original time-value column and the timestamp column for time variables
                            dataFields.Add(new DataField(variable.Name, typeof(string))); // Original time-value
                            dataFields.Add(new DataField("timestamp", typeof(DateTime))); // Parsed timestamp
                        }
                        else
                        {
                            dataFields.Add(new DataField(variable.Name, typeof(string))); // Non-time variable
                        }
                    }
                }
            }

            return dataFields;
        }



        /// <summary>
        /// Populates a single row in the Parquet table based on the specified index and data.
        /// </summary>
        /// <param name="index">The index representing the position of the row in the PXModel data.</param>
        /// <param name="dataFields">The list of Parquet data fields representing the schema.</param>
        /// <param name="variableValueCounts">The counts of values for each variable in the model.</param>
        /// <param name="data">The array containing the PXModel data.</param>
        /// <returns>The populated Parquet row.</returns>
        private object[] PopulateRow(int[] index, List<DataField> dataFields, int[] variableValueCounts, double[] data)
        {
            int variableCount = model.Meta.Variables.Count;
            var row = new object[dataFields.Count];
            Dictionary<string, int> dataFieldIndices = dataFields.Select((field, idx) => new { field.Name, idx })
                                                                 .ToDictionary(x => x.Name, x => x.idx);

            for (int i = 0; i < variableCount; i++)
            {
                var variable = model.Meta.Variables[i];

                if (variable.IsContentVariable && variable.Values.Count > 0)
                {
                    for (int j = 0; j < variable.Values.Count; j++)
                    {
                        string columnName;
                        string symbolColumnName;

                        if (variable.Values.Count > 1)
                        {
                            var value = variable.Values[j];
                            columnName = $"{variable.Code}_{value.Code}";
                            symbolColumnName = $"{columnName}_symbol"; // Corresponding symbol column name
                        }
                        else
                        {
                            columnName = "value";
                            symbolColumnName = "value_symbol"; // Corresponding symbol column name
                        }

                        int columnIndex = dataFieldIndices[columnName];
                        int symbolColumnIndex = dataFieldIndices[symbolColumnName]; // Get index of the symbol column
                        int dataIndex = GetDataIndex(index, variableValueCounts);

                        if (dataIndex + j < data.Length)
                        {
                            dataIndex += j;
                        }

                        if (dataIndex >= 0 && dataIndex < data.Length)
                        {
                            if (dataSymbolMap.ContainsKey(data[dataIndex]))
                            {
                                row[symbolColumnIndex] = dataSymbolMap[data[dataIndex]]; // Set the symbol value
                                row[columnIndex] = double.NaN; // Replace the value with double.NaN
                            }
                            else
                            {
                                row[columnIndex] = data[dataIndex];
                                row[symbolColumnIndex] = null; // No symbol
                            }
                        }
                    }
                }
                else
                {
                    var value = variable.Values[index[i]].Code;
                    if (variable.IsTime)
                    {
                        value = variable.Values[index[i]].TimeValue;
                        row[dataFieldIndices[variable.Name]] = value; // Original time-value
                        row[dataFieldIndices["timestamp"]] = ParseTimeScale(value, variable.TimeScale); // Parsed timestamp
                    }
                    else
                    {
                        row[dataFieldIndices[variable.Name]] = value;
                    }
                }
            }

            return row;
        }


        /// <summary>
        /// Parses a string representation of a time scale value and returns the start date of the time scale interval.
        /// </summary>
        /// <param name="value">A string representing the time scale value to parse.</param>
        /// <param name="timeScaleType">The type of time scale (Annual, Halfyear, Quarterly, Monthly, Weekly).</param>
        /// <returns>A DateTime object representing the start date of the time scale interval.</returns>
        /// <exception cref="ArgumentException">Thrown if an invalid TimeScaleType or value is provided.</exception>
        private DateTime ParseTimeScale(string value, TimeScaleType timeScaleType)
        {

            // Check cache first
            if (parseCache.TryGetValue(value, out DateTime cachedDate))
            {
                return cachedDate;
            }

            switch (timeScaleType)
            {
                case TimeScaleType.Annual:
                    if (int.TryParse(value, out int year))
                    {
                        parseCache[value] = new DateTime(year, 1, 1);
                        return parseCache[value];
                    }
                    break;

                case TimeScaleType.Halfyear:
                    if (int.TryParse(value.Substring(0, 4), out int halfYearYear) && int.TryParse(value.Substring(4), out int halfYear))
                    {
                        int monthHalfyear = halfYear == 1 ? 1 : 7;  // For half year 1, it gives month 1 (January), for half year 2 it gives month 7 (July).
                        parseCache[value] = new DateTime(halfYearYear, monthHalfyear, 1);
                        return parseCache[value];
                    }
                    break;

                case TimeScaleType.Quartely:
                    if (int.TryParse(value.Substring(0, 4), out int quarterYear) && int.TryParse(value.Substring(4), out int quarter))
                    {
                        int monthQuarter = (quarter - 1) * 3 + 1;  // For quarter 1, this gives month 1 (January), for quarter 2 it gives 4 (April), and so on.
                        parseCache[value] = new DateTime(quarterYear, monthQuarter, 1);
                        return parseCache[value];
                    }
                    break;

                case TimeScaleType.Monthly:
                    if (int.TryParse(value.Substring(0, 4), out int monthYear) && int.TryParse(value.Substring(4, 2), out int month))
                    {
                        parseCache[value] = new DateTime(monthYear, month, 1);
                        return parseCache[value];
                    }
                    break;

                // This time ISO-8601 compliant.
                case TimeScaleType.Weekly:
                    if (int.TryParse(value.Substring(0, 4), out int weekYear) && int.TryParse(value.Substring(4), out int week))
                    {
                        DateTime jan1 = new DateTime(weekYear, 1, 1);
                        // Find the first Thursday of the year
                        DateTime firstThursday = jan1.AddDays((DayOfWeek.Thursday + 7 - jan1.DayOfWeek) % 7);
                        // Calculate the start date of the first week
                        DateTime firstWeekStart = firstThursday.AddDays(-(int)firstThursday.DayOfWeek + (int)DayOfWeek.Monday);
                        // Calculate the start date of the desired week
                        DateTime weekStartDate = firstWeekStart.AddDays((week - 1) * 7);

                        parseCache[value] = weekStartDate;
                        return parseCache[value];
                    }
                    break;

            }

            throw new ArgumentException("Invalid TimeScaleType or value.");
        }

        /// <summary>
        /// Retrieves the variable value counts from the PXModel.
        /// </summary>
        /// <returns>An array of integers representing the counts of values for each variable in the model.</returns>
        private int[] GetVariableValueCounts()
        {
            return model.Meta.Variables
                .Select(v => v.Values.Count)
                .ToArray();
        }

        int GetDataIndex(int[] index, int[] variableValueCounts)
        {
            int dataIndex = 0;
            int multiplier = 1;

            for (int i = index.Length - 1; i >= 0; i--)
            {
                dataIndex += index[i] * multiplier;
                if (i < variableValueCounts.Length - 1) // Adjusting the condition here
                {
                    multiplier *= variableValueCounts[i + 1];
                }
            }

            return dataIndex;
        }

        /// <summary>
        /// Rearranges the values in the PXModel based on the variable descriptions.
        /// </summary>
        /// <param name="model">The PXModel to be rearranged.</param>
        /// <returns>The rearranged PXModel.</returns>
        private PXModel RearrangeValues(PXModel model)
        {
            var pivotDescriptions = model.Meta.Variables
                .Where(col => !col.IsContentVariable)
                .Select(contentCol => new PivotDescription(contentCol.Name, PlacementType.Stub))
                .ToList();

            if (model.Meta.ContentVariable != null)
            {
                pivotDescriptions.Add(new PivotDescription(model.Meta.ContentVariable.Name, PlacementType.Heading));
            }
            else
            {
                // FIXME: There is no explicit content in a lot of PX Files. Let's fake it.
                // FIXME: There must be a better way than this. Paxiom should have handled this.

                if (model.Meta.ContentVariable == null)
                {
                    #region 
                    var virtualContent = new Variable();
                    var virtualContentValue = new Value();

                    // Instead of using ContentsCode/ContentsValue. "value" is simpler.
                    virtualContent.Name = "value";
                    virtualContent.IsContentVariable = true;

                    virtualContentValue.Value = model.Meta.Contents;

                    virtualContent.Values.Add(virtualContentValue);

                    model.Meta.AddVariable(virtualContent);
                    model.Meta.ContentVariable = virtualContent;
                    #endregion

                    pivotDescriptions.Add(new PivotDescription(model.Meta.ContentVariable.Name, PlacementType.Heading));
                }
            }

            return new Pivot().Execute(model, pivotDescriptions.ToArray());
        }

        /// <summary>
        /// Generates the data point indices for the Parquet table based on the data and variable value countsspecified.
        /// </summary>
        /// <param name="variableValueCounts">An array of integers representing the counts of values for each variable.</param>
        /// <returns>A list of integer arrays representing the data point indices.</returns>
        private static List<int[]> GenerateDataPointIndices(int[] variableValueCounts)
        {
            int variableCount = variableValueCounts.Length;
            int[] variableIndexCounts = new int[variableCount];

            int totalDataPoints = 1;
            for (int i = variableCount - 1; i >= 0; i--)
            {
                variableIndexCounts[i] = totalDataPoints;
                totalDataPoints *= variableValueCounts[i];
            }

            List<int[]> dataPointIndices = new List<int[]>(totalDataPoints);

            for (int dataIndex = 0; dataIndex < totalDataPoints; dataIndex++)
            {
                int[] indices = new int[variableCount];

                int tempIndex = dataIndex;
                for (int variableIndex = 0; variableIndex < variableCount; variableIndex++)
                {
                    indices[variableIndex] = tempIndex % variableValueCounts[variableIndex];
                    tempIndex /= variableValueCounts[variableIndex];
                }

                dataPointIndices.Add(indices);
            }
            return dataPointIndices;
        }
    }
}
