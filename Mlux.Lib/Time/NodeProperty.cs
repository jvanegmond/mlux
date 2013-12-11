using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mlux.Lib.Time
{
    public class NodeProperty
    {
        public const string Brightness = "Brightness";
        public const string ColorTemperature = "ColorTemperature";

        public string Name { get; set; }
        public object Value { get; set; }

        public NodeProperty()
        {
            
        }

        public NodeProperty(string name, object value)
        {
            Name = name;
            Value = value;
        }

        public override string ToString()
        {
            return String.Format("{0} with name {1} and value {2}", GetType(), Name, Value);
        }
    }
}
