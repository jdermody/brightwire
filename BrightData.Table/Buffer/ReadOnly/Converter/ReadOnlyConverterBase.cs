using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData.Table.Buffer.ReadOnly.Converter
{
    abstract class ReadOnlyConverterBase<FT, TT> : IReadOnlyBuffer<TT>
        where FT : notnull
        where TT : notnull
    {
        readonly IReadOnlyBuffer<FT> _from;

        protected ReadOnlyConverterBase(IReadOnlyBuffer<FT> from)
        {
            _from = from;
        }

        protected abstract void Convert(in FT from, ref TT to);
        protected abstract TT Convert(in FT from);

        public uint Size => _from.Size;
        public uint BlockSize => _from.BlockSize;
        public uint BlockCount => _from.BlockCount;
        public Type DataType => typeof(TT);
        public async IAsyncEnumerable<object> EnumerateAll()
        {
            await foreach (var item in _from.EnumerateAll()) {
                yield return Convert((FT)item);
            }
        }

        public Task ForEachBlock(BlockCallback<TT> callback, INotifyUser? notify = null, string? message = null, CancellationToken ct = default)
        {
            return _from.ForEachBlock(x =>
            {
                using var temp = SpanOwner<TT>.Allocate(x.Length);
                var span = temp.Span;
                for (var i = 0; i < x.Length; i++)
                    Convert(x[i], ref span[i]);
                callback(span);
            }, notify, message, ct);
        }

        public async Task<ReadOnlyMemory<TT>> GetBlock(uint blockIndex)
        {
            var block = await _from.GetBlock(blockIndex);
            var ret = new Memory<TT>(new TT[block.Length]);
            for (var i = 0; i < block.Length; i++)
                Convert(block.Span[i], ref ret.Span[i]);
            return ret;
        }

        public async IAsyncEnumerable<TT> EnumerateAllTyped()
        {
            await foreach (var item in _from.EnumerateAllTyped())
                yield return Convert(item);
        }

        public IAsyncEnumerator<TT> GetAsyncEnumerator(CancellationToken ct = default) => EnumerateAllTyped().GetAsyncEnumerator(ct);
    }
}
