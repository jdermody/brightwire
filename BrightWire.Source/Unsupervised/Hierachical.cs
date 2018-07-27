using System;
using System.Collections.Generic;
using System.Linq;

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
            readonly List<IVector> _data = new List<IVector>();
            readonly Centroid _left = null, _right = null;

            public IVector Center { get; }
            public IReadOnlyList<IVector> Data => _data;

	        public Centroid(IVector data)
            {
                _data.Add(data);
                Center = data.Clone();
            }
            public Centroid(Centroid left, Centroid right)
            {
                _left = left;
                _right = right;
                _data.AddRange(left._data);
                _data.AddRange(right._data);

                var isFirst = true;
                var denominator = 1f / _data.Count;
                foreach (var item in _data) {
                    if (Center == null)
                        Center = item.Clone();
                    else {
                        Center.AddInPlace(item, isFirst ? denominator : 1, denominator);
                        isFirst = false;
                    }
                }
            }
            public void Dispose()
            {
                Center.Dispose();
            }

            public IEnumerable<Centroid> Children
            {
                get
                {
                    if(_left != null)
                        yield return _left;
                    if(_right != null)
                        yield return _right;
                }
            }
        }

        class DistanceMatrix
        {
            readonly Dictionary<Tuple<Centroid, Centroid>, float> _distance = new Dictionary<Tuple<Centroid, Centroid>, float>();

            public bool HasDistance(Centroid item1, Centroid item2)
            {
                return _distance.ContainsKey(Tuple.Create(item1, item2)) || _distance.ContainsKey(Tuple.Create(item2, item1));
            }
            public void Remove(Centroid centroid)
            {
                foreach(var item in _distance.ToList()) {
                    if (item.Key.Item1 == centroid || item.Key.Item2 == centroid)
                        _distance.Remove(item.Key);
                }
            }
            public void Add(Centroid item1, Centroid item2, float distance)
            {
                _distance.Add(Tuple.Create(item1, item2), distance);
            }
            public KeyValuePair<Tuple<Centroid, Centroid>, float> GetNextMerge()
            {
                return _distance.OrderBy(d => d.Value).FirstOrDefault();
            }
        }

        readonly int _k;
        readonly DistanceMetric _distanceMetric;
        readonly DistanceMatrix _distanceMatrix = new DistanceMatrix();
        readonly List<Centroid> _centroid;

        public Hierachical(int k, IReadOnlyList<IVector> data, DistanceMetric distanceMetric = DistanceMetric.Euclidean)
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
            foreach(var item1 in _centroid) {
                foreach(var item2 in _centroid.Where(c => c != item1)) {
                    if(!_distanceMatrix.HasDistance(item1, item2)) {
                        var distance = item1.Center.FindDistance(item2.Center, _distanceMetric);
                        _distanceMatrix.Add(item1, item2, distance);
                    }
                }
            }

            // agglomerative cluster
            while(_centroid.Count > _k) {
                var merge = _distanceMatrix.GetNextMerge();
                var target = merge.Key.Item1;
                var source = merge.Key.Item2;

                _distanceMatrix.Remove(target);
                _distanceMatrix.Remove(source);
                _centroid.Remove(target);
                _centroid.Remove(source);
                var combined = new Centroid(target, source);

                // find the distances between the new centroid and the remaining centroids
                foreach (var item2 in _centroid) {
                    if (!_distanceMatrix.HasDistance(combined, item2)) {
                        var distance = combined.Center.FindDistance(item2.Center, _distanceMetric);
                        _distanceMatrix.Add(combined, item2, distance);
                    }
                }
                _centroid.Add(combined);
            }
        }

        void _Visit(int depth, Centroid centroid, Dictionary<int, List<Centroid>> depthTable)
        {
            if (!depthTable.TryGetValue(depth, out List<Centroid> list))
                depthTable.Add(depth, list = new List<Centroid>());
            list.Add(centroid);
            foreach (var child in centroid.Children)
                _Visit(depth + 1, child, depthTable);
        }

        public IReadOnlyList<IReadOnlyList<IVector>> Clusters
        {
            get
            {
                return _centroid.Select(c => c.Data).ToList();
            }
        }
    }
}
