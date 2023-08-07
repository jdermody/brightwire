using BrightWire.ExecutionGraph.ErrorMetric;
using System.Collections.Generic;
using System.Linq;
using BrightData;
using BrightData.LinearAlgebra;

namespace BrightWire.Unsupervised
{
    /// <summary>
    /// Non negative matrix factorisation based clustering
    /// https://en.wikipedia.org/wiki/Non-negative_matrix_factorization
    /// </summary>
    internal class NonNegativeMatrixFactorisation
    {
        readonly uint _numClusters;
        readonly LinearAlgebraProvider _lap;
        readonly IErrorMetric _costFunction;

        public NonNegativeMatrixFactorisation(LinearAlgebraProvider lap, uint numClusters, IErrorMetric? costFunction = null)
        {
            _lap = lap;
            _numClusters = numClusters;
            _costFunction = costFunction ?? new Quadratic();
        }

        public IVector[][] Cluster(IEnumerable<IVector> data, uint numIterations, float errorThreshold = 0.001f)
        {
            var dataArray = data.ToArray();

            // create the main matrix
            using var v = _lap.CreateMatrixFromRows(dataArray);

            // create the weights and features
            var context = _lap.Context;
            var weights = _lap.CreateMatrix(v.RowCount, _numClusters, (_, _) => context.NextRandomFloat());
            var features = _lap.CreateMatrix(_numClusters, v.ColumnCount, (_, _) => context.NextRandomFloat());

            // iterate
            //float lastCost = 0;
            for (int i = 0; i < numIterations; i++) {
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
                .Select((r, i) => (Index: i, r.ReadOnlySegment.GetMinAndMaxValues().MaxIndex))
                .ToList();
            weights.Dispose();
            features.Dispose();
            return documentClusters
                .GroupBy(d => d.MaxIndex)
                .Select(g => g.Select(d => dataArray[d.Index]).ToArray())
                .ToArray();
        }

        float DifferenceCost(IMatrix m1, IMatrix m2)
        {
            return m1.AllRowsAsReadOnly(false).Zip(m2.AllRowsAsReadOnly(false), (r1, r2) => _costFunction.Compute(r1.ToArray(), r2.ToArray())).Average();
        }
    }
}
