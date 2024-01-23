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
    /// Read only 4D tensor
    /// </summary>
    public class ReadOnlyTensor4D : IReadOnlyTensor4D, IEquatable<ReadOnlyTensor4D>, IHaveReadOnlyContiguousSpan<float>, IHaveDataAsReadOnlyByteSpan
    {
        const int HeaderSize = 16;
        ReadOnlyTensor4DValueSemantics<ReadOnlyTensor4D> _valueSemantics;

        /// <summary>
        /// Creates a 4D tensor from 3D tensors
        /// </summary>
        /// <param name="tensors"></param>
        public ReadOnlyTensor4D(IReadOnlyTensor3D[] tensors)
        {
            Count = (uint)tensors.Length;
            var firstTensor = tensors[0];
            RowCount = firstTensor.RowCount;
            ColumnCount = firstTensor.ColumnCount;
            Depth = firstTensor.Depth;
            
            var data = new float[Size];
            var ptr = data.AsSpan();
            uint offset = 0;
            foreach (var tensor in tensors) {
                var temp = SpanOwner<float>.Empty;
                var span = tensor.GetSpan(ref temp, out var wasTempUsed);
                span.CopyTo(ptr.Slice((int)offset, (int)TensorSize));
                if(wasTempUsed)
                    temp.Dispose();
                offset += TensorSize;
            }
            ReadOnlySegment = new ReadOnlyTensorSegment(data);
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
        public ReadOnlyTensor4D(IReadOnlyNumericSegment<float> segment, uint count, uint depth, uint rowCount, uint columnCount)
        {
            if(segment.Contiguous is null)
                throw new ArgumentNullException(nameof(segment), "Expected a contiguous segment");
            Count = count;
            Depth = depth;
            RowCount = rowCount;
            ColumnCount = columnCount;
            ReadOnlySegment = segment;
            _valueSemantics = new(this);
        }

        /// <summary>
        /// Creates a tensor from bytes
        /// </summary>
        /// <param name="data"></param>
        public ReadOnlyTensor4D(ReadOnlySpan<byte> data)
        {
            ColumnCount = BinaryPrimitives.ReadUInt32LittleEndian(data);
            RowCount = BinaryPrimitives.ReadUInt32LittleEndian(data[4..]);
            Depth = BinaryPrimitives.ReadUInt32LittleEndian(data[8..]);
            Count = BinaryPrimitives.ReadUInt32LittleEndian(data[12..]);
            var floats = data[HeaderSize..].Cast<byte, float>();
            ReadOnlySegment = new ReadOnlyTensorSegment(floats.ToArray());
            _valueSemantics = new(this);
        }

        /// <summary>
        /// Creates a tensor from float memory
        /// </summary>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <param name="depth"></param>
        /// <param name="rowCount"></param>
        /// <param name="columnCount"></param>
        public ReadOnlyTensor4D(ReadOnlyMemory<float> data, uint count, uint depth, uint rowCount, uint columnCount)
        {
            Count = count;
            Depth = depth;
            RowCount = rowCount;
            ColumnCount = columnCount;
            ReadOnlySegment = new ReadOnlyTensorSegment(data);
            _valueSemantics = new(this);
        }

        static IReadOnlyTensor3D[] BuildTensors(ReadOnlySpan<float> floats, uint count, uint depth, uint rows, uint columns)
        {
            var ret = new IReadOnlyTensor3D[count];
            var matrixSize = (int)(columns * rows);
            for (uint i = 0; i < count; i++) {
                var matrices = new IReadOnlyMatrix[depth];
                for (uint j = 0; j < depth; j++) {
                    matrices[j] = new ReadOnlyMatrix(floats[..matrixSize].ToArray(), rows, columns);
                    floats = floats[matrixSize..];
                }
                ret[i] = new ReadOnlyTensor3D(matrices);
            }
            return ret;
        }

        /// <inheritdoc />
        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(4);
            writer.Write(ColumnCount);
            writer.Write(RowCount);
            writer.Write(Depth);
            writer.Write(Count);
            writer.Write(ReadOnlySegment);
        }

        /// <inheritdoc />
        public void Initialize(BrightDataContext context, BinaryReader reader)
        {
            if (reader.ReadInt32() != 4)
                throw new Exception("Unexpected array size");

            ColumnCount = reader.ReadUInt32();
            RowCount = reader.ReadUInt32();
            Depth = reader.ReadUInt32();
            Count = reader.ReadUInt32();
            ReadOnlySegment = new ReadOnlyTensorSegment(reader.BaseStream.ReadArray<float>(Size));
            _valueSemantics = new(this);
        }

        /// <inheritdoc />
        public ReadOnlySpan<float> GetSpan(ref SpanOwner<float> temp, out bool wasTempUsed) => ReadOnlySegment.GetSpan(ref temp, out wasTempUsed);

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
        public uint Size => TensorSize * Count;

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
        public IReadOnlyNumericSegment<float> ReadOnlySegment { get; private set; }

        /// <inheritdoc />
        public float this[int count, int depth, int rowY, int columnX] => ReadOnlySegment[count * TensorSize + depth * MatrixSize + columnX * RowCount + rowY];

        /// <inheritdoc />
        public float this[uint count, uint depth, uint rowY, uint columnX] => ReadOnlySegment[count * TensorSize + depth * MatrixSize + columnX * RowCount + rowY];

        /// <inheritdoc />
        public ITensor4D Create(LinearAlgebraProvider lap) => lap.CreateTensor4D(this);

        /// <inheritdoc />
        public IReadOnlyTensor3D GetTensor(uint index)
        {
            var segment = new ReadOnlyTensorSegmentWrapper(ReadOnlySegment, index * TensorSize, 1, TensorSize);
            return new ReadOnlyTensor3D(segment, Depth, RowCount, ColumnCount);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj) => _valueSemantics.Equals(obj as ReadOnlyTensor4D);

        /// <inheritdoc />
        public override int GetHashCode() => _valueSemantics.GetHashCode();

        /// <inheritdoc />
        public bool Equals(ReadOnlyTensor4D? other) => _valueSemantics.Equals(other);

        /// <inheritdoc />
        public override string ToString()
        {
            var preview = String.Join("|", ReadOnlySegment.Values.Take(Consts.DefaultPreviewSize));
            if (Size > Consts.DefaultPreviewSize)
                preview += "|...";
            return $"Read Only Tensor 4D (Count: {Count}, Depth: {Depth}, Rows: {RowCount}, Columns: {ColumnCount}) {preview}";
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
                BinaryPrimitives.WriteUInt32LittleEndian(ret[12..], Count);
                buffer.CopyTo(ret[HeaderSize..]);
                return ret;
            }
        }
    }
}
