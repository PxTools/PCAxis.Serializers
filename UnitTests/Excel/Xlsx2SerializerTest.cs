using System;
using System.Globalization;
using System.IO;
using System.Reflection;

using ClosedXML.Excel;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using PCAxis.Paxiom;
using PCAxis.Serializers;
using PCAxis.Serializers.Excel;

namespace UnitTests.Excel
{
    [TestClass]
    public class Xlsx2SerializerTest
    {
        private readonly ExcelHelper helper = new ExcelHelper();

        [TestMethod]
        [DeploymentItem("TestFiles/BE0101A1.px")]
        public void ShouldWriteNumericDataCellsAsExcelNumbers()
        {
            // Arrange
            var model = helper.GetSelectAllModel("BE0101A1.px");
            var serializer = new Xlsx2Serializer
            {
                IncludeTitle = false,
                ExcludeZerosAndMissingValues = false
            };

            using var stream = new MemoryStream();

            // Act
            serializer.Serialize(model, stream);
            stream.Position = 0;

            using var workbook = new XLWorkbook(stream);
            var sheet = workbook.Worksheet(model.Meta.Matrix);

            // Same indentation logic as serializer (private static method via reflection)
            var calcIndentMethod = typeof(Xlsx2Serializer).GetMethod(
                "CalculateLeftIndentation",
                BindingFlags.NonPublic | BindingFlags.Static);

            Assert.IsNotNull(calcIndentMethod, "Could not access CalculateLeftIndentation.");

            var leftIndent = (int)calcIndentMethod.Invoke(null, new object[] { model });

            // Same formatter defaults as CreateDataFormater(...)
            var fmt = new DataFormatter(model)
            {
                ThousandSeparator = "",
                DecimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator,
                InformationLevel = InformationLevelType.AllInformation
            };

            // In Xlsx2Serializer:
            // row starts at 1 (when IncludeTitle=false) + number of heading rows
            // column = leftIndent + j
            var dataStartRow = 1 + model.Meta.Heading.Count;
            var dataStartColumn = leftIndent;

            var validatedNumericCells = 0;
            const int maxNumericCellsToCheck = 10;

            for (int i = 0; i < model.Data.MatrixRowCount; i++)
            {
                for (int j = 0; j < model.Data.MatrixColumnCount; j++)
                {
                    var note = string.Empty;
                    var dataNote = string.Empty;
                    var value = fmt.ReadElement(i, j, ref note, ref dataNote);

                    if (!StringTests.IsNumeric(value))
                    {
                        continue;
                    }

                    var row = dataStartRow + i;
                    var column = dataStartColumn + j;
                    var cell = sheet.Cell(row, column);

                    Assert.AreEqual(
                        XLDataType.Number,
                        cell.DataType,
                        $"Expected numeric Excel cell at R{row}C{column} for value '{value}', but got '{cell.DataType}'.");

                    validatedNumericCells++;
                    if (validatedNumericCells >= maxNumericCellsToCheck)
                        break;
                }
                if (validatedNumericCells >= maxNumericCellsToCheck)
                    break;
            }

            Assert.IsGreaterThan(0, validatedNumericCells, "No numeric data cells were found to validate.");
        }
    }
}
