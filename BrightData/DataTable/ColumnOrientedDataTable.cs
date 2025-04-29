using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using BrightData.Buffer.Composite;
using BrightData.Buffer.ReadOnly;
using BrightData.Buffer.ReadOnly.Helper;
using BrightData.Converter;
using BrightData.DataTable.Columns;
using BrightData.DataTable.Meta;
using BrightData.DataTable.Rows;
using BrightData.Helper;
using BrightData.LinearAlgebra.ReadOnly;
using BrightData.Types;
using CommunityToolkit.HighPerformance;

namespace BrightData.DataTable
{
    /// <summary>
    /// Column oriented data table
    /// </summary>
    internal class ColumnOrientedDataTable : IDataTable, ITensorDataProvider
    {
        const int ReaderBlockSize = 128;
        internal struct Column
        {
            public BrightDataType DataType;
            public uint DataTypeSize;
            public readonly override string ToString() => $"{DataType} ({DataTypeSize})";
        }
        protected readonly IByteBlockReader                      _reader;
        protected readonly TableHeader                           _header;
        protected readonly Column[]                              _columns;
        protected readonly IReadOnlyBufferWithMetaData[]         _columnReader;
        protected readonly MetaData[]                            _columnMetaData;
        BlockMapper<DataRangeColumnType, ReadOnlyVector<float>>  _vectorMapper;
        BlockMapper<MatrixColumnType, ReadOnlyMatrix<float>>     _matrixMapper;
        BlockMapper<Tensor3DColumnType, ReadOnlyTensor3D<float>> _tensor3DMapper;
        BlockMapper<Tensor4DColumnType, ReadOnlyTensor4D<float>> _tensor4DMapper;
        List<string>?                                            _strings;
        ReadOnlyMemory<byte>?                                    _binaryData;
        ReadOnlyMemory<float>?                                   _tensors;
        ReadOnlyMemory<uint>?                                    _indices;
        ReadOnlyMemory<WeightedIndexList.Item>?                  _weightedIndices;

        ColumnOrientedDataTable(BrightDataContext context, TableHeader header, Column[] columns, ReadOnlyMemory<byte> metaData, IByteBlockReader reader)
        {
            Context = context;
            _reader = reader;
            _header = header;
            _columns = columns;
            if (_header.Orientation != DataTableOrientation.ColumnOriented)
                throw new ArgumentException("Expected to read a column oriented table");
            if (_header.ColumnCount == 0)
                throw new Exception("Expected data table to contain at least one column");
            if (_header.RowCount == 0)
                throw new Exception("Expected data table to contain at least one row");

            // default tensor creators
            _vectorMapper   = GetVectors;
            _matrixMapper   = GetMatrices;
            _tensor3DMapper = Get3DTensors;
            _tensor4DMapper = Get4DTensors;

            // get column types
            ColumnTypes = new BrightDataType[ColumnCount];
            for(var i = 0; i < ColumnCount; i++)
                ColumnTypes[i] = _columns[i].DataType;

            // read the meta data
            _columnMetaData = new MetaData[ColumnCount];
            var metadataReader = new BinaryReader(metaData.AsStream(), Encoding.UTF8, true);
            for (var i = 0; i < ColumnCount + 1; i++) {
                if (i == 0)
                    MetaData = new(metadataReader);
                else
                    _columnMetaData[i-1] = new(metadataReader);
            }
            MetaData ??= new();

            // create column readers
            _columnReader       = new IReadOnlyBufferWithMetaData[ColumnCount];
            CreateColumnReaders();
        }

        public static async Task<IDataTable> Load(BrightDataContext context, IByteBlockReader reader)
        {
            var headerSize = Unsafe.SizeOf<TableHeader>();
            var header = (await reader.GetBlock(0, (uint)headerSize)).Span.Cast<byte, TableHeader>()[0];
            var columnsBlock = reader.GetBlock(header.InfoOffset, header.InfoSizeBytes);
            var metaDataBlock = reader.GetBlock(header.MetaDataOffset, reader.Size - header.MetaDataOffset);
            await Task.WhenAll(columnsBlock, metaDataBlock);
            var columns = columnsBlock.Result.Span.Cast<byte, Column>().ToArray();
            return new ColumnOrientedDataTable(context, header, columns, metaDataBlock.Result, reader);
        }

        public MetaData MetaData { get; }

        void CreateColumnReaders()
        {
            var prevOffset = _header.DataOffset;
            for (uint i = 1; i < _columns.Length; i++) {
                var prevColumnType = ColumnTypes[i - 1];
                var (prevType, prevSize) = prevColumnType.GetColumnType();
                var nextOffset = prevOffset + prevSize * RowCount;
                CreateColumnReader(prevColumnType, prevType, i - 1, prevOffset, nextOffset - prevOffset);
                prevOffset = nextOffset;
            }
            var lastColumnType = ColumnTypes[_columns.Length - 1];
            var (lastColumnDataType, _) = lastColumnType.GetColumnType();
            CreateColumnReader(lastColumnType, lastColumnDataType, (uint)_columns.Length - 1, prevOffset, _header.DataOffset + _header.DataSizeBytes - prevOffset);
        }

        async Task<List<string>> ReadStrings(uint headerStringOffset, uint headerStringSizeBytes)
        {
            var ret = new List<string>();
            if (headerStringSizeBytes > 0) {
                var data = await _reader.GetBlock(headerStringOffset, headerStringSizeBytes);
                StringCompositeBuffer.Block.Decode(data.Span, str => ret.Add(new string(str)));
            }
            return ret;
        }

        async Task<ReadOnlyMemory<T>> GetBlock<T>(uint start, uint size) where T : unmanaged
        {
            var ret = await _reader.GetBlock(start, size);
            return ret.Cast<byte, T>();
        }

        delegate RT GetItem<T, out RT>(in T item);
        static RT[] Copy<T, RT>(ReadOnlyMemory<T> block, GetItem<T, RT> getItem)
        {
            var index = 0;
            var ret = new RT[block.Length];
            foreach (ref readonly var item in block.Span)
                ret[index++] = getItem(in item);
            return ret;
        }

        protected async Task<ReadOnlyMemory<string>> GetStrings(ReadOnlyMemory<uint> block)
        {
            var data = await GetStringData();
            return Copy(block, (in uint item) => data[(int)item]);
        }

        protected async Task<ReadOnlyMemory<BinaryData>> GetBinaryData(ReadOnlyMemory<DataRangeColumnType> block)
        {
            var data = await GetBinaryData();
            return Copy(block, (in DataRangeColumnType item) => new BinaryData(data.Slice((int)item.StartIndex, (int)item.Size)));
        }

        protected async Task<ReadOnlyMemory<ReadOnlyVector<float>>> GetVectors(ReadOnlyMemory<DataRangeColumnType> block)
        {
            var data = await GetTensorData();
            return Copy(block, (in DataRangeColumnType item) => new ReadOnlyVector<float>(data.Slice((int)item.StartIndex, (int)item.Size)));
        }

        protected async Task<ReadOnlyMemory<ReadOnlyMatrix<float>>> GetMatrices(ReadOnlyMemory<MatrixColumnType> block)
        {
            var data = await GetTensorData();
            return Copy(block, (in MatrixColumnType item) => new ReadOnlyMatrix<float>(data.Slice((int)item.StartIndex, (int)item.Size), item.RowCount, item.ColumnCount));
        }

        protected async Task<ReadOnlyMemory<ReadOnlyTensor3D<float>>> Get3DTensors(ReadOnlyMemory<Tensor3DColumnType> block)
        {
            var data = await GetTensorData();
            return Copy(block, (in Tensor3DColumnType item) => new ReadOnlyTensor3D<float>(data.Slice((int)item.StartIndex, (int)item.Size), item.Depth, item.RowCount, item.ColumnCount));
        }

        protected async Task<ReadOnlyMemory<ReadOnlyTensor4D<float>>> Get4DTensors(ReadOnlyMemory<Tensor4DColumnType> block)
        {
            var data = await GetTensorData();
            return Copy(block, (in Tensor4DColumnType item) => new ReadOnlyTensor4D<float>(data.Slice((int)item.StartIndex, (int)item.Size), item.Count, item.Depth, item.RowCount, item.ColumnCount));
        }

        protected async Task<ReadOnlyMemory<IndexList>> GetIndexLists(ReadOnlyMemory<DataRangeColumnType> block)
        {
            var data = await GetIndices();
            return Copy(block, (in DataRangeColumnType item) => new IndexList(data.Slice((int)item.StartIndex, (int)item.Size)));
        }

        protected async Task<ReadOnlyMemory<WeightedIndexList>> GetWeightedIndexLists(ReadOnlyMemory<DataRangeColumnType> block)
        {
            var data = await GetWeightedIndices();
            return Copy(block, (in DataRangeColumnType item) => new WeightedIndexList(data.Slice((int)item.StartIndex, (int)item.Size)));
        }

        public async ValueTask<ReadOnlyMemory<float>> GetTensorData() => _tensors ??= await GetBlock<float>(_header.TensorOffset, _header.TensorSizeBytes);
        public async ValueTask<ReadOnlyMemory<byte>> GetBinaryData() => _binaryData ??= await GetBlock<byte>(_header.BinaryDataOffset, _header.BinaryDataSizeBytes);
        public async ValueTask<ReadOnlyMemory<uint>> GetIndices() => _indices ??= await GetBlock<uint>(_header.IndexOffset, _header.IndexSizeBytes);
        public async ValueTask<ReadOnlyMemory<WeightedIndexList.Item>> GetWeightedIndices() => _weightedIndices ??= await GetBlock<WeightedIndexList.Item>(_header.WeightedIndexOffset, _header.WeightedIndexSizeBytes);
        public async ValueTask<List<string>> GetStringData() => _strings ??= await ReadStrings(_header.StringOffset, _header.StringSizeBytes);

        public uint RowCount => _header.RowCount;
        public uint ColumnCount => _header.ColumnCount;
        public DataTableOrientation Orientation => _header.Orientation;
        public BrightDataType[] ColumnTypes { get; }
        public MetaData[] ColumnMetaData => _columnMetaData;

        ~ColumnOrientedDataTable()
        {
            InternalDispose();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            InternalDispose();
        }

        void InternalDispose()
        {
            _reader.Dispose();
        }

        /// <summary>
        /// Saves current meta data into the underlying stream
        /// </summary>
        public void PersistMetaData()
        {
            using var tempBuffer = new MemoryStream();
            using var metaDataWriter = new BinaryWriter(tempBuffer, Encoding.UTF8, true);
            MetaData.WriteTo(metaDataWriter);
            foreach(var item in _columnMetaData)
                item.WriteTo(metaDataWriter);
            metaDataWriter.Flush();

            var memory = new Memory<byte>(tempBuffer.GetBuffer(), 0, (int)tempBuffer.Length);
            _reader.Update(_header.MetaDataOffset, memory);
        }

        public IReadOnlyBufferWithMetaData[] GetColumns(params uint[] columnIndices)
        {
            IReadOnlyBufferWithMetaData[] ret;
            if (columnIndices.Length == 0) {
                ret = new IReadOnlyBufferWithMetaData[ColumnCount];
                for (uint i = 0; i < ColumnCount; i++)
                    ret[i] = GetColumn(i);
            }
            else {
                var ind = 0;
                ret = new IReadOnlyBufferWithMetaData[columnIndices.Length];
                foreach(var index in columnIndices)
                    ret[ind++] = GetColumn(index);
            }

            return ret;
        }

        public void SetTensorMappers(
            BlockMapper<DataRangeColumnType, ReadOnlyVector<float>> vectorMapper,
            BlockMapper<MatrixColumnType, ReadOnlyMatrix<float>> matrixMapper,
            BlockMapper<Tensor3DColumnType, ReadOnlyTensor3D<float>> tensor3DMapper,
            BlockMapper<Tensor4DColumnType, ReadOnlyTensor4D<float>> tensor4DMapper
        )
        {
            _vectorMapper = vectorMapper;
            _matrixMapper = matrixMapper;
            _tensor3DMapper = tensor3DMapper;
            _tensor4DMapper = tensor4DMapper;
            CreateColumnReaders();
        }

        public BrightDataContext Context { get; }
        public Task<GenericTableRow[]> GetRows(params uint[] rowIndices) => ExtensionMethods.GetRows(this, rowIndices);
        public Task<GenericTableRow> this[uint index] => this.GetRow(index);
        public IAsyncEnumerable<GenericTableRow> EnumerateRows() => ExtensionMethods.EnumerateRows(this);

        public Task<T> Get<T>(uint columnIndex, uint rowIndex) where T: notnull
        {
            var column = _columnReader[columnIndex];
            if (column.DataType != typeof(T))
                throw new ArgumentException($"Column {columnIndex} is {column.DataType} but requested {typeof(T)}");
            var reader = (IReadOnlyBuffer<T>)column;
            return reader.GetItem(rowIndex);
        }

        public Task<T[]> Get<T>(uint columnIndex, params uint[] rowIndices) where T: notnull
        {
            var column = _columnReader[columnIndex];
            if (column.DataType != typeof(T))
                throw new ArgumentException($"Column {columnIndex} is {column.DataType} but requested {typeof(T)}");
            var reader = (IReadOnlyBuffer<T>)column;
            return reader.GetItems(rowIndices);
        }

        void CreateColumnReader(BrightDataType dataType, Type type, uint columnIndex, uint offset, uint size)
        {
            IReadOnlyBufferWithMetaData reader;
            if(type == typeof(bool))
                reader = new BlockReaderReadOnlyBuffer<bool>(_reader, _columnMetaData[columnIndex], offset, size, ReaderBlockSize);
            else if(type == typeof(sbyte))
                reader = new BlockReaderReadOnlyBuffer<sbyte>(_reader, _columnMetaData[columnIndex], offset, size, ReaderBlockSize);
            else if(type == typeof(short))
                reader = new BlockReaderReadOnlyBuffer<short>(_reader, _columnMetaData[columnIndex], offset, size, ReaderBlockSize);
            else if(type == typeof(int))
                reader = new BlockReaderReadOnlyBuffer<int>(_reader, _columnMetaData[columnIndex], offset, size, ReaderBlockSize);
            else if(type == typeof(long))
                reader = new BlockReaderReadOnlyBuffer<long>(_reader, _columnMetaData[columnIndex], offset, size, ReaderBlockSize);
            else if(type == typeof(float))
                reader = new BlockReaderReadOnlyBuffer<float>(_reader, _columnMetaData[columnIndex], offset, size, ReaderBlockSize);
            else if(type == typeof(double))
                reader = new BlockReaderReadOnlyBuffer<double>(_reader, _columnMetaData[columnIndex], offset, size, ReaderBlockSize);
            else if(type == typeof(decimal))
                reader = new BlockReaderReadOnlyBuffer<decimal>(_reader, _columnMetaData[columnIndex], offset, size, ReaderBlockSize);
            else if(type == typeof(uint))
                reader = new BlockReaderReadOnlyBuffer<uint>(_reader, _columnMetaData[columnIndex], offset, size, ReaderBlockSize);
            else if(type == typeof(DateTime))           
                reader = new BlockReaderReadOnlyBuffer<DateTime>(_reader, _columnMetaData[columnIndex], offset, size, ReaderBlockSize);                                                                                                     
            else if(type == typeof(TimeOnly))           
                reader = new BlockReaderReadOnlyBuffer<TimeOnly>(_reader, _columnMetaData[columnIndex], offset, size, ReaderBlockSize);                                                                                                     
            else if(type == typeof(DateOnly))           
                reader = new BlockReaderReadOnlyBuffer<DateOnly>(_reader, _columnMetaData[columnIndex], offset, size, ReaderBlockSize);                                                                                                     
            else if(type == typeof(DataRangeColumnType))           
                reader = new BlockReaderReadOnlyBuffer<DataRangeColumnType>(_reader, _columnMetaData[columnIndex], offset, size, ReaderBlockSize);
            else if(type == typeof(MatrixColumnType))           
                reader = new BlockReaderReadOnlyBuffer<MatrixColumnType>(_reader, _columnMetaData[columnIndex], offset, size, ReaderBlockSize);
            else if(type == typeof(Tensor3DColumnType))           
                reader = new BlockReaderReadOnlyBuffer<Tensor3DColumnType>(_reader, _columnMetaData[columnIndex], offset, size, ReaderBlockSize);
            else if(type == typeof(Tensor4DColumnType))           
                reader = new BlockReaderReadOnlyBuffer<Tensor4DColumnType>(_reader, _columnMetaData[columnIndex], offset, size, ReaderBlockSize);
            else
                throw new NotImplementedException($"{type} is not a supported column type");

            _columnReader[columnIndex] = dataType switch {
                BrightDataType.String            => new MappedReadOnlyBuffer<uint, string>((IReadOnlyBufferWithMetaData<uint>)reader, GetStrings),
                BrightDataType.BinaryData        => new MappedReadOnlyBuffer<DataRangeColumnType, BinaryData>((IReadOnlyBufferWithMetaData<DataRangeColumnType>)reader, GetBinaryData),
                BrightDataType.IndexList         => new MappedReadOnlyBuffer<DataRangeColumnType, IndexList>((IReadOnlyBufferWithMetaData<DataRangeColumnType>)reader, GetIndexLists),
                BrightDataType.WeightedIndexList => new MappedReadOnlyBuffer<DataRangeColumnType, WeightedIndexList>((IReadOnlyBufferWithMetaData<DataRangeColumnType>)reader, GetWeightedIndexLists),
                BrightDataType.Vector            => new MappedReadOnlyBuffer<DataRangeColumnType, ReadOnlyVector<float>>((IReadOnlyBufferWithMetaData<DataRangeColumnType>)reader, _vectorMapper),
                BrightDataType.Matrix            => new MappedReadOnlyBuffer<MatrixColumnType, ReadOnlyMatrix<float>>((IReadOnlyBufferWithMetaData<MatrixColumnType>)reader, _matrixMapper),
                BrightDataType.Tensor3D          => new MappedReadOnlyBuffer<Tensor3DColumnType, ReadOnlyTensor3D<float>>((IReadOnlyBufferWithMetaData<Tensor3DColumnType>)reader, _tensor3DMapper),
                BrightDataType.Tensor4D          => new MappedReadOnlyBuffer<Tensor4DColumnType, ReadOnlyTensor4D<float>>((IReadOnlyBufferWithMetaData<Tensor4DColumnType>)reader, _tensor4DMapper),
                _                                => reader
            };
        }

        public IReadOnlyBufferWithMetaData<T> GetColumn<T>(uint index) 
            where T: notnull
        {
            var typeofT = typeof(T);
            var reader = _columnReader[index];
            var dataType = reader.DataType;

            if(dataType == typeofT)
                return (IReadOnlyBufferWithMetaData<T>)reader;

            if (typeofT == typeof(object)) {
                var ret = new ReadOnlyBufferMetaDataWrapper<object>(reader.ToObjectBuffer(), _columnMetaData[index]);
                return (IReadOnlyBufferWithMetaData<T>)ret;
            }

            if (typeofT == typeof(string)) {
                var ret = new ReadOnlyBufferMetaDataWrapper<string>(reader.ToStringBuffer(), _columnMetaData[index]);
                return (IReadOnlyBufferWithMetaData<T>)ret;
            }

            if (dataType.GetBrightDataType().IsNumeric() && typeofT.GetBrightDataType().IsNumeric()) {
                var converter = StaticConverters.GetConverter(dataType, typeof(T));
                return new ReadOnlyBufferMetaDataWrapper<T>((IReadOnlyBuffer<T>)GenericTypeMapping.TypeConverter(typeof(T), reader, converter), _columnMetaData[index]);
            }

            throw new NotImplementedException($"Not able to create a column of type {typeof(T)} from {dataType}");
        }

        public IReadOnlyBufferWithMetaData GetColumn(uint index) => _columnReader[index];

        public IDataDimension[] Dimensions
        {
            get
            {
                var len = _columns.Length;
                var ret = new IDataDimension[len];
                for (var i = 0; i < len; i++)
                    ret[i] = new TableDataDimension(_columns[i].DataType, _columnReader[i]);
                return ret;
            }
        }
    }
}
