using System;
using System.Collections.Generic;
using System.Text;
using BrightWire.Models;
using System.Linq;

namespace BrightWire.ExecutionGraph.Input
{
    class SequentialDataTableAdaptor : DataTableAdaptorBase
    {
        readonly int[] _rowDepth;
        readonly int _inputSize, _outputSize;

        public SequentialDataTableAdaptor(ILinearAlgebraProvider lap, IDataTable dataTable) : base(lap, dataTable)
        {
            _rowDepth = new int[dataTable.RowCount];
            FloatMatrix inputMatrix = null, outputMatrix = null;
            dataTable.ForEach((row, i) => {
                inputMatrix = row.GetField<FloatMatrix>(0);
                outputMatrix = row.GetField<FloatMatrix>(1);
                _rowDepth[i] = inputMatrix.RowCount;
                if (outputMatrix.RowCount != inputMatrix.RowCount)
                    throw new ArgumentException("Rows between input and output data tables do not match");
            });
            _inputSize = inputMatrix.ColumnCount;
            _outputSize = outputMatrix.ColumnCount;
        }

        public override bool IsSequential => true;
        public override int InputSize => _inputSize;
        public override int OutputSize => _outputSize;
        public override int RowCount => _rowDepth.Length;

        public override IMiniBatch Get(IReadOnlyList<int> rows)
        {
            var data = _dataTable
                .GetRows(rows)
                .Select(r => (r.GetField<FloatMatrix>(0), r.GetField<FloatMatrix>(1)))
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
