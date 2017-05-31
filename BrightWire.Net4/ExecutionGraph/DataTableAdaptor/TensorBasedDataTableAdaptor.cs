using System.Collections.Generic;
using System.Linq;
using BrightWire.Models;
using BrightWire.ExecutionGraph.Helper;

namespace BrightWire.ExecutionGraph.DataTableAdaptor
{
    /// <summary>
    /// Adapts data tables that classify tensors (volumes)
    /// </summary>
    class TensorBasedDataTableAdaptor : RowBasedDataTableAdaptorBase
    {
        readonly int _inputSize, _outputSize, _rows, _columns, _depth;
        readonly List<(IContext Context, int Rows, int Columns, int Depth)> _processedContext = new List<(IContext, int, int, int)>();

        public TensorBasedDataTableAdaptor(ILinearAlgebraProvider lap, IDataTable dataTable)
            : base(lap, dataTable)
        {
            var firstRow = dataTable.GetRow(0);
            var input = (FloatTensor)firstRow.Data[0];
            var output = (FloatVector)firstRow.Data[1];
            _outputSize = output.Size;
            _inputSize = input.Size;
            _rows = input.RowCount;
            _columns = input.ColumnCount;
            _depth = input.Depth;
        }

        private TensorBasedDataTableAdaptor(ILinearAlgebraProvider lap, IDataTable dataTable, int inputSize, int outputSize, int rows, int columns, int depth) 
            :base(lap, dataTable)
        {
            _inputSize = inputSize;
            _outputSize = outputSize;
            _rows = rows;
            _columns = columns;
            _depth = depth;
        }

        public override IDataSource CloneWith(IDataTable dataTable)
        {
            return new TensorBasedDataTableAdaptor(_lap, dataTable, _inputSize, _outputSize, _rows, _columns, _depth);
        }

        public override bool IsSequential => false;
        public override int InputSize => _inputSize;
        public override int OutputSize => _outputSize;

        public override IMiniBatch Get(IExecutionContext executionContext, IReadOnlyList<int> rows)
        {
            var data = _GetRows(rows)
                .Select(r => (((FloatTensor)r.Data[0]).GetAsRaw(), ((FloatVector)r.Data[1]).Data))
                .ToList()
            ;
            var input = _lap.CreateMatrix(InputSize, data.Count, (x, y) => data[y].Item1[x]);
            var output = OutputSize > 0 ? _lap.CreateMatrix(data.Count, OutputSize, (x, y) => data[x].Item2[y]) : null;
            var tensor = new Tensor4DGraphData(input, _rows, _columns, _depth);
            return new MiniBatch(rows, this, tensor, new MatrixGraphData(output));
        }
    }
}
