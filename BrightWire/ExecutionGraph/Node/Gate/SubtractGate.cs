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

            public override void BackwardInternal(INode? fromNode, IGraphData errorSignal, IGraphSequenceContext context, INode[] parents)
            {
                var es = errorSignal.GetMatrix();
                var negative = es.Clone();
                negative.Multiply(-1f);

                context.AddBackward(errorSignal, parents.First(), _source);
                context.AddBackward(errorSignal.ReplaceWith(negative), parents.Last(), _source);
            }

            public override IEnumerable<(IGraphData signal, INode toNode)> Backward(IGraphData errorSignal, IGraphSequenceContext context, INode[] parents)
            {
                var es = errorSignal.GetMatrix();
                var negative = es.Clone();
                negative.Multiply(-1f);

                yield return (errorSignal, parents.First());
                yield return (errorSignal.ReplaceWith(negative), parents.Last());
            }
        }
        public SubtractGate(string? name = null) : base(name) { }

        protected override void Activate(IGraphSequenceContext context, IFloatMatrix primary, IFloatMatrix secondary)
        {
            var output = primary.Subtract(secondary);
            AddHistory(context, output, () => new Backpropagation(this));
        }
    }
}
