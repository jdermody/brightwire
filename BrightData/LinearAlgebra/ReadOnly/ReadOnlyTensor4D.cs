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
    public class ReadOnlyTensor4D : IReadOnlyTensor4D, IEquatable<ReadOnlyTensor4D>, IHaveReadOnlyContiguousSpan<float>, IHaveDataAsReadOnlyByteSpan
    {
        readonly ReadOnlyTensor4DValueSemantics<ReadOnlyTensor4D> _valueSemantics;
        readonly Lazy<IReadOnlyNumericSegment<float>> _segment;
        IReadOnlyTensor3D[] _tensors;

        public ReadOnlyTensor4D(IReadOnlyTensor3D[] tensors)
        {
            _tensors = tensors;
            Count = (uint)tensors.Length;
            var firstTensor = tensors[0];
            RowCount = firstTensor.RowCount;
            ColumnCount = firstTensor.ColumnCount;
            Depth = firstTensor.Depth;
            _valueSemantics = new(this);
            _segment = new(() => {
                var data = new float[Size];
                var ptr = data.AsSpan();
                uint offset = 0;
                foreach (var tensor in _tensors) {
                    var temp = SpanOwner<float>.Empty;
                    var span = tensor.GetSpan(ref temp, out var wasTempUsed);
                    span.CopyTo(ptr.Slice((int)offset, (int)TensorSize));
                    if(wasTempUsed)
                        temp.Dispose();
                    offset += TensorSize;
                }
                return new ArrayBasedTensorSegment(data);
            });
        }

        public ReadOnlyTensor4D(ReadOnlySpan<byte> data) : this(BuildTensors(data))
        {
        }

        static IReadOnlyTensor3D[] BuildTensors(ReadOnlySpan<byte> data)
        {
            var columns = BinaryPrimitives.ReadUInt32LittleEndian(data);
            var rows = BinaryPrimitives.ReadUInt32LittleEndian(data[4..]);
            var depth = BinaryPrimitives.ReadUInt32LittleEndian(data[8..]);
            var count = BinaryPrimitives.ReadUInt32LittleEndian(data[12..]);
            var floats = data[16..].Cast<byte, float>();
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
            foreach (var item in _tensors) {
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
            if (reader.ReadInt32() != 4)
                throw new Exception("Unexpected array size");
            if (_segment.IsValueCreated)
                throw new Exception("Segment was created before type was initialized");

            ColumnCount = reader.ReadUInt32();
            RowCount = reader.ReadUInt32();
            Depth = reader.ReadUInt32();
            Count = reader.ReadUInt32();
            _tensors = new IReadOnlyTensor3D[Depth];
            for (var i = 0; i < Count; i++) {
                var buffer = reader.BaseStream.ReadArray<float>(MatrixSize);
                _tensors[i] = new ReadOnlyTensor3DWrapper(new ArrayBasedTensorSegment(buffer), Depth, RowCount, ColumnCount);
            }
        }

        /// <inheritdoc />
        public ReadOnlySpan<float> GetSpan(ref SpanOwner<float> temp, out bool wasTempUsed) => ReadOnlySegment.GetSpan(ref temp, out wasTempUsed);

        /// <inheritdoc />
        public ReadOnlySpan<float> FloatSpan => ReadOnlySegment.GetSpan();

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
        public bool IsReadOnly => true;

        /// <inheritdoc />
        public IReadOnlyNumericSegment<float> ReadOnlySegment => _segment.Value;

        /// <inheritdoc />
        public float this[int count, int depth, int rowY, int columnX] => _tensors[count][depth, rowY, columnX];

        /// <inheritdoc />
        public float this[uint count, uint depth, uint rowY, uint columnX] => _tensors[count][depth, rowY, columnX];

        /// <inheritdoc />
        public ITensor4D Create(LinearAlgebraProvider lap) => lap.CreateTensor4D(_tensors);

        /// <inheritdoc />
        public IReadOnlyTensor3D GetTensor3D(uint index) => _tensors[index];

        /// <inheritdoc />
        public IReadOnlyTensor3D[] AllTensors() => _tensors;

        /// <inheritdoc />
        public override bool Equals(object? obj) => _valueSemantics.Equals(obj as ReadOnlyTensor4D);

        /// <inheritdoc />
        public override int GetHashCode() => _valueSemantics.GetHashCode();

        /// <inheritdoc />
        public bool Equals(ReadOnlyTensor4D? other) => _valueSemantics.Equals(other);

        /// <inheritdoc />
        public override string ToString()
        {
            var preview = String.Join("|", Enumerable.Range(0, Consts.DefaultPreviewSize).Select(x => ReadOnlySegment[x]));
            if (Size > Consts.DefaultPreviewSize)
                preview += "|...";
            return $"Read Only Tensor 4D (Count: {Count}, Depth: {Depth}, Rows: {RowCount}, Columns: {ColumnCount}) {preview}";
        }

        /// <inheritdoc />
        public ReadOnlySpan<byte> DataAsBytes
        {
            get
            {
                var buffer = _segment.Value;
                var ret = new Span<byte>(new byte[buffer.Size + 16]);
                BinaryPrimitives.WriteUInt32LittleEndian(ret, ColumnCount);
                BinaryPrimitives.WriteUInt32LittleEndian(ret[4..], RowCount);
                BinaryPrimitives.WriteUInt32LittleEndian(ret[8..], Depth);
                BinaryPrimitives.WriteUInt32LittleEndian(ret[12..], Count);
                buffer.CopyTo(ret[16..].Cast<byte, float>());
                return ret;
            }
        }
    }
}
