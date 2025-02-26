using System;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using PCAxis.Paxiom;

namespace UnitTests.JsonStat2
{
    [TestClass]
    [DeploymentItem("TestFiles/BE0101A1_misc_metadata.px")]
    [DeploymentItem("TestFiles/MultipleContentSmall.px")]
    public class MiscMetadataTest
    {
        private JObject jsonstat2AsJObjectBE0101A1;
        private JObject jsonstat2AsJObjectMultiContent;
        private readonly JsonStat2Helper _helper = new();

        [TestInitialize]
        public void TestSetup()
        {
            CultureInfo ci = new CultureInfo("sv-SE");
            System.Threading.Thread.CurrentThread.CurrentCulture = ci;
            System.Threading.Thread.CurrentThread.CurrentUICulture = ci;

            PXModel myModel = _helper.GetSelectAllModel("BE0101A1_misc_metadata.px");
            var actual = _helper.GetActual(myModel);

            jsonstat2AsJObjectBE0101A1 = JObject.Parse(actual);

            myModel = _helper.GetSelectAllModel("MultipleContentSmall.px");
            actual = _helper.GetActual(myModel);

            jsonstat2AsJObjectMultiContent = JObject.Parse(actual);
        }

        [TestMethod]
        [Description("Testing output for updateFrequency (pxfile: BE0101A1_misc_metadata.px)")]
        public void TestUpdateFrequency()
        {
            var expectedUpdateFrequency = "Every two months";
            var actualUpdateFrequency = jsonstat2AsJObjectBE0101A1["extension"]["px"]["updateFrequency"];

            Assert.AreEqual(expectedUpdateFrequency, actualUpdateFrequency);
        }

        [TestMethod]
        [Description("Testing output for priceType (pxfiles: BE0101A1_misc_metadata.px and MultipleContentSmall.px)")]
        public void TestPriceType()
        {
            var expectedPriceType = "Fixed";
            var actualPriceType = jsonstat2AsJObjectBE0101A1["dimension"]["ContentsCode"]["extension"]["priceType"]["EliminatedValue"];

            Assert.AreEqual(expectedPriceType, actualPriceType);

            expectedPriceType = "Current";
            actualPriceType = jsonstat2AsJObjectMultiContent["dimension"]["ContentsCode"]["extension"]["priceType"]["KOSmenn0000"];

            Assert.AreEqual(expectedPriceType, actualPriceType);

            expectedPriceType = "NotApplicable";
            actualPriceType = jsonstat2AsJObjectMultiContent["dimension"]["ContentsCode"]["extension"]["priceType"]["KOSfolkemengde0000"];

            Assert.AreEqual(expectedPriceType, actualPriceType);
        }

        [TestMethod]
        [Description("Testing output for adjustment (pxfile: BE0101A1_misc_metadata.px and MultipleContentSmall.px)")]
        public void TestAdjustment()
        {
            var expectedAdjustement = "SesOnly";
            var actualAdjustment = jsonstat2AsJObjectBE0101A1["dimension"]["ContentsCode"]["extension"]["adjustment"]["EliminatedValue"];

            Assert.AreEqual(expectedAdjustement, actualAdjustment);

            expectedAdjustement = "None";
            actualAdjustment = jsonstat2AsJObjectMultiContent["dimension"]["ContentsCode"]["extension"]["adjustment"]["KOSfolkemengde0000"];

            Assert.AreEqual(expectedAdjustement, actualAdjustment);

            expectedAdjustement = "WorkAndSes";
            actualAdjustment = jsonstat2AsJObjectMultiContent["dimension"]["ContentsCode"]["extension"]["adjustment"]["KOSkvinne0000"];

            Assert.AreEqual(expectedAdjustement, actualAdjustment);

            expectedAdjustement = "WorkOnly";
            actualAdjustment = jsonstat2AsJObjectMultiContent["dimension"]["ContentsCode"]["extension"]["adjustment"]["KOSmenn0000"];

            Assert.AreEqual(expectedAdjustement, actualAdjustment);
        }

        [TestMethod]
        [Description("Testing output for measuringType (pxfile: BE0101A1_misc_metadata.px and MultipleContentSmall.px)")]
        public void TestMeasuringType()
        {
            var expectedType = "Stock";
            var actualType = jsonstat2AsJObjectBE0101A1["dimension"]["ContentsCode"]["extension"]["measuringType"]["EliminatedValue"];

            Assert.AreEqual(expectedType, actualType);

            expectedType = "Flow";
            actualType = jsonstat2AsJObjectMultiContent["dimension"]["ContentsCode"]["extension"]["measuringType"]["KOSmenn0000"];

            Assert.AreEqual(expectedType, actualType);

            expectedType = "Average";
            actualType = jsonstat2AsJObjectMultiContent["dimension"]["ContentsCode"]["extension"]["measuringType"]["KOSkvinne0000"];

            Assert.AreEqual(expectedType, actualType);

            expectedType = "Other";
            actualType = jsonstat2AsJObjectMultiContent["dimension"]["ContentsCode"]["extension"]["measuringType"]["KOSfolkemengde0000"];

            Assert.AreEqual(expectedType, actualType);
        }

        [TestMethod]
        [Description("Testing output for basePeriod (pxfile: BE0101A1_misc_metadata.px and MultipleContentSmall.px)")]
        public void TestBasePeriod()
        {
            var expectedBaseperiod = "This is a BASEPERIOD";
            var actualBasePeriod = jsonstat2AsJObjectBE0101A1["dimension"]["ContentsCode"]["extension"]["basePeriod"]["EliminatedValue"];

            Assert.AreEqual(expectedBaseperiod, actualBasePeriod);

            expectedBaseperiod = "Dummy baseperiod";
            actualBasePeriod = jsonstat2AsJObjectMultiContent["dimension"]["ContentsCode"]["extension"]["basePeriod"]["KOSmenn0000"];

            Assert.AreEqual(expectedBaseperiod, actualBasePeriod);
        }

        [TestMethod]
        [Description("Testing output for nextUpdate (pxfile: BE0101A1_misc_metadata.px)")]
        public void NextUpdate()
        {
            var expectedNextUpdate = _helper.ConvertToLocalUtcString("20250707 08:00");
            var dtNextUpate = jsonstat2AsJObjectBE0101A1["extension"]["px"]["nextUpdate"].ToObject<DateTime>();
            var actualNextUpdate = dtNextUpate.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);

            Assert.AreEqual(expectedNextUpdate, actualNextUpdate);
        }

        [TestMethod]
        [Description("Testing output for link (pxfile: BE0101A1_misc_metadata.px)")]
        public void TestLink()
        {
            var expectedLink = "this is a link on extension PX";
            var actualLink = jsonstat2AsJObjectBE0101A1["extension"]["px"]["link"].ToString();

            Assert.AreEqual(expectedLink, actualLink);
        }

        [TestMethod]
        [Description("Testing output for survey (pxfile: BE0101A1_misc_metadata.px)")]
        public void TestSurvey()
        {
            var expectedSurvey = "I am a survey";
            var actualSurvey = jsonstat2AsJObjectBE0101A1["extension"]["px"]["survey"].ToString();

            Assert.AreEqual(expectedSurvey, actualSurvey);
        }
    }
}
