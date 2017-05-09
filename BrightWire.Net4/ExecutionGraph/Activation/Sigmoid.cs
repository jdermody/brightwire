using BrightWire.ExecutionGraph.Helper;
using BrightWire.ExecutionGraph.Node;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.ExecutionGraph.Activation
{
    class Sigmoid : NodeBase
    {
        class Backpropagation : SingleBackpropagationBase
        {
            readonly IMatrix _input;
            readonly Sigmoid _source;

            public Backpropagation(Sigmoid source, IMatrix matrix)
            {
                _source = source;
                _input = matrix;
            }

            protected override void _Dispose(bool isDisposing)
            {
                _input.Dispose();
            }

            protected override IMatrix _Backward(IMatrix errorSignal, IContext context, IReadOnlyList<INode> parents)
            {
                using (var od = _input.SigmoidDerivative()) {
                    var delta = errorSignal.PointwiseMultiply(od);
                    //context.LearningContext.Log("sigmoid-backpropagation", channel, _source.GetHashCode(), errorSignal, delta);
                    return delta;
                }
            }
        }

        public Sigmoid(string name = null) : base(name) { }

        public override void SetPrimaryInput(IContext context)
        {
            var input = context.Data.GetAsMatrix();
            var output = input.SigmoidActivation();
            _AddNextGraphAction(context, new MatrixGraphData(output), () => new Backpropagation(this, input));
        }
    }
}
