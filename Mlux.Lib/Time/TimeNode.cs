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

		[XmlIgnore]
        public TimeSpan TimeOfDay { get; set; }

		[Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
		public DateTime TimeOfDayDateTime {
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
            this.TimeOfDay = timeOfDay;
        }

        public override string ToString()
        {
            return String.Format("{0} at {1} with {2} properties", GetType(), TimeOfDay, Properties.Count);
        }
    }
}
