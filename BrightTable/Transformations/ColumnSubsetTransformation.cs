using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightTable.Builders;

namespace BrightTable.Transformations
{
    class ColumnSubsetTransformation : TableTransformationBase
    {
        readonly HashSet<uint> _columnIndices;

        public ColumnSubsetTransformation(params uint[] columnIndices)
        {
            _columnIndices = new HashSet<uint>(columnIndices);
        }

        protected override (uint ColumnCount, uint RowCount) CalculateNewSize(IDataTable dataTable)
        {
            return ((uint)_columnIndices.Count, dataTable.RowCount);
        }

        protected override ISingleTypeTableSegment Transform(IColumnOrientedDataTable dataTable, uint index, ISingleTypeTableSegment column)
        {
            return _columnIndices.Contains(index) ? column : null;
        }

        internal override void Transform(IRowOrientedDataTable dataTable, RowOrientedTableBuilder builder)
        {
            throw new NotImplementedException();
            //var orderedIndices = _columnIndices.OrderBy(i => i).ToList();
            //var types = dataTable.ColumnTypes;
            //foreach (var index in orderedIndices) {
            //    var type = types[(int)index];
            //    var metadata = dataTable.ColumnMetaData(index).Single();
            //    builder.AddColumn(type, metadata);
            //}

            //dataTable.ForEachRow((data, index) => builder.AddRow(orderedIndices.Select(i => data[i])));
        }

        internal override IReadOnlyList<(long Position, long EndOfColumnOffset)> Transform(ColumnOrientedTableBuilder builder)
        {
            throw new System.NotImplementedException();
        }
    }
}
