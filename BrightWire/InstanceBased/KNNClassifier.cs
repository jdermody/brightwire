using System;
using System.Collections.Generic;
using System.Linq;
using BrightData;
using BrightWire.Models.InstanceBased;

namespace BrightWire.InstanceBased
{
    /// <summary>
    /// K Nearest Neighbour classifier
    /// </summary>
    internal class KnnClassifier : IRowClassifier
    {
        readonly KNearestNeighbours _model;
        readonly ILinearAlgebraProvider _lap;
        readonly IFloatVector[] _instance;
        readonly DistanceMetric _distanceMetric;
        readonly uint _k;

        public KnnClassifier(ILinearAlgebraProvider lap, KNearestNeighbours model, uint k, DistanceMetric distanceMetric = DistanceMetric.Euclidean)
        {
            _k = k;
            _lap = lap;
            _model = model;
            _distanceMetric = distanceMetric;

            _instance = new IFloatVector[model.Instance.Length];
            for (int i = 0, len = model.Instance.Length; i < len; i++)
                _instance[i] = lap.CreateVector(model.Instance[i].Segment);
        }

        IEnumerable<Tuple<string, float>> ClassifyInternal(IConvertibleRow row)
        {
            // encode the features into a vector
            var featureCount = _model.DataColumns.Length;
            var features = new float[featureCount];
            for (var i = 0; i < featureCount; i++)
                features[i] = row.GetTyped<float>(_model.DataColumns[i]);

            // TODO: categorical features?

            // find the k closest neighbours and score the results based on proximity to rank the classifications
            using var vector = _lap.CreateVector(features);
            var distances = vector.FindDistances(_instance, _distanceMetric).AsIndexable();
            return distances.Values
                    .Zip(_model.Classification, (s, l) => Tuple.Create(l, s))
                    .OrderBy(d => d.Item2)
                    .Take((int)_k)
                    .GroupBy(d => d.Item1)
                    .Select(g => Tuple.Create(g.Key, g.Sum(d => 1f / d.Item2)))
                ;
        }

        public (string Label, float Weight)[] Classify(IConvertibleRow row)
        {
            return ClassifyInternal(row)
                .Select(d => (d.Item1, d.Item2))
                .ToArray()
            ;
        }
    }
}
