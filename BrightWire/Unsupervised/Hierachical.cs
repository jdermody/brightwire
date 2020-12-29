using System;
using System.Collections.Generic;
using System.Linq;
using BrightData;

namespace BrightWire.Unsupervised
{
    /// <summary>
    /// Hierachical clustering
    /// https://en.wikipedia.org/wiki/Hierarchical_clustering
    /// </summary>
    class Hierachical : IDisposable
    {
        class Centroid : IDisposable
        {
            readonly Centroid _left = null, _right = null;

            public IFloatVector Center { get; }
            public IFloatVector[] Data { get; }

            public Centroid(IFloatVector data)
            {
                Data = new[] { data };
                Center = data.Clone();
            }
            public Centroid(Centroid left, Centroid right)
            {
                _left = left;
                _right = right;
                Data = left.Data.Concat(right.Data).ToArray();

                // average the two centroid vectors
                Center = left.Center;
                Center.AddInPlace(right.Center, 0.5f, 0.5f);
            }
            public void Dispose()
            {
                Center.Dispose();
            }

            public IEnumerable<Centroid> Children
            {
                get {
                    if (_left != null)
                        yield return _left;
                    if (_right != null)
                        yield return _right;
                }
            }
        }

        class DistanceMatrix
        {
            readonly Dictionary<(Centroid, Centroid), float> _distance = new Dictionary<(Centroid, Centroid), float>();

            public bool HasDistance(Centroid item1, Centroid item2)
            {
                return _distance.ContainsKey((item1, item2)) || _distance.ContainsKey((item2, item1));
            }
            public void Remove(Centroid centroid)
            {
                foreach (var item in _distance.ToList()) {
                    if (item.Key.Item1 == centroid || item.Key.Item2 == centroid)
                        _distance.Remove(item.Key);
                }
            }
            public void Add(Centroid item1, Centroid item2, float distance)
            {
                _distance.Add((item1, item2), distance);
            }
            public KeyValuePair<(Centroid, Centroid), float> GetNextMerge()
            {
                return _distance.OrderBy(d => d.Value).FirstOrDefault();
            }
        }

        readonly uint _k;
        readonly DistanceMetric _distanceMetric;
        readonly DistanceMatrix _distanceMatrix = new DistanceMatrix();
        readonly List<Centroid> _centroid;

        public Hierachical(uint k, IEnumerable<IFloatVector> data, DistanceMetric distanceMetric = DistanceMetric.Euclidean)
        {
            _k = k;
            _distanceMetric = distanceMetric;
            _centroid = data.Select(v => new Centroid(v)).ToList();
        }

        public void Dispose()
        {
            _centroid.ForEach(c => c.Dispose());
        }

        public void Cluster()
        {
            // build the distance matrix
            foreach (var item1 in _centroid) {
                foreach (var item2 in _centroid.Where(c => c != item1)) {
                    if (!_distanceMatrix.HasDistance(item1, item2)) {
                        var distance = item1.Center.FindDistance(item2.Center, _distanceMetric);
                        _distanceMatrix.Add(item1, item2, distance);
                    }
                }
            }

            // agglomerative cluster
            while (_centroid.Count > _k) {
                var merge = _distanceMatrix.GetNextMerge();
                var target = merge.Key.Item1;
                var source = merge.Key.Item2;

                _distanceMatrix.Remove(target);
                _distanceMatrix.Remove(source);
                _centroid.Remove(target);
                _centroid.Remove(source);
                var combined = new Centroid(target, source);

                // find the distances between the new centroid and the remaining centroids
                foreach (var item in _centroid) {
                    if (!_distanceMatrix.HasDistance(combined, item)) {
                        var distance = combined.Center.FindDistance(item.Center, _distanceMetric);
                        _distanceMatrix.Add(combined, item, distance);
                    }
                }
                _centroid.Add(combined);
            }
        }

        public IFloatVector[][] Clusters
        {
            get {
                return _centroid.Select(c => c.Data).ToArray();
            }
        }
    }
}
