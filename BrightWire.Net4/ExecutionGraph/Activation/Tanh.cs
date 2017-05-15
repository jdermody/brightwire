using BrightWire.ExecutionGraph.Helper;
using BrightWire.ExecutionGraph.Node;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrightWire.ExecutionGraph.Activation
{
    class Tanh : NodeBase
    {
        class Backpropagation : SingleBackpropagationBase<Tanh>
        {
            readonly IReadOnlyList<IMatrix> _input;

            public Backpropagation(Tanh source, IReadOnlyList<IMatrix> matrix) : base(source)
            {
                _input = matrix;
            }

            protected override void _Dispose(bool isDisposing)
            {
                foreach(var item in _input)
                    item.Dispose();
            }

            protected override IGraphData _Backpropagate(INode fromNode, IGraphData errorSignal, IContext context, IReadOnlyList<INode> parents)
            {
                return context.ToGraphData(_input.Zip(errorSignal.Decompose(), (input, es) => {
                    using (var od = input.TanhDerivative()) {
                        var delta = es.PointwiseMultiply(od);
                        //context.LearningContext.Log("tanh-backpropagation", channel, _source.GetHashCode(), errorSignal, delta);
                        return delta;
                    }
                }));
            }
        }

        public Tanh(string name = null) : base(name) { }

        public override void ExecuteForward(IContext context)
        {
            var input = context.Data.Decompose();
            var output = context.ToGraphData(input.Select(m => m.TanhActivation()));
            _AddNextGraphAction(context, output, () => new Backpropagation(this, input));
        }
    }
}
