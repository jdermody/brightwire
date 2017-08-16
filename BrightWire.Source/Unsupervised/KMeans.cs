using MathNet.Numerics.Distributions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BrightWire.Unsupervised
{
    /// <summary>
    /// K Means clustering
    /// https://en.wikipedia.org/wiki/K-means_clustering
    /// </summary>
    internal class KMeans : IDisposable
    {
        class ClusterData : IDisposable
        {
            public class Centroid : IDisposable
            {
                readonly List<IVector> _data = new List<IVector>();
                public IVector Current { get; private set; }
                public IReadOnlyList<IVector> Data { get { return _data; } }

                public Centroid(IVector data)
                {
                    _data.Add(data);
                    Current = data.Clone();
                }
                public void Dispose()
                {
                    Current.Dispose();
                }
                public bool Update(IReadOnlyList<IVector> data)
                {
                    // check if the cluster has been updated
                    if (data.Count == _data.Count) {
                        var isChanged = false;
                        var existing = new HashSet<IVector>(_data);
                        foreach (var item in data) {
                            if (!existing.Contains(item)) {
                                isChanged = true;
                                break;
                            }
                        }
                        if (!isChanged)
                            return false;
                    }

                    Current.Dispose();
                    Current = null;
                    _data.Clear();
                    var denominator = 1f / data.Count;

                    var isFirst = true;
                    foreach (var item in data) {
                        if (Current == null)
                            Current = item.Clone();
                        else {
                            Current.AddInPlace(item, isFirst ? denominator : 1, denominator);
                            isFirst = false;
                        }
                        _data.Add(item);
                    }
                    return true;
                }
            }
            readonly List<Centroid> _centroid = new List<Centroid>();
            float[] _clusterNorm = null;
            IVector[] _curr = null;

            public void Add(IVector item)
            {
                _centroid.Add(new Centroid(item));
                _clusterNorm = null;
                _curr = null;
            }
            public void Dispose()
            {
                foreach (var item in _centroid)
                    item.Dispose();
            }
            public IVector CalculateDistance(IVector vector, DistanceMetric distanceMetric)
            {
                if (_curr == null)
                    _curr = _centroid.Select(c => c.Current).ToArray();

                if (distanceMetric == DistanceMetric.Cosine)
                    return vector.CosineDistance(_curr, ref _clusterNorm);
                else
                    return vector.FindDistances(_curr, distanceMetric);
            }
            public bool Update(IReadOnlyList<IReadOnlyList<IVector>> clusterAssignment)
            {
                var ret = false;
                for (var i = 0; i < clusterAssignment.Count; i++) {
                    if (_centroid[i].Update(clusterAssignment[i]))
                        ret = true;
                }
                if (ret) {
                    _clusterNorm = null;
                    _curr = null;
                }
                return ret;
            }
            public IReadOnlyList<IReadOnlyList<IVector>> GetClusters()
            {
                return _centroid.Select(c => c.Data).ToList();
            }
            public IReadOnlyList<Centroid> Centroids { get { return _centroid; } }
        }

        readonly int _k;
        readonly ClusterData _cluster;
        readonly DistanceMetric _distanceMetric;
        readonly IReadOnlyList<IVector> _data;

        public KMeans(int k, IReadOnlyList<IVector> data, DistanceMetric distanceMetric = DistanceMetric.Euclidean, int? randomSeed = null)
        {
            _k = k;
            _distanceMetric = distanceMetric;
            _cluster = new ClusterData();
            _data = data;

            // use kmeans++ to find best initial positions
            // https://normaldeviate.wordpress.com/2012/09/30/the-remarkable-k-means/
            var rand = randomSeed.HasValue ? new Random(randomSeed.Value) : new Random();
            var data2 = data.ToList();

            // pick the first at random
            var firstIndex = rand.Next(0, data2.Count);
            _cluster.Add(data2[firstIndex]);
            data2.RemoveAt(firstIndex);

            // create a categorical distribution for each subsequent pick
            for (var i = 1; i < _k && data2.Count > 0; i++) {
                var probabilityList = new List<double>();
                foreach(var item in data2) {
                    using (var distance = _cluster.CalculateDistance(item, _distanceMetric)) {
                        var minIndex = distance.MinimumIndex();
                        probabilityList.Add(distance.AsIndexable()[minIndex]);
                    }
                }
                var distribution = new Categorical(probabilityList.ToArray());
                var nextIndex = distribution.Sample();
                _cluster.Add(data2[nextIndex]);
                data2.RemoveAt(nextIndex);
            }
        }

        void IDisposable.Dispose()
        {
            _cluster.Dispose();
        }

        public bool Cluster()
        {
            var clusterAssignment = Enumerable.Range(0, _k).Select(i => new List<IVector>()).ToArray();

            foreach(var item in _data) {
                using(var distance = _cluster.CalculateDistance(item, _distanceMetric)) {
                    clusterAssignment[distance.MinimumIndex()].Add(item);
                }
            }

            return _cluster.Update(clusterAssignment);
        }

        public void ClusterUntilConverged(int maxIterations = 1000)
        {
            for (var i = 0; i < maxIterations; i++) {
                if (!Cluster())
                    break;
            }
        }

        public IReadOnlyList<IReadOnlyList<IVector>> Clusters
        {
            get
            {
                return _cluster.GetClusters();
            }
        }
    }
}
