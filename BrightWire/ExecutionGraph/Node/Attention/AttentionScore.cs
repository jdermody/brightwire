using BrightWire.ExecutionGraph.Node.Gate;
using System;
using BrightData;

namespace BrightWire.ExecutionGraph.Node.Attention
{
    internal class AttentionScore(string? name) : BinaryGateBase(name)
    {
        protected override (IGraphData Next, Func<IBackpropagate>? BackProp) Activate(IGraphContext context, IGraphData primary, IGraphData secondary, NodeBase primarySource, NodeBase secondarySource)
        {
            var query = primary.Get3DTensor()!;
            var keys = secondary.Get3DTensor()!;
            var depth = query.Depth;
            var attentionScores = new IMatrix[depth];

            for (uint i = 0; i < depth; i++) {
                using var q = query.GetMatrix(i);
                using var scoreTensor = keys.MultiplyEachMatrixBy(q);
                var scoreMatrix = scoreTensor.AddAllMatrices();
                scoreMatrix.MultiplyInPlace(1f / depth);
                attentionScores[i] = scoreMatrix;
            }

            throw new NotImplementedException();
        }
    }
}
