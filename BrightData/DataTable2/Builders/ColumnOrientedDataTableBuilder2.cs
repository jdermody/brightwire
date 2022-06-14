using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using BrightData.Buffer;
using BrightData.DataTable;
using BrightData.Helper;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace BrightData.DataTable2.Builders
{
    internal class ColumnOrientedDataTableBuilder2 : IDisposable
    {
        readonly BrightDataContext   _context;
        readonly uint                _inMemoryBufferSize;
        readonly ushort              _maxUniqueItemCount;
        readonly IProvideTempStreams _tempStreams;
        readonly List<IHybridBuffer> _columns = new();
        readonly MethodInfo          _writeStructs;

        public ColumnOrientedDataTableBuilder2(BrightDataContext context, uint inMemoryBufferSize = 32768 * 1024, ushort maxUniqueItemCount = 32768)
        {
            _context = context;
            _inMemoryBufferSize = inMemoryBufferSize;
            _maxUniqueItemCount = maxUniqueItemCount;
            _tempStreams = new TempStreamManager(context.Get<string>(Consts.BaseTempPath));

            var methods = GetType().GetMethods(BindingFlags.Instance);
            _writeStructs = methods.Single(m => m.Name == nameof(WriteStructs) && m.IsGenericMethod);
        }

        public MetaData TableMetaData { get; } = new();

        public void Dispose()
        {
            _tempStreams.Dispose();
        }

        public IHybridBuffer AddColumn(BrightDataType type, string? name = null)
        {
            var columnMetaData = new MetaData();
            columnMetaData.Set(Consts.Name, DataTableBase.DefaultColumnName(name, _columns.Count));
            columnMetaData.Set(Consts.Type, type);
            columnMetaData.Set(Consts.Index, (uint)_columns.Count);
            if(type.IsNumeric())
                columnMetaData.Set(Consts.IsNumeric, true);

            var buffer = columnMetaData.GetGrowableSegment(type, _context, _tempStreams, _inMemoryBufferSize, _maxUniqueItemCount);
            _columns.Add(buffer);
            return buffer;
        }

        public IHybridBuffer<T> AddColumn<T>(BrightDataType type, string? name = null)
            where T: notnull
        {
            return (IHybridBuffer<T>)AddColumn(type, name);
        }

        public void AddRow(params object[] items)
        {
            foreach (var (buffer, value) in _columns.Zip(items))
                buffer.Add(value, buffer.Size);
        }

        public void WriteTo(Stream stream)
        {
            // check that all columns have the same number of rows
            var firstColumn = _columns.First();
            foreach (var otherColumn in _columns.Skip(1)) {
                if (firstColumn.Size != otherColumn.Size)
                    throw new Exception("Columns have different sizes");
            }

            var stringTableWriter   = new Lazy<IHybridBuffer<string>>(()                 => _context.CreateHybridStringBuffer(_tempStreams, _inMemoryBufferSize, _maxUniqueItemCount));
            var tensorWriter        = new Lazy<IHybridBuffer<float>>(()                  => _context.CreateHybridStructBuffer<float>(_tempStreams, _inMemoryBufferSize, _maxUniqueItemCount));
            var byteWriter          = new Lazy<IHybridBuffer<byte>>(()                   => _context.CreateHybridStructBuffer<byte>(_tempStreams, _inMemoryBufferSize));
            var indexWriter         = new Lazy<IHybridBuffer<uint>>(()                   => _context.CreateHybridStructBuffer<uint>(_tempStreams, _inMemoryBufferSize, _maxUniqueItemCount));
            var weightedIndexWriter = new Lazy<IHybridBuffer<WeightedIndexList.Item>>(() => _context.CreateHybridStructBuffer<WeightedIndexList.Item>(_tempStreams, _inMemoryBufferSize, _maxUniqueItemCount));

            // write the header
            var headers = new ColumnOrientedDataTable2.Header[1];
            stream.Write(MemoryMarshal.AsBytes<ColumnOrientedDataTable2.Header>(headers));
            ref var header = ref headers[0];
            header.ColumnCount = (uint)_columns.Count;
            header.RowCount = firstColumn.Size;

            // write the meta data
            using var writer = new BinaryWriter(stream, Encoding.UTF8, true);
            TableMetaData.WriteTo(writer);

            // prepare the columns and continue writing meta data
            var columns = new ColumnOrientedDataTable2.Column[_columns.Count];
            var index = 0;
            foreach (var column in _columns) {
                ref var c = ref columns[index++];
                var tableSegment = (ISingleTypeTableSegment)column;
                c.DataType = tableSegment.SingleType;
                (_, c.DataTypeSize) = c.DataType.GetColumnType();
                tableSegment.MetaData.WriteTo(writer);
            }

            // write the headers
            writer.Flush();
            stream.Write(MemoryMarshal.AsBytes<ColumnOrientedDataTable2.Column>(columns));
            header.DataOffset = (uint)stream.Position;

            // write the data
            foreach (var column in _columns) {
                var tableSegment = (ISingleTypeTableSegment)column;
                var dataType = tableSegment.SingleType;
                var (columnType, _) = dataType.GetColumnType();

                if (dataType == BrightDataType.IndexList)
                    WriteIndexLists((IHybridBuffer<IndexList>)column, stream, indexWriter.Value);
                else if(dataType == BrightDataType.WeightedIndexList)
                    WriteWeightedIndexLists((IHybridBuffer<WeightedIndexList>)column, stream, weightedIndexWriter.Value);
                else if(dataType == BrightDataType.BinaryData)
                    WriteBinaryData((IHybridBuffer<BinaryData>)column, stream, byteWriter.Value);
                else if(dataType == BrightDataType.String)
                    WriteStringData((IHybridBuffer<string>)column, stream, stringTableWriter.Value);
                else
                    _writeStructs.MakeGenericMethod(columnType).Invoke(this, new object[] { column, stream });
            }
            header.DataSizeBytes = (uint)(stream.Position - header.DataOffset);

            // write the strings
            if (stringTableWriter.IsValueCreated) {
                var data = stringTableWriter.Value;
                header.StringOffset = (uint)stream.Position;
                header.StringCount = data.Size;
                data.CopyTo(stream);
            }

            // write the tensor data
            if (tensorWriter.IsValueCreated) {
                var data = tensorWriter.Value;
                header.TensorOffset = (uint)stream.Position;
                header.TensorCount = data.Size;
                data.CopyTo(stream);
            }

            // write the binary data
            if (byteWriter.IsValueCreated) {
                var data = byteWriter.Value;
                header.BinaryDataOffset = (uint)stream.Position;
                header.BinaryDataCount = data.Size;
                data.CopyTo(stream);
            }

            // write the index data
            if (indexWriter.IsValueCreated) {
                var data = indexWriter.Value;
                header.IndexOffset = (uint)stream.Position;
                header.IndexCount = data.Size;
                data.CopyTo(stream);
            }

            // write the weighted index data
            if (weightedIndexWriter.IsValueCreated) {
                var data = weightedIndexWriter.Value;
                header.WeightedIndexOffset = (uint)stream.Position;
                header.WeightedIndexCount = data.Size;
                data.CopyTo(stream);
            }

            // update the header
            stream.Seek(0, SeekOrigin.Begin);
            stream.Write(MemoryMarshal.AsBytes<ColumnOrientedDataTable2.Header>(headers));
            stream.Seek(0, SeekOrigin.End);
        }

        delegate void FillDelegate<CT, in T>(T item, Span<CT> ptr, int index) where T : notnull where CT : struct;
        void Write<T, CT>(IHybridBuffer<T> buffer, Stream stream, FillDelegate<CT, T> filler)
            where T: notnull
            where CT : struct
        {
            using var temp = SpanOwner<CT>.Allocate((int)Math.Min(buffer.Size, _inMemoryBufferSize));
            var ptr = temp.Span;
            var index = 0;
            foreach (var item in buffer.EnumerateTyped()) {
                filler(item, ptr, index++);
                if (index == temp.Length) {
                    stream.Write(MemoryMarshal.AsBytes(ptr));
                    index = 0;
                }
            }
            if(index > 0)
                stream.Write(MemoryMarshal.AsBytes(ptr[..index]));
        }
        void WriteStructs<T>(IHybridBuffer<T> buffer, Stream stream) where T: struct => 
            Write<T, T>(buffer, stream, (item, ptr, index) => ptr[index] = item);

        delegate Span<IT> GetSpans<T, IT>(T item) where IT: struct;
        void WriteDataRange<T, IT>(IHybridBuffer<T> buffer, Stream stream, IHybridBuffer<IT> indices, GetSpans<T, IT> getSpan)
            where T : notnull
            where IT : struct
        {
            Write<T, DataRangeColumnType>(buffer, stream, (item, ptr, index) => {
                ref var data = ref ptr[index];
                var span = getSpan(item);
                data.StartIndex = indices.Size;
                data.Count = (uint)span.Length;
                indices.Append(span);
            });
        }
        void WriteIndexLists(IHybridBuffer<IndexList> buffer, Stream stream, IHybridBuffer<uint> indices) => 
            WriteDataRange(buffer, stream, indices, indexList => indexList.Indices);
        void WriteWeightedIndexLists(IHybridBuffer<WeightedIndexList> buffer, Stream stream, IHybridBuffer<WeightedIndexList.Item> indices) => 
            WriteDataRange(buffer, stream, indices, indexList => indexList.Indices);
        void WriteBinaryData(IHybridBuffer<BinaryData> buffer, Stream stream, IHybridBuffer<byte> indices) => 
            WriteDataRange(buffer, stream, indices, indexList => indexList.Data);

        void WriteStringData(IHybridBuffer<string> buffer, Stream stream, IHybridBuffer<string> indices)
        {
            if (buffer.DistinctItems != null) {
                var existing = indices.DistinctItems;
                if (existing != null) {
                    foreach (var item in buffer.DistinctItems.Keys.Where(d => !existing.ContainsKey(d)))
                        indices.Add(item, indices.Size);
                }
            }

            Write<string, uint>(buffer, stream, (item, ptr, index) => {
                if (indices.DistinctItems is not null)
                    ptr[index] = indices.DistinctItems[item];
                else {
                    ptr[index] = indices.Size;
                    indices.Add(item, indices.Size);
                }
            });
        }
    }
}
