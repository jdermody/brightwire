using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using BrightWire.Helper;
using BrightWire.Models.Input;

namespace BrightWire.Connectionist.Helper
{
    internal class WeightedClassificationSetDataProvider : ITrainingDataProvider
    {
        readonly ILinearAlgebraProvider _lap;
        readonly List<Tuple<Dictionary<int, float>, uint>> _data;
        readonly Dictionary<uint, string> _classification;
        readonly int _inputSize, _outputSize;

        public WeightedClassificationSetDataProvider(ILinearAlgebraProvider lap, SparseVectorClassificationSet data, uint maxIndex)
        {
            _lap = lap;
            var classifications = data.GetClassifications();
            _classification = classifications.ToDictionary(d => d.Value, d => d.Key);
            _data = data.Classification.Select(c => Tuple.Create(c.Data.ToDictionary(d => (int)d.Index, d => d.Weight), classifications[c.Name])).ToList();
            _inputSize = (int)maxIndex;
            _outputSize = classifications.Count;
        }

        public int Count { get { return _data.Count; } }
        public int InputSize { get { return _inputSize; } }
        public int OutputSize { get { return _outputSize; } }

        public void StartEpoch()
        {
            // nop
        }

        public float Get(int row, int column)
        {
            float ret;
            if (_data[row].Item1.TryGetValue(column, out ret))
                return ret;
            return 0f;
        }

        public float GetPrediction(int row, int column)
        {
            return _data[row].Item2 == column ? 1f : 0f;
        }

        public IMiniBatch GetTrainingData(IReadOnlyList<int> rows)
        {
            var input = _lap.Create(rows.Count, _inputSize, (x, y) => Get(rows[x], y));
            var output = _lap.Create(rows.Count, _outputSize, (x, y) => GetPrediction(rows[x], y));
            return new MiniBatch(input, output);
        }
    }
}
