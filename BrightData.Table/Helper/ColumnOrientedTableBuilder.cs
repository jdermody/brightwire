using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using BrightData.LinearAlgebra.ReadOnly;
using BrightData.Table.Buffer;

namespace BrightData.Table.Helper
{
    internal class ColumnOrientedTableBuilder
    {
        readonly IProvideTempStreams? _tempStreams;
        readonly int _blockSize;
        readonly uint? _maxInMemoryBlocks;
        readonly MethodInfo _writeStructs;

        public ColumnOrientedTableBuilder(IProvideTempStreams? tempStreams = null, int blockSize = Consts.DefaultBlockSize, uint? maxInMemoryBlocks = null)
        {
            _tempStreams = tempStreams;
            _blockSize = blockSize;
            _maxInMemoryBlocks = maxInMemoryBlocks;
            var methods = typeof(ColumnOrientedTableBuilder).GetGenericMethods();
            _writeStructs = methods[nameof(WriteStructs)];
        }

        public void Write(MetaData tableMetaData, ICompositeBuffer[] buffers, Stream output)
        {
            // check that all columns have the same number of rows
            var firstColumn = buffers.First();
            foreach (var otherColumn in buffers.Skip(1)) {
                if (firstColumn.Size != otherColumn.Size)
                    throw new Exception("Columns have different sizes");
            }

            // write the header
            Span<TableHeader> headers = stackalloc TableHeader[1];
            output.Write(headers.AsBytes());
            ref var header = ref headers[0];
            header.Version = 1;
            header.Orientation = DataTableOrientation.ColumnOriented;
            header.ColumnCount = (uint)buffers.Length;
            header.RowCount = firstColumn.Size;

            // write the meta data to a temp stream
            using var tempBuffer = new MemoryStream();
            using var metaDataWriter = new BinaryWriter(tempBuffer, Encoding.UTF8, true);
            tableMetaData.WriteTo(metaDataWriter);

            // prepare the columns and continue writing meta data
            var columns = new TableBase.Column[buffers.Length];
            var index = 0;
            foreach (var columnSegment in buffers) {
                ref var c = ref columns[index++];
                c.DataType = columnSegment.DataType.GetTableDataType();
                (var type, c.DataTypeSize) = c.DataType.GetColumnType();
                if (type != columnSegment.DataType)
                    throw new Exception("Unexpected");
                // TODO: write type
                columnSegment.MetaData.WriteTo(metaDataWriter);
            }

            // write the headers
            output.Write(MemoryMarshal.AsBytes<TableBase.Column>(columns));
            header.DataOffset = (uint)output.Position;

            var indexWriter = new Lazy<ICompositeBuffer<uint>>(() => ExtensionMethods.CreateCompositeBuffer<uint>(_tempStreams, _blockSize, _maxInMemoryBlocks));
            var weightedIndexWriter = new Lazy<ICompositeBuffer<WeightedIndexList.Item>>(() => ExtensionMethods.CreateCompositeBuffer<WeightedIndexList.Item>(_tempStreams, _blockSize, _maxInMemoryBlocks));
            var byteWriter = new Lazy<ICompositeBuffer<byte>>(() => ExtensionMethods.CreateCompositeBuffer<byte>(_tempStreams, _blockSize, _maxInMemoryBlocks));
            var stringWriter = new Lazy<ICompositeBuffer<string>>(() => ExtensionMethods.CreateCompositeBuffer(_tempStreams, _blockSize, _maxInMemoryBlocks));
            var floatWriter = new Lazy<ICompositeBuffer<float>>(() => ExtensionMethods.CreateCompositeBuffer<float>(_tempStreams, _blockSize, _maxInMemoryBlocks));

            // write the data (column oriented)
            foreach (var columnSegment in buffers) {
                var columnType = columnSegment.DataType;
                var dataType = columnType.GetTableDataType();

                if (dataType == BrightDataType.IndexList)
                    WriteIndexLists((ICompositeBuffer<IndexList>)columnSegment, indexWriter.Value, output);
                else if (dataType == BrightDataType.WeightedIndexList)
                    WriteWeightedIndexLists((ICompositeBuffer<WeightedIndexList>)columnSegment, weightedIndexWriter.Value, output);
                else if (dataType == BrightDataType.BinaryData)
                    WriteBinaryData((ICompositeBuffer<BinaryData>)columnSegment, byteWriter.Value, output);
                else if (dataType == BrightDataType.String)
                    WriteStringData((ICompositeBuffer<string>)columnSegment, stringWriter.Value, output);
                else if (dataType == BrightDataType.Vector)
                    WriteVectors((ICompositeBuffer<ReadOnlyVector>)columnSegment, floatWriter.Value, output);
                else if (dataType == BrightDataType.Matrix)
                    WriteMatrices((ICompositeBuffer<ReadOnlyMatrix>)columnSegment, floatWriter.Value, output);
                else if (dataType == BrightDataType.Tensor3D)
                    WriteTensors((ICompositeBuffer<ReadOnlyTensor3D>)columnSegment, floatWriter.Value, output);
                else if (dataType == BrightDataType.Tensor4D)
                    WriteTensors((ICompositeBuffer<ReadOnlyTensor4D>)columnSegment, floatWriter.Value, output);
                else
                    _writeStructs.MakeGenericMethod(columnType).Invoke(this, new object[] { columnSegment });
            }
            header.DataSizeBytes = (uint)(output.Position - header.DataOffset);

            // write the strings
            if (stringWriter.IsValueCreated) {
                uint totalSize = 0;
                header.StringOffset = (uint)output.Position;
                stringWriter.Value.ForEachBlock(block => {
                    foreach (var str in block) {
                        StringCompositeBuffer.Encode(str, bytes => {
                            output.Write(bytes);
                            totalSize += (uint)bytes.Length;
                        });
                    }
                });
                header.StringSizeBytes = totalSize;
            }

            // write the float data
            if (floatWriter.IsValueCreated) {
                uint totalSize = 0;
                header.TensorOffset = (uint)output.Position;
                floatWriter.Value.ForEachBlock(block => {
                    var bytes = block.AsBytes();
                    output.Write(bytes);
                    totalSize += (uint)bytes.Length;
                });
                header.TensorSizeBytes = totalSize;
            }

            // write the binary data
            if (byteWriter.IsValueCreated) {
                uint totalSize = 0;
                header.BinaryDataOffset = (uint)output.Position;
                byteWriter.Value.ForEachBlock(block => {
                    output.Write(block);
                    totalSize += (uint)block.Length;
                });
                header.BinaryDataSizeBytes = totalSize;
            }

            // write the index data
            if (indexWriter.IsValueCreated) {
                uint totalSize = 0;
                header.IndexOffset = (uint)output.Position;
                indexWriter.Value.ForEachBlock(block => {
                    var bytes = block.AsBytes();
                    output.Write(bytes);
                    totalSize += (uint)bytes.Length;
                });
                header.IndexSizeBytes = totalSize;
            }

            // write the weighted index data
            if (weightedIndexWriter.IsValueCreated) {
                uint totalSize = 0;
                header.WeightedIndexOffset = (uint)output.Position;
                weightedIndexWriter.Value.ForEachBlock(block => {
                    var bytes = block.AsBytes();
                    output.Write(bytes);
                    totalSize += (uint)bytes.Length;
                });
                header.WeightedIndexSizeBytes = totalSize;
            }

            // write the meta data
            header.MetaDataOffset = (uint)output.Position;
            metaDataWriter.Flush();
            tempBuffer.WriteTo(output);
            header.MetaDataSizeBytes = (uint)output.Position - header.MetaDataOffset;

            // update the header
            output.Seek(0, SeekOrigin.Begin);
            output.Write(MemoryMarshal.AsBytes<TableHeader>(headers));
            output.Seek(0, SeekOrigin.End);
        }

        static void WriteStructs<T>(ICompositeBuffer<T> input, Stream stream) where T : unmanaged
        {
            input.ForEachBlock(block => stream.Write(MemoryMarshal.AsBytes(block)));
        }

        delegate void CopyDelegate<CT, T>(ReadOnlySpan<T> from, Span<CT> to) 
            where T : notnull 
            where CT : unmanaged
        ;
        static void Convert<T, CT>(ICompositeBuffer<T> buffer, Stream stream, CopyDelegate<CT, T> filler)
            where T : notnull
            where CT : unmanaged
        {
            buffer.ForEachBlock(block => {
                using var temp = SpanOwner<CT>.Allocate(block.Length);
                var span = temp.Span;
                filler(block, span);
                stream.Write(MemoryMarshal.AsBytes(span));
            });
        }
        delegate ReadOnlySpan<IT> GetArray<in T, IT>(T item) where IT : unmanaged;
        static void WriteDataRange<T, IT>(ICompositeBuffer<T> buffer, ICompositeBuffer<IT> indices, Stream stream, GetArray<T, IT> getArray)
            where T : notnull
            where IT : unmanaged
        {
            Convert<T, DataRangeColumnType>(buffer, stream, (from, to) => {
                var index = 0;
                foreach (var item in from) {
                    ref var data = ref to[index];
                    var array = getArray(item);
                    data.StartIndex = indices.Size;
                    data.Size = (uint)array.Length;
                    foreach (var val in array)
                        indices.Add(val);
                    ++index;
                }
            });
        }

        void WriteStringData(
            ICompositeBuffer<string> buffer,
            ICompositeBuffer<string> indices,
            Stream stream)
        {
            var stringTable = buffer.DistinctItems.HasValue
                ? new Dictionary<string, uint>((int)buffer.DistinctItems.Value)
                : null;

            Convert<string, uint>(buffer, stream, (from, to) => {
                var index = 0;
                foreach (var str in from) {
                    if (stringTable != null) {
                        if(!stringTable.TryGetValue(str, out var stringIndex))
                            stringTable.Add(str, stringIndex = (uint)stringTable.Count);
                        to[index] = stringIndex;
                    }
                    else {
                        to[index] = indices.Size;
                        indices.Add(str);
                    }

                    ++index;
                }
            });
            if (stringTable is not null) {
                var orderedStrings = new string[stringTable.Count];
                foreach (var item in stringTable)
                    orderedStrings[item.Value] = item.Key;
                indices.Add(orderedStrings);
            }
        }

        void WriteIndexLists(ICompositeBuffer<IndexList> buffer, ICompositeBuffer<uint> indices, Stream stream) =>
            WriteDataRange(buffer, indices, stream, indexList => indexList.AsSpan());

        void WriteWeightedIndexLists(ICompositeBuffer<WeightedIndexList> buffer, ICompositeBuffer<WeightedIndexList.Item> indices, Stream stream) =>
            WriteDataRange(buffer, indices, stream, indexList => indexList.AsSpan());

        void WriteBinaryData(ICompositeBuffer<BinaryData> buffer, ICompositeBuffer<byte> indices, Stream stream) =>
            WriteDataRange(buffer, indices, stream, indexList => indexList.Data);

        void WriteVectors(ICompositeBuffer<ReadOnlyVector> buffer, ICompositeBuffer<float> floats, Stream stream)
        {
            WriteDataRange(buffer, floats, stream, vector => vector.FloatSpan);
        }

        void WriteMatrices(ICompositeBuffer<ReadOnlyMatrix> buffer, ICompositeBuffer<float> floats, Stream stream)
        {
            Convert<ReadOnlyMatrix, MatrixColumnType>(buffer, stream, (from, to) => {
                var index = 0;
                foreach (var item in from) {
                    ref var data = ref to[index];
                    data.StartIndex = floats.Size;
                    data.RowCount = item.RowCount;
                    data.ColumnCount = item.ColumnCount;
                    floats.Add(item.FloatSpan);
                    ++index;
                }
            });
        }

        void WriteTensors(ICompositeBuffer<ReadOnlyTensor3D> buffer, ICompositeBuffer<float> floats, Stream stream)
        {
            Convert<ReadOnlyTensor3D, Tensor3DColumnType>(buffer, stream, (from, to) => {
                var index = 0;
                foreach (var item in from) {
                    ref var data = ref to[index];
                    data.StartIndex = floats.Size;
                    data.RowCount = item.RowCount;
                    data.ColumnCount = item.ColumnCount;
                    data.Depth = item.Depth;
                    floats.Add(item.FloatSpan);
                    ++index;
                }
            });
        }

        void WriteTensors(ICompositeBuffer<ReadOnlyTensor4D> buffer, ICompositeBuffer<float> floats, Stream stream)
        {
            Convert<ReadOnlyTensor4D, Tensor4DColumnType>(buffer, stream, (from, to) => {
                var index = 0;
                foreach (var item in from) {
                    ref var data = ref to[index];
                    data.StartIndex = floats.Size;
                    data.RowCount = item.RowCount;
                    data.ColumnCount = item.ColumnCount;
                    data.Depth = item.Depth;
                    data.Count = item.Count;
                    floats.Add(item.FloatSpan);
                    ++index;
                }
            });
        }
    }
}
