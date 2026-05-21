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
                var column = await GetData(i);
                foreach (var item in column)
                    yield return item;
            }
        }

        public async Task<Array> GetBlock(uint blockIndex)
        {
            var column = await GetData(blockIndex);
            return column;
        }

        public MetaData MetaData { get; } = metaData;

        public async Task ForEachBlock(BlockCallback<T> callback, CancellationToken ct = default)
        {
            for (uint i = 0; i < rowGroupProvider.RowGroupCount; i++)
            {
                var column = await GetData(i, ct);
                callback(column);
            }
        }

        public async Task<ReadOnlyMemory<T>> GetTypedBlock(uint blockIndex)
        {
            var column = await GetData(blockIndex);
            return column;
        }

        public async IAsyncEnumerable<T> EnumerateAllTyped()
        {
            for (uint i = 0; i < rowGroupProvider.RowGroupCount; i++)
            {
                var column = await GetData(i);
                foreach (var item in column)
                    yield return item;
            }
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken ct = default) => EnumerateAllTyped().GetAsyncEnumerator(ct);

        protected virtual async ValueTask<T[]> GetData(uint blockIndex, CancellationToken ct = default)
        {
            return (T[])await rowGroupProvider.GetColumn(typeof(T), blockIndex, columnIndex, ct);
        }
    }
}
