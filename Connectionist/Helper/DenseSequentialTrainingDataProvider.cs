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
        readonly Tuple<int, int>[] _sequenceLength;
        readonly Dictionary<int, List<Tuple<Tuple<float[], float[]>[], int>>> _inputData;
        readonly int _inputSize, _outputSize, _totalCount;

        class MiniBatch : ISequentialMiniBatch
        {
            readonly IMatrix[] _expectedOutput;

            public int SequenceLength { get; private set; }
            public int BatchSize { get; private set; }
            public IMatrix[] Input { get; private set; }
            public int[] CurrentRows { get; private set; }

            public MiniBatch(IMatrix[] input, IMatrix[] output, int[] rows)
            {
                Input = input;
                SequenceLength = Input.Length;
                BatchSize = Input[0].RowCount;
                CurrentRows = rows;
                _expectedOutput = output;
            }

            public void Dispose()
            {
                foreach (var item in _expectedOutput)
                    item.Dispose();
                foreach (var item in Input)
                    item.Dispose();
            }

            public IMatrix GetExpectedOutput(IReadOnlyList<IMatrix> output, int k)
            {
                return _expectedOutput[k];
            }
        }

        public DenseSequentialTrainingDataProvider(ILinearAlgebraProvider lap, IReadOnlyList<Tuple<float[], float[]>[]> data)
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
            _sequenceLength = _inputData.Select(s => Tuple.Create(s.Key, s.Value.Count)).ToArray();

            // find the dimensions of the input and output
            var firstItem = _inputData.First().Value.First().Item1.First();
            _inputSize = firstItem.Item1.Length;
            _outputSize = firstItem.Item2.Length;
        }

        public int Count { get { return _totalCount; } }

        public int InputSize { get { return _inputSize; } }
        public int OutputSize { get { return _outputSize; } }

        public Tuple<int, int>[] Length
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
                input[k] = _lap.Create(rows.Count, _inputSize, (x, y) => dataGroup[rows[x]].Item1[k].Item1[y]);
                output[k] = _lap.Create(rows.Count, _outputSize, (x, y) => dataGroup[rows[x]].Item1[k].Item2[y]);
            }
            return new MiniBatch(input, output, rows.Select(r => dataGroup[r].Item2).ToArray());
        }
    }
}
