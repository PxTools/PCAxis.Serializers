using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using PCAxis.Paxiom;
using System.Globalization;
using System.IO;
using System.Text;

namespace UnitTests.JsonStat
{
    [TestClass]
    public class FullStringEqualTest
	{
		private JsonStatHelper helper = new JsonStatHelper();

		[TestMethod]
		public void PR0101B3_finnish()
		{

			CultureInfo ci = new CultureInfo("fi-FI");
			CultureInfo.DefaultThreadCurrentCulture = ci;
			CultureInfo.DefaultThreadCurrentUICulture = ci;


			PXModel myModel = helper.GetSelectAllModel("TestFiles//PR0101B3.px");

			string actual = helper.GetActual(myModel);


			var expected = "{\"dataset\":{\"dimension\":{\"Product group\":{\"label\":\"Product group\",\"category\":{\"index\":{\"01\":0,\"01.1\":1,\"01.1.1\":2,\"01.1.2\":3,\"01.1.3\":4,\"01.1.4\":5,\"01.1.5\":6,\"01.1.6\":7},\"label\":{\"01\":\"01 Food and non-alcoholic beverages\",\"01.1\":\"01.1 Food\",\"01.1.1\":\"01.1.1 Bread and cereals\",\"01.1.2\":\"01.1.2 Meat\",\"01.1.3\":\"01.1.3 Fish\",\"01.1.4\":\"01.1.4 Milk, cheese and eggs\",\"01.1.5\":\"01.1.5 Oils and fats\",\"01.1.6\":\"01.1.6 Fruit\"}}},\"period\":{\"label\":\"period\",\"category\":{\"index\":{\"0\":0,\"1\":1,\"2\":2,\"3\":3,\"4\":4},\"label\":{\"0\":\"2006M01\",\"1\":\"2006M02\",\"2\":\"2006M03\",\"3\":\"2006M04\",\"4\":\"2006M05\"}}},\"id\":[\"Product group\",\"period\"],\"size\":[8,5],\"role\":{\"time\":[\"period\"]}},\"label\":\"Consumer Price Index (CPI) (by COICOP), 1980=100 PxcMetaTitleBy Product group PxcMetaTitleAnd period\",\"source\":\"Statistics Sweden\",\"updated\":\"2006-07-05T12:21:00Z\",\"value\":[237,238.6,238.7,238.3,242.1,246.1,247.9,248,247.6,251.9,245.3,243.6,245.3,244.7,245,229.3,232.2,232.2,232,232.9,316.3,314.9,321.9,315.3,332.8,266.9,267.4,267,266.3,267.8,208.8,211.2,211.8,211.5,211.7,232.2,227,231.8,232.8,238.2],\"extension\":{\"px\":{\"infofile\":\"PR0101\",\"decimals\":1}}}}";


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
			CultureInfo.DefaultThreadCurrentCulture = ci;
			CultureInfo.DefaultThreadCurrentUICulture = ci;


			PXModel myModel = helper.GetSelectAllModel("TestFiles//PR0101B3.px");

			string actual = helper.GetActual(myModel);


			var expected = "{\"dataset\":{\"dimension\":{\"Product group\":{\"label\":\"Product group\",\"category\":{\"index\":{\"01\":0,\"01.1\":1,\"01.1.1\":2,\"01.1.2\":3,\"01.1.3\":4,\"01.1.4\":5,\"01.1.5\":6,\"01.1.6\":7},\"label\":{\"01\":\"01 Food and non-alcoholic beverages\",\"01.1\":\"01.1 Food\",\"01.1.1\":\"01.1.1 Bread and cereals\",\"01.1.2\":\"01.1.2 Meat\",\"01.1.3\":\"01.1.3 Fish\",\"01.1.4\":\"01.1.4 Milk, cheese and eggs\",\"01.1.5\":\"01.1.5 Oils and fats\",\"01.1.6\":\"01.1.6 Fruit\"}}},\"period\":{\"label\":\"period\",\"category\":{\"index\":{\"0\":0,\"1\":1,\"2\":2,\"3\":3,\"4\":4},\"label\":{\"0\":\"2006M01\",\"1\":\"2006M02\",\"2\":\"2006M03\",\"3\":\"2006M04\",\"4\":\"2006M05\"}}},\"id\":[\"Product group\",\"period\"],\"size\":[8,5],\"role\":{\"time\":[\"period\"]}},\"label\":\"Consumer Price Index (CPI) (by COICOP), 1980=100 PxcMetaTitleBy Product group PxcMetaTitleAnd period\",\"source\":\"Statistics Sweden\",\"updated\":\"2006-07-05T12:21:00Z\",\"value\":[237,238.6,238.7,238.3,242.1,246.1,247.9,248,247.6,251.9,245.3,243.6,245.3,244.7,245,229.3,232.2,232.2,232,232.9,316.3,314.9,321.9,315.3,332.8,266.9,267.4,267,266.3,267.8,208.8,211.2,211.8,211.5,211.7,232.2,227,231.8,232.8,238.2],\"extension\":{\"px\":{\"infofile\":\"PR0101\",\"decimals\":1}}}}";


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
