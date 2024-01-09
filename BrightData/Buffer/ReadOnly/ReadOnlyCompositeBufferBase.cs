﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData.Buffer.ReadOnly
{
    /// <summary>
    /// Read only composite buffer base class
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal abstract class ReadOnlyCompositeBufferBase<T> : IReadOnlyBuffer<T> where T : notnull
    {
        readonly Stream _stream;
        readonly (long Position, uint Size)[] _blockData;
        readonly SemaphoreSlim _lock = new(1);

        protected ReadOnlyCompositeBufferBase(Stream stream)
        {
            _stream = stream;
            using var reader = new BinaryReader(stream, Encoding.UTF8, true);
            Size = reader.ReadUInt32();
            BlockSize = reader.ReadUInt32();
            BlockCount = reader.ReadUInt32();

            _blockData = new (long Position, uint Size)[BlockCount];
            for (var i = 0; i < BlockCount; i++)
                _blockData[i] = (reader.ReadInt64(), reader.ReadUInt32());
        }

        public uint Size { get; }
        public uint BlockSize { get; }
        public uint BlockCount { get; }
        public Type DataType => typeof(T);

        public async IAsyncEnumerable<object> EnumerateAll()
        {
            await foreach(var item in EnumerateAllTyped())
                yield return item;
        }

        public async Task ForEachBlock(BlockCallback<T> callback, INotifyUser? notify = null, string? message = null, CancellationToken ct = default)
        {
            var guid = Guid.NewGuid();
            notify?.OnStartOperation(guid, message);
            var count = 0;

            for (uint i = 0; i < BlockCount; i++) {
                var (position, size) = _blockData[i];
                using var data = MemoryOwner<byte>.Allocate((int)size);
                await _lock.WaitAsync(ct);
                try {
                    _stream.Seek(position, SeekOrigin.Begin);
                    await _stream.ReadExactlyAsync(data.Memory, ct);
                }
                finally {
                    _lock.Release();
                }

                var typedData = Get(data.Memory);
                callback(typedData.Span);
                notify?.OnOperationProgress(guid, (float)++count / BlockCount);
            }
            notify?.OnCompleteOperation(guid, ct.IsCancellationRequested);
        }

        public async Task<ReadOnlyMemory<T>> GetTypedBlock(uint blockIndex)
        {
            var (position, size) = _blockData[blockIndex];
            var data = new byte[size];
            await _lock.WaitAsync();
            try {
                _stream.Seek(position, SeekOrigin.Begin);
                await _stream.ReadExactlyAsync(data);
            }
            finally {
                _lock.Release();
            }
            return Get(data);
        }

        public async IAsyncEnumerable<T> EnumerateAllTyped()
        {
            for (uint i = 0; i < BlockCount; i++) {
                var (position, size) = _blockData[i];
                using var data = MemoryOwner<byte>.Allocate((int)size);
                await _lock.WaitAsync();
                try {
                    _stream.Seek(position, SeekOrigin.Begin);
                    await _stream.ReadExactlyAsync(data.Memory);
                }
                finally {
                    _lock.Release();
                }

                var typedData = Get(data.Memory);
                for (var j = 0; j < typedData.Length; j++)
                    yield return typedData.Span[j];
            }
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken ct = default) => EnumerateAllTyped().GetAsyncEnumerator(ct);

        protected abstract ReadOnlyMemory<T> Get(ReadOnlyMemory<byte> byteData);
    }
}
