using System;
using System.IO;
using System.Linq;
using BrightData.LinearAlgebra;
using BrightData.LinearAlgebra.ReadOnly;
using BrightData.LinearAlgebra.ReadOnlyTensorValueSemantics;
using BrightData.LinearAlgebra.Segments;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData.DataTable.TensorData
{
    internal class Tensor3DData : IReadOnlyTensor3D, IEquatable<Tensor3DData>, IHaveReadOnlyContiguousFloatSpan
    {
        readonly ReadOnlyTensor3DValueSemantics<Tensor3DData> _valueSemantics;
        ICanRandomlyAccessUnmanagedData<float> _data;
        IReadOnlyTensorSegment? _segment;
        uint _startIndex;

        public Tensor3DData(ICanRandomlyAccessUnmanagedData<float> data, uint depth, uint rowCount, uint columnCount, uint startIndex)
        {
            _data = data;
            _startIndex = startIndex;
            Depth = depth;
            RowCount = rowCount;
            ColumnCount = columnCount;
            _valueSemantics = new(this);
        }

        public uint Depth { get; private set; }
        public uint RowCount { get; private set; }
        public uint ColumnCount { get; private set; }
        public uint MatrixSize => RowCount * ColumnCount;
        public bool IsReadOnly => true;

        public float this[int depth, int rowY, int columnX]
        {
            get
            {
                _data.Get((int)(_startIndex + depth * MatrixSize + rowY * (int)ColumnCount + columnX), out var ret);
                return ret;
            }
        }

        public float this[uint depth, uint rowY, uint columnX]
        {
            get
            {
                _data.Get(_startIndex + depth * MatrixSize + rowY * ColumnCount + columnX, out var ret);
                return ret;
            }
        }

        public ReadOnlySpan<float> GetFloatSpan(ref SpanOwner<float> temp, out bool wasTempUsed)
        {
            wasTempUsed = false;
            return FloatSpan;
        }
        public ReadOnlySpan<float> FloatSpan => _data.GetSpan(_startIndex, Size);

        public ITensor3D Create(LinearAlgebraProvider lap)
        {
            var size = Size;
            var span = _data.GetSpan(_startIndex, size);
            var segment = lap.CreateSegment(size, false);
            segment.CopyFrom(span);
            return lap.CreateTensor3D(Depth, RowCount, ColumnCount, segment);
        }

        public IReadOnlyMatrix GetMatrix(uint index) => new MatrixData(_data, RowCount, ColumnCount, index * MatrixSize);

        public IReadOnlyMatrix[] AllMatrices()
        {
            var ret = new IReadOnlyMatrix[Depth];
            for (uint i = 0; i < Depth; i++)
                ret[i] = GetMatrix(i);
            return ret;
        }

        public void Initialize(BrightDataContext context, BinaryReader reader)
        {
            if (reader.ReadInt32() != 3)
                throw new Exception("Unexpected array size");
            ColumnCount = reader.ReadUInt32();
            RowCount = reader.ReadUInt32();
            Depth = reader.ReadUInt32();
            _startIndex = 0;
            _data = new TempFloatData(reader, Size);
        }

        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(3);
            writer.Write(ColumnCount);
            writer.Write(RowCount);
            writer.Write(Depth);
            writer.Write(_data.GetSpan(_startIndex, Size).AsBytes());
        }

        public uint Size => Depth * ColumnCount * RowCount;
        public IReadOnlyTensorSegment ReadOnlySegment => _segment ??= new ArrayBasedTensorSegment(this.ToArray());

        // value semantics
        public override bool Equals(object? obj) => _valueSemantics.Equals(obj as Tensor3DData);
        public bool Equals(Tensor3DData? other) => _valueSemantics.Equals(other);
        public override int GetHashCode() => _valueSemantics.GetHashCode();

        public override string ToString()
        {
            var preview = String.Join("|", Math.Min(Consts.DefaultPreviewSize, Size).AsRange().Select(i => {
                _data.Get(_startIndex + i, out var ret);
                return ret;
            }));
            if (Size > Consts.DefaultPreviewSize)
                preview += "|...";
            return $"Tensor 3D Data (Depth: {Depth}, Rows: {RowCount}, Columns: {ColumnCount}) {preview}";
        }
    }
}
