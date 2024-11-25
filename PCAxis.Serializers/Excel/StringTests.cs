using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using ClosedXML.Excel;
using PCAxis.Paxiom;
using PCAxis.Paxiom.Extensions;

namespace PCAxis.Serializers.Excel
{


	public static class StringTests
	{
		public static bool IsNumeric(this string str)
		{
			double result;
			return double.TryParse(str, out result);
		}
	}


}
