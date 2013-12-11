using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mlux.Lib.Time;

namespace Mlux.Lib.Tests
{
    [TestClass]
    public class TimeProfileSerializerTest
    {
        [TestMethod]
        public void TestItNow()
        {
            var result = new TimeProfile
                {
                    Name = "Test time profile"
                };
            var wakeUpTime = new TimeNode(TimeSpan.FromHours(7));
            wakeUpTime.Properties.Add(new NodeProperty(NodeProperty.Brightness, 20));
            wakeUpTime.Properties.Add(new NodeProperty(NodeProperty.ColorTemperature, 3300));
            result.Nodes.Add(wakeUpTime);

            var str = TimeProfileSerializer.Serialize(result);

            Assert.IsNotNull(str);
        }
    }
}
