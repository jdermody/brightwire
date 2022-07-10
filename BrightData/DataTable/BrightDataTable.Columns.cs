using System;
using System.Collections.Generic;
using System.Linq;
using BrightData.Helper;

namespace BrightData.DataTable
{
    public partial class BrightDataTable
    {
        public BrightDataType[] ColumnTypes { get; }
        public MetaData[] ColumnMetaData => _columnMetaData.Value;
        public MetaData GetColumnMetaData(uint columnIndex) => _metaData.Value[columnIndex+1];
        public IEnumerable<uint> ColumnIndices => _header.ColumnCount.AsRange();
        public IEnumerable<uint> AllOrSpecifiedColumnIndices(uint[]? indices) => (indices is null || indices.Length == 0)
            ? _header.ColumnCount.AsRange()
            : indices;

        public ICanEnumerateDisposable ReadColumn(uint columnIndex) => GetColumnReader(columnIndex, _header.RowCount);
        public ICanEnumerateDisposable<T> ReadColumn<T>(uint columnIndex) where T : notnull => GetColumnReader<T>(columnIndex, _header.RowCount);

        public IEnumerable<ISingleTypeTableSegment> GetAllColumns() => GetColumns(ColumnIndices);
        public IEnumerable<ISingleTypeTableSegment> GetColumns(IEnumerable<uint> columnIndices) => columnIndices.Select(GetColumn);
        public ISingleTypeTableSegment GetColumn(uint columnIndex)
        {
            var brightDataType = ColumnTypes[columnIndex];
            var columnDataType = brightDataType.GetColumnType().Type;
            var dataType = brightDataType.GetDataType();
            return GenericActivator.Create<ISingleTypeTableSegment>(typeof(ColumnSegment<,>).MakeGenericType(columnDataType, dataType),
                Context,
                brightDataType,
                _header.RowCount,
                GetColumnReader(columnIndex, _header.RowCount),
                GetColumnMetaData(columnIndex)
            );
        }
        public IDataTableSegment<T> GetColumn<T>(uint columnIndex) where T : notnull => (IDataTableSegment<T>)GetColumn(columnIndex);

        static void ValidateColumnTypes(Type columnType, Type requestedType)
        {
            if (columnType != requestedType)
                throw new ArgumentException($"Data types do not align - expected {columnType} but received {requestedType}");
        }

        ICanEnumerateDisposable<T> GetColumnReader<T>(uint columnIndex, uint countToRead, Func<uint, uint>? offsetAdjuster = null) where T : notnull
        {
            ref readonly var column = ref _columns[columnIndex];
            var requestedType = typeof(T);
            ValidateColumnTypes(column.DataType.GetDataType(), requestedType);
            return (ICanEnumerateDisposable<T>)GetColumnReader(columnIndex, countToRead, offsetAdjuster);
        }
        ICanEnumerateDisposable[] GetColumnReaders(IEnumerable<uint> columnIndices) => columnIndices.Select(i => GetColumnReader(i, _header.RowCount)).ToArray();
        ICanEnumerateDisposable GetColumnReader(uint columnIndex, uint countToRead, Func<uint, uint>? offsetAdjuster = null)
        {
            ref readonly var column = ref _columns[columnIndex];
            var dataType = column.DataType.GetDataType();
            var offset = _columnOffset[columnIndex];
            if(offsetAdjuster is not null)
                offset += offsetAdjuster(column.DataTypeSize);
            var (columnDataType, _) = column.DataType.GetColumnType();
            var sizeInBytes = countToRead * column.DataTypeSize;
            return (ICanEnumerateDisposable)_getReader.MakeGenericMethod(columnDataType, dataType).Invoke(this, new object[] { columnIndex, offset, sizeInBytes })!;
        }

        ICanEnumerate<T> GetReader<CT, T>(uint columnIndex, uint offset, uint sizeInBytes)
            where T: notnull
            where CT: unmanaged
        {
            var block = _buffer.GetIterator<CT>(offset, sizeInBytes);
            var converter = (IConvertStructsToObjects<CT, T>)_columnConverters.Value[columnIndex];
            return new SequentialColumnReader<CT, T>(block.GetEnumerator(), converter, block);
        }
    }
}
