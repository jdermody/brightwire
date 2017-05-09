using BrightWire.ExecutionGraph.Helper;
using BrightWire.ExecutionGraph.Node;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.ExecutionGraph.Activation
{
    class Tanh : NodeBase
    {
        class Backpropagation : SingleBackpropagationBase
        {
            readonly IMatrix _input;
            readonly Tanh _source;

            public Backpropagation(Tanh source, IMatrix matrix)
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
                using (var od = _input.TanhDerivative()) {
                    var delta = errorSignal.PointwiseMultiply(od);
                    //context.LearningContext.Log("tanh-backpropagation", channel, _source.GetHashCode(), errorSignal, delta);
                    return delta;
                }
            }
        }

        public Tanh(string name = null) : base(name) { }

        public override void ExecuteForward(IContext context)
        {
            var input = context.Data.GetAsMatrix();
            var output = input.TanhActivation();
            _AddNextGraphAction(context, new MatrixGraphData(output), () => new Backpropagation(this, input));
        }
    }
}
