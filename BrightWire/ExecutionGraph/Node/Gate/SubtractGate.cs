using System;
using System.Collections.Generic;
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
            readonly NodeBase _primarySource;
            readonly NodeBase _secondarySource;

            public Backpropagation(SubtractGate source, NodeBase primarySource, NodeBase secondarySource) : base(source)
            {
                _primarySource = primarySource;
                _secondarySource = secondarySource;
            }

            public override IEnumerable<(IGraphData Signal, IGraphContext Context, NodeBase? ToNode)> Backward(IGraphData errorSignal, IGraphContext context, NodeBase[] parents)
            {
                var es = errorSignal.GetMatrix();
                var negative = es.Clone();
                negative.MultiplyInPlace(-1f);

                yield return (errorSignal, context, _primarySource);
                yield return (errorSignal.ReplaceWith(negative), context, _secondarySource);
            }
        }
        public SubtractGate(string? name = null) : base(name) { }

        protected override (IGraphData Next, Func<IBackpropagate>? BackProp) Activate(IGraphContext context, IGraphData primary, IGraphData secondary, NodeBase primarySource, NodeBase secondarySource)
        {
            var output = primary.GetMatrix().Subtract(secondary.GetMatrix());
            return (output.AsGraphData(), () => new Backpropagation(this, primarySource, secondarySource));
        }
    }
}
