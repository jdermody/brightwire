using BrightWire.Models.Input;
using BrightWire.Models.Output;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.Connectionist.Helper
{
    internal class DenseSequentialTrainingDataProvider : ISequentialTrainingDataProvider
    {
        readonly ILinearAlgebraProvider _lap;
        readonly SequenceInfo[] _sequenceLength;
        readonly Dictionary<int, List<Tuple<TrainingExample[], int>>> _inputData;
        readonly int _inputSize, _outputSize, _totalCount;

        public DenseSequentialTrainingDataProvider(ILinearAlgebraProvider lap, IReadOnlyList<TrainingExample[]> data)
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
            _inputSize = firstItem.Input.Length;
            _outputSize = firstItem.Output.Length;
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
            for (var k = 0; k < sequenceLength; k++) {
                input[k] = _lap.Create(rows.Count, _inputSize, (x, y) => dataGroup[rows[x]].Item1[k].Input[y]);
                output[k] = _lap.Create(rows.Count, _outputSize, (x, y) => dataGroup[rows[x]].Item1[k].Output[y]);
            }
            return new SequentialMiniBatch(input, output, rows.Select(r => dataGroup[r].Item2).ToArray());
        }
    }
}
