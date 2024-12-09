using System.IO;
using System.Text;

using PCAxis.Paxiom;
using PCAxis.Serializers;

namespace UnitTests.Excel
{
    internal class ExcelHelper : UnitTests.Helper
    {




        internal string GetActual(PXModel myModel)
        {
            string actual = "";

            var ser = new XlsxSerializer();

            using (MemoryStream stream = new MemoryStream())
            {


                ser.Serialize(myModel, stream);

                //Hmmm MemoryStream destination seems not to do anything, so I try to outComment
                //				using (MemoryStream destination = new MemoryStream())
                //				{
                //
                //					stream.CopyTo(destination);
                //				}



                actual = Encoding.UTF8.GetString(stream.ToArray());
            }

            return actual;



        }
    }
}
