using System;
using System.IO;
using System.Linq;
using BrightData.LinearAlgebra.ReadOnlyTensorValueSemantics;
using BrightData.LinearAlgebra.Segments;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData.LinearAlgebra.ReadOnly
{
    internal class ReadOnlyTensor3DWrapper : IReadOnlyTensor3D, IEquatable<ReadOnlyTensor3DWrapper>, IHaveReadOnlyContiguousSpan<float>
    {
        readonly ReadOnlyTensor3DValueSemantics<ReadOnlyTensor3DWrapper> _valueSemantics;

        public ReadOnlyTensor3DWrapper(IReadOnlyNumericSegment<float> segment, uint depth, uint rowCount, uint columnCount)
        {
            Depth = depth;
            RowCount = rowCount;
            ColumnCount = columnCount;
            ReadOnlySegment = segment;
            _valueSemantics = new(this);
        }

        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(3);
            writer.Write(ColumnCount);
            writer.Write(RowCount);
            writer.Write(Depth);
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
        public ReadOnlySpan<float> FloatSpan => ReadOnlySegment.GetSpan();

        public uint Size => MatrixSize * Depth;
        public IReadOnlyNumericSegment<float> ReadOnlySegment { get; }
        public uint Depth { get; }
        public uint RowCount { get; }
        public uint ColumnCount { get; }
        public uint MatrixSize => RowCount * ColumnCount;
        public bool IsReadOnly => true;

        public float this[int depth, int rowY, int columnX] => ReadOnlySegment[depth * MatrixSize + columnX * RowCount + rowY];
        public float this[uint depth, uint rowY, uint columnX] => ReadOnlySegment[depth * MatrixSize + columnX * RowCount + rowY];

        public ITensor3D Create(LinearAlgebraProvider lap) => lap.CreateTensor3D(this);
        public IReadOnlyMatrix GetMatrix(uint index)
        {
            var segment = new ReadOnlyTensorSegmentWrapper(ReadOnlySegment, index * MatrixSize, 1, MatrixSize);
            return new ReadOnlyMatrixWrapper(segment, RowCount, ColumnCount);
        }
        public IReadOnlyMatrix[] AllMatrices()
        {
            var ret = new IReadOnlyMatrix[Depth];
            for (uint i = 0; i < Depth; i++)
                ret[i] = GetMatrix(i);
            return ret;
        }

        // value semantics
        public override bool Equals(object? obj) => _valueSemantics.Equals(obj as ReadOnlyTensor3DWrapper);
        public override int GetHashCode() => _valueSemantics.GetHashCode();
        public bool Equals(ReadOnlyTensor3DWrapper? other) => _valueSemantics.Equals(other);

        public override string ToString()
        {
            var preview = String.Join("|", Enumerable.Range(0, Consts.DefaultPreviewSize).Select(x => ReadOnlySegment[x]));
            if (Size > Consts.DefaultPreviewSize)
                preview += "|...";
            return $"Read Only Tensor 3D Wrapper (Depth: {Depth}, Rows: {RowCount}, Columns: {ColumnCount}) {preview}";
        }
    }
}
