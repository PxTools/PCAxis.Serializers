using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Parquet.Rows;

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
        public void ShouldSerializePxModel(string pxFile)
        {
            var model = GetPxModelFromFile(pxFile);

            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(pxFile);
            string outputFile = Path.Combine(OutputDirectoryPath, $"{fileNameWithoutExtension}.parquet");

            SerializePxModelToParquet(model, outputFile);

            // Sync wrapper around async call
            Table table = ReadBackParquetFileSync(outputFile);

            // Assertion: Ensure that the model's matrix size is grather than or equal to the table's rowcount.
            Assert.IsGreaterThanOrEqualTo(model.Data.MatrixSize, table.Count, $"Mismatch in matrix size for file {fileNameWithoutExtension}.parquet.");

            // Assertion: Calculate the amount of columns we should have, based on the metadata
            // Number of columns in meta, number of columns in table.

            var numberOfColsInPx = CalculateNumberOfColumnsFromPxFile(model);
            var numberOfColsInParq = table.Schema.DataFields.Length;

            // Assertion: Calculate the amount of rows we should have, based on the metadata
            // Number of rows in meta, number of rows in table.

            Assert.AreEqual(numberOfColsInPx, numberOfColsInParq, $"Mismatch in column number for {fileNameWithoutExtension}.parquet.");

            var numberOfRowsInPx = CalculateNumberOfRowsFromPxFile(model);
            var numberOfRowsInParq = table.Count;

            Assert.AreEqual(numberOfRowsInPx, numberOfRowsInParq, $"Mismatch in row number for {fileNameWithoutExtension}.parquet.");
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

        private static int CalculateNumberOfRowsFromPxFile(PXModel model)
        {
            int numberOfRows = 1;

            foreach (var variable in model.Meta.Variables)
            {
                if (!variable.IsContentVariable)
                {
                    numberOfRows *= variable.Values.Count;
                }
            }

            return numberOfRows;
        }

        private static Task<Table> ReadBackParquetFileAsync(string parquetFile)
        {
            return Table.ReadAsync(parquetFile);
        }

        // Synchronous wrapper around the asynchronous method
        private static Table ReadBackParquetFileSync(string parquetFile)
        {
            return Task.Run(() => ReadBackParquetFileAsync(parquetFile)).Result;
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
            using (FileStream stream = new FileStream(outputPath, FileMode.Create))
            {
                var parquetSer = new ParquetSerializer();
                parquetSer.Serialize(model, stream);
            }
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
            //Directory.Delete(OutputDirectoryPath, true);
        }
    }
}
