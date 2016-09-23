using BrightWire.Unsupervised.Clustering.Helper;
using MathNet.Numerics.Distributions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.Unsupervised.Clustering
{
    public class KMeans : IDisposable
    {
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

        public void Dispose()
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
