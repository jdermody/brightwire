using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.Models.ExecutionResults
{
    /// <summary>
    /// The output of a feed forward neural network and its expected output
    /// </summary>
    internal class FeedForwardOutput : IFeedForwardOutput
    {
        /// <summary>
        /// The actual output
        /// </summary>
        public IIndexableVector Output { get; private set; }

        /// <summary>
        /// The expected output
        /// </summary>
        public IIndexableVector ExpectedOutput { get; private set; }

        public FeedForwardOutput(IIndexableVector output, IIndexableVector expectedOutput)
        {
            Output = output;
            ExpectedOutput = expectedOutput;
        }
    }
}
