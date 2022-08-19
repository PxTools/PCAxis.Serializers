using Microsoft.VisualStudio.TestTools.UnitTesting;
using PCAxis.Serializers.Excel;
using System;

namespace UnitTests.Excel
{
    [TestClass]
	public class XlsxSerializerTest
	{
        ExcelHelper helper = new ExcelHelper();


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
            var model = helper.GetSelectAllModel("TestFiles\\BE0101A1_20200914-143936.px");

            try
            {
                
                string actual = helper.GetActual(model);
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
            var model = helper.GetSelectAllModel("TestFiles\\BE0101A1.px");

            try
            {
                string actual = helper.GetActual(model);

                Assert.IsTrue(actual.Length >= 1 );
            }
            catch (Exception)
            {
                Assert.Fail();
            }
        }

    }

}



