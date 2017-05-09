using BrightWire.ExecutionGraph.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.Node.Gate
{
    class AddGate : GateBase
    {
        class Backpropagation : BackpropagationBase
        {
            public override IMatrix Backward(IMatrix errorSignal, IContext context, bool calculateOutput)
            {
                return errorSignal;
            }
        }

        public AddGate(string name = null) : base(name) { }

        protected override void _Activate(IContext context, IMatrix primary, IMatrix secondary)
        {
            var output = primary.Add(secondary);

            //_AddHistory(context, output, () => new Backpropagation());
            // default is to pass the error signal through, which is correct for adds
            _AddHistory(context, output, null);
        }
    }
}
