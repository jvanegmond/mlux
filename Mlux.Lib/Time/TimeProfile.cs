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
        public const int MaxTemperature = 6500;
        public const int MinTemperature = 3200;
        public const int MinBrightness = 0;
        public const int MaxBrightness = 100;

        public string Name { get; set; }

        // node --------------- node ----------- time ------------- node ------------ node
        // current node   =   last node before given time
        // next node      =   first node after given time

		
        public List<TimeNode> Nodes { get; private set; }

        public TimeProfile()
        {
            Nodes = new List<TimeNode>();
        }

        public override string ToString()
        {
            return $"{GetType()} with {Nodes.Count} nodes";
        }

        public object GetCurrentValue(TimeNode previous, TimeNode next, TimeSpan time, string nodePropertyName)
        {
            return LinearNodeInterpolation.Interpolate(time, previous, next, nodePropertyName);
        }
    }
}
