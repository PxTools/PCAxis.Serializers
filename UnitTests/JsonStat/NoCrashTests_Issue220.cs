using System;
using System.Globalization;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using PCAxis.Paxiom;

namespace UnitTests.JsonStat
{
    [TestClass]
    [DeploymentItem("TestFiles/PR0101B3.px")]
    [DeploymentItem("TestFiles/Issue220Finland.px")]
    public class NoCrashTests_Issue220
    {
        private readonly JsonStatHelper helper = new JsonStatHelper();


        [TestMethod]
        public void PR0101B3_CultureInfoFinnish()
        {
            CultureInfo ci = new CultureInfo("fi-FI");
            System.Threading.Thread.CurrentThread.CurrentCulture = ci;
            System.Threading.Thread.CurrentThread.CurrentUICulture = ci;


            PXModel myModel = helper.GetSelectAllModel("PR0101B3.px");


            try
            {
                string actual = helper.GetActual(myModel);

                Assert.IsTrue(actual.Length >= 1, "Made it!");
            }
            catch (Exception)
            {
                Assert.Fail();
            }
        }


        [TestMethod]
        public void NoCrash_CultureInfoFinnish()
        {
            CultureInfo ci = new CultureInfo("fi-FI");
            System.Threading.Thread.CurrentThread.CurrentCulture = ci;
            System.Threading.Thread.CurrentThread.CurrentUICulture = ci;


            PXModel myModel = helper.GetSelectAllModel("Issue220Finland.px");


            try
            {
                string actual = helper.GetActual(myModel);

                Assert.IsTrue(actual.Length >= 1, "Made it!");
            }
            catch (Exception)
            {
                Assert.Fail();
            }
        }



        [TestMethod]
        public void NoCrash_CultureInfoNorway()
        {

            CultureInfo ci = new CultureInfo("nb-NO");
            System.Threading.Thread.CurrentThread.CurrentCulture = ci;
            System.Threading.Thread.CurrentThread.CurrentUICulture = ci;


            PXModel myModel = helper.GetSelectAllModel("Issue220Finland.px");



            try
            {
                string actual = helper.GetActual(myModel);

                Assert.IsTrue(actual.Length >= 1, "Made it!");
            }
            catch (Exception)
            {
                Assert.Fail();
            }
        }
    }
}
