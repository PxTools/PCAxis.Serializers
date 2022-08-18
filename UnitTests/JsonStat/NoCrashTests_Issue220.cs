using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using PCAxis.Paxiom;
using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace UnitTests.JsonStat
{
	[TestClass]
	public class NoCrashTests_Issue220
	{
		private JsonStatHelper helper = new JsonStatHelper();


		[TestMethod]
		public void PR0101B3_CultureInfoFinnish()
		{
			CultureInfo ci = new CultureInfo("fi-FI");
			System.Threading.Thread.CurrentThread.CurrentCulture = ci;
			System.Threading.Thread.CurrentThread.CurrentUICulture = ci;


			PXModel myModel = helper.GetSelectAllModel("TestFiles//PR0101B3.px");


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


			PXModel myModel = helper.GetSelectAllModel("TestFiles//Issue220Finland.px");


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


			PXModel myModel = helper.GetSelectAllModel("TestFiles//Issue220Finland.px");



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
