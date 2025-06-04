using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace BrightData.LinearAlgebra.VectorIndexing.Storage
{
    internal class DiskBasedVectorStorage<T>: IStoreVectors<T>
        where T : unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        readonly SafeFileHandle _fileHandle;
        readonly string _filePath;
        readonly int _elementSize;
        long _vectorCount;

        public DiskBasedVectorStorage(string filePath, uint vectorSize)
        {
            _filePath = filePath;
            VectorSize = vectorSize;
            _elementSize = Marshal.SizeOf<T>();

            // Open or create the file for read/write, no sharing, async enabled
            _fileHandle = File.OpenHandle(
                filePath,
                FileMode.OpenOrCreate,
                FileAccess.ReadWrite,
                FileShare.None,
                FileOptions.RandomAccess | FileOptions.Asynchronous
            );

            // Calculate current vector count
            var fileLength = RandomAccess.GetLength(_fileHandle);
            if (fileLength % (VectorSize * _elementSize) != 0)
                throw new IOException("Corrupt vector storage file: size is not a multiple of vector size.");

            _vectorCount = fileLength / (VectorSize * _elementSize);
        }

        public void Dispose()
        {
            _fileHandle?.Dispose();
        }

        public VectorStorageType StorageType => VectorStorageType.OnDisk;
        public uint VectorSize { get; }
        public uint Size => (uint)_vectorCount;

        public ReadOnlySpan<T> this[uint index]
        {
            get
            {
                var buffer = new T[VectorSize];
                var offset = index * VectorSize * (uint)_elementSize;
                var bytes = MemoryMarshal.AsBytes(buffer.AsSpan());

                var read = RandomAccess.Read(_fileHandle, bytes, offset);
                if (read != bytes.Length)
                    throw new IOException("Failed to read full vector from disk.");
                return buffer;
            }
        }

        public uint Add(ReadOnlySpan<T> vector)
        {
            if (vector.Length != VectorSize)
                throw new ArgumentException($"Expected vector to be size {VectorSize} but received {vector.Length}", nameof(vector));

            var index = (uint)Interlocked.Increment(ref _vectorCount) - 1;
            var offset = index * VectorSize * (uint)_elementSize;
            var bytes = MemoryMarshal.AsBytes(vector);

            RandomAccess.Write(_fileHandle, bytes, offset);
            return index;
        }

        public void ForEach(IndexedSpanCallbackWithVectorIndex<T> callback, CancellationToken ct)
        {
            var size = Size;
            if (size < Consts.MinimumSizeForParallel)
            {
                for (uint i = 0; i < size && !ct.IsCancellationRequested; i++)
                    callback(this[i], i);
            }
            else
            {
                Parallel.For(0, size, new ParallelOptions { CancellationToken = ct }, i =>
                {
                    callback(this[(uint)i], (uint)i);
                });
            }
        }

        public unsafe void ForEach(ReadOnlySpan<uint> indices, IndexedSpanCallbackWithVectorIndexAndRelativeIndex<T> callback)
        {
            var len = indices.Length;
            if (len < Consts.MinimumSizeForParallel)
            {
                for (var i = 0U; i < len; i++)
                {
                    var vectorIndex = indices[(int)i];
                    callback(this[vectorIndex], vectorIndex, i);
                }
            }
            else
            {
                fixed (uint* indexPtr = indices)
                {
                    var index = indexPtr;
                    Parallel.For(0, len, i =>
                    {
                        var vectorIndex = index[i];
                        callback(this[vectorIndex], vectorIndex, (uint)i);
                    });
                }
            }
        }

        public ReadOnlyMemory<T>[] GetAll()
        {
            var size = Size;
            var ret = new ReadOnlyMemory<T>[size];
            for (var i = 0U; i < size; i++)
                ret[i] = this[i].ToArray();
            return ret;
        }
    }
}
