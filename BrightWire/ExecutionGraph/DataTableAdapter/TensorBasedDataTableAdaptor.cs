using System.Collections.Generic;
using System.Linq;
using BrightData;
using BrightData.LinearAlgebra;
using BrightWire.ExecutionGraph.Helper;
using Microsoft.Toolkit.HighPerformance.Buffers;
using BrightDataTable = BrightData.DataTable.BrightDataTable;

namespace BrightWire.ExecutionGraph.DataTableAdapter
{
    /// <summary>
    /// Adapts data tables that classify tensors (volumes)
    /// </summary>
    internal class TensorBasedDataTableAdapter : RowBasedDataTableAdapterBase, IVolumeDataSource
    {
        readonly uint[] _featureColumns;
        readonly uint _inputSize, _outputSize, _inputColumnIndex;

        public TensorBasedDataTableAdapter(BrightDataTable dataTable, uint[] featureColumns)
            : base(dataTable, featureColumns)
        {
            _featureColumns = featureColumns;
            var firstRow = dataTable.GetRow(0);
            var input = (IReadOnlyTensor3D)firstRow[_inputColumnIndex = _featureColumnIndices[0]];
            var output = (IReadOnlyVector)firstRow[_targetColumnIndex];
            _outputSize = output.Size;
            _inputSize = input.Segment.Size;
            Height = input.RowCount;
            Width = input.ColumnCount;
            Depth = input.Depth;
        }

        TensorBasedDataTableAdapter(BrightDataTable dataTable, uint inputSize, uint outputSize, uint rows, uint columns, uint depth, uint[] featureColumns) 
            : base(dataTable, featureColumns)
        {
            _inputSize = inputSize;
            _outputSize = outputSize;
            Height = rows;
            Width = columns;
            Depth = depth;
            _featureColumns = featureColumns;
        }

        public override IDataSource CloneWith(BrightDataTable dataTable)
        {
            return new TensorBasedDataTableAdapter(dataTable, _inputSize, _outputSize, Height, Width, Depth, _featureColumns);
        }

        public override uint InputSize => _inputSize;
        public override uint? OutputSize => _outputSize;
        public uint Width { get; }
	    public uint Height { get; }
	    public uint Depth { get; }

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
                inputRowPtr[index] = GetSegment(rowIndex, _inputColumnIndex, row, lap);
                targetRowsPtr[index] = GetSegment(rowIndex, _targetColumnIndex, row, lap);
                ++index;
            }

            var input = lap.CreateMatrixFromColumns(inputRowPtr);
            var output = OutputSize > 0 
                ? lap.CreateMatrixFromRows(targetRowsPtr).AsGraphData()
                : null;
            
            return new MiniBatch(rows, this, new Tensor4DGraphData(input, Height, Width, Depth), output);
        }
    }
}
