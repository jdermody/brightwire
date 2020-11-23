using System;
using System.Linq;
using BrightData;
using BrightTable;

namespace BrightWire.ExecutionGraph.DataTableAdaptor
{
    /// <summary>
    /// Adapts data tables that classify each step of a sequence
    /// </summary>
    class SequentialDataTableAdaptor : RowBasedDataTableAdaptorBase
    {
        readonly uint[] _rowDepth;

	    public SequentialDataTableAdaptor(ILinearAlgebraProvider lap, IRowOrientedDataTable dataTable) : base(lap, dataTable)
        {
            if (_dataColumnIndex.Length > 1)
                throw new NotImplementedException("Sequential datasets not supported with more than one input data column");

            _rowDepth = new uint[dataTable.RowCount];

            Matrix<float> inputMatrix = null, outputMatrix = null;
            dataTable.ForEachRow((row, i) => {
                inputMatrix = (Matrix<float>)row[_dataColumnIndex[0]];
                outputMatrix = (Matrix<float>)row[_dataTargetIndex];
                _rowDepth[i] = inputMatrix.RowCount;
                if (outputMatrix.RowCount != inputMatrix.RowCount)
                    throw new ArgumentException("Rows between input and output data tables do not match");
            });
            InputSize = inputMatrix.ColumnCount;
            OutputSize = outputMatrix.ColumnCount;
        }

        public override IDataSource CloneWith(IRowOrientedDataTable dataTable)
        {
            return new SequentialDataTableAdaptor(_lap, dataTable);
        }

        public override bool IsSequential => true;
        public override uint InputSize { get; }
	    public override uint? OutputSize { get; }
	    public override uint RowCount => (uint)_rowDepth.Length;

        public override IMiniBatch Get(IExecutionContext executionContext, uint[] rows)
        {
            var data = _GetRows(rows)
                .Select(r => ((Matrix<float>)r[0], (Matrix<float>)r[1]))
                .ToArray()
            ;
            return _GetSequentialMiniBatch(rows, data);
        }

        public override uint[][] GetBuckets()
        {
            return _rowDepth
                .Select((r, i) => (r, i))
                .GroupBy(t => t.Item1)
                .Select(g => g.Select(d => (uint)d.Item2).ToArray())
                .ToArray()
            ;
        }
    }
}
