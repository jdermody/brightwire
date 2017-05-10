using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.Node.Gate
{
    class MultiplyGate : BinaryGateBase
    {
        class Backpropagation : BackpropagationBase
        {
            readonly IMatrix _input1, _input2;

            public Backpropagation(IMatrix input1, IMatrix input2)
            {
                _input1 = input1;
                _input2 = input2;
            }

            protected override void _Dispose(bool isDisposing)
            {
                _input1.Dispose();
                _input2.Dispose();
            }

            public override void Backward(IMatrix errorSignal, IContext context, IReadOnlyList<INode> parents)
            {
                using (var delta1 = errorSignal.PointwiseMultiply(_input2))
                using (var delta2 = errorSignal.PointwiseMultiply(_input1)) {
                    context.AddBackward(delta1, parents.First());
                    context.AddBackward(delta2, parents.Last());
                }
            }
        }
        public MultiplyGate(string name = null) : base(name) { }

        protected override void _Activate(IContext context, IMatrix primary, IMatrix secondary)
        {
            var output = primary.PointwiseMultiply(secondary);
            _AddHistory(context, output, () => new Backpropagation(primary,  secondary));
        }
    }
}
