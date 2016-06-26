using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.Net4.Helper
{
    public class DenseSequentialTrainingDataProvider : ISequentialTrainingDataProvider
    {
        readonly ILinearAlgebraProvider _lap;
        readonly Tuple<int, int>[] _sequenceLength;
        readonly IReadOnlyList<Tuple<Tuple<float[], float[]>[], int>>[] _inputData;
        readonly int _inputSize, _outputSize;

        public DenseSequentialTrainingDataProvider(ILinearAlgebraProvider lap, IReadOnlyList<Tuple<float[], float[]>[]> data)
        {
            _lap = lap;

            // group the data by sequence size
            _inputData = data
                .Select((a, ind) => Tuple.Create(a, ind))
                .GroupBy(s => s.Item1.Length)
                .OrderBy(g => g.Key)
                .Select(g => g.ToList())
                .ToArray()
            ;
            _sequenceLength = _inputData.Select(s => Tuple.Create(s.First().Item1.Length, s.Count)).ToArray();

            var firstItem = _inputData[0].First().Item1;
            _inputSize = firstItem.First().Item1.Length;
            _outputSize = firstItem.First().Item2.Length;
        }


        public Tuple<int, int>[] Length
        {
            get
            {
                return _sequenceLength;
            }
        }

        public Tuple<IMatrix[], IMatrix[]> GetTrainingData(int sequenceLength, IReadOnlyList<int> rows, int inputSize, int outputSize)
        {
            var input = new IMatrix[sequenceLength];
            var output = new IMatrix[sequenceLength];
            var dataGroup = _inputData[sequenceLength];
            for (var k = 0; k < sequenceLength; k++) {
                input[k] = _lap.Create(rows.Count, inputSize, (x, y) => dataGroup[rows[x]].Item1[k].Item1[y]);
                output[k] = _lap.Create(rows.Count, outputSize, (x, y) => dataGroup[rows[x]].Item1[k].Item2[y]);
            }
            return Tuple.Create(input, output);
        }
    }
}
