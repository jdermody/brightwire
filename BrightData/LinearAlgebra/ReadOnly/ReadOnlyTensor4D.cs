using System;
using System.Buffers.Binary;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using BrightData.LinearAlgebra.ReadOnlyTensorValueSemantics;
using BrightData.LinearAlgebra.Segments;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData.LinearAlgebra.ReadOnly
{
    /// <summary>
    /// Read only 4D tensor
    /// </summary>
    public class ReadOnlyTensor4D<T> : ReadOnlyTensorBase<T, IReadOnlyTensor4D<T>>, IReadOnlyTensor4D<T>, IEquatable<ReadOnlyTensor4D<T>>, IHaveReadOnlyContiguousMemory<T>, IHaveDataAsReadOnlyByteSpan
        where T : unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        const int HeaderSize = 16;
        readonly ReadOnlyTensor4DValueSemantics<T, ReadOnlyTensor4D<T>> _valueSemantics;

        /// <summary>
        /// Creates a 4D tensor from 3D tensors
        /// </summary>
        /// <param name="tensors"></param>
        public ReadOnlyTensor4D(IReadOnlyTensor3D<T>[] tensors) : base(new ReadOnlyTensorSegment<T>(ReadOnlyMemory<T>.Empty))
        {
            Count = (uint)tensors.Length;
            var firstTensor = tensors[0];
            RowCount = firstTensor.RowCount;
            ColumnCount = firstTensor.ColumnCount;
            Depth = firstTensor.Depth;
            
            var data = new T[TensorSize * Count];
            var ptr = data.AsSpan();
            uint offset = 0;
            foreach (var tensor in tensors) {
                var temp = SpanOwner<T>.Empty;
                var span = tensor.GetSpan(ref temp, out var wasTempUsed);
                span.CopyTo(ptr.Slice((int)offset, (int)TensorSize));
                if(wasTempUsed)
                    temp.Dispose();
                offset += TensorSize;
            }
            ReadOnlySegment = new ReadOnlyTensorSegment<T>(data);
            _valueSemantics = new(this);
        }

        /// <summary>
        /// Create tensor from segment
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="count"></param>
        /// <param name="depth"></param>
        /// <param name="rowCount"></param>
        /// <param name="columnCount"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public ReadOnlyTensor4D(IReadOnlyNumericSegment<T> segment, uint count, uint depth, uint rowCount, uint columnCount) : base(segment)
        {
            if(segment.Contiguous is null)
                throw new ArgumentNullException(nameof(segment), "Expected a contiguous segment");
            Count = count;
            Depth = depth;
            RowCount = rowCount;
            ColumnCount = columnCount;
            _valueSemantics = new(this);
            if (TensorSize * Count != ReadOnlySegment.Size)
                throw new ArgumentException($"Expected tensor size ({TensorSize}) * count ({Count}) to equal input size ({ReadOnlySegment.Size})");
        }

        /// <summary>
        /// Creates a tensor from bytes
        /// </summary>
        /// <param name="data"></param>
        public ReadOnlyTensor4D(ReadOnlySpan<byte> data) : base(new ReadOnlyTensorSegment<T>(data[HeaderSize..].Cast<byte, T>().ToArray()))
        {
            ColumnCount = BinaryPrimitives.ReadUInt32LittleEndian(data);
            RowCount = BinaryPrimitives.ReadUInt32LittleEndian(data[4..]);
            Depth = BinaryPrimitives.ReadUInt32LittleEndian(data[8..]);
            Count = BinaryPrimitives.ReadUInt32LittleEndian(data[12..]);
            _valueSemantics = new(this);
            if (TensorSize * Count != ReadOnlySegment.Size)
                throw new ArgumentException($"Expected tensor size ({TensorSize}) * count ({Count}) to equal input size ({ReadOnlySegment.Size})");
        }

        /// <summary>
        /// Creates a tensor from memory
        /// </summary>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <param name="depth"></param>
        /// <param name="rowCount"></param>
        /// <param name="columnCount"></param>
        public ReadOnlyTensor4D(ReadOnlyMemory<T> data, uint count, uint depth, uint rowCount, uint columnCount) : base(new ReadOnlyTensorSegment<T>(data))
        {
            Count = count;
            Depth = depth;
            RowCount = rowCount;
            ColumnCount = columnCount;
            _valueSemantics = new(this);
            if (TensorSize * Count != ReadOnlySegment.Size)
                throw new ArgumentException($"Expected tensor size ({TensorSize}) * count ({Count}) to equal input size ({ReadOnlySegment.Size})");
        }

        //static IReadOnlyTensor3D<T>[] BuildTensors(ReadOnlySpan<T> floats, uint count, uint depth, uint rows, uint columns)
        //{
        //    var ret = new IReadOnlyTensor3D<T>[count];
        //    var matrixSize = (int)(columns * rows);
        //    for (uint i = 0; i < count; i++) {
        //        var matrices = new IReadOnlyMatrix<T>[depth];
        //        for (uint j = 0; j < depth; j++) {
        //            matrices[j] = new ReadOnlyMatrix<T>(floats[..matrixSize].ToArray(), rows, columns);
        //            floats = floats[matrixSize..];
        //        }
        //        ret[i] = new ReadOnlyTensor3D<T>(matrices);
        //    }
        //    return ret;
        //}

        /// <inheritdoc />
        public override void WriteTo(BinaryWriter writer)
        {
            writer.Write(4);
            writer.Write(ColumnCount);
            writer.Write(RowCount);
            writer.Write(Depth);
            writer.Write(Count);
            writer.Write(ReadOnlySegment);
        }

        /// <inheritdoc />
        public override void Initialize(BrightDataContext context, BinaryReader reader)
        {
            if (reader.ReadInt32() != 4)
                throw new Exception("Unexpected array size");

            ColumnCount = reader.ReadUInt32();
            RowCount = reader.ReadUInt32();
            Depth = reader.ReadUInt32();
            Count = reader.ReadUInt32();
            ReadOnlySegment = new ReadOnlyTensorSegment<T>(reader.BaseStream.ReadArray<T>(TensorSize * Count));
            Unsafe.AsRef(in _valueSemantics) = new(this);
        }

        /// <inheritdoc />
        public ReadOnlySpan<T> ReadOnlySpan
        {
            get
            {
                var contiguous = ReadOnlySegment.Contiguous;
                if (contiguous is not null)
                    return contiguous.ReadOnlySpan;
                return ReadOnlySegment.ToNewArray();
            }
        }

        /// <inheritdoc />
        public ReadOnlyMemory<T> ContiguousMemory => ReadOnlySegment.GetMemory();

        /// <inheritdoc />
        public uint Count { get; private set; }

        /// <inheritdoc />
        public uint Depth { get; private set; }

        /// <inheritdoc />
        public uint RowCount { get; private set; }

        /// <inheritdoc />
        public uint ColumnCount { get; private set; }

        /// <inheritdoc />
        public uint MatrixSize => RowCount * ColumnCount;

        /// <inheritdoc />
        public uint TensorSize => MatrixSize * Depth;

        /// <inheritdoc />
        public T this[int count, int depth, int rowY, int columnX] => ReadOnlySegment[count * TensorSize + depth * MatrixSize + columnX * RowCount + rowY];

        /// <inheritdoc />
        public T this[uint count, uint depth, uint rowY, uint columnX] => ReadOnlySegment[count * TensorSize + depth * MatrixSize + columnX * RowCount + rowY];

        /// <inheritdoc />
        public ITensor4D<T> Create(LinearAlgebraProvider<T> lap) => lap.CreateTensor4D(this);

        /// <inheritdoc />
        public IReadOnlyTensor3D<T> GetTensor(uint index)
        {
            var segment = new ReadOnlyTensorSegmentWrapper<T>(ReadOnlySegment, index * TensorSize, 1, TensorSize);
            return new ReadOnlyTensor3D<T>(segment, Depth, RowCount, ColumnCount);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj) => _valueSemantics.Equals(obj as ReadOnlyTensor4D<T>);

        /// <inheritdoc />
        public override int GetHashCode() => _valueSemantics.GetHashCode();

        /// <inheritdoc />
        public bool Equals(ReadOnlyTensor4D<T>? other) => _valueSemantics.Equals(other);

        /// <inheritdoc />
        public override string ToString()
        {
            var preview = String.Join("|", ReadOnlySegment.Values.Take(Consts.DefaultPreviewSize));
            if (Size > Consts.DefaultPreviewSize)
                preview += "|...";
            return $"Read Only Tensor 4D (Count: {Count}, Depth: {Depth}, Rows: {RowCount}, Columns: {ColumnCount}) {preview}";
        }

        /// <inheritdoc />
        public override ReadOnlySpan<byte> DataAsBytes => GetDataAsBytes(ReadOnlySegment, Count, Depth, RowCount, ColumnCount);

        internal static ReadOnlySpan<byte> GetDataAsBytes(IReadOnlyNumericSegment<T> segment, uint count, uint depth, uint rowCount, uint columnCount)
        {
            var buffer = segment.AsBytes();
            var ret = new Span<byte>(new byte[buffer.Length + HeaderSize]);
            BinaryPrimitives.WriteUInt32LittleEndian(ret, columnCount);
            BinaryPrimitives.WriteUInt32LittleEndian(ret[4..], rowCount);
            BinaryPrimitives.WriteUInt32LittleEndian(ret[8..], depth);
            BinaryPrimitives.WriteUInt32LittleEndian(ret[12..], count);
            buffer.CopyTo(ret[HeaderSize..]);
            return ret;
        }

        /// <inheritdoc />
        protected override ReadOnlyTensor4D<T> Create(MemoryOwner<T> memory)
        {
            try {
                return new ReadOnlyTensor4D<T>(memory.Span.ToArray(), Count, Depth, RowCount, ColumnCount);
            }
            finally {
                memory.Dispose();
            }
        }
    }
}
