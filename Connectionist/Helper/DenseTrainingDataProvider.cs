using BrightWire.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.Connectionist.Helper
{
    internal class DenseTrainingDataProvider : ITrainingDataProvider
    {
        readonly IReadOnlyList<Tuple<float[], float[]>> _data;
        readonly ILinearAlgebraProvider _lap;
        readonly int _inputSize, _outputSize;

        public DenseTrainingDataProvider(ILinearAlgebraProvider lap, IReadOnlyList<Tuple<float[], float[]>> data)
        {
            _lap = lap;
            _data = data;

            var first = data.First();
            _inputSize = first.Item1.Length;
            _outputSize = first.Item2.Length;
        }

        public int InputSize { get { return _inputSize; } }
        public int OutputSize { get { return _outputSize; } }

        public float Get(int row, int column)
        {
            return _data[row].Item1[column];
        }

        public float GetPrediction(int row, int column)
        {
            return _data[row].Item2[column];
        }

        public IMiniBatch GetTrainingData(IReadOnlyList<int> rows)
        {
            var input = _lap.Create(rows.Count, _inputSize, (x, y) => Get(rows[x], y));
            var output = _lap.Create(rows.Count, _outputSize, (x, y) => GetPrediction(rows[x], y));
            return new MiniBatch(input, output);
        }

        public int Count { get { return _data.Count; } }
    }
}
