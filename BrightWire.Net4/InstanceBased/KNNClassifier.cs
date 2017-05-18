using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using BrightWire.Models.InstanceBased;

namespace BrightWire.InstanceBased
{
    /// <summary>
    /// K Nearest Neighbour classifier
    /// </summary>
    internal class KNNClassifier : IRowClassifier
    {
        readonly KNearestNeighbours _model;
        readonly ILinearAlgebraProvider _lap;
        readonly List<IVector> _instance = new List<IVector>();
        readonly DistanceMetric _distanceMetric;
        readonly int _k;

        public KNNClassifier(ILinearAlgebraProvider lap, KNearestNeighbours model, int k, DistanceMetric distanceMetric = DistanceMetric.Euclidean)
        {
            _k = k;
            _lap = lap;
            _model = model;
            _distanceMetric = distanceMetric;

            for (int i = 0, len = model.Instance.Length; i < len; i++)
                _instance.Add(lap.CreateVector(model.Instance[i].Data));
        }

        IEnumerable<Tuple<string, float>> _Classify(IRow row)
        {
            // encode the features into a vector
            var featureCount = _model.FeatureColumn.Length;
            var features = new float[featureCount];
            for (var i = 0; i < featureCount; i++)
                features[i] = row.GetField<float>(_model.FeatureColumn[i]);

            // find the k closest neighbours and score the results based on proximity to rank the classifications
            using (var vector = _lap.CreateVector(features)) {
                var distances = vector.FindDistances(_instance, _distanceMetric).AsIndexable();
                return distances.Values
                    .Zip(_model.Classification, (s, l) => Tuple.Create(l, s))
                    .OrderBy(d => d.Item2)
                    .Take(_k)
                    .GroupBy(d => d.Item1)
                    .Select(g => Tuple.Create(g.Key, g.Sum(d => 1f / d.Item2)))
                ;
            }
        }

        //public IEnumerable<string> Classify(IRow row)
        //{
        //    return _Classify(row)
        //        .OrderByDescending(d => d.Item2)
        //        .Select(d => d.Item1)
        //    ;
        //}

        public IReadOnlyList<(string Classification, float Weight)> Classify(IRow row)
        {
            return _Classify(row)
                .Select(d => (d.Item1, d.Item2))
                .ToList()
            ;
        }
    }
}
