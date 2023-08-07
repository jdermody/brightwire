using System;
using System.IO;
using System.Linq;
using BrightData.LinearAlgebra.ReadOnlyTensorValueSemantics;
using BrightData.LinearAlgebra.Segments;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData.LinearAlgebra.ReadOnly
{
    internal class ReadOnlyTensor3D : IReadOnlyTensor3D, IEquatable<ReadOnlyTensor3D>, IHaveReadOnlyContiguousFloatSpan
    {
        readonly ReadOnlyTensor3DValueSemantics<ReadOnlyTensor3D> _valueSemantics;
        readonly Lazy<IReadOnlyTensorSegment> _segment;
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
                    var span = matrix.GetFloatSpan(ref temp, out var wasTempUsed);
                    span.CopyTo(ptr.Slice((int)offset, (int)MatrixSize));
                    if (wasTempUsed)
                        temp.Dispose();
                    offset += MatrixSize;
                }

                return new ArrayBasedTensorSegment(data);
            });
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

        public ReadOnlySpan<float> GetFloatSpan(ref SpanOwner<float> temp, out bool wasTempUsed) => ReadOnlySegment.GetSpan(ref temp, out wasTempUsed);
        public IReadOnlyTensorSegment ReadOnlySegment => _segment.Value;
        public ReadOnlySpan<float> FloatSpan => ReadOnlySegment.GetSpan();

        public uint Size => MatrixSize * Depth;
        public uint Depth { get; private set; }
        public uint RowCount { get; private set; }
        public uint ColumnCount { get; private set; }
        public uint MatrixSize => RowCount * ColumnCount;
        public bool IsReadOnly => true;

        public float this[int depth, int rowY, int columnX] => _matrices[depth][rowY, columnX];
        public float this[uint depth, uint rowY, uint columnX] => _matrices[depth][rowY, columnX];

        public ITensor3D Create(LinearAlgebraProvider lap) => lap.CreateTensor3D(this);
        public IReadOnlyMatrix GetMatrix(uint index) => _matrices[index];
        public IReadOnlyMatrix[] AllMatrices() => _matrices;

        // value semantics
        public override bool Equals(object? obj) => _valueSemantics.Equals(obj as ReadOnlyTensor3D);
        public override int GetHashCode() => _valueSemantics.GetHashCode();
        public bool Equals(ReadOnlyTensor3D? other) => _valueSemantics.Equals(other);

        public override string ToString()
        {
            var preview = String.Join("|", Enumerable.Range(0, Consts.DefaultPreviewSize).Select(x => ReadOnlySegment[x]));
            if (Size > Consts.DefaultPreviewSize)
                preview += "|...";
            return $"Read Only Tensor 3D (Depth: {Depth}, Rows: {RowCount}, Columns: {ColumnCount}) {preview}";
        }
    }
}
