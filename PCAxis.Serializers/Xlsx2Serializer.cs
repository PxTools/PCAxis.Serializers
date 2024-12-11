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

        public LablePreference ValueLablesDisplay { get; set; } = LablePreference.None;

        public bool IncludeTitle { get; set; } = false;


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
                WriteAllTableExtraMetadata(model, sheet, fmt);

                return book;
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = currentCulture;
            }
        }

        private void WriteAllTableExtraMetadata(PXModel model, IXLWorksheet sheet, DataFormatter fmt)
        {
           
            int r = model.Data.MatrixRowCount + model.Meta.Heading.Count + 4;

            // Writes footnotes
            r = WriteFootnotes(r, model, sheet);

            // Writes rest of the information
            r = WriteTableInformation(r, model, sheet);
        }

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
                    GetStubCell(model, sheet, k, i, IsDoubleColumn(model.Meta.Stub[k]));
                }
                for (int j = 0; j < model.Data.MatrixColumnCount; j++)
                {
                    row = 3 + model.Meta.Heading.Count + i;
                    column = j * dataNoteFactor + sIndent + 1;
                    value = fmt.ReadElement(i, j, ref n, ref dataNote);

                    //TODO: Improve performance of setting value format, takes a lot of CPU at the moment
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
                        //sheet.Cell(row, column + dataNoteValueOffset).Comment.AddText(n);
                        SetCell(
                            sheet.Cell(row, column + dataNoteValueOffset),
                            CellContentType.Comment,
                            n,
                            null
                        );
                    }

                    if (_showDataNoteCells && !String.IsNullOrEmpty(dataNote))
                    {
                        //sheet.Cell(row, column + dataNoteNoteOffset).Value = dataNote;    
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

        private DataFormatter CreateDataFormater(PXModel model)
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

        private int WriteFootnotes(int row, PXModel model, IXLWorksheet sheet)
        {
            int columnCount = sheet.Columns().Count();
            row = WriteMandatoryTableNotes(row, model, sheet);

            //Writes mandantory variable notes
            row = WriteMandatoryVariableNotes(row, model, sheet);

            //Writes mandantory value notes
            row = WriteMandatoryValueNotes(row, model, sheet);

            //Writes mandantory cellnotes 
            row = WriteMandatoryCellNotes(row, model, sheet);
            return row++;
        }

        private int WriteMandatoryCellNotes(int row, PXModel model, IXLWorksheet sheet)
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

        private int WriteMandatoryValueNotes(int row, PXModel model, IXLWorksheet sheet)
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

        private int WriteMandatoryVariableNotes(int row, PXModel model, IXLWorksheet sheet)
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

        private int WriteMandatoryTableNotes(int row, PXModel model, IXLWorksheet sheet)
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
            bool contvar = false;
            Variable var;
            PCAxis.Paxiom.ContInfo info;
            string value;
            Dictionary<string, string> lastUpdated = new Dictionary<string, string>();
            Dictionary<string, string> contact = new Dictionary<string, string>();
            Dictionary<string, string> units = new Dictionary<string, string>();
            Dictionary<string, string> stockfa = new Dictionary<string, string>();
            Dictionary<string, string> refperiod = new Dictionary<string, string>();
            Dictionary<string, string> baseperiod = new Dictionary<string, string>();
            Dictionary<string, string> cfprices = new Dictionary<string, string>();
            Dictionary<string, string> dayadj = new Dictionary<string, string>();
            Dictionary<string, string> seasadj = new Dictionary<string, string>();

            //    With model.Meta
            if (model.Meta.ContentVariable != null && model.Meta.ContentVariable.Values.Count > 0)
            {
                contvar = true;
                var = model.Meta.ContentVariable;

                //1. Collect information for all the values
                //-----------------------------------------
                for (int i = 0; i < var.Values.Count; i++)
                {
                    info = var.Values[i].ContentInfo;
                    value = var.Values[i].Text;

                    if (info != null)
                    {
                        //LAST-UPDATED
                        if (!String.IsNullOrEmpty(info.LastUpdated))
                        {
                            lastUpdated.Add(value, info.LastUpdated);
                        }

                        //CONTACT
                        if (!String.IsNullOrEmpty(info.Contact))
                        {
                            contact.Add(value, info.Contact);
                        }

                        //UNITS
                        if (!String.IsNullOrEmpty(info.Units))
                        {
                            units.Add(value, info.Units);
                        }

                        //STOCKFA
                        if (!String.IsNullOrEmpty(info.StockFa))
                        {
                            stockfa.Add(value, info.StockFa);
                        }

                        //REFPERIOD
                        if (!String.IsNullOrEmpty(info.RefPeriod))
                        {
                            refperiod.Add(value, info.RefPeriod);
                        }

                        //BASEPERIOD
                        if (!String.IsNullOrEmpty(info.Baseperiod))
                        {
                            baseperiod.Add(value, info.Baseperiod);
                        }

                        //CFPRICES
                        if (!String.IsNullOrEmpty(info.CFPrices))
                        {
                            cfprices.Add(value, info.CFPrices);
                        }

                        //DAYADJ
                        if (!String.IsNullOrEmpty(info.DayAdj))
                        {
                            if (info.DayAdj.ToUpper().Equals("YES"))
                            {
                                dayadj.Add(value, info.DayAdj);
                            }
                        }

                        //SEASADJ
                        if (!String.IsNullOrEmpty(info.SeasAdj))
                        {
                            if (info.SeasAdj.ToUpper().Equals("YES"))
                            {
                                seasadj.Add(value, info.SeasAdj);
                            }
                        }
                    }
                }
            }

            //2. Write the collected information
            //----------------------------------

            //LAST-UPDATED
            row++;
            if (contvar)
            {
                //sheet.Cell(row++, 1).Value = model.Meta.GetLocalizedString("PxcKeywordLastUpdated") + ":";
                SetCell(
                    sheet.Cell(row++, 1),
                    CellContentType.Info,
                    model.Meta.GetLocalizedString("PxcKeywordLastUpdated") + ":",
                    null
                );
                foreach (KeyValuePair<string, string> kvp in lastUpdated)
                {
                    //sheet.Cell(row, 1).Value = kvp.Key + ":";
                    SetCell(
                        sheet.Cell(row, 1),
                        CellContentType.Info,
                        kvp.Key + ":",
                        null
                    );
                    row++;
                    //sheet.Cell(row++, 2).Value = kvp.Value;
                    SetCell(
                        sheet.Cell(row++, 1),
                        CellContentType.Info,
                        kvp.Value,
                        null
                    );
                }
            }
            else
            {
                if (model.Meta.ContentInfo != null)
                {
                    if (!String.IsNullOrEmpty(model.Meta.ContentInfo.LastUpdated))
                    {
                        //sheet.Cell(row, 1).Value = model.Meta.GetLocalizedString("PxcKeywordLastUpdated") + ":";
                        SetCell(
                            sheet.Cell(row, 1),
                            CellContentType.Info,
                            model.Meta.GetLocalizedString("PxcKeywordLastUpdated") + ":",
                            null
                        );
                        //TODO DATEFORMAT
                        //sheet.Cell(row++, 2).Value = model.Meta.ContentInfo.LastUpdated;
                        row++;
                        SetCell(
                            sheet.Cell(row++, 1),
                            CellContentType.Info,
                            model.Meta.ContentInfo.LastUpdated,
                            null
                        );
                    }
                }
            }

            //SOURCE
            row++;
            if (!String.IsNullOrEmpty(model.Meta.Source))
            {
                //sheet.Cell(row, 1).Value = model.Meta.GetLocalizedString("PxcKeywordSource") + ":";
                SetCell(
                    sheet.Cell(row, 1),
                    CellContentType.Info,
                    model.Meta.GetLocalizedString("PxcKeywordSource") + ":",
                    null
                );
                //sheet.Cell(row++, 2).Value = model.Meta.Source;
                row++;
                SetCell(
                    sheet.Cell(row++, 1),
                    CellContentType.Info,
                    model.Meta.Source,
                    null
                );
            }

            //CONTACT
            row++;
            if (contvar)
            {
                if (contact.Count > 0)
                {
                    //sheet.Cell(row++, 1).Value = model.Meta.GetLocalizedString("PxcKeywordContact") + ":";
                    SetCell(
                        sheet.Cell(row++, 1),
                        CellContentType.Info,
                        model.Meta.GetLocalizedString("PxcKeywordContact") + ":",
                        null
                    );

                    string[] str;
                    var firstElement = contact.FirstOrDefault(); //Show only first contact person

                    SetCell(
                        sheet.Cell(row, 1),
                        CellContentType.Info,
                        firstElement.Key + ":",
                        null
                    );
                    str = firstElement.Value.Split('#');

                    row++;
                    for (int i = 0; i < str.Length; i++)
                    {
                        SetCell(
                            sheet.Cell(row++, 1),
                            CellContentType.Info,
                            str[i],
                            null
                        );
                    }

                }
            }
            else
            {
                if (model.Meta.ContentInfo != null)
                {
                    if (!String.IsNullOrEmpty(model.Meta.ContentInfo.Contact))
                    {
                        string[] str;

                        //sheet.Cell(row, 1).Value = model.Meta.GetLocalizedString("PxcKeywordContact") + ":";
                        SetCell(
                            sheet.Cell(row, 1),
                            CellContentType.Info,
                            model.Meta.GetLocalizedString("PxcKeywordContact") + ":",
                            null
                        );

                        str = model.Meta.ContentInfo.Contact.Split('#');
                        row++;
                        for (int i = 0; i < str.Length; i++)
                        {
                            //sheet.Cell(row++, 2).Value = str[headingRow];                            
                            SetCell(
                                sheet.Cell(row++, 1),
                                CellContentType.Info,
                                str[i],
                                null
                            );
                        }
                    }
                }
            }

            //COPYRIGHT
            row++;
            if (model.Meta.Copyright)
            {
                //sheet.Cell(row++, 1).Value = model.Meta.GetLocalizedString("PxcKeywordCopyright");
                SetCell(
                    sheet.Cell(row++, 1),
                    CellContentType.Info,
                    model.Meta.GetLocalizedString("PxcKeywordCopyright"),
                    null
                );
            }

            //UNITS
            row++;
            if (contvar)
            {
                if (units.Count > 0)
                {
                    //sheet.Cell(row, 1).Value = model.Meta.GetLocalizedString("PxcKeywordUnits") + ":";
                    SetCell(
                        sheet.Cell(row, 1),
                        CellContentType.Info,
                        model.Meta.GetLocalizedString("PxcKeywordUnits") + ":",
                        null
                    );
                    foreach (KeyValuePair<string, String> kvp in units)
                    {
                        //sheet.Cell(row, 1).Value = kvp.Key + ":";
                        row++;
                        SetCell(
                            sheet.Cell(row++, 1),
                            CellContentType.Info,
                            kvp.Key + ":",
                            null
                        );
                        //sheet.Cell(row++, 2).Value = kvp.Value;
                        SetCell(
                            sheet.Cell(row, 1),
                            CellContentType.Info,
                            kvp.Value,
                            null
                        );
                    }
                }
            }
            else
            {
                if (model.Meta.ContentInfo != null)
                {
                    if (!String.IsNullOrEmpty(model.Meta.ContentInfo.Units))
                    {
                        //sheet.Cell(row, 1).Value = model.Meta.GetLocalizedString("PxcKeywordUnits") + ":";
                        SetCell(
                            sheet.Cell(row, 1),
                            CellContentType.Info,
                            model.Meta.GetLocalizedString("PxcKeywordUnits") + ":",
                            null
                        );
                        //sheet.Cell(row++, 2).Value = model.Meta.ContentInfo.Units;
                        row++;
                        SetCell(
                            sheet.Cell(row++, 1),
                            CellContentType.Info,
                            model.Meta.ContentInfo.Units,
                            null
                        );
                    }
                }
            }

            //STOCKFA
            row++;
            if (contvar)
            {
                if (stockfa.Count > 0)
                {
                    //sheet.Cell(row++, 1).Value = model.Meta.GetLocalizedString("PxcKeywordStockfa") + ":";

                    SetCell(
                        sheet.Cell(row++, 1),
                        CellContentType.Info,
                        model.Meta.GetLocalizedString("PxcKeywordStockfa") + ":",
                        null
                    );

                    foreach (KeyValuePair<String, String> kvp in stockfa)
                    {
                        //sheet.Cell(row, 1).Value = kvp.Key + ":";
                        SetCell(
                            sheet.Cell(row, 1),
                            CellContentType.Info,
                            kvp.Key + ":",
                            null
                        );
                        switch (kvp.Value.ToUpper())
                        {
                            case "S":
                                //sheet.Cell(row++, 2).Value = model.Meta.GetLocalizedString("PxcKeywordStockfaValueStock");
                                row++;
                                SetCell(
                                    sheet.Cell(row++, 1),
                                    CellContentType.Info,
                                    model.Meta.GetLocalizedString("PxcKeywordStockfaValueStock"),
                                    null
                                );
                                break;
                            case "F":
                                //sheet.Cell(row++, 2).Value = model.Meta.GetLocalizedString("PxcKeywordStockfaValueFlow");
                                row++;
                                SetCell(
                                    sheet.Cell(row++, 1),
                                    CellContentType.Info,
                                    model.Meta.GetLocalizedString("PxcKeywordStockfaValueFlow"),
                                    null
                                );
                                break;
                            case "A":
                                //sheet.Cell(row++, 2).Value = model.Meta.GetLocalizedString("PxcKeywordStockfaValueAverage");
                                row++;
                                SetCell(
                                    sheet.Cell(row++, 1),
                                    CellContentType.Info,
                                    model.Meta.GetLocalizedString("PxcKeywordStockfaValueAverage"),
                                    null
                                );
                                break;
                        }
                    }
                }
            }
            else
            {
                if (model.Meta.ContentInfo != null)
                {
                    if (!String.IsNullOrEmpty(model.Meta.ContentInfo.StockFa))
                    {
                        //sheet.Cell(row, 1).Value = model.Meta.GetLocalizedString("PxcKeywordStockfa") + ":";
                        SetCell(
                            sheet.Cell(row, 1),
                            CellContentType.Info,
                            model.Meta.GetLocalizedString("PxcKeywordStockfa") + ":",
                            null
                        );
                        switch (model.Meta.ContentInfo.StockFa.ToUpper())
                        {
                            case "S":
                                //sheet.Cell(row++, 2).Value = model.Meta.GetLocalizedString("PxcKeywordStockfaValueStock");
                                row++;
                                SetCell(
                                    sheet.Cell(row++, 1),
                                    CellContentType.Info,
                                    model.Meta.GetLocalizedString("PxcKeywordStockfaValueStock"),
                                    null
                                );
                                break;
                            case "F":
                                //sheet.Cell(row++, 2).Value = model.Meta.GetLocalizedString("PxcKeywordStockfaValueFlow");
                                row++;
                                SetCell(
                                    sheet.Cell(row++, 1),
                                    CellContentType.Info,
                                    model.Meta.GetLocalizedString("PxcKeywordStockfaValueFlow"),
                                    null
                                );
                                break;
                            case "A":
                                //sheet.Cell(row++, 2).Value = model.Meta.GetLocalizedString("PxcKeywordStockfaValueAverage");
                                row++;
                                SetCell(
                                    sheet.Cell(row++, 1),
                                    CellContentType.Info,
                                    model.Meta.GetLocalizedString("PxcKeywordStockfaValueAverage"),
                                    null
                                );
                                break;
                        }
                    }
                }
            }

            //REFPERIOD
            row++;
            if (contvar)
            {
                if (refperiod.Count > 0)
                {
                    //sheet.Cell(row++, 1).Value = model.Meta.GetLocalizedString("PxcKeywordRefPeriod") + ":";
                    SetCell(
                        sheet.Cell(row++, 1),
                        CellContentType.Info,
                        model.Meta.GetLocalizedString("PxcKeywordRefPeriod") + ":",
                        null
                    );
                    foreach (KeyValuePair<string, string> kvp in refperiod)
                    {
                        //sheet.Cell(row, 1).Value = kvp.Key;
                        SetCell(
                            sheet.Cell(row, 1),
                            CellContentType.Info,
                            kvp.Key,
                            null
                        );
                        //sheet.Cell(row++, 2).Value = kvp.Value;
                        row++;
                        SetCell(
                            sheet.Cell(row++, 1),
                            CellContentType.Info,
                            kvp.Value,
                            null
                        );
                    }
                }
            }
            else
            {
                if (model.Meta.ContentInfo != null)
                {
                    if (!String.IsNullOrEmpty(model.Meta.ContentInfo.RefPeriod))
                    {
                        //sheet.Cell(row, 1).Value = model.Meta.GetLocalizedString("PxcKeywordRefPeriod") + ":";
                        SetCell(
                            sheet.Cell(row, 1),
                            CellContentType.Info,
                            model.Meta.GetLocalizedString("PxcKeywordRefPeriod") + ":",
                            null
                        );
                        //sheet.Cell(row++, 2).Value = model.Meta.ContentInfo.RefPeriod;
                        row++;
                        SetCell(
                            sheet.Cell(row++, 1),
                            CellContentType.Info,
                            model.Meta.ContentInfo.RefPeriod,
                            null
                        );
                    }
                }
            }

            //BASEPERIOD
            row++;
            if (contvar)
            {
                if (baseperiod.Count > 0)
                {
                    //writer.WriteEmptyRow()
                    //sheet.Cell(row, 1).Value = model.Meta.GetLocalizedString("PxcKeywordBasePeriod") + ":";
                    SetCell(
                        sheet.Cell(row, 1),
                        CellContentType.Info,
                        model.Meta.GetLocalizedString("PxcKeywordBasePeriod") + ":",
                        null
                    );
                    foreach (KeyValuePair<string, string> kvp in baseperiod)
                    {
                        //sheet.Cell(row, 1).Value = kvp.Key + ":";
                        SetCell(
                            sheet.Cell(row, 1),
                            CellContentType.Info,
                            kvp.Key + ":",
                            null
                        );
                        //sheet.Cell(row++, 2).Value = kvp.Value;
                        row++;
                        SetCell(
                            sheet.Cell(row++, 1),
                            CellContentType.Info,
                            kvp.Value,
                            null
                        );
                    }
                }
            }
            else
            {
                if (model.Meta.ContentInfo != null)
                {
                    if (!String.IsNullOrEmpty(model.Meta.ContentInfo.Baseperiod))
                    {
                        //sheet.Cell(row, 1).Value = model.Meta.GetLocalizedString("PxcKeywordBasePeriod") + ":";
                        SetCell(
                            sheet.Cell(row, 1),
                            CellContentType.Info,
                            model.Meta.GetLocalizedString("PxcKeywordBasePeriod") + ":",
                            null
                        );
                        //sheet.Cell(row++, 2).Value = model.Meta.ContentInfo.Baseperiod;
                        row++;
                        SetCell(
                            sheet.Cell(row++, 1),
                            CellContentType.Info,
                            model.Meta.ContentInfo.Baseperiod,
                            null
                        );
                    }
                }
            }

            //CFPRICES
            row++;
            if (contvar)
            {
                if (cfprices.Count > 0)
                {
                    foreach (KeyValuePair<string, string> kvp in cfprices)
                    {
                        //sheet.Cell(row, 1).Value = kvp.Key + ":";
                        SetCell(
                            sheet.Cell(row, 1),
                            CellContentType.Info,
                            kvp.Key + ":",
                            null
                        );
                        switch (kvp.Value.ToUpper())
                        {
                            case "C":
                                //sheet.Cell(row++, 2).Value = model.Meta.GetLocalizedString("PxcKeywordCFPricesValueCurrent");
                                row++;
                                SetCell(
                                    sheet.Cell(row++, 1),
                                    CellContentType.Info,
                                    model.Meta.GetLocalizedString("PxcKeywordCFPricesValueCurrent"),
                                    null
                                );
                                break;
                            case "F":
                                //sheet.Cell(row++, 2).Value = model.Meta.GetLocalizedString("PxcKeywordCFPricesValueFixed");
                                row++;
                                SetCell(
                                    sheet.Cell(row++, 1),
                                    CellContentType.Info,
                                    model.Meta.GetLocalizedString("PxcKeywordCFPricesValueFixed"),
                                    null
                                );
                                break;
                        }
                    }
                }
            }
            else
            {
                if (model.Meta.ContentInfo != null)
                {
                    if (!String.IsNullOrEmpty(model.Meta.ContentInfo.CFPrices))
                    {
                        switch (model.Meta.ContentInfo.CFPrices.ToUpper())
                        {
                            case "C":
                                //sheet.Cell(row, 1).Value = model.Meta.GetLocalizedString("PxcKeywordCFPricesValueCurrent");
                                SetCell(
                                    sheet.Cell(row, 1),
                                    CellContentType.Info,
                                    model.Meta.GetLocalizedString("PxcKeywordCFPricesValueCurrent"),
                                    null
                                );
                                break;
                            case "F":
                                //sheet.Cell(row, 1).Value = model.Meta.GetLocalizedString("PxcKeywordCFPricesValueFixed");
                                SetCell(
                                    sheet.Cell(row, 1),
                                    CellContentType.Info,
                                    model.Meta.GetLocalizedString("PxcKeywordCFPricesValueFixed"),
                                    null
                                );
                                break;
                        }
                    }
                }
            }

            //DAYADJ
            row++;
            if (contvar)
            {
                if (dayadj.Count > 0)
                {
                    //writer.WriteEmptyRow()
                    foreach (KeyValuePair<string, string> kvp in dayadj)
                    {
                        //sheet.Cell(row, 1).Value = kvp.Key + ":";
                        SetCell(
                            sheet.Cell(row, 1),
                            CellContentType.Info,
                            kvp.Key + ":",
                            null
                        );
                        //sheet.Cell(row++, 2).Value = model.Meta.GetLocalizedString("PxcKeywordDayAdj");
                        row++;
                        SetCell(
                            sheet.Cell(row++, 1),
                            CellContentType.Info,
                            model.Meta.GetLocalizedString("PxcKeywordDayAdj"),
                            null
                        );
                    }
                }
            }
            else
            {
                if (model.Meta.ContentInfo != null)
                {
                    if (!String.IsNullOrEmpty(model.Meta.ContentInfo.DayAdj))
                    {
                        if (model.Meta.ContentInfo.DayAdj.ToUpper().Equals("YES"))
                        {
                            //sheet.Cell(row++, 1).Value = model.Meta.GetLocalizedString("PxcKeywordDayAdj");
                            SetCell(
                                sheet.Cell(row++, 1),
                                CellContentType.Info,
                                model.Meta.GetLocalizedString("PxcKeywordDayAdj"),
                                null
                            );
                        }
                    }
                }
            }

            //SEASADJ
            row++;
            if (contvar)
            {
                if (seasadj.Count > 0)
                {
                    //writer.WriteEmptyRow()
                    foreach (KeyValuePair<string, string> kvp in seasadj)
                    {
                        //sheet.Cell(row, 1).Value = kvp.Key + ":";
                        SetCell(
                            sheet.Cell(row, 1),
                            CellContentType.Info,
                            kvp.Key + ":",
                            null
                        );
                        //sheet.Cell(row++, 2).Value = model.Meta.GetLocalizedString("PxcKeywordSeasAdj");
                        row++;
                        SetCell(
                            sheet.Cell(row++, 1),
                            CellContentType.Info,
                            model.Meta.GetLocalizedString("PxcKeywordSeasAdj"),
                            null
                        );
                    }
                }
            }
            else
            {
                if (model.Meta.ContentInfo != null)
                {
                    if (!String.IsNullOrEmpty(model.Meta.ContentInfo.SeasAdj))
                    {
                        if (model.Meta.ContentInfo.SeasAdj.ToUpper().Equals("YES"))
                        {
                            //sheet.Cell(row++, 1).Value = model.Meta.GetLocalizedString("PxcKeywordSeasAdj");
                            SetCell(
                                sheet.Cell(row++, 1),
                                CellContentType.Info,
                                model.Meta.GetLocalizedString("PxcKeywordSeasAdj"),
                                null
                            );
                        }
                    }
                }
            }

            //OFFICIAL STATISTICS
            //If the statistics are official, insert information about that in the file 
            //Reqtest error report #406
            row++;
            if (model.Meta.OfficialStatistics)
            {

                SetCell(
                    sheet.Cell(row++, 1),
                    CellContentType.Info,
                    model.Meta.GetLocalizedString("PxcKeywordOfficialStatistics"),
                    null
                );
            }
            //DATABASE
            row++;
            if (!String.IsNullOrEmpty(model.Meta.Database))
            {
                //sheet.Cell(row, 1).Value = model.Meta.GetLocalizedString("PxcKeywordDatabase") + ":";
                SetCell(
                    sheet.Cell(row, 1),
                    CellContentType.Info,
                    model.Meta.GetLocalizedString("PxcKeywordDatabase") + ":",
                    null
                );
                //sheet.Cell(row++, 2).Value = model.Meta.Database;
                row++;
                SetCell(
                    sheet.Cell(row++, 1),
                    CellContentType.Info,
                    model.Meta.Database,
                    null
                );
            }

            //MATRIX
            row++;
            if (!String.IsNullOrEmpty(model.Meta.Matrix))
            {
                //sheet.Cell(row, 1).Value = model.Meta.GetLocalizedString("PxcKeywordMatrix") + ":";
                SetCell(
                    sheet.Cell(row, 1),
                    CellContentType.Info,
                    model.Meta.GetLocalizedString("PxcKeywordMatrix") + ":",
                    null
                );
                //sheet.Cell(row++, 2).Value = model.Meta.Matrix;
                row++;
                SetCell(
                    sheet.Cell(row++, 1),
                    CellContentType.Info,
                    model.Meta.Matrix,
                    null
                );
            }
            return row;
        }

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

        private int CalcHeadingRepeatInterval(int headingLevel, PXModel model)
        {
            int interval = 1;

            for (int i = headingLevel + 1; i < model.Meta.Heading.Count; i++)
            {
                interval *= model.Meta.Heading[i].Values.Count;
            }

            return interval;
        }

        private int CalcHeadingRepeats(int headingLevel, PXModel model)
        {
            int repeats = 1;
            for (int i = 0; i < headingLevel; i++)
            {
                repeats *= model.Meta.Heading[i].Values.Count;
            }

            return repeats;
        }

        private int CalculateLeftIndentation(PXModel model)
        {
            int lIndent = 0;
            for (int k = 0; k < model.Meta.Stub.Count; k++)
            {
                if (IsDoubleColumn(model.Meta.Stub[k]))
                {
                    lIndent++;
                }
                lIndent++;
            }
            return lIndent;
        }

        private void GetStubCell(PXModel model, IXLWorksheet sheet, int stubNr, int rowNr, bool code)
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
                for (int x = stubNr - 1; x > -1; x--)
                {
                    if (IsDoubleColumn(model.Meta.Stub[x])) offset++;
                }
                val = model.Meta.Stub[stubNr].Values[(rowNr / Interval) % count];
                row = rowNr + 3 + model.Meta.Heading.Count;
                column = stubNr + 1 + offset;
                if (code)
                {
                    //Writes the Code
                    //sheet.Cell(row, column).Value = val.Code;
                    SetCell(
                        sheet.Cell(row, column),
                        CellContentType.Code,
                        val.Code,
                        c => { c.DataType = XLDataType.Text; c.Style.Font.Bold = true; }
                    );

                    SetCell(
                        sheet.Cell(row, column + 1),
                        CellContentType.Stub,
                        val.Value,
                        c => c.Style.Font.Bold = true
                    );

                    if (val.HasNotes())
                    {
                        SetCell(
                            sheet.Cell(row, column + 1),
                            CellContentType.Comment,
                            val.Notes.GetAllNotes(),
                            null
                        );
                    }
                }
                else
                {


                    SetCell(
                        sheet.Cell(row, column),
                        CellContentType.Stub,
                        val.Text,
                        c => c.Style.Font.Bold = true
                    );
                    if (val.HasNotes())
                    {
                        //sheet.Cell(row, column).Comment.AddText(val.Notes.GetAllNotes());
                        SetCell(
                            sheet.Cell(row, column),
                            CellContentType.Comment,
                            val.Notes.GetAllNotes(),
                            null
                        );
                    }
                }
            }
        }

        private int CalcStubInterval(int stubChildNr, PXModel model)
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

        private bool IsDoubleColumn(Variable variable)
        {
            return false;
        }
        private int GetDecimalPrecision(string value, string separtor)
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
    }
}
