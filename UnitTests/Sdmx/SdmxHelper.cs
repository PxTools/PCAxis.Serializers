using System.IO;
using System.Text;

using PCAxis.Paxiom;
using PCAxis.Serializers;

namespace UnitTests.Sdmx
{
    internal class SdmxHelper : UnitTests.Helper
    {

        internal string GetActualStructure(PXModel myModel)
        {
            string actual = "";


            using (MemoryStream memStream = new MemoryStream(1000))
            {
                SdmxStructureSerializer serializer = new SdmxStructureSerializer();

                serializer.Serialize(myModel, memStream);

                actual = Encoding.UTF8.GetString(memStream.GetBuffer(), 0, (int)memStream.Length);
            }
            return actual;
        }


        internal string GetActualData(PXModel myModel)
        {
            string actual = "";


            using (MemoryStream memStream = new MemoryStream(1000))
            {
                SdmxDataSerializer serializer = new SdmxDataSerializer();
                serializer.Serialize(myModel, memStream);

                actual = Encoding.UTF8.GetString(memStream.GetBuffer(), 0, (int)memStream.Length);
            }
            return actual;
        }


        //replaces date in <Prepared>2022-08-18T14:49:14Z</Prepared> to something constant
        internal string AlignDateInPreparedElement(string inSdmx)
        {
            var start_pos = inSdmx.IndexOf("<Prepared>");

            var end_pos = inSdmx.IndexOf("</Prepared>");

            var badString = inSdmx.Substring(start_pos, end_pos - start_pos);

            return inSdmx.Replace(badString, "<Prepared>2000-12-24T17:00:00Z");


        }

    }
}
