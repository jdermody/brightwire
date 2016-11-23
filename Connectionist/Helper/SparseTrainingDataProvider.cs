using BrightWire.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.Connectionist.Helper
{
    internal class SparseTrainingDataProvider : ITrainingDataProvider
    {
        readonly ILinearAlgebraProvider _lap;
        readonly IReadOnlyList<Tuple<Dictionary<uint, float>, Dictionary<uint, float>>> _data;
        readonly int _inputSize, _outputSize;
        int _lastBatchSize = 0;

        public SparseTrainingDataProvider(ILinearAlgebraProvider lap, IReadOnlyList<Tuple<Dictionary<uint, float>, Dictionary<uint, float>>> data, int inputSize, int outputSize)
        {
            _lap = lap;
            _data = data;
            _inputSize = inputSize;
            _outputSize = outputSize;
        }

        public int Count { get { return _data.Count; } }
        public int InputSize { get { return _inputSize; } }
        public int OutputSize { get { return _outputSize; } }

        public float Get(int row, uint column)
        {
            float ret;
            if (_data[row].Item1.TryGetValue(column, out ret))
                return ret;
            return 0f;
        }

        public float GetPrediction(int row, uint column)
        {
            float ret;
            if (_data[row].Item2.TryGetValue(column, out ret))
                return ret;
            return 0f;
        }

        public IMiniBatch GetTrainingData(IReadOnlyList<int> rows)
        {
            var input = _lap.Create(rows.Count, _inputSize, (x, y) => Get(rows[x], (uint)y));
            var output = _lap.Create(rows.Count, _outputSize, (x, y) => GetPrediction(rows[x], (uint)y));
            return new MiniBatch(input, output);
        }

        public void StartEpoch()
        {
            // nop
        }
    }
}
