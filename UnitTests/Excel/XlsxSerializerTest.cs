using System;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PCAxis.Paxiom;

namespace PCAxis.Excel.UnitTest
{
	[TestClass]
	public class XlsxSerializerTest
	{
		[TestMethod]
		public void ShouldReturnTrueIsNumeric()
		{
			//Arrange,
			var numericVariable = "8";

			// Act
			var numericTrue = StringTests.IsNumeric (numericVariable);


			//Assert
			Assert.IsTrue(numericTrue);


		}

        [TestMethod]
        [DeploymentItem("TestFiles\\BE0101A1_20200914-143936.px")]
		public void ShouldSerializeCommaSeparated()
        {
            var model = GetSelectAllModel("TestFiles\\BE0101A1_20200914-143936.px");

            var ser = new XlsxSerializer();

            var stream = new MemoryStream();

            try
            {
                ser.Serialize(model, stream);

                MemoryStream destination = new MemoryStream();
                stream.CopyTo(destination);
                string actual = Encoding.UTF8.GetString(stream.ToArray());
                Assert.IsTrue(actual.Length >= 1);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }
    

        [TestMethod]
        [DeploymentItem("TestFiles\\BE0101A1.px")]
        public void ShouldSerialize()
        {
            var model = GetSelectAllModel("TestFiles\\BE0101A1.px");

            var ser = new XlsxSerializer();

            var stream = new MemoryStream();

            try
            {
                ser.Serialize(model, stream);
                
                MemoryStream destination = new MemoryStream();

                stream.CopyTo(destination);

                string actual = Encoding.UTF8.GetString(stream.ToArray());

                Assert.IsTrue(actual.Length >= 1 );
            }
            catch (Exception e)
            {
                Assert.Fail();
            }
        }


        private PXModel GetSelectAllModel(string file)
        {
            PCAxis.Paxiom.IPXModelBuilder builder = new PXFileBuilder();

            builder.SetPath(file);
            builder.BuildForSelection();

            PXMeta meta = builder.Model.Meta;
            builder.BuildForPresentation(Selection.SelectAll(meta));

            return builder.Model;
        }

    }

}



