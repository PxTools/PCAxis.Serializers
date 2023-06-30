using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using PCAxis.Paxiom;
using System.Globalization;
using System.IO;
using System.Text;

namespace UnitTests.Sdmx
{
    [TestClass]
	[DeploymentItem("TestFiles/PR0101B3.px")]
	[DeploymentItem("ExceptationFiles/PR0101B3_sdmx_data.txt")]
	[DeploymentItem("ExceptationFiles/PR0101B3_sdmx_structure.txt")]
    public class FullStringEqualTest
	{
		private SdmxHelper helper = new SdmxHelper();


		[TestMethod]
		public void PR0101B3_Data_norway()
		{

			CultureInfo ci = new CultureInfo("nb-NO");
			System.Threading.Thread.CurrentThread.CurrentCulture = ci;
			System.Threading.Thread.CurrentThread.CurrentUICulture = ci;

			PXModel myModel = helper.GetSelectAllModel("PR0101B3.px");

			string actual = helper.AlignDateInPreparedElement(helper.GetActualData(myModel));

			var expectedfromFile = Encoding.UTF8.GetString( System.IO.File.ReadAllBytes(@"PR0101B3_sdmx_data.txt"));
			//Going via bytes since ReadAllText(@"ExceptationFiles\PR0101B3_sdmx_data.txt")  eats the BOM

			string expected = helper.AlignDateInPreparedElement(expectedfromFile);

			//Checks the first 10 chars first.

			var actual_updated_string = actual.Substring(0, 10);
			var expected_updated_string = expected.Substring(0, 10);
			Assert.AreEqual(expected_updated_string, actual_updated_string);

			//Assert
			//Check their equality.
			Assert.AreEqual(expected, actual);

		}


		[TestMethod]
		public void PR0101B3_Structure_norway()
		{

			CultureInfo ci = new CultureInfo("nb-NO");
			System.Threading.Thread.CurrentThread.CurrentCulture = ci;
			System.Threading.Thread.CurrentThread.CurrentUICulture = ci;

			PXModel myModel = helper.GetSelectAllModel("PR0101B3.px");

			string actual = helper.AlignDateInPreparedElement(helper.GetActualStructure(myModel));

			var expectedfromFile = Encoding.UTF8.GetString(System.IO.File.ReadAllBytes(@"PR0101B3_sdmx_structure.txt"));
			//Going via bytes since ReadAllText(@"ExceptationFiles\PR0101B3_sdmx_data.txt")  eats the BOM

			string expected = helper.AlignDateInPreparedElement(expectedfromFile);

			//Checks the first 10 chars first.

			var actual_updated_string = actual.Substring(0, 10);
			var expected_updated_string = expected.Substring(0, 10);
			Assert.AreEqual(expected_updated_string, actual_updated_string);

			//Assert
			//Check their equality.
			Assert.AreEqual(expected, actual);

		}

	}
}
