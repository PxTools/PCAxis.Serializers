using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using PCAxis.Serializers.Util.MetaId;

namespace PCAxis.MetaId.UnitTest
{
    [TestClass]
    public class UnitTest1
    {


        public UnitTest1()
        {

        }

        [TestMethod]
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
        public void TestMissing()
        {
            string metaid_raw = "missing:123";
            var links = MetaIdResolverStatic.GetTableLinks(metaid_raw, "no");
            Assert.AreEqual(0, links.Count);
        }

        [TestMethod]
        public void TestMulti()
        {
            // both comma and space as separators
            string metaid_raw = "urn:ssb:classification:klass:1 , urn:ssb:classification:klass:2 urn:ssb:classification:klass:3 ";
            string expectedUrl_0 = "https://www.ssb.no/en/klass/klassifikasjoner/1";
            string expectedUrl_1 = "https://www.ssb.no/en/klass/klassifikasjoner/2";
            string expectedUrl_2 = "https://www.ssb.no/en/klass/klassifikasjoner/3";

            List<Link> links = MetaIdResolverStatic.GetVariableLinks(metaid_raw, "en", "region");
            Assert.AreEqual(3, links.Count);
            Assert.AreEqual(expectedUrl_0, links[0].Url);
            Assert.AreEqual(expectedUrl_1, links[1].Url);
            Assert.AreEqual(expectedUrl_2, links[2].Url);

        }
    }
}
