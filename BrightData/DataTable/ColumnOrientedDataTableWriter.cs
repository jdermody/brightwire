﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using BrightData.Buffer.Composite;
using BrightData.DataTable.Columns;
using BrightData.LinearAlgebra.ReadOnly;
using BrightData.Types;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData.DataTable
{
    /// <summary>
    /// Writes buffers to a data table
    /// </summary>
    internal class ColumnOrientedDataTableWriter(
        IProvideByteBlocks? tempData = null, 
        int blockSize = Consts.DefaultInitialBlockSize, 
        int maxBlockSize = Consts.DefaultMaxBlockSize,
        uint? maxInMemoryBlocks = Consts.DefaultMaxBlocksInMemory)
    {
        public async Task Write(MetaData tableMetaData, IReadOnlyBufferWithMetaData[] buffers, Stream output)
        {
            // check that all columns have the same number of rows
            var firstColumn = buffers[0];
            foreach (var otherColumn in buffers.Skip(1)) {
                if (firstColumn.Size != otherColumn.Size)
                    throw new ArgumentException("Columns have different sizes", nameof(buffers));
            }

            // write the header
            var header = new TableHeader();
            output.Write(MemoryMarshal.CreateReadOnlySpan(ref header, 1).AsBytes());
            header.Version = 1;
            header.Orientation = DataTableOrientation.ColumnOriented;
            header.ColumnCount = (uint)buffers.Length;
            header.RowCount = firstColumn.Size;

            // write the metadata to a temp stream
            using var tempBuffer = new MemoryStream();
            await using var metaDataWriter = new BinaryWriter(tempBuffer, Encoding.UTF8, true);
            tableMetaData.WriteTo(metaDataWriter);

            // prepare the columns and continue writing metadata
            var columns = GetColumns(buffers, metaDataWriter);

            // write the headers
            header.InfoOffset = (uint)output.Position;
            output.Write(MemoryMarshal.AsBytes<ColumnOrientedDataTable.Column>(columns));
            header.InfoSizeBytes = (uint)(output.Position - header.InfoOffset);
            header.DataOffset = (uint)output.Position;

            var indexWriter         = new Lazy<ICompositeBuffer<uint>>(() => tempData.CreateCompositeBuffer<uint>(blockSize, maxBlockSize, maxInMemoryBlocks));
            var weightedIndexWriter = new Lazy<ICompositeBuffer<WeightedIndexList.Item>>(() => tempData.CreateCompositeBuffer<WeightedIndexList.Item>(blockSize, maxBlockSize, maxInMemoryBlocks));
            var byteWriter          = new Lazy<ICompositeBuffer<byte>>(() => tempData.CreateCompositeBuffer<byte>(blockSize, maxBlockSize, maxInMemoryBlocks));
            var stringWriter        = new Lazy<ICompositeBuffer<string>>(() => tempData.CreateCompositeBuffer(blockSize, maxBlockSize, maxInMemoryBlocks));
            var floatWriter         = new Lazy<ICompositeBuffer<float>>(() => tempData.CreateCompositeBuffer<float>(blockSize, maxBlockSize, maxInMemoryBlocks));

            // write the data (column oriented)
            foreach (var columnSegment in buffers) {
                var columnType = columnSegment.DataType;
                var dataType = columnType.GetBrightDataType();

                if (dataType == BrightDataType.IndexList)
                    await WriteIndexLists((IReadOnlyBuffer<IndexList>)columnSegment, indexWriter.Value, output);
                else if (dataType == BrightDataType.WeightedIndexList)
                    await WriteWeightedIndexLists((IReadOnlyBuffer<WeightedIndexList>)columnSegment, weightedIndexWriter.Value, output);
                else if (dataType == BrightDataType.BinaryData)
                    await WriteBinaryData((IReadOnlyBuffer<BinaryData>)columnSegment, byteWriter.Value, output);
                else if (dataType == BrightDataType.String)
                    await WriteStringData((IReadOnlyBuffer<string>)columnSegment, stringWriter.Value, output);
                else if (dataType == BrightDataType.Vector)
                    await WriteVectors((IReadOnlyBuffer<ReadOnlyVector<float>>)columnSegment, floatWriter.Value, output);
                else if (dataType == BrightDataType.Matrix)
                    await WriteMatrices((IReadOnlyBuffer<ReadOnlyMatrix<float>>)columnSegment, floatWriter.Value, output);
                else if (dataType == BrightDataType.Tensor3D)
                    await WriteTensors((IReadOnlyBuffer<ReadOnlyTensor3D<float>>)columnSegment, floatWriter.Value, output);
                else if (dataType == BrightDataType.Tensor4D)
                    await WriteTensors((IReadOnlyBuffer<ReadOnlyTensor4D<float>>)columnSegment, floatWriter.Value, output);
                else
                    await WriteStructs(columnSegment, output);
            }
            header.DataSizeBytes = (uint)(output.Position - header.DataOffset);

            // write the strings
            if (stringWriter.IsValueCreated) {
                uint totalSize = 0;
                header.StringOffset = (uint)output.Position;
                await stringWriter.Value.ForEachBlock(block => {
                    foreach (var str in block) {
                        StringCompositeBuffer.Block.Encode(str, bytes => {
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
            //header.MetaDataSizeBytes = (uint)output.Position - header.MetaDataOffset;

            // update the header
            output.Seek(0, SeekOrigin.Begin);
            output.Write(MemoryMarshal.CreateReadOnlySpan(ref header, 1).AsBytes());
            output.Seek(0, SeekOrigin.Begin);
        }

        static ColumnOrientedDataTable.Column[] GetColumns(IReadOnlyBufferWithMetaData[] buffers, BinaryWriter metaDataWriter)
        {
            var ret = new ColumnOrientedDataTable.Column[buffers.Length];
            var index = 0;
            foreach (var columnSegment in buffers) {
                var dataType = columnSegment.DataType.GetBrightDataType();
                var (_, dataTypeSize) = dataType.GetColumnType();
                ret[index++] = new ColumnOrientedDataTable.Column {
                    DataType = dataType,
                    DataTypeSize = dataTypeSize
                };
                columnSegment.MetaData.SetType(dataType);
                columnSegment.MetaData.WriteTo(metaDataWriter);
            }
            return ret;
        }

        static Task WriteStructs(IReadOnlyBuffer buffer, Stream stream)
        {
            var type = buffer.DataType;
            if (type == typeof(bool))
                return WriteStructs((IReadOnlyBuffer<bool>)buffer, stream);
            if (type == typeof(sbyte))
                return WriteStructs((IReadOnlyBuffer<sbyte>)buffer, stream);
            if (type == typeof(short))
                return WriteStructs((IReadOnlyBuffer<short>)buffer, stream);
            if (type == typeof(int))
                return WriteStructs((IReadOnlyBuffer<int>)buffer, stream);
            if (type == typeof(long))
                return WriteStructs((IReadOnlyBuffer<long>)buffer, stream);
            if (type == typeof(float))
                return WriteStructs((IReadOnlyBuffer<float>)buffer, stream);
            if (type == typeof(double))
                return WriteStructs((IReadOnlyBuffer<double>)buffer, stream);
            if (type == typeof(decimal))
                return WriteStructs((IReadOnlyBuffer<decimal>)buffer, stream);
            if (type == typeof(uint))
                return WriteStructs((IReadOnlyBuffer<uint>)buffer, stream);
            if (type == typeof(DateTime))
                return WriteStructs((IReadOnlyBuffer<DateTime>)buffer, stream);
            if (type == typeof(TimeOnly))
                return WriteStructs((IReadOnlyBuffer<TimeOnly>)buffer, stream);
            if (type == typeof(DateOnly))
                return WriteStructs((IReadOnlyBuffer<DateOnly>)buffer, stream);
            throw new NotImplementedException($"{type} is not a supported column type");
        }
        static Task WriteStructs<T>(IReadOnlyBuffer<T> input, Stream stream) where T : unmanaged
        {
            return input.ForEachBlock(block => stream.Write(MemoryMarshal.AsBytes(block)));
        }
        delegate void CopyDelegate<CT, T>(in T from, ref CT to)
            where T : notnull 
            where CT : unmanaged
        ;
        static Task Convert<T, CT>(IReadOnlyBuffer<T> buffer, Stream stream, CopyDelegate<CT, T> filler)
            where T : notnull
            where CT : unmanaged
        {
            if (buffer is IHaveDistinctItemCount { DistinctItems: not null } hasDistinctItems) {
                var encodingTable = new Dictionary<T, CT>((int)hasDistinctItems.DistinctItems.Value);

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

        static Task WriteDataRange<T, IT>(IReadOnlyBuffer<T> buffer, ICompositeBuffer<IT> indices, Stream stream)
            where T : IHaveSize, IHaveReadOnlyTensorSegment<IT>
            where IT : unmanaged, INumber<IT>
        {
            return Convert(buffer, stream, (in T from, ref DataRangeColumnType to) => {
                to.StartIndex = indices.Size;
                to.Size = from.Size;

                var segment = from.ReadOnlySegment;
                var contiguous = segment.Contiguous;
                if(contiguous is not null)
                    indices.Append(contiguous.ReadOnlySpan);
                else {
                    var temp = SpanOwner<IT>.Empty;
                    var span = segment.GetSpan(ref temp, out var wasTempUsed);
                    try {
                        indices.Append(span);
                    }
                    finally {
                        if(wasTempUsed)
                            temp.Dispose();
                    }
                }
            });
        }

        static Task WriteContiguousDataRange<T, IT>(IReadOnlyBuffer<T> buffer, ICompositeBuffer<IT> indices, Stream stream)
            where T : IHaveSize, IHaveReadOnlyContiguousMemory<IT>
            where IT : unmanaged
        {
            return Convert(buffer, stream, (in T from, ref DataRangeColumnType to) => {
                to.StartIndex = indices.Size;
                to.Size = from.Size;
                indices.Append(from.ReadOnlySpan);
            });
        }

        async Task WriteStringData(
            IReadOnlyBuffer<string> buffer,
            ICompositeBuffer<string> indices,
            Stream stream)
        {
            var stringTable = buffer is IHaveDistinctItemCount { DistinctItems: not null } distinctItems
                ? new Dictionary<string, uint>((int)distinctItems.DistinctItems.Value)
                : null;

            await Convert(buffer, stream, (in string str, ref uint to) => {
                if (stringTable != null) {
                    if(!stringTable.TryGetValue(str, out var stringIndex))
                        stringTable.Add(str, stringIndex = (uint)stringTable.Count);
                    to = stringIndex;
                }
                else {
                    to = indices.Size;
                    indices.Append(str);
                }
            });
            if (stringTable is not null) {
                var orderedStrings = new string[stringTable.Count];
                foreach (var item in stringTable)
                    orderedStrings[item.Value] = item.Key;
                indices.Append(orderedStrings);
            }
        }

        static Task WriteIndexLists(IReadOnlyBuffer<IndexList> buffer, ICompositeBuffer<uint> indices, Stream stream) => WriteContiguousDataRange(buffer, indices, stream);
        static Task WriteWeightedIndexLists(IReadOnlyBuffer<WeightedIndexList> buffer, ICompositeBuffer<WeightedIndexList.Item> indices, Stream stream) => WriteContiguousDataRange(buffer, indices, stream);
        static Task WriteBinaryData(IReadOnlyBuffer<BinaryData> buffer, ICompositeBuffer<byte> indices, Stream stream) => WriteContiguousDataRange(buffer, indices, stream);
        static Task WriteVectors(IReadOnlyBuffer<ReadOnlyVector<float>> buffer, ICompositeBuffer<float> floats, Stream stream) => WriteDataRange(buffer, floats, stream);

        static Task WriteMatrices(IReadOnlyBuffer<ReadOnlyMatrix<float>> buffer, ICompositeBuffer<float> floats, Stream stream)
        {
            return Convert(buffer, stream, (in ReadOnlyMatrix<float> matrix, ref MatrixColumnType data) => {
                data.StartIndex = floats.Size;
                data.RowCount = matrix.RowCount;
                data.ColumnCount = matrix.ColumnCount;
                floats.Append(matrix.ReadOnlySpan);
            });
        }

        static Task WriteTensors(IReadOnlyBuffer<ReadOnlyTensor3D<float>> buffer, ICompositeBuffer<float> floats, Stream stream)
        {
            return Convert(buffer, stream, (in ReadOnlyTensor3D<float> tensor, ref Tensor3DColumnType data) => {
                data.StartIndex = floats.Size;
                data.RowCount = tensor.RowCount;
                data.ColumnCount = tensor.ColumnCount;
                data.Depth = tensor.Depth;
                floats.Append(tensor.ReadOnlySpan);
            });
        }

        static Task WriteTensors(IReadOnlyBuffer<ReadOnlyTensor4D<float>> buffer, ICompositeBuffer<float> floats, Stream stream)
        {
            return Convert(buffer, stream, (in ReadOnlyTensor4D<float> tensor, ref Tensor4DColumnType data) => {
                data.StartIndex = floats.Size;
                data.RowCount = tensor.RowCount;
                data.ColumnCount = tensor.ColumnCount;
                data.Depth = tensor.Depth;
                data.Count = tensor.Count;
                floats.Append(tensor.ReadOnlySpan);
            });
        }
    }
}
