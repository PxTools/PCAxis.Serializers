using System.Globalization;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json.Linq;

using PCAxis.Paxiom;


namespace UnitTests.JsonStat2
{
    [TestClass]
    public class ShowPresentationFormInJason2StatOutputTest
    {
        private readonly JsonStat2Helper helper = new JsonStat2Helper();

        [TestMethod]
        [DeploymentItem("TestFiles/BE0101A1_show_codes_or.px")]
        public void ShouldShowCorrectPresentationForm()
        {
            CultureInfo ci = new CultureInfo("sv-SE");
            System.Threading.Thread.CurrentThread.CurrentCulture = ci;
            System.Threading.Thread.CurrentThread.CurrentUICulture = ci;

            PXModel myModel = helper.GetSelectAllModel("BE0101A1_show_codes_or.px");

            var actual = helper.GetActual(myModel);

            var jsonstat2Object = JObject.Parse(actual);

            //PX:No mention, should defalt to 1 
            var showValueSex = jsonstat2Object["dimension"]["sex"]["extension"]["show"].ToString();
            Assert.AreEqual("value", showValueSex, "For var=sex");

            //PX: =2
            var showValueMaritalStatus = jsonstat2Object["dimension"]["marital status"]["extension"]["show"].ToString();
            Assert.AreEqual("code_value", showValueMaritalStatus, "For var=marital status");

            //PX: =3
            var showValueRegion = jsonstat2Object["dimension"]["region"]["extension"]["show"].ToString();
            Assert.AreEqual("value_code", showValueRegion, "For var=region");

        }

    }
}
