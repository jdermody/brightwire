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

        public void PersistMetaData()
        {
            throw new NotImplementedException();
        }

        public Table.IReadOnlyBuffer GetColumn(uint index)
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

        public MetaData MetaData => _metaData.Value;
        public T ConvertObjectTo<T>(uint index, object ret) where T : notnull
        {
            throw new NotImplementedException();
        }
    }
}