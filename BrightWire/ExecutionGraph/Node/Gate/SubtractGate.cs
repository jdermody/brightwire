using System;
using System.Collections.Generic;

namespace BrightWire.ExecutionGraph.Node.Gate
{
    /// <summary>
    /// Subtracts the second input from the first input
    /// </summary>
    internal class SubtractGate(string? name = null) : BinaryGateBase(name)
    {
        class Backpropagation(SubtractGate source, NodeBase primarySource, NodeBase secondarySource)
            : BackpropagationBase<SubtractGate>(source)
        {
            public override IEnumerable<(IGraphData Signal, IGraphContext Context, NodeBase? ToNode)> Backward(IGraphData errorSignal, IGraphContext context, NodeBase[] parents)
            {
                var es = errorSignal.GetMatrix();
                var negative = es.Clone();
                negative.MultiplyInPlace(-1f);

                yield return (errorSignal, context, primarySource);
                yield return (errorSignal.ReplaceWith(negative), context, secondarySource);
            }
        }

        protected override (IGraphData Next, Func<IBackpropagate>? BackProp) Activate(IGraphContext context, IGraphData primary, IGraphData secondary, NodeBase primarySource, NodeBase secondarySource)
        {
            var output = primary.GetMatrix().Subtract(secondary.GetMatrix());
            return (output.AsGraphData(), () => new Backpropagation(this, primarySource, secondarySource));
        }
    }
}
