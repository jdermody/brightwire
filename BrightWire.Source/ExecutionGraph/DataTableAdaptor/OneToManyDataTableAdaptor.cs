using BrightWire.ExecutionGraph.Helper;
using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BrightWire.ExecutionGraph.DataTableAdaptor
{
    /// <summary>
    /// Adapts data tables that generate sequences from a single vector
    /// </summary>
    class OneToManyDataTableAdaptor : RowBasedDataTableAdaptorBase
    {
        readonly int[] _rowDepth;
        readonly int _inputSize, _outputSize;

        public OneToManyDataTableAdaptor(ILinearAlgebraProvider lap, IDataTable dataTable) 
            : base(lap, dataTable)
        {
            if (_dataColumnIndex.Length > 1)
                throw new NotImplementedException("Sequential datasets not supported with more than one input data column");

            _rowDepth = new int[dataTable.RowCount];
            FloatVector inputVector = null;
            FloatMatrix outputMatrix = null;
            dataTable.ForEach((row, i) => {
                inputVector = row.GetField<FloatVector>(_dataColumnIndex[0]);
                outputMatrix = row.GetField<FloatMatrix>(_dataTargetIndex);
                _rowDepth[i] = outputMatrix.RowCount;
                if (outputMatrix.ColumnCount != inputVector.Size)
                    throw new ArgumentException("Rows between input and output data tables do not match");
            });

            _inputSize = inputVector.Size;
            _outputSize = outputMatrix.ColumnCount;
        }

        public override IDataSource CloneWith(IDataTable dataTable)
        {
            return new OneToManyDataTableAdaptor(_lap, dataTable);
        }

        public override bool IsSequential => true;
        public override int InputSize => _inputSize;
        public override int OutputSize => _outputSize;

        public override IReadOnlyList<IReadOnlyList<int>> GetBuckets()
        {
            return _rowDepth
                .Select((r, i) => (r, i))
                .GroupBy(t => t.Item1)
                .Select(g => g.Select(d => d.Item2).ToList())
                .ToList()
            ;
        }

        public override IMiniBatch Get(IExecutionContext executionContext, IReadOnlyList<int> rows)
        {
            var data = _GetRows(rows)
                .Select(r => ((FloatVector)r.Data[_dataColumnIndex[0]], (FloatMatrix)r.Data[_dataTargetIndex]))
                .ToList()
            ;
            var outputData = new Dictionary<int, List<FloatVector>>();
            foreach (var item in data) {
                var input = item.Item1;
                var output = item.Item2;
                for (int i = 0, len = output.RowCount; i < len; i++) {
                    if (!outputData.TryGetValue(i, out List<FloatVector> temp))
                        outputData.Add(i, temp = new List<FloatVector>());
                    temp.Add(output.Row[i]);
                }
            }

            var miniBatch = new MiniBatch(rows, this);
            var curr = _lap.CreateMatrix(data.Count, InputSize, (x, y) => data[x].Item1.Data[y]);
            foreach (var item in outputData.OrderBy(kv => kv.Key)) {
                var output = _lap.CreateMatrix(item.Value);
                var type = (item.Key == 0)
                    ? MiniBatchSequenceType.SequenceStart
                    : item.Key == (outputData.Count - 1)
                        ? MiniBatchSequenceType.SequenceEnd
                        : MiniBatchSequenceType.Standard
                ;
                var inputList = new List<IGraphData> {
                    new MatrixGraphData(curr)
                };
                miniBatch.Add(type, inputList, new MatrixGraphData(output));
                curr = output;
            }
            return miniBatch;
        }
    }
}
