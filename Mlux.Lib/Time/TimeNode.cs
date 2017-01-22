using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Mlux.Lib.Time
{
    [Serializable]
    public class TimeNode
    {
        public List<NodeProperty> Properties { get; private set; }

        // TimeSpan is not Serializable so TimeOfDayDateTime is serialized
        [XmlIgnore]
        public TimeSpan TimeOfDay { get; set; }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public DateTime TimeOfDayDateTime
        {
            get
            {
                return new DateTime(TimeOfDay.Ticks);
            }
            set
            {
                TimeOfDay = new TimeSpan(value.Ticks);
            }
        }

        public TimeNode()
        {
            Properties = new List<NodeProperty>();
        }

        public TimeNode(TimeSpan timeOfDay) : this()
        {
            TimeOfDay = timeOfDay;
        }

        public override string ToString()
        {
            return $"{GetType()} at {TimeOfDay} with  ({string.Join(") (", Properties)})";
        }
    }
}
