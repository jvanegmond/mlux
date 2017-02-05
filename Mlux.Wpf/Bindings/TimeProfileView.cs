using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mlux.Lib.Time;

namespace Mlux.Wpf.Bindings
{
    public class TimeProfileView
    {
        public TimeProfile UnderlyingProfile { get; }

        public ObservableCollection<TimeNodeView> TimeNodes { get; set; }
        public List<TimeNodeView> Nodes;

        public TimeProfileView(TimeProfile profile)
        {
            UnderlyingProfile = profile;
            Nodes = new List<TimeNodeView>();
            foreach (var node in UnderlyingProfile.Nodes)
            {
                Nodes.Add(new TimeNodeView(node));
            }
        }

        public TimeNodeView Next(DateTime now)
        {
            var timeNodeNext = UnderlyingProfile.Next(now);
            return Nodes.First(_ => _.UnderlyingNode == timeNodeNext);
        }

        public TimeNodeView Prev(DateTime now)
        {
            var timeNodeNext = UnderlyingProfile.Previous(now);
            return Nodes.First(_ => _.UnderlyingNode == timeNodeNext);
        }

        public TimeNodeView First()
        {
            var minValue = TimeSpan.MaxValue;
            TimeNodeView minResult = null;
            foreach (var node in Nodes)
            {
                if (node.TimeOfDay < minValue)
                {
                    minResult = node;
                    minValue = node.TimeOfDay;
                }
            }
            return minResult;
        }

        public TimeNodeView Last()
        {
            var maxValue = TimeSpan.MinValue;
            TimeNodeView minResult = null;
            foreach (var node in Nodes)
            {
                if (node.TimeOfDay > maxValue)
                {
                    minResult = node;
                    maxValue = node.TimeOfDay;
                }
            }
            return minResult;
        }
    }
}
