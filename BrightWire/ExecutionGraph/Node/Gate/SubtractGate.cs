using System;
using System.Collections.Generic;
using System.Linq;
using BrightData;

namespace BrightWire.ExecutionGraph.Node.Gate
{
    /// <summary>
    /// Subtracts the second input from the first input
    /// </summary>
    internal class SubtractGate : BinaryGateBase
    {
        class Backpropagation : BackpropagationBase<SubtractGate>
        {
            public Backpropagation(SubtractGate source) : base(source)
            {
            }

            public override IEnumerable<(IGraphData Signal, IGraphSequenceContext Context, NodeBase ToNode)> Backward(IGraphData errorSignal, IGraphSequenceContext context, NodeBase[] parents)
            {
                var es = errorSignal.GetMatrix();
                var negative = es.Clone();
                negative.Multiply(-1f);

                yield return (errorSignal, context, parents.First());
                yield return (errorSignal.ReplaceWith(negative), context, parents.Last());
            }
        }
        public SubtractGate(string? name = null) : base(name) { }

        protected override void Activate(IGraphSequenceContext context, IFloatMatrix primary, IFloatMatrix secondary)
        {
            var output = primary.Subtract(secondary);
            AddHistory(context, output, () => new Backpropagation(this));
        }

        protected override (IFloatMatrix Next, Func<IBackpropagate>? BackProp) Activate2(IGraphSequenceContext context, IFloatMatrix primary, IFloatMatrix secondary)
        {
            var output = primary.Subtract(secondary);
            return (output, () => new Backpropagation(this));
        }
    }
}
