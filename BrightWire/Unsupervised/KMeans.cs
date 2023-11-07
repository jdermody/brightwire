using System;
using System.Collections.Generic;
using System.Linq;
using BrightData;
using BrightWire.Helper;

namespace BrightWire.Unsupervised
{
	/// <summary>
	/// K Means clustering
	/// https://en.wikipedia.org/wiki/K-means_clustering
	/// </summary>
    internal class KMeans : IDisposable
    {
        record ClusterVector(uint[] DataIndices, IVector Vector);

		readonly VectorDistanceHelper _distance;
		List<ClusterVector> _clusters = new();
        readonly IReadOnlyVector[] _originalData;

		public KMeans(BrightDataContext context, uint k, IEnumerable<IReadOnlyVector> data, DistanceMetric distanceMetric = DistanceMetric.Euclidean)
        {
            var lap = context.LinearAlgebraProvider;
            _originalData = data.ToArray();
			var vectors = _originalData.Select(x => x.Create(lap)).ToArray();
			_distance = new VectorDistanceHelper(vectors, distanceMetric);

			// use kmeans++ to find best initial positions
			// https://normaldeviate.wordpress.com/2012/09/30/the-remarkable-k-means/
            var clusterIndexSet = new HashSet<uint>();
			var distanceTable = new List<float>[vectors.Length];

            // pick the first cluster at random and set up the distance table
            AddCluster(context.RandomIndex(vectors.Length), (index, distance) => distanceTable[index] = new List<float>{ distance });

			for (uint i = 1; i < k && i < vectors.Length; i++) {
				// create a categorical distribution to calculate the probability of choosing each subsequent item
				var distribution = context.CreateCategoricalDistribution(distanceTable.Select(l => l.Min()).ToArray());

				// sample the next index and add the distances to the table
				uint nextIndex;
				do {
					nextIndex = distribution.Sample();
				} while (!AddCluster(nextIndex, (index, distance) => distanceTable[index].Add(distance)));
			}

            return;

            bool AddCluster(uint index, Action<uint, float> callback)
            {
                if (!clusterIndexSet.Add(index))
                    return false;

                var vector = vectors[index].Clone();
                _distance.AddComparison(vector);
                _clusters.Add(new(new[] { index }, vector));

                // calculate distances
                for (uint i = 0; i < vectors.Length; i++) {
                    float distance = 0;
                    if (i != index)
                        distance = vectors[i].FindDistance(vector, distanceMetric);
                    callback(i, distance);
                }

                return true;
            }
        }

		void IDisposable.Dispose()
		{
			((IDisposable)_distance).Dispose();
		}

		public bool Cluster()
		{
			// cluster the data
			var closest = _distance.GetClosest();
			var clusters = closest
				.Select((ci, i) => (ClusterIndex: ci, Index: (uint)i))
				.GroupBy(d => d.ClusterIndex)
				.Select(c => c.Select(d => d.Index).ToArray())
				.ToList();

			var differenceCount = 0;
			var newClusters = new List<ClusterVector>();
			for (var i = 0; i < _clusters.Count; i++) {
				var oldIndices = _clusters[i].DataIndices;
				if (i < clusters.Count) {
					var newIndices = clusters[i];
					if (oldIndices.Length == newIndices.Length && new HashSet<uint>(oldIndices).SetEquals(newIndices))
						newClusters.Add(_clusters[i]);
					else {
						differenceCount++;
						newClusters.Add(new(newIndices, _distance.GetAverageFromData(newIndices)));
					}
				}
			}

			// check if the clusters have converged
			if (differenceCount == 0)
				return false;

			_clusters = newClusters;
			_distance.SetComparisonVectors(newClusters.Select(d => d.Vector).ToList());
			return true;
		}

		public void ClusterUntilConverged(uint maxIterations = 1000)
		{
			for (var i = 0; i < maxIterations; i++) {
				if (!Cluster())
					break;
			}
		}

		public IReadOnlyVector[][] Clusters => _clusters.Select(c => c.DataIndices.Select(i => _originalData[i]).ToArray()).ToArray();
    }
}
