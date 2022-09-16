using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using PCAxis.Paxiom;
using UnitTests.JsonStat2;

namespace UnitTests.JsonStat
{
    [TestClass]
    public class ShowPresentationFormInJosonstat2OutputTest
    {

        private JsonStatHelper helper = new JsonStatHelper();

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

            var showValueSex = jsonstat2Object["dataset"]["dimension"]["sex"]["extension"]["show"].ToString();
            Assert.AreEqual("code", showValueSex);

            var showValueRegion = jsonstat2Object["dataset"]["dimension"]["region"]["extension"]["show"].ToString();
            Assert.AreEqual("code_value", showValueRegion);
        }


    }
}
