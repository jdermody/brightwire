using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.Net4.Helper
{
    public class SequenceToSequenceTrainingDataProvider : ISequentialTrainingDataProvider
    {
        readonly ILinearAlgebraProvider _lap;
        readonly Tuple<int, int>[] _sequenceLength;
        readonly Dictionary<int, List<Tuple<IReadOnlyList<float>[], IReadOnlyList<float>[], int>>> _inputData;
        readonly int _inputSize, _totalCount;

        //class MiniBatch : ISequentialMiniBatch
        //{

        //}

        public SequenceToSequenceTrainingDataProvider(ILinearAlgebraProvider lap, IReadOnlyList<Tuple<IReadOnlyList<float>[], IReadOnlyList<float>[]>> data)
        {
            _lap = lap;
            _totalCount = data.Count;

            // group the data by sequence size
            _inputData = data
                .Select((a, ind) => Tuple.Create(a.Item1, a.Item2, ind))
                .GroupBy(s => s.Item1.Length)
                .OrderBy(g => g.Key)
                .ToDictionary(g => g.Key, g => g.ToList())
            ;
            _sequenceLength = _inputData.Select(s => Tuple.Create(s.Key, s.Value.Count)).ToArray();

            // find the dimensions of the input and output
            var firstItem = _inputData.First().Value.First().Item1.First();
            _inputSize = firstItem.Count;
        }

        public int Count { get { return _totalCount; } }

        public int InputSize { get { return _inputSize; } }
        public int OutputSize { get { return _inputSize; } }

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
            var dataGroup = _inputData[sequenceLength];
            var outputList = new List<IReadOnlyList<float>[]>();

            foreach(var index in rows)
                outputList.Add(dataGroup[index].Item2);

            for (var k = 0; k < sequenceLength; k++)
                input[k] = _lap.Create(rows.Count, _inputSize, (x, y) => dataGroup[rows[x]].Item1[k][y]);

            //return new MiniBatch(input, output, rows.Select(r => dataGroup[r].Item2).ToArray());
            return null;
        }
    }
}
