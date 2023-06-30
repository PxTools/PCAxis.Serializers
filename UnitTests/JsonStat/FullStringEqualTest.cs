using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using PCAxis.Paxiom;
using System.Globalization;
using System.IO;
using System.Text;

namespace UnitTests.JsonStat
{
    [TestClass]
	[DeploymentItem("TestFiles/PR0101B3.px")]
    public class FullStringEqualTest
	{
		private JsonStatHelper helper = new JsonStatHelper();

		[TestMethod]
		public void PR0101B3_finnish()
		{

			CultureInfo ci = new CultureInfo("fi-FI");
			System.Threading.Thread.CurrentThread.CurrentCulture = ci;
			System.Threading.Thread.CurrentThread.CurrentUICulture = ci;
			/* Or use this ? 
			CultureInfo.DefaultThreadCurrentCulture = ci;
			CultureInfo.DefaultThreadCurrentUICulture = ci;
			*/


			PXModel myModel = helper.GetSelectAllModel("PR0101B3.px");

			string actual = helper.GetActual(myModel);


			var expected = helper.GetExpectedPR0101B3ForServerTimezone();

			var actual_updated_pos = actual.IndexOf("updated");
			var expected_updated_pos = expected.IndexOf("updated");
			Assert.AreEqual(expected_updated_pos, actual_updated_pos, "Position of first occurence of word updated does not match");


			var actual_updated_string= actual.Substring(actual_updated_pos,40);
			var expected_updated_string = expected.Substring(actual_updated_pos, 40);
			Assert.AreEqual(expected_updated_string, actual_updated_string);


			//Assert
			//Check their equality.
			Assert.AreEqual(expected, actual);
			
		}

		[TestMethod]
		public void PR0101B3_norway()
		{

			CultureInfo ci = new CultureInfo("nb-NO");
			System.Threading.Thread.CurrentThread.CurrentCulture = ci;
			System.Threading.Thread.CurrentThread.CurrentUICulture = ci;


			PXModel myModel = helper.GetSelectAllModel("PR0101B3.px");

			string actual = helper.GetActual(myModel);


			var expected = helper.GetExpectedPR0101B3ForServerTimezone();

			var actual_updated_pos = actual.IndexOf("updated");
			var expected_updated_pos = expected.IndexOf("updated");
			Assert.AreEqual(expected_updated_pos, actual_updated_pos, "Position of first occurence of word updated does not match");


			var actual_updated_string = actual.Substring(actual_updated_pos, 40);
			var expected_updated_string = expected.Substring(actual_updated_pos, 40);
			Assert.AreEqual(expected_updated_string, actual_updated_string);


			//Assert
			//Check their equality.
			Assert.AreEqual(expected, actual);

		}

	}
}
