using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.Unsupervised.Clustering.Helper
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
}
