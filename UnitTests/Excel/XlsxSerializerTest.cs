using System;
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
    public class XlsxSerializerTest
    {
        readonly ExcelHelper helper = new ExcelHelper();


        [TestMethod]
        public void ShouldReturnTrueIsNumeric()
        {
            //Arrange,
            var numericVariable = "8";

            // Act
            var numericTrue = StringTests.IsNumeric(numericVariable);


            //Assert
            Assert.IsTrue(numericTrue);
        }

        [TestMethod]
        [DeploymentItem("TestFiles/BE0101A1.px")]
        public void ShouldWriteNumericDataCellsAsExcelNumbers()
        {
            // Arrange
            var model = helper.GetSelectAllModel("BE0101A1.px");
            var serializer = new XlsxSerializer();

            using var stream = new MemoryStream();

            // Act
            serializer.Serialize(model, stream);
            stream.Position = 0;

            using var workbook = new XLWorkbook(stream);
            var sheet = workbook.Worksheet(model.Meta.Matrix);

            // Same left indentation logic as serializer (private method via reflection)
            var calcIndentMethod = typeof(XlsxSerializer).GetMethod(
                "CalculateLeftIndentation",
                BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.IsNotNull(calcIndentMethod, "Could not access CalculateLeftIndentation.");

            var leftIndent = (int)calcIndentMethod.Invoke(serializer, new object[] { model });

            var fmt = new DataFormatter(model)
            {
                ThousandSeparator = "",
                DecimalSeparator = ".",
                InformationLevel = serializer.InformationLevel
            };

            // Centralize layout offsets to avoid magic numbers
            const int HeadingOffset = 3;
            const int DataColumnOffset = 1;
            var dataStartRow = HeadingOffset + model.Meta.Heading.Count;
            var dataStartColumn = leftIndent + DataColumnOffset;

            int validatedNumericCells = 0;
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

        [TestMethod]
        [DeploymentItem("TestFiles/BE0101A1_20200914-143936.px")]
        public void ShouldSerializeCommaSeparated()
        {
            var model = helper.GetSelectAllModel("BE0101A1_20200914-143936.px");

            try
            {
                string actual = helper.GetActual(model);
                Assert.IsGreaterThanOrEqualTo(1, actual.Length);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }


        [TestMethod]
        [DeploymentItem("TestFiles/BE0101A1.px")]
        public void ShouldSerialize()
        {
            var model = helper.GetSelectAllModel("BE0101A1.px");

            try
            {
                string actual = helper.GetActual(model);

                Assert.IsGreaterThanOrEqualTo(1, actual.Length);
            }
            catch (Exception)
            {
                Assert.Fail();
            }
        }
    }
}







