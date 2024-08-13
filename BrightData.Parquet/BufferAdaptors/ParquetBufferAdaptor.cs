using BrightData.Types;
using Parquet.Data;

namespace BrightData.Parquet.BufferAdaptors
{
    internal class ParquetBufferAdaptor<T>(RowGroupReaderProvider rowGroupProvider, int columnIndex, MetaData metaData) : IReadOnlyBufferWithMetaData<T>
        where T : notnull
    {
        public uint Size => rowGroupProvider.Size;
        public uint[] BlockSizes => rowGroupProvider.RowGroupSizes;
        public Type DataType => typeof(T);

        public async IAsyncEnumerable<object> EnumerateAll()
        {
            for (uint i = 0; i < rowGroupProvider.RowGroupCount; i++)
            {
                var column = await rowGroupProvider.GetColumn(i, columnIndex);
                foreach (var item in GetArray(column))
                    yield return item;
            }
        }

        public async Task<Array> GetBlock(uint blockIndex)
        {
            var column = await rowGroupProvider.GetColumn(blockIndex, columnIndex);
            return GetArray(column);
        }

        public MetaData MetaData { get; } = metaData;

        public async Task ForEachBlock(BlockCallback<T> callback, CancellationToken ct = default)
        {
            for (uint i = 0; i < rowGroupProvider.RowGroupCount; i++)
            {
                var column = await rowGroupProvider.GetColumn(i, columnIndex);
                var data = GetArray(column);
                callback(data);
            }
        }

        public async Task<ReadOnlyMemory<T>> GetTypedBlock(uint blockIndex)
        {
            var column = await rowGroupProvider.GetColumn(blockIndex, columnIndex);
            var data = GetArray(column);
            return data;
        }

        public async IAsyncEnumerable<T> EnumerateAllTyped()
        {
            for (uint i = 0; i < rowGroupProvider.RowGroupCount; i++)
            {
                var column = await rowGroupProvider.GetColumn(i, columnIndex);
                var data = GetArray(column);
                foreach (var item in data)
                    yield return item;
            }
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken ct = default) => EnumerateAllTyped().GetAsyncEnumerator(ct);

        protected virtual T[] GetArray(DataColumn column) => (T[])column.Data;
    }
}
