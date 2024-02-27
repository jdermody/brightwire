using BrightData.LinearAlgebra.CostFunctions;
using System.Linq;

namespace BrightData.LinearAlgebra.Clustering
{
    internal class NonNegativeMatrixFactorisation(LinearAlgebraProvider lap, uint numIterations, float errorThreshold = 0.001f, ICostFunction<float>? costFunction = null) : IClusteringStrategy
    {
        readonly ICostFunction<float> _costFunction = costFunction ?? new QuadraticCostFunction(lap);

        public uint[][] Cluster(IReadOnlyVector[] vectors, uint numClusters, DistanceMetric metric)
        {
            // create the main matrix
            using var v = lap.CreateMatrixFromRows(vectors);

            // create the weights and features
            var context = lap.Context;
            var weights = lap.CreateMatrix(v.RowCount, numClusters, (_, _) => context.NextRandomFloat());
            var features = lap.CreateMatrix(numClusters, v.ColumnCount, (_, _) => context.NextRandomFloat());

            try {
                // iterate
                //float lastCost = 0;
                for (var i = 0; i < numIterations; i++) {
                    using var wh = weights.Multiply(features);
                    var cost = DifferenceCost(v, wh);
                    //if (i % (numIterations / 10) == 0)
                    //    Console.WriteLine("NNMF cost: " + cost);
                    if (cost <= errorThreshold)
                        break;
                    //lastCost = cost;

                    using var wT = weights.Transpose();
                    using var hn = wT.Multiply(v);
                    using var wTw = wT.Multiply(weights);
                    using var hd = wTw.Multiply(features);
                    using var fhn = features.PointwiseMultiply(hn);
                    features.Dispose();
                    features = fhn.PointwiseDivide(hd);

                    using var fT = features.Transpose();
                    using var wn = v.Multiply(fT);
                    using var wf = weights.Multiply(features);
                    using var wd = wf.Multiply(fT);
                    using var wwn = weights.PointwiseMultiply(wn);
                    weights.Dispose();
                    weights = wwn.PointwiseDivide(wd);
                }

                // weights gives cluster membership
                var documentClusters = weights.AllRowsAsReadOnly(false)
                    .Select((r, i) => (Index: i, MaxIndex: r.GetMaximumIndex()))
                    .ToList();
                
                return documentClusters
                        .GroupBy(d => d.MaxIndex)
                        .Select(g => g.Select(d => (uint)d.Index).ToArray())
                        .ToArray()
                    ;
            }
            finally {
                weights.Dispose();
                features.Dispose();
            }
        }

        float DifferenceCost(IReadOnlyMatrix m1, IReadOnlyMatrix m2) => _costFunction.Cost(m1.ReadOnlySegment, m2.ReadOnlySegment);
    }
}
