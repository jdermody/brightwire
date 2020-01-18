using System;
using System.Collections.Generic;
using System.Text;
using BrightData;
using BrightTable.Builders;

namespace BrightTable.Transformations
{
    class ProjectRowsTransformation : TableTransformationBase
    {
        readonly Func<object[], object[]> _projector;

        public ProjectRowsTransformation(Func<object[], object[]> projector)
        {
            _projector = projector;
        }

        protected override ISingleTypeTableSegment Transform(IColumnOrientedDataTable dataTable, uint index, ISingleTypeTableSegment column)
        {
            throw new NotImplementedException();
        }

        internal override void Transform(IRowOrientedDataTable dataTable, RowOrientedTableBuilder builder)
        {
            var isFirst = true;
            dataTable.ForEachRow(row => {
                var projected = _projector(row);
                if (projected != null) {
                    if (isFirst) {
                        isFirst = false;
                        uint index = 0;
                        foreach (var column in projected) {
                            var type = column.GetType().GetColumnType();
                            var metadata = builder.AddColumn(type, $"Column {index}");
                            metadata.Set(Consts.Index, index);
                            ++index;
                        }
                    }

                    builder.AddRow(projected);
                }
            });
        }

        internal override IReadOnlyList<(long Position, long EndOfColumnOffset)> Transform(ColumnOrientedTableBuilder builder)
        {
            throw new NotImplementedException();
        }
    }
}
