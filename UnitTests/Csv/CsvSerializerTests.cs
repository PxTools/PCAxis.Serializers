using System;
using System.IO;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using PCAxis.Paxiom;

namespace PCAxis.Serializers.Tests.Csv
{
    [TestClass]
    [DeploymentItem("TestFiles/PR0101B3.px")]
    public class CsvSerializerTests
    {
        [TestMethod]
        public void Serialize_NullModel_ThrowsArgumentNullException()
        {
            var serializer = new CsvSerializer();
            Assert.ThrowsException<ArgumentNullException>(() => serializer.Serialize(null, "path"));
        }

        [TestMethod]
        public void Serialize_NullStream_ThrowsArgumentNullException()
        {
            var serializer = new CsvSerializer();
            var model = new PXModel();
            Assert.ThrowsException<ArgumentNullException>(() => serializer.Serialize(model, (Stream)null));
        }

        [TestMethod]
        public void Serialize_UnwritableStream_ThrowsArgumentException()
        {
            var serializer = new CsvSerializer();
            var model = new PXModel();
            var stream = new MemoryStream(new byte[0], false);
            Assert.ThrowsException<ArgumentException>(() => serializer.Serialize(model, stream));
        }

        [TestMethod]
        public void Serialize_ValidModel_WritesToFile()
        {
            var serializer = new CsvSerializer();
            var helper = new UnitTests.Helper();
            var model = helper.GetSelectAllModel("PR0101B3.px");
            var path = "test.csv";

            serializer.Serialize(model, path);

            Assert.IsTrue(File.Exists(path));
            File.Delete(path);
        }

        [TestMethod]
        public void Serialize_ValidModel_WritesToStream()
        {
            var serializer = new CsvSerializer();
            var helper = new UnitTests.Helper();
            var model = helper.GetSelectAllModel("TestFiles/PR0101B3.px");
            var stream = new MemoryStream();

            serializer.Serialize(model, stream);

            Assert.IsTrue(stream.Length > 0);
        }

        [TestMethod]
        public void ValueDelimiter_SetToSemicolon_UsesSemicolon()
        {
            var serializer = new CsvSerializer();
            serializer.ValueDelimiter = CsvSerializer.Delimiters.Semicolon;

            Assert.AreEqual(CsvSerializer.Delimiters.Semicolon, serializer.ValueDelimiter);
        }

        [TestMethod]
        public void ValueDelimiter_CheckDefault_IsComma()
        {
            var serializer = new CsvSerializer();

            Assert.AreEqual(CsvSerializer.Delimiters.Comma, serializer.ValueDelimiter);
        }


        [TestMethod]
        public void IncludeTitle_SetToTrue_WritesTitle()
        {
            var serializer = new CsvSerializer();
            serializer.IncludeTitle = true;
            var helper = new UnitTests.Helper();
            var model = helper.GetSelectAllModel("PR0101B3.px");
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream, Encoding.UTF8);

            serializer.Serialize(model, stream);

            stream.Position = 0;
            var reader = new StreamReader(stream);
            var content = reader.ReadToEnd();

            Assert.IsTrue(content.Contains("Consumer Price Index"));
        }
    }
}
