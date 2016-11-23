using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using BrightWire.Models.Output;

namespace BrightWire.Connectionist.Helper
{
    internal class SparseSequentialTrainingDataProvider : ISequentialTrainingDataProvider
    {
        readonly ILinearAlgebraProvider _lap;
        readonly SequenceInfo[] _sequenceLength;
        readonly Dictionary<int, List<Tuple<Tuple<Dictionary<uint, float>, Dictionary<uint, float>>[], int>>> _inputData;
        readonly int _inputSize, _outputSize, _totalCount;

        public SparseSequentialTrainingDataProvider(ILinearAlgebraProvider lap, IReadOnlyList<Tuple<Dictionary<uint, float>, Dictionary<uint, float>>[]> data)
        {
            _lap = lap;
            _totalCount = data.Count;

            // group the data by sequence size
            _inputData = data
                .Select((a, ind) => Tuple.Create(a, ind))
                .GroupBy(s => s.Item1.Length)
                .OrderBy(g => g.Key)
                .ToDictionary(g => g.Key, g => g.ToList())
            ;
            _sequenceLength = _inputData.Select(s => new SequenceInfo(s.Key, s.Value.Count)).ToArray();

            // find the dimensions of the input and output
            var firstItem = _inputData.First().Value.First().Item1.First();
            _inputSize = firstItem.Item1.Count;
            _outputSize = firstItem.Item2.Count;
        }

        public int Count { get { return _totalCount; } }

        public int InputSize { get { return _inputSize; } }
        public int OutputSize { get { return _outputSize; } }

        public SequenceInfo[] Length
        {
            get
            {
                return _sequenceLength;
            }
        }

        public ISequentialMiniBatch GetTrainingData(int sequenceLength, IReadOnlyList<int> rows)
        {
            var input = new IMatrix[sequenceLength];
            var output = new IMatrix[sequenceLength];
            var dataGroup = _inputData[sequenceLength];
            var selectedRow = rows.Select(r => dataGroup[r]).ToList();

            for (var k = 0; k < sequenceLength; k++) {
                input[k] = _lap.Create(rows.Count, _inputSize, (x, y) => _GetValue(y, dataGroup[rows[x]].Item1[k].Item1));
                output[k] = _lap.Create(rows.Count, _outputSize, (x, y) => _GetValue(y, dataGroup[rows[x]].Item1[k].Item2));
            }
            return new SequentialMiniBatch(input, output, rows.Select(r => dataGroup[r].Item2).ToArray());
        }

        float _GetValue(int index, Dictionary<uint, float> table)
        {
            float ret;
            if (table.TryGetValue((uint)index, out ret))
                return ret;
            return 0f;
        }
    }
}
