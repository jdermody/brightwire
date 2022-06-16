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
using BrightData.Buffer2;
using BrightData.DataTable2.Bindings;
using BrightData.Helper;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace BrightData.DataTable2
{
    public partial class BrightDataTable : IDisposable
    {
        internal struct Header
        {
            public byte Version;
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

        readonly BrightDataContext                                    _context;
        readonly IReadOnlyBuffer                                      _buffer;
        readonly uint                                                 _bufferSize;
        readonly Header                                               _header;
        readonly Column[]                                             _columns;
        readonly MetaData[]                                           _metaData;
        readonly Lazy<string[]>                                       _stringTable;
        readonly Lazy<ICanRandomlyAccessData<float>>                  _tensors;
        readonly Lazy<ICanRandomlyAccessData<byte>>                   _binaryData;
        readonly Lazy<ICanRandomlyAccessData<uint>>                   _indices;
        readonly Lazy<ICanRandomlyAccessData<WeightedIndexList.Item>> _weightedIndices;
        readonly uint                                                 _rowSize;
        readonly long                                                 _dataOffset;
        readonly MethodInfo                                           _getReader;
        readonly uint[]                                               _columnOffset;

        public BrightDataTable(BrightDataContext context, Stream stream, uint bufferSize = 32768)
        {
            if (stream is FileStream fileStream)
                _buffer = new ReadOnlyFileBasedBuffer(fileStream);
            else if (stream is MemoryStream memoryStream)
                _buffer = new ReadOnlyMemoryBasedBuffer(new ReadOnlyMemory<byte>(memoryStream.GetBuffer()));
            else
                throw new ArgumentException("Expected file or memory stream", nameof(stream));

            _context = context;
            _bufferSize = bufferSize;
            using var reader = new BinaryReader(stream, Encoding.UTF8, true);

            // read the header
            _header = MemoryMarshal.Cast<byte, Header>(reader.ReadBytes(Unsafe.SizeOf<Header>()))[0];
            var numColumns = (int)_header.ColumnCount;
            var numRows = _header.RowCount;

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

            // setup the ancillary data
            string[] ReadStringArray(uint offset, uint count)
            {
                if(offset == 0)
                    return Array.Empty<string>();

                // TODO: think about on demand loading
                stream.Seek(_header.StringOffset, SeekOrigin.Begin);
                using var reader = new BinaryReader(stream, Encoding.UTF8, true);
                var ret = _context.GetBufferReader<string>(reader, _bufferSize).EnumerateTyped().ToArray();
                return ret;
            }
            unsafe ICanRandomlyAccessData<T> GetBlockAccessor<T>(uint offset, uint count) where T: unmanaged => offset == 0 
                ? new EmptyBlock<T>() 
                : _buffer.GetBlock<T>(offset, count * sizeof(T));
            _stringTable     = new(() => ReadStringArray(_header.StringOffset, _header.StringCount));
            _binaryData      = new(() => GetBlockAccessor<byte>(_header.BinaryDataOffset, _header.BinaryDataCount));
            _tensors         = new(() => GetBlockAccessor<float>(_header.TensorOffset, _header.TensorCount));
            _indices         = new(() => GetBlockAccessor<uint>(_header.IndexOffset, _header.IndexCount));
            _weightedIndices = new(() => GetBlockAccessor<WeightedIndexList.Item>(_header.WeightedIndexOffset, _header.WeightedIndexCount));

            // resolve generic methods
            var genericMethods = GetType().GetGenericMethods();
            _getReader = genericMethods[nameof(GetReader)];

            // determine column offsets
            _columnOffset = new uint[numColumns];
            _columnOffset[0] = _header.DataOffset;
            for (uint i = 1; i < _columns.Length; i++) {
                ref readonly var previousColumn = ref _columns[i-1];
                _columnOffset[i] = _columnOffset[i-1] + previousColumn.DataTypeSize * numRows;
            }
        }
        
        public void Dispose()
        {
            _buffer.Dispose();
        }

        public ICanEnumerateDisposable<T> ReadColumn<T>(uint columnIndex) where T : notnull => GetColumnReader<T>(columnIndex, _header.RowCount);

        public T Get<T>(uint rowIndex, uint columnIndex)
        {
            using var reader = GetColumnReader<T>(columnIndex, 1, size => size * rowIndex);
            return reader.EnumerateTyped().First();
        }

        public object[] GetRow(uint rowIndex)
        {
            var readers = new ICanEnumerateDisposable[_header.ColumnCount];
            try {
                for (uint i = 0; i < _header.ColumnCount; i++)
                    readers[i] = GetColumnReader(i, 1, size => size * rowIndex);
                return readers.Select(r => r.Enumerate().First()).ToArray();
            }
            finally {
                foreach(var item in readers)
                    item.Dispose();
            }
        }

        ICanEnumerateDisposable<T> GetColumnReader<T>(uint columnIndex, uint countToRead, Func<uint, uint>? offsetAdjuster = null)
        {
            ref readonly var column = ref _columns[columnIndex];
            if (column.DataType.GetDataType() != typeof(T))
                throw new ArgumentException($"Data types do not align - expected {column.DataType.GetDataType()} but received {typeof(T)}", nameof(T));
            
            var offset = _columnOffset[columnIndex];
            if(offsetAdjuster is not null)
                offset += offsetAdjuster(column.DataTypeSize);
            var (columnDataType, _) = column.DataType.GetColumnType();
            var sizeInBytes = countToRead * column.DataTypeSize;
            return (ICanEnumerateDisposable<T>)_getReader.MakeGenericMethod(columnDataType, typeof(T)).Invoke(this, new object[] { offset, sizeInBytes })!;
        }

        ICanEnumerateDisposable GetColumnReader(uint columnIndex, uint countToRead, Func<uint, uint>? offsetAdjuster = null)
        {
            ref readonly var column = ref _columns[columnIndex];

            var offset = _columnOffset[columnIndex];
            if(offsetAdjuster is not null)
                offset += offsetAdjuster(column.DataTypeSize);
            var (columnDataType, _) = column.DataType.GetColumnType();
            var dataType = column.DataType.GetDataType();
            var sizeInBytes = countToRead * column.DataTypeSize;
            return (ICanEnumerateDisposable)_getReader.MakeGenericMethod(columnDataType, dataType).Invoke(this, new object[] { offset, sizeInBytes })!;
        }
    }
}
