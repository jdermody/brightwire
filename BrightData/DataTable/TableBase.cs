using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BrightData.Buffer.Composite;
using BrightData.Buffer.ReadOnly.Converter;
using BrightData.Helper;
using BrightData.LinearAlgebra.ReadOnly;
using BrightData.Operation;
using CommunityToolkit.HighPerformance;

namespace BrightData.DataTable
{
    internal abstract partial class TableBase : IDataTable
    {
        internal struct Column
        {
            public BrightDataType DataType;
            public uint DataTypeSize;
            public override string ToString() => $"{DataType} ({DataTypeSize})";
        }
        protected readonly IByteBlockReader                         _reader;
        protected readonly TableHeader                              _header;
        protected readonly Column[]                                 _columns;
        protected Lazy<Task<List<string>>>                          _strings;
        readonly Lazy<Task<ReadOnlyMemory<float>>>                  _tensors;
        readonly Lazy<Task<ReadOnlyMemory<byte>>>                   _data;
        readonly Lazy<Task<ReadOnlyMemory<uint>>>                   _indices;
        readonly Lazy<Task<ReadOnlyMemory<WeightedIndexList.Item>>> _weightedIndices;
        protected readonly MetaData[]                               _columnMetaData;
        ITensorDataProvider                                         _tensorDataProvider;

        protected unsafe TableBase(BrightDataContext context, IByteBlockReader reader, DataTableOrientation expectedOrientation)
        {
            Context = context;
            _reader = reader;
            _header = _reader.GetBlock(0, (uint)sizeof(TableHeader)).Result.Span.Cast<byte, TableHeader>()[0];
            if (_header.Orientation != expectedOrientation)
                throw new ArgumentException($"Expected to read a {expectedOrientation} table");
            if (_header.ColumnCount == 0)
                throw new Exception("Expected data table to contain at least one column");
            if (_header.RowCount == 0)
                throw new Exception("Expected data table to contain at least one row");

            // read the columns
            _columns = _reader
                .GetBlock(_header.InfoOffset, _header.InfoSizeBytes).Result.Span.Cast<byte, Column>()
                .ToArray()
            ;

            // get column types
            ColumnTypes = new BrightDataType[ColumnCount];
            for(var i = 0; i < ColumnCount; i++)
                ColumnTypes[i] = _columns[i].DataType;

            // read the meta data
            _columnMetaData = new MetaData[ColumnCount];
            var data = _reader.GetBlock(_header.MetaDataOffset, _header.MetaDataSizeBytes).Result;
            var metadataReader = new BinaryReader(data.AsStream(), Encoding.UTF8, true);
            for (var i = 0; i < ColumnCount + 1; i++) {
                if (i == 0)
                    MetaData = new(metadataReader);
                else
                    _columnMetaData[i-1] = new(metadataReader);
            }
            MetaData ??= new();

            // create data readers
            _strings         = new(() => ReadStrings(_header.StringOffset, _header.StringSizeBytes));
            _tensors         = new(() => GetBlock<float>(_header.TensorOffset, _header.TensorSizeBytes));
            _data            = new(() => GetBlock<byte>(_header.DataOffset, _header.DataSizeBytes));
            _indices         = new(() => GetBlock<uint>(_header.IndexOffset, _header.IndexSizeBytes));
            _weightedIndices = new(() => GetBlock<WeightedIndexList.Item>(_header.WeightedIndexOffset, _header.WeightedIndexSizeBytes));
            _tensorDataProvider = this;
        }

        public MetaData MetaData { get; }

        async Task<List<string>> ReadStrings(uint headerStringOffset, uint headerStringSizeBytes)
        {
            var ret = new List<string>();
            if (headerStringSizeBytes > 0) {
                var data = await _reader.GetBlock(headerStringOffset, headerStringSizeBytes);
                StringCompositeBuffer.Decode(data.Span, str => ret.Add(new string(str)));
            }
            return ret;
        }

        async Task<ReadOnlyMemory<T>> GetBlock<T>(uint start, uint size) where T : unmanaged
        {
            var ret = await _reader.GetBlock(start, size);
            return ret.Cast<byte, T>();
        }

        protected ReadOnlyMemory<string> GetStrings(ReadOnlySpan<uint> indices)
        {
            var index = 0;
            var stringTable = _strings.Value.Result;
            var ret = new string[indices.Length];
            foreach (var item in indices)
                ret[index++] = stringTable[(int)item];
            return ret;
        }
        protected string GetString(uint index) => _strings.Value.Result[(int)index];

        protected ReadOnlyMemory<BinaryData> GetBinaryData(ReadOnlySpan<DataRangeColumnType> span)
        {
            var index = 0;
            var ret = new BinaryData[span.Length];
            foreach (ref readonly var item in span)
                ret[index++] = GetBinaryData(item);
            return ret;
        }
        protected BinaryData GetBinaryData(in DataRangeColumnType dataRange)
        {
            var data = _data.Value.Result.Span.Slice((int)dataRange.StartIndex, (int)dataRange.Size);
            return new BinaryData(data);
        }

        protected ReadOnlyMemory<ReadOnlyVector> GetVectors(ReadOnlySpan<DataRangeColumnType> span)
        {
            var index = 0;
            var ret = new ReadOnlyVector[span.Length];
            var data = _tensorDataProvider.GetTensorData();
            foreach (ref readonly var item in span)
                ret[index++] = new ReadOnlyVector(data.Slice((int)item.StartIndex, (int)item.Size));
            return ret;
        }

        protected ReadOnlyMemory<ReadOnlyMatrix> GetMatrices(ReadOnlySpan<MatrixColumnType> span)
        {
            var index = 0;
            var ret = new ReadOnlyMatrix[span.Length];
            var data = _tensorDataProvider.GetTensorData();
            foreach (ref readonly var item in span)
                ret[index++] = new ReadOnlyMatrix(data.Slice((int)item.StartIndex, (int)item.Size), item.RowCount, item.ColumnCount);
            return ret;
        }

        protected ReadOnlyMemory<ReadOnlyTensor3D> GetTensors(ReadOnlySpan<Tensor3DColumnType> span)
        {
            var index = 0;
            var ret = new ReadOnlyTensor3D[span.Length];
            var data = _tensorDataProvider.GetTensorData();
            foreach (ref readonly var item in span)
                ret[index++] = new ReadOnlyTensor3D(data.Slice((int)item.StartIndex, (int)item.Size), item.Depth, item.RowCount, item.ColumnCount);
            return ret;
        }

        protected ReadOnlyMemory<ReadOnlyTensor4D> GetTensors(ReadOnlySpan<Tensor4DColumnType> span)
        {
            var index = 0;
            var ret = new ReadOnlyTensor4D[span.Length];
            var data = _tensorDataProvider.GetTensorData();
            foreach (ref readonly var item in span)
                ret[index++] = new ReadOnlyTensor4D(data.Slice((int)item.StartIndex, (int)item.Size), item.Count, item.Depth, item.RowCount, item.ColumnCount);
            return ret;
        }

        protected ReadOnlyMemory<IndexList> GetIndexLists(ReadOnlySpan<DataRangeColumnType> span)
        {
            var index = 0;
            var ret = new IndexList[span.Length];
            var data = _indices.Value.Result;
            foreach (ref readonly var item in span)
                ret[index++] = new IndexList(data.Slice((int)item.StartIndex, (int)item.Size));
            return ret;
        }
        protected IndexList GetIndexList(in DataRangeColumnType dataRange)
        {
            var data = _indices.Value.Result.Slice((int)dataRange.StartIndex, (int)dataRange.Size);
            return new IndexList(data);
        }

        protected ReadOnlyMemory<WeightedIndexList> GetWeightedIndexLists(ReadOnlySpan<DataRangeColumnType> span)
        {
            var index = 0;
            var ret = new WeightedIndexList[span.Length];
            var data = _weightedIndices.Value.Result;
            foreach (ref readonly var item in span)
                ret[index++] = new WeightedIndexList(data.Slice((int)item.StartIndex, (int)item.Size));
            return ret;
        }
        protected WeightedIndexList GetWeightedIndexList(in DataRangeColumnType dataRange)
        {
            var data = _weightedIndices.Value.Result.Slice((int)dataRange.StartIndex, (int)dataRange.Size);
            return new WeightedIndexList(data);
        }

        public uint RowCount => _header.RowCount;
        public uint ColumnCount => _header.ColumnCount;
        public DataTableOrientation Orientation => _header.Orientation;
        public abstract IReadOnlyBufferWithMetaData<T> GetColumn<T>(uint index) where T : notnull;
        public abstract IReadOnlyBufferWithMetaData GetColumn(uint index);
        public abstract Task<T> Get<T>(uint columnIndex, uint rowIndex) where T : notnull;
        public abstract Task<T[]> Get<T>(uint columnIndex, params uint[] rowIndices) where T : notnull;

        public BrightDataType[] ColumnTypes { get; }
        public MetaData[] ColumnMetaData => _columnMetaData;

        ~TableBase()
        {
            InternalDispose();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            
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

        public async Task<MetaData[]> GetColumnAnalysis(params uint[] columnIndices)
        {
            if (!this.AllOrSpecifiedColumnIndices(true, columnIndices).All(i => _columnMetaData[i].Get(Consts.HasBeenAnalysed, false))) {
                var operations = this.AllOrSpecifiedColumnIndices(true, columnIndices).Select(i => _columnMetaData[i].Analyse(false, GetColumn(i))).ToArray();
                await operations.Process();
            }
            return this.AllOrSpecifiedColumnIndices(false, columnIndices).Select(x => _columnMetaData[x]).ToArray();
        }

        public void SetTensorData(ITensorDataProvider dataProvider) => _tensorDataProvider = dataProvider;

        public async IAsyncEnumerable<TableRow> EnumerateRows([EnumeratorCancellation] CancellationToken ct = default)
        {
            var size = _header.ColumnCount;
            var enumerators = new IAsyncEnumerator<object>[size];
            var currentTasks = new ValueTask<bool>[size];
            var isValid = true;

            for (uint i = 0; i < size; i++)
                enumerators[i] = GetColumn(i).EnumerateAll().GetAsyncEnumerator(ct);

            uint rowIndex = 0;
            while (!ct.IsCancellationRequested && isValid) {
                for (var i = 0; i < size; i++)
                    currentTasks[i] = enumerators[i].MoveNextAsync();
                for (var i = 0; i < size; i++) {
                    if (await currentTasks[i] != true) {
                        isValid = false;
                        break;
                    }
                }

                var curr = new object[size];
                for (var i = 0; i < size; i++)
                    curr[i] = enumerators[i].Current;
                yield return new TableRow(rowIndex++, curr);
            }
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

        public IReadOnlyBufferWithMetaData[] GetColumns(IEnumerable<uint> columnIndices)
        {
            return columnIndices.Select(GetColumn).ToArray();
        }

        public async Task WriteColumnsTo(Stream stream, params uint[] columnIndices)
        {
            var writer = new ColumnOrientedDataTableWriter(Context);
            await writer.Write(MetaData, GetColumns(columnIndices), stream);
        }

        public ReadOnlyMemory<float> GetTensorData() => _tensors.Value.Result;
        public BrightDataContext Context { get; }

        public async Task WriteRowsTo(Stream stream, params uint[] rowIndices)
        {
            var writer = new ColumnOrientedDataTableBuilder(Context);
            var newColumns = writer.AddColumnsFrom(this);
            var wantedRowIndices = rowIndices.Length > 0 ? rowIndices : RowCount.AsRange().ToArray();
            var operations = newColumns
                .Select((x, i) => GenericActivator.Create<IOperation>(typeof(IndexedCopyOperation<>).MakeGenericType(x.DataType), GetColumn((uint)i), x, wantedRowIndices))
                .ToArray();
            await operations.Process();
            await writer.WriteTo(stream);
        }

        IReadOnlyBufferWithMetaData<object>[] GetColumnsAsObjectBuffers()
        {
            var index = 0;
            var ret = new IReadOnlyBufferWithMetaData<object>[ColumnCount];
            for (uint i = 0; i < ColumnCount; i++) {
                var column = GetColumn(i);
                ret[index++] = GenericActivator.Create<IReadOnlyBufferWithMetaData<object>>(typeof(ToObjectConverter<>).MakeGenericType(column.DataType), column);
            }
            return ret;
        }

        public async Task<TableRow[]> GetRows(params uint[] rowIndices)
        {
            var columns = GetColumnsAsObjectBuffers();
            var len = columns.Length;
            var blockSize = columns[0].BlockSize;
            Debug.Assert(columns.Skip(1).All(x => x.BlockSize == blockSize));

            var blocks = rowIndices.Select(x => (Index: x, BlockIndex: x / blockSize, RelativeIndex: x % blockSize))
                .GroupBy(x => x.BlockIndex)
                .OrderBy(x => x.Key)
            ;
            var ret = rowIndices.Select(x => new TableRow(x, new object[len])).ToArray();
            var tasks = new Task<ReadOnlyMemory<object>>[len];
            foreach (var block in blocks) {
                for(var i = 0; i < len; i++)
                    tasks[i] = columns[i].GetTypedBlock(block.Key);
                await Task.WhenAll(tasks);
                foreach (var (index, _, relativeIndex) in block) {
                    var row = ret[index].Values;
                    for (var i = 0; i < len; i++)
                        row[i] = tasks[i].Result.Span[(int)relativeIndex];
                }
            }
            return ret;
        }
    }
}
