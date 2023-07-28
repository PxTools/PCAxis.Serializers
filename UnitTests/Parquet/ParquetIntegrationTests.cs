using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PCAxis.Paxiom;
using PCAxis.Serializers;
using System.Collections.Generic;
using Parquet.Rows;
using System.Diagnostics;
using System.Threading.Tasks;

namespace UnitTests.Parquet
{
    [TestClass]
    public class ParquetIntegrationTests
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

        [TestMethod]
        [DynamicData(nameof(GetPxFilePaths), DynamicDataSourceType.Method)]
        public void ParquetSerialization(string pxFile)
        {
            var model = GetPxModelFromFile(pxFile);

            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(pxFile);
            string outputFile = Path.Combine(OutputDirectoryPath, $"{fileNameWithoutExtension}.parquet");

            SerializePxModelToParquet(model, outputFile);

            // Sync wrapper around async call
            Table table = ReadBackParquetFileSync(outputFile);

            // Assertion: Ensure that the model's matrix size is equal to the table's count.
            Assert.AreEqual(model.Data.MatrixSize, table.Count, $"Mismatch in matrix size for file {pxFile}");
        }

        private async Task<Table> ReadBackParquetFileAsync(string parquetFile)
        {
            return await Table.ReadAsync(parquetFile);
        }

        // Synchronous wrapper around the asynchronous method
        private Table ReadBackParquetFileSync(string parquetFile)
        {
            return Task.Run(() => ReadBackParquetFileAsync(parquetFile)).Result;
        }

        private PXModel GetPxModelFromFile(string pxFile)
        {
            var builder = new PXFileBuilder();
            builder.SetPath(new Uri(pxFile, UriKind.Relative).ToString());
            builder.BuildForSelection();
            var selection = Selection.SelectAll(builder.Model.Meta);
            builder.BuildForPresentation(selection);
            return builder.Model;
        }

        private void SerializePxModelToParquet(PXModel model, string outputPath)
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
            // Directory.Delete(OutputDirectoryPath, true);
        }
    }
}