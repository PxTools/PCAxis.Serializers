using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Parquet;
using Parquet.Data;
using Parquet.Schema;

using PCAxis.Paxiom;
using PCAxis.Serializers;

namespace UnitTests.Parquet
{
    [TestClass]
    [DeploymentItem("TestFiles", "TestFiles")]
    public class ParquetSerializationIntegrationTests
    {
        private const string InputDirectoryPath = @"TestFiles";
        private const string OutputDirectoryPath = @"OutputParquetFiles";

        [TestInitialize]
        public void TestInitialize()
        {
            // Clean up the output directory before each test.
            if (Directory.Exists(OutputDirectoryPath))
            {
                Directory.Delete(OutputDirectoryPath, true);
            }
            Directory.CreateDirectory(OutputDirectoryPath);
        }

        [TestMethod, Description("Tests the serialization of PXModel to Parquet format and its correctness.")]
        [DynamicData(nameof(GetPxFilePaths))]
        public async Task ShouldSerializePxModel(string pxFile)
        {
            var model = GetPxModelFromFile(pxFile);

            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(pxFile);
            string outputFile = Path.Combine(OutputDirectoryPath, $"{fileNameWithoutExtension}.parquet");

            SerializePxModelToParquet(model, outputFile);

            var columns = await ReadBackParquetFile(outputFile);

            // Assertion: Ensure that the table's row count equals the number of observations
            // for a single ContentsCode. If the model has multiple contents, the serializer
            // emits additional content columns rather than duplicating rows.
            int contentCount = model.Meta.ContentVariable != null ? model.Meta.ContentVariable.Values.Count : 1;
            int expectedRows = model.Data.MatrixSize / contentCount;
            Assert.HasCount(expectedRows, columns[0], $"Mismatch in matrix size for file {fileNameWithoutExtension}.parquet.");

            // Assertion: Calculate the amount of columns we should have, based on the metadata
            // Number of columns in meta, number of columns in table.

            var numberOfColsInPx = CalculateNumberOfColumnsFromPxFile(model);
            int numberOfColsInParq = columns.Count;
            Assert.AreEqual(numberOfColsInPx, numberOfColsInParq, $"Mismatch in column number for {fileNameWithoutExtension}.parquet.");
        }

        [TestMethod, Description("Tests correct ordering of time variable (pxfile: 16216.px)")]
        [DeploymentItem("TestFiles/14216.px")]
        public async Task TestTimeVariableOrdering()
        {
            var pxFile = "14216.px";
            var model = GetPxModelFromFile(pxFile);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(pxFile);
            string outputFile = Path.Combine(OutputDirectoryPath, $"{fileNameWithoutExtension}.parquet");
            SerializePxModelToParquet(model, outputFile);
            var columns = await ReadBackParquetFile(outputFile);

            int rowCount = columns.Count == 0 ? 0 : columns[0].Length;
            Assert.AreEqual(2, rowCount, "Test number of rows");

            Assert.AreEqual("0801", columns[0][0], "Test tettsted");
            Assert.AreEqual("2025", columns[1][0], "Test year");
            Assert.AreEqual(275.87, columns[3][0], "Test ContentsCode_Areal");
            Assert.AreEqual(double.Parse("1110887"), columns[5][0], "Test ContentsCode_Bosatte");

            Assert.AreEqual("0801", columns[0][1], "Test tettsted");
            Assert.AreEqual("2024", columns[1][1], "Test year");
            Assert.AreEqual(276.30, columns[3][1], "Test ContentsCode_Areal");
            Assert.AreEqual(double.Parse("1098061"), columns[5][1], "Test ContentsCode_Bosatte");
        }

        private static int CalculateNumberOfColumnsFromPxFile(PXModel model)
        {

            int numberOfCols = 0;

            foreach (var variable in model.Meta.Variables)
            {
                if (variable.IsContentVariable)
                {
                    // Each content-variable value is a column, plus an additional symbol column
                    numberOfCols += variable.Values.Count * 2;
                }
                else if (variable.IsTime)
                {
                    // Each time-variable have a Parsed-column in DateTime.
                    numberOfCols += 2;
                }
                else
                {
                    numberOfCols++;
                }
            }

            return numberOfCols;
        }

        private static async Task<List<object[]>> ReadBackParquetFile(string parquetFile)
        {
            using Stream fs = File.OpenRead(parquetFile);
            using ParquetReader reader = await ParquetReader.CreateAsync(fs);
            ParquetSchema schema = reader.Schema;
            DataField[] dataFields = schema.GetDataFields();
            int totalRowCount = 0;

            // First pass: count rows across all row groups so each column array can be preallocated.
            for (int i = 0; i < reader.RowGroupCount; i++)
            {
                using ParquetRowGroupReader rowGroupReader = reader.OpenRowGroupReader(i);
                totalRowCount += (int)rowGroupReader.RowCount;
            }

            var columns = new List<object[]>(dataFields.Length);
            // Allocate one dense array per column to avoid per-value list growth/conversion.
            for (int fieldIndex = 0; fieldIndex < dataFields.Length; fieldIndex++)
            {
                columns.Add(new object[totalRowCount]);
            }

            int rowOffset = 0;

            // Second pass: read each row group and copy values into final column arrays.
            for (int i = 0; i < reader.RowGroupCount; i++)
            {
                using ParquetRowGroupReader rowGroupReader = reader.OpenRowGroupReader(i);
                int rowCount = (int)rowGroupReader.RowCount;

                for (int columnIndex = 0; columnIndex < dataFields.Length; columnIndex++)
                {
                    DataColumn columnData = await rowGroupReader.ReadColumnAsync(dataFields[columnIndex]);
                    for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
                    {
                        columns[columnIndex][rowOffset + rowIndex] = columnData.Data.GetValue(rowIndex);
                    }
                }

                // Move the write window to the next row-group segment.
                rowOffset += rowCount;
            }

            return columns;
        }

        private static PXModel GetPxModelFromFile(string pxFile)
        {
            var builder = new PXFileBuilder();
            builder.SetPath(new Uri(pxFile, UriKind.Relative).ToString());
            builder.BuildForSelection();
            var selection = Selection.SelectAll(builder.Model.Meta);
            builder.BuildForPresentation(selection);
            return builder.Model;
        }

        private static void SerializePxModelToParquet(PXModel model, string outputPath)
        {
            using FileStream stream = new(outputPath, FileMode.Create);
            var parquetSer = new ParquetSerializer();
            parquetSer.Serialize(model, stream);
        }

        public static IEnumerable<object[]> GetPxFilePaths()
        {
            foreach (var pxFile in Directory.GetFiles(InputDirectoryPath, "*.px"))
            {
                Console.WriteLine(pxFile);
                yield return new object[] { pxFile };
            }
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Directory.Delete(OutputDirectoryPath, true);
        }
    }
}
