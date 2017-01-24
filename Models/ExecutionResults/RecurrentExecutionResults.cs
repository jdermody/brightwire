using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.Models.ExecutionResults
{
    internal class RecurrentExecutionResults : IRecurrentExecutionResults
    {
        public IIndexableVector Memory { get; private set; }
        public IIndexableVector Output { get; private set; }
        public IIndexableVector ExpectedOutput { get; private set; }

        public RecurrentExecutionResults(IIndexableVector output, IIndexableVector target, IIndexableVector memory) { Output = output; ExpectedOutput = target; Memory = memory; }
    }
}
