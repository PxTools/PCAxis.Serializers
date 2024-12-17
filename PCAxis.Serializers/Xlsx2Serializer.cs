using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using ClosedXML.Excel;
using PCAxis.Serializers.Excel;
using PCAxis.Paxiom;
using PCAxis.Paxiom.Extensions;
using Value = PCAxis.Paxiom.Value;
using System.Reflection.Emit;
using DocumentFormat.OpenXml.EMMA;
using PCAxis.Serializers.JsonStat2.Model;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Runtime.CompilerServices;
using DocumentFormat.OpenXml.Office2016.Excel;
using Parquet.Rows;

namespace PCAxis.Serializers
{
    public class Xlsx2Serializer : PCAxis.Paxiom.IPXModelStreamSerializer
    {
        public enum LablePreference
        {
            None,
            Code,
            Text,
            BothCodeAndText
        }

        protected enum CellContentType
        {
            Undefined,
            Title,
            Code,
            Stub,
            Head,
            Data,
            Footnote,
            Info,
            DataNote,
            VariableNote,
            ValueNote,
            CellNote,
            Comment
        }

        #region "Public properties"
        public LablePreference ValueLablesDisplay { get; set; } = LablePreference.None;

        public bool IncludeTitle { get; set; } = false;

        #endregion

        private bool _showDataNoteCells = false;
        private DataNotePlacementType _modelDataNotePlacement;


        public void Serialize(PCAxis.Paxiom.PXModel model, System.IO.Stream stream)
        {
            Serialize(model, (object)stream);
        }

        public void Serialize(PCAxis.Paxiom.PXModel model, string path)
        {
            Serialize(model, (object)path);
        }

        private void Serialize(PCAxis.Paxiom.PXModel model, object output)
        {
            XLWorkbook book = CreateWorkbook(model);

            AdditionalSheetFunctionality(book.Worksheet(model.Meta.Matrix), model);

            if (book != null)
            {
                //book.Worksheet(model.Meta.Matrix).Columns().AdjustToContents(2, 2, 40);
                if (output is System.IO.Stream)
                    book.SaveAs((System.IO.Stream)output);
                else if (output is string)
                    book.SaveAs(output.ToString());
                book.Dispose();
            }
        }

        protected virtual void AdditionalSheetFunctionality(IXLWorksheet sheet, PXModel model)
        {
        }

        protected delegate void FormatCellDescription(IXLCell cell);

        private void SetCell(IXLCell cell, CellContentType type, object value, FormatCellDescription changes)
        {
            SetCellValue(cell, type, value);
            SetCellFormat(cell, type, value, changes);
        }

        protected virtual void SetCellValue(IXLCell cell, CellContentType type, object value)
        {
            if (value != null)
                if (type == CellContentType.Comment)
                    cell.GetComment().AddText(value.ToString());
                else
                    cell.SetValue(value); //Change from cell.Value = value to SetValue(..) For not format e.g 10-11 to date
        }

        protected virtual void SetCellFormat(IXLCell cell, CellContentType type, object value, FormatCellDescription changes)
        {
            changes?.Invoke(cell);
        }

        /// <summary>
        /// Creates a workbook and writes all the data from the model to it.
        /// </summary>
        /// <param name="model">The model with the data</param>
        /// <returns>A excel workbook with all data written to it.</returns>
        private XLWorkbook CreateWorkbook(PCAxis.Paxiom.PXModel model)
        {

            var currentCulture = Thread.CurrentThread.CurrentCulture;
            try
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

                // Create the workbook
                var book = new XLWorkbook();
                var sheet = book.Worksheets.Add(model.Meta.Matrix);

                // Writes the title
                WriteTableTitle(model, sheet);

                // Creates and initializes the dataformatter
                DataFormatter fmt = CreateDataFormater(model);

                // Initialize data notes variables
                InitializeDataNotes(model, fmt);

                // Writes the heading for the table
                WriteHeading(model, sheet);

                // Writes values for the stub and data cells
                WriteAllRows(model, sheet, fmt);

                // Writes the information             
                WriteAllTableExtraMetadata(model, sheet);

                return book;
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = currentCulture;
            }
        }

        /// <summary>
        /// Writes the title of the table to the Excel sheet.
        /// </summary>
        /// <param name="model">The model containing the title</param>
        /// <param name="sheet">The sheet to write to</param>
        private void WriteTableTitle(PXModel model, IXLWorksheet sheet)
        {
            if (IncludeTitle)
            {
                SetCell(
                    sheet.Cell(1, 1),
                    CellContentType.Title,
                    model.Meta.DescriptionDefault ? model.Meta.Description : model.Meta.Title,
                    c => { c.Style.Font.FontSize = 14; c.Style.Font.Bold = true; }
                );
            }
        }

        /// <summary>
        /// Initializes how data notes should be shown in the Excel sheet.
        /// </summary>
        /// <param name="model">The model conatining the data</param>
        /// <param name="fmt">The data formatter used to format data</param>
        private void InitializeDataNotes(PXModel model, DataFormatter fmt)
        {
            _showDataNoteCells = (model.Meta.DataNoteCells.Count > 0);
            _modelDataNotePlacement = fmt.DataNotePlacment;

            if (_modelDataNotePlacement == DataNotePlacementType.None)
            {
                //Make sure we do not show any datanotecells
                _showDataNoteCells = false;
            }
        }

        /// <summary>
        /// Write the heading of the table to the Excel sheet.
        /// </summary>
        /// <param name="model">The model containg the data</param>
        /// <param name="sheet">The sheet where the data should be written</param>
        private void WriteHeading(PXModel model, IXLWorksheet sheet)
        {
            //Calc left indention caused by the stub
            int indentation = CalculateLeftIndentation(model);

            //HEADING
            for (int headingRow = 0; headingRow < model.Meta.Heading.Count; headingRow++)
            {
                //Calculates the repeats of cells the heading should be repeated iself
                int repeatInterval = CalcHeadingRepeatInterval(headingRow, model);

                //Calculates how many times the heading should be repeated
                int numberOfRepeats = CalcHeadingRepeats(headingRow, model);

                WriteHeaderVariable(model.Meta.Heading[headingRow], repeatInterval, numberOfRepeats, indentation, 3 + model.Meta.Heading.Count, sheet);
            }
        }

        /// <summary>
        /// Writes the stub labels and the data to the Excel sheet.
        /// </summary>
        /// <param name="model">The model containing the data</param>
        /// <param name="sheet">The Excel sheet where the data sould be written</param>
        /// <param name="fmt">The data formater used when writing the data</param>
        private void WriteAllRows(PXModel model, IXLWorksheet sheet, DataFormatter fmt)
        {
            string n = string.Empty;
            string dataNote = string.Empty;
            int row, column;
            string value;
            int dataNoteFactor = 1;
            int dataNoteValueOffset = 0;
            int dataNoteNoteOffset = 1;
            if (_showDataNoteCells)
            {
                dataNoteFactor = 2;
                if (_modelDataNotePlacement == DataNotePlacementType.Before)
                {
                    dataNoteValueOffset = 1;
                    dataNoteNoteOffset = 0;
                }
                else
                {
                    dataNoteValueOffset = 0;
                    dataNoteNoteOffset = 1;
                }
            }
            int sIndent = CalculateLeftIndentation(model);
            for (int i = 0; i < model.Data.MatrixRowCount; i++)
            {

                for (int k = 0; k < model.Meta.Stub.Count; k++)
                {
                    GetStubCell(model, sheet, k, i);
                }
                for (int j = 0; j < model.Data.MatrixColumnCount; j++)
                {
                    row = 3 + model.Meta.Heading.Count + i;
                    column = j * dataNoteFactor + sIndent + 1;
                    value = fmt.ReadElement(i, j, ref n, ref dataNote);

                    SetCell(
                        sheet.Cell(row, column + dataNoteValueOffset),
                        CellContentType.Data,
                        value,
                        !value.IsNumeric() ?
                            (FormatCellDescription)(c => { c.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right; })
                            :
                            (FormatCellDescription)(c => { c.DataType = XLDataType.Number; c.Style.NumberFormat.Format = FormatNumericCell(GetDecimalPrecision(value, fmt.DecimalSeparator)); })
                    );
                    if (!string.IsNullOrEmpty(n))
                    {
                        SetCell(
                            sheet.Cell(row, column + dataNoteValueOffset),
                            CellContentType.Comment,
                            n,
                            null
                        );
                    }

                    if (_showDataNoteCells && !String.IsNullOrEmpty(dataNote))
                    {
                        SetCell(
                            sheet.Cell(row, column + dataNoteNoteOffset),
                            CellContentType.DataNote,
                            dataNote,
                            null
                        );
                    }

                }

            }
        }

        /// <summary>
        /// Writes all the extra metadata like footnotes and other information underneith the table.
        /// </summary>
        /// <param name="model">The model containing the infromation</param>
        /// <param name="sheet">The Excel sheet where the data sould be written</param>
        /// <returns>The row where the last information was written</returns>
        private int WriteAllTableExtraMetadata(PXModel model, IXLWorksheet sheet)
        {

            int r = model.Data.MatrixRowCount + model.Meta.Heading.Count + 4;

            // Writes footnotes
            r = WriteAllNotes(r, model, sheet);

            // Writes rest of the information
            r = WriteTableInformation(r, model, sheet);

            return r;
        }


        /// <summary>
        /// Writes all footnotes to the Excel sheet.
        /// </summary>
        /// <param name="row">Row position where to start writing data</param>
        /// <param name="model"></param>
        /// <param name="sheet"></param>
        /// <returns>The row where the last information was written</returns>
        private int WriteAllNotes(int row, PXModel model, IXLWorksheet sheet)
        {
            row = WriteTableNotes(row, model, sheet);

            //Writes mandantory variable notes
            row = WriteVariableNotes(row, model, sheet);

            //Writes mandantory value notes
            row = WriteValueNotes(row, model, sheet);

            //Writes mandantory cellnotes 
            row = WriteCellNotes(row, model, sheet);
            return row;
        }

        private int WriteCellNotes(int row, PXModel model, IXLWorksheet sheet)
        {
            Variable var;
            Value val;
            CellNote cn;
            VariableValuePair vvp;
            for (int i = 0; i < model.Meta.CellNotes.Count; i++)
            {
                cn = model.Meta.CellNotes[i];
                for (int j = 0; j < cn.Conditions.Count; j++)
                {
                    vvp = cn.Conditions[j];
                    var = model.Meta.Variables.GetByCode(vvp.VariableCode);
                    val = var.Values.GetByCode(vvp.ValueCode);
                    SetCell(
                        sheet.Cell(row, 1),
                        CellContentType.CellNote,
                        var.Name + ":",
                        null
                    );
                    row++;
                    SetCell(
                        sheet.Cell(row, 1),
                        CellContentType.CellNote,
                        val.Value + ":",
                        null
                    );
                    row++;
                }
                SetCell(
                    sheet.Cell(row, 1),
                    CellContentType.CellNote,
                    cn.Text,
                    null
                );
                row += 2;
            }

            return row;
        }

        private int WriteValueNotes(int row, PXModel model, IXLWorksheet sheet)
        {
            Variable var;
            Value val;
            Note n;
            for (int i = 0; i < model.Meta.Variables.Count; i++)
            {
                var = model.Meta.Variables[i];
                for (int j = 0; j < var.Values.Count; j++)
                {
                    val = var.Values[j];
                    if (val.HasNotes())
                    {
                        for (int k = 0; k < val.Notes.Count; k++)
                        {
                            n = val.Notes[k];
                            SetCell(
                                sheet.Cell(row, 1),
                                CellContentType.ValueNote,
                                var.Name + ":",
                                null
                            );
                            row++;
                            SetCell(
                                sheet.Cell(row, 1),
                                CellContentType.ValueNote,
                                val.Value + ":",
                                null
                            );
                            row++;
                            SetCell(
                                sheet.Cell(row, 1),
                                CellContentType.ValueNote,
                                n.Text,
                                null
                            );
                            row += 2;
                            
                        }
                    }
                }
            }
            return row;
        }

        private int WriteVariableNotes(int row, PXModel model, IXLWorksheet sheet)
        {
            Variable var;
            Note n;
            for (int i = 0; i < model.Meta.Variables.Count; i++)
            {
                var = model.Meta.Variables[i];
                if (var.HasNotes())
                {
                    for (int j = 0; j < var.Notes.Count; j++)
                    {
                        n = var.Notes[j];
                        
                        SetCell(
                            sheet.Cell(row, 1),
                            CellContentType.VariableNote,
                            var.Name + ":",
                            null
                        );
                        row++;

                        SetCell(
                            sheet.Cell(row, 1),
                            CellContentType.VariableNote,
                            n.Text,
                            null
                        );

                        row += 2;
                        
                    }
                }
            }
            return row;
        }

        private int WriteTableNotes(int row, PXModel model, IXLWorksheet sheet)
        {
            Note n;
            //Writes mandantory table notes
            for (int i = 0; i < model.Meta.Notes.Count; i++)
            {
                n = model.Meta.Notes[i];
                
                    SetCell(
                        sheet.Cell(row, 1),
                        CellContentType.Footnote,
                        n.Text,
                        c => c.Style.Alignment.WrapText = false
                    );
                    row++;
            }
            return row;
        }


        private int WriteTableInformation(int row, PXModel model, IXLWorksheet sheet)
        {

            var meta = model.Meta;

            // Writes the table specific information
            row = WriteTableInformationValue(row, meta.GetLocalizedString("PxcKeywordDatabase") + ":", meta.Database, sheet);
            row = WriteTableInformationValue(row, meta.GetLocalizedString("PxcKeywordMatrix") + ":", meta.Matrix, sheet);
            row = WriteTableInformationValue(row, meta.GetLocalizedString("PxcKeywordSource") + ":", meta.Source, sheet);

            if (model.Meta.OfficialStatistics)
            {
                SetCell(
                    sheet.Cell(row++, 1),
                    CellContentType.Info,
                    model.Meta.GetLocalizedString("PxcKeywordOfficialStatistics"),
                    null
                );
            }

            if (model.Meta.Copyright)
            {
                SetCell(
                    sheet.Cell(row++, 1),
                    CellContentType.Info,
                    model.Meta.GetLocalizedString("PxcKeywordCopyright"),
                    null
                );
            }

            row = WriteTableInformationContacts(row, meta, sheet);

            //Check if the information is attached on the table or on the values on the content variable
            if (meta.ContentVariable != null && meta.ContentVariable.Values.Count > 0 && meta.ContentInfo is null)
            {
                row = WriteTableInformationFromContentVariable(row, model, sheet);
            }
            else
            {
                row = WriteTableInformationFromTable(row, model, sheet);
            }

            return row;
        }

        private int WriteTableInformationFromContentVariable(int row, PXModel model, IXLWorksheet sheet)
        {

            var meta = model.Meta;

            row = WriteTableInformationValue(row, meta.GetLocalizedString("PxcKeywordLastUpdated") + ":", meta, (c) => c.LastUpdated, sheet);
            row = WriteTableInformationValue(row, meta.GetLocalizedString("PxcKeywordUnits") + ":", meta, (c) => c.Units, sheet);
            row = WriteTableInformationValue(row, meta.GetLocalizedString("PxcKeywordStockfa") + ":", meta, (c) => c.StockFa, sheet, (s ,m) => ConvertStockFlowAverageToLocalText(s, m));
            row = WriteTableInformationValue(row, meta.GetLocalizedString("PxcKeywordBasePeriod") + ":", meta, (c) => c.Baseperiod, sheet);
            row = WriteTableInformationValue(row, "", meta, (c) => c.CFPrices, sheet, (s,m) => ConvertCurrentOrFiexedPricesToLocalText(s,m));
            row = WriteTableInformationBooleanValue(row, meta.GetLocalizedString("PxcKeywordDayAdj") + ":", meta, (c) => c.DayAdj, sheet);
            row = WriteTableInformationBooleanValue(row, meta.GetLocalizedString("PxcKeywordSeasAdj") + ":", meta, (c) => c.SeasAdj, sheet);

            if (!String.IsNullOrEmpty(model.Meta.ContentInfo.SeasAdj) && model.Meta.ContentInfo.SeasAdj.ToUpper().Equals("YES"))
            {
                SetCell(
                    sheet.Cell(row++, 1),
                    CellContentType.Info,
                    model.Meta.GetLocalizedString("PxcKeywordSeasAdj"),
                    null
                );
            }

            return row;
        }

        private int WriteTableInformationFromTable(int row, PXModel model, IXLWorksheet sheet)
        {

            var meta = model.Meta;

            row = WriteTableInformationValue(row, meta.GetLocalizedString("PxcKeywordLastUpdated") + ":", meta.ContentInfo.LastUpdated, sheet);
            row = WriteTableInformationValue(row, meta.GetLocalizedString("PxcKeywordUnits") + ":", meta.ContentInfo.Units, sheet);
            row = WriteTableInformationValue(row, meta.GetLocalizedString("PxcKeywordStockfa") + ":", ConvertStockFlowAverageToLocalText(meta.ContentInfo.StockFa, meta), sheet);
            row = WriteTableInformationValue(row, meta.GetLocalizedString("PxcKeywordBasePeriod") + ":", meta.ContentInfo.Baseperiod, sheet);
            row = WriteTableInformationValue(row, "", ConvertCurrentOrFiexedPricesToLocalText(meta.ContentInfo.CFPrices, meta), sheet);

            if (!String.IsNullOrEmpty(model.Meta.ContentInfo.DayAdj) && model.Meta.ContentInfo.DayAdj.ToUpper().Equals("YES"))
            {
                SetCell(
                    sheet.Cell(row++, 1),
                    CellContentType.Info,
                    model.Meta.GetLocalizedString("PxcKeywordDayAdj"),
                    null
                );
            }

            if (!String.IsNullOrEmpty(model.Meta.ContentInfo.SeasAdj) && model.Meta.ContentInfo.SeasAdj.ToUpper().Equals("YES"))
            {
                SetCell(
                    sheet.Cell(row++, 1),
                    CellContentType.Info,
                    model.Meta.GetLocalizedString("PxcKeywordSeasAdj"),
                    null
                );
            }

            return row;
        }

        #region "Converter functions"

        private static string ConvertStockFlowAverageToLocalText(string stockFa, PXMeta meta)
        {
            switch (stockFa.ToUpper())
            {
                case "S":
                    return meta.GetLocalizedString("PxcKeywordStockfaValueStock");
                case "F":
                    return meta.GetLocalizedString("PxcKeywordStockfaValueFlow");
                case "A":
                    return meta.GetLocalizedString("PxcKeywordStockfaValueAverage");
                default:
                    return stockFa;
            }
        }

        private static string ConvertCurrentOrFiexedPricesToLocalText(string stockFa, PXMeta meta)
        {
            switch (stockFa.ToUpper())
            {
                case "C":
                    return meta.GetLocalizedString("PxcKeywordCFPricesValueCurrent");
                case "F":
                    return meta.GetLocalizedString("PxcKeywordCFPricesValueFixed");
                default:
                    return stockFa;
            }
        }

        #endregion



        private int WriteTableInformationContacts(int row, PXMeta meta, IXLWorksheet sheet)
        {
            SetCell(
                sheet.Cell(row, 1),
                CellContentType.Info,
                meta.GetLocalizedString("PxcKeywordContact") + ":",
                null
            );
            var memo = new HashSet<string>();
            if (meta.ContentInfo != null)
            {
                row = WriteContact(row, meta.ContentInfo.Contact, sheet, memo);
            }
            else
            {
                foreach(var value in meta.ContentVariable.Values)
                {
                    if (value.ContentInfo != null)
                    {
                        row = WriteContact(row, value.ContentInfo.Contact, sheet, memo);
                    }
                }
            }
            return row;
        }

        private int WriteContact(int row, string contacts, IXLWorksheet sheet, HashSet<string> memo)
        {
            // Skip if contacts is empty
            if (string.IsNullOrEmpty(contacts))
            {
                return row;
            }

            // Split contacts and write them it they have not been written before
            foreach (var contact in contacts.Split('#'))
            {
                // Skip if contact already written
                if (memo.Contains(contact))
                {
                    continue;
                }

                // Remembers that this contact has been written
                memo.Add(contact);

                SetCell(
                    sheet.Cell(row, 1),
                    CellContentType.Info,
                    contact,
                    null
                );
                row++;
            }

            return row;
        }

        private int WriteTableInformationValue(int row, string label, string value, IXLWorksheet sheet)
        {
            //Only write the information if the value is not empty
            if (!string.IsNullOrEmpty(value))
            {
                //Write label
                SetCell(
                    sheet.Cell(row, 1),
                    CellContentType.Info,
                    label,
                    null);

                //Write value
                SetCell(
                    sheet.Cell(row, 2),
                    CellContentType.Info,
                    value,
                    null
                );
                //Increase row
                row++;
            }
            return row;
        }


        private int WriteTableInformationValue(int row, string label, PXMeta meta, Func<ContInfo, string> filter, IXLWorksheet sheet, Func<string, PXMeta, string> converter = null)
        {

            if (converter is null)
            {
                converter = (s, m) => s;
            }

            var map = new Dictionary<string, string>(); 
            foreach (var value in meta.ContentVariable.Values)
            {
                if (value.ContentInfo is null)
                {
                    continue;
                }

                var infoValue = filter(value.ContentInfo);
                infoValue = converter(infoValue, meta);

                if (string.IsNullOrEmpty(infoValue))
                {
                    continue;
                }

                map[infoValue] = map[infoValue] is null ? value.Text : map[infoValue] + ", " + value.Text;
            }

            if (map.Count == 1)
            {
                //All are same
                row = WriteTableInformationValue(row, label, map.First().Value, sheet);
            } else
            {
                //Write label
                SetCell(
                    sheet.Cell(row, 1),
                    CellContentType.Info,
                    label,
                    null);
                row++;
                foreach (var pair in map)
                {
                    row = WriteTableInformationValue(row, pair.Key, pair.Value, sheet);
                }
            }

            return row;
        }

        private int WriteTableInformationBooleanValue(int row, string label, PXMeta meta, Func<ContInfo, string> filter, IXLWorksheet sheet)
        {
            string values = null;
            foreach (var value in meta.ContentVariable.Values)
            {
                if (value.ContentInfo is null)
                {
                    continue;
                }

                var infoValue = filter(value.ContentInfo);

                if (string.IsNullOrEmpty(infoValue))
                {
                    continue;
                }

                if (infoValue.ToUpper().Equals("YES"))
                {
                    values = values is null ? value.Text : values + ", " + value.Text;
                }
            }

            if (!string.IsNullOrEmpty(values))
            {
                //All are same
                row = WriteTableInformationValue(row, label, values, sheet);
            }

            return row;
        }



        private void WriteHeaderVariable(Variable variable, int repeatInterval, int numberOfRepeats, int row, int indentation, IXLWorksheet sheet)
        {
            int column;
            string text;
            string notes;
            for (int valueIndex = 0; valueIndex < variable.Values.Count; valueIndex++)
            {
                text = variable.Values[valueIndex].Text;
                if (variable.Values[valueIndex].HasNotes())
                {
                    notes = variable.Values[valueIndex].Notes.GetAllNotes();
                }
                else
                {
                    notes = null;
                }

                for (int repeat = 0; repeat < numberOfRepeats; repeat++)
                {
                    //indentation + the values in a certain interval + repeated for the number of repeats
                    column = indentation + (valueIndex * repeatInterval) + (repeat * variable.Values.Count * repeatInterval);

                    //TODO double column
                    //TODO make a SetHeaderCell method with 2 args
                    //Writes the value text
                    SetCell(
                        sheet.Cell(row, column),
                        CellContentType.Head,
                        text,
                        c => c.Style.Font.Bold = true
                    );

                    //Writes value notes
                    if (notes != null)
                    {
                        SetCell(
                            sheet.Cell(row, column),
                            CellContentType.Comment,
                            notes,
                            null
                        );
                    }
                }
            }
        }

        private static int CalcHeadingRepeatInterval(int headingLevel, PXModel model)
        {
            int interval = 1;

            for (int i = headingLevel + 1; i < model.Meta.Heading.Count; i++)
            {
                interval *= model.Meta.Heading[i].Values.Count;
            }

            return interval;
        }

        private static int CalcHeadingRepeats(int headingLevel, PXModel model)
        {
            int repeats = 1;
            for (int i = 0; i < headingLevel; i++)
            {
                repeats *= model.Meta.Heading[i].Values.Count;
            }

            return repeats;
        }

        private static int CalculateLeftIndentation(PXModel model)
        {
            int lIndent = 0;
            for (int k = 0; k < model.Meta.Stub.Count; k++)
            {
                lIndent++;
            }
            return lIndent;
        }

        private void GetStubCell(PXModel model, IXLWorksheet sheet, int stubNr, int rowNr)
        {
            int Interval;
            int count = model.Meta.Stub[stubNr].Values.Count;

            if (stubNr < model.Meta.Stub.Count - 1)
            {
                Interval = CalcStubInterval(stubNr + 1, model);
            }
            else
            {
                Interval = 1;
            }

            Value val;
            int row, column;
            if (rowNr % Interval == 0)
            {
                //Dim Cell As New Cell
                int offset = 0;
                val = model.Meta.Stub[stubNr].Values[(rowNr / Interval) % count];
                row = rowNr + 3 + model.Meta.Heading.Count;
                column = stubNr + 1 + offset;

                SetCell(
                    sheet.Cell(row, column),
                    CellContentType.Stub,
                    val.Text,
                    c => c.Style.Font.Bold = true
                );
                if (val.HasNotes())
                {
                    SetCell(
                        sheet.Cell(row, column),
                        CellContentType.Comment,
                        val.Notes.GetAllNotes(),
                        null
                    );
                }
                
            }
        }

        private static int CalcStubInterval(int stubChildNr, PXModel model)
        {
            int interv = 1;
            //Intervall
            for (int i = model.Meta.Stub.Count - 1; i >= stubChildNr; i--)
            {
                interv *= model.Meta.Stub[i].Values.Count;
            }

            return interv;
        }


        static readonly string[] _numberFormats = new string[] { "0", "0.0", "0.00", "0.000", "0.0000", "0.00000", "0.000000" };

        /// <summary>
        /// A method for format the cell in Excel that contains numeric/decimal values.
        /// </summary>
        /// <param name="dfm"></param>
        /// <returns></returns>
        private static string FormatNumericCell(int dfm)
        {
            if (dfm < 0 || dfm > 6)
            {
                return "0.0";
            }

            return _numberFormats[dfm];
        }

        private static int GetDecimalPrecision(string value, string separtor)
        {
            var index = value.IndexOf(separtor);

            if (index < 0)
            {
                return 0;
            }

            try
            {
                return value.Length - index - 1;
            }
            catch (Exception)
            {
                return 0;
            }

        }

        private static DataFormatter CreateDataFormater(PXModel model)
        {
            DataFormatter fmt = new DataFormatter(model);
            fmt.ThousandSeparator = "";
            try
            {
                fmt.DecimalSeparator = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            }
            catch (Exception)
            {
                fmt.DecimalSeparator = ",";
            }

            fmt.InformationLevel = InformationLevelType.AllInformation;
            return fmt;
        }
    }
}
