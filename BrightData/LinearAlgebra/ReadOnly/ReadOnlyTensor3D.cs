using System;
using System.Buffers.Binary;
using System.IO;
using System.Linq;
using BrightData.LinearAlgebra.ReadOnlyTensorValueSemantics;
using BrightData.LinearAlgebra.Segments;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData.LinearAlgebra.ReadOnly
{
    /// <summary>
    /// Read only tensor
    /// </summary>
    public class ReadOnlyTensor3D : IReadOnlyTensor3D, IEquatable<ReadOnlyTensor3D>, IHaveReadOnlyContiguousSpan<float>, IHaveDataAsReadOnlyByteSpan
    {
        const int HeaderSize = 12;
        ReadOnlyTensor3DValueSemantics<ReadOnlyTensor3D> _valueSemantics;

        /// <summary>
        /// Creates a tensor from matrices
        /// </summary>
        /// <param name="matrices"></param>
        public ReadOnlyTensor3D(IReadOnlyMatrix[] matrices)
        {
            Depth = (uint)matrices.Length;
            var firstMatrix = matrices[0];
            RowCount = firstMatrix.RowCount;
            ColumnCount = firstMatrix.ColumnCount;
            
            var data = new float[Size];
            var ptr = data.AsSpan();
            uint offset = 0;
            foreach (var matrix in matrices) {
                var temp = SpanOwner<float>.Empty;
                var span = matrix.GetSpan(ref temp, out var wasTempUsed);
                span.CopyTo(ptr.Slice((int)offset, (int)MatrixSize));
                if (wasTempUsed)
                    temp.Dispose();
                offset += MatrixSize;
            }
            ReadOnlySegment = new ReadOnlyTensorSegment(data);
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
        public ReadOnlyTensor3D(IReadOnlyNumericSegment<float> segment, uint depth, uint rowCount, uint columnCount)
        {
            Depth = depth;
            RowCount = rowCount;
            ColumnCount = columnCount;
            ReadOnlySegment = segment;
            _valueSemantics = new(this);
        }

        /// <summary>
        /// Creates a tensor from byte data
        /// </summary>
        /// <param name="data"></param>
        public ReadOnlyTensor3D(ReadOnlySpan<byte> data)
        {
            ColumnCount = BinaryPrimitives.ReadUInt32LittleEndian(data);
            RowCount = BinaryPrimitives.ReadUInt32LittleEndian(data[4..]);
            Depth = BinaryPrimitives.ReadUInt32LittleEndian(data[8..]);
            ReadOnlySegment = new ReadOnlyTensorSegment(data[HeaderSize..].Cast<byte, float>().ToArray());
            _valueSemantics = new(this);
        }

        /// <summary>
        /// Creates a tensor from float memory
        /// </summary>
        /// <param name="data"></param>
        /// <param name="depth"></param>
        /// <param name="rowCount"></param>
        /// <param name="columnCount"></param>
        public ReadOnlyTensor3D(ReadOnlyMemory<float> data, uint depth, uint rowCount, uint columnCount)
        {
            Depth = depth;
            RowCount = rowCount;
            ColumnCount = columnCount;
            ReadOnlySegment = new ReadOnlyTensorSegment(data);
            _valueSemantics = new(this);
        }

        /// <inheritdoc />
        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(3);
            writer.Write(ColumnCount);
            writer.Write(RowCount);
            writer.Write(Depth);
            writer.Write(ReadOnlySegment);
        }

        /// <inheritdoc />
        public void Initialize(BrightDataContext context, BinaryReader reader)
        {
            if (reader.ReadInt32() != 3)
                throw new Exception("Unexpected array size");

            ColumnCount = reader.ReadUInt32();
            RowCount = reader.ReadUInt32();
            Depth = reader.ReadUInt32();
            ReadOnlySegment = new ReadOnlyTensorSegment(reader.BaseStream.ReadArray<float>(Size));
            _valueSemantics = new(this);
        }

        static IReadOnlyMatrix[] BuildMatrices(ReadOnlySpan<float> floats, uint depth, uint rows, uint columns)
        {
            var ret = new IReadOnlyMatrix[depth];
            var matrixSize = (int)(columns * rows);
            for (uint i = 0; i < depth; i++) {
                ret[i] = new ReadOnlyMatrix(floats[..matrixSize].ToArray(), rows, columns);
                floats = floats[matrixSize..];
            }
            return ret;
        }

        /// <inheritdoc />
        public ReadOnlySpan<float> GetSpan(ref SpanOwner<float> temp, out bool wasTempUsed) => ReadOnlySegment.GetSpan(ref temp, out wasTempUsed);

        /// <inheritdoc />
        public IReadOnlyNumericSegment<float> ReadOnlySegment { get; private set; }

        /// <inheritdoc />
        public ReadOnlySpan<float> ReadOnlySpan
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
        public uint Size => MatrixSize * Depth;

        /// <inheritdoc />
        public uint Depth { get; private set; }

        /// <inheritdoc />
        public uint RowCount { get; private set; }

        /// <inheritdoc />
        public uint ColumnCount { get; private set; }

        /// <inheritdoc />
        public uint MatrixSize => RowCount * ColumnCount;

        /// <inheritdoc />
        public float this[int depth, int rowY, int columnX] => ReadOnlySegment[depth * MatrixSize + columnX * RowCount + rowY];

        /// <inheritdoc />
        public float this[uint depth, uint rowY, uint columnX] => ReadOnlySegment[depth * MatrixSize + columnX * RowCount + rowY];

        /// <inheritdoc />
        public ITensor3D Create(LinearAlgebraProvider lap) => lap.CreateTensor3D(this);

        /// <inheritdoc />
        public IReadOnlyMatrix GetMatrix(uint index)
        {
            var segment = new ReadOnlyTensorSegmentWrapper(ReadOnlySegment, index * MatrixSize, 1, MatrixSize);
            return new ReadOnlyMatrix(segment, RowCount, ColumnCount);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj) => _valueSemantics.Equals(obj as ReadOnlyTensor3D);

        /// <inheritdoc />
        public override int GetHashCode() => _valueSemantics.GetHashCode();

        /// <inheritdoc />
        public bool Equals(ReadOnlyTensor3D? other) => _valueSemantics.Equals(other);

        /// <inheritdoc />
        public override string ToString()
        {
            var preview = String.Join("|", ReadOnlySegment.Values.Take(Consts.DefaultPreviewSize));
            if (Size > Consts.DefaultPreviewSize)
                preview += "|...";
            return $"Read Only Tensor 3D (Depth: {Depth}, Rows: {RowCount}, Columns: {ColumnCount}) {preview}";
        }

        /// <inheritdoc />
        public ReadOnlySpan<byte> DataAsBytes
        {
            get
            {
                var buffer = ReadOnlySegment.AsBytes();
                var ret = new Span<byte>(new byte[buffer.Length + HeaderSize]);
                BinaryPrimitives.WriteUInt32LittleEndian(ret, ColumnCount);
                BinaryPrimitives.WriteUInt32LittleEndian(ret[4..], RowCount);
                BinaryPrimitives.WriteUInt32LittleEndian(ret[8..], Depth);
                buffer.CopyTo(ret[HeaderSize..]);
                return ret;
            }
        }
    }
}
