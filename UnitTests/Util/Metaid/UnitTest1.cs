using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using PCAxis.Serializers.Util.MetaId;

namespace UnitTests.Util.Metaid
{
    [TestClass]
    public class UnitTest1
    {
        public UnitTest1()
        {
        }

        [TestMethod]
        [DeploymentItem("Util/Metaid/metaid.config")]
        public void TestOnTable()
        {
            string metaid_raw = "STATISTICS:aku";
            string expectedUrl = "https://www.ssb.no/aku#om-statistikken";
            string expectedLinkText = "Om statistikken";
            string expectedType = "text/html";
            string expectedRelation = "about-statistics";


            var links = MetaIdResolverStatic.GetTableLinks(metaid_raw, "no");
            Assert.AreEqual(2, links.Count);
            Assert.AreEqual(expectedUrl, links[1].Url);
            Assert.AreEqual(expectedLinkText, links[1].Label);
            Assert.AreEqual(expectedRelation, links[1].Relation);
            Assert.AreEqual(expectedType, links[1].Type);

        }

        [TestMethod]
        [DeploymentItem("Util/Metaid/metaid.config")]
        public void TestOnVaiable()
        {

            string metaid_raw = "urn:ssb:classification:klass:123";
            string expectedUrl = "https://www.ssb.no/en/klass/klassifikasjoner/123";
            string expectedLinkText = "Classification for region.";
            string expectedType = "text/html";
            string expectedRelation = "definition-classification";

            List<Link> links = MetaIdResolverStatic.GetVariableLinks(metaid_raw, "en", "region");

            Assert.AreEqual(expectedUrl, links[0].Url);
            Assert.AreEqual(expectedLinkText, links[0].Label);
            Assert.AreEqual(expectedRelation, links[0].Relation);
            Assert.AreEqual(expectedType, links[0].Type);

            //norwegian
            expectedUrl = "https://www.ssb.no/klass/klassifikasjoner/123";
            expectedLinkText = "Klassifikasjon for region.";
            links = MetaIdResolverStatic.GetVariableLinks(metaid_raw, "no", "region");

            Assert.AreEqual(expectedUrl, links[0].Url);
            Assert.AreEqual(expectedLinkText, links[0].Label);
            Assert.AreEqual(expectedRelation, links[0].Relation);
            Assert.AreEqual(expectedType, links[0].Type);

        }

        [TestMethod]
        [DeploymentItem("Util/Metaid/metaid.config")]
        public void TestOnValue()
        {

            string metaid_raw = "urn:ssb:conceptvariable:vardok:123";
            string expectedUrl_en = "https://www.ssb.no/a/metadata/conceptvariable/vardok/123/en";
            string expectedUrl_no = "https://www.ssb.no/a/metadata/conceptvariable/vardok/123/nb";

            string expectedLinkText_en = "Definition of Oslo for vaiable region.";
            string expectedLinkText_no = "Definisjon av Oslo for vaiabel region.";

            string expectedType = "text/html";
            string expectedRelation = "definition-value";

            List<Link> links = MetaIdResolverStatic.GetValueLinks(metaid_raw, "en", "region", "Oslo");

            Assert.AreEqual(expectedUrl_en, links[0].Url);
            Assert.AreEqual(expectedLinkText_en, links[0].Label);
            Assert.AreEqual(expectedRelation, links[0].Relation);
            Assert.AreEqual(expectedType, links[0].Type);

            links = MetaIdResolverStatic.GetValueLinks(metaid_raw, "no", "region", "Oslo");

            Assert.AreEqual(expectedUrl_no, links[0].Url);
            Assert.AreEqual(expectedLinkText_no, links[0].Label);
            Assert.AreEqual(expectedRelation, links[0].Relation);
            Assert.AreEqual(expectedType, links[0].Type);
        }


        [TestMethod]
        [DeploymentItem("Util/Metaid/metaid.config")]
        public void TestMissing()
        {
            string metaid_raw = "missing:123";
            var links = MetaIdResolverStatic.GetTableLinks(metaid_raw, "no");
            Assert.AreEqual(0, links.Count);
        }

        [TestMethod]
        [DeploymentItem("Util/Metaid/metaid.config")]
        public void TestMulti()
        {
            // both comma and space as separators
            string metaid_raw = "urn:ssb:classification:klass:1 ,urn:ssb:conceptvariable:vardok:2 urn:ssb:classification:klass:3 ";
            string expectedUrl_0 = "https://www.ssb.no/en/klass/klassifikasjoner/1";
            string expectedUrl_1 = "https://www.ssb.no/a/metadata/conceptvariable/vardok/2/en";
            string expectedUrl_2 = "https://www.ssb.no/en/klass/klassifikasjoner/3";

            string expectedMataid_2 = "urn:ssb:classification:klass:3";

            List<Link> links = MetaIdResolverStatic.GetVariableLinks(metaid_raw, "en", "region");
            Assert.AreEqual(3, links.Count);
            Assert.AreEqual(expectedUrl_0, links[0].Url);
            Assert.AreEqual(expectedUrl_1, links[1].Url);
            Assert.AreEqual(expectedUrl_2, links[2].Url);
            Assert.AreEqual(expectedMataid_2, links[2].MetaId);
        }

        [TestMethod]
        [DeploymentItem("Util/Metaid/metaid.config")]
        public void TestNoLabel()
        {
            string metaid_raw = "NO_LABEL:1";
            string expectedUrl = "https://www.ssb.no/en/nice-document1";
            string expectedLabel = "";


            List<Link> links = MetaIdResolverStatic.GetTableLinks(metaid_raw, "en");
            Assert.AreEqual(1, links.Count);
            Assert.AreEqual(expectedUrl, links[0].Url);
            Assert.AreEqual(expectedLabel, links[0].Label);
        }

        [TestMethod]
        [DeploymentItem("Util/Metaid/metaid.config")]
        public void TestAnyUrl()
        {
            // possible but is it a good idea?
            string metaid_raw = "ANY_URL://www.ssb.no/en/nice-document2";
            string expectedUrl = "https://www.ssb.no/en/nice-document2";
            string expectedLabel = "";

            List<Link> links = MetaIdResolverStatic.GetTableLinks(metaid_raw, "en");
            Assert.AreEqual(1, links.Count);
            Assert.AreEqual(expectedUrl, links[0].Url);
            Assert.AreEqual(expectedLabel, links[0].Label);
        }


        [TestMethod]
        [DeploymentItem("Util/Metaid/metaid.config")]
        public void TestTooFewParams()
        {
            // exception text is passed when config or metaid dont fit.
            // throwing an exception seems to much 
            string metaid_raw = "urn:ssb:contextvariable:common:3";
            string expectedUrl = "Index (zero based) must be greater than or equal to zero and less than the size of the argument list.";

            List<Link> links = MetaIdResolverStatic.GetValueLinks(metaid_raw, "en", "region", "some value");
            Assert.AreEqual(1, links.Count);
            Assert.AreEqual(expectedUrl, links[0].Url);

        }
    }
}
