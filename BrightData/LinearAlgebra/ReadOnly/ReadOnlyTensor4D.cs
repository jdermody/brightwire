using System;
using System.IO;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData.LinearAlgebra.ReadOnly
{
    internal class ReadOnlyTensor4D : IReadOnlyTensor4D
    {
        IReadOnlyTensor3D[] _tensors;
        ITensorSegment? _segment;

        public ReadOnlyTensor4D(IReadOnlyTensor3D[] tensors)
        {
            _tensors = tensors;
            Count = (uint)tensors.Length;
            var firstTensor = tensors[0];
            RowCount = firstTensor.RowCount;
            ColumnCount = firstTensor.ColumnCount;
            Depth = firstTensor.Depth;
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
                var span = item.GetFloatSpan(ref temp, out var wasTempUsed);
                writer.Write(span.AsBytes());
                if(wasTempUsed)
                    temp.Dispose();
            }
        }

        public void Initialize(BrightDataContext context, BinaryReader reader)
        {
            if (reader.ReadInt32() != 4)
                throw new Exception("Unexpected array size");
            ColumnCount = reader.ReadUInt32();
            RowCount = reader.ReadUInt32();
            Depth = reader.ReadUInt32();
            Count = reader.ReadUInt32();
            _tensors = new IReadOnlyTensor3D[Depth];
            for (var i = 0; i < Count; i++) {
                var buffer = reader.BaseStream.ReadArray<float>(MatrixSize);
                _tensors[i] = new ReadOnlyTensor3DWrapper(new ArrayBasedTensorSegment(buffer), Depth, RowCount, ColumnCount);
            }
            _segment = null;
        }

        public ReadOnlySpan<float> GetFloatSpan(ref SpanOwner<float> temp, out bool wasTempUsed) => Segment.GetSpan(ref temp, out wasTempUsed);
        public uint Size => TensorSize * Count;
        public uint Count { get; private set; }
        public uint Depth { get; private set; }
        public uint RowCount { get; private set; }
        public uint ColumnCount { get; private set; }
        public uint MatrixSize => RowCount * ColumnCount;
        public uint TensorSize => MatrixSize * Depth;

        public ITensorSegment Segment
        {
            get
            {
                if (_segment is null) {
                    var data = new float[Size];
                    var ptr = data.AsSpan();
                    uint offset = 0;
                    foreach (var tensor in _tensors) {
                        var temp = SpanOwner<float>.Empty;
                        var span = tensor.GetFloatSpan(ref temp, out var wasTempUsed);
                        span.CopyTo(ptr.Slice((int)offset, (int)TensorSize));
                        if(wasTempUsed)
                            temp.Dispose();
                        offset += TensorSize;
                    }
                    _segment = new ArrayBasedTensorSegment(data);
                }
                return _segment;
            }
        }

        public float this[int count, int depth, int rowY, int columnX] => _tensors[count][depth, rowY, columnX];
        public float this[uint count, uint depth, uint rowY, uint columnX] => _tensors[count][depth, rowY, columnX];
        public ITensor4D Create(LinearAlgebraProvider lap) => lap.CreateTensor4D(_tensors);
    }
}
