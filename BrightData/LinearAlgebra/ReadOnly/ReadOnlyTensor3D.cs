using System;
using System.IO;
using Microsoft.Toolkit.HighPerformance;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace BrightData.LinearAlgebra.ReadOnly
{
    internal class ReadOnlyTensor3D : IReadOnlyTensor3D
    {
        IReadOnlyMatrix[] _matrices;
        ITensorSegment? _segment;

        public ReadOnlyTensor3D(IReadOnlyMatrix[] matrices)
        {
            _matrices = matrices;
            Depth = (uint)_matrices.Length;
            var firstMatrix = _matrices[0];
            RowCount = firstMatrix.RowCount;
            ColumnCount = firstMatrix.ColumnCount;
        }

        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(3);
            writer.Write(ColumnCount);
            writer.Write(RowCount);
            writer.Write(Depth);
            foreach (var item in _matrices) {
                var temp = SpanOwner<float>.Empty;
                var span = item.GetFloatSpan(ref temp, out var wasTempUsed);
                writer.Write(span.AsBytes());
                if(wasTempUsed)
                    temp.Dispose();
            }
        }

        public void Initialize(BrightDataContext context, BinaryReader reader)
        {
            if (reader.ReadInt32() != 3)
                throw new Exception("Unexpected array size");
            ColumnCount = reader.ReadUInt32();
            RowCount = reader.ReadUInt32();
            Depth = reader.ReadUInt32();
            _matrices = new IReadOnlyMatrix[Depth];
            for (var i = 0; i < Depth; i++) {
                var buffer = reader.BaseStream.ReadArray<float>(MatrixSize);
                _matrices[i] = new ReadOnlyMatrixWrapper(new ArrayBasedTensorSegment(buffer), RowCount, ColumnCount);
            }
            _segment = null;
        }

        public ReadOnlySpan<float> GetFloatSpan(ref SpanOwner<float> temp, out bool wasTempUsed) => Segment.GetSpan(ref temp, out wasTempUsed);
        public ITensorSegment Segment
        {
            get
            {
                if (_segment is null) {
                    var data = new float[Size];
                    var ptr = data.AsSpan();
                    uint offset = 0;
                    foreach (var matrix in _matrices) {
                        var temp = SpanOwner<float>.Empty;
                        var span = matrix.GetFloatSpan(ref temp, out var wasTempUsed);
                        span.CopyTo(ptr.Slice((int)offset, (int)MatrixSize));
                        if(wasTempUsed)
                            temp.Dispose();
                        offset += MatrixSize;
                    }
                    _segment = new ArrayBasedTensorSegment(data);
                }

                return _segment;
            }
        }

        public uint Size => MatrixSize * Depth;
        public uint Depth { get; private set; }
        public uint RowCount { get; private set; }
        public uint ColumnCount { get; private set; }
        public uint MatrixSize => RowCount * ColumnCount;

        public float this[int depth, int rowY, int columnX] => _matrices[depth][rowY, columnX];
        public float this[uint depth, uint rowY, uint columnX] => _matrices[depth][rowY, columnX];

        public ITensor3D Create(LinearAlgebraProvider lap) => lap.CreateTensor3D(this);
        public IReadOnlyMatrix GetReadOnlyMatrix(uint index) => _matrices[index];
        public IReadOnlyMatrix[] AllMatrices() => _matrices;
    }
}
