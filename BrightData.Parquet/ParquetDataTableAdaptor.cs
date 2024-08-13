using BrightData.Buffer.ReadOnly.Helper;
using BrightData.Converter;
using BrightData.DataTable.Helper;
using BrightData.DataTable.Rows;
using BrightData.Helper;
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
        uint?                                    _rowCount = null;
        

        private ParquetDataTableAdaptor(BrightDataContext context, ParquetReader reader)
        {
            Context = context;
            _reader = reader;
            var dataFields = reader.Schema.DataFields;
            ColumnMetaData = dataFields.Select(x => {
                var m = new MetaData();
                m.SetName(x.Name);
                return m;
            }).ToArray();
            _rowGroupReaderProvider = new(reader);
            _columns = dataFields.Select(GetColumn).ToArray();
        }

        IReadOnlyBufferWithMetaData GetColumn(DataField dataField, int columnIndex)
        {
            var metaData = ColumnMetaData[columnIndex];
            var dataType = dataField.ClrType;
            var isNullable = dataField.IsNullable;

            if (dataType == typeof(byte[])) {
                return isNullable
                    ? (IReadOnlyBufferWithMetaData)new ParquetMappedBufferAdaptor<byte[]?, BinaryData>(_rowGroupReaderProvider, columnIndex, metaData, x => x is null ? new BinaryData() : new BinaryData(x))
                    : new ParquetMappedBufferAdaptor<byte[], BinaryData>(_rowGroupReaderProvider, columnIndex, metaData, x => new BinaryData(x))
                ;
            }

            var typeCode = Type.GetTypeCode(dataType);
            return typeCode switch {
                TypeCode.Boolean when isNullable => new ParquetNullableBufferAdaptor<bool>(_rowGroupReaderProvider, columnIndex, metaData, false),
                TypeCode.Boolean => new ParquetBufferAdaptor<bool>(_rowGroupReaderProvider, columnIndex, metaData),

                TypeCode.DateTime when isNullable => new ParquetNullableBufferAdaptor<DateTime>(_rowGroupReaderProvider, columnIndex, metaData, DateTime.MinValue),
                TypeCode.DateTime => new ParquetBufferAdaptor<DateTime>(_rowGroupReaderProvider, columnIndex, metaData),

                TypeCode.Decimal when isNullable => new ParquetNullableBufferAdaptor<decimal>(_rowGroupReaderProvider, columnIndex, metaData, decimal.MinValue),
                TypeCode.Decimal => new ParquetBufferAdaptor<decimal>(_rowGroupReaderProvider, columnIndex, metaData),

                TypeCode.Double when isNullable => new ParquetNullableBufferAdaptor<double>(_rowGroupReaderProvider, columnIndex, metaData, double.MinValue),
                TypeCode.Double => new ParquetBufferAdaptor<double>(_rowGroupReaderProvider, columnIndex, metaData),

                TypeCode.Single when isNullable => new ParquetNullableBufferAdaptor<float>(_rowGroupReaderProvider, columnIndex, metaData, float.MinValue),
                TypeCode.Single => new ParquetBufferAdaptor<float>(_rowGroupReaderProvider, columnIndex, metaData),

                TypeCode.Byte when isNullable => new ParquetNullableBufferAdaptor<byte>(_rowGroupReaderProvider, columnIndex, metaData, byte.MinValue),
                TypeCode.Byte => new ParquetBufferAdaptor<byte>(_rowGroupReaderProvider, columnIndex, metaData),

                TypeCode.String when isNullable => new ParquetNullableStringAdaptor(_rowGroupReaderProvider, columnIndex, metaData, string.Empty),
                TypeCode.String => new ParquetBufferAdaptor<string>(_rowGroupReaderProvider, columnIndex, metaData),

                TypeCode.Int16 when isNullable => new ParquetNullableBufferAdaptor<short>(_rowGroupReaderProvider, columnIndex, metaData, short.MinValue),
                TypeCode.Int16 => new ParquetBufferAdaptor<short>(_rowGroupReaderProvider, columnIndex, metaData),

                TypeCode.Int32 when isNullable => new ParquetNullableBufferAdaptor<int>(_rowGroupReaderProvider, columnIndex, metaData, int.MinValue),
                TypeCode.Int32 => new ParquetBufferAdaptor<int>(_rowGroupReaderProvider, columnIndex, metaData),

                TypeCode.Int64 when isNullable => new ParquetNullableBufferAdaptor<long>(_rowGroupReaderProvider, columnIndex, metaData, long.MinValue),
                TypeCode.Int64 => new ParquetBufferAdaptor<long>(_rowGroupReaderProvider, columnIndex, metaData),

                TypeCode.UInt16 when isNullable => new ParquetNullableBufferAdaptor<ushort>(_rowGroupReaderProvider, columnIndex, metaData, ushort.MinValue),
                TypeCode.UInt16 => new ParquetBufferAdaptor<ushort>(_rowGroupReaderProvider, columnIndex, metaData),

                TypeCode.UInt32 when isNullable => new ParquetNullableBufferAdaptor<uint>(_rowGroupReaderProvider, columnIndex, metaData, uint.MinValue),
                TypeCode.UInt32 => new ParquetBufferAdaptor<uint>(_rowGroupReaderProvider, columnIndex, metaData),

                TypeCode.UInt64 when isNullable => new ParquetNullableBufferAdaptor<ulong>(_rowGroupReaderProvider, columnIndex, metaData, ulong.MinValue),
                TypeCode.UInt64 => new ParquetBufferAdaptor<ulong>(_rowGroupReaderProvider, columnIndex, metaData),

                _ => throw new ArgumentOutOfRangeException($"{typeCode} could not be converted")
            };
        }

        public static async Task<ParquetDataTableAdaptor> Create(BrightDataContext context, Stream stream)
        {
            return new(context, await ParquetReader.CreateAsync(stream));
        }

        public void Dispose()
        {
            _reader.Dispose();
            _rowGroupReaderProvider.Dispose();
        }

        public MetaData MetaData
        {
            get
            {
                var ret = new MetaData();
                foreach (var (key, value) in _reader.CustomMetadata)
                    ret.Set(key, value);
                return ret;
            }
        }

        public BrightDataContext Context { get; }

        public uint RowCount
        {
            get
            {
                if (_rowCount is null) {
                    _rowCount = 0;
                    for (var i = 0; i < _reader.RowGroupCount; i++) {
                        using var rowGroupReader = _reader.OpenRowGroupReader(i);
                        _rowCount += (uint)rowGroupReader.RowCount;
                    }
                }
                return _rowCount.Value;
            }
        }
        public uint ColumnCount => (uint)_reader.Schema.DataFields.Length;
        public DataTableOrientation Orientation => DataTableOrientation.ColumnOriented;
        public BrightDataType[] ColumnTypes => _reader.Schema.DataFields.Select(x => x.ClrType.GetBrightDataType()).ToArray();
        public MetaData[] ColumnMetaData { get; }

        public void PersistMetaData()
        {
            throw new NotImplementedException();
        }

        public IReadOnlyBufferWithMetaData GetColumn(uint index) => _columns[index];

        public IReadOnlyBufferWithMetaData<T> GetColumn<T>(uint index) where T : notnull
        {
            var typeofT = typeof(T);
            var reader = _columns[index];
            var dataType = reader.DataType;

            if(dataType == typeofT)
                return (IReadOnlyBufferWithMetaData<T>)reader;

            if (typeofT == typeof(object))
                return (IReadOnlyBufferWithMetaData<T>)new ReadOnlyBufferMetaDataWrapper<object>(GetColumn(index).ToObjectBuffer(), ColumnMetaData[index]);

            if (typeofT == typeof(string)) {
                var ret = new ReadOnlyBufferMetaDataWrapper<string>(GetColumn(index).ToStringBuffer(), ColumnMetaData[index]);
                return (IReadOnlyBufferWithMetaData<T>)ret;
            }

            if (dataType.GetBrightDataType().IsNumeric() && typeofT.GetBrightDataType().IsNumeric()) {
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

        public IDataDimension[] Dimensions
        {
            get
            {
                var len = _columns.Length;
                var ret = new IDataDimension[len];
                for (var i = 0; i < len; i++)
                    ret[i] = new TableDataDimension(ColumnTypes[i], _columns[i]);
                return ret;
            }
        }
    }
}
