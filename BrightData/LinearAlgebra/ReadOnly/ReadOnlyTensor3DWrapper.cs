using System;
using System.IO;
using Microsoft.Toolkit.HighPerformance;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace BrightData.LinearAlgebra.ReadOnly
{
    internal class ReadOnlyTensor3DWrapper : IReadOnlyTensor3D
    {
        public ReadOnlyTensor3DWrapper(ITensorSegment segment, uint depth, uint rowCount, uint columnCount)
        {
            Depth = depth;
            RowCount = rowCount;
            ColumnCount = columnCount;
            Segment = segment;
        }

        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(3);
            writer.Write(ColumnCount);
            writer.Write(RowCount);
            writer.Write(Depth);
            var temp = SpanOwner<float>.Empty;
            Segment.GetSpan(ref temp, out var wasTempUsed);
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

        public ReadOnlySpan<float> GetSpan(ref SpanOwner<float> temp, out bool wasTempUsed) => Segment.GetSpan(ref temp, out wasTempUsed);

        public uint Size => MatrixSize * Depth;
        public ITensorSegment Segment { get; }
        public uint Depth { get; }
        public uint RowCount { get; }
        public uint ColumnCount { get; }
        public uint MatrixSize => RowCount * ColumnCount;

        public float this[int depth, int rowY, int columnX] => Segment[depth * MatrixSize + columnX * RowCount + rowY];
        public float this[uint depth, uint rowY, uint columnX] => Segment[depth * MatrixSize + columnX * RowCount + rowY];

        public ITensor3D Create(LinearAlgebraProvider lap) => lap.CreateTensor3D(this);

        public IReadOnlyMatrix GetReadOnlyMatrix(uint index)
        {
            var segment = new TensorSegmentWrapper(Segment, index * MatrixSize, 1, MatrixSize);
            return new ReadOnlyMatrixWrapper(segment, RowCount, ColumnCount);
        }
        public IReadOnlyMatrix[] AllMatrices()
        {
            var ret = new IReadOnlyMatrix[Depth];
            for (uint i = 0; i < Depth; i++)
                ret[i] = GetReadOnlyMatrix(i);
            return ret;
        }
    }
}
