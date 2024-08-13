using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BrightData.Types;

namespace BrightData.Buffer.ReadOnly.Helper
{
    /// <summary>
    /// Extends IReadOnlyBuffer with metadata
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="buffer"></param>
    /// <param name="metaData"></param>
    public class ReadOnlyBufferMetaDataWrapper<T>(IReadOnlyBuffer<T> buffer, MetaData metaData) : IReadOnlyBufferWithMetaData<T> where T : notnull
    {
        /// <inheritdoc />
        public uint Size => buffer.Size;

        /// <inheritdoc />
        public uint[] BlockSizes => buffer.BlockSizes;

        /// <inheritdoc />
        public Type DataType => buffer.DataType;

        /// <inheritdoc />
        public IAsyncEnumerable<object> EnumerateAll()
        {
            return buffer.EnumerateAll();
        }

        /// <inheritdoc />
        public Task<Array> GetBlock(uint blockIndex) => buffer.GetBlock(blockIndex);

        /// <inheritdoc />
        public Task ForEachBlock(BlockCallback<T> callback, CancellationToken ct = default)
        {
            return buffer.ForEachBlock(callback, ct);
        }

        /// <inheritdoc />
        public Task<ReadOnlyMemory<T>> GetTypedBlock(uint blockIndex)
        {
            return buffer.GetTypedBlock(blockIndex);
        }

        /// <inheritdoc />
        public IAsyncEnumerable<T> EnumerateAllTyped()
        {
            return buffer.EnumerateAllTyped();
        }

        /// <inheritdoc />
        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken ct = default)
        {
            return buffer.GetAsyncEnumerator(ct);
        }

        /// <inheritdoc />
        public MetaData MetaData => metaData;
    }
}
