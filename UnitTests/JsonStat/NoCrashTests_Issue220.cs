using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using PCAxis.Paxiom;
using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace PCAxis.Serializers.JsonStat.UnitTest
{
    [TestClass]
    public class NoCrashTests_Issue220
	{

	

		[TestMethod]
		public void PR0101B3_CultureInfoFinnish()
		{
			CultureInfo ci = new CultureInfo("fi-FI");
			System.Threading.Thread.CurrentThread.CurrentCulture = ci;
			System.Threading.Thread.CurrentThread.CurrentUICulture = ci;


			PXModel myModel = GetSelectAllModel("TestFiles//PR0101B3.px");

			string actual = "";

			try
			{

				using (MemoryStream memStream = new MemoryStream(1000))
				{
					JsonStatSerializer jss = new JsonStatSerializer();
					jss.Serialize(myModel, memStream);

					actual = Encoding.UTF8.GetString(memStream.GetBuffer(), 0, (int)memStream.Length);
				}


				Assert.IsTrue(actual.Length >= 1, "Made it!");
			}
			catch (Exception e)
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


			PXModel myModel = GetSelectAllModel("TestFiles//Issue220Finland.px");

			string actual = "";

			try
			{

				using (MemoryStream memStream = new MemoryStream(1000))
				{
					JsonStatSerializer jss = new JsonStatSerializer();
					jss.Serialize(myModel, memStream);

					actual = Encoding.UTF8.GetString(memStream.GetBuffer(), 0, (int)memStream.Length);
				}


				Assert.IsTrue(actual.Length >= 1, "Made it!");
			}
			catch (Exception e)
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


			PXModel myModel = GetSelectAllModel("TestFiles//Issue220Finland.px");


			string actual = "";

			try
			{

				using (MemoryStream memStream = new MemoryStream(1000))
				{
					JsonStatSerializer jss = new JsonStatSerializer();
					jss.Serialize(myModel, memStream);

					actual = Encoding.UTF8.GetString(memStream.GetBuffer(), 0, (int)memStream.Length);
				}


				Assert.IsTrue(actual.Length >= 1,"Made it!");
			}
			catch (Exception e)
			{
				Assert.Fail();
			}
		}


		private PXModel GetSelectAllModel(string file)
        {
			PCAxis.Paxiom.IPXModelBuilder builder = new PXFileBuilder();

			builder.SetPath(file);
			builder.BuildForSelection();

			PXMeta meta = builder.Model.Meta;
			builder.BuildForPresentation(Selection.SelectAll(meta));

			return builder.Model;
		}


	}
}
