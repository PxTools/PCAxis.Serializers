using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PCAxis.Paxiom;
using PCAxis.Serializers;
using UnitTests.Sdmx;

namespace UnitTests.JsonStat2
{
    [TestClass]
    public class UnitTest1
    {
        private JsonStat2Helper helper = new JsonStat2Helper();

        [TestMethod]
		public void TestMethod1()
		{
			//Arrange
			var a = 0;
			var b = 0;

			//Act

			//Assert
			//Check their equality.
			Assert.AreEqual(a, b);

		}


		[TestMethod]

        public void ShouldReturnSomething()
        {

            CultureInfo ci = new CultureInfo("sv-SE");
            System.Threading.Thread.CurrentThread.CurrentCulture = ci;
            System.Threading.Thread.CurrentThread.CurrentUICulture = ci;
                
            PXModel myModel = helper.GetSelectAllModel("TestFiles/BE0101A1_20200914-143936.px");

            string actual = helper.GetActual(myModel);
            
            Assert.IsNotNull(actual);

        }

    }
}
