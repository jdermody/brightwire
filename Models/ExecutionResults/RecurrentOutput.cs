using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.Models.ExecutionResults
{
    public class RecurrentOutput : IRecurrentOutput
    {
        public IIndexableVector Output { get; private set; }
        public IIndexableVector Memory { get; private set; }

        public RecurrentOutput(IIndexableVector output, IIndexableVector memory)
        {
            Output = output;
            Memory = memory;
        }
    }
}
