using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightTable.Builders;

namespace BrightTable.Transformations
{
    class ConcatTablesTransformation : TableTransformationBase
    {
        readonly IDataTable[] _tables;
        readonly uint _newRowCount = 0;

        public ConcatTablesTransformation(params IRowOrientedDataTable[] tables)
        {
            _tables = tables;

            var first = tables.First();
            foreach (var item in tables.Skip(1)) {
                foreach (var column in first.ColumnTypes.Zip(item.ColumnTypes, (c1, c2) => (Type1: c1, Type2: c2))) {
                    if (column.Type1 != column.Type2)
                        throw new ArgumentException("Column types need to agree across all tables");
                }

                _newRowCount += item.RowCount;
            }
        }

        protected override (uint ColumnCount, uint RowCount) CalculateNewSize(IDataTable dataTable)
        {
            return (_tables.First().ColumnCount, _newRowCount);
        }

        protected override ISingleTypeTableSegment Transform(IColumnOrientedDataTable dataTable, uint index, ISingleTypeTableSegment column)
        {
            throw new NotImplementedException();
            //var (segment, buffer) = _CreateColumn(dataTable.Context, column.SingleType, column.MetaData, _newRowCount);
            //uint ind = 0;
            //foreach (var table in _tables) {
            //    var tableColumn = table.Columns(index).Single();
            //    foreach (var item in tableColumn.Enumerate())
            //        buffer.Set(ind++, item);
            //}

            //buffer.Finalise();
            //return segment;
        }

        internal override void Transform(IRowOrientedDataTable dataTable, RowOrientedTableBuilder builder)
        {
            builder.AddColumnsFrom(dataTable);

            foreach (var table in _tables)
                table.ForEachRow((row, index) => builder.AddRow(row));
        }

        internal override IReadOnlyList<(long Position, long EndOfColumnOffset)> Transform(ColumnOrientedTableBuilder builder)
        {
            throw new NotImplementedException();
        }
    }
}
