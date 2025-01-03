using System;
using System.Globalization;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json.Linq;

using PCAxis.Paxiom;


namespace UnitTests.JsonStat2
{
    [TestClass]
    [DeploymentItem("TestFiles/HalfYearStatistics.px")]
    [DeploymentItem("TestFiles/BE0101A1.px")]
    [DeploymentItem("TestFiles/MultipleContent.px")]
    public class JsonStat2SerializerTests
    {
        private JObject _jsonHalfYearStatistics;
        private JObject _jsonBE0101A1;
        private JObject _jsonMultipleContent;


        [TestInitialize]
        public void TestSetup()
        {
            var helper = new JsonStat2Helper();

            PXModel myModel = helper.GetSelectAllModel("HalfYearStatistics.px");
            var actual = helper.GetActual(myModel);
            _jsonHalfYearStatistics = JObject.Parse(actual);

            myModel = helper.GetSelectAllModel("BE0101A1.px");
            actual = helper.GetActual(myModel);
            _jsonBE0101A1 = JObject.Parse(actual);

            myModel = helper.GetSelectAllModel("MultipleContent.px");
            actual = helper.GetActual(myModel);
            _jsonMultipleContent = JObject.Parse(actual);

        }

        [TestMethod]
        public void Check_Root_Label_Element()
        {
            // TODO: the PxcMeta fileds should be resolved with languages files in these tests
            var actualLabel = _jsonHalfYearStatistics["label"].ToString();
            actualLabel = actualLabel.Replace("PxcMetaTitleBy", "by");
            actualLabel = actualLabel.Replace("PxcMetaTitleAnd", "and");

            Assert.AreEqual("Wage and salary indices by industry 2015=100 by Half-year, Industry and Information", actualLabel);

        }
        [TestMethod]
        public void Check_Root_Updated_Element()
        {
            var localTimeZone = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneInfo.Local.Id);
            var helsinkiTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Helsinki");

            var updated = _jsonHalfYearStatistics["updated"].ToObject<DateTime>();
            updated = TimeZoneInfo.ConvertTimeFromUtc(updated, localTimeZone);
            updated = DateTime.SpecifyKind(updated, DateTimeKind.Unspecified);
            updated = TimeZoneInfo.ConvertTimeToUtc(updated, helsinkiTimeZone);

            var actual = updated.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);

            Assert.AreEqual("2023-07-13T05:00:00Z", actual);
        }

        [TestMethod]
        public void Check_More_Root_Elements()
        {
            Assert.AreEqual("2.0", _jsonHalfYearStatistics["version"]);
            Assert.AreEqual("dataset", _jsonHalfYearStatistics["class"]);
            Assert.AreEqual("Statistics Finland, wage and salary indices", _jsonHalfYearStatistics["source"]);
        }

        [TestMethod]
        public void Check_Root_Extension_Px_Elements()
        {
            Assert.AreEqual("statfin_ktps_pxt_111p", _jsonHalfYearStatistics["extension"]["px"]["tableid"]);
            Assert.AreEqual(1, _jsonHalfYearStatistics["extension"]["px"]["decimals"]);
            Assert.AreEqual("False", _jsonHalfYearStatistics["extension"]["px"]["official-statistics"].ToString());
            Assert.AreEqual("True", _jsonHalfYearStatistics["extension"]["px"]["aggregallowed"].ToString());
            Assert.AreEqual("en", _jsonHalfYearStatistics["extension"]["px"]["language"]);
            Assert.AreEqual("Wage and salary indices by industry 2015=100", _jsonHalfYearStatistics["extension"]["px"]["contents"]);
            Assert.AreEqual("111p -- Wage and salary indices by industry semiannually (2015=100), 1995H1-2022H2", _jsonHalfYearStatistics["extension"]["px"]["description"]);
            Assert.AreEqual("False", _jsonHalfYearStatistics["extension"]["px"]["descriptiondefault"].ToString());
            Assert.AreEqual("Toimiala", _jsonHalfYearStatistics["extension"]["px"]["heading"][0]);
            Assert.AreEqual("Tiedot", _jsonHalfYearStatistics["extension"]["px"]["heading"][1]);
            Assert.AreEqual("Puolivuosi", _jsonHalfYearStatistics["extension"]["px"]["stub"][0]);
            Assert.AreEqual("001_111p_2023m05", _jsonHalfYearStatistics["extension"]["px"]["matrix"]);
            Assert.AreEqual("KTPS", _jsonHalfYearStatistics["extension"]["px"]["subject-code"]);
            Assert.AreEqual("ktps", _jsonHalfYearStatistics["extension"]["px"]["subject-area"]);
        }


        [TestMethod]
        public void CheckIdArray()
        {
            //Assert.AreEqual("ContentsCode", _jsonBE0101A1["id"][0]);
            //Assert.AreEqual("region", _jsonBE0101A1["id"][1]);
            //Assert.AreEqual("marital status", _jsonBE0101A1["id"][2]);
            //Assert.AreEqual("age", _jsonBE0101A1["id"][3]);
            //Assert.AreEqual("sex", _jsonBE0101A1["id"][4]);
            //Assert.AreEqual("period", _jsonBE0101A1["id"][5]);

            Assert.AreEqual("Statsbrgskap", _jsonMultipleContent["id"][0]);
            Assert.AreEqual("Tid", _jsonMultipleContent["id"][1]);
            Assert.AreEqual("ContentsCode", _jsonMultipleContent["id"][2]);
        }

        [TestMethod]
        public void CheckSizeArray()
        {
            //Assert.AreEqual(1, _jsonBE0101A1["size"][0]);
            //Assert.AreEqual(1, _jsonBE0101A1["size"][1]);
            //Assert.AreEqual(1, _jsonBE0101A1["size"][2]);
            //Assert.AreEqual(1, _jsonBE0101A1["size"][3]);
            //Assert.AreEqual(1, _jsonBE0101A1["size"][4]);
            //Assert.AreEqual(1, _jsonBE0101A1["size"][5]);

            Assert.AreEqual(276, _jsonMultipleContent["size"][0]);
            Assert.AreEqual(65, _jsonMultipleContent["size"][1]);
            Assert.AreEqual(3, _jsonMultipleContent["size"][2]);
        }

        [TestMethod]
        public void CheckRoleArray()
        {
            //Assert.AreEqual("period", _jsonBE0101A1["role"]["time"][0]);
            //Assert.AreEqual("ContentsCode", _jsonBE0101A1["role"]["metric"][0]);

            Assert.AreEqual("Tid", _jsonMultipleContent["role"]["time"][0]);
            Assert.AreEqual("ContentsCode", _jsonMultipleContent["role"]["metric"][0]);
        }
    }
}
