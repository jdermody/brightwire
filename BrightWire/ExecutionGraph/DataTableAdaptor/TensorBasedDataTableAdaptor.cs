﻿using System.Collections.Generic;
using System.Linq;
using BrightWire.Models;
using BrightWire.ExecutionGraph.Helper;
using BrightTable;

namespace BrightWire.ExecutionGraph.DataTableAdaptor
{
    /// <summary>
    /// Adapts data tables that classify tensors (volumes)
    /// </summary>
    class TensorBasedDataTableAdaptor : RowBasedDataTableAdaptorBase, IVolumeDataSource
    {
        readonly uint _inputSize, _outputSize;

        public TensorBasedDataTableAdaptor(ILinearAlgebraProvider lap, IRowOrientedDataTable dataTable)
            : base(lap, dataTable)
        {
            var firstRow = dataTable.Row(0);
            var input = (FloatTensor)firstRow[(uint)_dataColumnIndex[0]];
            var output = (FloatVector)firstRow[_dataTargetIndex];
            _outputSize = output.Size;
            _inputSize = input.Size;
            Height = input.RowCount;
            Width = input.ColumnCount;
            Depth = input.Depth;
        }

        TensorBasedDataTableAdaptor(ILinearAlgebraProvider lap, IRowOrientedDataTable dataTable, uint inputSize, uint outputSize, uint rows, uint columns, uint depth) 
            :base(lap, dataTable)
        {
            _inputSize = inputSize;
            _outputSize = outputSize;
            Height = rows;
            Width = columns;
            Depth = depth;
        }

        public override IDataSource CloneWith(IRowOrientedDataTable dataTable)
        {
            return new TensorBasedDataTableAdaptor(_lap, dataTable, _inputSize, _outputSize, Height, Width, Depth);
        }

        public override bool IsSequential => false;
        public override uint InputSize => _inputSize;
        public override uint? OutputSize => _outputSize;
        public uint Width { get; }
	    public uint Height { get; }
	    public uint Depth { get; }

	    public override IMiniBatch Get(IExecutionContext executionContext, IReadOnlyList<uint> rows)
        {
            var data = _GetRows(rows)
                .Select(r => (_dataColumnIndex.Select(i => ((FloatTensor)r[i]).GetAsRaw()).ToList(), ((FloatVector)r[_dataTargetIndex]).Data))
                .ToArray()
            ;
            var inputList = new List<IGraphData>();
            for (var i = 0; i < _dataColumnIndex.Length; i++) {
	            var i1 = i;
	            var input = _lap.CreateMatrix((uint)InputSize, (uint)data.Length, (x, y) => data[y].Item1[i1][x]);
                var tensor = new Tensor4DGraphData(input, Height, Width, Depth);
                inputList.Add(tensor);
            }
            var output = OutputSize > 0 ? _lap.CreateMatrix((uint)data.Length, (uint)OutputSize, (x, y) => data[x].Item2[y]) : null;
            
            return new MiniBatch(rows, this, inputList, new MatrixGraphData(output));
        }
    }
}
