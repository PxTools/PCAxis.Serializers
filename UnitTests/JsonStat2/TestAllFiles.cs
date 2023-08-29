using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using PCAxis.Paxiom;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace UnitTests.JsonStat2
{
    [TestClass]
    [DeploymentItem("TestFiles", "TestFiles")]
    public class TestAllFiles
    {
        private JsonStat2Helper helper = new JsonStat2Helper();
        private const string InputDirectoryPath = @"TestFiles";

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
