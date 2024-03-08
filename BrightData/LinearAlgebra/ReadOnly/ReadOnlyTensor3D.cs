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
    /// Read only tensor
    /// </summary>
    public class ReadOnlyTensor3D<T> : ReadOnlyTensorBase<T, IReadOnlyTensor3D<T>>, IReadOnlyTensor3D<T>, IEquatable<ReadOnlyTensor3D<T>>, IHaveReadOnlyContiguousSpan<T>, IHaveDataAsReadOnlyByteSpan
        where T : unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        const int HeaderSize = 12;
        readonly ReadOnlyTensor3DValueSemantics<T, ReadOnlyTensor3D<T>> _valueSemantics;

        /// <summary>
        /// Creates a tensor from matrices
        /// </summary>
        /// <param name="matrices"></param>
        public ReadOnlyTensor3D(IReadOnlyMatrix<T>[] matrices) : base(new ReadOnlyTensorSegment<T>(ReadOnlyMemory<T>.Empty))
        {
            Depth = (uint)matrices.Length;
            var firstMatrix = matrices[0];
            RowCount = firstMatrix.RowCount;
            ColumnCount = firstMatrix.ColumnCount;
            
            var data = new T[MatrixSize * Depth];
            var ptr = data.AsSpan();
            uint offset = 0;
            foreach (var matrix in matrices) {
                var temp = SpanOwner<T>.Empty;
                var span = matrix.GetSpan(ref temp, out var wasTempUsed);
                span.CopyTo(ptr.Slice((int)offset, (int)MatrixSize));
                if (wasTempUsed)
                    temp.Dispose();
                offset += MatrixSize;
            }
            ReadOnlySegment = new ReadOnlyTensorSegment<T>(data);
            _valueSemantics = new(this);
        }

        /// <summary>
        /// Creates a tensor from a segment
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="depth"></param>
        /// <param name="rowCount"></param>
        /// <param name="columnCount"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public ReadOnlyTensor3D(IReadOnlyNumericSegment<T> segment, uint depth, uint rowCount, uint columnCount) : base(segment)
        {
            Depth = depth;
            RowCount = rowCount;
            ColumnCount = columnCount;
            _valueSemantics = new(this);
            if (MatrixSize * Depth != ReadOnlySegment.Size)
                throw new ArgumentException($"Expected matrix size ({MatrixSize}) * depth ({Depth}) to equal input size ({ReadOnlySegment.Size})");
        }

        /// <summary>
        /// Creates a tensor from byte data
        /// </summary>
        /// <param name="data"></param>
        public ReadOnlyTensor3D(ReadOnlySpan<byte> data) : base(new ReadOnlyTensorSegment<T>(data[HeaderSize..].Cast<byte, T>().ToArray()))
        {
            ColumnCount = BinaryPrimitives.ReadUInt32LittleEndian(data);
            RowCount = BinaryPrimitives.ReadUInt32LittleEndian(data[4..]);
            Depth = BinaryPrimitives.ReadUInt32LittleEndian(data[8..]);
            _valueSemantics = new(this);
            if (MatrixSize * Depth != ReadOnlySegment.Size)
                throw new ArgumentException($"Expected matrix size ({MatrixSize}) * depth ({Depth}) to equal input size ({ReadOnlySegment.Size})");
        }

        /// <summary>
        /// Creates a tensor from memory
        /// </summary>
        /// <param name="data"></param>
        /// <param name="depth"></param>
        /// <param name="rowCount"></param>
        /// <param name="columnCount"></param>
        public ReadOnlyTensor3D(ReadOnlyMemory<T> data, uint depth, uint rowCount, uint columnCount) : base(new ReadOnlyTensorSegment<T>(data))
        {
            Depth = depth;
            RowCount = rowCount;
            ColumnCount = columnCount;
            _valueSemantics = new(this);
            if (MatrixSize * Depth != ReadOnlySegment.Size)
                throw new ArgumentException($"Expected matrix size ({MatrixSize}) * depth ({Depth}) to equal input size ({ReadOnlySegment.Size})");
        }

        /// <inheritdoc />
        public override void WriteTo(BinaryWriter writer)
        {
            writer.Write(3);
            writer.Write(ColumnCount);
            writer.Write(RowCount);
            writer.Write(Depth);
            writer.Write(ReadOnlySegment);
        }

        /// <inheritdoc />
        public override void Initialize(BrightDataContext context, BinaryReader reader)
        {
            if (reader.ReadInt32() != 3)
                throw new Exception("Unexpected array size");

            ColumnCount = reader.ReadUInt32();
            RowCount = reader.ReadUInt32();
            Depth = reader.ReadUInt32();
            ReadOnlySegment = new ReadOnlyTensorSegment<T>(reader.BaseStream.ReadArray<T>(ColumnCount * RowCount * Depth));
            Unsafe.AsRef(in _valueSemantics) = new(this);
        }

        //static IReadOnlyMatrix<T>[] BuildMatrices(ReadOnlySpan<T> floats, uint depth, uint rows, uint columns)
        //{
        //    var ret = new IReadOnlyMatrix<T>[depth];
        //    var matrixSize = (int)(columns * rows);
        //    for (uint i = 0; i < depth; i++) {
        //        ret[i] = new ReadOnlyMatrix<T>(floats[..matrixSize].ToArray(), rows, columns);
        //        floats = floats[matrixSize..];
        //    }
        //    return ret;
        //}

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
        public uint Depth { get; private set; }

        /// <inheritdoc />
        public uint RowCount { get; private set; }

        /// <inheritdoc />
        public uint ColumnCount { get; private set; }

        /// <inheritdoc />
        public uint MatrixSize => RowCount * ColumnCount;

        /// <inheritdoc />
        public T this[int depth, int rowY, int columnX] => ReadOnlySegment[depth * MatrixSize + columnX * RowCount + rowY];

        /// <inheritdoc />
        public T this[uint depth, uint rowY, uint columnX] => ReadOnlySegment[depth * MatrixSize + columnX * RowCount + rowY];

        /// <inheritdoc />
        public ITensor3D<T> Create(LinearAlgebraProvider<T> lap) => lap.CreateTensor3D(this);

        /// <inheritdoc />
        public IReadOnlyMatrix<T> GetMatrix(uint index)
        {
            var segment = new ReadOnlyTensorSegmentWrapper<T>(ReadOnlySegment, index * MatrixSize, 1, MatrixSize);
            return new ReadOnlyMatrix<T>(segment, RowCount, ColumnCount);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj) => _valueSemantics.Equals(obj as ReadOnlyTensor3D<T>);

        /// <inheritdoc />
        public override int GetHashCode() => _valueSemantics.GetHashCode();

        /// <inheritdoc />
        public bool Equals(ReadOnlyTensor3D<T>? other) => _valueSemantics.Equals(other);

        /// <inheritdoc />
        public override string ToString()
        {
            var preview = String.Join("|", ReadOnlySegment.Values.Take(Consts.DefaultPreviewSize));
            if (Size > Consts.DefaultPreviewSize)
                preview += "|...";
            return $"Read Only Tensor 3D (Depth: {Depth}, Rows: {RowCount}, Columns: {ColumnCount}) {preview}";
        }

        /// <inheritdoc />
        public override ReadOnlySpan<byte> DataAsBytes => GetDataAsBytes(ReadOnlySegment, Depth, RowCount, ColumnCount);

        internal static ReadOnlySpan<byte> GetDataAsBytes(IReadOnlyNumericSegment<T> segment, uint depth, uint rowCount, uint columnCount)
        {
            var buffer = segment.AsBytes();
            var ret = new Span<byte>(new byte[buffer.Length + HeaderSize]);
            BinaryPrimitives.WriteUInt32LittleEndian(ret, columnCount);
            BinaryPrimitives.WriteUInt32LittleEndian(ret[4..], rowCount);
            BinaryPrimitives.WriteUInt32LittleEndian(ret[8..], depth);
            buffer.CopyTo(ret[HeaderSize..]);
            return ret;
        }

        /// <inheritdoc />
        protected override ReadOnlyTensor3D<T> Create(MemoryOwner<T> memory)
        {
            try {
                return new ReadOnlyTensor3D<T>(memory.Span.ToArray(), Depth, RowCount, ColumnCount);
            }
            finally {
                memory.Dispose();
            }
        }
    }
}
