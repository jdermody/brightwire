using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.Node.Gate
{
    class MultiplyGate : BinaryGateBase
    {
        class Backpropagation : BackpropagationBase<MultiplyGate>
        {
            readonly IMatrix _input1, _input2;

            public Backpropagation(MultiplyGate source, IMatrix input1, IMatrix input2) : base(source)
            {
                _input1 = input1;
                _input2 = input2;
            }

            protected override void _Dispose(bool isDisposing)
            {
                //_input1.Dispose();
                //_input2.Dispose();
            }

            public override void _Backward(INode fromNode, IGraphData errorSignal, IContext context, IReadOnlyList<INode> parents)
            {
                var es = errorSignal.GetMatrix();
                var delta1 = es.PointwiseMultiply(_input2);
                var delta2 = es.PointwiseMultiply(_input1);
                context.AddBackward(errorSignal.ReplaceWith(delta1), parents.First(), _source);
                context.AddBackward(errorSignal.ReplaceWith(delta2), parents.Last(), _source);
            }
        }
        public MultiplyGate(string name = null) : base(name) { }

        protected override void _Activate(IContext context, IMatrix primary, IMatrix secondary)
        {
            var output = primary.PointwiseMultiply(secondary);
            _AddHistory(context, output, () => new Backpropagation(this, primary,  secondary));
        }
    }
}
