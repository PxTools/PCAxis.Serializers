using PCAxis.Paxiom;
using PCAxis.Serializers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UnitTests.JsonStat
{
	internal class JsonStatHelper : UnitTests.Helper
	{

		internal string GetActual(PXModel myModel)
		{
			string actual = "";


			using (MemoryStream memStream = new MemoryStream(1000))
			{
				JsonStatSerializer jss = new JsonStatSerializer();
				jss.Serialize(myModel, memStream);

				actual = Encoding.UTF8.GetString(memStream.GetBuffer(), 0, (int)memStream.Length);
			}
			return actual;
		}



		/// <summary>
		/// The "updated" is in utc. Its value is (for some, depending on stuff
		///   in the PX-file like  model.Meta.ContentVariable ) a transformation of a string without explisit timezone.
		///   In PR0101B3 CREATION-DATE="20060705 14:21";
		///   This means it depends on the timezone where the server is located,
		///   so the UTC value will not be the same in London and Oslo. 
		///   This means the expected value has to change with where the test is run.
		/// </summary>
		/// <returns></returns>
		internal string GetExpectedPR0101B3ForServerTimezone()
        {
			var timeZone = TimeZoneInfo.Local;
			var t1 = timeZone.BaseUtcOffset.Hours;  //Will this value change in winter, and break the tests?
			
            if (t1 == 1)
            {
                //Oslo   
                return "{\"dataset\":{\"dimension\":{\"Product group\":{\"label\":\"Product group\",\"category\":{\"index\":{\"01\":0,\"01.1\":1,\"01.1.1\":2,\"01.1.2\":3,\"01.1.3\":4,\"01.1.4\":5,\"01.1.5\":6,\"01.1.6\":7},\"label\":{\"01\":\"01 Food and non-alcoholic beverages\",\"01.1\":\"01.1 Food\",\"01.1.1\":\"01.1.1 Bread and cereals\",\"01.1.2\":\"01.1.2 Meat\",\"01.1.3\":\"01.1.3 Fish\",\"01.1.4\":\"01.1.4 Milk, cheese and eggs\",\"01.1.5\":\"01.1.5 Oils and fats\",\"01.1.6\":\"01.1.6 Fruit\"}},\"extension\":{\"show\":\"value\"}},\"period\":{\"label\":\"period\",\"category\":{\"index\":{\"0\":0,\"1\":1,\"2\":2,\"3\":3,\"4\":4},\"label\":{\"0\":\"2006M01\",\"1\":\"2006M02\",\"2\":\"2006M03\",\"3\":\"2006M04\",\"4\":\"2006M05\"}},\"extension\":{\"show\":\"value\"}},\"id\":[\"Product group\",\"period\"],\"size\":[8,5],\"role\":{\"time\":[\"period\"]}},\"label\":\"Consumer Price Index (CPI) (by COICOP), 1980=100 PxcMetaTitleBy Product group PxcMetaTitleAnd period\",\"source\":\"Statistics Sweden\",\"updated\":\"2006-07-05T12:21:00Z\",\"value\":[237,238.6,238.7,238.3,242.1,246.1,247.9,248,247.6,251.9,245.3,243.6,245.3,244.7,245,229.3,232.2,232.2,232,232.9,316.3,314.9,321.9,315.3,332.8,266.9,267.4,267,266.3,267.8,208.8,211.2,211.8,211.5,211.7,232.2,227,231.8,232.8,238.2],\"extension\":{\"px\":{\"infofile\":\"PR0101\",\"decimals\":1}}}}";

            }
            else if (t1 == 0)
            {
                //GitHub  
                return "{\"dataset\":{\"dimension\":{\"Product group\":{\"label\":\"Product group\",\"category\":{\"index\":{\"01\":0,\"01.1\":1,\"01.1.1\":2,\"01.1.2\":3,\"01.1.3\":4,\"01.1.4\":5,\"01.1.5\":6,\"01.1.6\":7},\"label\":{\"01\":\"01 Food and non-alcoholic beverages\",\"01.1\":\"01.1 Food\",\"01.1.1\":\"01.1.1 Bread and cereals\",\"01.1.2\":\"01.1.2 Meat\",\"01.1.3\":\"01.1.3 Fish\",\"01.1.4\":\"01.1.4 Milk, cheese and eggs\",\"01.1.5\":\"01.1.5 Oils and fats\",\"01.1.6\":\"01.1.6 Fruit\"}},\"extension\":{\"show\":\"value\"}},\"period\":{\"label\":\"period\",\"category\":{\"index\":{\"0\":0,\"1\":1,\"2\":2,\"3\":3,\"4\":4},\"label\":{\"0\":\"2006M01\",\"1\":\"2006M02\",\"2\":\"2006M03\",\"3\":\"2006M04\",\"4\":\"2006M05\"}},\"extension\":{\"show\":\"value\"}},\"id\":[\"Product group\",\"period\"],\"size\":[8,5],\"role\":{\"time\":[\"period\"]}},\"label\":\"Consumer Price Index (CPI) (by COICOP), 1980=100 PxcMetaTitleBy Product group PxcMetaTitleAnd period\",\"source\":\"Statistics Sweden\",\"updated\":\"2006-07-05T14:21:00Z\",\"value\":[237,238.6,238.7,238.3,242.1,246.1,247.9,248,247.6,251.9,245.3,243.6,245.3,244.7,245,229.3,232.2,232.2,232,232.9,316.3,314.9,321.9,315.3,332.8,266.9,267.4,267,266.3,267.8,208.8,211.2,211.8,211.5,211.7,232.2,227,231.8,232.8,238.2],\"extension\":{\"px\":{\"infofile\":\"PR0101\",\"decimals\":1}}}}";
            }



            else
            {
				throw new Exception(String.Format("Your timeZone.BaseUtcOffset.Hours is {0}, is not supported yet. Please add it here.", t1));
            }

		}


	}
}
