using BrightData.LinearAlgebra.CostFunctions;
using System.Linq;

namespace BrightData.LinearAlgebra.Clustering
{
    /// <summary>
    /// Non-negative matrix factorisation based clustering
    /// https://en.wikipedia.org/wiki/Non-negative_matrix_factorization
    /// </summary>
    internal class NonNegativeMatrixFactorisation(LinearAlgebraProvider<float> lap, uint numIterations, float errorThreshold = 0.001f, ICostFunction<float>? costFunction = null) : IClusteringStrategy
    {
        readonly ICostFunction<float> _costFunction = costFunction ?? new MeanSquaredErrorCostFunction<float>(lap);

        public uint[][] Cluster(IReadOnlyVector<float>[] vectors, uint numClusters, DistanceMetric metric)
        {
            // create the main matrix
            using var v = lap.CreateMatrixFromRows(vectors);

            // create the weights and features
            var context = lap.Context;
            var weights = lap.CreateMatrix(v.RowCount, numClusters, (_, _) => context.NextRandomFloat());
            var features = lap.CreateMatrix(numClusters, v.ColumnCount, (_, _) => context.NextRandomFloat());

            try {
                // iterate
                var lastCost = float.MaxValue;
                for (var i = 0; i < numIterations; i++) {
                    using var wh = weights.Multiply(features);
                    var cost = DifferenceCost(v, wh);
                    if (cost <= errorThreshold || cost > lastCost || (lastCost - cost) < errorThreshold)
                        break;
                    lastCost = cost;

                    using var wT = weights.Transpose();
                    using var wTv = wT.Multiply(v);

                    using var wTw = wT.Multiply(weights);
                    using var wTwf = wTw.Multiply(features);
                    using var fwTv = features.PointwiseMultiply(wTv);
                    features.Dispose();
                    features = fwTv.PointwiseDivide(wTwf);
                    features.ConstrainInPlace(null, null);

                    using var fT = features.Transpose();
                    using var vfT = v.Multiply(fT);

                    using var wf = weights.Multiply(features);
                    using var wffT = wf.Multiply(fT);
                    using var wwn = weights.PointwiseMultiply(vfT);
                    weights.Dispose();
                    weights = wwn.PointwiseDivide(wffT);
                    weights.ConstrainInPlace(null, null);
                }

                // weights gives cluster membership
                var documentClusters = weights.AllRowsAsReadOnly(false)
                    .Select((r, i) => (Index: i, r.GetMinAndMaxValues().MaxIndex))
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

        float DifferenceCost(IHaveReadOnlyTensorSegment<float> m1, IHaveReadOnlyTensorSegment<float> m2) => _costFunction.Cost(m1.ReadOnlySegment, m2.ReadOnlySegment);
    }
}
