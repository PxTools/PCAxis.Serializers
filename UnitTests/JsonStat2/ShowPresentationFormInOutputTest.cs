using System.Globalization;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PCAxis.Paxiom;
using Newtonsoft.Json.Linq;
using DocumentFormat.OpenXml.Drawing.Charts;


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
        public void ShouldShowCorrectPresentationForm()
        {
            CultureInfo ci = new CultureInfo("sv-SE");
            System.Threading.Thread.CurrentThread.CurrentCulture = ci;
            System.Threading.Thread.CurrentThread.CurrentUICulture = ci;
                
            PXModel myModel = helper.GetSelectAllModel("TestFiles/BE0101A1_20200914-143936.px");
            
            myModel.Meta.Variables.Where(x => x.PresentationText == 1 && x.Code == "region")
                .ToList().ForEach(y => y.PresentationText = 3);
            
            var actual = helper.GetActual(myModel);
         
            var jsonstat2Object = JObject.Parse(actual);
         
            var showValueSex = jsonstat2Object["dimension"]["sex"]["extension"]["show"].ToString();
            Assert.AreEqual("code",showValueSex);

            var showValueRegion = jsonstat2Object["dimension"]["region"]["extension"]["show"].ToString();
            Assert.AreEqual("code_value", showValueRegion);
        }

    }
}
