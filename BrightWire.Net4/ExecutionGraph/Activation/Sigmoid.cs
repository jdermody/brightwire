using BrightWire.ExecutionGraph.Helper;
using BrightWire.ExecutionGraph.Node;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrightWire.ExecutionGraph.Activation
{
    class Sigmoid : NodeBase
    {
        class Backpropagation : SingleBackpropagationBase<Sigmoid>
        {
            readonly IReadOnlyList<IMatrix> _input;

            public Backpropagation(Sigmoid source, IReadOnlyList<IMatrix> matrix) : base(source)
            {
                _input = matrix;
            }

            protected override void _Dispose(bool isDisposing)
            {
                //foreach(var item in _input)
                //    item.Dispose();
            }

            protected override IGraphData _Backpropagate(INode fromNode, IGraphData errorSignal, IContext context, IReadOnlyList<INode> parents)
            {
                return _input.Zip(errorSignal.Decompose(), (i, e) => {
                    using (var od = i.SigmoidDerivative()) {
                        var delta = e.PointwiseMultiply(od);
                        //context.LearningContext.Log("sigmoid-backpropagation", channel, _source.GetHashCode(), errorSignal, delta);
                        return delta;
                    }
                }).ToList().ToGraphData(context.LinearAlgebraProvider);
            }
        }

        public Sigmoid(string name = null) : base(name) { }

        public override void ExecuteForward(IContext context)
        {
            var input = context.Data.Decompose();
            var output = context.ToGraphData(input.Select(m => m.SigmoidActivation()));
            _AddNextGraphAction(context, output, () => new Backpropagation(this, input));
        }
    }
}
