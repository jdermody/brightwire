using BrightData.Table;
using Parquet;
using Parquet.Schema;

namespace BrightData.Parquet
{
    internal class ParquetDataTable : IDataTable
    {
        record RowGroup(uint Index, uint StartIndex, uint EndIndex);
        readonly ParquetReader _reader;
        readonly DataField[] _fields;
        readonly RowGroup[] _rowGroups;
        readonly Lazy<MetaData> _metaData;

        public ParquetDataTable(ParquetReader reader)
        {
            _reader = reader;
            _fields = _reader.Schema.DataFields;
            ColumnCount = (uint)_fields.Length;
            ColumnTypes = _fields.Select(x => x.ClrType.GetBrightDataType()).ToArray();
            _rowGroups = new RowGroup[reader.RowGroupCount];

            uint offset = 0;
            for(var i = 0; i < reader.RowGroupCount; i++) {
                using var rowGroupReader = reader.OpenRowGroupReader(i);
                var size = (uint)rowGroupReader.RowCount;
                _rowGroups[i] = new RowGroup((uint)i, offset, offset + size);
                RowCount += size;
                offset += size;
            }

            _metaData = new(() => {
                var ret = new MetaData();
                foreach (var item in reader.CustomMetadata)
                    ret.Set(item.Key, item.Value);
                return ret;
            });
        }

        public static async Task<ParquetDataTable> Create(Stream parquetData)
        {
            var reader = await ParquetReader.CreateAsync(parquetData);
            return new ParquetDataTable(reader);
        }

        public void Dispose()
        {
            _reader.Dispose();
        }

        public uint RowCount { get; }
        public uint ColumnCount { get; }
        public DataTableOrientation Orientation => DataTableOrientation.ColumnOriented;
        public BrightDataType[] ColumnTypes { get; }
        public MetaData[] ColumnMetaData { get; }
        public MetaData MetaData => _metaData.Value;

        public void PersistMetaData()
        {
            throw new NotImplementedException();
        }

        public IReadOnlyBufferWithMetaData GetColumn(uint index)
        {
            throw new NotImplementedException();
        }

        IReadOnlyBufferWithMetaData<T> IDataTable.GetColumn<T>(uint index)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyBuffer<T> GetColumn<T>(uint index) where T : notnull
        {
            throw new NotImplementedException();
        }

        public Task<MetaData[]> GetColumnAnalysis(params uint[] columnIndices)
        {
            throw new NotImplementedException();
        }

        public void SetTensorData(ITensorDataProvider dataProvider)
        {
            throw new NotImplementedException();
        }

        public Task<T> Get<T>(uint columnIndex, uint rowIndex) where T : notnull
        {
            throw new NotImplementedException();
        }

        public Task<T[]> Get<T>(uint columnIndex, params uint[] rowIndices) where T : notnull
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<TableRow> EnumerateRows(CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyBufferWithMetaData[] GetColumns(params uint[] columnIndices)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyBufferWithMetaData[] GetColumns(IEnumerable<uint> columnIndices)
        {
            throw new NotImplementedException();
        }

        public Task WriteColumnsTo(Stream stream, params uint[] columnIndices)
        {
            throw new NotImplementedException();
        }

        public Task WriteRowsTo(Stream stream, params uint[] rowIndices)
        {
            throw new NotImplementedException();
        }

        public Task<TableRow[]> GetRows(params uint[] rowIndices)
        {
            throw new NotImplementedException();
        }

        public TableRow this[uint index] => throw new NotImplementedException();

        public ReadOnlyMemory<float> GetTensorData()
        {
            throw new NotImplementedException();
        }

        public BrightDataContext Context { get; }
    }
}