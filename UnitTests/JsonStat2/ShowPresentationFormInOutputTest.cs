using System.Globalization;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PCAxis.Paxiom;
using Newtonsoft.Json.Linq;


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
                
            PXModel myModel = helper.GetSelectAllModel("TestFiles/BE0101A1_show_codes_or.px");
            
            var actual = helper.GetActual(myModel);
         
            var jsonstat2Object = JObject.Parse(actual);
   
            /* From px-file_format_specification_2013.pdf
             The values text should always be defined by the VALUES keyword and the codes should always be defined by the CODES keyword.
               0 - Only the value code should be displayed.
               1 - Only the value text should be displayed.
               2 - Both code and value should be displayed and the order should be the code and then the value text.
               3 - Both code and value should be displayed and the order should be the value text then the value code
             */

            //PX:No mention, should defalt to 1 
            var showValueSex = jsonstat2Object["dimension"]["sex"]["extension"]["show"].ToString();
            Assert.AreEqual("text",showValueSex,"For var=sex");

            //PX: =2
            var showValueMaritalStatus = jsonstat2Object["dimension"]["marital status"]["extension"]["show"].ToString();
            Assert.AreEqual("code_value", showValueMaritalStatus, "For var=marital status");

            //PX: =3
            var showValueRegion = jsonstat2Object["dimension"]["region"]["extension"]["show"].ToString();
            Assert.AreEqual("value_code", showValueRegion, "For var=region");

        }

    }
}
