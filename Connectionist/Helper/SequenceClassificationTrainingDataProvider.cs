using BrightWire.Helper;
using BrightWire.Models.Input;
using BrightWire.Models.Output;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrightWire.Connectionist.Helper
{
    public class SequenceClassificationTrainingDataProvider : ITrainingDataProvider, ICanBackpropagate
    {
        readonly IReadOnlyList<SequenceClassificationTrainingExample> _data;
        readonly ILinearAlgebraProvider _lap;
        readonly int _inputSize, _outputSize, _sequenceInputSize;
        readonly Func<ISequentialMiniBatch, IMatrix> _feedForward;
        readonly Action<IReadOnlyList<IMatrix>> _backpropagate;
        SequenceInfo[] _currentBatchSequence;

        public SequenceClassificationTrainingDataProvider(
            ILinearAlgebraProvider lap, 
            IReadOnlyList<SequenceClassificationTrainingExample> data, 
            int sequenceOutputSize,
            Func<ISequentialMiniBatch, IMatrix> feedForward,
            Action<IReadOnlyList<IMatrix>> backpropagate
        ) {
            _lap = lap;
            _data = data;

            var first = data.First();
            _inputSize = sequenceOutputSize;
            _outputSize = first.Output.Length;
            _sequenceInputSize = first.Input.Sequence.First().Data.Length;
            _feedForward = feedForward;
            _backpropagate = backpropagate;
        }

        public int InputSize { get { return _inputSize; } }
        public int OutputSize { get { return _outputSize; } }

        IMatrix _Get(IReadOnlyList<int> rows)
        {
            var batch = rows
                .Select(i => _data[i].Input)
                .ToList()
            ;

            var inputData = batch
                .Select((a, ind) => Tuple.Create(a, ind))
                .GroupBy(s => s.Item1.Sequence.Length)
                .OrderBy(g => g.Key)
                .ToDictionary(g => g.Key, g => g.ToList())
            ;
            _currentBatchSequence = inputData.Select(s => new SequenceInfo(s.Key, s.Value.Count)).ToArray();

            IMatrix ret = null;
            foreach(var item in _currentBatchSequence) {
                var input = new IMatrix[item.SequenceLength];
                var dataGroup = inputData[item.SequenceLength];
                var batchRows = dataGroup.Select(d => rows[d.Item2]).ToArray();
                for (var k = 0; k < item.SequenceLength; k++)
                    input[k] = _lap.Create(item.SampleCount, _sequenceInputSize, (x, y) => dataGroup[x].Item1.Sequence[k].Data[y]);

                var trainingSequence = new SequentialMiniBatch(input, null, batchRows);
                var matrix = _feedForward(trainingSequence);
                if (ret == null)
                    ret = matrix;
                else
                    ret = ret.ConcatColumns(matrix);
            }
            return ret;
        }

        public float GetPrediction(int row, int column)
        {
            return _data[row].Output[column];
        }

        public IMiniBatch GetTrainingData(IReadOnlyList<int> rows)
        {
            var input = _Get(rows);
            var output = _lap.Create(rows.Count, _outputSize, (x, y) => GetPrediction(rows[x], y));
            return new MiniBatch(input, output);
        }

        public int Count { get { return _data.Count; } }

        public void StartEpoch()
        {
            _currentBatchSequence = null;
        }

        public IMatrix Backpropagate(IMatrix errorSignal, ITrainingContext context, bool calculateOutput, INeuralNetworkUpdateAccumulator updates = null)
        {
            var offset = 0;
            var list = new List<IMatrix>();
            if (_currentBatchSequence.Length > 1) {
                foreach (var item in _currentBatchSequence) {
                    list.Add(errorSignal.GetNewMatrixFromRows(Enumerable.Range(offset, item.SampleCount).ToList()));
                    offset += item.SampleCount;
                }
            }
            else
                list.Add(errorSignal);

            _backpropagate(list);
            return null;
        }
    }
}
