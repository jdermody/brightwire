using BrightData.DataTable.Rows;
using BrightData.Types;
using Parquet;

namespace BrightData.Parquet
{
    internal class ParquetDataTableAdaptor : IDataTable
    {
        readonly ParquetReader _reader;
        uint? _rowCount = null;

        private ParquetDataTableAdaptor(BrightDataContext context, ParquetReader reader)
        {
            Context = context;
            _reader = reader;
            ColumnMetaData = reader.Schema.DataFields.Select(x => {
                var m = new MetaData();
                m.SetName(x.Name);
                return m;
            }).ToArray();
        }

        public async Task<ParquetDataTableAdaptor> Create(BrightDataContext context, Stream stream)
        {
            return new(context, await ParquetReader.CreateAsync(stream));
        }

        public void Dispose()
        {
            _reader.Dispose();
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

        public IReadOnlyBufferWithMetaData GetColumn(uint index)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyBufferWithMetaData<T> GetColumn<T>(uint index) where T : notnull
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

        public Task<GenericTableRow[]> GetRows(params uint[] rowIndices)
        {
            throw new NotImplementedException();
        }

        public GenericTableRow this[uint index] => throw new NotImplementedException();
    }
}
