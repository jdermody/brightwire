using BrightData.LinearAlgebra.ReadOnlyTensorValueSemantics;
using CommunityToolkit.HighPerformance.Buffers;
using System;
using System.IO;
using System.Linq;
using BrightData.LinearAlgebra.Segments;
using CommunityToolkit.HighPerformance;

namespace BrightData.LinearAlgebra.ReadOnly
{
    internal class ReadOnlyTensor4DWrapper : IReadOnlyTensor4D, IEquatable<ReadOnlyTensor4DWrapper>, IHaveReadOnlyContiguousSpan<float>
    {
        ReadOnlyTensor4DValueSemantics<ReadOnlyTensor4DWrapper>? _valueSemantics;

        public ReadOnlyTensor4DWrapper(IReadOnlyNumericSegment<float> segment, uint count, uint depth, uint rowCount, uint columnCount)
        {
            if(segment.Contiguous is null)
                throw new ArgumentNullException(nameof(segment), "Expected a contiguous segment");
            Count = count;
            Depth = depth;
            RowCount = rowCount;
            ColumnCount = columnCount;
            ReadOnlySegment = segment;
        }

        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(4);
            writer.Write(ColumnCount);
            writer.Write(RowCount);
            writer.Write(Depth);
            writer.Write(Count);
            var temp = SpanOwner<float>.Empty;
            ReadOnlySegment.GetSpan(ref temp, out var wasTempUsed);
            try {
                writer.Write(temp.Span.AsBytes());
            }
            finally {
                if (wasTempUsed)
                    temp.Dispose();
            }
        }

        public void Initialize(BrightDataContext context, BinaryReader reader)
        {
            throw new NotImplementedException();
        }

        public ReadOnlySpan<float> GetSpan(ref SpanOwner<float> temp, out bool wasTempUsed) => ReadOnlySegment.GetSpan(ref temp, out wasTempUsed);

        public uint Size => TensorSize * Count;
        public IReadOnlyNumericSegment<float> ReadOnlySegment { get; }
        public uint Count { get; }
        public uint Depth { get; }
        public uint RowCount { get; }
        public uint ColumnCount { get; }
        public uint MatrixSize => RowCount * ColumnCount;
        public uint TensorSize => MatrixSize * Depth;
        public bool IsReadOnly => true;

        public float this[int count, int depth, int rowY, int columnX] => ReadOnlySegment[count * TensorSize + depth * MatrixSize + columnX * RowCount + rowY];
        public float this[uint count, uint depth, uint rowY, uint columnX] => ReadOnlySegment[count * TensorSize + depth * MatrixSize + columnX * RowCount + rowY];

        public ITensor4D Create(LinearAlgebraProvider lap) => lap.CreateTensor4D(this);
        public IReadOnlyTensor3D GetTensor3D(uint index)
        {
            var segment = new ReadOnlyTensorSegmentWrapper(ReadOnlySegment, index * TensorSize, 1, TensorSize);
            return new ReadOnlyTensor3DWrapper(segment, Depth, RowCount, ColumnCount);
        }
        public IReadOnlyTensor3D[] AllTensors()
        {
            var ret = new IReadOnlyTensor3D[Depth];
            for (uint i = 0; i < Depth; i++)
                ret[i] = GetTensor3D(i);
            return ret;
        }

        // value semantics
        public override bool Equals(object? obj) => (_valueSemantics ??= new(this)).Equals(obj as ReadOnlyTensor4DWrapper);
        // ReSharper disable once NonReadonlyMemberInGetHashCode
        public override int GetHashCode() => (_valueSemantics ??= new(this)).GetHashCode();
        public bool Equals(ReadOnlyTensor4DWrapper? other) => (_valueSemantics ??= new(this)).Equals(other);

        public override string ToString()
        {
            var preview = String.Join("|", Enumerable.Range(0, Consts.DefaultPreviewSize).Select(x => ReadOnlySegment[x]));
            if (Size > Consts.DefaultPreviewSize)
                preview += "|...";
            return $"Read Only Tensor 4D Wrapper (Count: {Count}, Depth: {Depth}, Rows: {RowCount}, Columns: {ColumnCount}) {preview}";
        }

        public ReadOnlySpan<float> ReadOnlySpan => ReadOnlySegment.Contiguous!.ReadOnlySpan;
    }
}
