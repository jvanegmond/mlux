using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Mlux.Lib.Time
{
    [Serializable]
    public class TimeProfile
    {
        public string Name { get; set; }

        // node --------------- node ----------- time ------------- node ------------ node
        // current node   =   last node before given time
        // next node      =   first node after given time

		
        public List<TimeNode> Nodes { get; private set; }

        public TimeProfile()
        {
            Nodes = new List<TimeNode>();
        }

        public TimeNode GetCurrentNode(DateTime time)
        {
            var relTime = TimeUtil.GetRelativeTime(time);
            return GetCurrentNode(relTime);
        }

        public TimeNode GetCurrentNode(TimeSpan relTime)
        {
            // find the last node before the given time
            var last = Nodes.LastOrDefault(node => node.TimeOfDay < relTime);
            if (last != null) return last;

            // if no nodes are found which are after the given time, return the last of the previous day
            return Nodes.LastOrDefault();
        }

        public TimeNode GetNextNode(DateTime time)
        {
            var relTime = TimeUtil.GetRelativeTime(time);
            return GetNextNode(relTime);
        }

        public TimeNode GetNextNode(TimeSpan relTime)
        {
            // find the first node after given time
            var last = Nodes.FirstOrDefault(node => node.TimeOfDay > relTime);
            if (last != null) return last;

            // if no nodes are found which are after the given time, return the first of today
            return Nodes.FirstOrDefault();
        }

        public override string ToString()
        {
            return String.Format("{0} with {1} nodes", GetType(), Nodes.Count);
        }

        public object GetCurrentValue(DateTime time, string nodePropertyName)
        {
            var currentNode = GetCurrentNode(time);
            if (currentNode == null) return null;

            var nextNode = GetNextNode(time);
            if (nextNode == null) return null;

            return LinearNodeInterpolation.Interpolate(time, currentNode, nextNode, nodePropertyName);
        }
    }
}
