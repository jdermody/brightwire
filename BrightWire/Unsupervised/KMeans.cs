using MathNet.Numerics.Distributions;
using System;
using System.Collections.Generic;
using System.Linq;
using BrightWire.LinearAlgebra.Helper;
using BrightWire.Models;
using BrightWire.Source.Helper;

namespace BrightWire.Unsupervised
{
	/// <summary>
	/// K Means clustering
	/// https://en.wikipedia.org/wiki/K-means_clustering
	/// </summary>
	class KMeans : IDisposable
	{
		readonly VectorDistanceHelper _distance;
		List<(int[] DataIndices, IVector Cluster)> _clusters = new List<(int[] DataIndices, IVector Cluster)>();
		readonly IReadOnlyList<IVector> _data;

		public KMeans(ILinearAlgebraProvider lap, int k, IReadOnlyList<IVector> data, DistanceMetric distanceMetric = DistanceMetric.Euclidean, int? randomSeed = null)
		{
			_data = data;
			_distance = new VectorDistanceHelper(lap, data, distanceMetric);

			// use kmeans++ to find best initial positions
			// https://normaldeviate.wordpress.com/2012/09/30/the-remarkable-k-means/
			var rand = randomSeed.HasValue ? new Random(randomSeed.Value) : new Random();
			var clusterIndexSet = new HashSet<int>();
			var distanceTable = new List<float>[data.Count];

			bool AddCluster(int index, Action<int, float> callback)
			{
				if (!clusterIndexSet.Add(index))
					return false;

				var vector = data[index].Clone();
				_distance.AddComparison(vector);
				_clusters.Add((new[] { index }, vector));

				// calculate distances
				for (var i = 0; i < data.Count; i++) {
					float distance = 0;
					if (i != index)
						distance = data[i].FindDistance(vector, distanceMetric);
					callback(i, distance);
				}

				return true;
			}

			// pick the first cluster at random and set up the distance table
			AddCluster(rand.Next(0, data.Count), (index, distance) => distanceTable[index] = new List<float>{ distance });

			for (var i = 1; i < k && i < data.Count; i++) {
				// create a categorical distribution to calculate the probability of choosing each subsequent item
				var distribution = new Categorical(distanceTable.Select(l => (double)l.Min()).ToArray());

				// sample the next index and add the distances to the table
				int nextIndex;
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
				.Select((ci, i) => (ci, i))
				.GroupBy(d => d.Item1)
				.Select(c => c.Select(d => d.Item2).ToArray())
				.ToList();

			var differenceCount = 0;
			var newClusters = new List<(int[] DataIndices, IVector Cluster)>();
			for (var i = 0; i < _clusters.Count; i++) {
				var oldIndices = _clusters[i].DataIndices;
				if (i < clusters.Count) {
					var newIndices = clusters[i];
					if (oldIndices.Length == newIndices.Length && new HashSet<int>(oldIndices).SetEquals(newIndices))
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

		public void ClusterUntilConverged(int maxIterations = 1000)
		{
			for (var i = 0; i < maxIterations; i++) {
				if (!Cluster())
					break;
			}
		}

		public IReadOnlyList<IReadOnlyList<IVector>> Clusters => _clusters.Select(c => c.DataIndices.Select(i => _data[i]).ToList()).ToList();
	}
}
