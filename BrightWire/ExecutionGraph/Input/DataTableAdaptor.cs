using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrightWire.ExecutionGraph.Input
{
    public class DataTableAdaptor : IDataSource
    {
        readonly IDataTable _dataTable;
        readonly uint[] _rowDepth;
        readonly bool _isSequential;
        readonly IDataTableVectoriser _vectoriser;

        public DataTableAdaptor(IDataTable dataTable, IDataTableVectoriser vectoriser = null)
        {
            _dataTable = dataTable;
            _rowDepth = dataTable.GetRowDepths();
            _isSequential = _rowDepth.Any(d => d > 1);
            _vectoriser = vectoriser ?? dataTable.GetVectoriser(true);
        }

        public int InputSize { get { return _vectoriser.InputSize; } }
        public int OutputSize { get { return _vectoriser.OutputSize; } }
        public int RowCount { get { return _dataTable.RowCount; } }
        public bool IsSequential { get { return _isSequential; } }
        public uint[] RowDepth { get { return _rowDepth; } }

        public IReadOnlyList<(float[], float[])> Get(IReadOnlyList<int> rows)
        {
            return _dataTable
                .GetRows(rows)
                .Select(r => (_vectoriser.GetInput(r), _vectoriser.GetOutput(r)))
                .ToList()
            ;
        }

        public IReadOnlyList<IReadOnlyList<(float[], float[])>> GetSequential(IReadOnlyList<int> rows)
        {
            return _dataTable
                .GetRows(rows)
                .Select(r => r.SubItem
                    .Select(sr => (_vectoriser.GetInput(sr), _vectoriser.GetOutput(sr)))
                    .ToList()
                )
                .ToList()
            ;
        }

        public string GetOutputLabel(int columnIndex, int vectorIndex)
        {
            return _vectoriser.GetOutputLabel(columnIndex, vectorIndex);
        }
    }
}
