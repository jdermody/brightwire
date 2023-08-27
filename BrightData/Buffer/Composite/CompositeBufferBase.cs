using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BrightData.Buffer.EncodedStream;

namespace BrightData.Buffer.Composite
{
    /// <summary>
    /// Composite buffers write to disk after their in memory cache is exhausted
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal abstract class CompositeBufferBase<T> : ICompositeBuffer<T> where T : notnull
    {
        protected record Block(T[] Data)
        {
            public uint Size { get; private set; }
            public ref T GetNext() => ref Data[Size++];
            public bool HasFreeCapacity => Size < Data.Length;
            public ReadOnlySpan<T> GetSpan() => new(Data, 0, (int)Size);
        }

        readonly int                 _itemSize;
        readonly uint                _blockSize;
        readonly IProvideTempStreams _tempStream;
        readonly string              _id;
        readonly ushort              _maxDistinct = 0;
        List<Block>?                 _inMemoryBlocks;
        Block?                       _currBlock;

        protected CompositeBufferBase(IProvideTempStreams tempStream, uint blockSize, ushort? maxDistinct)
        {
            _id = Guid.NewGuid().ToString("n");
            _tempStream = tempStream;
            _blockSize = blockSize;
            _inMemoryBlocks = new();
            _itemSize = Unsafe.SizeOf<T>();

            if (maxDistinct > 0) {
                DistinctItems = new Dictionary<T, uint>();
                _maxDistinct = maxDistinct.Value;
            }
        }

        public Predicate<T>? ConstraintValidator { get; set; } = null;

        public void Add(T item)
        {
            if (!ConstraintValidator?.Invoke(item) == false)
                throw new InvalidOperationException($"Failed to add item to buffer as it failed validation: {item}");

            if (_currBlock?.HasFreeCapacity != true) {
                if(_currBlock is not null)
                    (_inMemoryBlocks ??= new()).Add(_currBlock);
                var wasRetried = false;
                while (!wasRetried) {
                    try {
                        using var memoryCheck = new MemoryFailPoint(Math.Max(1, _itemSize * (int)_blockSize / 1024 / 1024));
                        _currBlock = new(new T[_blockSize]);
                        break;
                    }
                    catch(InsufficientMemoryException) {
                        // try to recover by writing everything in memory to disk and then retrying the allocation
                        if (!wasRetried && _inMemoryBlocks is not null) {
                            var stream = _tempStream.Get(_id);
                            stream.Seek(0, SeekOrigin.End);
                            foreach(var block in _inMemoryBlocks)
                                WriteTo(block.GetSpan(), stream);
                            _inMemoryBlocks.Clear();
                            GC.Collect();
                            wasRetried = true;
                        }else
                            throw;
                    }

                }
            }

            _currBlock!.GetNext() = item;
            if (DistinctItems?.TryAdd(item, Size) == true && DistinctItems.Count > _maxDistinct)
                DistinctItems = null;
            ++Size;
        }

        public IEnumerable<T> Values
        {
            get
            {
                // read from the stream
                if (_tempStream.HasStream(_id)) {
                    var stream = _tempStream.Get(_id);
                    stream.Seek(0, SeekOrigin.Begin);
                    var buffer = new T[_blockSize];
                    while (stream.Position < stream.Length) {
                        var count = ReadTo(stream, _blockSize, buffer);
                        for (uint i = 0; i < count; i++)
                            yield return buffer[i];
                    }
                }

                // then from the in memory blocks
                if (_inMemoryBlocks is not null) {
                    foreach (var block in _inMemoryBlocks) {
                        for(var i = 0; i < block.Size; i++)
                            yield return block.Data[i];
                    }
                }

                // then from the current block
                if (_currBlock is not null) {
                    for (var i = 0; i < _currBlock.Size; i++)
                        yield return _currBlock.Data[i];
                }
            }
        }

        public void CopyTo(Stream stream) => EncodedStreamWriter.CopyTo(this, stream);

        IEnumerable<object> ICanEnumerate.Values => Values.Select(o => (object)o);
        public uint Size { get; private set; } = 0;
        public uint? NumDistinct => (uint?)DistinctItems?.Count;
        public void AddObject(object obj) => Add((T)obj);
        public Type DataType { get; } = typeof(T);
        protected abstract void WriteTo(ReadOnlySpan<T> ptr, Stream stream);
        protected abstract uint ReadTo(Stream stream, uint count, T[] buffer);
        public Dictionary<T, uint>? DistinctItems { get; private set; } = null;
    }
}
