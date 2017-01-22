using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mlux.Lib.Database;
using Mlux.Lib.Time;

namespace Mlux.Lib.Tests
{
    [TestClass]
    public class TimeProfileSerializerTest
    {
        [TestMethod]
        public void TestItNow()
        {
            var originalProfile = new TimeProfile
                {
                    Name = "Test time profile"
                };
            var originalNode = new TimeNode(TimeSpan.FromHours(7));
            originalNode.Properties.Add(new NodeProperty(NodeProperty.Brightness, 20));
            originalNode.Properties.Add(new NodeProperty(NodeProperty.ColorTemperature, 3300));
            originalProfile.Nodes.Add(originalNode);

            var str = TimeProfileSerializer.Serialize(originalProfile);
            Assert.IsNotNull(str);

	        var deserializedProfile = TimeProfileSerializer.Deserialize(str) as TimeProfile;
			Assert.IsNotNull(deserializedProfile);
			Assert.AreEqual(originalProfile.Name, deserializedProfile.Name);
			Assert.AreEqual(originalProfile.Nodes.Count, deserializedProfile.Nodes.Count);

	        var deserializedNode = deserializedProfile.Nodes[0];
			Assert.AreEqual(originalNode.TimeOfDay, deserializedNode.TimeOfDay);

	        var originalBrightnessProperty = originalNode.Properties.First(_ => _.Name == NodeProperty.Brightness);
			var deserializedBrightnessProperty = deserializedNode.Properties.First(_ => _.Name == NodeProperty.Brightness);

			Assert.AreEqual(originalBrightnessProperty.Name, deserializedBrightnessProperty.Name);
			Assert.AreEqual(originalBrightnessProperty.Value, deserializedBrightnessProperty.Value);

			var originalColorTemperatureProperty = originalNode.Properties.First(_ => _.Name == NodeProperty.Brightness);
			var deserializedColorTemperatureProperty = deserializedNode.Properties.First(_ => _.Name == NodeProperty.Brightness);

			Assert.AreEqual(originalColorTemperatureProperty.Name, deserializedColorTemperatureProperty.Name);
			Assert.AreEqual(originalColorTemperatureProperty.Value, deserializedColorTemperatureProperty.Value);
        }
    }
}
