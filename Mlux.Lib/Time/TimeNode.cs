using System;
using System.Collections.Generic;

namespace Mlux.Lib.Time
{
    public class TimeNode
    {
        public List<NodeProperty> Properties { get; private set; }
        public TimeSpan TimeOfDay { get; set; }

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
