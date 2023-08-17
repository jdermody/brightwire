using System;
using System.IO;
using System.Linq;
using BrightData.LinearAlgebra.ReadOnlyTensorValueSemantics;
using BrightData.LinearAlgebra.Segments;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData.LinearAlgebra.ReadOnly
{
    internal class ReadOnlyTensor4D : IReadOnlyTensor4D, IEquatable<ReadOnlyTensor4D>, IHaveReadOnlyContiguousSpan<float>
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

        public ReadOnlySpan<float> GetSpan(ref SpanOwner<float> temp, out bool wasTempUsed) => ReadOnlySegment.GetSpan(ref temp, out wasTempUsed);
        public ReadOnlySpan<float> FloatSpan => ReadOnlySegment.GetSpan();

        public uint Size => TensorSize * Count;
        public uint Count { get; private set; }
        public uint Depth { get; private set; }
        public uint RowCount { get; private set; }
        public uint ColumnCount { get; private set; }
        public uint MatrixSize => RowCount * ColumnCount;
        public uint TensorSize => MatrixSize * Depth;
        public bool IsReadOnly => true;

        public IReadOnlyNumericSegment<float> ReadOnlySegment => _segment.Value;

        public float this[int count, int depth, int rowY, int columnX] => _tensors[count][depth, rowY, columnX];
        public float this[uint count, uint depth, uint rowY, uint columnX] => _tensors[count][depth, rowY, columnX];
        public ITensor4D Create(LinearAlgebraProvider lap) => lap.CreateTensor4D(_tensors);
        public IReadOnlyTensor3D GetTensor3D(uint index) => _tensors[index];
        public IReadOnlyTensor3D[] AllTensors() => _tensors;

        // value semantics
        public override bool Equals(object? obj) => _valueSemantics.Equals(obj as ReadOnlyTensor4D);
        public override int GetHashCode() => _valueSemantics.GetHashCode();
        public bool Equals(ReadOnlyTensor4D? other) => _valueSemantics.Equals(other);

        public override string ToString()
        {
            var preview = String.Join("|", Enumerable.Range(0, Consts.DefaultPreviewSize).Select(x => ReadOnlySegment[x]));
            if (Size > Consts.DefaultPreviewSize)
                preview += "|...";
            return $"Read Only Tensor 4D (Count: {Count}, Depth: {Depth}, Rows: {RowCount}, Columns: {ColumnCount}) {preview}";
        }
    }
}
