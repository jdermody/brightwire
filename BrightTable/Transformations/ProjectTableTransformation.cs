using System;
using System.Collections.Generic;
using System.Text;
using BrightData;
using BrightTable.Builders;

namespace BrightTable.Transformations
{
    class ProjectTableTransformation : TableTransformationBase
    {
        readonly Func<object[], object[]> _projector;
        readonly Predicate<object[]> _filter;

        public ProjectTableTransformation(Func<object[], object[]> projector, Predicate<object[]> filter)
        {
            if (projector == null && filter == null)
                throw new ArgumentException("Must specify one of projector or filter");
            _projector = projector;
            _filter = filter;
        }

        protected override ISingleTypeTableSegment Transform(IColumnOrientedDataTable dataTable, uint index, ISingleTypeTableSegment column)
        {
            throw new NotImplementedException();
        }

        internal override void Transform(IRowOrientedDataTable dataTable, RowOrientedTableBuilder builder)
        {
            var rowIndices = new List<uint>();
            if (_filter != null) {
                dataTable.ForEachRow((row, index) => {
                    if (_filter(row))
                        rowIndices.Add(index);
                });
            } else
                rowIndices.AddRange(dataTable.RowIndices());

            if (_projector != null) {
                var isFirst = true;
                dataTable.ForEachRow(row => {
                    if (_filter == null || _filter(row)) {
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
                    }
                });
            } else
                dataTable.ForEachRow(rowIndices, builder.AddRow);
        }

        internal override IReadOnlyList<(long Position, long EndOfColumnOffset)> Transform(ColumnOrientedTableBuilder builder)
        {
            throw new NotImplementedException();
        }
    }
}
