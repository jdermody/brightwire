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

            public override IEnumerable<(IGraphData Signal, INode ToNode)> Backward(IGraphData errorSignal, IGraphSequenceContext context, INode[] parents)
            {
                var es = errorSignal.GetMatrix();
                var delta1 = es.PointwiseMultiply(_input2);
                var delta2 = es.PointwiseMultiply(_input1);
                yield return (errorSignal.ReplaceWith(delta1), parents.First());
                yield return (errorSignal.ReplaceWith(delta2), parents.Last());
            }
        }
        public MultiplyGate(string? name = null) : base(name) { }

        protected override void Activate(IGraphSequenceContext context, IFloatMatrix primary, IFloatMatrix secondary)
        {
            var output = primary.PointwiseMultiply(secondary);
            AddHistory(context, output, () => new Backpropagation(this, primary,  secondary));
        }

        protected override (IFloatMatrix Next, Func<IBackpropagate>? BackProp) Activate2(IGraphSequenceContext context, IFloatMatrix primary, IFloatMatrix secondary)
        {
            var output = primary.PointwiseMultiply(secondary);
            return (output, () => new Backpropagation(this, primary,  secondary));
        }
    }
}
