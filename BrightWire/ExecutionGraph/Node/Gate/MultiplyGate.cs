using System;
using System.Collections.Generic;
using BrightData;

namespace BrightWire.ExecutionGraph.Node.Gate
{
    /// <summary>
    /// Outputs the two input signals multiplied together
    /// </summary>
    internal class MultiplyGate(string? name = null) : BinaryGateBase(name)
    {
        class Backpropagation(MultiplyGate source, IMatrix<float> primary, IMatrix<float> secondary, NodeBase primarySource, NodeBase secondarySource)
            : BackpropagationBase<MultiplyGate>(source)
        {
            protected override void DisposeMemory(bool isDisposing)
            {
                primary.Dispose();
                secondary.Dispose();
            }

            public override IEnumerable<(IGraphData Signal, IGraphContext Context, NodeBase? ToNode)> Backward(IGraphData errorSignal, IGraphContext context, NodeBase[] parents)
            {
                var es = errorSignal.GetMatrix();
                var delta1 = es.PointwiseMultiply(secondary);
                var delta2 = es.PointwiseMultiply(primary);
                yield return (errorSignal.ReplaceWith(delta1), context, primarySource);
                yield return (errorSignal.ReplaceWith(delta2), context, secondarySource);
            }
        }

        protected override (IGraphData Next, Func<IBackpropagate>? BackProp) Activate(IGraphContext context, IGraphData primary, IGraphData secondary, NodeBase primarySource, NodeBase secondarySource)
        {
            var primaryMatrix = primary.GetMatrix();
            var secondaryMatrix = secondary.GetMatrix();
            var output = primaryMatrix.PointwiseMultiply(secondaryMatrix);

            return (output.AsGraphData(), () => new Backpropagation(this, primaryMatrix,  secondaryMatrix, primarySource, secondarySource));
        }
    }
}
