using BrightWire.ExecutionGraph.ErrorMetric;
using System.Collections.Generic;
using System.Linq;
using BrightData;
using BrightData.LinearAlgebra;
using BrightData.LinearAlgebra.ReadOnly;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BrightWire.Unsupervised
{
    /// <summary>
    /// Non negative matrix factorisation based clustering
    /// https://en.wikipedia.org/wiki/Non-negative_matrix_factorization
    /// </summary>
    internal class NonNegativeMatrixFactorisation : IClusteringStrategy
    {
        readonly uint                  _numIterations;
        readonly float                 _errorThreshold;
        readonly LinearAlgebraProvider _lap;
        readonly IErrorMetric          _costFunction;

        public NonNegativeMatrixFactorisation(LinearAlgebraProvider lap, uint numIterations, float errorThreshold = 0.001f, IErrorMetric? costFunction = null)
        {
            _lap            = lap;
            _numIterations  = numIterations;
            _errorThreshold = errorThreshold;
            _costFunction   = costFunction ?? new Quadratic();
        }

        
        public uint[][] Cluster(IReadOnlyVector[] data, uint numClusters, DistanceMetric metric)
        {
            // create the main matrix
            using var v = _lap.CreateMatrixFromRows(data);

            // create the weights and features
            var context = _lap.Context;
            var weights = _lap.CreateMatrix(v.RowCount, numClusters, (_, _) => context.NextRandomFloat());
            var features = _lap.CreateMatrix(numClusters, v.ColumnCount, (_, _) => context.NextRandomFloat());

            try {
                // iterate
                //float lastCost = 0;
                for (var i = 0; i < _numIterations; i++) {
                    using var wh = weights.Multiply(features);
                    var cost = DifferenceCost(v, wh);
                    //if (i % (numIterations / 10) == 0)
                    //    Console.WriteLine("NNMF cost: " + cost);
                    if (cost <= _errorThreshold)
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

        float DifferenceCost(IMatrix m1, IMatrix m2) => m1.AllRowsAsReadOnly(false)
            .Zip(m2.AllRowsAsReadOnly(false), (r1, r2) => _costFunction.Compute(r1.ToArray(), r2.ToArray()))
            .Average()
        ;
    }
}
