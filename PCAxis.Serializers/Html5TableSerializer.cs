using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PCAxis.Paxiom;
using PCAxis.Paxiom.Extensions;

namespace PCAxis.Serializers
{
	public class Html5TableSerializer : HtmlSerializer
	{

        public Html5TableSerializer()
        {
            IncludeTitle = true;
        }

    }
}

