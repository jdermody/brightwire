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
    class NonNegativeMatrixFactorisation
    {
        readonly int _numClusters;
        readonly ILinearAlgebraProvider _lap;
        readonly IErrorMetric _costFunction;

        public NonNegativeMatrixFactorisation(ILinearAlgebraProvider lap, int numClusters, IErrorMetric costFunction = null)
        {
            _lap = lap;
            _numClusters = numClusters;
            _costFunction = costFunction ?? new Quadratic();
        }

        public IFloatVector[][] Cluster(IFloatVector[] data, int numIterations, float errorThreshold = 0.001f)
        {
            if (data.Length == 0)
                return new IFloatVector[][] {};

            // create the main matrix
            var data2 = new List<IIndexableFloatVector>();
            foreach (var item in data)
                data2.Add(item.AsIndexable());
            using (var v = _lap.CreateMatrix((uint)data.Length, (uint)data.First().Count, (x, y) => data2[(int)x][y])) {
                data2.ForEach(d => d.Dispose());

                // create the weights and features
                var rand = new Random();
                var weights = _lap.CreateMatrix(v.RowCount, (uint)_numClusters, (x, y) => Convert.ToSingle(rand.NextDouble()));
                var features = _lap.CreateMatrix((uint)_numClusters, v.ColumnCount, (x, y) => Convert.ToSingle(rand.NextDouble()));

                // iterate
                //float lastCost = 0;
                for (int i = 0; i < numIterations; i++) {
                    using (var wh = weights.Multiply(features)) {
                        var cost = _DifferenceCost(v, wh);
                        //if (i % (numIterations / 10) == 0)
                        //    Console.WriteLine("NNMF cost: " + cost);
                        if (cost <= errorThreshold)
                            break;
                        //lastCost = cost;

                        using (var wT = weights.Transpose())
                        using (var hn = wT.Multiply(v))
                        using (var wTw = wT.Multiply(weights))
                        using (var hd = wTw.Multiply(features))
                        using (var fhn = features.PointwiseMultiply(hn)) {
                            features.Dispose();
                            features = fhn.PointwiseDivide(hd);
                        }

                        using (var fT = features.Transpose())
                        using (var wn = v.Multiply(fT))
                        using (var wf = weights.Multiply(features))
                        using (var wd = wf.Multiply(fT))
                        using (var wwn = weights.PointwiseMultiply(wn)) {
                            weights.Dispose();
                            weights = wwn.PointwiseDivide(wd);
                        }
                    }
                }

                // weights gives cluster membership
                var documentClusters = weights.AsIndexable().Rows.Select((c, i) => Tuple.Create(i, c.MaximumIndex())).ToList();
                weights.Dispose();
                features.Dispose();
                return documentClusters.GroupBy(d => d.Item2).Select(g => g.Select(d => data[d.Item1]).ToArray()).ToArray();
            }
        }

        float _DifferenceCost(IFloatMatrix m1, IFloatMatrix m2)
        {
            return m1.AsIndexable().Rows.Zip(m2.AsIndexable().Rows, (r1, r2) => _costFunction.Compute(r1.Data, r2.Data)).Average();
        }
    }
}
