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
    public partial class BrightDataTable : IDisposable
    {
        internal struct Header
        {
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
        readonly MethodInfo                     _getReader;
        readonly uint[]                         _columnOffset;

        public BrightDataTable(BrightDataContext context, Stream stream, uint bufferSize = 32768)
        {
            _context = context;
            _stream = new StreamCloner(stream);
            _bufferSize = bufferSize;
            using var reader = new BinaryReader(stream, Encoding.UTF8);

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
            _stream.Dispose();
        }

        public ICanEnumerateDisposable<T> ReadColumn<T>(uint columnIndex) where T : notnull => GetColumnReader<T>(columnIndex);

        public T Get<T>(uint rowIndex, uint columnIndex)
        {
            using var reader = GetColumnReader<T>(columnIndex, size => size * rowIndex);
            return reader.EnumerateTyped().First();
        }

        public object[] GetRow(uint rowIndex)
        {
            var readers = new ICanEnumerateDisposable[_header.ColumnCount];
            try {
                for (uint i = 0; i < _header.ColumnCount; i++)
                    readers[i] = GetColumnReader(i, size => size * rowIndex);
                return readers.Select(r => r.Enumerate().First()).ToArray();
            }
            finally {
                foreach(var item in readers)
                    item.Dispose();
            }
        }

        ICanEnumerateDisposable<T> GetColumnReader<T>(uint columnIndex, Func<uint, uint>? offsetAdjuster = null)
        {
            ref readonly var column = ref _columns[columnIndex];
            if (column.DataType.GetDataType() != typeof(T))
                throw new ArgumentException($"Data types do not align - expected {column.DataType.GetDataType()} but received {typeof(T)}", nameof(T));
            
            var offset = _columnOffset[columnIndex];
            if(offsetAdjuster is not null)
                offset += offsetAdjuster(column.DataTypeSize);
            var (columnDataType, _) = column.DataType.GetColumnType();
            return (ICanEnumerateDisposable<T>)_getReader.MakeGenericMethod(columnDataType, typeof(T)).Invoke(this, new object[] { _stream.Clone(offset) })!;
        }

        ICanEnumerateDisposable GetColumnReader(uint columnIndex, Func<uint, uint>? offsetAdjuster = null)
        {
            ref readonly var column = ref _columns[columnIndex];

            var offset = _columnOffset[columnIndex];
            if(offsetAdjuster is not null)
                offset += offsetAdjuster(column.DataTypeSize);
            var (columnDataType, _) = column.DataType.GetColumnType();
            var dataType = column.DataType.GetDataType();
            return (ICanEnumerateDisposable)_getReader.MakeGenericMethod(columnDataType, dataType).Invoke(this, new object[] { _stream.Clone(offset) })!;
        }
    }
}
