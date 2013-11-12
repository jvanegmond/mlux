using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mlux.Lib.Time;

namespace Mlux.Lib.Tests
{
    [TestClass]
    public class LinearNodeInterpolationTest
    {
        [TestMethod]
        public void GetRelTimePercentageTest1()
        {
            var start = TimeSpan.FromHours(10);
            var end = TimeSpan.FromHours(14);

            Assert.AreEqual(0, LinearNodeInterpolation.GetRelTimePercentage(TimeSpan.FromHours(10), start, end));
            Assert.AreEqual(0.25, LinearNodeInterpolation.GetRelTimePercentage(TimeSpan.FromHours(11), start, end));
            Assert.AreEqual(0.5, LinearNodeInterpolation.GetRelTimePercentage(TimeSpan.FromHours(12), start, end));
            Assert.AreEqual(0.75, LinearNodeInterpolation.GetRelTimePercentage(TimeSpan.FromHours(13), start, end));
            Assert.AreEqual(1, LinearNodeInterpolation.GetRelTimePercentage(TimeSpan.FromHours(14), start, end));
        }

        [TestMethod]
        public void GetRelTimePercentageTest2()
        {
            var start = TimeSpan.FromHours(22);
            var end = TimeSpan.FromHours(2);

            Assert.AreEqual(0, LinearNodeInterpolation.GetRelTimePercentage(TimeSpan.FromHours(22), start, end));
            Assert.AreEqual(0.25, LinearNodeInterpolation.GetRelTimePercentage(TimeSpan.FromHours(23), start, end));
            Assert.AreEqual(0.5, LinearNodeInterpolation.GetRelTimePercentage(TimeSpan.FromHours(0), start, end));
            Assert.AreEqual(0.75, LinearNodeInterpolation.GetRelTimePercentage(TimeSpan.FromHours(1), start, end));
            Assert.AreEqual(1, LinearNodeInterpolation.GetRelTimePercentage(TimeSpan.FromHours(2), start, end));
        }

        [TestMethod]
        public void GetRelTimePercentageTest3()
        {
            var start = TimeSpan.FromHours(22);
            var end = TimeSpan.FromHours(6);

            Assert.AreEqual(0, LinearNodeInterpolation.GetRelTimePercentage(TimeSpan.FromHours(22), start, end));
            Assert.AreEqual(0.25, LinearNodeInterpolation.GetRelTimePercentage(TimeSpan.FromHours(0), start, end));
            Assert.AreEqual(0.5, LinearNodeInterpolation.GetRelTimePercentage(TimeSpan.FromHours(2), start, end));
            Assert.AreEqual(0.75, LinearNodeInterpolation.GetRelTimePercentage(TimeSpan.FromHours(4), start, end));
            Assert.AreEqual(1, LinearNodeInterpolation.GetRelTimePercentage(TimeSpan.FromHours(6), start, end));
        }

        [TestMethod]
        public void InterpolateTest1()
        {
            Assert.AreEqual(10, LinearNodeInterpolation.Interpolate(10, 20, 0));
            Assert.AreEqual(12, LinearNodeInterpolation.Interpolate(10, 20, 0.25));
            Assert.AreEqual(15, LinearNodeInterpolation.Interpolate(10, 20, 0.5));
            Assert.AreEqual(17, LinearNodeInterpolation.Interpolate(10, 20, 0.75));
            Assert.AreEqual(20, LinearNodeInterpolation.Interpolate(10, 20, 1));
        }
    }
}
