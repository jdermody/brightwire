using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Models.Output
{
    /// <summary>
    /// A classification result together with the expected classification
    /// </summary>
    public class ClassificationResult
    {
        /// <summary>
        /// The actual prediction
        /// </summary>
        public string Actual { get; private set; }

        /// <summary>
        /// The expected prediction
        /// </summary>
        public string Expected { get; private set; }

        internal ClassificationResult(string actual, string expected)
        {
            Actual = actual;
            Expected = expected;
        }

        /// <summary>
        /// Converts the result into a number
        /// </summary>
        public float Score { get { return Actual == Expected ? 1f : 0f; } }
    }
}
