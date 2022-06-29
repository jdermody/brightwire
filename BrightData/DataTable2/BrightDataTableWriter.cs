using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace BrightData.DataTable2
{
    internal class BrightDataTableWriter
    {
        readonly BrightDataContext   _context;
        readonly IProvideTempStreams _tempStreams;
        readonly Stream              _stream;
        readonly uint                _inMemoryBufferSize;
        readonly ushort              _maxUniqueItemCount;
        readonly MethodInfo          _writeStructs;

        public BrightDataTableWriter(
            BrightDataContext context,
            IProvideTempStreams tempStreams,
            Stream stream, 
            uint inMemoryBufferSize = 32768 * 1024, 
            ushort maxUniqueItemCount = 32768
        ) {
            _context            = context;
            _tempStreams        = tempStreams;
            _stream             = stream;
            _inMemoryBufferSize = inMemoryBufferSize;
            _maxUniqueItemCount = maxUniqueItemCount;

            var methods = GetType().GetGenericMethods();
            _writeStructs = methods[nameof(WriteStructs)];
        }

        public void Write(
            MetaData tableMetaData, 
            ISingleTypeTableSegment[] columnSegments
        ) {
            // check that all columns have the same number of rows
            var firstColumn = columnSegments.First();
            foreach (var otherColumn in columnSegments.Skip(1)) {
                if (firstColumn.Size != otherColumn.Size)
                    throw new Exception("Columns have different sizes");
            }

            var stringTableWriter   = new Lazy<IHybridBuffer<string>>(()                 => _context.CreateHybridStringBuffer(_tempStreams, _inMemoryBufferSize, _maxUniqueItemCount));
            var tensorWriter        = new Lazy<IHybridBuffer<float>>(()                  => _context.CreateHybridStructBuffer<float>(_tempStreams, _inMemoryBufferSize, _maxUniqueItemCount));
            var byteWriter          = new Lazy<IHybridBuffer<byte>>(()                   => _context.CreateHybridStructBuffer<byte>(_tempStreams, _inMemoryBufferSize));
            var indexWriter         = new Lazy<IHybridBuffer<uint>>(()                   => _context.CreateHybridStructBuffer<uint>(_tempStreams, _inMemoryBufferSize, _maxUniqueItemCount));
            var weightedIndexWriter = new Lazy<IHybridBuffer<WeightedIndexList.Item>>(() => _context.CreateHybridStructBuffer<WeightedIndexList.Item>(_tempStreams, _inMemoryBufferSize, _maxUniqueItemCount));

            // write the header
            var headers = new BrightDataTable.Header[1];
            _stream.Write(MemoryMarshal.AsBytes<BrightDataTable.Header>(headers));
            ref var header = ref headers[0];
            header.Version = 1;
            header.Orientation = DataTableOrientation.ColumnOriented;
            header.ColumnCount = (uint)columnSegments.Length;
            header.RowCount = firstColumn.Size;

            // write the meta data to a temp stream
            using var tempBuffer = new MemoryStream();
            using var metaDataWriter = new BinaryWriter(tempBuffer, Encoding.UTF8, true);
            tableMetaData.WriteTo(metaDataWriter);

            // prepare the columns and continue writing meta data
            var columns = new BrightDataTable.Column[columnSegments.Length];
            var index = 0;
            foreach (var columnSegment in columnSegments) {
                ref var c = ref columns[index++];
                c.DataType = columnSegment.SingleType;
                (_, c.DataTypeSize) = c.DataType.GetColumnType();
                columnSegment.MetaData.WriteTo(metaDataWriter);
            }

            // write the headers
            _stream.Write(MemoryMarshal.AsBytes<BrightDataTable.Column>(columns));
            header.DataOffset = (uint)_stream.Position;

            // write the data (column oriented)
            foreach (var columnSegment in columnSegments) {
                var dataType = columnSegment.SingleType;
                var (columnType, _) = dataType.GetColumnType();

                if (dataType == BrightDataType.IndexList)
                    WriteIndexLists((ICanEnumerateWithSize<IndexList>)columnSegment, indexWriter.Value);
                else if (dataType == BrightDataType.WeightedIndexList)
                    WriteWeightedIndexLists((ICanEnumerateWithSize<WeightedIndexList>)columnSegment, weightedIndexWriter.Value);
                else if (dataType == BrightDataType.BinaryData)
                    WriteBinaryData((ICanEnumerateWithSize<BinaryData>)columnSegment, byteWriter.Value);
                else if (dataType == BrightDataType.String)
                    WriteStringData((ICanEnumerateWithSize<string>)columnSegment, stringTableWriter.Value);
                else if (dataType == BrightDataType.Vector)
                    WriteVectors((ICanEnumerateWithSize<IVector>)columnSegment, tensorWriter.Value);
                else if (dataType == BrightDataType.Matrix)
                    WriteMatrices((ICanEnumerateWithSize<IMatrix>)columnSegment, tensorWriter.Value);
                else if (dataType == BrightDataType.Tensor3D)
                    WriteTensors((ICanEnumerateWithSize<ITensor3D>)columnSegment, tensorWriter.Value);
                else if (dataType == BrightDataType.Tensor4D)
                    WriteTensors((ICanEnumerateWithSize<ITensor4D>)columnSegment, tensorWriter.Value);
                else
                    _writeStructs.MakeGenericMethod(columnType).Invoke(this, new object[] { columnSegment });
            }
            header.DataSizeBytes = (uint)(_stream.Position - header.DataOffset);

            // write the strings
            if (stringTableWriter.IsValueCreated) {
                var data = stringTableWriter.Value;
                header.StringOffset = (uint)_stream.Position;
                header.StringCount = data.Size;
                data.CopyTo(_stream);
            }

            // write the tensor data
            if (tensorWriter.IsValueCreated) {
                var data = tensorWriter.Value;
                header.TensorOffset = (uint)_stream.Position;
                header.TensorCount = data.Size;
                data.EnumerateTyped().WriteTo(_stream);
            }

            // write the binary data
            if (byteWriter.IsValueCreated) {
                var data = byteWriter.Value;
                header.BinaryDataOffset = (uint)_stream.Position;
                header.BinaryDataCount = data.Size;
                data.EnumerateTyped().WriteTo(_stream);
            }

            // write the index data
            if (indexWriter.IsValueCreated) {
                var data = indexWriter.Value;
                header.IndexOffset = (uint)_stream.Position;
                header.IndexCount = data.Size;
                data.EnumerateTyped().WriteTo(_stream);
            }

            // write the weighted index data
            if (weightedIndexWriter.IsValueCreated) {
                var data = weightedIndexWriter.Value;
                header.WeightedIndexOffset = (uint)_stream.Position;
                header.WeightedIndexCount = data.Size;
                data.EnumerateTyped().WriteTo(_stream);
            }

            // write the meta data
            header.MetaDataOffset = (uint)_stream.Position;
            metaDataWriter.Flush();
            tempBuffer.WriteTo(_stream);

            // update the header
            _stream.Seek(0, SeekOrigin.Begin);
            _stream.Write(MemoryMarshal.AsBytes<BrightDataTable.Header>(headers));
            _stream.Seek(0, SeekOrigin.End);
        }

        void Write<T, CT>(ICanEnumerateWithSize<T> buffer, FillDelegate<CT, T> filler)
            where T : notnull
            where CT : struct
        {
            using var temp = SpanOwner<CT>.Allocate((int)Math.Min(buffer.Size, _inMemoryBufferSize));
            var ptr = temp.Span;
            var index = 0;
            foreach (var item in buffer.EnumerateTyped()) {
                filler(item, ptr, index++);
                if (index == temp.Length) {
                    _stream.Write(MemoryMarshal.AsBytes(ptr));
                    index = 0;
                }
            }
            if (index > 0)
                _stream.Write(MemoryMarshal.AsBytes(ptr[..index]));
        }
        void WriteStructs<T>(ICanEnumerateWithSize<T> buffer) where T : struct =>
            Write<T, T>(buffer, (item, ptr, index) => ptr[index] = item);
        delegate IT[] GetArray<in T, out IT>(T item) where IT : struct;
        void WriteDataRange<T, IT>(ICanEnumerateWithSize<T> buffer, IHybridBuffer<IT> indices, GetArray<T, IT> getArray)
            where T : notnull
            where IT : struct
        {
            Write<T, DataRangeColumnType>(buffer, (item, ptr, index) => {
                ref var data = ref ptr[index];
                var array = getArray(item);
                data.StartIndex = indices.Size;
                data.Count = (uint)array.Length;
                foreach(var val in array)
                    indices.Add(val);
            });
        }

        void WriteIndexLists(ICanEnumerateWithSize<IndexList> buffer, IHybridBuffer<uint> indices) =>
            WriteDataRange(buffer, indices, indexList => indexList.Indices);
        void WriteWeightedIndexLists(ICanEnumerateWithSize<WeightedIndexList> buffer, IHybridBuffer<WeightedIndexList.Item> indices) =>
            WriteDataRange(buffer, indices, indexList => indexList.Indices);
        void WriteBinaryData(ICanEnumerateWithSize<BinaryData> buffer, IHybridBuffer<byte> indices) =>
            WriteDataRange(buffer, indices, indexList => indexList.Data);

        void WriteStringData(
            ICanEnumerateWithSize<string> buffer,
            IHybridBuffer<string> indices)
        {
            Write<string, uint>(buffer, (item, ptr, index) => {
                if (indices.DistinctItems is not null && indices.DistinctItems.TryGetValue(item, out var stringIndex))
                    ptr[index] = stringIndex;
                else {
                    ptr[index] = indices.Size;
                    indices.Add(item);
                }
            });
        }

        void WriteVectors(ICanEnumerateWithSize<IVector> buffer, IHybridBuffer<float> floats)
        {
            Write<IVector, DataRangeColumnType>(buffer, (item, ptr, index) => {
                ref var data = ref ptr[index];
                data.StartIndex = floats.Size;
                data.Count = item.Size;
                floats.CopyFrom(item.Segment);
            });
        }

        void WriteMatrices(ICanEnumerateWithSize<IMatrix> buffer, IHybridBuffer<float> floats)
        {
            Write<IMatrix, MatrixColumnType>(buffer, (item, ptr, index) => {
                ref var data = ref ptr[index];
                data.StartIndex = floats.Size;
                data.RowCount = item.RowCount;
                data.ColumnCount = item.ColumnCount;
                floats.CopyFrom(item.Segment);
            });
        }

        void WriteTensors(ICanEnumerateWithSize<ITensor3D> buffer, IHybridBuffer<float> floats)
        {
            Write<ITensor3D, Tensor3DColumnType>(buffer, (item, ptr, index) => {
                ref var data = ref ptr[index];
                data.StartIndex = floats.Size;
                data.Depth = item.Depth;
                data.RowCount = item.RowCount;
                data.ColumnCount = item.ColumnCount;
                floats.CopyFrom(item.Segment);
            });
        }

        void WriteTensors(ICanEnumerateWithSize<ITensor4D> buffer, IHybridBuffer<float> floats)
        {
            Write<ITensor4D, Tensor4DColumnType>(buffer, (item, ptr, index) => {
                ref var data = ref ptr[index];
                data.StartIndex = floats.Size;
                data.Count = item.Count;
                data.Depth = item.Depth;
                data.RowCount = item.RowCount;
                data.ColumnCount = item.ColumnCount;
                floats.CopyFrom(item.Segment);
            });
        }
    }
}
