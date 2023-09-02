using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using BrightData.LinearAlgebra.ReadOnly;
using System;
using System.Buffers;
using BrightData.Table.Buffer.Composite;

namespace BrightData.Table.Helper
{
    internal class ColumnOrientedTableBuilder
    {
        readonly IProvideTempData? _tempStreams;
        readonly int _blockSize;
        readonly uint? _maxInMemoryBlocks;
        readonly MethodInfo _writeStructs;

        public ColumnOrientedTableBuilder(IProvideTempData? tempStreams = null, int blockSize = Consts.DefaultBlockSize, uint? maxInMemoryBlocks = null)
        {
            _tempStreams = tempStreams;
            _blockSize = blockSize;
            _maxInMemoryBlocks = maxInMemoryBlocks;
            var methods = typeof(ColumnOrientedTableBuilder).GetGenericMethods();
            _writeStructs = methods[nameof(WriteStructs)];
        }

        public async Task Write(MetaData tableMetaData, ICompositeBuffer[] buffers, Stream output)
        {
            // check that all columns have the same number of rows
            var firstColumn = buffers[0];
            foreach (var otherColumn in buffers.Skip(1)) {
                if (firstColumn.Size != otherColumn.Size)
                    throw new Exception("Columns have different sizes");
            }

            // write the header
            var header = new TableHeader();
            output.Write(MemoryMarshal.CreateReadOnlySpan(ref header, 1).AsBytes());
            header.Version = 1;
            header.Orientation = DataTableOrientation.ColumnOriented;
            header.ColumnCount = (uint)buffers.Length;
            header.RowCount = firstColumn.Size;

            // write the meta data to a temp stream
            using var tempBuffer = new MemoryStream();
            await using var metaDataWriter = new BinaryWriter(tempBuffer, Encoding.UTF8, true);
            tableMetaData.WriteTo(metaDataWriter);

            // prepare the columns and continue writing meta data
            var columns = GetColumns(buffers, metaDataWriter);

            // write the headers
            header.InfoOffset = (uint)output.Position;
            output.Write(MemoryMarshal.AsBytes<TableBase.Column>(columns));
            header.InfoSizeBytes = (uint)(output.Position - header.InfoOffset);
            header.DataOffset = (uint)output.Position;

            var indexWriter         = new Lazy<ICompositeBuffer<uint>>(() => ExtensionMethods.CreateCompositeBuffer<uint>(_tempStreams, _blockSize, _maxInMemoryBlocks));
            var weightedIndexWriter = new Lazy<ICompositeBuffer<WeightedIndexList.Item>>(() => ExtensionMethods.CreateCompositeBuffer<WeightedIndexList.Item>(_tempStreams, _blockSize, _maxInMemoryBlocks));
            var byteWriter          = new Lazy<ICompositeBuffer<byte>>(() => ExtensionMethods.CreateCompositeBuffer<byte>(_tempStreams, _blockSize, _maxInMemoryBlocks));
            var stringWriter        = new Lazy<ICompositeBuffer<string>>(() => ExtensionMethods.CreateCompositeBuffer(_tempStreams, _blockSize, _maxInMemoryBlocks));
            var floatWriter         = new Lazy<ICompositeBuffer<float>>(() => ExtensionMethods.CreateCompositeBuffer<float>(_tempStreams, _blockSize, _maxInMemoryBlocks));

            // write the data (column oriented)
            foreach (var columnSegment in buffers) {
                var columnType = columnSegment.DataType;
                var dataType = columnType.GetTableDataType();

                if (dataType == BrightDataType.IndexList)
                    await WriteIndexLists((ICompositeBuffer<IndexList>)columnSegment, indexWriter.Value, output);
                else if (dataType == BrightDataType.WeightedIndexList)
                    await WriteWeightedIndexLists((ICompositeBuffer<WeightedIndexList>)columnSegment, weightedIndexWriter.Value, output);
                else if (dataType == BrightDataType.BinaryData)
                    await WriteBinaryData((ICompositeBuffer<BinaryData>)columnSegment, byteWriter.Value, output);
                else if (dataType == BrightDataType.String)
                    await WriteStringData((ICompositeBuffer<string>)columnSegment, stringWriter.Value, output);
                else if (dataType == BrightDataType.Vector)
                        await WriteVectors((ICompositeBuffer<ReadOnlyVector>)columnSegment, floatWriter.Value, output);
                else if (dataType == BrightDataType.Matrix)
                    await WriteMatrices((ICompositeBuffer<ReadOnlyMatrix>)columnSegment, floatWriter.Value, output);
                else if (dataType == BrightDataType.Tensor3D)
                    await WriteTensors((ICompositeBuffer<ReadOnlyTensor3D>)columnSegment, floatWriter.Value, output);
                else if (dataType == BrightDataType.Tensor4D)
                    await WriteTensors((ICompositeBuffer<ReadOnlyTensor4D>)columnSegment, floatWriter.Value, output);
                else
                    await (Task<bool>)_writeStructs.MakeGenericMethod(columnType).Invoke(this, new object[] { columnSegment, output })!;
            }
            header.DataSizeBytes = (uint)(output.Position - header.DataOffset);

            // write the strings
            if (stringWriter.IsValueCreated) {
                uint totalSize = 0;
                header.StringOffset = (uint)output.Position;
                await stringWriter.Value.ForEachBlock(block => {
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
                await floatWriter.Value.ForEachBlock(block => {
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
                await byteWriter.Value.ForEachBlock(block => {
                    output.Write(block);
                    totalSize += (uint)block.Length;
                });
                header.BinaryDataSizeBytes = totalSize;
            }

            // write the index data
            if (indexWriter.IsValueCreated) {
                uint totalSize = 0;
                header.IndexOffset = (uint)output.Position;
                await indexWriter.Value.ForEachBlock(block => {
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
                await weightedIndexWriter.Value.ForEachBlock(block => {
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
            output.Write(MemoryMarshal.CreateReadOnlySpan(ref header, 1).AsBytes());
            output.Seek(0, SeekOrigin.End);
        }

        static TableBase.Column[] GetColumns(ICompositeBuffer[] buffers, BinaryWriter metaDataWriter)
        {
            var ret = new TableBase.Column[buffers.Length];
            var index = 0;
            foreach (var columnSegment in buffers) {
                ref var c = ref ret[index++];
                c.DataType = columnSegment.DataType.GetTableDataType();
                (var type, c.DataTypeSize) = c.DataType.GetColumnType();
                columnSegment.MetaData.SetType(c.DataType);
                columnSegment.MetaData.WriteTo(metaDataWriter);
            }
            return ret;
        }

        static Task WriteStructs<T>(ICompositeBuffer<T> input, Stream stream) where T : unmanaged
        {
            return input.ForEachBlock(block => stream.Write(MemoryMarshal.AsBytes(block)));
        }

        delegate void CopyDelegate<CT, T>(in T from, ref CT to) 
            where T : notnull 
            where CT : unmanaged
        ;
        static Task Convert<T, CT>(ICompositeBuffer<T> buffer, Stream stream, CopyDelegate<CT, T> filler)
            where T : notnull
            where CT : unmanaged
        {
            if (buffer.DistinctItems.HasValue) {
                var encodingTable = new Dictionary<T, CT>((int)buffer.DistinctItems.Value);

                return buffer.ForEachBlock(block => {
                    using var temp = SpanOwner<CT>.Allocate(block.Length);
                    var span = temp.Span;
                    var index = 0;
                    foreach (ref readonly var item in block) {
                        if (encodingTable.TryGetValue(item, out var existing))
                            span[index++] = existing;
                        else {
                            ref var converted = ref span[index++];
                            filler(item, ref converted);
                            encodingTable.Add(item, converted);
                        }
                    }
                });
            }

            return buffer.ForEachBlock(block => {
                using var temp = SpanOwner<CT>.Allocate(block.Length);
                var span = temp.Span;
                var index = 0;
                foreach (ref readonly var item in block)
                    filler(item, ref span[index++]);
                stream.Write(span.AsBytes());
            });
        }
        delegate ReadOnlySpan<IT> GetArray<T, IT>(in T item) where IT : unmanaged;
        static Task WriteDataRange<T, IT>(ICompositeBuffer<T> buffer, ICompositeBuffer<IT> indices, Stream stream)
            where T : IHaveSize, IHaveReadOnlyContiguousSpan<IT>
            where IT : unmanaged
        {
            return Convert(buffer, stream, (in T from, ref DataRangeColumnType to) => {
                to.StartIndex = indices.Size;
                to.Size = from.Size;
                indices.Add(from.ReadOnlySpan);
            });
        }

        async Task WriteStringData(
            ICompositeBuffer<string> buffer,
            ICompositeBuffer<string> indices,
            Stream stream)
        {
            var stringTable = buffer.DistinctItems.HasValue
                ? new Dictionary<string, uint>((int)buffer.DistinctItems.Value)
                : null;

            await Convert(buffer, stream, (in string str, ref uint to) => {
                if (stringTable != null) {
                    if(!stringTable.TryGetValue(str, out var stringIndex))
                        stringTable.Add(str, stringIndex = (uint)stringTable.Count);
                    to = stringIndex;
                }
                else {
                    to = indices.Size;
                    indices.Add(str);
                }
            });
            if (stringTable is not null) {
                var orderedStrings = new string[stringTable.Count];
                foreach (var item in stringTable)
                    orderedStrings[item.Value] = item.Key;
                indices.Add(orderedStrings);
            }
        }

        Task WriteIndexLists(ICompositeBuffer<IndexList> buffer, ICompositeBuffer<uint> indices, Stream stream) =>
            WriteDataRange(buffer, indices, stream);

        Task WriteWeightedIndexLists(ICompositeBuffer<WeightedIndexList> buffer, ICompositeBuffer<WeightedIndexList.Item> indices, Stream stream) =>
            WriteDataRange(buffer, indices, stream);

        Task WriteBinaryData(ICompositeBuffer<BinaryData> buffer, ICompositeBuffer<byte> indices, Stream stream) =>
            WriteDataRange(buffer, indices, stream);

        Task WriteVectors(ICompositeBuffer<ReadOnlyVector> buffer, ICompositeBuffer<float> floats, Stream stream) =>
            WriteDataRange(buffer, floats, stream);

        Task WriteMatrices(ICompositeBuffer<ReadOnlyMatrix> buffer, ICompositeBuffer<float> floats, Stream stream)
        {
            return Convert(buffer, stream, (in ReadOnlyMatrix matrix, ref MatrixColumnType data) => {
                data.StartIndex = floats.Size;
                data.RowCount = matrix.RowCount;
                data.ColumnCount = matrix.ColumnCount;
                floats.Add(matrix.ReadOnlySpan);
            });
        }

        Task WriteTensors(ICompositeBuffer<ReadOnlyTensor3D> buffer, ICompositeBuffer<float> floats, Stream stream)
        {
            return Convert(buffer, stream, (in ReadOnlyTensor3D tensor, ref Tensor3DColumnType data) => {
                data.StartIndex = floats.Size;
                data.RowCount = tensor.RowCount;
                data.ColumnCount = tensor.ColumnCount;
                data.Depth = tensor.Depth;
                floats.Add(tensor.ReadOnlySpan);
            });
        }

        Task WriteTensors(ICompositeBuffer<ReadOnlyTensor4D> buffer, ICompositeBuffer<float> floats, Stream stream)
        {
            return Convert(buffer, stream, (in ReadOnlyTensor4D tensor, ref Tensor4DColumnType data) => {
                data.StartIndex = floats.Size;
                data.RowCount = tensor.RowCount;
                data.ColumnCount = tensor.ColumnCount;
                data.Depth = tensor.Depth;
                data.Count = tensor.Count;
                floats.Add(tensor.ReadOnlySpan);
            });
        }
    }
}
