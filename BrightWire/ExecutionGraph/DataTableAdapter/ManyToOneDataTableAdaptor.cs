using BrightWire.ExecutionGraph.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using BrightData;

namespace BrightWire.ExecutionGraph.DataTableAdapter
{
    /// <summary>
    /// Adapts data tables that classify a sequence into a single classification
    /// </summary>
    internal class ManyToOneDataTableAdapter : RowBasedDataTableAdapterBase
    {
        readonly uint[] _featureColumns;
        readonly uint[] _rowDepth;
        readonly uint _outputSize;

	    public ManyToOneDataTableAdapter(IDataTable dataTable, uint[] featureColumns) 
            : base(dataTable, featureColumns)
        {
            if (_featureColumnIndices.Length > 1)
                throw new NotImplementedException("Sequential data sets not supported with more than one input data column");
            _featureColumns = featureColumns;

            // find the number of sequences of each row
            _rowDepth = new uint[dataTable.RowCount];
            IReadOnlyMatrix? inputMatrix = null;
            IReadOnlyVector? outputVector = null;
            foreach(var (i, row) in dataTable.GetAllRowData()) {
                inputMatrix = (IReadOnlyMatrix)row[_featureColumnIndices[0]];
                outputVector = (IReadOnlyVector)row[_targetColumnIndex];
                _rowDepth[i] = inputMatrix.RowCount;
                if (inputMatrix.ColumnCount != outputVector.Size)
                    throw new ArgumentException("Rows between input and output data tables do not match");
            }
            if (inputMatrix == null || outputVector == null)
                throw new Exception("No data found");

            InputSize = inputMatrix.ColumnCount;
            OutputSize = _outputSize = outputVector.Size;
        }

        public override IDataSource CloneWith(IDataTable dataTable)
        {
            return new ManyToOneDataTableAdapter(dataTable, _featureColumns);
        }

        public override uint InputSize { get; }
	    public override uint? OutputSize { get; }

	    public override uint[][] GetSequentialBatches()
        {
            return _rowDepth
                .Select((r, i) => (Row: r, Index: i))
                .GroupBy(t => t.Row)
                .Select(g => g.Select(d => (uint)d.Index).ToArray())
                .ToArray()
            ;
        }

        public override IMiniBatch Get(uint[] rows)
        {
            var lap = _dataTable.Context.LinearAlgebraProvider;
            var data = GetRows(rows)
                .Select(r => (Matrix: (IReadOnlyMatrix)r[_featureColumnIndices[0]], Vector: (IReadOnlyVector)r[_targetColumnIndex]))
                .ToList()
            ;
            var inputData = new Dictionary<uint, List<IReadOnlyNumericSegment<float>>>();
            foreach (var (input, _) in data) {
                for (uint i = 0, len = input.RowCount; i < len; i++) {
                    if (!inputData.TryGetValue(i, out var temp))
                        inputData.Add(i, temp = new());
                    temp.Add(input.GetRow(i).ReadOnlySegment);
                }
            }

            var miniBatch = new MiniBatch(rows, this);
            var outputVector = lap.CreateMatrix((uint)data.Count, _outputSize, (x, y) => data[(int)x].Vector[y]);
            foreach (var item in inputData.OrderBy(kv => kv.Key)) {
                var input = lap.CreateMatrixFromRows(CollectionsMarshal.AsSpan(item.Value));
                var type = (item.Key == 0)
                    ? MiniBatchSequenceType.SequenceStart
                    : item.Key == (inputData.Count - 1)
                        ? MiniBatchSequenceType.SequenceEnd
                        : MiniBatchSequenceType.Standard
                ;
                miniBatch.Add(type, input.AsGraphData(), type == MiniBatchSequenceType.SequenceEnd ? outputVector.AsGraphData() : null);
            }
            return miniBatch;
        }
    }
}
