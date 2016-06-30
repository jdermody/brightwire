using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.Helper
{
    public class SparseTrainingDataProvider : ITrainingDataProvider
    {
        readonly ILinearAlgebraProvider _lap;
        readonly IReadOnlyList<Tuple<Dictionary<int, float>, Dictionary<int, float>>> _data;
        readonly int _inputSize, _outputSize;
        IIndexableMatrix _inputCache = null, _outputCache = null;
        int _lastBatchSize = 0;

        public SparseTrainingDataProvider(ILinearAlgebraProvider lap, IReadOnlyList<Tuple<Dictionary<int, float>, Dictionary<int, float>>> data, int inputSize, int outputSize)
        {
            _lap = lap;
            _data = data;
            _inputSize = inputSize;
            _outputSize = outputSize;
        }

        public int Count { get { return _data.Count; } }
        public int InputSize { get { return _inputSize; } }
        public int OutputSize { get { return _outputSize; } }

        public float Get(int row, int column)
        {
            float ret;
            if (_data[row].Item1.TryGetValue(column, out ret))
                return ret;
            return 0f;
        }

        public float GetPrediction(int row, int column)
        {
            float ret;
            if (_data[row].Item2.TryGetValue(column, out ret))
                return ret;
            return 0f;
        }

        public Tuple<IMatrix, IMatrix> GetTrainingData(IReadOnlyList<int> rows, int inputSize, int outputSize)
        {
            if (_inputCache == null || _lastBatchSize != rows.Count) {
                _inputCache?.Dispose();
                _inputCache = _lap.CreateIndexable(rows.Count, inputSize);
            }
            else
                _inputCache.Clear();
            if (_outputCache == null || _lastBatchSize != rows.Count) {
                _outputCache?.Dispose();
                _outputCache = _lap.CreateIndexable(rows.Count, outputSize);
            }
            else
                _outputCache.Clear();
            _lastBatchSize = rows.Count;

            int index = 0;
            foreach (var row in rows) {
                foreach (var item in _data[row].Item1)
                    _inputCache[index, item.Key] = item.Value;
                foreach (var item in _data[row].Item2)
                    _outputCache[index, item.Key] = item.Value;
                ++index;
            }
            return Tuple.Create<IMatrix, IMatrix>(_inputCache, _outputCache);
        }
    }
}
