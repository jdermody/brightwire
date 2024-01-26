using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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

        ReadOnlyMatrix(ReadOnlyMemory<float> data)
        {
            ReadOnlySegment = new ReadOnlyTensorSegment(data);
            _valueSemantics = new(this);
        }

        /// <summary>
        /// Creates a matrix from bytes
        /// </summary>
        /// <param name="data"></param>
        public ReadOnlyMatrix(ReadOnlySpan<byte> data) : this(data[HeaderSize..].Cast<byte, float>().ToArray())
        {
            ColumnCount = BinaryPrimitives.ReadUInt32LittleEndian(data);
            RowCount = BinaryPrimitives.ReadUInt32LittleEndian(data[4..]);
        }

        /// <summary>
        /// Creates a matrix from float memory
        /// </summary>
        /// <param name="data"></param>
        /// <param name="rows"></param>
        /// <param name="columns"></param>
        public ReadOnlyMatrix(ReadOnlyMemory<float> data, uint rows, uint columns) : this(data)
        {
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
        public unsafe ReadOnlyMatrix(uint rows, uint columns, Func<uint, uint, float> initializer) : this(ReadOnlyMemory<float>.Empty)
        {
            RowCount = rows;
            ColumnCount = columns;
            var data = new float[rows * columns];
            fixed (float* ptr = data) {
                var p = ptr;
                for (uint i = 0, len = (uint)data.Length; i < len; i++)
                    *p++ = initializer(i % rows, i / rows);
            }
            ReadOnlySegment = new ReadOnlyTensorSegment(data);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="rowCount"></param>
        /// <param name="columnCount"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public ReadOnlyMatrix(IReadOnlyNumericSegment<float> segment, uint rowCount, uint columnCount)
        {
            RowCount = rowCount;
            ColumnCount = columnCount;
            ReadOnlySegment = segment;
            _valueSemantics = new(this);
        }

        /// <inheritdoc />
        public IReadOnlyNumericSegment<float> ReadOnlySegment { get; private set; }

        /// <inheritdoc />
        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(2);
            writer.Write(ColumnCount);
            writer.Write(RowCount);
            writer.Write(ReadOnlySegment);
        }

        /// <inheritdoc />
        public void Initialize(BrightDataContext context, BinaryReader reader)
        {
            var size = reader.ReadInt32();
            if (size != 2)
                throw new Exception("Unexpected array size");
            ColumnCount = reader.ReadUInt32();
            RowCount = reader.ReadUInt32();
            ReadOnlySegment = new ReadOnlyTensorSegment(reader.BaseStream.ReadArray<float>(Size));
            Unsafe.AsRef(in _valueSemantics) = new(this);
        }

        /// <inheritdoc />
        public ReadOnlySpan<float> GetSpan(ref SpanOwner<float> temp, out bool wasTempUsed) => ReadOnlySegment.GetSpan(ref temp, out wasTempUsed);

        /// <inheritdoc />
        public ReadOnlySpan<float> ReadOnlySpan
        {
            get
            {
                var contiguous = ReadOnlySegment.Contiguous;
                if (contiguous is not null)
                    return contiguous.ReadOnlySpan;
                return ReadOnlySegment.ToNewArray();
            }
        }

        /// <inheritdoc />
        public uint Size => RowCount * ColumnCount;

        /// <inheritdoc />
        public uint RowCount { get; private set; }

        /// <inheritdoc />
        public uint ColumnCount { get; private set; }

        /// <inheritdoc />
        public float this[int rowY, int columnX] => ReadOnlySegment[(int)(columnX * RowCount + rowY)];

        /// <inheritdoc />
        public float this[uint rowY, uint columnX] => ReadOnlySegment[columnX * RowCount + rowY];

        /// <inheritdoc />
        public IMatrix Create(LinearAlgebraProvider lap) => lap.CreateMatrix(RowCount, ColumnCount, ReadOnlySegment);

        /// <inheritdoc />
        public IReadOnlyNumericSegment<float> GetReadOnlyRow(uint index) => new ReadOnlyTensorSegmentWrapper(ReadOnlySegment, index, RowCount, ColumnCount);

        /// <inheritdoc />
        public IReadOnlyNumericSegment<float> GetReadOnlyColumn(uint index) => new ReadOnlyTensorSegmentWrapper(ReadOnlySegment, index * RowCount, 1, RowCount);

        /// <inheritdoc />
        public override bool Equals(object? obj) => _valueSemantics.Equals(obj as ReadOnlyMatrix);

        /// <inheritdoc />
        public bool Equals(ReadOnlyMatrix? other) => _valueSemantics.Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => _valueSemantics.GetHashCode();

        /// <inheritdoc />
        public override string ToString()
        {
            var preview = String.Join("|", ReadOnlySegment.Values.Take(Consts.DefaultPreviewSize));
            if (Size > Consts.DefaultPreviewSize)
                preview += "|...";
            return $"Read Only Matrix (Rows: {RowCount}, Columns: {ColumnCount}) {preview}";
        }

        /// <inheritdoc />
        public ReadOnlySpan<byte> DataAsBytes
        {
            get
            {
                var buffer = ReadOnlySegment.AsBytes();
                var ret = new Span<byte>(new byte[buffer.Length + HeaderSize]);
                BinaryPrimitives.WriteUInt32LittleEndian(ret, ColumnCount);
                BinaryPrimitives.WriteUInt32LittleEndian(ret[4..], RowCount);
                buffer.CopyTo(ret[HeaderSize..]);
                return ret;
            }
        }
    }
}
