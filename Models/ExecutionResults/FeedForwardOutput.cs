using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.Net4.Models.ExecutionResults
{
    public class FeedForwardOutput : IFeedForwardOutput
    {
        public IIndexableVector Output { get; private set; }
        public IIndexableVector ExpectedOutput { get; private set; }

        public FeedForwardOutput(IIndexableVector output, IIndexableVector expectedOutput)
        {
            Output = output;
            ExpectedOutput = expectedOutput;
        }
    }
}
