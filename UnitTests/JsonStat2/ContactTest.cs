using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using PCAxis.Paxiom;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.JsonStat2
{
    [TestClass]
    [DeploymentItem("TestFiles/BE0101N1.px")]
    public class ContactTest
    {
        private JObject jsonstat2AsJObject;

        [TestInitialize]
        public void TestSetup()
        {
            var helper = new JsonStat2Helper();
            CultureInfo ci = new CultureInfo("sv-SE");
            System.Threading.Thread.CurrentThread.CurrentCulture = ci;
            System.Threading.Thread.CurrentThread.CurrentUICulture = ci;

            PXModel myModel = helper.GetSelectAllModel("BE0101N1.px");

            var actual = helper.GetActual(myModel);

            jsonstat2AsJObject = JObject.Parse(actual);
        }

        [TestMethod]
        [Description("Testing multiple contacts on contentvariable (pxfile: BE0101N1.px)")]
        public void TestContactSplitIntoThreeParts()
        {
            var expectedContactCount = 3;
            var actualContactCount = jsonstat2AsJObject["extension"]["contact"].Count();
            Assert.AreEqual(expectedContactCount, actualContactCount);
        }

        [TestMethod]
        [Description("Testing multiple contacts on contentvariable (pxfile: BE0101N1.px)")]
        public void TestContactInformation()
        {
            var expectedFirstContact = "Ann-Marie Persson, SCB# +46 010-479 63 38#ann-marie.persson@scb.se";
            var expectedSecondContact = " Statistikservice, SCB# +46 010-479 50 00#information@scb.se";
            var expectedThirdContact = "Rasmus Andersson, SCB# +46 010-479 66 55#rasmus.andersson@scb.se";
            
            var actualFirstContact = jsonstat2AsJObject["extension"]["contact"][0]["raw"].ToString();
            var actualSecondContact = jsonstat2AsJObject["extension"]["contact"][1]["raw"].ToString();
            var actualThirdContact = jsonstat2AsJObject["extension"]["contact"][2]["raw"].ToString();

            Assert.AreEqual(expectedFirstContact, actualFirstContact);
            Assert.AreEqual(expectedSecondContact, actualSecondContact);
            Assert.AreEqual(expectedThirdContact, actualThirdContact);
        }
    }

    [TestClass]
    [DeploymentItem("TestFiles/BE0101A1_show_codes_or.px")]
    public class ContactWithoutContentvariableTest
    {
        private JObject jsonstat2AsJObject;

        [TestInitialize]
        public void TestSetup()
        {
            var helper = new JsonStat2Helper();
            CultureInfo ci = new CultureInfo("sv-SE");
            System.Threading.Thread.CurrentThread.CurrentCulture = ci;
            System.Threading.Thread.CurrentThread.CurrentUICulture = ci;

            PXModel myModel = helper.GetSelectAllModel("BE0101A1_show_codes_or.px");

            var actual = helper.GetActual(myModel);

            jsonstat2AsJObject = JObject.Parse(actual);
        }

        [TestMethod]
        [Description("Testing contact count for pxfile without contentvaribale defined (pxfile: BE0101A1_show_codes_or.px)")]
        public void TestContactCount()
        {
            var expectedContactCount = 1;
            var actualContactCount = jsonstat2AsJObject["extension"]["contact"].Count();
            Assert.AreEqual(expectedContactCount, actualContactCount);
        }

        [TestMethod]
        [Description("Testing contact information for pxfile without contentvaribale defined (pxfile: BE0101A1_show_codes_or.px)")]
        public void TestContactInformation()
        {
            var expectedContact = "   Befolkningsstatistik, SCB#Tel: 019-17 60 10#E-mail: befolkning@scb.se";
            
            var actualContact = jsonstat2AsJObject["extension"]["contact"][0]["raw"].ToString();

            Assert.AreEqual(expectedContact, actualContact);
        }
    }

    [TestClass]
    [DeploymentItem("TestFiles/BE0101A1_contact.px")]
    public class TestContactFromContactInfo
    {
        private JObject jsonstat2AsJObject;

        [TestInitialize]
        public void TestSetup()
        {
            var helper = new JsonStat2Helper();
            CultureInfo ci = new CultureInfo("sv-SE");
            System.Threading.Thread.CurrentThread.CurrentCulture = ci;
            System.Threading.Thread.CurrentThread.CurrentUICulture = ci;

            PXModel myModel = helper.GetSelectAllModel("BE0101A1_contact.px");

            var actual = helper.GetActual(myModel);

            jsonstat2AsJObject = JObject.Parse(actual);
        }

        [TestMethod]
        [Description("Testing contact count for pxfile without contentvaribale defined (pxfile: BE0101A1_contact.px)")]
        public void TestContactWithPipesCount()
        {
            var expectedContactCount = 2;
            var actualContactCount = jsonstat2AsJObject["extension"]["contact"].Count();
            Assert.AreEqual(expectedContactCount, actualContactCount);
        }

        [TestMethod]
        [Description("Testing contact information for pxfile without contentvaribale defined (pxfile: BE0101A1_contact.px)")]
        public void TestFirstContactWithPipes()
        {
            var expectedName = "Firstname Secondname";
            var expectedMail = "email@email";
            var expextedPhone = "12345678";
            var expectedRaw = "Firstname Secondname, organzation#+46 12345678#email@email";

            var actualName = jsonstat2AsJObject["extension"]["contact"][0]["name"].ToString();
            var actualMail = jsonstat2AsJObject["extension"]["contact"][0]["mail"].ToString();
            var actualPhone = jsonstat2AsJObject["extension"]["contact"][0]["phone"].ToString();
            var actualRaw = jsonstat2AsJObject["extension"]["contact"][0]["raw"].ToString();

            Assert.AreEqual(expectedName, actualName);
            Assert.AreEqual(expectedMail, actualMail);
            Assert.AreEqual(expextedPhone, actualPhone);
            Assert.AreEqual(expectedRaw, actualRaw);
        }
    }
}
