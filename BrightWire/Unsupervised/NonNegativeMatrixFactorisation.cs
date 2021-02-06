using BrightWire.ExecutionGraph.ErrorMetric;
using System;
using System.Collections.Generic;
using System.Linq;
using BrightData;

namespace BrightWire.Unsupervised
{
    /// <summary>
    /// Non negative matrix factorisation based clustering
    /// https://en.wikipedia.org/wiki/Non-negative_matrix_factorization
    /// </summary>
    internal class NonNegativeMatrixFactorisation
    {
        readonly uint _numClusters;
        readonly ILinearAlgebraProvider _lap;
        readonly IErrorMetric _costFunction;

        public NonNegativeMatrixFactorisation(ILinearAlgebraProvider lap, uint numClusters, IErrorMetric? costFunction = null)
        {
            _lap = lap;
            _numClusters = numClusters;
            _costFunction = costFunction ?? new Quadratic();
        }

        public IFloatVector[][] Cluster(IEnumerable<IFloatVector> data, uint numIterations, float errorThreshold = 0.001f)
        {
            var dataArray = data.ToArray();

            // create the main matrix
            using var v = _lap.CreateMatrixFromRows(dataArray);

            // create the weights and features
            var context = _lap.Context;
            var weights = _lap.CreateMatrix(v.RowCount, (uint)_numClusters, (x, y) => context.NextRandomFloat());
            var features = _lap.CreateMatrix((uint)_numClusters, v.ColumnCount, (x, y) => context.NextRandomFloat());

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
            var documentClusters = weights.AsIndexable().Rows
                .Select((c, i) => Tuple.Create(i, c.MaximumIndex()))
                .ToList();
            weights.Dispose();
            features.Dispose();
            return documentClusters
                .GroupBy(d => d.Item2)
                .Select(g => g.Select(d => dataArray[d.Item1]).ToArray())
                .ToArray();
        }

        float DifferenceCost(IFloatMatrix m1, IFloatMatrix m2)
        {
            return m1.AsIndexable().Rows.Zip(m2.AsIndexable().Rows, (r1, r2) => _costFunction.Compute(r1.Data, r2.Data)).Average();
        }
    }
}
