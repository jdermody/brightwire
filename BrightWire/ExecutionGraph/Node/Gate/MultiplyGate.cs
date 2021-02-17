using System;
using System.Collections.Generic;
using System.Linq;
using BrightData;

namespace BrightWire.ExecutionGraph.Node.Gate
{
    /// <summary>
    /// Outputs the two input signals multiplied together
    /// </summary>
    internal class MultiplyGate : BinaryGateBase
    {
        class Backpropagation : BackpropagationBase<MultiplyGate>
        {
            readonly IFloatMatrix _input1, _input2;

            public Backpropagation(MultiplyGate source, IFloatMatrix input1, IFloatMatrix input2) : base(source)
            {
                _input1 = input1;
                _input2 = input2;
            }

            protected override void DisposeMemory(bool isDisposing)
            {
                //_input1.Dispose();
                //_input2.Dispose();
            }

            public override IEnumerable<(IGraphData Signal, IGraphSequenceContext Context, NodeBase ToNode)> Backward(IGraphData errorSignal, IGraphSequenceContext context, NodeBase[] parents)
            {
                var es = errorSignal.GetMatrix();
                var delta1 = es.PointwiseMultiply(_input2);
                var delta2 = es.PointwiseMultiply(_input1);
                yield return (errorSignal.ReplaceWith(delta1), context, parents.First());
                yield return (errorSignal.ReplaceWith(delta2), context, parents.Last());
            }
        }
        public MultiplyGate(string? name = null) : base(name) { }

        protected override (IFloatMatrix Next, Func<IBackpropagate>? BackProp) Activate(IGraphSequenceContext context, IFloatMatrix primary, IFloatMatrix secondary)
        {
            var output = primary.PointwiseMultiply(secondary);
            return (output, () => new Backpropagation(this, primary,  secondary));
        }
    }
}
