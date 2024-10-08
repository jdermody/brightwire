﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData.Buffer.ReadOnly.Converter
{
    /// <summary>
    /// Read only converter base class
    /// </summary>
    /// <typeparam name="FT"></typeparam>
    /// <typeparam name="TT"></typeparam>
    /// <param name="from"></param>
    abstract class ReadOnlyConverterBase<FT, TT>(IReadOnlyBuffer<FT> from) : TypedBufferBase<TT>, IReadOnlyBuffer<TT>
        where FT : notnull
        where TT : notnull
    {
        protected abstract TT Convert(in FT from);

        public uint Size => from.Size;
        public uint[] BlockSizes => from.BlockSizes;
        public Type DataType => typeof(TT);
        public override async IAsyncEnumerable<object> EnumerateAll()
        {
            await foreach (var item in from.EnumerateAll()) {
                yield return Convert((FT)item);
            }
        }

        public Task ForEachBlock(BlockCallback<TT> callback, CancellationToken ct = default)
        {
            return from.ForEachBlock(x =>
            {
                using var temp = SpanOwner<TT>.Allocate(x.Length);
                var span = temp.Span;
                for (var i = 0; i < x.Length; i++)
                    span[i] = Convert(x[i]);
                callback(span);
            }, ct);
        }

        public override async Task<ReadOnlyMemory<TT>> GetTypedBlock(uint blockIndex)
        {
            var block = await from.GetTypedBlock(blockIndex);
            var ret = new Memory<TT>(new TT[block.Length]);
            for (var i = 0; i < block.Length; i++)
                ret.Span[i] = Convert(block.Span[i]);
            return ret;
        }

        public override async IAsyncEnumerable<TT> EnumerateAllTyped()
        {
            await foreach (var item in from.EnumerateAllTyped())
                yield return Convert(item);
        }
    }
}
