using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using PCAxis.Paxiom;
using System;
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
            var expectedMandatory = "{\r\n  \"0\": true\r\n}".ReplaceLineEndings();
            var tableNoteMandatory = jsonstat2AsJObject["extension"]["noteMandatory"].ToString();
            Assert.AreEqual(expectedMandatory, tableNoteMandatory);
        }

        [TestMethod]
        [Description("Testing output for note and noteMandatory (pxfile: BE0101A1_with_notes.px)")]
        public void TestDimensionNotesForPeriod()
        {
            //Variable period have two notes. One is mandatory
            var expectedPeriodNote = $"[{Environment.NewLine}  \"This note is mandatory! Note for variable period\",{Environment.NewLine}  \"This note is NOT mandatory. Note for variable period\"{Environment.NewLine}]";
            var actualPeriodNote = jsonstat2AsJObject["dimension"]["period"]["note"].ToString();
            Assert.AreEqual(expectedPeriodNote, actualPeriodNote);

            var expectedMandatory = "{\r\n  \"0\": true\r\n}";
            var dimensionNoteIsMandatory = jsonstat2AsJObject["dimension"]["period"]["extension"]["noteMandatory"].ToString();
            Assert.AreEqual(expectedMandatory, dimensionNoteIsMandatory);
        }

        [TestMethod]
        [Description("Testing number of category notes for dimension period (pxfile: BE0101A1_with_notes.px)")]
        public void TestCatergoryNotesForPeriod()
        {
            var expectedCountOfCategoryNotes = 2;
            var actualCount = jsonstat2AsJObject["dimension"]["period"]["category"]["note"].Count();
            Assert.AreEqual(expectedCountOfCategoryNotes, actualCount);

            var expectedMandatoryOutput = "{\r\n  \"0\": {\r\n    \"0\": true\r\n  },\r\n  \"1\": {\r\n    \"0\": true\r\n  }\r\n}".ReplaceLineEndings();
            var actualOutput = jsonstat2AsJObject["dimension"]["period"]["extension"]["categoryNoteMandatory"].ToString();
            Assert.AreEqual(expectedMandatoryOutput, actualOutput);
        }
    }
}
