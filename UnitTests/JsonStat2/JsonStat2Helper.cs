using System.IO;
using System.Text;
using PCAxis.Paxiom;
using PCAxis.Serializers;

namespace UnitTests.JsonStat2
{
    internal class JsonStat2Helper : UnitTests.Helper
    {

        internal string GetActual(PXModel myModel)
        {
            string actual = "";
            
            using (MemoryStream memStream = new MemoryStream(1000))
            {
                JsonStat2Serializer jss = new JsonStat2Serializer();
                jss.Serialize(myModel, memStream);

                actual = Encoding.UTF8.GetString(memStream.GetBuffer(), 0, (int)memStream.Length);
            }

            return actual;
        }
    }
}