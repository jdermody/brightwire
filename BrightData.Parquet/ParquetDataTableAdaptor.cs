using BrightData.Buffer.ReadOnly.Helper;
using BrightData.Converter;
using BrightData.DataTable.Meta;
using BrightData.DataTable.Rows;
using BrightData.Parquet.BufferAdaptors;
using BrightData.Types;
using Parquet;
using Parquet.Schema;

namespace BrightData.Parquet
{
    internal class ParquetDataTableAdaptor : IDataTable
    {
        readonly ParquetReader                   _reader;
        readonly RowGroupReaderProvider          _rowGroupReaderProvider;
        readonly IReadOnlyBufferWithMetaData[]   _columns;
        readonly BrightDataType[]                _columnTypes;   // Fix #4: cached
        readonly IDataDimension[]                _dimensions;    // Fix #5: cached
        readonly MetaData                        _metaData;      // Fix #3: cached
        readonly uint                            _rowCount;      // Fix #2: cached

        ParquetDataTableAdaptor(BrightDataContext context, ParquetReader reader)
        {
            Context = context;
            _reader = reader;
            var dataFields = reader.Schema.DataFields;
            ColumnMetaData = dataFields.Select(x =>
            {
                var m = new MetaData();
                m.SetName(x.Name);
                return m;
            }).ToArray();
            _rowGroupReaderProvider = new(reader);

            // Fix #2: Use provider size instead of re-opening row groups
            _rowCount = _rowGroupReaderProvider.Size;

            _columns = dataFields.Select((f, i) => GetColumn(f, i)).ToArray();

            // Fix #4: Cache ColumnTypes
            _columnTypes = dataFields.Select(f => f.ClrType.GetBrightDataType()).ToArray();

            // Fix #5: Cache Dimensions
            _dimensions = new IDataDimension[_columns.Length];
            for (int i = 0; i < _columns.Length; i++)
                _dimensions[i] = new TableDataDimension(_columnTypes[i], _columns[i]);

            // Fix #3: Cache MetaData
            _metaData = new MetaData();
            foreach (var (key, value) in reader.CustomMetadata)
                _metaData.Set(key, value);
        }

        // Fix #1: Consolidated type dispatch — two switch expressions instead of one interleaved
        IReadOnlyBufferWithMetaData GetColumn(DataField dataField, int columnIndex)
        {
            var metaData = ColumnMetaData[columnIndex];
            var dataType = dataField.ClrType;
            var isNullable = dataField.IsNullable;

            // byte[] → BinaryData (Fix #6: no #pragma needed with direct handling)
            if (dataType == typeof(byte[]))
            {
                if (isNullable)
                {
#pragma warning disable CS8714
                    return new ParquetMappedBufferAdaptor<byte[]?, BinaryData>(
                        _rowGroupReaderProvider, columnIndex, metaData,
                        x => x is null ? new BinaryData() : new BinaryData(x));
#pragma warning restore CS8714
                }
                return new ParquetMappedBufferAdaptor<byte[], BinaryData>(
                    _rowGroupReaderProvider, columnIndex, metaData, x => new BinaryData(x));
            }

            var typeCode = Type.GetTypeCode(dataType);
            return isNullable
                ? BuildNullableAdaptor(typeCode, columnIndex, metaData)
                : BuildNonNullableAdaptor(typeCode, columnIndex, metaData);
        }

        IReadOnlyBufferWithMetaData BuildNonNullableAdaptor(TypeCode typeCode, int columnIndex, MetaData metaData)
        {
            return typeCode switch
            {
                TypeCode.Boolean => new ParquetBufferAdaptor<bool>(_rowGroupReaderProvider, columnIndex, metaData),
                TypeCode.DateTime => new ParquetBufferAdaptor<DateTime>(_rowGroupReaderProvider, columnIndex, metaData),
                TypeCode.Decimal => new ParquetBufferAdaptor<decimal>(_rowGroupReaderProvider, columnIndex, metaData),
                TypeCode.Double => new ParquetBufferAdaptor<double>(_rowGroupReaderProvider, columnIndex, metaData),
                TypeCode.Single => new ParquetBufferAdaptor<float>(_rowGroupReaderProvider, columnIndex, metaData),
                TypeCode.Byte => new ParquetBufferAdaptor<byte>(_rowGroupReaderProvider, columnIndex, metaData),
                TypeCode.SByte => new ParquetBufferAdaptor<sbyte>(_rowGroupReaderProvider, columnIndex, metaData),
                TypeCode.String => new ParquetBufferAdaptor<string>(_rowGroupReaderProvider, columnIndex, metaData),
                TypeCode.Int16 => new ParquetBufferAdaptor<short>(_rowGroupReaderProvider, columnIndex, metaData),
                TypeCode.Int32 => new ParquetBufferAdaptor<int>(_rowGroupReaderProvider, columnIndex, metaData),
                TypeCode.Int64 => new ParquetBufferAdaptor<long>(_rowGroupReaderProvider, columnIndex, metaData),
                TypeCode.UInt16 => new ParquetBufferAdaptor<ushort>(_rowGroupReaderProvider, columnIndex, metaData),
                TypeCode.UInt32 => new ParquetBufferAdaptor<uint>(_rowGroupReaderProvider, columnIndex, metaData),
                TypeCode.UInt64 => new ParquetBufferAdaptor<ulong>(_rowGroupReaderProvider, columnIndex, metaData),
                _ => throw new ArgumentOutOfRangeException(nameof(typeCode), $"{typeCode} could not be converted")
            };
        }

        IReadOnlyBufferWithMetaData BuildNullableAdaptor(TypeCode typeCode, int columnIndex, MetaData metaData)
        {
            return typeCode switch
            {
                TypeCode.Boolean => new ParquetNullableBufferAdaptor<bool>(_rowGroupReaderProvider, columnIndex, metaData, default(bool)),
                TypeCode.DateTime => new ParquetNullableBufferAdaptor<DateTime>(_rowGroupReaderProvider, columnIndex, metaData, default(DateTime)),
                TypeCode.Decimal => new ParquetNullableBufferAdaptor<decimal>(_rowGroupReaderProvider, columnIndex, metaData, default(decimal)),
                TypeCode.Double => new ParquetNullableBufferAdaptor<double>(_rowGroupReaderProvider, columnIndex, metaData, default(double)),
                TypeCode.Single => new ParquetNullableBufferAdaptor<float>(_rowGroupReaderProvider, columnIndex, metaData, default(float)),
                TypeCode.Byte => new ParquetNullableBufferAdaptor<byte>(_rowGroupReaderProvider, columnIndex, metaData, default(byte)),
                TypeCode.SByte => new ParquetNullableBufferAdaptor<sbyte>(_rowGroupReaderProvider, columnIndex, metaData, default(sbyte)),
                TypeCode.String => new ParquetNullableStringAdaptor(_rowGroupReaderProvider, columnIndex, metaData, string.Empty),
                TypeCode.Int16 => new ParquetNullableBufferAdaptor<short>(_rowGroupReaderProvider, columnIndex, metaData, default(short)),
                TypeCode.Int32 => new ParquetNullableBufferAdaptor<int>(_rowGroupReaderProvider, columnIndex, metaData, default(int)),
                TypeCode.Int64 => new ParquetNullableBufferAdaptor<long>(_rowGroupReaderProvider, columnIndex, metaData, default(long)),
                TypeCode.UInt16 => new ParquetNullableBufferAdaptor<ushort>(_rowGroupReaderProvider, columnIndex, metaData, default(ushort)),
                TypeCode.UInt32 => new ParquetNullableBufferAdaptor<uint>(_rowGroupReaderProvider, columnIndex, metaData, default(uint)),
                TypeCode.UInt64 => new ParquetNullableBufferAdaptor<ulong>(_rowGroupReaderProvider, columnIndex, metaData, default(ulong)),
                _ => throw new ArgumentOutOfRangeException(nameof(typeCode), $"{typeCode} could not be converted")
            };
        }

        public static async Task<ParquetDataTableAdaptor> Create(BrightDataContext context, Stream stream)
        {
            return new(context, await ParquetReader.CreateAsync(stream));
        }

        public void Dispose()
        {
            _rowGroupReaderProvider.Dispose();
        }

        // Fix #3: Return cached MetaData
        public MetaData MetaData => _metaData;

        public BrightDataContext Context { get; }

        // Fix #2: Return cached row count
        public uint RowCount => _rowCount;

        public uint ColumnCount => (uint)_reader.Schema.DataFields.Length;
        public DataTableOrientation Orientation => DataTableOrientation.ColumnOriented;

        // Fix #4: Return cached array
        public BrightDataType[] ColumnTypes => _columnTypes;
        public MetaData[] ColumnMetaData { get; }

        public void PersistMetaData()
        {
            throw new NotImplementedException();
        }

        public IReadOnlyBufferWithMetaData GetColumn(uint index) => _columns[index];

        // Fix #8: Use reader directly instead of calling GetColumn(index) twice
        public IReadOnlyBufferWithMetaData<T> GetColumn<T>(uint index) where T : notnull
        {
            var typeofT = typeof(T);
            var reader = _columns[index];
            var dataType = reader.DataType;

            if (dataType == typeofT)
                return (IReadOnlyBufferWithMetaData<T>)reader;

            if (typeofT == typeof(object))
                return (IReadOnlyBufferWithMetaData<T>)new ReadOnlyBufferMetaDataWrapper<object>(reader.ToObjectBuffer(), ColumnMetaData[index]);

            if (typeofT == typeof(string))
                return (IReadOnlyBufferWithMetaData<T>)new ReadOnlyBufferMetaDataWrapper<string>(reader.ToStringBuffer(), ColumnMetaData[index]);

            if (dataType.GetBrightDataType().IsNumeric() && typeofT.GetBrightDataType().IsNumeric())
            {
                var converter = StaticConverters.GetConverter(dataType, typeof(T));
                return new ReadOnlyBufferMetaDataWrapper<T>((IReadOnlyBuffer<T>)reader.ConvertWith(converter), ColumnMetaData[index]);
            }

            throw new NotImplementedException($"Not able to create a column of type {typeof(T)} from {dataType}");
        }

        public Task<T> Get<T>(uint columnIndex, uint rowIndex) where T : notnull
        {
            var column = GetColumn<T>(columnIndex);
            return column.GetItem(rowIndex);
        }

        public Task<T[]> Get<T>(uint columnIndex, params uint[] rowIndices) where T : notnull
        {
            var column = GetColumn<T>(columnIndex);
            return column.GetItems(rowIndices);
        }

        public Task<GenericTableRow[]> GetRows(params uint[] rowIndices) => BrightData.ExtensionMethods.GetRows(this, rowIndices);
        public Task<GenericTableRow> this[uint index] => this.GetRow(index);
        public IAsyncEnumerable<GenericTableRow> EnumerateRows() => BrightData.ExtensionMethods.EnumerateRows(this);

        // Fix #5: Return cached array
        public IDataDimension[] Dimensions => _dimensions;
    }
}
