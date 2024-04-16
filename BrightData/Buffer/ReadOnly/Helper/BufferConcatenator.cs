﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BrightData.Types;

namespace BrightData.Buffer.ReadOnly.Helper
{
    /// <summary>
    /// Concatenates multiple buffers into one single buffer
    /// </summary>
    /// <typeparam name="T"></typeparam>
    //internal class BufferConcatenator<T> : TypedBufferBase<T>, IReadOnlyBufferWithMetaData<T> where T : notnull
    //{
    //    readonly IReadOnlyBufferWithMetaData<T>[] _buffers;

    //    public BufferConcatenator(params IReadOnlyBufferWithMetaData<T>[] buffers)
    //    {
    //        _buffers = buffers;
    //        var first = buffers.First();
    //        var size = first.Size;
    //        var blockCount = first.BlockCount;
    //        MetaData = first.MetaData;
    //        foreach (var buffer in buffers.Skip(1))
    //        {
    //            if (first.BlockSize != buffer.BlockSize)
    //                throw new ArgumentException("All buffer block sizes must be the same");
    //            size += buffer.Size;
    //            blockCount += buffer.BlockCount;
    //        }
    //        Size = size;
    //        BlockSize = first.BlockSize;
    //        BlockCount = blockCount;
    //        DataType = typeof(T);
    //    }

    //    public uint Size { get; }
    //    public uint BlockSize { get; }
    //    public uint BlockCount { get; }
    //    public Type DataType { get; }
    //    public MetaData MetaData { get; }

    //    public override async IAsyncEnumerable<object> EnumerateAll()
    //    {
    //        foreach (var buffer in _buffers)
    //        {
    //            await foreach (var item in buffer.EnumerateAll())
    //                yield return item;
    //        }
    //    }

    //    public async Task ForEachBlock(BlockCallback<T> callback, INotifyOperationProgress? notify = null, string? message = null, CancellationToken ct = default)
    //    {
    //        foreach (var buffer in _buffers)
    //            await buffer.ForEachBlock(callback, notify, message, ct);
    //    }

    //    public override Task<ReadOnlyMemory<T>> GetTypedBlock(uint blockIndex)
    //    {
    //        uint curr = 0;
    //        foreach (var buffer in _buffers)
    //        {
    //            if (blockIndex < curr + buffer.BlockCount)
    //                return buffer.GetTypedBlock(curr - blockIndex);
    //            curr += buffer.BlockCount;
    //        }
    //        throw new Exception("Block not found");
    //    }

    //    public override async IAsyncEnumerable<T> EnumerateAllTyped()
    //    {
    //        foreach (var buffer in _buffers)
    //        {
    //            await foreach (var item in buffer.EnumerateAllTyped())
    //                yield return item;
    //        }
    //    }
    //}
}
