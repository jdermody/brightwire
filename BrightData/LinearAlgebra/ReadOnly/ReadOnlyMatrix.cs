using System;
using System.Buffers.Binary;
using System.IO;
using System.Linq;
using System.Numerics;
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
    public class ReadOnlyMatrix<T> : ReadOnlyTensorBase<T, IReadOnlyMatrix<T>>, IReadOnlyMatrix<T>, IEquatable<ReadOnlyMatrix<T>>, IHaveReadOnlyContiguousSpan<T>, IHaveDataAsReadOnlyByteSpan
        where T: unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        const int HeaderSize = 8;
        readonly ReadOnlyMatrixValueSemantics<T, ReadOnlyMatrix<T>> _valueSemantics;

        ReadOnlyMatrix(ReadOnlyMemory<T> data) : base(new ReadOnlyTensorSegment<T>(data))
        {
            _valueSemantics = new(this);
        }

        /// <summary>
        /// Creates a matrix from bytes
        /// </summary>
        /// <param name="data"></param>
        public ReadOnlyMatrix(ReadOnlySpan<byte> data) : this(data[HeaderSize..].Cast<byte, T>().ToArray())
        {
            ColumnCount = BinaryPrimitives.ReadUInt32LittleEndian(data);
            RowCount = BinaryPrimitives.ReadUInt32LittleEndian(data[4..]);
            if (ColumnCount * RowCount != ReadOnlySegment.Size)
                throw new ArgumentException($"Expected rows ({RowCount}) * columns ({ColumnCount}) to equal input size ({ReadOnlySegment.Size})");
        }

        /// <summary>
        /// Creates a matrix from float memory
        /// </summary>
        /// <param name="data"></param>
        /// <param name="rows"></param>
        /// <param name="columns"></param>
        public ReadOnlyMatrix(ReadOnlyMemory<T> data, uint rows, uint columns) : this(data)
        {
            RowCount = rows;
            ColumnCount = columns;
            if (ColumnCount * RowCount != ReadOnlySegment.Size)
                throw new ArgumentException($"Expected rows ({RowCount}) * columns ({ColumnCount}) to equal input size ({ReadOnlySegment.Size})");
        }

        /// <summary>
        /// Creates an empty matrix
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="columns"></param>
        public ReadOnlyMatrix(uint rows, uint columns) : this(new T[rows * columns], rows, columns)
        {
        }

        /// <summary>
        /// Creates a matrix from the initializer
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="columns"></param>
        /// <param name="initializer"></param>
        public unsafe ReadOnlyMatrix(uint rows, uint columns, Func<uint, uint, T> initializer) : this(ReadOnlyMemory<T>.Empty)
        {
            RowCount = rows;
            ColumnCount = columns;
            var data = new T[rows * columns];
            fixed (T* ptr = data) {
                var p = ptr;
                for (uint i = 0, len = (uint)data.Length; i < len; i++)
                    *p++ = initializer(i % rows, i / rows);
            }
            ReadOnlySegment = new ReadOnlyTensorSegment<T>(data);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="rowCount"></param>
        /// <param name="columnCount"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public ReadOnlyMatrix(IReadOnlyNumericSegment<T> segment, uint rowCount, uint columnCount) : base(segment)
        {
            RowCount = rowCount;
            ColumnCount = columnCount;
            _valueSemantics = new(this);
            if (ColumnCount * RowCount != ReadOnlySegment.Size)
                throw new ArgumentException($"Expected rows ({RowCount}) * columns ({ColumnCount}) to equal input size ({ReadOnlySegment.Size})");
        }

        /// <inheritdoc />
        public override void WriteTo(BinaryWriter writer)
        {
            writer.Write(2);
            writer.Write(ColumnCount);
            writer.Write(RowCount);
            writer.Write(ReadOnlySegment);
        }

        /// <inheritdoc />
        public override void Initialize(BrightDataContext context, BinaryReader reader)
        {
            var size = reader.ReadInt32();
            if (size != 2)
                throw new Exception("Unexpected array size");
            ColumnCount = reader.ReadUInt32();
            RowCount = reader.ReadUInt32();
            ReadOnlySegment = new ReadOnlyTensorSegment<T>(reader.BaseStream.ReadArray<T>(ColumnCount * RowCount));
            Unsafe.AsRef(in _valueSemantics) = new(this);
        }

        /// <inheritdoc />
        public ReadOnlySpan<T> ReadOnlySpan
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
        public uint RowCount { get; private set; }

        /// <inheritdoc />
        public uint ColumnCount { get; private set; }

        /// <inheritdoc />
        public T this[int rowY, int columnX] => ReadOnlySegment[(int)(columnX * RowCount + rowY)];

        /// <inheritdoc />
        public T this[uint rowY, uint columnX] => ReadOnlySegment[columnX * RowCount + rowY];

        /// <inheritdoc />
        public IMatrix<T> Create(LinearAlgebraProvider<T> lap) => lap.CreateMatrix(RowCount, ColumnCount, ReadOnlySegment);

        /// <inheritdoc />
        public IReadOnlyMatrix<T> Transpose()
        {
            var (segment, rowCount, columnCount) = ReadOnlySegment.Transpose(RowCount, ColumnCount);
            return new ReadOnlyMatrix<T>(segment, rowCount, columnCount);
        }

        /// <inheritdoc />
        public IReadOnlyNumericSegment<T> GetReadOnlyRow(uint index) => new ReadOnlyTensorSegmentWrapper<T>(ReadOnlySegment, index, RowCount, ColumnCount);

        /// <inheritdoc />
        public IReadOnlyNumericSegment<T> GetReadOnlyColumn(uint index) => new ReadOnlyTensorSegmentWrapper<T>(ReadOnlySegment, index * RowCount, 1, RowCount);

        /// <inheritdoc />
        public override bool Equals(object? obj) => _valueSemantics.Equals(obj as ReadOnlyMatrix<T>);

        /// <inheritdoc />
        public bool Equals(ReadOnlyMatrix<T>? other) => _valueSemantics.Equals(other);

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
        public override ReadOnlySpan<byte> DataAsBytes => GetDataAsBytes(ReadOnlySegment, RowCount, ColumnCount);

        internal static ReadOnlySpan<byte> GetDataAsBytes(IReadOnlyNumericSegment<T> segment, uint rowCount, uint columnCount)
        {
            var buffer = segment.AsBytes();
            var ret = new Span<byte>(new byte[buffer.Length + HeaderSize]);
            BinaryPrimitives.WriteUInt32LittleEndian(ret, columnCount);
            BinaryPrimitives.WriteUInt32LittleEndian(ret[4..], rowCount);
            buffer.CopyTo(ret[HeaderSize..]);
            return ret;
        }

        /// <inheritdoc />
        protected override ReadOnlyMatrix<T> Create(MemoryOwner<T> memory)
        {
            try {
                return new ReadOnlyMatrix<T>(memory.Span.ToArray(), RowCount, ColumnCount);
            }
            finally {
                memory.Dispose();
            }
        }
    }
}
