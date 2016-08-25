using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.Net4.Unsupervised.Clustering
{
    public class KMeans
    {
        readonly int _k;
        readonly DistanceMetric _distanceMetric;

        public KMeans(int k, DistanceMetric distanceMetric)
        {
            _k = k;
            _distanceMetric = distanceMetric;
        }

        public IVector Merge(IReadOnlyList<IVector> list)
        {
            IVector main = null;
            var denominator = 1f / list.Count;

            var isFirst = true;
            foreach (var item in list) {
                if (main == null)
                    main = item.Clone();
                else {
                    main.AddInPlace(item, isFirst ? denominator : 1, denominator);
                    isFirst = false;
                }
            }
            return main;
        }

        public IVector FindFurthestFrom(IReadOnlyList<IVector> data, IReadOnlyList<IVector> clusters)
        {
            IVector ret = null;
            float best = float.MinValue;
            foreach (var item in data) {
                float closest = float.MaxValue;
                foreach (var item2 in clusters) {
                    var distance = _distanceMetric.Calculate(item, item2);
                    if (distance < closest)
                        closest = distance;
                }
                if (closest > best) {
                    best = closest;
                    ret = item;
                }
            }
            return ret;
        }

        public IReadOnlyList<Tuple<IVector, IVector[]>> Cluster(IReadOnlyList<IVector> data, IReadOnlyList<IVector> initialClusters = null)
        {
            // randomly choose some initial positions
            IReadOnlyList<IVector> clusters = initialClusters != null ? initialClusters : data.Shuffle().Take(_k).Select(v => v.AsIndexable()).ToList();
            var curr = _Cluster(data, clusters);
            clusters = curr.GroupBy(kv => kv.Value).OrderBy(g => g.Key).Select(g => Merge(g.Select(d => d.Key).ToList())).ToList();

            // cluster until converged
            for (int i = 0; i < 1000; i++) {
                var next = _Cluster(data, clusters);
                var divergence = curr.Count(d => next[d.Key] != d.Value);
                if (divergence == 0)
                    break;
                curr = next;
                clusters = curr.GroupBy(kv => kv.Value).OrderBy(g => g.Key).Select(g => Merge(g.Select(d => d.Key).ToList())).ToList();
            }

            return curr.GroupBy(kv => kv.Value).Select(g => Tuple.Create(clusters[g.Key], g.Select(d => d.Key).ToArray())).ToList();
        }

        Dictionary<IVector, int> _Cluster(IReadOnlyList<IVector> data, IReadOnlyList<IVector> clusters)
        {
            var ret = new Dictionary<IVector, int>();
            foreach (var item in data) {
                var results = clusters
                    .Select((c, ind) => Tuple.Create(ind, _distanceMetric.Calculate(c, item)))
                    .OrderBy(d => d.Item2)
                    .ToList()
                ;
                ret[item] = results.Select(d => d.Item1).FirstOrDefault();
            }
            return ret;
        }

        public IEnumerable<int> Cluster2(IReadOnlyList<IVector> clusters, IReadOnlyList<IVector> data)
        {
            var dataLength = data.First().Count;

            foreach (var item in data) {
                var results = clusters
                    .Select((c, ind) => Tuple.Create(ind, _distanceMetric.Calculate(c, item)))
                    .OrderBy(d => d.Item2)
                    .ToList()
                ;
                yield return results.Select(d => d.Item1).First();
            }
        }
    }
}
