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
        readonly IDataTableVectoriser _vectoriser;

        public DataTableAdaptor(IDataTable dataTable, IDataTableVectoriser vectoriser = null)
        {
            _dataTable = dataTable;
            _vectoriser = vectoriser ?? dataTable.GetVectoriser(true);
        }

        public int InputSize => _vectoriser.InputSize;
        public int OutputSize => _vectoriser.OutputSize;
        public bool IsSequential => false;
        public int RowCount => _dataTable.RowCount;

        public IReadOnlyList<(float[], float[])> Get(IReadOnlyList<int> rows)
        {
            return _dataTable
                .GetRows(rows)
                .Select(r => (_vectoriser.GetInput(r), _vectoriser.GetOutput(r)))
                .ToList()
            ;
        }

        public IReadOnlyList<(FloatMatrix Input, FloatMatrix Output)> GetSequential(IReadOnlyList<int> rows)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<IReadOnlyList<int>> GetBuckets()
        {
            return new[] {
                Enumerable.Range(0, _dataTable.RowCount).ToList()
            };
        }

        //public IReadOnlyList<IReadOnlyList<(float[], float[])>> GetSequential(IReadOnlyList<int> rows)
        //{
        //    return _dataTable
        //        .GetRows(rows)
        //        .Select(r => r.AllItems
        //            .Select(sr => (_vectoriser.GetInput(sr), _vectoriser.GetOutput(sr)))
        //            .ToList()
        //        )
        //        .ToList()
        //    ;
        //}

        //public string GetOutputLabel(int columnIndex, int vectorIndex)
        //{
        //    return _vectoriser.GetOutputLabel(columnIndex, vectorIndex);
        //}
    }
}
