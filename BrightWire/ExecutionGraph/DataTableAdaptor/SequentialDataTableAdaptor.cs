using System;
using System.Collections.Generic;
using BrightWire.Models;
using System.Linq;
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

            FloatMatrix inputMatrix = null, outputMatrix = null;
            dataTable.ForEachRow((row, i) => {
                inputMatrix = (FloatMatrix)row[_dataColumnIndex[0]];
                outputMatrix = (FloatMatrix)row[_dataTargetIndex];
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

        public override IMiniBatch Get(IExecutionContext executionContext, IReadOnlyList<uint> rows)
        {
            //var data = _GetRows(rows)
            //    .Select(r => ((FloatMatrix)r.Data[0], (FloatMatrix)r.Data[1]))
            //    .ToList()
            //;
            //return _GetSequentialMiniBatch(rows, data);
            throw new NotImplementedException();
        }

        public override IReadOnlyList<IReadOnlyList<uint>> GetBuckets()
        {
            return _rowDepth
                .Select((r, i) => (r, i))
                .GroupBy(t => t.Item1)
                .Select(g => g.Select(d => (uint)d.Item2).ToList())
                .ToList()
            ;
        }
    }
}
