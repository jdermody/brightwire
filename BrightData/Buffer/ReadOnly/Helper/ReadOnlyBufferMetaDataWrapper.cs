using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    internal class ReadOnlyBufferMetaDataWrapper<T>(IReadOnlyBuffer<T> buffer, MetaData metaData) : IReadOnlyBufferWithMetaData<T> where T : notnull
    {
        public uint Size => buffer.Size;

        public uint BlockSize => buffer.BlockSize;

        public uint BlockCount => buffer.BlockCount;

        public Type DataType => buffer.DataType;

        public IAsyncEnumerable<object> EnumerateAll()
        {
            return buffer.EnumerateAll();
        }

        public Task<Array> GetBlock(uint blockIndex) => buffer.GetBlock(blockIndex);

        public Task ForEachBlock(BlockCallback<T> callback, INotifyOperationProgress? notify = null, string? message = null, CancellationToken ct = default)
        {
            return buffer.ForEachBlock(callback, notify, message, ct);
        }

        public Task<ReadOnlyMemory<T>> GetTypedBlock(uint blockIndex)
        {
            return buffer.GetTypedBlock(blockIndex);
        }

        public IAsyncEnumerable<T> EnumerateAllTyped()
        {
            return buffer.EnumerateAllTyped();
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken ct = default)
        {
            return buffer.GetAsyncEnumerator(ct);
        }

        public MetaData MetaData => metaData;
    }
}
