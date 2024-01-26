using BrightData;
using BrightWire.ExecutionGraph.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using BrightData.LinearAlgebra.ReadOnly;

namespace BrightWire.ExecutionGraph.DataTableAdapter
{
    /// <summary>
    /// Adapts data tables that generate sequences from a single vector
    /// </summary>
    internal class OneToManyDataTableAdapter : TypedRowBasedDataTableAdapterBase<ReadOnlyVector, ReadOnlyMatrix>
    {
        readonly uint[] _rowDepth;

	    public OneToManyDataTableAdapter(IDataTable dataTable, uint[] featureColumns) 
            : base(dataTable, featureColumns)
        {
            if (_featureColumnIndices.Length > 1)
                throw new NotImplementedException("Sequential datasets not supported with more than one input data column");

            // find the number of sequences of each row
            _rowDepth = new uint[dataTable.RowCount];
            ReadOnlyVector? inputVector = null;
            ReadOnlyMatrix? outputMatrix = null;
            foreach(var row in _buffer.EnumerateAllTyped().ToBlockingEnumerable()) {
                inputVector = row.C1;
                outputMatrix = row.C2;
                _rowDepth[row.RowIndex] = outputMatrix.RowCount;
                if (outputMatrix.ColumnCount != inputVector.Size)
                    throw new ArgumentException("Rows between input and output data tables do not match");
            }
            if (inputVector == null || outputMatrix == null)
                throw new Exception("No data found");

            InputSize = inputVector.Size;
            OutputSize = outputMatrix.ColumnCount;
        }

        public override IDataSource CloneWith(IDataTable dataTable)
        {
            return new OneToManyDataTableAdapter(dataTable, _featureColumns);
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
            var lap = _dataTable.Context.LinearAlgebraProvider;
            var inputData = new ReadOnlyVector[rows.Length];
            var index = 0;
            var outputData = new Dictionary<uint, List<IReadOnlyNumericSegment<float>>>();
            await foreach (var item in GetRows(rows)) {
                var output = item.C2;
                for (uint i = 0, len = output.RowCount; i < len; i++) {
                    if (!outputData.TryGetValue(i, out var temp))
                        outputData.Add(i, temp = []);
                    temp.Add(output.GetReadOnlyRow(i));
                }
                inputData[index++] = item.C1;
            }

            var miniBatch = new MiniBatch(rows, this);
            var curr = lap.CreateMatrixFromRows(inputData);
            foreach (var item in outputData.OrderBy(kv => kv.Key)) {
                var output = lap.CreateMatrixFromRows(CollectionsMarshal.AsSpan(item.Value));
                var type = (item.Key == 0)
                    ? MiniBatchSequenceType.SequenceStart
                    : item.Key == (outputData.Count - 1)
                        ? MiniBatchSequenceType.SequenceEnd
                        : MiniBatchSequenceType.Standard
                ;
                miniBatch.Add(type, curr.AsGraphData(), output.AsGraphData());
                curr = output;
            }
            return miniBatch;
        }
    }
}
