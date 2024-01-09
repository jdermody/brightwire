using System.Collections.Generic;
using System.Linq;
using BrightData.Types;

namespace BrightData.LinearAlgebra.Clustering
{
    internal class KMeans(BrightDataContext context, uint maxIterations = 1000) : IClusteringStrategy
    {
        public uint[][] Cluster(IReadOnlyVector[] vectors, uint numClusters, DistanceMetric metric)
        {
            var nextCentroidIndex = 0;
            var centroids = new uint[numClusters][];
            var centroidVectors = new IReadOnlyVector[numClusters];

            // use kmeans++ to find best initial positions
            // https://normaldeviate.wordpress.com/2012/09/30/the-remarkable-k-means/
            var clusterIndexSet = new HashSet<uint>();

            // pick the first cluster at random and set up the distance table
            var lastClusterDistance = AddCluster(context.RandomIndex(vectors.Length))!;

            for (uint i = 1; i < numClusters && i < vectors.Length; i++) {
                // create a categorical distribution to calculate the probability of choosing each subsequent item
                var distribution = context.CreateCategoricalDistribution(lastClusterDistance);

                // sample the next index and add the distances to the table
                uint nextIndex;
                do {
                    nextIndex = distribution.Sample();
                } while ((lastClusterDistance = AddCluster(nextIndex)) == null);
            }

            float[]? AddCluster(uint index)
            {
                if (!clusterIndexSet.Add(index))
                    return null;

                // add the centroid
                var vector = vectors[index];
                centroids[nextCentroidIndex] = new [] { index };
                centroidVectors[nextCentroidIndex] = vector;
                nextCentroidIndex++;

                // calculate distances of each vector to this centroid
                var ret = new float[vectors.Length];
                for (uint i = 0; i < vectors.Length; i++) {
                    float distance = 0;
                    if (i != index)
                        distance = vectors[i].FindDistance(vector, metric);
                    ret[i] = distance;
                }

                return ret;
            }

            // add the vectors
            var vectorSet = new VectorSet(vectors.First().Size);
            vectorSet.Add(vectors);

            for (uint i = 0; i < maxIterations; i++) {
                if (!Cluster(vectorSet, ref centroids, centroidVectors, metric))
                    break;
            }

            return centroids;
        }

        static bool Cluster(VectorSet vectors, ref uint[][] centroids, IReadOnlyVector[] centroidVectors, DistanceMetric metric)
        {
            // cluster the data
            var closest = vectors.Closest(centroidVectors, metric);
            var clusters = closest
                .Select((ci, i) => (ClusterIndex: ci, VectorIndex: (uint)i))
                .GroupBy(d => d.ClusterIndex)
                .Select(c => c.Select(d => d.VectorIndex).ToArray())
                .ToList();

            var differenceCount = 0;
            var newClusters = new uint[centroids.Length][];
            for (var i = 0; i < centroids.Length; i++) {
                var centroid = centroids[i];
                if (i < clusters.Count) {
                    var newIndices = clusters[i];
                    if (centroid.Length == newIndices.Length && new HashSet<uint>(centroid).SetEquals(newIndices))
                        newClusters[i] = centroid;
                    else {
                        differenceCount++;
                        centroidVectors[i] = vectors.GetAverage(newIndices).ToReadOnlyVector();
                        newClusters[i] = newIndices;
                    }
                }
            }

            // check if the clusters have converged
            if (differenceCount == 0)
                return false;

            centroids = newClusters;
            return true;
        }
    }
}
