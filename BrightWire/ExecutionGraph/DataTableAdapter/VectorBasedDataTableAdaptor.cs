using System.Linq;
using System.Numerics;
using BrightData;
using BrightData.LinearAlgebra;
using BrightWire.ExecutionGraph.Helper;
using Microsoft.Toolkit.HighPerformance.Buffers;
using BrightDataTable = BrightData.DataTable.BrightDataTable;

namespace BrightWire.ExecutionGraph.DataTableAdapter
{
    /// <summary>
    /// Segment table adapter for tables with vector data
    /// </summary>
    internal class VectorBasedDataTableAdapter : RowBasedDataTableAdapterBase
    {
        readonly uint[] _featureColumns;
        readonly uint _inputColumnIndex;

        public VectorBasedDataTableAdapter(BrightDataTable dataTable, uint[] featureColumns) 
            : base(dataTable, featureColumns)
        {
            _featureColumns = featureColumns;
            var firstRow = dataTable.GetRow(0);
            var input = (IReadOnlyVector)firstRow[_inputColumnIndex = _featureColumnIndices.Single()];
            var output = (IReadOnlyVector)firstRow[_targetColumnIndex];

            InputSize = input.Size;
            OutputSize = output.Size;
        }

        public override uint InputSize { get; }
	    public override uint? OutputSize { get; }

        public override IMiniBatch Get(uint[] rows)
        {
            var lap = _dataTable.Context.LinearAlgebraProvider;
            using var inputRows = SpanOwner<ITensorSegment>.Allocate(rows.Length);
            using var targetRows = SpanOwner<ITensorSegment>.Allocate(rows.Length);
            var inputRowPtr = inputRows.Span;
            var targetRowsPtr = targetRows.Span;
            var index = 0;

            foreach (var row in GetRows(rows)) {
                var rowIndex = rows[index];
                inputRowPtr[index] = GetSegment(rowIndex, _inputColumnIndex, row);
                targetRowsPtr[index] = GetSegment(rowIndex, _targetColumnIndex, row);
                ++index;
            }

            var input = lap.CreateMatrixFromRows(inputRowPtr);
            var output = OutputSize > 0
                ? lap.CreateMatrixFromRows(targetRowsPtr)
                : null;

            return new MiniBatch(rows, this, input.AsGraphData(), output?.AsGraphData());

            //var data = GetRows(rows)
            //    .Select(r => (((IReadOnlyVector)r[_inputColumnIndex]).ToArray(), ((IReadOnlyVector)r[_targetColumnIndex]).ToArray()))
            //    .ToArray()
            //;
            //return GetMiniBatch(rows, data);
        }

        public override IDataSource CloneWith(BrightDataTable dataTable)
        {
            return new VectorBasedDataTableAdapter(dataTable, _featureColumns);
        }
    }
}
