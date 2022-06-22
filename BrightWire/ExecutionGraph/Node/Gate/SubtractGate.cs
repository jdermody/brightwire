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

            public override IEnumerable<(IGraphData Signal, IGraphSequenceContext Context, NodeBase? ToNode)> Backward(IGraphData errorSignal, IGraphSequenceContext context, NodeBase[] parents)
            {
                var es = errorSignal.GetMatrix();
                var negative = es.Clone();
                negative.Multiply(-1f);

                yield return (errorSignal, context, _primarySource);
                yield return (errorSignal.ReplaceWith(negative), context, _secondarySource);
            }
        }
        public SubtractGate(string? name = null) : base(name) { }

        protected override (IMatrix Next, Func<IBackpropagate>? BackProp) Activate(IGraphSequenceContext context, IMatrix primary, IMatrix secondary, NodeBase primarySource, NodeBase secondarySource)
        {
            var output = primary.Subtract(secondary);
            return (output, () => new Backpropagation(this, primarySource, secondarySource));
        }
    }
}
