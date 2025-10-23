using System;
using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using PCAxis.Paxiom;

namespace PCAxis.Serializers.Tests
{
    [TestClass]
    public class HtmlSerializerTests
    {
        [TestMethod]
        public void Serialize_NullModel_ThrowsArgumentNullException()
        {
            var serializer = new HtmlSerializer();
            Assert.ThrowsExactly<ArgumentNullException>(() => serializer.Serialize(null, "path"));
        }

        [TestMethod]
        public void Serialize_NullStream_ThrowsArgumentNullException()
        {
            var serializer = new HtmlSerializer();
            var model = new PXModel();
            Assert.ThrowsExactly<ArgumentNullException>(() => serializer.Serialize(model, (Stream)null));
        }

        [TestMethod]
        public void Serialize_UnwritableStream_ThrowsArgumentException()
        {
            var serializer = new HtmlSerializer();
            var model = new PXModel();
            var stream = new MemoryStream(new byte[0], false);
            Assert.ThrowsExactly<ArgumentException>(() => serializer.Serialize(model, stream));
        }

        [TestMethod]
        public void Serialize_ValidModel_WritesToStream()
        {
            var serializer = new HtmlSerializer();
            var model = CreateTestModel();
            using (var stream = new MemoryStream())
            {
                serializer.Serialize(model, stream);
                stream.Position = 0;
                using (var reader = new StreamReader(stream))
                {
                    var result = reader.ReadToEnd();
                    Assert.Contains("<table", result);
                    Assert.Contains("</table>", result);
                }
            }
        }

        private PXModel CreateTestModel()
        {

            PXMeta meta = new PXMeta();

            // Create time variable
            var name = "PointOfTime";
            Variable variable = new Variable(name, PlacementType.Heading);

            for (int i = 1968; i < 2025; i++)
            {
                variable.Values.Add(CreateValue($"{i}"));
            }
            variable.TimeValue = $"TLIST(A, \"1968\"-\"2025\")";
            variable.IsTime = true;
            meta.AddVariable(variable);

            // Create content variable
            name = $"MEASURE";
            variable = new Variable(name, PlacementType.Heading);
            variable.Values.Add(CreateValue($"M1"));
            variable.Values.Add(CreateValue($"M2"));
            variable.Elimination = false;
            variable.IsContentVariable = true;
            meta.Variables.Add(variable);

            //Create classification variable gender

            name = "GENDER";

            variable = new Variable(name, PlacementType.Stub);
            variable.Values.Add(CreateValue($"M"));
            variable.Values.Add(CreateValue($"F"));
            variable.Elimination = true;
            meta.Variables.Add(variable);

            meta.Decimals = 0;


            var data = new PXData();

            var model = new PXModel(meta, data);
            model.Data.SetMatrixSize(2, 120);

            return model;
        }

        private static PCAxis.Paxiom.Value CreateValue(string code)
        {
            PCAxis.Paxiom.Value value = new PCAxis.Paxiom.Value(code);
            PaxiomUtil.SetCode(value, code);
            return value;
        }
    }
}
