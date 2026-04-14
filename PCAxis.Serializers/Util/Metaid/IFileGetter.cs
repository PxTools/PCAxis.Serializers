using System.Collections.Generic;
using System.Xml.Linq;

namespace PCAxis.Serializers.Util.MetaId
{
    internal interface IFileGetter
    {
        Dictionary<string, string> ReadLabelsfile(string filepath);

        XDocument ReadConfig(string configurationFile);
    }
}
