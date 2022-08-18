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
	}
}
