using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightData;
using BrightTable.Buffers;
using BrightTable.Builders;
using BrightTable.Segments;

namespace BrightTable.Transformations
{
    abstract class TableTransformationBase : ITransformColumnOrientedDataTable, ITransformRowOrientedDataTable
    {
        protected virtual (uint ColumnCount, uint RowCount) CalculateNewSize(IDataTable dataTable) => (dataTable.ColumnCount, dataTable.RowCount);
        protected abstract ISingleTypeTableSegment Transform(IColumnOrientedDataTable dataTable, uint index, ISingleTypeTableSegment column);
        internal abstract void Transform(IRowOrientedDataTable dataTable, RowOrientedTableBuilder builder);
        internal abstract IReadOnlyList<(long Position, long EndOfColumnOffset)> Transform(ColumnOrientedTableBuilder builder);

        public IColumnOrientedDataTable Transform(IBrightDataContext context, string filePath = null)
        {
            using (var builder = new ColumnOrientedTableBuilder(filePath)) {
                var columnOffsets = Transform(builder);
                builder.WriteColumnOffsets(columnOffsets);
                return builder.Build(context);
            }
        }

        public IColumnOrientedDataTable Transform(IColumnOrientedDataTable dataTable, string filePath = null)
        {
            var (columns, rows) = CalculateNewSize(dataTable);
            var columnOffsets = new List<(long Position, long EndOfColumnOffset)>();
            using var builder = new ColumnOrientedTableBuilder(filePath);

            builder.WriteHeader(columns, rows);
            for(uint i = 0; i < dataTable.ColumnCount; i++) {
                var column = dataTable.Columns(i).Single();
                var transformed = Transform(dataTable, i, column);
                if (transformed != null) {
                    var position = builder.Write(transformed);
                    columnOffsets.Add((position, builder.GetCurrentPosition()));
                }
            }
            builder.WriteColumnOffsets(columnOffsets);
            return builder.Build(dataTable.Context);
        }

        public IRowOrientedDataTable Transform(IRowOrientedDataTable dataTable, string filePath = null)
        {
            var (columns, rows) = CalculateNewSize(dataTable);
            using var builder = new RowOrientedTableBuilder(rows, filePath);
            Transform(dataTable, builder);
            return builder.Build(dataTable.Context);
        }

        protected (ISingleTypeTableSegment Segment, IEditableBuffer Buffer) _CreateColumn(IBrightDataContext context, ColumnType columnType, IMetaData metadata, uint rowCount)
        {
            var type = typeof(InMemoryBuffer<>).MakeGenericType(columnType.GetColumnType());
            var ret = Activator.CreateInstance(type, context, columnType, metadata, rowCount);
            return ((ISingleTypeTableSegment) ret, (IEditableBuffer) ret);
        }

        protected void _Process(ISingleTypeTableSegment column, Action<uint, object> callback)
        {
            uint index = 0;
            foreach(var item in column.Enumerate())
                callback(index++, item);
        }

        protected void _Process<T>(IDataTableSegment<T> column, Action<uint, T> callback)
        {
            uint index = 0;
            foreach(var item in column.EnumerateTyped())
                callback(index++, item);
        }

        internal void _WriteRows(IEnumerable<uint> rowIndices, IRowOrientedDataTable dataTable, RowOrientedTableBuilder builder)
        {
            builder.AddColumnsFrom(dataTable);
            dataTable.ForEachRow(rowIndices, builder.AddRow);
        }
    }
}
