using BrightWire.ExecutionGraph.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.Node.Gate
{
    class AddGate : BinaryGateBase
    {
        public AddGate(string name = null) : base(name) { }

        protected override void _Activate(IContext context, IMatrix primary, IMatrix secondary)
        {
            var output = primary.Add(secondary);

            // default is to pass the error signal through, which is correct for addition
            _AddHistory(context, output, null);
        }
    }
}
