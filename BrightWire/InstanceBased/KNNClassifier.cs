using System;
using System.Collections.Generic;
using System.Linq;
using BrightData;
using BrightData.DataTable2;
using BrightData.LinearAlegbra2;
using BrightWire.Models.InstanceBased;

namespace BrightWire.InstanceBased
{
    /// <summary>
    /// K Nearest Neighbour classifier
    /// </summary>
    internal class KnnClassifier : IRowClassifier
    {
        readonly KNearestNeighbours _model;
        readonly LinearAlgebraProvider _lap;
        readonly IVector[] _instance;
        readonly DistanceMetric _distanceMetric;
        readonly uint _k;

        public KnnClassifier(LinearAlgebraProvider lap, KNearestNeighbours model, uint k, DistanceMetric distanceMetric = DistanceMetric.Euclidean)
        {
            _k = k;
            _lap = lap;
            _model = model;
            _distanceMetric = distanceMetric;

            _instance = new IVector[model.Instance.Length];
            for (int i = 0, len = model.Instance.Length; i < len; i++)
                _instance[i] = lap.CreateVector(model.Instance[i]);
        }

        IEnumerable<(string Label, float Score)> ClassifyInternal(BrightDataTableRow row)
        {
            // encode the features into a vector
            var featureCount = _model.DataColumns.Length;
            var features = new float[featureCount];
            for (var i = 0; i < featureCount; i++)
                features[i] = row.Get<float>(_model.DataColumns[i]);

            // TODO: categorical features?

            // find the k closest neighbours and score the results based on proximity to rank the classifications
            using var vector = _lap.CreateVector(features);
            var distances = vector.FindDistances(_instance, _distanceMetric);
            return distances.Segment.Values
                .Zip(_model.Classification, (s, l) => (Label: l, Score:s))
                .OrderBy(d => d.Score)
                .Take((int)_k)
                .GroupBy(d => d.Label)
                .Select(g => (g.Key, g.Sum(d => 1f / d.Score)))
            ;
        }

        public (string Label, float Weight)[] Classify(BrightDataTableRow row)
        {
            return ClassifyInternal(row)
                .Select(d => (d.Label, d.Score))
                .ToArray()
            ;
        }
    }
}
