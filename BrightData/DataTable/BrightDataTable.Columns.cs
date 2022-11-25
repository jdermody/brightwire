using System;
using System.Collections.Generic;
using System.Linq;
using BrightData.Helper;

namespace BrightData.DataTable
{
    public partial class BrightDataTable
    {
        /// <summary>
        /// Column data types
        /// </summary>
        public BrightDataType[] ColumnTypes { get; }

        /// <summary>
        /// Column meta data
        /// </summary>
        public MetaData[] ColumnMetaData => _columnMetaData.Value;

        /// <summary>
        /// Gets the meta data for a column
        /// </summary>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        public MetaData GetColumnMetaData(uint columnIndex) => _metaData.Value[columnIndex+1];

        /// <summary>
        /// Enumerates the column indices
        /// </summary>
        public IEnumerable<uint> ColumnIndices => _header.ColumnCount.AsRange();

        /// <summary>
        /// Enumerates specified column indices (or all if none specified)
        /// </summary>
        /// <param name="indices"></param>
        /// <returns></returns>
        public IEnumerable<uint> AllOrSpecifiedColumnIndices(params uint[]? indices) => (indices is null || indices.Length == 0)
            ? _header.ColumnCount.AsRange()
            : indices;

        /// <summary>
        /// Creates a column value enumerator
        /// </summary>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        public ICanEnumerateDisposable ReadColumn(uint columnIndex) => GetColumnReader(columnIndex, _header.RowCount);

        /// <summary>
        /// Returns a type column value enumerator
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        public ICanEnumerateDisposable<T> ReadColumn<T>(uint columnIndex) where T : notnull => GetColumnReader<T>(columnIndex, _header.RowCount);

        /// <summary>
        /// Gets all columns as typed segments
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ITypedSegment> GetAllColumns() => GetColumns(ColumnIndices);


        /// <summary>
        /// Returns specified columns as type segments
        /// </summary>
        /// <param name="columnIndices"></param>
        /// <returns></returns>
        public IEnumerable<ITypedSegment> GetColumns(IEnumerable<uint> columnIndices) => columnIndices.Select(GetColumn);

        /// <summary>
        /// Returns a column as a typed segment
        /// </summary>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        public ITypedSegment GetColumn(uint columnIndex)
        {
            var brightDataType = ColumnTypes[columnIndex];
            var columnDataType = brightDataType.GetColumnType().Type;
            var dataType = brightDataType.GetDataType();
            return GenericActivator.Create<ITypedSegment>(typeof(ColumnSegment<,>).MakeGenericType(columnDataType, dataType),
                Context,
                brightDataType,
                _header.RowCount,
                GetColumnReader(columnIndex, _header.RowCount),
                GetColumnMetaData(columnIndex)
            );
        }

        /// <summary>
        /// Returns a column as a strongly typed segment
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        public ITypedSegment<T> GetColumn<T>(uint columnIndex) where T : notnull => (ITypedSegment<T>)GetColumn(columnIndex);

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
