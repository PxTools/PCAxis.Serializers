using Microsoft.VisualStudio.TestTools.UnitTesting;
using PCAxis.Paxiom;
using System.Globalization;

namespace UnitTests.JsonStat2
{
    [TestClass]
    public class TestWholeResponse
    {
        [TestMethod]
        [DeploymentItem("TestFiles/BE0101A1_with_notes.px")]
        public void TestJsonStat2ResponseString()
        {
            var helper = new JsonStat2Helper();
            CultureInfo ci = new CultureInfo("sv-SE");
            System.Threading.Thread.CurrentThread.CurrentCulture = ci;
            System.Threading.Thread.CurrentThread.CurrentUICulture = ci;

            PXModel myModel = helper.GetSelectAllModel("BE0101A1_with_notes.px");

            var jsonstat2AsString = helper.GetActual(myModel);

            var expectedResponseString = helper.GetExpectedBE0101A1WithLocalDate();

            Assert.AreEqual(expectedResponseString, jsonstat2AsString);
        }
    }
}
