using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PCAxis.Paxiom;
using Newtonsoft.Json.Linq;



namespace UnitTests.JsonStat
{
    [TestClass]
    public class ShowPresentationFormInJasonStatOutputTest
    {
        private JsonStatHelper helper = new JsonStatHelper();
        
		[TestMethod]
        [DeploymentItem("TestFiles/BE0101A1_show_codes_or.px")]
        public void ShouldShowCorrectPresentationFormInJasoStat()
        {
            CultureInfo ci = new CultureInfo("sv-SE");
            System.Threading.Thread.CurrentThread.CurrentCulture = ci;
            System.Threading.Thread.CurrentThread.CurrentUICulture = ci;
                
            PXModel myModel = helper.GetSelectAllModel("BE0101A1_show_codes_or.px");
            
            var actual = helper.GetActual(myModel);
         
            var jsonstatObject = JObject.Parse(actual);
   
            //PX:No mention, should defalt to 1 
            var showValueSex = jsonstatObject["dataset"]["dimension"]["sex"]["extension"]["show"].ToString();
            Assert.AreEqual("value",showValueSex,"For var=sex");

            //PX: =2
            var showValueMaritalStatus = jsonstatObject["dataset"]["dimension"]["marital status"]["extension"]["show"].ToString();
            Assert.AreEqual("code_value", showValueMaritalStatus, "For var=marital status");

            //PX: =3
            var showValueRegion = jsonstatObject["dataset"]["dimension"]["region"]["extension"]["show"].ToString();
            Assert.AreEqual("value_code", showValueRegion, "For var=region");
        }
    }
}
