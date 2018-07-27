using System;
using System.Collections.Generic;
using BrightWire.Models;
using System.Linq;

namespace BrightWire.ExecutionGraph.DataTableAdaptor
{
    /// <summary>
    /// Adapts data tables that classify each step of a sequence
    /// </summary>
    class SequentialDataTableAdaptor : RowBasedDataTableAdaptorBase
    {
        readonly int[] _rowDepth;

	    public SequentialDataTableAdaptor(ILinearAlgebraProvider lap, IDataTable dataTable) : base(lap, dataTable)
        {
            if (_dataColumnIndex.Length > 1)
                throw new NotImplementedException("Sequential datasets not supported with more than one input data column");

            _rowDepth = new int[dataTable.RowCount];

            FloatMatrix inputMatrix = null, outputMatrix = null;
            dataTable.ForEach((row, i) => {
                inputMatrix = row.GetField<FloatMatrix>(_dataColumnIndex[0]);
                outputMatrix = row.GetField<FloatMatrix>(_dataTargetIndex);
                _rowDepth[i] = inputMatrix.RowCount;
                if (outputMatrix.RowCount != inputMatrix.RowCount)
                    throw new ArgumentException("Rows between input and output data tables do not match");
            });
            InputSize = inputMatrix.ColumnCount;
            OutputSize = outputMatrix.ColumnCount;
        }

        public override IDataSource CloneWith(IDataTable dataTable)
        {
            return new SequentialDataTableAdaptor(_lap, dataTable);
        }

        public override bool IsSequential => true;
        public override int InputSize { get; }
	    public override int OutputSize { get; }
	    public override int RowCount => _rowDepth.Length;

        public override IMiniBatch Get(IExecutionContext executionContext, IReadOnlyList<int> rows)
        {
            var data = _GetRows(rows)
                .Select(r => ((FloatMatrix)r.Data[0], (FloatMatrix)r.Data[1]))
                .ToList()
            ;
            return _GetSequentialMiniBatch(rows, data);
        }

        public override IReadOnlyList<IReadOnlyList<int>> GetBuckets()
        {
            return _rowDepth
                .Select((r, i) => (r, i))
                .GroupBy(t => t.Item1)
                .Select(g => g.Select(d => d.Item2).ToList())
                .ToList()
            ;
        }
    }
}
