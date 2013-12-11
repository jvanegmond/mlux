using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Mlux.Lib.Time
{
    public class TimeNode
    {
        [XmlArrayAttribute]
        public List<NodeProperty> Properties { get; private set; }

        [XmlIgnore]
        public TimeSpan TimeOfDay { get; set; }

        [XmlElement]
        public long TimeOfDayTicks
        {
            get { return TimeOfDay.Ticks; }
            set { TimeOfDay = new TimeSpan(value); }
        }

        public TimeNode()
        {
            Properties = new List<NodeProperty>();
        }

        public TimeNode(TimeSpan timeOfDay)
            : this()
        {
            this.TimeOfDay = timeOfDay;
        }

        public override string ToString()
        {
            return String.Format("{0} at {1} with {2} properties", GetType(), TimeOfDay, Properties.Count);
        }
    }
}
