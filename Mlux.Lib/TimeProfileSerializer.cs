using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Mlux.Lib.Time;

namespace Mlux.Lib
{
    public static class TimeProfileSerializer
    {
        static readonly XmlSerializer Serializer = new XmlSerializer(typeof(TimeProfile));

        public static string Serialize(TimeProfile profile)
        {
            var sb = new StringBuilder();
            var stream = new StringWriter(sb);

            Serializer.Serialize(stream, profile);

            return sb.ToString();
        }

        public static TimeProfile Deserialize(string profile)
        {
            var reader = new StringReader(profile);
            var result = (TimeProfile)Serializer.Deserialize(reader);
            return result;
        }

    }
}
