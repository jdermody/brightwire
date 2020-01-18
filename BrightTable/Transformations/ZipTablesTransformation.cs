using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightTable.Builders;

namespace BrightTable.Transformations
{
    class ZipTablesTransformation : TableTransformationBase
    {
        readonly IDataTable[] _dataTables;
        readonly uint _newColumnCount = 0;

        public ZipTablesTransformation(params IColumnOrientedDataTable[] dataTables)
        {
            _dataTables = dataTables;

            var first = dataTables.First();
            _newColumnCount = first.ColumnCount;
            foreach (var item in dataTables.Skip(1)) {
                if (item.RowCount != first.RowCount)
                    throw new Exception("Table rows need to be the same");

                _newColumnCount += item.ColumnCount;
            }
        }

        protected override (uint ColumnCount, uint RowCount) CalculateNewSize(IDataTable dataTable)
        {
            return (_newColumnCount, dataTable.RowCount);
        }

        protected override ISingleTypeTableSegment Transform(IColumnOrientedDataTable dataTable, uint index, ISingleTypeTableSegment column)
        {
            throw new NotImplementedException();
        }

        internal override void Transform(IRowOrientedDataTable dataTable, RowOrientedTableBuilder builder)
        {
            throw new NotImplementedException();
        }

        internal override IReadOnlyList<(long Position, long EndOfColumnOffset)> Transform(ColumnOrientedTableBuilder builder)
        {
            var ret = new List<(long Position, long EndOfColumnOffset)>();
            var first = _dataTables.First();

            builder.WriteHeader(_newColumnCount, first.RowCount);
            foreach(var table in _dataTables) {
                foreach(var column in table.AllColumns()) {
                    var position = builder.Write(column);
                    ret.Add((position, builder.GetCurrentPosition()));
                }
            }
            return ret;
        }
    }
}
