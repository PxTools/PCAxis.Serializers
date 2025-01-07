using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using PCAxis.Paxiom;

namespace UnitTests.JsonStat
{
    [TestClass]
    [DeploymentItem("TestFiles", "TestFiles")]
    public class TestAllFiles
    {
        private readonly JsonStatHelper helper = new JsonStatHelper();
        private const string InputDirectoryPath = @"TestFiles";
        private const string OutputDirectoryPath = @"OutputJsonFiles";

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
        public void SerializeAllTestFilesAndParse(string pxFile)
        {
            CultureInfo ci = new CultureInfo("sv-SE");
            System.Threading.Thread.CurrentThread.CurrentCulture = ci;
            System.Threading.Thread.CurrentThread.CurrentUICulture = ci;

            PXModel myModel = helper.GetSelectAllModel(pxFile);

            var jsonstat2asString = helper.GetActual(myModel);

            //jsonstat2asString is valid json
            var jsonstat2Object = JObject.Parse(jsonstat2asString);

            // write to file
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(pxFile);
            string outputFile = Path.Combine(OutputDirectoryPath, $"{fileNameWithoutExtension}-jsonstat.json");
            using (StreamWriter file = File.CreateText(outputFile))
            {
                using (JsonTextWriter writer = new JsonTextWriter(file))
                {
                    Console.WriteLine("Writing to: " + outputFile);
                    jsonstat2Object.WriteTo(writer);
                }
            }
        }


        public static IEnumerable<object[]> GetPxFilePaths()
        {
            foreach (var pxFile in Directory.GetFiles(InputDirectoryPath, "*.px"))
            {
                yield return new object[] { pxFile };
            }
        }
    }

}
