using BrightData;
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
        readonly uint[] _rowDepth;

	    public OneToManyDataTableAdaptor(ILinearAlgebraProvider lap, IRowOrientedDataTable dataTable) 
            : base(lap, dataTable)
        {
            if (_dataColumnIndex.Length > 1)
                throw new NotImplementedException("Sequential datasets not supported with more than one input data column");

            _rowDepth = new uint[dataTable.RowCount];
            Vector<float> inputVector = null;
            Matrix<float> outputMatrix = null;
            dataTable.ForEachRow((row, i) => {
                inputVector = (Vector<float>)row[_dataColumnIndex[0]];
                outputMatrix = (Matrix<float>)row[_dataTargetIndex];
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
        public override uint InputSize { get; }
	    public override uint? OutputSize { get; }

	    public override uint[][] GetBuckets()
        {
            return _rowDepth
                .Select((r, i) => (r, i))
                .GroupBy(t => t.Item1)
                .Select(g => g.Select(d => (uint)d.Item2).ToArray())
                .ToArray()
            ;
        }

        public override IMiniBatch Get(IExecutionContext executionContext, uint[] rows)
        {
            var data = _GetRows(rows)
                .Select(r => ((Vector<float>)r[_dataColumnIndex[0]], (Matrix<float>)r[_dataTargetIndex]))
                .ToList()
            ;
            var outputData = new Dictionary<uint, List<Vector<float>>>();
            foreach (var item in data) {
                var output = item.Item2;
                for (uint i = 0, len = output.RowCount; i < len; i++) {
                    if (!outputData.TryGetValue(i, out var temp))
                        outputData.Add(i, temp = new List<Vector<float>>());
                    temp.Add(output.Row(i));
                }
            }

            var miniBatch = new MiniBatch(rows, this);
            var curr = _lap.CreateMatrix((uint)data.Count, (uint)InputSize, (x, y) => data[(int)x].Item1.Segment[y]);
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
