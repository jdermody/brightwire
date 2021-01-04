using System;
using System.Collections.Generic;
using System.Linq;
using BrightData;
using BrightData.Distributions;
using BrightWire.Helper;

namespace BrightWire.Unsupervised
{
	/// <summary>
	/// K Means clustering
	/// https://en.wikipedia.org/wiki/K-means_clustering
	/// </summary>
	class KMeans : IDisposable
	{
		readonly VectorDistanceHelper _distance;
		List<(uint[] DataIndices, IFloatVector Cluster)> _clusters = new List<(uint[], IFloatVector)>();
		readonly IFloatVector[] _data;

		public KMeans(IBrightDataContext context, uint k, IEnumerable<IFloatVector> data, DistanceMetric distanceMetric = DistanceMetric.Euclidean)
		{
			_data = data.ToArray();
			_distance = new VectorDistanceHelper(_data, distanceMetric);

			// use kmeans++ to find best initial positions
			// https://normaldeviate.wordpress.com/2012/09/30/the-remarkable-k-means/
            var clusterIndexSet = new HashSet<uint>();
			var distanceTable = new List<float>[_data.Length];

			bool AddCluster(uint index, Action<uint, float> callback)
			{
				if (!clusterIndexSet.Add(index))
					return false;

				var vector = _data[index].Clone();
				_distance.AddComparison(vector);
				_clusters.Add((new[] { index }, vector));

				// calculate distances
				for (uint i = 0; i < _data.Length; i++) {
					float distance = 0;
					if (i != index)
						distance = _data[i].FindDistance(vector, distanceMetric);
					callback(i, distance);
				}

				return true;
			}

			// pick the first cluster at random and set up the distance table
            AddCluster(context.RandomIndex(_data.Length), (index, distance) => distanceTable[index] = new List<float>{ distance });

			for (uint i = 1; i < k && i < _data.Length; i++) {
				// create a categorical distribution to calculate the probability of choosing each subsequent item
				var distribution = context.CreateCategoricalDistribution(distanceTable.Select(l => l.Min()).ToArray());

				// sample the next index and add the distances to the table
				uint nextIndex;
				do {
					nextIndex = distribution.Sample();
				} while (!AddCluster(nextIndex, (index, distance) => distanceTable[index].Add(distance)));
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
				.Select((ci, i) => (ci, (uint)i))
				.GroupBy(d => d.Item1)
				.Select(c => c.Select(d => d.Item2).ToArray())
				.ToList();

			var differenceCount = 0;
			var newClusters = new List<(uint[] DataIndices, IFloatVector Cluster)>();
			for (var i = 0; i < _clusters.Count; i++) {
				var oldIndices = _clusters[i].DataIndices;
				if (i < clusters.Count) {
					var newIndices = clusters[i];
					if (oldIndices.Length == newIndices.Length && new HashSet<uint>(oldIndices).SetEquals(newIndices))
						newClusters.Add(_clusters[i]);
					else {
						differenceCount++;
						newClusters.Add((newIndices, _distance.GetAverageFromData(newIndices)));
					}
				}
			}

			// check if the clusters have converged
			if (differenceCount == 0)
				return false;

			_clusters = newClusters;
			_distance.SetComparisonVectors(newClusters.Select(d => d.Item2).ToList());
			return true;
		}

		public void ClusterUntilConverged(uint maxIterations = 1000)
		{
			for (var i = 0; i < maxIterations; i++) {
				if (!Cluster())
					break;
			}
		}

		public IFloatVector[][] Clusters => _clusters.Select(c => c.DataIndices.Select(i => _data[i]).ToArray()).ToArray();
    }
}
