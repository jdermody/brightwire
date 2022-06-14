using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BrightData.DataTable2.Bindings;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace BrightData.DataTable2
{
    internal class DataTable : IDisposable
    {
        internal struct Header
        {
            public DataTableOrientation Orientation;
            public uint ColumnCount;
            public uint RowCount;

            public uint DataOffset;
            public uint DataSizeBytes;

            public uint StringOffset;
            public uint StringCount;

            public uint TensorOffset;
            public uint TensorCount;

            public uint BinaryDataOffset;
            public uint BinaryDataCount;

            public uint IndexOffset;
            public uint IndexCount;

            public uint WeightedIndexOffset;
            public uint WeightedIndexCount;
        }
        internal struct Column
        {
            public BrightDataType DataType;
            public uint DataTypeSize;
        }

        readonly Stream                         _stream;
        readonly Header                         _header;
        readonly Column[]                       _columns;
        readonly MetaData[]                     _metaData;
        readonly Lazy<string[]>                 _stringTable;
        readonly Lazy<float[]>                  _tensors;
        readonly Lazy<byte[]>                   _binaryData;
        readonly Lazy<uint[]>                   _indices;
        readonly Lazy<WeightedIndexList.Item[]> _weightedIndices;
        readonly uint                           _rowSize;
        readonly long                           _dataOffset;

        public DataTable(Stream stream)
        {
            _stream = stream;
            using var reader = new BinaryReader(stream, Encoding.UTF8);

            // read the header
            _header = MemoryMarshal.Cast<byte, Header>(reader.ReadBytes(Unsafe.SizeOf<Header>()))[0];
            var numColumns = (int)_header.ColumnCount;

            // read the meta data
            _metaData = new MetaData[numColumns + 1];
            for (uint i = 0; i < numColumns + 1; i++)
                _metaData[i] = new MetaData(reader);

            // read the columns
            _columns = new Column[numColumns];
            MemoryMarshal
                .Cast<byte, Column>(reader.ReadBytes(Unsafe.SizeOf<Column>() * numColumns))
                .CopyTo(_columns)
            ;
            _rowSize = (uint)_columns.Sum(c => c.DataTypeSize);
            _dataOffset = stream.Position;

            // setup the lazy readers
            T[] ReadArray<T>(uint offset, uint count, Func<Stream, uint, uint, T[]> reader) => offset > 0 ? reader(_stream, offset, count) : Array.Empty<T>();
            _stringTable     = new(() => ReadArray(_header.StringOffset, _header.StringCount, ReadStringTable));
            _binaryData      = new(() => ReadArray(_header.BinaryDataOffset, _header.BinaryDataCount, ReadBinaryData));
            _tensors         = new(() => ReadArray(_header.TensorOffset, _header.TensorCount, ReadStruct<float>));
            _indices         = new(() => ReadArray(_header.IndexOffset, _header.IndexCount, ReadStruct<uint>));
            _weightedIndices = new(() => ReadArray(_header.WeightedIndexOffset, _header.WeightedIndexCount, ReadStruct<WeightedIndexList.Item>));
        }
        

        public void Dispose()
        {
            _stream.Dispose();
        }

        public void ReadColumn<T>(uint columnIndex, Action<T, uint> callback, CancellationToken cancellationToken = default)
            where T: notnull
        {
            ref var column = ref _columns[columnIndex];
            if (column.DataType.GetDataType() != typeof(T))
                throw new ArgumentException($"Data types do not align - expected {column.DataType.GetDataType()} but received {typeof(T)}", nameof(T));

            var columnType = column.DataType.GetColumnType();
        }

        static ColumnTypeCallback<CT, T> GetConverter<CT, T>()
            where CT : struct
            where T: notnull
        {
            var converterType = typeof(CT);
            if (converterType == typeof(MatrixColumnType)) {
                var d = MatrixColumnConverter;
                return __refvalue(__makeref(d), ColumnTypeCallback<CT, T>);
            }
            return NopColumnConverter<CT, T>;
        }

        static T NopColumnConverter<CT, T>(ref CT item)
            where CT : struct
            where T: notnull
        {
            return __refvalue(__makeref(item), T);
        }

        static IMatrix MatrixColumnConverter(ref MatrixColumnType matrix)
        {
            return null;
        }

        static void ReadColumn<CT, T>(
            Stream stream, 
            uint offset, 
            uint count, 
            ColumnReaderBinding<CT, T> binding, 
            CancellationToken cancellationToken, 
            int bufferSize = 4096
        )
            where CT : struct
            where T: notnull
        {
            var dataSize = Unsafe.SizeOf<CT>();
            lock (stream) {
                stream.Seek(offset, SeekOrigin.Begin);
                using var buffer = SpanOwner<CT>.Allocate(Math.Min(bufferSize, (int)count));
                var ptr = MemoryMarshal.AsBytes(buffer.Span);
                uint index = 0;

                do {
                    var readBytes = stream.Read(ptr);
                    if (readBytes == 0)
                        break;

                    foreach (ref var item in buffer.Span[..(readBytes / dataSize)]) {
                        binding.OnItem(ref item, index++);
                        if (cancellationToken.IsCancellationRequested)
                            break;
                    }
                } while (index < count && !cancellationToken.IsCancellationRequested);
            }
        }

        static string[] ReadStringTable(Stream stream, uint offset, uint count)
        {
            using var reader = new BinaryReader(stream, Encoding.UTF8);
            lock (stream) {
                stream.Seek(offset, SeekOrigin.Begin);
                var ret = new string[count];
                for (uint i = 0; i < count; i++)
                    ret[i] = reader.ReadString();
                return ret;
            }
        }

        static byte[] ReadBinaryData(Stream stream, uint offset, uint count)
        {
            using var reader = new BinaryReader(stream, Encoding.UTF8);
            lock (stream) {
                stream.Seek(offset, SeekOrigin.Begin);
                var ret = new byte[count];
                var bytesRead = stream.Read(ret.AsSpan());
                return ret;
            }
        }

        static T[] ReadStruct<T>(Stream stream, uint offset, uint count)
            where T : struct
        {
            lock (stream) {
                stream.Seek(offset, SeekOrigin.Begin);
                var ret = new T[count];
                var bytesRead = stream.Read(MemoryMarshal.AsBytes(ret.AsSpan()));
                return ret;
            }
        }
    }
}
