using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightTable.Builders;

namespace BrightTable.Transformations
{
    class SortTableTransformation : TableTransformationBase
    {
        readonly bool _ascending;
        readonly uint _columnIndex;

        public SortTableTransformation(bool ascending, uint columnIndex)
        {
            _ascending = ascending;
            _columnIndex = columnIndex;
        }

        protected override ISingleTypeTableSegment Transform(IColumnOrientedDataTable dataTable, uint index, ISingleTypeTableSegment column)
        {
            throw new NotImplementedException();
        }

        internal override void Transform(IRowOrientedDataTable dataTable, RowOrientedTableBuilder builder)
        {
            var sortData = new List<(object Item, uint RowIndex)>();
            dataTable.ForEachRow((row, rowIndex) => sortData.Add((row[_columnIndex], rowIndex)));
            var sorted = _ascending 
                ? sortData.OrderBy(d => d.Item) 
                : sortData.OrderByDescending(d => d.Item);
            var rowIndices = sortData.Select(d => d.RowIndex).ToList();
            _WriteRows(rowIndices, dataTable, builder);
        }

        internal override IReadOnlyList<(long Position, long EndOfColumnOffset)> Transform(ColumnOrientedTableBuilder builder)
        {
            throw new NotImplementedException();
        }
    }
}
