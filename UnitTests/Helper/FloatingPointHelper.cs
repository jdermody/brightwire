using BrightWire;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.Helper
{
    public static class FloatingPointHelper
    {
        public static unsafe int FloatToInt32Bits(float f)
        {
            return *((int*)&f);
        }

        public static bool AlmostEqual2sComplement(float a, float b, int maxDeltaBits)
        {
            int aInt = FloatToInt32Bits(a);
            if (aInt < 0)
                aInt = Int32.MinValue - aInt;

            int bInt = FloatToInt32Bits(b);
            if (bInt < 0)
                bInt = Int32.MinValue - bInt;

            int intDiff = Math.Abs(aInt - bInt);
            return intDiff <= (1 << maxDeltaBits);
        }

        public static void AssertEqual(float v1, float v2, int maxDifference = 6)
        {
            if (float.IsNaN(v1) && float.IsNaN(v2))
                return;
            Assert.IsTrue(AlmostEqual2sComplement(v1, v2, maxDifference));
        }

        public static void AssertEqual(IIndexableMatrix m1, IIndexableMatrix m2, int maxDifference = 6)
        {
            Assert.AreEqual(m1.RowCount, m2.RowCount);
            Assert.AreEqual(m1.ColumnCount, m2.ColumnCount);
            for (var i = 0; i < m1.RowCount; i++) {
                for (var j = 0; j < m1.ColumnCount; j++) {
                    AssertEqual(m1[i, j], m2[i, j], maxDifference);
                }
            }
        }

        public static void AssertEqual(IIndexableVector v1, IIndexableVector v2, int maxDifference = 6)
        {
            Assert.AreEqual(v1.Count, v2.Count);
            for (var i = 0; i < v1.Count; i++) {
                AssertEqual(v1[i], v2[i], maxDifference);
            }
        }
    }
}
