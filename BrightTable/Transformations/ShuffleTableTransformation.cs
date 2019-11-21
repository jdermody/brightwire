using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightData;
using BrightTable.Builders;

namespace BrightTable.Transformations
{
    public class ShuffleTableTransformation : TableTransformationBase
    {
        readonly int? _randomSeed;

        public ShuffleTableTransformation(int? randomSeed = null)
        {
            _randomSeed = randomSeed;
        }

        protected override ISingleTypeTableSegment Transform(IColumnOrientedDataTable dataTable, uint index, ISingleTypeTableSegment column)
        {
            throw new NotImplementedException();
        }

        internal override void Transform(IRowOrientedDataTable dataTable, RowOrientedTableBuilder builder)
        {
            var rowIndices = dataTable.RowIndices().Shuffle(_randomSeed).ToList();
            _WriteRows(rowIndices, dataTable, builder);
        }

        internal override IReadOnlyList<(long Position, long EndOfColumnOffset)> Transform(ColumnOrientedTableBuilder builder)
        {
            throw new NotImplementedException();
        }
    }
}
