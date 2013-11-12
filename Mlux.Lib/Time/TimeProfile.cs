using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;

namespace Mlux.Lib.Time
{
    public class TimeProfile
    {
        public NodeManager NodeManager { get; set; }

        public TimeProfile()
        {
            NodeManager = new NodeManager();
        }

        public object GetCurrentValue(DateTime time, string nodePropertyName)
        {
            var currentNode = NodeManager.GetCurrentNode(time);
            if (currentNode == null) return null;
            
            var nextNode = NodeManager.GetNextNode(time);
            if (nextNode == null) return null;

            return LinearNodeInterpolation.Interpolate(time, currentNode, nextNode, nodePropertyName);
        }
    }
}
