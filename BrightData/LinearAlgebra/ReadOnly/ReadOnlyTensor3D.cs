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
    public class ReadOnlyTensor3D : IReadOnlyTensor3D, IEquatable<ReadOnlyTensor3D>, IHaveReadOnlyContiguousSpan<float>, IHaveDataAsReadOnlyByteSpan
    {
        const int HeaderSize = 12;
        readonly ReadOnlyTensor3DValueSemantics<ReadOnlyTensor3D> _valueSemantics;
        readonly Lazy<IReadOnlyNumericSegment<float>> _segment;
        IReadOnlyMatrix[] _matrices;

        public ReadOnlyTensor3D(IReadOnlyMatrix[] matrices)
        {
            _matrices = matrices;
            Depth = (uint)_matrices.Length;
            var firstMatrix = _matrices[0];
            RowCount = firstMatrix.RowCount;
            ColumnCount = firstMatrix.ColumnCount;
            _valueSemantics = new(this);
            _segment = new(() => {
                var data = new float[Size];
                var ptr = data.AsSpan();
                uint offset = 0;
                foreach (var matrix in _matrices) {
                    var temp = SpanOwner<float>.Empty;
                    var span = matrix.GetSpan(ref temp, out var wasTempUsed);
                    span.CopyTo(ptr.Slice((int)offset, (int)MatrixSize));
                    if (wasTempUsed)
                        temp.Dispose();
                    offset += MatrixSize;
                }

                return new ArrayBasedTensorSegment(data);
            });
        }

        public ReadOnlyTensor3D(ReadOnlySpan<byte> data) : this(BuildMatrices(data))
        {
        }

        public ReadOnlyTensor3D(ReadOnlyMemory<float> data, uint depth, uint rows, uint columns) : this(BuildMatrices(data.Span, depth, rows, columns))
        {
        }

        static IReadOnlyMatrix[] BuildMatrices(ReadOnlySpan<byte> data)
        {
            var columns = BinaryPrimitives.ReadUInt32LittleEndian(data);
            var rows = BinaryPrimitives.ReadUInt32LittleEndian(data[4..]);
            var depth = BinaryPrimitives.ReadUInt32LittleEndian(data[8..]);
            return BuildMatrices(data[HeaderSize..].Cast<byte, float>(), depth, rows, columns);
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
        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(3);
            writer.Write(ColumnCount);
            writer.Write(RowCount);
            writer.Write(Depth);
            foreach (var item in _matrices) {
                var temp = SpanOwner<float>.Empty;
                var span = item.GetSpan(ref temp, out var wasTempUsed);
                writer.Write(span.AsBytes());
                if(wasTempUsed)
                    temp.Dispose();
            }
        }

        /// <inheritdoc />
        public void Initialize(BrightDataContext context, BinaryReader reader)
        {
            if (reader.ReadInt32() != 3)
                throw new Exception("Unexpected array size");
            if (_segment.IsValueCreated)
                throw new Exception("Segment was created before type was initialized");

            ColumnCount = reader.ReadUInt32();
            RowCount = reader.ReadUInt32();
            Depth = reader.ReadUInt32();
            _matrices = new IReadOnlyMatrix[Depth];
            for (var i = 0; i < Depth; i++) {
                var buffer = reader.BaseStream.ReadArray<float>(MatrixSize);
                _matrices[i] = new ReadOnlyMatrixWrapper(new ArrayBasedTensorSegment(buffer), RowCount, ColumnCount);
            }
        }

        /// <inheritdoc />
        public ReadOnlySpan<float> GetSpan(ref SpanOwner<float> temp, out bool wasTempUsed) => ReadOnlySegment.GetSpan(ref temp, out wasTempUsed);

        /// <inheritdoc />
        public IReadOnlyNumericSegment<float> ReadOnlySegment => _segment.Value;

        /// <inheritdoc />
        public ReadOnlySpan<float> ReadOnlySpan => ReadOnlySegment.Contiguous!.ReadOnlySpan;

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
        public bool IsReadOnly => true;

        /// <inheritdoc />
        public float this[int depth, int rowY, int columnX] => _matrices[depth][rowY, columnX];

        /// <inheritdoc />
        public float this[uint depth, uint rowY, uint columnX] => _matrices[depth][rowY, columnX];

        /// <inheritdoc />
        public ITensor3D Create(LinearAlgebraProvider lap) => lap.CreateTensor3D(this);

        /// <inheritdoc />
        public IReadOnlyMatrix GetMatrix(uint index) => _matrices[index];

        /// <inheritdoc />
        public IReadOnlyMatrix[] AllMatrices() => _matrices;

        /// <inheritdoc />
        public override bool Equals(object? obj) => _valueSemantics.Equals(obj as ReadOnlyTensor3D);

        /// <inheritdoc />
        public override int GetHashCode() => _valueSemantics.GetHashCode();

        /// <inheritdoc />
        public bool Equals(ReadOnlyTensor3D? other) => _valueSemantics.Equals(other);

        /// <inheritdoc />
        public override string ToString()
        {
            var preview = String.Join("|", Enumerable.Range(0, Consts.DefaultPreviewSize).Select(x => ReadOnlySegment[x]));
            if (Size > Consts.DefaultPreviewSize)
                preview += "|...";
            return $"Read Only Tensor 3D (Depth: {Depth}, Rows: {RowCount}, Columns: {ColumnCount}) {preview}";
        }

        /// <inheritdoc />
        public ReadOnlySpan<byte> DataAsBytes
        {
            get
            {
                var buffer = _segment.Value;
                var ret = new Span<byte>(new byte[buffer.Size + HeaderSize]);
                BinaryPrimitives.WriteUInt32LittleEndian(ret, ColumnCount);
                BinaryPrimitives.WriteUInt32LittleEndian(ret[4..], RowCount);
                BinaryPrimitives.WriteUInt32LittleEndian(ret[8..], Depth);
                buffer.CopyTo(ret[HeaderSize..].Cast<byte, float>());
                return ret;
            }
        }
    }
}
