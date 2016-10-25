using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Models.Simple
{
    public class ClassificationResult
    {
        public string Actual { get; private set; }
        public string Expected { get; private set; }

        public ClassificationResult(string actual, string expected)
        {
            Actual = actual;
            Expected = expected;
        }

        public float Score { get { return Actual == Expected ? 1f : 0f; } }
    }
}
