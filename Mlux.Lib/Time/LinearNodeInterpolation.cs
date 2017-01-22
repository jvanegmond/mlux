using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mlux.Lib.Time
{
    public static class LinearNodeInterpolation
    {
        public static object Interpolate(TimeSpan time, TimeNode currentNode, TimeNode nextNode, string propertyName)
        {
            var currentProperty = currentNode.Properties.First(prop => prop.Name == propertyName);
            var nextProperty = nextNode.Properties.First(prop => prop.Name == propertyName);

            var currentValue = currentProperty.Value;
            var nextValue = nextProperty.Value;

            if (currentValue == null) return null;
            if (nextValue == null) return null;

            if (currentValue.GetType() != nextValue.GetType()) return null;
            
            var percentage = GetRelTimePercentage(time, currentNode.TimeOfDay, nextNode.TimeOfDay);

            return Interpolate(currentValue, nextValue, percentage);
        }

        public static double GetRelTimePercentage(TimeSpan relTime, TimeSpan currentTime, TimeSpan nextTime)
        {
            var timeToCurrentNode = relTime.TotalHours - currentTime.TotalHours;
            var timeToNextNode = nextTime.TotalHours - relTime.TotalHours;

            if (timeToCurrentNode < 0) {
                timeToCurrentNode = 24 + timeToCurrentNode;
            }

            if (timeToNextNode < 0) {
                timeToNextNode = 24 + timeToNextNode;
            }

            return timeToCurrentNode / (timeToCurrentNode + timeToNextNode);
        }

        public static object Interpolate(object a, object b, double percentage)
        {
            var type = a.GetType();

            if (type == typeof(int)) {
                var diff = (int) b - (int) a;
                var result = (int) a + (diff*percentage);
                return (int)result;
            }
            else if (type == typeof(double))
            {
                var diff = (double)b - (double)a;
                var result = (double)a + (diff * percentage);
                return (double) result;
            }

            throw new NotImplementedException();
        }
    }
}
