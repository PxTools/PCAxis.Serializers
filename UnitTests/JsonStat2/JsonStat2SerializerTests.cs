using System;
using System.Globalization;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json.Linq;

using PCAxis.Paxiom;


namespace UnitTests.JsonStat2
{
    [TestClass]
    [DeploymentItem("TestFiles/HalfYearStatistics.px")]
    public class JsonStat2SerializerTests
    {
        private JObject _jsonstat;

        [TestInitialize]
        public void TestSetup()
        {
            var helper = new JsonStat2Helper();

            PXModel myModel = helper.GetSelectAllModel("HalfYearStatistics.px");

            var actual = helper.GetActual(myModel);

            _jsonstat = JObject.Parse(actual);
        }

        [TestMethod]
        public void Check_Root_Label_Element()
        {
            // TODO: the PxcMeta fileds should be resolved with languages files in these tests
            var actualLabel = _jsonstat["label"].ToString();
            actualLabel = actualLabel.Replace("PxcMetaTitleBy", "by");
            actualLabel = actualLabel.Replace("PxcMetaTitleAnd", "and");

            Assert.AreEqual("Wage and salary indices by industry 2015=100 by Half-year, Industry and Information", actualLabel);

        }
        [TestMethod]
        public void Check_Root_Updated_Element()
        {
            var localTimeZone = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneInfo.Local.Id);
            var helsinkiTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Helsinki");

            var updated = _jsonstat["updated"].ToObject<DateTime>();
            updated = TimeZoneInfo.ConvertTimeFromUtc(updated, localTimeZone);
            updated = TimeZoneInfo.ConvertTimeToUtc(updated, helsinkiTimeZone);

            var actual = updated.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);

            Assert.AreEqual("2023-07-13T05:00:00Z", actual);
        }

        [TestMethod]
        public void Check_More_Root_Elements()
        {
            Assert.AreEqual("2.0", _jsonstat["version"]);
            Assert.AreEqual("dataset", _jsonstat["class"]);
            Assert.AreEqual("Statistics Finland, wage and salary indices", _jsonstat["source"]);
        }

        [TestMethod]
        public void Check_Root_Extension_Px_Elements()
        {
            Assert.AreEqual("statfin_ktps_pxt_111p", _jsonstat["extension"]["px"]["tableid"]);
            Assert.AreEqual(1, _jsonstat["extension"]["px"]["decimals"]);
            Assert.AreEqual(false, _jsonstat["extension"]["px"]["official-statistics"]);
            Assert.AreEqual(true, _jsonstat["extension"]["px"]["aggregallowed"]);
            Assert.AreEqual("en", _jsonstat["extension"]["px"]["language"]);
            Assert.AreEqual("Wage and salary indices by industry 2015=100", _jsonstat["extension"]["px"]["contents"]);
            Assert.AreEqual("111p -- Wage and salary indices by industry semiannually (2015=100), 1995H1-2022H2", _jsonstat["extension"]["px"]["description"]);
            Assert.AreEqual(false, _jsonstat["extension"]["px"]["descriptiondefault"]);
            Assert.AreEqual("Industry", _jsonstat["extension"]["px"]["heading"][0]);
            Assert.AreEqual("Information", _jsonstat["extension"]["px"]["heading"][1]);
            Assert.AreEqual("Half-year", _jsonstat["extension"]["px"]["stub"][0]);
            Assert.AreEqual("001_111p_2023m05", _jsonstat["extension"]["px"]["matrix"]);
            Assert.AreEqual("KTPS", _jsonstat["extension"]["px"]["subject-code"]);
            Assert.AreEqual("ktps", _jsonstat["extension"]["px"]["subject-area"]);
        }
    }
}
