using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BrightData.DataTable2.Bindings;
using BrightData.Helper;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace BrightData.DataTable2
{
    public class BrightDataTable : IDisposable
    {
        internal struct Header
        {
            public DataTableOrientation Orientation;
            public uint ColumnCount;
            public uint RowCount;
            public uint DataOffset;
            public uint DataSizeBytes;
            public uint StringOffset;
            public uint TensorOffset;
            public uint BinaryDataOffset;
            public uint IndexOffset;
            public uint WeightedIndexOffset;
        }
        internal struct Column
        {
            public BrightDataType DataType;
            public uint DataTypeSize;
        }

        readonly BrightDataContext              _context;
        readonly StreamCloner                   _stream;
        readonly uint                           _bufferSize;
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
        readonly MethodInfo _getReader;

        public BrightDataTable(BrightDataContext context, Stream stream, uint bufferSize = 32768)
        {
            _context = context;
            _stream = new StreamCloner(stream);
            _bufferSize = bufferSize;
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

            // setup the lazy readers
            T[] ReadArray<T>(uint offset) where T: notnull
            {
                if(offset == 0)
                    return Array.Empty<T>();

                using var stream = _stream.Clone(offset);
                using var reader = stream.GetReader();
                return _context.GetBufferReader<T>(reader, _bufferSize).EnumerateTyped().ToArray();
            }
            _stringTable     = new(() => ReadArray<string>(_header.StringOffset));
            _binaryData      = new(() => ReadArray<byte>(_header.BinaryDataOffset));
            _tensors         = new(() => ReadArray<float>(_header.TensorOffset));
            _indices         = new(() => ReadArray<uint>(_header.IndexOffset));
            _weightedIndices = new(() => ReadArray<WeightedIndexList.Item>(_header.WeightedIndexOffset));

            var genericMethods = GetType().GetGenericMethods();
            _getReader = genericMethods[nameof(GetReader)];
        }
        

        public void Dispose()
        {
            _stream.Dispose();
        }

        public ICanEnumerateDisposable<T> ReadColumn<T>(uint columnIndex)
            where T: notnull
        {
            ref var column = ref _columns[columnIndex];
            if (column.DataType.GetDataType() != typeof(T))
                throw new ArgumentException($"Data types do not align - expected {column.DataType.GetDataType()} but received {typeof(T)}", nameof(T));

            // find the offset to the column
            var columnDataOffset = _header.BinaryDataOffset;
            for (uint i = 0; i < columnIndex; i++) {
                ref var previousColumn = ref _columns[i];
                columnDataOffset += previousColumn.DataTypeSize * _header.RowCount;
            }

            var (columnDataType, _) = column.DataType.GetColumnType();
            var stream = _stream.Clone(_header.DataOffset + columnDataOffset);
            return (ICanEnumerateDisposable<T>)_getReader.MakeGenericMethod(columnDataType, typeof(T)).Invoke(this, new object[] { stream, column.DataTypeSize })!;
        }

        ICanEnumerate<T> GetReader<CT, T>(ICanReadSection stream, uint columnDataTypeSize)
            where T: notnull
            where CT: struct
        {
            var enumerator = stream.GetStructByReferenceEnumerator<CT>(_header.RowCount);
            var converter = GetConverter<CT, T>();
            return new ColumnReaderBinding<CT, T>(enumerator, converter);
        }

        IConvertStructsToObjects<CT, T> GetConverter<CT, T>()
            where CT : struct
            where T: notnull
        {
            var dataType = typeof(T);
            //var converterType = typeof(CT);
            if (dataType == typeof(string)) {
                var ret = new StringColumnConverter(_stringTable.Value);
                return (IConvertStructsToObjects<CT, T>)ret;
            }
            return new NopColumnConverter<CT, T>();
        }

        class NopColumnConverter<CT, T> : IConvertStructsToObjects<CT, T>
            where CT : struct
            where T: notnull
        {
            public T Convert(ref CT item)
            {
                return __refvalue(__makeref(item), T);
            }
        }

        class StringColumnConverter : IConvertStructsToObjects<uint, string>
        {
            readonly string[] _stringTable;

            public StringColumnConverter(string[] stringTable)
            {
                _stringTable = stringTable;
            }

            public string Convert(ref uint item) => _stringTable[item];
        }

        //static void ReadColumn<CT, T>(
        //    Stream stream, 
        //    uint offset,
        //    uint sizeBytes,
        //    ColumnReaderBinding<CT, T> binding, 
        //    CancellationToken cancellationToken, 
        //    int bufferSize = 4096
        //)
        //    where CT : struct
        //    where T: notnull
        //{
        //    var dataSize = Unsafe.SizeOf<CT>();
        //    var count = sizeBytes / dataSize;

        //    lock (stream) {
        //        stream.Seek(offset, SeekOrigin.Begin);
        //        using var buffer = SpanOwner<CT>.Allocate(Math.Min(bufferSize, (int)count));
        //        var ptr = MemoryMarshal.AsBytes(buffer.Span);
        //        uint index = 0;

        //        do {
        //            var readBytes = stream.Read(ptr);
        //            if (readBytes == 0)
        //                break;

        //            foreach (ref var item in buffer.Span[..(readBytes / dataSize)]) {
        //                binding.OnItem(ref item, index++);
        //                if (cancellationToken.IsCancellationRequested)
        //                    break;
        //            }
        //        } while (index < count && !cancellationToken.IsCancellationRequested);
        //    }
        //}

        //static string[] ReadStringTable(Stream stream, uint offset, uint sizeBytes)
        //{
        //    using var reader = new BinaryReader(stream, Encoding.UTF8);
        //    _context.GetBufferReader<string>(reader, bufferSize);
        //    lock (stream) {
        //        stream.Seek(offset, SeekOrigin.Begin);
        //        var ret = new string[count];
        //        for (uint i = 0; i < count; i++)
        //            ret[i] = reader.ReadString();
        //        return ret;
        //    }
        //}

        //static byte[] ReadBinaryData(Stream stream, uint offset, uint sizeBytes, uint bufferSize)
        //{
        //    using var reader = new BinaryReader(stream, Encoding.UTF8);
        //    lock (stream) {
        //        stream.Seek(offset, SeekOrigin.Begin);
        //        var ret = new byte[count];
        //        var bytesRead = stream.Read(ret.AsSpan());
        //        return ret;
        //    }
        //}

        //static T[] ReadStruct<T>(Stream stream, uint offset, uint sizeBytes, uint bufferSize)
        //    where T : struct
        //{
        //    lock (stream) {
        //        stream.Seek(offset, SeekOrigin.Begin);
        //        var ret = new T[count];
        //        var bytesRead = stream.Read(MemoryMarshal.AsBytes(ret.AsSpan()));
        //        return ret;
        //    }
        //}
    }
}
