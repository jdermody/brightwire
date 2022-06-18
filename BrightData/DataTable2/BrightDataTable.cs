using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BrightData.Buffer;
using BrightData.Buffer.ReadOnly;
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

            public uint MetaDataOffset;
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
        readonly Lazy<MetaData[]>                                     _metaData;
        readonly Lazy<string[]>                                       _stringTable;
        readonly Lazy<ICanRandomlyAccessData<float>>                  _tensors;
        readonly Lazy<ICanRandomlyAccessData<byte>>                   _binaryData;
        readonly Lazy<ICanRandomlyAccessData<uint>>                   _indices;
        readonly Lazy<ICanRandomlyAccessData<WeightedIndexList.Item>> _weightedIndices;
        readonly uint                                                 _rowSize;
        readonly long                                                 _dataOffset;
        readonly MethodInfo                                           _getReader;
        readonly uint[]                                               _columnOffset;
        readonly Stream                                               _stream;

        public BrightDataTable(BrightDataContext context, Stream stream, uint bufferSize = 32768)
        {
            _stream = stream;
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
                lock (_stream) {
                    _stream.Seek(_header.StringOffset, SeekOrigin.Begin);
                    using var reader2 = new BinaryReader(_stream, Encoding.UTF8, true);
                    var ret = _context.GetBufferReader<string>(reader2, _bufferSize).EnumerateTyped().ToArray();
                    return ret;
                }
            }
            MetaData[] ReadMetaData(uint offset)
            {
                var ret = new MetaData[numColumns + 1];
                lock (_stream) {
                    _stream.Seek(_header.MetaDataOffset, SeekOrigin.Begin);
                    using var reader2 = new BinaryReader(_stream, Encoding.UTF8, true);
                    for (uint i = 0; i < numColumns + 1; i++)
                        ret[i] = new MetaData(reader2);
                }
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
            _metaData        = new(() => ReadMetaData(_header.MetaDataOffset));

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

        /// <inheritdoc />
        public void Dispose()
        {
            // _stream will be disposed by the buffer
            _buffer.Dispose();
        }

        public MetaData TableMetaData => _metaData.Value[0];
        public MetaData GetColumnMetaData(uint columnIndex) => _metaData.Value[columnIndex+1];
        public IEnumerable<MetaData> ColumnMetaData => _header.ColumnCount.AsRange().Select(GetColumnMetaData);
        public IEnumerable<uint> ColumnIndices => _header.ColumnCount.AsRange();
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

        public IEnumerable<object[]> GetRows(bool reuseArrayForEachIteration = true)
        {
            var rowCount = _header.RowCount;
            var columnCount = _header.ColumnCount;
            var readers = GetColumnReaders(ColumnIndices);
            var enumerators = readers.Select(r => r.Enumerate().GetEnumerator()).ToArray();
            
            try {
                var ret = new object[columnCount];
                for (uint j = 0; j < rowCount; j++) {
                    if(!reuseArrayForEachIteration && j > 0)
                        ret = new object[columnCount];

                    for (uint i = 0; i < columnCount; i++) {
                        var enumerator = enumerators[i];
                        enumerator.MoveNext();
                        ret[i] = enumerator.Current;
                    }

                    yield return ret;
                }
            }
            finally {
                foreach(var item in enumerators)
                    item.Dispose();
                foreach(var item in readers)
                    item.Dispose();
            }
        }

        public IMetaData GetColumnAnalysis(uint columnIndex, bool force = false, uint writeCount = Consts.MaxWriteCount, uint maxDistinctCount = Consts.MaxDistinct)
        {
            var ret = GetColumnMetaData(columnIndex);
            if (force || !ret.Get(Consts.HasBeenAnalysed, false)) {
                using var operation = CreateColumnAnalyser(columnIndex, writeCount, maxDistinctCount);
                operation.Complete(null, _context.CancellationToken);
                ret.Set(Consts.HasBeenAnalysed, true);
            }

            return ret;
        }
        public IEnumerable<(uint ColumnIndex, IMetaData MetaData)> GetColumnAnalysis(IEnumerable<uint> columnIndices) => columnIndices.Select(ci => (ci, GetColumnAnalysis(ci)));

        public void PersistMetaData()
        {
            using var tempBuffer = new MemoryStream();
            using var metaDataWriter = new BinaryWriter(tempBuffer, Encoding.UTF8, true);
            TableMetaData.WriteTo(metaDataWriter);
            foreach(var item in ColumnMetaData)
                item.WriteTo(metaDataWriter);
            metaDataWriter.Flush();
            lock (_stream) {
                _stream.Seek(_header.MetaDataOffset, SeekOrigin.Begin);
                tempBuffer.WriteTo(_stream);
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

        ICanEnumerateDisposable[] GetColumnReaders(IEnumerable<uint> columnIndices)
        {
            var columnCount = _header.ColumnCount;
            var ret = new ICanEnumerateDisposable[columnCount];
            for (uint i = 0; i < columnCount; i++)
                ret[i] = GetColumnReader(i, _header.RowCount);
            return ret;
        }
    }
}
