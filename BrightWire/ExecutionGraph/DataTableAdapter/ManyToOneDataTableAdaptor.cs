using BrightWire.ExecutionGraph.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using BrightData;
using BrightData.LinearAlgebra.ReadOnly;

namespace BrightWire.ExecutionGraph.DataTableAdapter
{
    /// <summary>
    /// Adapts data tables that classify a sequence into a single classification
    /// </summary>
    internal class ManyToOneDataTableAdapter : TypedRowBasedDataTableAdapterBase<ReadOnlyMatrix<float>, ReadOnlyVector<float>>
    {
        readonly uint[] _rowDepth;

	    public ManyToOneDataTableAdapter(IDataTable dataTable, uint[] featureColumns) 
            : base(dataTable, featureColumns)
        {
            if (_featureColumnIndices.Length > 1)
                throw new NotImplementedException("Sequential data sets not supported with more than one input data column");

            // find the number of sequences of each row
            _rowDepth = new uint[dataTable.RowCount];
            ReadOnlyMatrix<float>? inputMatrix = null;
            ReadOnlyVector<float>? outputVector = null;
            foreach(var row in _buffer.EnumerateAllTyped().ToBlockingEnumerable()) {
                inputMatrix = row.C1;
                outputVector = row.C2;
                _rowDepth[row.RowIndex] = inputMatrix.RowCount;
                if (inputMatrix.ColumnCount != outputVector.Size)
                    throw new ArgumentException("Rows between input and output data tables do not match");
            }
            if (inputMatrix == null || outputVector == null)
                throw new Exception("No data found");

            InputSize = inputMatrix.ColumnCount;
            OutputSize = outputVector.Size;
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

        public override async Task<MiniBatch> Get(uint[] rows)
        {
            var index = 0;
            var outputRows = new ReadOnlyVector<float>[rows.Length];
            var inputData = new Dictionary<uint, List<IReadOnlyNumericSegment<float>>>();
            await foreach (var row in GetRows(rows)) {
                var input = row.C1;
                for (uint i = 0, len = input.RowCount; i < len; i++) {
                    if (!inputData.TryGetValue(i, out var temp))
                        inputData.Add(i, temp = []);
                    temp.Add(input.GetReadOnlyRow(i));
                }

                outputRows[index++] = row.C2;
            }

            var miniBatch = new MiniBatch(rows, this);
            var lap = _dataTable.Context.LinearAlgebraProvider;
            var output = lap.CreateMatrixFromRows(outputRows);
            foreach (var item in inputData.OrderBy(kv => kv.Key)) {
                var input = lap.CreateMatrixFromRows(CollectionsMarshal.AsSpan(item.Value));
                var type = (item.Key == 0)
                    ? MiniBatchSequenceType.SequenceStart
                    : item.Key == (inputData.Count - 1)
                        ? MiniBatchSequenceType.SequenceEnd
                        : MiniBatchSequenceType.Standard
                ;
                miniBatch.Add(type, input.AsGraphData(), type == MiniBatchSequenceType.SequenceEnd ? output.AsGraphData() : null);
            }
            return miniBatch;
        }
    }
}
