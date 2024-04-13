using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parquet;
using Parquet.Data;

namespace BrightData.Parquet
{
    internal class RowGroupReaderProvider : IDisposable
    {
        readonly ParquetRowGroupReader[]                            _rowGroupReaders;
        readonly ConcurrentDictionary<uint, Lazy<Task<DataColumn>>> _columnData = new();

        public RowGroupReaderProvider(ParquetReader reader)
        {
            Reader = reader;
            var rowGroups = reader.RowGroupCount;
            _rowGroupReaders = new ParquetRowGroupReader[rowGroups];
            RowGroupSizes = new uint[rowGroups];
            for (var i = 0; i < rowGroups; i++) {
                var rowGroupReader = _rowGroupReaders[i] = reader.OpenRowGroupReader(i);
                var rowCount = RowGroupSizes[i] = (uint)rowGroupReader.RowCount;
                Size += rowCount;
            }
        }

        public ParquetReader Reader { get; }
        public uint[] RowGroupSizes { get; }
        public uint Size { get; }
        public uint RowGroupCount => (uint)RowGroupSizes.Length;

        public ParquetRowGroupReader GetRowGroupReader(uint rowGroup) => _rowGroupReaders[rowGroup];

        public Task<DataColumn> GetColumn(uint rowGroup, int columnIndex)
        {
            var rowGroupReader = GetRowGroupReader(rowGroup);
            return _columnData.GetOrAdd((uint)columnIndex, _ => new(() => rowGroupReader.ReadColumnAsync(Reader.Schema.DataFields[columnIndex]))).Value;
        }

        public void Dispose()
        {
            foreach (var item in _rowGroupReaders) { 
                item.Dispose();
            }
        }
    }
}
