using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using PCAxis.Paxiom;
using System.Globalization;
using System.Linq;

namespace UnitTests.JsonStat2
{
    [TestClass]
    [DeploymentItem("TestFiles/BE0101A1_with_notes.px")]
    public class NoteTest
    {
        private JObject jsonstat2AsJObject;

        [TestInitialize]
        public void TestSetup()
        {
            var helper = new JsonStat2Helper();
            CultureInfo ci = new CultureInfo("sv-SE");
            System.Threading.Thread.CurrentThread.CurrentCulture = ci;
            System.Threading.Thread.CurrentThread.CurrentUICulture = ci;

            PXModel myModel = helper.GetSelectAllModel("BE0101A1_with_notes.px");

            var actual = helper.GetActual(myModel);

            jsonstat2AsJObject = JObject.Parse(actual);
        }

        [TestMethod]
        [Description("Testing output for noteMandatory (pxfile: BE0101A1_with_notes.px)")]
        public void TestTableNotes()
        {
            var expectedMandatory = "True";
            var tableNoteMandatory = jsonstat2AsJObject["extension"]["noteMandatory"]["0"].ToString();
            Assert.AreEqual(expectedMandatory, tableNoteMandatory);
        }

        [TestMethod]
        [Description("Testing output for note and noteMandatory (pxfile: BE0101A1_with_notes.px)")]
        public void TestDimensionNotesForPeriod()
        {
            //Variable period have two notes. One is mandatory
            var actualPeriodNote1 = jsonstat2AsJObject["dimension"]["period"]["note"][0].ToString();
            var actualPeriodNote2 = jsonstat2AsJObject["dimension"]["period"]["note"][1].ToString();
            Assert.AreEqual("This note is mandatory! Note for variable period", actualPeriodNote1);
            Assert.AreEqual("This note is NOT mandatory. Note for variable period", actualPeriodNote2);

            var expectedMandatory = "True";
            var dimensionNoteIsMandatory = jsonstat2AsJObject["dimension"]["period"]["extension"]["noteMandatory"]["0"].ToString();
            Assert.AreEqual(expectedMandatory, dimensionNoteIsMandatory);
        }

        [TestMethod]
        [Description("Testing number of category notes for dimension period (pxfile: BE0101A1_with_notes.px)")]
        public void TestCatergoryNotesForPeriod()
        {
            var expectedCountOfCategoryNotes = 2;
            var actualCount = jsonstat2AsJObject["dimension"]["period"]["category"]["note"].Count();
            Assert.AreEqual(expectedCountOfCategoryNotes, actualCount);

            var actualCategoryNoteMandatory1 = jsonstat2AsJObject["dimension"]["period"]["extension"]["categoryNoteMandatory"]["0"]["0"].ToString();
            var actualCategoryNoteMandatory2 = jsonstat2AsJObject["dimension"]["period"]["extension"]["categoryNoteMandatory"]["1"]["0"].ToString();

            Assert.AreEqual("True", actualCategoryNoteMandatory1);
            Assert.AreEqual("True", actualCategoryNoteMandatory2);

            Assert.IsNull(jsonstat2AsJObject["dimension"]["period"]["extension"]["categoryNoteMandatory"]["0"]["1"]);
        }
    }
}
