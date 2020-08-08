using BrightTable;
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

	    public OneToManyDataTableAdaptor(ILinearAlgebraProvider lap, IRowOrientedDataTable dataTable) 
            : base(lap, dataTable)
        {
            if (_dataColumnIndex.Length > 1)
                throw new NotImplementedException("Sequential datasets not supported with more than one input data column");

            _rowDepth = new int[dataTable.RowCount];
            FloatVector inputVector = null;
            FloatMatrix outputMatrix = null;
            dataTable.ForEachRow((row, i) => {
                inputVector = (FloatVector)row[_dataColumnIndex[0]];
                outputMatrix = (FloatMatrix)row[_dataTargetIndex];
                _rowDepth[i] = outputMatrix.RowCount;
                if (outputMatrix.ColumnCount != inputVector.Size)
                    throw new ArgumentException("Rows between input and output data tables do not match");
            });

            InputSize = inputVector.Size;
            OutputSize = outputMatrix.ColumnCount;
        }

        public override IDataSource CloneWith(IRowOrientedDataTable dataTable)
        {
            return new OneToManyDataTableAdaptor(_lap, dataTable);
        }

        public override bool IsSequential => true;
        public override int InputSize { get; }
	    public override int OutputSize { get; }

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
                var output = item.Item2;
                for (int i = 0, len = output.RowCount; i < len; i++) {
                    if (!outputData.TryGetValue(i, out var temp))
                        outputData.Add(i, temp = new List<FloatVector>());
                    temp.Add(output.Row[i]);
                }
            }

            var miniBatch = new MiniBatch(rows, this);
            var curr = _lap.CreateMatrix(data.Count, InputSize, (x, y) => data[x].Item1.Data[y]);
            foreach (var item in outputData.OrderBy(kv => kv.Key)) {
                var output = _lap.CreateMatrixFromRows(item.Value);
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
