using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icbld.BrightWire.Helper
{
    public class SparseDataProvider : IDataProvider
    {
        readonly ILinearAlgebraProvider _lap;
        readonly IReadOnlyList<Dictionary<int, float>> _data;
        readonly bool _normalise;
        IIndexableMatrix _cache = null;
        int _lastBatchSize = 0;

        public SparseDataProvider(ILinearAlgebraProvider lap, IReadOnlyList<Dictionary<int, float>> data, bool normalise = false)
        {
            _lap = lap;
            _data = data;
            _normalise = normalise;
        }

        public int Count
        {
            get
            {
                return _data.Count;
            }
        }

        public IMatrix GetData(IReadOnlyList<int> rows, int inputSize)
        {
            if (_cache == null || _lastBatchSize != rows.Count) {
                _cache?.Dispose();
                _cache = _lap.CreateIndexable(rows.Count, inputSize);
            }
            else
                _cache.Clear();
            _lastBatchSize = rows.Count;

            int index = 0;
            foreach (var row in rows) {
                foreach (var item in _data[row])
                    _cache[index, item.Key] = item.Value;
                ++index;
            }
            if (_normalise)
                _cache.Normalise(MatrixGrouping.ByRow, NormalisationType.Euclidean);
            return _cache;
        }
    }
}
