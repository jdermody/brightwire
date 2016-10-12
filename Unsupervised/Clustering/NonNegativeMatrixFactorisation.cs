using BrightWire.ErrorMetrics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.Unsupervised.Clustering
{
    internal class NonNegativeMatrixFactorisation
    {
        readonly int _numClusters;
        readonly ILinearAlgebraProvider _lap;
        readonly IErrorMetric _costFunction;

        public NonNegativeMatrixFactorisation(ILinearAlgebraProvider lap, int numClusters, IErrorMetric costFunction = null)
        {
            _lap = lap;
            _numClusters = numClusters;
            _costFunction = costFunction ?? new RMSE();
        }

        public IReadOnlyList<IReadOnlyList<IVector>> Cluster(IReadOnlyList<IVector> data, int numIterations, float errorThreshold = 0.001f)
        {
            if (data.Count == 0)
                return new List<IVector[]>();

            // create the main matrix
            var data2 = new List<IIndexableVector>();
            foreach (var item in data)
                data2.Add(item.AsIndexable());
            var v = _lap.Create(data.Count, data.First().Count, (x, y) => data2[x][y]);
            data2.ForEach(d => d.Dispose());

            // create the weights and features
            var rand = new Random();
            var weights = _lap.Create(v.RowCount, _numClusters, (x, y) => Convert.ToSingle(rand.NextDouble()));
            var features = _lap.Create(_numClusters, v.ColumnCount, (x, y) => Convert.ToSingle(rand.NextDouble()));

            // iterate
            float lastCost = 0;
            for (int i = 0; i < numIterations; i++) {
                var wh = weights.Multiply(features);
                var cost = _DifferenceCost(v, wh);
                if (cost <= errorThreshold)
                    break;
                lastCost = cost;

                using (var wT = weights.Transpose())
                using (var hn = wT.Multiply(v))
                using (var wTw = wT.Multiply(weights))
                using (var hd = wTw.Multiply(features))
                using (var fhn = features.PointwiseMultiply(hn)) {
                    using (var f = features)
                        features = fhn.PointwiseDivide(hd);
                }

                using (var fT = features.Transpose())
                using (var wn = v.Multiply(fT))
                using (var wf = weights.Multiply(features))
                using (var wd = wf.Multiply(fT))
                using (var wwn = weights.PointwiseMultiply(wn)) {
                    using (var w = weights)
                        weights = wwn.PointwiseDivide(wd);
                }
            }

            // weights gives cluster membership
            var documentClusters = weights.AsIndexable().Rows.Select((c, i) => Tuple.Create(i, c.MaximumIndex())).ToList();
            return documentClusters.GroupBy(d => d.Item2).Select(g => g.Select(d => data[d.Item1]).ToArray()).ToList();
        }

        float _DifferenceCost(IMatrix m1, IMatrix m2)
        {
            return m1.AsIndexable().Rows.Zip(m2.AsIndexable().Rows, (r1, r2) => _costFunction.Compute(r1, r2)).Average();
        }
    }
}
