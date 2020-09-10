using MathNet.Numerics.Distributions;
using System;
using System.Collections.Generic;
using System.Linq;
using BrightData;
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
		List<(int[] DataIndices, IFloatVector Cluster)> _clusters = new List<(int[] DataIndices, IFloatVector Cluster)>();
		readonly IReadOnlyList<IFloatVector> _data;

		public KMeans(ILinearAlgebraProvider lap, int k, IReadOnlyList<IFloatVector> data, DistanceMetric distanceMetric = DistanceMetric.Euclidean, int? randomSeed = null)
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
			var newClusters = new List<(int[] DataIndices, IFloatVector Cluster)>();
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

		public IReadOnlyList<IReadOnlyList<IFloatVector>> Clusters => _clusters.Select(c => c.DataIndices.Select(i => _data[i]).ToList()).ToList();




		//class ClusterData : IDisposable
		//{
		//	public class Centroid : IDisposable
		//	{
		//		readonly List<IVector> _data = new List<IVector>();
		//		public IVector Current { get; private set; }
		//		public IReadOnlyList<IVector> Data => _data;

		//		public Centroid(IVector data)
		//		{
		//			_data.Add(data);
		//			Current = data.Clone();
		//		}
		//		public void Dispose()
		//		{
		//			Current.Dispose();
		//		}
		//		public bool Update(IReadOnlyList<IVector> data)
		//		{
		//			// check if the cluster has been updated
		//			if (data.Count == _data.Count) {
		//				var isChanged = false;
		//				var existing = new HashSet<IVector>(_data);
		//				foreach (var item in data) {
		//					if (!existing.Contains(item)) {
		//						isChanged = true;
		//						break;
		//					}
		//				}
		//				if (!isChanged)
		//					return false;
		//			}

		//			Current.Dispose();
		//			Current = null;
		//			_data.Clear();
		//			var denominator = 1f / data.Count;

		//			var isFirst = true;
		//			foreach (var item in data) {
		//				if (Current == null)
		//					Current = item.Clone();
		//				else {
		//					Current.AddInPlace(item, isFirst ? denominator : 1, denominator);
		//					isFirst = false;
		//				}
		//				_data.Add(item);
		//			}
		//			return true;
		//		}
		//	}
		//	readonly List<Centroid> _centroid = new List<Centroid>();
		//	float[] _clusterNorm = null;
		//	IVector[] _curr = null;

		//	public void Add(IVector item)
		//	{
		//		_centroid.Add(new Centroid(item));
		//		_clusterNorm = null;
		//		_curr = null;
		//	}
		//	public void Dispose()
		//	{
		//		foreach (var item in _centroid)
		//			item.Dispose();
		//	}
		//	public IVector CalculateDistance(IVector vector, DistanceMetric distanceMetric)
		//	{
		//		if (_curr == null)
		//			_curr = _centroid.Select(c => c.Current).ToArray();

		//		if (distanceMetric == DistanceMetric.Cosine)
		//			return vector.CosineDistance(_curr, ref _clusterNorm);
		//		else
		//			return vector.FindDistances(_curr, distanceMetric);
		//	}
		//	public bool Update(IReadOnlyList<IReadOnlyList<IVector>> clusterAssignment)
		//	{
		//		var ret = false;
		//		for (var i = 0; i < clusterAssignment.Count; i++) {
		//			if (_centroid[i].Update(clusterAssignment[i]))
		//				ret = true;
		//		}
		//		if (ret) {
		//			_clusterNorm = null;
		//			_curr = null;
		//		}
		//		return ret;
		//	}
		//	public IReadOnlyList<IReadOnlyList<IVector>> GetClusters()
		//	{
		//		return _centroid.Select(c => c.Data).ToList();
		//	}
		//	public IReadOnlyList<Centroid> Centroids => _centroid;
		//}

		//readonly int _k;
		//readonly ClusterData _cluster;
		//readonly DistanceMetric _distanceMetric;
		//readonly IReadOnlyList<IVector> _data;

		//public KMeans(ILinearAlgebraProvider lap, int k, IReadOnlyList<IVector> data, DistanceMetric distanceMetric = DistanceMetric.Euclidean, int? randomSeed = null)
		//{
		//	_k = k;
		//	_distanceMetric = distanceMetric;
		//	_cluster = new ClusterData();
		//	_data = data;

		//	// use kmeans++ to find best initial positions
		//	// https://normaldeviate.wordpress.com/2012/09/30/the-remarkable-k-means/
		//	var rand = randomSeed.HasValue ? new Random(randomSeed.Value) : new Random();
		//	var data2 = data.ToList();

		//	// pick the first at random
		//	var firstIndex = rand.Next(0, data2.Count);
		//	_cluster.Add(data2[firstIndex]);
		//	data2.RemoveAt(firstIndex);

		//	// create a categorical distribution for each subsequent pick
		//	for (var i = 1; i < _k && data2.Count > 0; i++) {
		//		var probabilityList = new List<double>();
		//		foreach (var item in data2) {
		//			using (var distance = _cluster.CalculateDistance(item, _distanceMetric)) {
		//				var minIndex = distance.MinimumIndex();
		//				probabilityList.Add(distance.GetAt(minIndex));
		//			}
		//		}
		//		var distribution = new Categorical(probabilityList.ToArray());
		//		var nextIndex = distribution.Sample();
		//		_cluster.Add(data2[nextIndex]);
		//		data2.RemoveAt(nextIndex);
		//	}
		//}

		//void IDisposable.Dispose()
		//{
		//	_cluster.Dispose();
		//}

		//public bool Cluster()
		//{
		//	var clusterAssignment = Enumerable.Range(0, _k).Select(i => new List<IVector>()).ToArray();

		//	foreach (var item in _data) {
		//		using (var distance = _cluster.CalculateDistance(item, _distanceMetric)) {
		//			clusterAssignment[distance.MinimumIndex()].Add(item);
		//		}
		//	}

		//	return _cluster.Update(clusterAssignment);
		//}

		//public void ClusterUntilConverged(int maxIterations = 1000)
		//{
		//	for (var i = 0; i < maxIterations; i++) {
		//		if (!Cluster())
		//			break;
		//	}
		//}

		//public IReadOnlyList<IReadOnlyList<IVector>> Clusters => _cluster.GetClusters();
	}
}
