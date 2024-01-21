using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BrightData.LinearAlgebra.ReadOnlyTensorValueSemantics;
using BrightData.LinearAlgebra.Segments;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData.LinearAlgebra.ReadOnly
{
    /// <summary>
    /// Read only matrix
    /// </summary>
    public class ReadOnlyMatrix : IReadOnlyMatrix, IEquatable<ReadOnlyMatrix>, IHaveReadOnlyContiguousSpan<float>, IHaveDataAsReadOnlyByteSpan
    {
        const int HeaderSize = 8;
        readonly ReadOnlyMatrixValueSemantics<ReadOnlyMatrix> _valueSemantics;
        ReadOnlyMemory<float> _data;
        IReadOnlyNumericSegment<float>? _segment;

        ReadOnlyMatrix()
        {
            _valueSemantics = new(this);
        }

        /// <summary>
        /// Creates a matrix from bytes
        /// </summary>
        /// <param name="data"></param>
        public ReadOnlyMatrix(ReadOnlySpan<byte> data) : this()
        {
            ColumnCount = BinaryPrimitives.ReadUInt32LittleEndian(data);
            RowCount = BinaryPrimitives.ReadUInt32LittleEndian(data[4..]);
            _data = data[HeaderSize..].Cast<byte, float>().ToArray();
        }

        /// <summary>
        /// Creates a matrix from float memory
        /// </summary>
        /// <param name="data"></param>
        /// <param name="rows"></param>
        /// <param name="columns"></param>
        public ReadOnlyMatrix(ReadOnlyMemory<float> data, uint rows, uint columns) : this()
        {
            _data = data;
            RowCount = rows;
            ColumnCount = columns;
        }

        /// <summary>
        /// Creates an empty matrix
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="columns"></param>
        public ReadOnlyMatrix(uint rows, uint columns) : this(new float[rows * columns], rows, columns)
        {
        }

        /// <summary>
        /// Creates a matrix from the initializer
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="columns"></param>
        /// <param name="initializer"></param>
        public unsafe ReadOnlyMatrix(uint rows, uint columns, Func<uint, uint, float> initializer) : this()
        {
            RowCount = rows;
            ColumnCount = columns;
            _data = new float[rows * columns];
            fixed (float* ptr = _data.Span) {
                var p = ptr;
                for (uint i = 0, len = (uint)_data.Length; i < len; i++)
                    *p++ = initializer(i % rows, i / rows);
            }
        }

        /// <inheritdoc />
        public IReadOnlyNumericSegment<float> ReadOnlySegment => _segment ??= new ReadOnlyMemoryTensorSegment(_data[..(int)Size]);

        /// <inheritdoc />
        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(2);
            writer.Write(ColumnCount);
            writer.Write(RowCount);
            writer.Write(ReadOnlySpan.AsBytes());
        }

        /// <inheritdoc />
        public void Initialize(BrightDataContext context, BinaryReader reader)
        {
            var size = reader.ReadInt32();
            if (size != 2)
                throw new Exception("Unexpected array size");
            ColumnCount = reader.ReadUInt32();
            RowCount = reader.ReadUInt32();
            _data = reader.BaseStream.ReadArray<float>(Size);
        }

        /// <inheritdoc />
        public ReadOnlySpan<float> GetSpan(ref SpanOwner<float> temp, out bool wasTempUsed)
        {
            wasTempUsed = false;
            return ReadOnlySpan;
        }

        /// <inheritdoc />
        public ReadOnlySpan<float> ReadOnlySpan => _data.Span;

        /// <inheritdoc />
        public uint Size => RowCount * ColumnCount;

        /// <inheritdoc />
        public uint RowCount { get; private set; }

        /// <inheritdoc />
        public uint ColumnCount { get;private set; }

        /// <inheritdoc />
        public float this[int rowY, int columnX] => _data.Span[(int)(columnX * RowCount + rowY)];

        /// <inheritdoc />
        public float this[uint rowY, uint columnX] => _data.Span[(int)(columnX * RowCount + rowY)];

        /// <inheritdoc />
        public IMatrix Create(LinearAlgebraProvider lap) => lap.CreateMatrix(RowCount, ColumnCount, ReadOnlySegment);

        /// <summary>
        /// Returns a row segment
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public ReadOnlyTensorSegmentWrapper Row(uint index) => new(ReadOnlySegment, index, RowCount, ColumnCount);

        /// <summary>
        /// Returns a column segment
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public ReadOnlyTensorSegmentWrapper Column(uint index) => new(ReadOnlySegment, index * RowCount, 1, RowCount);

        /// <inheritdoc />
        public IReadOnlyVector GetRow(uint rowIndex) => new ReadOnlyVectorWrapper(Row(rowIndex));

        /// <inheritdoc />
        public IReadOnlyVector GetColumn(uint columnIndex) => new ReadOnlyVectorWrapper(Column(columnIndex));

        /// <summary>
        /// Returns all rows as vectors
        /// </summary>
        /// <returns></returns>
        public IReadOnlyVector[] AllRows() => RowCount.AsRange().Select(GetRow).ToArray();

        /// <summary>
        /// Returns all columns as vectors
        /// </summary>
        /// <returns></returns>
        public IReadOnlyVector[] AllColumns() => ColumnCount.AsRange().Select(GetColumn).ToArray();

        /// <inheritdoc />
        public override bool Equals(object? obj) => _valueSemantics.Equals(obj as ReadOnlyMatrix);

        /// <inheritdoc />
        public bool Equals(ReadOnlyMatrix? other) => _valueSemantics.Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => _valueSemantics.GetHashCode();

        /// <inheritdoc />
        public override string ToString()
        {
            var preview = String.Join("|", Values.Take(Consts.DefaultPreviewSize));
            if (Size > Consts.DefaultPreviewSize)
                preview += "|...";
            return $"Read Only Matrix (Rows: {RowCount}, Columns: {ColumnCount}) {preview}";
        }

        /// <summary>
        /// Enumerates all values in the matrix
        /// </summary>
        public IEnumerable<float> Values
        {
            get
            {
                for(var i = 0; i < _data.Length; i++)
                    yield return _data.Span[i];
            }
        }

        /// <inheritdoc />
        public ReadOnlySpan<byte> DataAsBytes
        {
            get
            {
                var buffer = _data.Span.Cast<float, byte>();
                var ret = new Span<byte>(new byte[buffer.Length + HeaderSize]);
                BinaryPrimitives.WriteUInt32LittleEndian(ret, ColumnCount);
                BinaryPrimitives.WriteUInt32LittleEndian(ret[4..], RowCount);
                buffer.CopyTo(ret[HeaderSize..]);
                return ret;
            }
        }
    }
}
