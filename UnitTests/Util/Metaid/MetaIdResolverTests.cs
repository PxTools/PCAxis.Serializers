using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using PCAxis.Serializers.Util.MetaId;

namespace UnitTests.Util.Metaid
{
    [TestClass]
    public class MetaIdResolverTests
    {

        private Mock<IFileGetter> _readerMock;

        private readonly string _config_1 = "<?xml version=\"1.0\"?>\n<metaId labelFilesFolder=\"\">\n  <onTable>\n    <metaSystem id=\"STATISTICS\">\n      <relationalGroup relation=\"statistics-homepage\" type=\"text/html\">\n        <link pxLang=\"no\" labelStringFormat=\"Statistikkens hjemmeside\" urlStringFormat=\"https://www.ssb.no/{0}\" />\n        <link pxLang=\"en\" labelStringFormat=\"Statistics homepage\" urlStringFormat=\"https://www.ssb.no/en/{0}\" />\n      </relationalGroup>\n      <relationalGroup relation=\"about-statistics\" type=\"text/html\">\n         <link pxLang=\"no\" labelStringFormat=\"Om statistikken\" urlStringFormat=\"https://www.ssb.no/{0}#om-statistikken\" />\n         <link pxLang=\"en\" labelStringFormat=\"About the statistics\" urlStringFormat=\"https://www.ssb.no/en/{0}#om-statistikken\" />\n      </relationalGroup>\n    </metaSystem>\n    <metaSystem id=\"NO_LABEL\">\n      <relationalGroup relation=\"test-no-label\" type=\"text/html\">\n        <link pxLang=\"no\" labelStringFormat=\"\" urlStringFormat=\"https://www.ssb.no/nice-document{0}\" />\n        <link pxLang=\"en\" labelStringFormat=\"\" urlStringFormat=\"https://www.ssb.no/en/nice-document{0}\" />\n      </relationalGroup>\n    </metaSystem>\n    <metaSystem id=\"ANY_URL\">\n      <relationalGroup relation=\"any-url\" type=\"text/html\">\n        <link pxLang=\"no\" labelStringFormat=\"\" urlStringFormat=\"https:{0}\" />\n        <link pxLang=\"en\" labelStringFormat=\"\" urlStringFormat=\"https:{0}\" />\n      </relationalGroup>\n    </metaSystem>\n  </onTable>\n  <onVariable>\n    <metaSystem id=\"urn:ssb:classification:klass\">\n       <relationalGroup relation=\"definitions\" type=\"text/html\">   \n         <link pxLang=\"no\" labelStringFormat=\"Klassifikasjon for {0}.\" labelsFile=\"Testfiles/metaid_labelFilesFolder/class_no.txt\" urlStringFormat=\"https://www.ssb.no/klass/klassifikasjoner/{0}\" />\n         <link pxLang=\"en\" labelStringFormat=\"Classification for {0}.\" labelsFile=\"Testfiles/metaid_labelFilesFolder/class_en.txt\" urlStringFormat=\"https://www.ssb.no/en/klass/klassifikasjoner/{0}\" />\n       </relationalGroup>  \n    </metaSystem>\n    <metaSystem id=\"urn:ssb:conceptvariable:vardok\">\n      <relationalGroup relation=\"definitions\" type=\"text/html\">   \n         <link pxLang=\"no\" labelStringFormat=\"Definisjon av {0}.\" urlStringFormat=\"https://www.ssb.no/a/metadata/conceptvariable/vardok/{0}/nb\" />\n         <link pxLang=\"en\" labelStringFormat=\"Definition of {0}.\" urlStringFormat=\"https://www.ssb.no/a/metadata/conceptvariable/vardok/{0}/en\" />\n      </relationalGroup>\n    </metaSystem>\n  </onVariable>\n  <onValue>\n    <metaSystem id=\"urn:ssb:conceptvariable:vardok\">\n      <relationalGroup relation=\"definitions\" type=\"text/html\">\n        <link pxLang=\"no\" labelStringFormat=\"Definisjon av {1} for vaiabel {0}.\" urlStringFormat=\"https://www.ssb.no/a/metadata/conceptvariable/vardok/{0}/nb\" />\n        <link pxLang=\"en\" labelStringFormat=\"Definition of {1} for vaiable {0}.\" urlStringFormat=\"https://www.ssb.no/a/metadata/conceptvariable/vardok/{0}/en\" />\n      </relationalGroup>\n    </metaSystem>\n    <metaSystem id=\"urn:ssb:contextvariable:common\">\n      <relationalGroup relation=\"definitions\" type=\"text/html\">\n        <link pxLang=\"no\" labelStringFormat=\"Definisjon av {1} (Kostra).\" urlStringFormat=\"https://www.ssb.no/kompis/statbank/?id={0}&amp;ver={1}&amp;val={2}\" />\n        <link pxLang=\"en\" labelStringFormat=\"Definition of {1} (Kostra).\" urlStringFormat=\"https://www.ssb.no/kompis/statbank/?id={0}&amp;ver={1}&amp;val={2}\" />\n      </relationalGroup>  \n    </metaSystem>\n  </onValue>\n</metaId>\n";

        private readonly string _config_labelsFolder = "<?xml version=\"1.0\"?>\n<metaId labelFilesFolder=\"MylabelFilesFolder\">\n  <onVariable>\n    <metaSystem id=\"urn:ssb:classification:klass\">\n       <relationalGroup relation=\"definitions\" type=\"text/html\">   \n         <link pxLang=\"en\" labelStringFormat=\"Classification for {0}.\" labelsFile=\"labels_folder_file_en.txt\" urlStringFormat=\"https://www.ssb.no/en/klass/klassifikasjoner/{0}\" />\n       </relationalGroup>  \n    </metaSystem>\n  </onVariable>\n</metaId>";

        private readonly Dictionary<string, string> _labelsfolder_en = new Dictionary<string, string>
        {
            ["urn:ssb:classification:klass:2"] = "Using labelsFolder , Standard for gender",
            ["urn:ssb:classification:klass:5"] = "Using labelsFolder ,Standard for PRODCOM codes"
        };

        private readonly Dictionary<string, string> _class_en = new Dictionary<string, string>
        {
            ["urn:ssb:classification:klass:2"] = "From file, Standard for gender",
            ["urn:ssb:classification:klass:5"] = "Standard for PRODCOM codes"
        };

        private readonly Dictionary<string, string> _class_no = new Dictionary<string, string>
        {
            ["urn:ssb:classification:klass:2"] = "Standard for kjønn",
            ["urn:ssb:classification:klass:5"] = "Standard for PRODCOM koder"
        };


        [TestInitialize]
        public void TestInitialize()
        {
            _readerMock = new Mock<IFileGetter>(MockBehavior.Strict);
            _readerMock.Setup(x => x.ReadConfig(It.IsAny<string>()))
                .Returns((string fileName) =>
                {

                    if (fileName.EndsWith("metaid.config", StringComparison.OrdinalIgnoreCase))
                    {
                        return XDocument.Parse(_config_1);
                    }

                    if (fileName.EndsWith("metaid_labelsFolder.config", StringComparison.OrdinalIgnoreCase))
                    {
                        return XDocument.Parse(_config_labelsFolder);
                    }

                    throw new FileNotFoundException(fileName);

                });

            _readerMock.Setup(x => x.ReadLabelsfile(It.IsAny<string>()))
                .Returns((string fileName) =>
                {
                    if (String.IsNullOrEmpty(fileName))
                    {
                        return new Dictionary<string, string>();
                    }

                    if (fileName.EndsWith("class_en.txt", StringComparison.OrdinalIgnoreCase))
                    {
                        return _class_en;
                    }
                    if (fileName.EndsWith("class_no.txt", StringComparison.OrdinalIgnoreCase))
                    {
                        return _class_no;
                    }

                    if (fileName.Equals("MylabelFilesFolder" + Path.DirectorySeparatorChar + "labels_folder_file_en.txt") ||
                        fileName.Equals("MylabelFilesFolder" + Path.AltDirectorySeparatorChar + "labels_folder_file_en.txt")
                    )
                    {
                        return _labelsfolder_en;
                    }


                    throw new FileNotFoundException(fileName);

                });
        }






        [TestMethod]
        public void ReadsMetaIdConfig()
        {
            // Act
            var resolver = new MetaIdResolver("metaid.config", _readerMock.Object);

            // Assert
            _readerMock.Verify(x => x.ReadConfig("metaid.config"), Times.Once);
        }


        [TestMethod]
        public void TestLabelFromFile()
        {
            var resolver = new MetaIdResolver("metaid.config", _readerMock.Object);

            string metaid_raw = "urn:ssb:classification:klass:2";
            string expectedLinkText = "From file, Standard for gender";
            List<Link> links = resolver.GetVariableLinks(metaid_raw, "en", "Override me");
            Assert.AreEqual(expectedLinkText, links[0].Label);
        }

        [TestMethod]
        public void TestLabelNotFromFile()
        {
            var resolver = new MetaIdResolver("metaid.config", _readerMock.Object);

            string metaid_raw = "urn:ssb:classification:klass:1";
            string expectedLinkText = "Classification for variablelabel.";
            List<Link> links = resolver.GetVariableLinks(metaid_raw, "en", "variablelabel");
            Assert.AreEqual(expectedLinkText, links[0].Label);

        }


        [TestMethod]
        public void TestLabelFolder()
        {
            var resolver = new MetaIdResolver("metaid_labelsFolder.config", _readerMock.Object);
            string metaid_raw = "urn:ssb:classification:klass:2";
            string expectedLinkText = _labelsfolder_en[metaid_raw];
            List<Link> links = resolver.GetVariableLinks(metaid_raw, "en", "Override me");
            Assert.AreEqual(expectedLinkText, links[0].Label);
        }


        [TestMethod]
        public void Ctor_WhenConfigCannotBeRead_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<FileNotFoundException>(
                () => new MetaIdResolver("no_such_config.xml", _readerMock.Object)
            );
        }

    }





}
