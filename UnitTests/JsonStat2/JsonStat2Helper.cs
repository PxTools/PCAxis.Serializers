using System;
using System.Globalization;
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

        internal string GetExpectedBE0101A1WithLocalDate()
        {
            var updatedValue = ConvertToLocalUtcString("20060209 15:20");

            var expectedResponseString = $$$$"""
            {"version":"2.0","class":"dataset","label":"Population PxcMetaTitleBy region, marital status, period, PxcSortVariable PxcMetaTitleAnd sex","source":"Statistics Sweden","updated":"{{{{updatedValue}}}}","note":["This note is mandatory! The tables show the conditions on December 31st for each respective year according to administrative subdivisions of January 1st of the following year","This note is NOT mandatory. Test table note"],"role":{"time":["period"],"metric":["ContentsCode"]},"id":["ContentsCode","region","marital status","period","$$SORT","sex"],"size":[1,3,4,2,2,2],"dimension":{"ContentsCode":{"label":"EliminatedContents","category":{"index":{"EliminatedValue":0},"label":{"EliminatedValue":"Population"},"unit":{"EliminatedValue":{"base":"","decimals":0}}},"extension":{"elimination":true,"refperiod":{"EliminatedValue":"31 Dec each year"}}},"region":{"label":"region","category":{"index":{"01":0,"03":1,"04":2},"label":{"01":"01 Stockholm county","03":"03 Uppsala county","04":"04 S�dermanland county"},"note":{"01":["This note is mandatory! Value note for region 01 Stockholm county","This note is NOT mandatory. Value note for region 01 Stockholm county","This note is also mandatory! Value note for region 01 Stockholm county"],"03":["This note is NOT mandatory. Value note for region 03 Uppsala county","This note is mandatory! Value note for region 03 Uppsala county"]}},"extension":{"elimination":false,"categoryNoteMandatory":{"01":{"0":true,"2":true},"03":{"1":true}},"show":"value_code"}},"marital status":{"label":"marital status","category":{"index":{"OG":0,"G":1,"�NKL":2,"SK":3},"label":{"OG":"single","G":"married","�NKL":"widowers/widows","SK":"divorced"},"note":{"OG":["This note is mandatory! Value note for marital status single"],"G":["This note is mandatory! Value note for marital status married","This note is NOT mandatory. Value note for marital status married"]}},"extension":{"elimination":false,"categoryNoteMandatory":{"OG":{"0":true},"G":{"0":true}},"show":"code_value"}},"period":{"label":"period","note":["This note is mandatory! Note for variable period","This note is NOT mandatory. Note for variable period"],"category":{"index":{"0":0,"1":1},"label":{"0":"2004","1":"2005"},"note":{"0":["This note is mandatory! Value note for period 2004","This note is NOT mandatory. Value note for period 2004"],"1":["This note is mandatory! Value note for period 2005"]}},"extension":{"elimination":false,"noteMandatory":{"0":true},"categoryNoteMandatory":{"0":{"0":true},"1":{"0":true}},"show":"value"}},"$$SORT":{"label":"PxcSortVariable","category":{"index":{"0":0,"1":1},"label":{"0":"number","1":"Per cent"}},"extension":{"elimination":false,"show":"value"}},"sex":{"label":"sex","category":{"index":{"1":0,"2":1},"label":{"1":"men","2":"women"}},"extension":{"elimination":false,"show":"value"}}},"extension":{"noteMandatory":{"0":true},"px":{"infofile":"BE0101","decimals":0,"official-statistics":false,"aggregallowed":true,"language":"en","contents":"Population","descriptiondefault":false,"heading":["period","$$SORT","sex"],"stub":["region","marital status"],"matrix":"BE0101A1","subject-code":"BE","subject-area":"Population"},"contact":[{"raw":"   Befolkningsstatistik, SCB#Tel: 019-17 60 10#E-mail: befolkning@scb.se"}]},"value":[522028,473644,52.43,47.57,528282,479346,52.43,47.57,297649,299470,49.85,50.15,300198,301731,49.87,50.13,16710,67116,19.93,80.07,16578,66248,20.02,79.98,82895,113388,42.23,57.77,83303,114259,42.17,57.83,84478,76684,52.42,47.58,85023,77258,52.39,47.61,50304,50548,49.88,50.12,50555,50765,49.9,50.1,2993,11019,21.36,78.64,3003,10887,21.62,78.38,11541,14997,43.49,56.51,11673,15203,43.43,56.57,67601,57757,53.93,46.07,67737,57988,53.88,46.12,46845,46917,49.96,50.04,46984,47078,49.95,50.05,3430,13046,20.82,79.18,3422,12903,20.96,79.04,11471,14003,45.03,54.97,11622,14161,45.08,54.92]}
            """;
            return expectedResponseString;
        }

        public string ConvertToLocalUtcString(string pxdate)
        {
            var pxfileDate = DateTime.ParseExact(pxdate, PXConstant.PXDATEFORMAT, CultureInfo.InvariantCulture, DateTimeStyles.None);

            var localUtcString = pxfileDate.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);

            return localUtcString;
        }
    }
}
