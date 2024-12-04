using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BrightData.Buffer;
using BrightData.Buffer.Composite;
using BrightData.Buffer.Operations;
using BrightData.Buffer.Operations.Conversion;
using BrightData.Buffer.ReadOnly;
using BrightData.Buffer.ReadOnly.Converter;
using BrightData.Converter;
using BrightData.Helper;
using BrightData.LinearAlgebra.ReadOnly;
using CommunityToolkit.HighPerformance.Buffers;
using BrightData.Types;
using BrightData.Buffer.Vectorisation;
using BrightData.DataTable.Columns;
using Microsoft.VisualBasic;

namespace BrightData
{
    public partial class ExtensionMethods
    {
        /// <summary>
        /// Creates an appendable in memory buffer
        /// </summary>
        /// <param name="blockSize">Initial size of each block</param>
        /// <param name="maxBlockSize">Max block size</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IAppendableBuffer<T> CreateInMemoryBuffer<T>(this uint blockSize, uint maxBlockSize) where T : notnull => new InMemoryBuffer<T>(blockSize, maxBlockSize);

        /// <summary>
        /// Creates a new buffer in which each value is converted via a mapping function
        /// </summary>
        /// <typeparam name="FT"></typeparam>
        /// <typeparam name="TT"></typeparam>
        /// <param name="buffer"></param>
        /// <param name="converter"></param>
        /// <returns></returns>
        public static IReadOnlyBuffer<TT> Map<FT, TT>(this IReadOnlyBuffer<FT> buffer, Func<FT, TT> converter)
            where FT: notnull
            where TT: notnull
        {
            return (IReadOnlyBuffer<TT>)GenericTypeMapping.TypeConverter(typeof(TT), buffer, new CustomConversionFunction<FT, TT>(converter));
        }

        /// <summary>
        /// Enumerates values in the buffer (blocking)
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static IEnumerable<object> Enumerate(this IReadOnlyBuffer buffer)
        {
            return buffer.EnumerateAll().ToBlockingEnumerable();
        }

        /// <summary>
        /// Enumerates values in the buffer (blocking)
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static IEnumerable<T> Enumerate<T>(this IReadOnlyBuffer<T> buffer)
            where T : notnull
        {
            return buffer.EnumerateAllTyped().ToBlockingEnumerable();
        }

        /// <summary>
        /// Async enumeration of values in the buffer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="buffer"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static IAsyncEnumerable<T> AsyncEnumerate<T>(this IReadOnlyBuffer buffer) where T: notnull
        {
            if (buffer.DataType != typeof(T))
                throw new ArgumentException($"Buffer is of type {buffer.DataType} but requested {typeof(T)}");
            var typedBuffer = (IReadOnlyBuffer<T>)buffer;
            return typedBuffer.EnumerateAllTyped();
        }

        /// <summary>
        /// Async enumeration of values in the buffer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="buffer"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static IAsyncEnumerable<T> AsyncEnumerate<T>(this IReadOnlyBuffer<T> buffer) where T: notnull
        {
            return buffer.EnumerateAllTyped();
        }

        /// <summary>
        /// Creates an array from the buffer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static async Task<T[]> ToArray<T>(this IReadOnlyBuffer buffer) where T : notnull
        {
            var ret = new T[buffer.Size];
            var index = 0;
            await foreach(var item in buffer.AsyncEnumerate<T>())
                ret[index++] = item;
            return ret;
        }

        /// <summary>
        /// Finds distinct groups within the buffers based on string comparison of the concatenated values
        /// </summary>
        /// <param name="buffers"></param>
        /// <returns></returns>
        public static async Task<Dictionary<string /* group */, List<uint> /* row indices in group */>> GetGroups(this IReadOnlyBuffer[] buffers)
        {
            // ReSharper disable once NotDisposedResourceIsReturned
            var enumerators = buffers.Select(x => x.EnumerateAll().GetAsyncEnumerator()).ToArray();
            var shouldContinue = true;
            var sb = new StringBuilder();
            var ret = new Dictionary<string, List<uint>>();
            uint rowIndex = 0;

            while (shouldContinue) {
                sb.Clear();
                foreach (var enumerator in enumerators) {
                    if (!await enumerator.MoveNextAsync()) {
                        shouldContinue = false; 
                        break;
                    }
                    if (sb.Length > 0)
                        sb.Append('|');
                    sb.Append(enumerator.Current);
                }

                if (shouldContinue) {
                    var str = sb.ToString();
                    if (!ret.TryGetValue(str, out var list))
                        ret.Add(str, list = []);
                    list.Add(rowIndex++);
                }
            }

            foreach (var enumerator in enumerators)
                await enumerator.DisposeAsync();

            return ret;
        }

        class MemorySegment<T> : ReadOnlySequenceSegment<T>
        {
            public MemorySegment(ReadOnlyMemory<T> memory) => Memory = memory;
            public MemorySegment<T> Append(ReadOnlyMemory<T> memory)
            {
                var segment = new MemorySegment<T>(memory) {
                    RunningIndex = RunningIndex + Memory.Length
                };
                Next = segment;
                return segment;
            }
        }

        /// <summary>
        /// Converts the buffer to a read only sequence
        /// </summary>
        /// <param name="buffer"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static async Task<ReadOnlySequence<T>> AsReadOnlySequence<T>(this IReadOnlyBuffer<T> buffer) where T : notnull
        {
            var blockSizes = buffer.BlockSizes;
            if(blockSizes.Length == 0)
                return ReadOnlySequence<T>.Empty;

            var first = new MemorySegment<T>(await buffer.GetTypedBlock(0));
            var last = first;
            for(uint i = 1; i < blockSizes.Length; i++)
                last = last.Append(await buffer.GetTypedBlock(i));
            return new ReadOnlySequence<T>(first, 0, last, last.Memory.Length);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sequence"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static SequenceReader<T> AsSequenceReader<T>(this ReadOnlySequence<T> sequence) where T : unmanaged, IEquatable<T>
        {
            return new SequenceReader<T>(sequence);
        }

        /// <summary>
        /// Retrieves an item from the buffer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="buffer"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static async Task<T> GetItem<T>(this IReadOnlyBuffer<T> buffer, uint index) where T: notnull
        {
            uint offset = 0, blockIndex = 0;
            foreach (var block in buffer.BlockSizes) {
                if (index - offset < block) {
                    var blockMemory = await buffer.GetTypedBlock(blockIndex);
                    var ret = blockMemory.Span[(int)(index - offset)];
                    return ret;
                }
                offset += block;
                ++blockIndex;
            }

            throw new ArgumentException($"Item not found at index {index}");
        }

        /// <summary>
        /// Retrieves items from the buffer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="buffer"></param>
        /// <param name="indices"></param>
        /// <returns></returns>
        public static async Task<T[]> GetItems<T>(this IReadOnlyBuffer<T> buffer, uint[] indices) where T: notnull
        {
            var blocks = buffer.GetIndices(indices)
                .GroupBy(x => x.BlockIndex)
                .OrderBy(x => x.Key)
            ;
            var ret = new T[indices.Length];
            foreach (var block in blocks) {
                var blockMemory = await buffer.GetTypedBlock(block.Key);
                AddIndexedItems(blockMemory, block, ret);
            }
            return ret;

            static void AddIndexedItems(ReadOnlyMemory<T> data, IEnumerable<(uint RowIndex, uint BlockIndex, uint RelativeBlockIndex, uint SourceIndex)> list, T[] output)
            {
                var span = data.Span;
                foreach (var (_, _, relativeIndex, sourceIndex) in list)
                    output[sourceIndex] = span[(int)relativeIndex];
            }
        }

        

        /// <summary>
        /// Converts the buffer to an array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static async Task<T[]> ToArray<T>(this IReadOnlyBuffer<T> buffer) where T : notnull
        {
            var ret = new T[buffer.Size];
            var offset = 0;
            await buffer.ForEachBlock(x => {
                x.CopyTo(new Span<T>(ret, offset, x.Length));
                offset += x.Length;
            });
            return ret;
        }

        /// <summary>
        /// Creates a composite buffer for strings
        /// </summary>
        /// <param name="tempStreams"></param>
        /// <param name="blockSize"></param>
        /// <param name="maxBlockSize"></param>
        /// <param name="maxInMemoryBlocks"></param>
        /// <param name="maxDistinctItems"></param>
        /// <returns></returns>
        public static ICompositeBuffer<string> CreateCompositeBuffer(
            this IProvideByteBlocks? tempStreams, 
            int blockSize = Consts.DefaultInitialBlockSize, 
            int maxBlockSize = Consts.DefaultMaxBlockSize,
            uint? maxInMemoryBlocks = null,
            uint? maxDistinctItems = null
        ) => new StringCompositeBuffer(tempStreams, blockSize, maxBlockSize, maxInMemoryBlocks, maxDistinctItems);

        /// <summary>
        /// Creates a composite buffer for types that can be created from a block of byte data
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tempStreams"></param>
        /// <param name="createItem"></param>
        /// <param name="blockSize"></param>
        /// <param name="maxBlockSize"></param>
        /// <param name="maxInMemoryBlocks"></param>
        /// <param name="maxDistinctItems"></param>
        /// <returns></returns>
        public static ICompositeBuffer<T> CreateCompositeBuffer<T>(
            this IProvideByteBlocks? tempStreams,
            CreateFromReadOnlyByteSpan<T> createItem,
            int blockSize = Consts.DefaultInitialBlockSize, 
            int maxBlockSize = Consts.DefaultMaxBlockSize,
            uint? maxInMemoryBlocks = null,
            uint? maxDistinctItems = null) where T: IHaveDataAsReadOnlyByteSpan => new ManagedCompositeBuffer<T>(createItem, tempStreams, blockSize, maxBlockSize, maxInMemoryBlocks, maxDistinctItems);

        /// <summary>
        /// Creates a composite buffer for unmanaged types
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tempStreams"></param>
        /// <param name="blockSize"></param>
        /// <param name="maxBlockSize"></param>
        /// <param name="maxInMemoryBlocks"></param>
        /// <param name="maxDistinctItems"></param>
        /// <returns></returns>
        public static ICompositeBuffer<T> CreateCompositeBuffer<T>(
            this IProvideByteBlocks? tempStreams, 
            int blockSize = Consts.DefaultInitialBlockSize, 
            int maxBlockSize = Consts.DefaultMaxBlockSize,
            uint? maxInMemoryBlocks = null,
            uint? maxDistinctItems = null
        ) where T: unmanaged => new UnmanagedCompositeBuffer<T>(tempStreams, blockSize, maxBlockSize, maxInMemoryBlocks, maxDistinctItems);

        /// <summary>
        /// Creates a composite buffer
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="tempStreams"></param>
        /// <param name="blockSize"></param>
        /// <param name="maxBlockSize"></param>
        /// <param name="maxInMemoryBlocks"></param>
        /// <param name="maxDistinctItems"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static ICompositeBuffer CreateCompositeBuffer(
            this BrightDataType dataType,
            IProvideByteBlocks? tempStreams = null,
            int blockSize = Consts.DefaultInitialBlockSize, 
            int maxBlockSize = Consts.DefaultMaxBlockSize,
            uint? maxInMemoryBlocks = null,
            uint? maxDistinctItems = null)
        {
            return dataType switch {
                BrightDataType.Boolean           => CreateCompositeBuffer<bool>(tempStreams, blockSize, maxBlockSize, maxInMemoryBlocks, maxDistinctItems),
                BrightDataType.Date              => CreateCompositeBuffer<DateTime>(tempStreams, blockSize, maxBlockSize, maxInMemoryBlocks, maxDistinctItems),
                BrightDataType.DateOnly          => CreateCompositeBuffer<DateOnly>(tempStreams, blockSize, maxBlockSize, maxInMemoryBlocks, maxDistinctItems),
                BrightDataType.TimeOnly          => CreateCompositeBuffer<TimeOnly>(tempStreams, blockSize, maxBlockSize, maxInMemoryBlocks, maxDistinctItems),
                BrightDataType.Decimal           => CreateCompositeBuffer<decimal>(tempStreams, blockSize, maxBlockSize, maxInMemoryBlocks, maxDistinctItems),
                BrightDataType.SByte             => CreateCompositeBuffer<sbyte>(tempStreams, blockSize, maxBlockSize, maxInMemoryBlocks, maxDistinctItems),
                BrightDataType.Short             => CreateCompositeBuffer<short>(tempStreams, blockSize, maxBlockSize, maxInMemoryBlocks, maxDistinctItems),
                BrightDataType.Int               => CreateCompositeBuffer<int>(tempStreams, blockSize, maxBlockSize, maxInMemoryBlocks, maxDistinctItems),
                BrightDataType.Long              => CreateCompositeBuffer<long>(tempStreams, blockSize, maxBlockSize, maxInMemoryBlocks, maxDistinctItems),
                BrightDataType.Float             => CreateCompositeBuffer<float>(tempStreams, blockSize, maxBlockSize, maxInMemoryBlocks, maxDistinctItems),
                BrightDataType.Double            => CreateCompositeBuffer<double>(tempStreams, blockSize, maxBlockSize, maxInMemoryBlocks, maxDistinctItems),
                BrightDataType.String            => CreateCompositeBuffer(tempStreams, blockSize, maxBlockSize, maxInMemoryBlocks, maxDistinctItems),
                BrightDataType.BinaryData        => CreateCompositeBuffer<BinaryData>(tempStreams, x              => new(x), blockSize, maxBlockSize, maxInMemoryBlocks, maxDistinctItems),
                BrightDataType.IndexList         => CreateCompositeBuffer<IndexList>(tempStreams, x               => new(x), blockSize, maxBlockSize, maxInMemoryBlocks, maxDistinctItems),
                BrightDataType.WeightedIndexList => CreateCompositeBuffer<WeightedIndexList>(tempStreams, x       => new(x), blockSize, maxBlockSize, maxInMemoryBlocks, maxDistinctItems),
                BrightDataType.Vector            => CreateCompositeBuffer<ReadOnlyVector<float>>(tempStreams, x   => new(x), blockSize, maxBlockSize, maxInMemoryBlocks, maxDistinctItems),
                BrightDataType.Matrix            => CreateCompositeBuffer<ReadOnlyMatrix<float>>(tempStreams, x   => new(x), blockSize, maxBlockSize, maxInMemoryBlocks, maxDistinctItems),
                BrightDataType.Tensor3D          => CreateCompositeBuffer<ReadOnlyTensor3D<float>>(tempStreams, x => new(x), blockSize, maxBlockSize, maxInMemoryBlocks, maxDistinctItems),
                BrightDataType.Tensor4D          => CreateCompositeBuffer<ReadOnlyTensor4D<float>>(tempStreams, x => new(x), blockSize, maxBlockSize, maxInMemoryBlocks, maxDistinctItems),
                _                                => throw new ArgumentOutOfRangeException(nameof(dataType), dataType, $"Not able to create a composite buffer for type: {dataType}")
            };
        }

        /// <summary>
        /// Creates a buffer writer from an appendable buffer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="buffer"></param>
        /// <param name="bufferSize"></param>
        /// <returns></returns>
        public static IBufferWriter<T> AsBufferWriter<T>(this IAppendBlocks<T> buffer, int bufferSize = 256) where T : notnull => new BlockBufferWriter<T>(buffer, bufferSize);

        /// <summary>
        /// Returns true of the buffer can be encoded (distinct items mapped to indices)
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static bool CanEncode(this IHaveDistinctItemCount buffer) => buffer.DistinctItems.HasValue;

        /// <summary>
        /// Encoding a composite buffer maps each item to an index and returns both the mapping table and a new composite buffer of the indices
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="buffer"></param>
        /// <param name="tempStreams"></param>
        /// <param name="blockSize"></param>
        /// <param name="maxBlockSize"></param>
        /// <param name="maxInMemoryBlocks"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static (T[] Table, ICompositeBuffer<uint> Data) Encode<T>(
            this ICompositeBuffer<T> buffer, 
            IProvideByteBlocks? tempStreams = null, 
            int blockSize = Consts.DefaultInitialBlockSize, 
            int maxBlockSize = Consts.DefaultMaxBlockSize,
            uint? maxInMemoryBlocks = null
        ) where T : notnull {
            if(!buffer.DistinctItems.HasValue)
                throw new ArgumentException("Buffer cannot be encoded as the number of distinct items is not known - create the composite buffer with a high max distinct items", nameof(buffer));
            var table = new Dictionary<T, uint>((int)buffer.DistinctItems.Value);
            var data = new UnmanagedCompositeBuffer<uint>(tempStreams, blockSize, maxBlockSize, maxInMemoryBlocks);

            buffer.ForEachBlock(block => {
                var len = block.Length;
                if (len == 1) {
                    var item = block[0];
                    if(!table.TryGetValue(item, out var index))
                        table.Add(item, index = (uint)table.Count);
                    data.Append(index);
                }
                else if (len > 1) {
                    var spanOwner = SpanOwner<uint>.Empty;
                    var indices = len <= Consts.MaxStackAllocSizeInBytes / sizeof(uint)
                        ? stackalloc uint[len]
                        : (spanOwner = SpanOwner<uint>.Allocate(len)).Span
                    ;
                    try {
                        // encode the block
                        for(var i = 0; i < len; i++) {
                            ref readonly var item = ref block[i];
                            if(!table.TryGetValue(item, out var index))
                                table.Add(item, index = (uint)table.Count);
                            indices[i] = index;
                        }
                        data.Append(indices);
                    }
                    finally {
                        if (spanOwner.Length > 0)
                            spanOwner.Dispose();
                    }
                }
            });

            var ret = new T[table.Count];
            foreach (var item in table)
                ret[item.Value] = item.Key;
            return (ret, data);
        }

        /// <summary>
        /// Creates a composite buffer for the type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="tempStreams"></param>
        /// <param name="blockSize"></param>
        /// <param name="maxBlockSize"></param>
        /// <param name="maxInMemoryBlocks"></param>
        /// <param name="maxDistinctItems"></param>
        /// <returns></returns>
        public static ICompositeBuffer CreateCompositeBuffer(this Type type,
            IProvideByteBlocks? tempStreams = null, 
            int blockSize = Consts.DefaultInitialBlockSize, 
            int maxBlockSize = Consts.DefaultMaxBlockSize,
            uint? maxInMemoryBlocks = null,
            uint? maxDistinctItems = null
        ) => CreateCompositeBuffer(GetBrightDataType(type), tempStreams, blockSize, maxBlockSize, maxInMemoryBlocks, maxDistinctItems);

        /// <summary>
        /// Creates a column analyser
        /// </summary>
        /// <param name="buffer">Buffer to analyse</param>
        /// <param name="metaData"></param>
        /// <param name="maxMetaDataWriteCount">Maximum count to write to meta data</param>
        /// <returns></returns>
        public static IDataAnalyser GetAnalyser(this IReadOnlyBuffer buffer, MetaData metaData, uint maxMetaDataWriteCount = Consts.MaxMetaDataWriteCount)
        {
            return buffer.DataType.GetBrightDataType().GetAnalyser(metaData, maxMetaDataWriteCount);
        }

        /// <summary>
        /// Analyses the buffer
        /// </summary>
        /// <param name="metaData"></param>
        /// <param name="force"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static IOperation Analyse(this MetaData metaData, bool force, IReadOnlyBuffer buffer)
        {
            if (force || !metaData.Get(Consts.HasBeenAnalysed, false)) {
                var analyser = buffer.GetAnalyser(metaData);
                void WriteToMetaData() => analyser.WriteTo(metaData);
                return buffer.CreateBufferCopyOperation(analyser, WriteToMetaData);
            }
            return new NopOperation();
        }

        /// <summary>
        /// Creates an operation that copies the blocks in the buffer to a destination 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="output"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static IOperation CreateBufferCopyOperation(this IReadOnlyBuffer buffer, IAppendBlocks output, Action? action = null)
        {
            return buffer.DataType.GetBrightDataType() switch {
                BrightDataType.IndexList         => CastBuffer<IndexList, IHaveIndices>(buffer, output, action),
                BrightDataType.WeightedIndexList => CastBuffer<WeightedIndexList, IHaveIndices>(buffer, output, action),
                BrightDataType.Vector            => CastBuffer<ReadOnlyVector<float>, IReadOnlyTensor<float>>(buffer, output, action),
                BrightDataType.Matrix            => CastBuffer<ReadOnlyMatrix<float>, IReadOnlyTensor<float>>(buffer, output, action),
                BrightDataType.Tensor3D          => CastBuffer<ReadOnlyTensor3D<float>, IReadOnlyTensor<float>>(buffer, output, action),
                BrightDataType.Tensor4D          => CastBuffer<ReadOnlyTensor4D<float>, IReadOnlyTensor<float>>(buffer, output, action),
                _                                => GenericTypeMapping.BufferCopyOperation(buffer, output, action)
            };

            static BufferCopyOperation<CT2> CastBuffer<T2, CT2>(IReadOnlyBuffer buffer, IAppendBlocks analyser, Action? action = null) 
                where T2 : notnull 
                where CT2 : notnull
            {
                var buffer2 = (IReadOnlyBuffer<T2>)buffer;
                var buffer3 = buffer2.Cast<T2, CT2>();
                var dataAnalyser2 = (IAppendBlocks<CT2>)analyser;
                return new BufferCopyOperation<CT2>(buffer3, dataAnalyser2, action);
            }
        }

        /// <summary>
        /// Analyse the buffer
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="force"></param>
        /// <returns></returns>
        public static IOperation Analyse(this IReadOnlyBufferWithMetaData buffer, bool force) => Analyse(buffer.MetaData, force, buffer);

        /// <summary>
        /// Creates a numeric composite buffer from an existing buffer
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public static async Task<IReadOnlyBuffer> ToNumeric(this IReadOnlyBuffer buffer) 
        {
            if(Type.GetTypeCode(buffer.DataType) is TypeCode.DBNull or TypeCode.Empty or TypeCode.Object)
                throw new NotSupportedException();

            // convert from strings
            if (buffer.DataType == typeof(string))
                buffer = buffer.ConvertTo<double>();

            var analysis = GenericTypeMapping.SimpleNumericAnalysis(buffer);
            await analysis.Execute();

            BrightDataType toType;
            if (analysis.IsInteger) {
                toType = analysis switch 
                {
                    { MinValue: >= sbyte.MinValue, MaxValue: <= sbyte.MaxValue } => BrightDataType.SByte,
                    { MinValue: >= short.MinValue, MaxValue: <= short.MaxValue } => BrightDataType.Short,
                    { MinValue: >= int.MinValue, MaxValue: <= int.MaxValue } => BrightDataType.Int,
                    _ => BrightDataType.Long
                };
            } else {
                toType = analysis is { MinValue: >= float.MinValue, MaxValue: <= float.MaxValue } 
                    ? BrightDataType.Float 
                    : BrightDataType.Double;
            }

            return buffer.ConvertUnmanagedTo(toType.GetDataType());
        }

        static readonly HashSet<string> TrueStrings = ["Y", "YES", "TRUE", "T", "1"];

        /// <summary>
        /// Creates a boolean read only buffer from this buffer
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static IReadOnlyBuffer<bool> ToBoolean(this IReadOnlyBuffer buffer) {
            if (buffer.DataType == typeof(bool))
                return (IReadOnlyBuffer<bool>)buffer;
            if (buffer.DataType == typeof(string)) {
                var stringBuffer = buffer.ToStringBuffer();
                return stringBuffer.Map(StringToBool);
            }
            return (IReadOnlyBuffer<bool>)buffer.ConvertUnmanagedTo(typeof(bool));

            static bool StringToBool(string str) => TrueStrings.Contains(str.ToUpperInvariant());
        }

        /// <summary>
        /// Creates a date time read o
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static IReadOnlyBuffer<DateTime> ToDateTime(this IReadOnlyBuffer buffer) {
            if (buffer.DataType == typeof(DateTime))
                return (IReadOnlyBuffer<DateTime>)buffer;
            if (buffer.DataType == typeof(DateOnly))
                return ((IReadOnlyBuffer<DateOnly>)buffer).Map(DateOnlyToDate);
            if (buffer.DataType == typeof(string))
                return buffer.ToStringBuffer().Map(StringToDate);
            return (IReadOnlyBuffer<DateTime>)buffer.ConvertUnmanagedTo(typeof(DateTime));

            static DateTime StringToDate(string str)
            {
                try {
                    return str.ToDateTime();
                }
                catch {
                    // return placeholder date
                    return DateTime.MinValue;
                }
            }

            static DateTime DateOnlyToDate(DateOnly date) => date.ToDateTime(new TimeOnly());
        }

        /// <summary>
        /// Creates a date composite buffer from an existing buffer
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static IReadOnlyBuffer<DateOnly> ToDate(this IReadOnlyBuffer buffer)
        {
            if (buffer.DataType == typeof(DateOnly))
                return (IReadOnlyBuffer<DateOnly>)buffer;
            if (buffer.DataType == typeof(DateTime))
                return ((IReadOnlyBuffer<DateTime>)buffer).Map(DateTimeToDate);
            if (buffer.DataType == typeof(string))
                return buffer.ToStringBuffer().Map(StringToDate);
            return (IReadOnlyBuffer<DateOnly>)buffer.ConvertUnmanagedTo(typeof(DateOnly));

            static DateOnly DateTimeToDate(DateTime date) => new(date.Year, date.Month, date.Day);
            static DateOnly StringToDate(string str)
            {
                try {
                    return DateOnly.Parse(str);
                }
                catch {
                    // return placeholder date
                    return DateOnly.MinValue;
                }
            }
        }

        /// <summary>
        /// Creates a time composite buffer from an existing buffer
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static IReadOnlyBuffer<TimeOnly> ToTime(this IReadOnlyBuffer buffer)
        {
            if (buffer.DataType == typeof(TimeOnly))
                return (IReadOnlyBuffer<TimeOnly>)buffer;
            if (buffer.DataType == typeof(DateTime))
                return ((IReadOnlyBuffer<DateTime>)buffer).Map(DateTimeToTime);
            if (buffer.DataType == typeof(string))
                buffer.ToStringBuffer().Map(StringToTime);
            return (IReadOnlyBuffer<TimeOnly>)buffer.ConvertUnmanagedTo(typeof(TimeOnly));

            static TimeOnly DateTimeToTime(DateTime date) => new(date.Hour, date.Minute, date.Second, date.Millisecond);
            static TimeOnly StringToTime(string str)
            {
                try {
                    return TimeOnly.Parse(str);
                }
                catch {
                    // return placeholder date
                    return TimeOnly.MinValue;
                }
            }
        }

        /// <summary>
        /// Creates a categorical index composite buffer from an existing buffer
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="tempStreams"></param>
        /// <param name="blockSize"></param>
        /// <param name="maxBlockSize"></param>
        /// <param name="maxInMemoryBlocks"></param>
        /// <param name="maxDistinctItems"></param>
        /// <returns></returns>
        public static async Task<ICompositeBuffer<int>> ToCategoricalIndex(this IReadOnlyBuffer buffer,
            IProvideByteBlocks? tempStreams = null, 
            int blockSize = Consts.DefaultInitialBlockSize, 
            int maxBlockSize = Consts.DefaultMaxBlockSize,
            uint? maxInMemoryBlocks = null,
            uint? maxDistinctItems = null
        ) {
            
            var output = CreateCompositeBuffer<int>(tempStreams, blockSize, maxBlockSize, maxInMemoryBlocks, maxDistinctItems);
            var index = GenericTypeMapping.TypedIndexer(buffer);
            await index.Execute();
            var indexer = (ICanIndex)index;
            var mapping = indexer.GetMapping();

            var metaData = output.MetaData;
            metaData.SetIsCategorical(true);
            foreach (var category in mapping.OrderBy(d => d.Value))
                metaData.Set(Consts.CategoryPrefix + category.Value, category.Key);

            var categories = GenericTypeMapping.CategoricalIndexConverter(buffer, indexer);
            var conversion = GenericTypeMapping.BufferCopyOperation(categories, output);
            await conversion.Execute();
            return output;
        }

        /// <summary>
        /// Creates an index list composite buffer from an existing buffer
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="tempStreams"></param>
        /// <param name="blockSize"></param>
        /// <param name="maxBlockSize"></param>
        /// <param name="maxInMemoryBlocks"></param>
        /// <param name="maxDistinctItems"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public static async Task<ICompositeBuffer<IndexList>> ToIndexList(this IReadOnlyBuffer buffer,
            IProvideByteBlocks? tempStreams = null, 
            int blockSize = Consts.DefaultInitialBlockSize, 
            int maxBlockSize = Consts.DefaultMaxBlockSize,
            uint? maxInMemoryBlocks = null,
            uint? maxDistinctItems = null
        ) {
            var output = CreateCompositeBuffer<IndexList>(tempStreams, x => new(x), blockSize, maxBlockSize, maxInMemoryBlocks, maxDistinctItems);
            IOperation conversion;
            if (buffer.DataType == typeof(IndexList))
                conversion = new NopConversion<IndexList>((IReadOnlyBuffer<IndexList>)buffer, output);
            else if (buffer.DataType == typeof(WeightedIndexList))
                conversion = new CustomConversion<WeightedIndexList, IndexList>(WeightedIndexListToIndexList, (IReadOnlyBuffer<WeightedIndexList>)buffer, output);
            else if (buffer.DataType == typeof(ReadOnlyVector<float>))
                conversion = new CustomConversion<ReadOnlyVector<float>, IndexList>(VectorToIndexList, (IReadOnlyBuffer<ReadOnlyVector<float>>)buffer, output);
            else
                throw new NotSupportedException("Only weighted index lists and vectors can be converted to index lists");
            await conversion.Execute();
            return output;

            static IndexList VectorToIndexList(ReadOnlyVector<float> vector) => vector.ReadOnlySegment.ToSparse().AsIndexList();
            static IndexList WeightedIndexListToIndexList(WeightedIndexList weightedIndexList) => weightedIndexList.AsIndexList();
        }

        /// <summary>
        /// Creates a vector composite buffer from an existing buffer
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="tempStreams"></param>
        /// <param name="blockSize"></param>
        /// <param name="maxBlockSize"></param>
        /// <param name="maxInMemoryBlocks"></param>
        /// <param name="maxDistinctItems"></param>
        /// <returns></returns>
        public static async Task<ICompositeBuffer<ReadOnlyVector<float>>> ToVector(this IReadOnlyBuffer buffer,
            IProvideByteBlocks? tempStreams = null, 
            int blockSize = Consts.DefaultInitialBlockSize, 
            int maxBlockSize = Consts.DefaultMaxBlockSize,
            uint? maxInMemoryBlocks = null,
            uint? maxDistinctItems = null
        ) {
            var output = CreateCompositeBuffer<ReadOnlyVector<float>>(tempStreams, x => new(x), blockSize, maxBlockSize, maxInMemoryBlocks, maxDistinctItems);
            IOperation conversion;
            if (buffer.DataType == typeof(ReadOnlyVector<float>))
                conversion = new NopConversion<ReadOnlyVector<float>>((IReadOnlyBuffer<ReadOnlyVector<float>>)buffer, output);
            else if (buffer.DataType == typeof(WeightedIndexList))
                conversion = new CustomConversion<WeightedIndexList, ReadOnlyVector<float>>(WeightedIndexListToVector, (IReadOnlyBuffer<WeightedIndexList>)buffer, output);
            else if (buffer.DataType == typeof(IndexList))
                conversion = new CustomConversion<IndexList, ReadOnlyVector<float>>(IndexListToVector, (IReadOnlyBuffer<IndexList>)buffer, output);
            else {
                var index = GenericTypeMapping.TypedIndexer(buffer);
                await index.Execute();
                var indexer = (ICanIndex)index;
                var vectorBuffer = GenericTypeMapping.OneHotConverter(buffer, indexer);
                conversion = GenericTypeMapping.BufferCopyOperation(vectorBuffer, output);
                var categoryIndex = indexer.GetMapping();

                var metaData = output.MetaData;
                metaData.SetIsOneHot(true);
                foreach (var category in categoryIndex)
                    metaData.Set(Consts.CategoryPrefix + category.Value, category.Key);
            }
            await conversion.Execute();
            return output;

            static ReadOnlyVector<float> WeightedIndexListToVector(WeightedIndexList weightedIndexList) => weightedIndexList.AsDense();
            static ReadOnlyVector<float> IndexListToVector(IndexList indexList) => indexList.AsDense();
        }

        /// <summary>
        /// Creates a weighted index list composite buffer from an existing buffer
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="tempStreams"></param>
        /// <param name="blockSize"></param>
        /// <param name="maxBlockSize"></param>
        /// <param name="maxInMemoryBlocks"></param>
        /// <param name="maxDistinctItems"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public static async Task<ICompositeBuffer<WeightedIndexList>> ToWeightedIndexList(this IReadOnlyBuffer buffer,
            IProvideByteBlocks? tempStreams = null, 
            int blockSize = Consts.DefaultInitialBlockSize, 
            int maxBlockSize = Consts.DefaultMaxBlockSize,
            uint? maxInMemoryBlocks = null,
            uint? maxDistinctItems = null
        ) {
            var output = CreateCompositeBuffer<WeightedIndexList>(tempStreams, x => new(x), blockSize, maxBlockSize, maxInMemoryBlocks, maxDistinctItems);
            IOperation conversion;
            if (buffer.DataType == typeof(WeightedIndexList))
                conversion = new NopConversion<WeightedIndexList>((IReadOnlyBuffer<WeightedIndexList>)buffer, output);
            else if (buffer.DataType == typeof(ReadOnlyVector<float>))
                conversion = new CustomConversion<ReadOnlyVector<float>, WeightedIndexList>(VectorToWeightedIndexList, (IReadOnlyBuffer<ReadOnlyVector<float>>)buffer, output);
            else if (buffer.DataType == typeof(IndexList))
                conversion = new CustomConversion<IndexList, WeightedIndexList>(IndexListToWeightedIndexList, (IReadOnlyBuffer<IndexList>)buffer, output);
            else
                throw new NotSupportedException("Only weighted index lists, index lists and vectors can be converted to vectors");
            await conversion.Execute();
            return output;

            static WeightedIndexList IndexListToWeightedIndexList(IndexList indexList) => indexList.AsWeightedIndexList();
            static WeightedIndexList VectorToWeightedIndexList(ReadOnlyVector<float> vector) => vector.ToSparse();
        }

        /// <summary>
        /// Converts a buffer to an unmanaged type
        /// </summary>
        /// <param name="buffer"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IReadOnlyBuffer<T> To<T>(this IReadOnlyBuffer buffer) where T: unmanaged
        {
            // convert from strings
            if (buffer.DataType == typeof(string))
                buffer = buffer.ConvertTo<double>();

            return (IReadOnlyBuffer<T>)buffer.ConvertUnmanagedTo(typeof(T));
        }

        /// <summary>
        /// Vectorise the buffers
        /// </summary>
        /// <param name="buffers"></param>
        /// <param name="tempStreams"></param>
        /// <param name="blockSize"></param>
        /// <param name="maxBlockSize"></param>
        /// <param name="maxInMemoryBlocks"></param>
        /// <param name="maxDistinctItems"></param>
        /// <returns></returns>
        public static async Task<ICompositeBuffer<ReadOnlyVector<float>>> Vectorise(this IReadOnlyBuffer[] buffers,
            IProvideByteBlocks? tempStreams = null,
            int blockSize = Consts.DefaultInitialBlockSize, 
            int maxBlockSize = Consts.DefaultMaxBlockSize,
            uint? maxInMemoryBlocks = null,
            uint? maxDistinctItems = null
        ) {
            var output = CreateCompositeBuffer<ReadOnlyVector<float>>(tempStreams, x => new(x), blockSize, maxBlockSize, maxInMemoryBlocks, maxDistinctItems);
            var floatBuffers = buffers.Select(x => x.ConvertTo<float>());
            var conversion = new ManyToOneMutation<float, ReadOnlyVector<float>>(floatBuffers, output, x => new(x));
            await conversion.Execute();
            return output;
        }

        /// <summary>
        /// Creates a read only string composite buffer from a stream
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static IReadOnlyBuffer<string> GetReadOnlyStringCompositeBuffer(this Stream stream)
        {
            return new ReadOnlyStringCompositeBuffer(stream);
        }

        /// <summary>
        /// Creates a read only composite buffer for unmanaged types from a stream
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static IReadOnlyBuffer<T> GetReadOnlyCompositeBuffer<T>(this Stream stream) where T: unmanaged
        {
            return new ReadOnlyUnmanagedCompositeBuffer<T>(stream);
        }

        /// <summary>
        /// Creates a read only composite buffer for types that can be initialised from a byte block from a stream
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <param name="createItem"></param>
        /// <returns></returns>
        public static IReadOnlyBuffer<T> GetReadOnlyCompositeBuffer<T>(this Stream stream, CreateFromReadOnlyByteSpan<T> createItem) where T : IHaveDataAsReadOnlyByteSpan
        {
            return new ReadOnlyManagedCompositeBuffer<T>(createItem, stream);
        }

        /// <summary>
        /// Casts a buffer to another type
        /// </summary>
        /// <typeparam name="FT"></typeparam>
        /// <typeparam name="TT"></typeparam>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static IReadOnlyBuffer<TT> Cast<FT, TT>(this IReadOnlyBuffer<FT> buffer) where FT : notnull where TT : notnull
        {
            return new CastConverter<FT, TT>(buffer);
        }

        /// <summary>
        /// Converts the buffer that contains an unmanaged type to the specified type
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IReadOnlyBuffer ConvertUnmanagedTo(this IReadOnlyBuffer buffer, Type type)
        {
            var converter = StaticConverters.GetConverter(buffer.DataType, type);
            return GenericTypeMapping.TypeConverter(type, buffer, converter);
        }

        /// <summary>
        /// Returns the block index and relative block index (index within each block) of each item in the buffer
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static IEnumerable<(uint BlockIndex, uint RelativeBlockIndex)> AllIndices(this IReadOnlyBuffer buffer)
        {
            uint blockIndex = 0;
            foreach (var block in buffer.BlockSizes) {
                for(uint i = 0; i < block; i++)
                    yield return (blockIndex, i);
                ++blockIndex;
            }
        }

        /// <summary>
        /// Returns all indices in the buffer, including their block index and relative block index (index within each block)
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="rowIndices">Row indices to select (or all if non specified)</param>
        /// <returns></returns>
        public static IEnumerable<(uint RowIndex, uint BlockIndex, uint RelativeBlockIndex, uint SourceIndex)> GetIndices(this IReadOnlyBuffer buffer, params uint[] rowIndices)
        {
            if (rowIndices.Length == 0) {
                uint absoluteIndex = 0;
                foreach (var (blockIndex, relativeBlockIndex) in AllIndices(buffer))
                    yield return (absoluteIndex++, blockIndex, relativeBlockIndex, absoluteIndex++);
            }
            else {
                foreach(var item in GetIndices(buffer, (IEnumerable<uint>)rowIndices))
                    yield return item;
            }
        }

        /// <summary>
        /// Returns all indices in the buffer, including their block index and relative block index (index within each block)
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="rowIndices">Row indices to select (or all if non specified)</param>
        /// <returns></returns>
        public static IEnumerable<(uint RowIndex, uint BlockIndex, uint RelativeBlockIndex, uint SourceIndex)> GetIndices(this IReadOnlyBuffer buffer, IEnumerable<uint> rowIndices)
        {
            var blockIndices = AllIndices(buffer).ToArray();
            var sourceIndex = 0U;
            foreach (var index in rowIndices)
                yield return (index, blockIndices[index].BlockIndex, blockIndices[index].RelativeBlockIndex, sourceIndex++);
        }

        /// <summary>
        /// Converts the buffer to a buffer of objects
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static IReadOnlyBuffer<object> ToObjectBuffer(this IReadOnlyBuffer buffer)
        {
            return GenericTypeMapping.ToObjectBuffer(buffer);
        }

        /// <summary>
        /// Converts the buffer to a buffer of strings
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static IReadOnlyBuffer<string> ToStringBuffer(this IReadOnlyBuffer buffer)
        {
            if (buffer.DataType == typeof(string))
                return (IReadOnlyBuffer<string>)buffer;
            return GenericTypeMapping.ToStringBuffer(buffer);
        }

        /// <summary>
        /// Converts the buffer to another type via a converter
        /// </summary>
        /// <param name="buffer">Buffer to convert</param>
        /// <param name="converter">Converter</param>
        /// <returns></returns>
        public static IReadOnlyBuffer ConvertWith(this IReadOnlyBuffer buffer, ICanConvert converter)
        {
            return GenericTypeMapping.TypeConverter(converter.To, buffer, converter);
        }

        /// <summary>
        /// Notifies about progress on buffer for each
        /// </summary>
        /// <param name="buffer">Underlying buffer</param>
        /// <param name="notify">Progress notification</param>
        /// <param name="msg">Msg associated with operation (optional)</param>
        /// <param name="callback"></param>
        /// <param name="ct"></param>
        /// <typeparam name="T"></typeparam>
        public static async Task ForEachWithProgressNotification<T>(this IReadOnlyBuffer<T> buffer, INotifyOperationProgress notify, BlockCallback<T> callback, string? msg = null, CancellationToken ct = default)
            where T: notnull
        {
            var id = Guid.NewGuid();
            notify.OnStartOperation(id);
            var count = 0;
            await buffer.ForEachBlock(x => {
                callback(x);
                notify.OnOperationProgress(id, (float)++count / buffer.Size);
            }, ct);
            notify.OnCompleteOperation(id, ct.IsCancellationRequested);
        }

        /// <summary>
        /// Creates a vectoriser for the buffer
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="metaData"></param>
        /// <param name="oneHotEncodeCategoricalData"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        /// <exception cref="Exception"></exception>
        /// <exception cref="NotSupportedException"></exception>
        public static async Task<ICanVectorise> GetVectoriser(this IReadOnlyBuffer buffer, MetaData metaData, bool oneHotEncodeCategoricalData)
        {
            var dataType = buffer.DataType.GetBrightDataType();
            var cls = ColumnTypeClassifier.GetClass(dataType, metaData);
            ICanVectorise? ret;

            if (dataType == BrightDataType.Boolean)
                ret = new BooleanVectoriser();
            else if (cls.HasFlag(ColumnClass.Numeric))
                ret = GenericTypeMapping.NumericVectoriser(buffer.DataType);
            else if (cls.HasFlag(ColumnClass.Categorical)) {
                if (oneHotEncodeCategoricalData)
                    ret = await GetOneHotEncoder(buffer, metaData);
                else
                    ret = GenericTypeMapping.CategoricalIndexVectoriser(buffer.DataType);
            }
            else if (cls.HasFlag(ColumnClass.IndexBased))
                ret = await GetIndexBasedVectoriser(buffer, metaData);
            else if (cls.HasFlag(ColumnClass.Tensor))
                ret = await GetTensorVectoriser(buffer, metaData);
            else
                throw new NotImplementedException();
            ret.ReadFrom(metaData);
            return ret;

            static async Task<ICanVectorise> GetIndexBasedVectoriser(IReadOnlyBuffer buffer, MetaData metaData)
            {
                var size = metaData.Get<uint>(Consts.VectorisationSize, 0);
                if (size == 0) {
                    await metaData.Analyse(false, buffer).Execute();
                    size = metaData.GetIndexAnalysis().MaxIndex ?? 0;
                    if (size == 0)
                        throw new Exception("Expected to find a max index size");
                }

                if(buffer.DataType == typeof(IndexList))
                    return GenericActivator.Create<ICanVectorise>(typeof(IndexListVectoriser), size);
                if(buffer.DataType == typeof(WeightedIndexList))
                    return GenericActivator.Create<ICanVectorise>(typeof(WeightedIndexListVectoriser), size);
                throw new NotSupportedException();
            }

            static async Task<ICanVectorise> GetOneHotEncoder(IReadOnlyBuffer buffer, MetaData metaData)
            {
                var size = metaData.Get<uint>(Consts.VectorisationSize, 0);
                if (size == 0) {
                    await metaData.Analyse(false, buffer).Execute();
                    size = metaData.Get<uint>(Consts.NumDistinct, 0);
                    if (size == 0)
                        throw new Exception("Expected to find a distinct size of items");
                }

                return GenericTypeMapping.OneHotVectoriser(buffer.DataType, size);
            }

            static async Task<ICanVectorise> GetTensorVectoriser(IReadOnlyBuffer buffer, MetaData metaData)
            {
                var size = metaData.Get<uint>(Consts.VectorisationSize, 0);
                if (size == 0) {
                    await metaData.Analyse(false, buffer).Execute();
                    size = metaData.GetDimensionAnalysis().Size;
                    if (size == 0)
                        throw new Exception("Expected to find non empty tensors");
                }

                return CreateTensorVectoriser(buffer.DataType, size);
            }
        }

        /// <summary>
        /// Creates a vectoriser from multiple buffers
        /// </summary>
        /// <param name="buffers"></param>
        /// <param name="oneHotEncodeCategoricalData"></param>
        /// <returns></returns>
        public static async Task<VectorisationModel> GetVectoriser(this IReadOnlyBufferWithMetaData[] buffers, bool oneHotEncodeCategoricalData)
        {
            var createTasks = buffers.Select(x => GetVectoriser(x, x.MetaData, oneHotEncodeCategoricalData)).ToArray();
            await Task.WhenAll(createTasks);
            var vectorisers = createTasks.Select(x => x.Result).ToArray();
            return new VectorisationModel(vectorisers);
        }

        public static Task WriteTo(this IHaveMemory<byte> itemWithMemory, string filePath)
        {
            return File.WriteAllBytesAsync(filePath, itemWithMemory.ReadOnlyMemory);
        }
    }
}
