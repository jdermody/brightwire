using System;
using System.Collections.Generic;
using System.Text;
using BrightTable.Builders;

namespace BrightTable.Transformations
{
    class RowSubsetTransformation : TableTransformationBase
    {
        public readonly uint[] _rowIndices;

        public RowSubsetTransformation(uint start, uint count)
        {
            var list = new List<uint>();
            for (uint i = start; i < count; i++)
                list.Add(i);
            _rowIndices = list.ToArray();
        }

        public RowSubsetTransformation(params uint[] rowIndices)
        {
            _rowIndices = rowIndices;
        }

        protected override ISingleTypeTableSegment Transform(IColumnOrientedDataTable dataTable, uint index, ISingleTypeTableSegment column)
        {
            throw new NotImplementedException();
        }

        internal override void Transform(IRowOrientedDataTable dataTable, RowOrientedTableBuilder builder)
        {
            _WriteRows(_rowIndices, dataTable, builder);
        }

        internal override IReadOnlyList<(long Position, long EndOfColumnOffset)> Transform(ColumnOrientedTableBuilder builder)
        {
            throw new NotImplementedException();
        }
    }
}
