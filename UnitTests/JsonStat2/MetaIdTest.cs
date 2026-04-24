using System.Globalization;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json.Linq;

using PCAxis.Paxiom;

namespace UnitTests.JsonStat2
{
    [TestClass]
    [DeploymentItem("TestFiles/metaid.config")]
    [DeploymentItem("TestFiles/tab_12909_metaid.px")]

    public class MetaIdTest
    {
        private JObject jsonstat2AsJObject;

        [TestInitialize]
        public void TestSetup()
        {
            var helper = new JsonStat2Helper();
            CultureInfo ci = new CultureInfo("sv-SE");
            System.Threading.Thread.CurrentThread.CurrentCulture = ci;
            System.Threading.Thread.CurrentThread.CurrentUICulture = ci;

            PXModel myModel = helper.GetSelectAllModel("tab_12909_metaid.px");

            var actual = helper.GetActual(myModel);

            jsonstat2AsJObject = JObject.Parse(actual);
        }

        [TestMethod]
        [Description("Testing output links from metaid")]
        public void TestTableHomepageLink()
        {
            string expected_relation = "statistics-homepage";
            //string expected_catagory = null;
            string expected_metaid = "STATISTICS:sykefratot";
            string expected_href = "https://www.ssb.no/sykefratot";
            string expected_label = "Statistikkens hjemmeside";
            string expected_type = "text/html";

            var links = jsonstat2AsJObject["link"]["related"];
            Assert.IsGreaterThan(1, links.Count(), "link.related has count < 2.");

            var homepage = links.Where(i => (string)i["extension"]?["metaid"].ToString() == expected_metaid).FirstOrDefault();
            Assert.IsNotNull(homepage, "Can find metaid " + expected_metaid);


            Assert.AreEqual(expected_relation, homepage["extension"]?["relation"].ToString());

            Assert.AreEqual(expected_href, homepage["href"].ToString());
            Assert.AreEqual(expected_label, homepage["label"].ToString());
            Assert.AreEqual(expected_type, homepage["type"].ToString());
        }


        [TestMethod]
        [Description("Testing output links from metaid")]
        public void TestDimLink()
        {
            string expected_relation = "definitions";
            //string expected_catagory = null;
            string expected_metaid = "urn:ssb:classification:klass:7";
            string expected_href = "https://www.ssb.no/klass/klassifikasjoner/7";
            string expected_label = "Klassifikasjon for yrke.";
            string expected_type = "text/html";

            var links = jsonstat2AsJObject["dimension"]["Yrke"]["link"]["related"];
            Assert.IsGreaterThan(1, links.Count(), "link.related has count < 2.");

            var aLink = links.Where(i => (string)i["extension"]?["metaid"].ToString() == expected_metaid).FirstOrDefault();
            Assert.IsNotNull(aLink, "Can find metaid " + expected_metaid);

            //var lala = aLink.ToString();


            Assert.AreEqual(expected_relation, aLink["extension"]?["relation"].ToString());

            Assert.AreEqual(expected_href, aLink["href"].ToString());
            Assert.AreEqual(expected_label, aLink["label"].ToString());
            Assert.AreEqual(expected_type, aLink["type"].ToString());
        }



    }
}
