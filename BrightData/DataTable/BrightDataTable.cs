using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using BrightData.Buffer.ReadOnly;
using BrightData.DataTable.Operations;
using BrightData.Helper;

namespace BrightData.DataTable
{
    public partial class BrightDataTable : IDisposable
    {
        internal struct Header
        {
            public byte Version;
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

            public uint MetaDataOffset;
        }
        internal struct Column
        {
            public BrightDataType DataType;
            public uint DataTypeSize;
        }

        readonly IReadOnlyBuffer                                               _buffer;
        readonly uint                                                          _bufferSize;
        readonly Header                                                        _header;
        readonly Column[]                                                      _columns;
        readonly Lazy<MetaData[]>                                              _metaData;
        readonly Lazy<MetaData[]>                                              _columnMetaData;
        readonly Lazy<string[]>                                                _stringTable;
        readonly Lazy<ICanRandomlyAccessUnmanagedData<float>>                  _tensors;
        readonly Lazy<ICanRandomlyAccessUnmanagedData<byte>>                   _binaryData;
        readonly Lazy<ICanRandomlyAccessUnmanagedData<uint>>                   _indices;
        readonly Lazy<ICanRandomlyAccessUnmanagedData<WeightedIndexList.Item>> _weightedIndices;
        readonly uint                                                          _rowSize;
        readonly long                                                          _dataOffset;
        readonly MethodInfo                                                    _getReader, _getConverter, _getRandomAccessColumnReader;
        readonly uint[]                                                        _columnOffset;
        readonly BrightDataType[]                                              _columnTypes;
        readonly Stream                                                        _stream;
        readonly Lazy<object[]>                                                _columnConverters;
        readonly Lazy<ICanRandomlyAccessData[]>                                _columnReaders;

        public BrightDataTable(BrightDataContext context, Stream stream, uint bufferSize = 32768)
        {
            _stream = stream;
            if (stream is FileStream fileStream)
                _buffer = new ReadOnlyFileBasedBuffer(fileStream);
            else if (stream is MemoryStream memoryStream)
                _buffer = new ReadOnlyMemoryBasedBuffer(new ReadOnlyMemory<byte>(memoryStream.GetBuffer()));
            else
                throw new ArgumentException("Expected file or memory stream", nameof(stream));

            Context = context;
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
                    var ret = Context.GetBufferReader<string>(reader2, _bufferSize).Values.ToArray();
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
            unsafe ICanRandomlyAccessUnmanagedData<T> GetBlockAccessor<T>(uint offset, uint count) where T: unmanaged => offset == 0 
                ? new EmptyBlock<T>() 
                : _buffer.GetBlock<T>(offset, count * sizeof(T));
            _stringTable     = new(() => ReadStringArray(_header.StringOffset, _header.StringCount));
            _binaryData      = new(() => GetBlockAccessor<byte>(_header.BinaryDataOffset, _header.BinaryDataCount));
            _tensors         = new(() => GetBlockAccessor<float>(_header.TensorOffset, _header.TensorCount));
            _indices         = new(() => GetBlockAccessor<uint>(_header.IndexOffset, _header.IndexCount));
            _weightedIndices = new(() => GetBlockAccessor<WeightedIndexList.Item>(_header.WeightedIndexOffset, _header.WeightedIndexCount));
            _metaData        = new(() => ReadMetaData(_header.MetaDataOffset));
            _columnMetaData  = new(() => _metaData.Value.Skip(1).ToArray());

            // resolve generic methods
            var genericMethods           = GetType().GetGenericMethods();
            _getReader                   = genericMethods[nameof(GetReader)];
            _getConverter                = genericMethods[nameof(GetConverter)];
            _getRandomAccessColumnReader = genericMethods[nameof(GetRandomAccessColumnReader)];

            // determine column offsets
            _columnOffset = new uint[numColumns];
            _columnOffset[0] = _header.DataOffset;
            for (uint i = 1; i < _columns.Length; i++) {
                ref readonly var previousColumn = ref _columns[i-1];
                _columnOffset[i] = _columnOffset[i-1] + previousColumn.DataTypeSize * numRows;
            }

            // get column types
            ColumnTypes = _columns.Select(c => c.DataType).ToArray();

            // create column converters (lazy)
            _columnConverters = new(() => {
                var ret = new object[_columns.Length];
                for (uint ci = 0, len = (uint)_columns.Length; ci < len; ci++) {
                    ref readonly var column = ref _columns[ci];
                    var dataType = column.DataType.GetDataType();
                    var (columnDataType, _) = column.DataType.GetColumnType();
                    ret[ci] = _getConverter.MakeGenericMethod(columnDataType, dataType).Invoke(this, null)!;
                }
                return ret;
            });

            // create column readers (lazy)
            _columnReaders = new(() => {
                var ret = new ICanRandomlyAccessData[_columns.Length];
                var converters = _columnConverters.Value;
                for (uint ci = 0, len = (uint)_columns.Length; ci < len; ci++) {
                    ref readonly var column = ref _columns[ci];
                    var dataType = column.DataType.GetDataType();
                    var offset = _columnOffset[ci];
                    var (columnDataType, _) = column.DataType.GetColumnType();
                    var sizeInBytes = _header.RowCount * column.DataTypeSize;
                    ret[ci] = (ICanRandomlyAccessData)_getRandomAccessColumnReader.MakeGenericMethod(columnDataType, dataType).Invoke(this, new [] {
                        offset, sizeInBytes, converters[ci]
                    })!;
                }
                return ret;
            });
        }

        /// <inheritdoc />
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            // _stream will be disposed by the buffer
            _buffer.Dispose();
        }

        public BrightDataContext Context { get; }
        public MetaData TableMetaData => _metaData.Value[0];
        public uint ColumnCount => _header.ColumnCount;
        public uint RowCount => _header.RowCount;
        public DataTableOrientation Orientation => _header.Orientation;
        public ICanRandomlyAccessData[] DefaultColumnReaders => _columnReaders.Value;
        public string[] StringTable => _stringTable.Value;
        public ICanRandomlyAccessUnmanagedData<byte> BinaryDataBlock => _binaryData.Value;
        public ICanRandomlyAccessUnmanagedData<float> TensorDataBlock => _tensors.Value;
        public ICanRandomlyAccessUnmanagedData<uint> IndexDataBlock => _indices.Value;
        public ICanRandomlyAccessUnmanagedData<WeightedIndexList.Item> WeightedIndexBlock => _weightedIndices.Value;

        public ICanRandomlyAccessUnmanagedData<CT> GetRawColumnData<CT>(uint columnIndex) where CT : unmanaged
        {
            ref readonly var column = ref _columns[columnIndex];
            var offset = _columnOffset[columnIndex];
            var sizeInBytes = _header.RowCount * column.DataTypeSize;
            return _buffer.GetBlock<CT>(offset, sizeInBytes);
        }

        public MetaData GetColumnAnalysis(uint columnIndex, bool force = false, uint writeCount = Consts.MaxWriteCount, uint maxDistinctCount = Consts.MaxDistinct)
        {
            var ret = GetColumnMetaData(columnIndex);
            if (force || !ret.Get(Consts.HasBeenAnalysed, false)) {
                using var operation = CreateColumnAnalyser(columnIndex, writeCount, maxDistinctCount);
                operation.Complete(null, CancellationToken.None);
            }

            return ret;
        }

        public IEnumerable<(uint ColumnIndex, MetaData MetaData)> GetColumnAnalysis(IEnumerable<uint> columnIndices, uint writeCount = Consts.MaxWriteCount, uint maxDistinctCount = Consts.MaxDistinct)
        {
            var operations = new List<IOperation<(uint ColumnIndex, MetaData MetaData)>>();
            foreach (var ci in columnIndices) {
                var metaData = GetColumnMetaData(ci);
                operations.Add(!metaData.Get(Consts.HasBeenAnalysed, false) 
                    ? CreateColumnAnalyser(ci, writeCount, maxDistinctCount) 
                    : new NopMetaDataOperation(ci, metaData)
                );
            }

            var results = operations.CompleteInParallel();
            return results.EnsureAllCompleted();
        }
        public MetaData[] AllColumnAnalysis(uint writeCount = Consts.MaxWriteCount, uint maxDistinctCount = Consts.MaxDistinct) => GetColumnAnalysis(ColumnCount.AsRange(), writeCount, maxDistinctCount).Select(d => d.MetaData).ToArray();

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

        public IEnumerable<IOperation<bool>> CopyToColumnConsumers(IEnumerable<IConsumeColumnData> consumers, uint maxRows = uint.MaxValue)
        {
            foreach (var consumer in consumers) {
                var columnReader = GetColumnReader(consumer.ColumnIndex, maxRows);
                var type = typeof(CopyToConsumerOperation<>).MakeGenericType(consumer.ColumnType.GetDataType());
                yield return GenericActivator.Create<IOperation<bool>>(type, RowCount, columnReader, consumer);
            }
        }

        /// <summary>
        /// Returns the first row as a string
        /// </summary>
        public string FirstRow => RowCount > 0 ? GetRow(0).ToString() : "No data";

        /// <summary>
        /// Returns the second row as a string
        /// </summary>
        public string SecondRow => RowCount > 1 ? GetRow(1).ToString() : "No data";

        /// <summary>
        /// Returns the third row as a string
        /// </summary>
        public string ThirdRow => RowCount > 2 ? GetRow(2).ToString() : "No data";

        /// <summary>
        /// Returns the last row as a string
        /// </summary>
        public string LastRow => RowCount > 0 ? GetRow(RowCount-1).ToString() : "No data";

        /// <inheritdoc />
        public override string ToString() => string.Join(", ", _columns.Select((c, i) => $"[{ColumnTypes[i]}]: {GetColumnMetaData((uint)i)}"));
    }
}
