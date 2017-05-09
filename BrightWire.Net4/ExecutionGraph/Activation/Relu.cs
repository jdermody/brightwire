using BrightWire.ExecutionGraph.Helper;
using BrightWire.ExecutionGraph.Node;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.ExecutionGraph.Activation
{
    class Relu : NodeBase
    {
        class Backpropagation : SingleBackpropagationBase
        {
            readonly IMatrix _input;
            readonly Relu _source;

            public Backpropagation(Relu source, IMatrix matrix)
            {
                _input = matrix;
                _source = source;
            }

            protected override void _Dispose(bool isDisposing)
            {
                _input.Dispose();
            }

            protected override IMatrix _Backward(IMatrix errorSignal, IContext context, IReadOnlyList<INode> parents)
            {
                using (var od = _input.ReluDerivative()) {
                    var delta = errorSignal.PointwiseMultiply(od);
                    //context.LearningContext.Log("relu-backpropagation", channel, _source.GetHashCode(), errorSignal, delta);
                    return delta;
                }
            }
        }

        public Relu(string name = null) : base(name) { }

        public override void SetPrimaryInput(IContext context)
        {
            var input = context.Data.GetAsMatrix();
            var output = input.ReluActivation();
            _AddNextGraphAction(context, new MatrixGraphData(output), () => new Backpropagation(this, input));
        }
    }
}
