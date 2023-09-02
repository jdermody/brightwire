using System;
using System.Buffers;
using System.Reflection;
using System.Runtime.CompilerServices;
using BrightData.Analysis;
using BrightData.Converter;
using BrightData.Helper;
using BrightData.LinearAlgebra.ReadOnly;
using BrightData.Table.Buffer.Composite;
using BrightData.Table.Buffer.Helper;
using BrightData.Table.ByteBlockReaders;
using BrightData.Table.Helper;
using BrightData.Table.Operation;
using BrightData.Table.Operation.Conversion;
using BrightData.Table.Operation.Helper;
using CommunityToolkit.HighPerformance.Buffers;
using Microsoft.VisualBasic;

namespace BrightData.Table
{
    public static class ExtensionMethods
    {
        public static async Task<IDataTable> CreateTableInMemory(
            this BrightDataContext _,
            params ICompositeBuffer[] buffers
        )
        {
            var builder = new ColumnOrientedTableBuilder();
            var ret = new MemoryStream();
            await builder.Write(new(), buffers, ret);
            var memory = new Memory<byte>(ret.GetBuffer(), 0, (int)ret.Length);
            return new ColumnOriented(new MemoryByteBlockReader(memory, ret));
        }

        public static ICompositeBuffer<string> CreateCompositeBuffer(
            IProvideTempData? tempStreams = null, 
            int blockSize = Consts.DefaultBlockSize, 
            uint? maxInMemoryBlocks = null,
            uint? maxDistinctItems = null
        ) => new StringCompositeBuffer(tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems);

        public static ICompositeBuffer<T> CreateCompositeBuffer<T>(
            CreateFromReadOnlyByteSpan<T> createItem,
            IProvideTempData? tempStreams = null, 
            int blockSize = Consts.DefaultBlockSize, 
            uint? maxInMemoryBlocks = null,
            uint? maxDistinctItems = null
        ) where T: IHaveDataAsReadOnlyByteSpan => new ManagedCompositeBuffer<T>(createItem, tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems);

        public static ICompositeBuffer<T> CreateCompositeBuffer<T>(
            IProvideTempData? tempStreams = null, 
            int blockSize = Consts.DefaultBlockSize, 
            uint? maxInMemoryBlocks = null,
            uint? maxDistinctItems = null
        ) where T: unmanaged => new UnmanagedCompositeBuffer<T>(tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems);

        public static IBufferWriter<T> AsBufferWriter<T>(this ICompositeBuffer<T> buffer, int bufferSize = 256) where T : notnull => new CompositeBufferWriter<T>(buffer, bufferSize);

        public static bool CanEncode<T>(this ICompositeBuffer<T> buffer) where T : notnull => buffer.DistinctItems.HasValue;

        /// <summary>
        /// Encoding a composite buffer maps each item to an index and returns both the mapping table and a new composite buffer of the indices
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="buffer"></param>
        /// <param name="tempStreams"></param>
        /// <param name="blockSize"></param>
        /// <param name="maxInMemoryBlocks"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static (T[] Table, ICompositeBuffer<uint> Data) Encode<T>(
            this ICompositeBuffer<T> buffer, 
            IProvideTempData? tempStreams = null, 
            int blockSize = Consts.DefaultBlockSize, 
            uint? maxInMemoryBlocks = null
        ) where T : notnull {
            if(!buffer.DistinctItems.HasValue)
                throw new ArgumentException("Buffer cannot be encoded as the number of distinct items is not known - create the composite buffer with a high max distinct items", nameof(buffer));
            var table = new Dictionary<T, uint>((int)buffer.DistinctItems.Value);
            var data = new UnmanagedCompositeBuffer<uint>(tempStreams, blockSize, maxInMemoryBlocks);

            buffer.ForEachBlock(block => {
                var len = block.Length;
                if (len == 1) {
                    var item = block[0];
                    if(!table.TryGetValue(item, out var index))
                        table.Add(item, index = (uint)table.Count);
                    data.Add(index);
                }
                else if (len > 1) {
                    var spanOwner = SpanOwner<uint>.Empty;
                    var indices = len <= Consts.MaxStackAllocSize / sizeof(uint)
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
                        data.Add(indices);
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

        public static T[] ToArray<T>(this ICompositeBuffer<T> buffer) where T : notnull
        {
            var ret = new T[buffer.Size];
            var offset = 0;
            buffer.ForEachBlock(x => {
                x.CopyTo(new Span<T>(ret, offset, x.Length));
                offset += x.Length;
            });
            return ret;
        }

        public ref struct CompositeBufferIterator<T> where T: notnull
        {
            readonly ICompositeBuffer<T> _buffer;
            ReadOnlyMemory<T> _currentBlock = ReadOnlyMemory<T>.Empty;
            uint _blockIndex = 0, _position = 0;

            public CompositeBufferIterator(ICompositeBuffer<T> buffer) => _buffer = buffer;

            public bool MoveNext()
            {
                if (++_position < _currentBlock.Length)
                    return true;

                while(_blockIndex < _buffer.BlockCount) {
                    _currentBlock = _buffer.GetBlock(_blockIndex++).Result;
                    if (_currentBlock.Length > 0) {
                        _position = 0;
                        return true;
                    }
                }
                return false;
            }

            public readonly ref readonly T Current => ref _currentBlock.Span[(int)_position];
            public readonly CompositeBufferIterator<T> GetEnumerator() => this;
        }
        public static CompositeBufferIterator<T> GetEnumerator<T>(this ICompositeBuffer<T> buffer) where T: notnull => new(buffer);

        static (Type, uint) GetTypeAndSize<T>() => (typeof(T), (uint)Unsafe.SizeOf<T>());

        /// <summary>
        /// Returns the .net type and its size to represent a bright data type within a column
        /// </summary>
        /// <param name="dataType">Bright data type</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static (Type Type, uint Size) GetColumnType(this BrightDataType dataType) => dataType switch 
        {
            BrightDataType.BinaryData        => GetTypeAndSize<DataRangeColumnType>(),
            BrightDataType.Boolean           => GetTypeAndSize<bool>(),
            BrightDataType.SByte             => GetTypeAndSize<sbyte>(),
            BrightDataType.Short             => GetTypeAndSize<short>(),
            BrightDataType.Int               => GetTypeAndSize<int>(),
            BrightDataType.Long              => GetTypeAndSize<long>(),
            BrightDataType.Float             => GetTypeAndSize<float>(),
            BrightDataType.Double            => GetTypeAndSize<double>(),
            BrightDataType.Decimal           => GetTypeAndSize<decimal>(),
            BrightDataType.String            => GetTypeAndSize<uint>(),
            BrightDataType.Date              => GetTypeAndSize<DateTime>(),
            BrightDataType.IndexList         => GetTypeAndSize<DataRangeColumnType>(),
            BrightDataType.WeightedIndexList => GetTypeAndSize<DataRangeColumnType>(),
            BrightDataType.Vector            => GetTypeAndSize<DataRangeColumnType>(),
            BrightDataType.Matrix            => GetTypeAndSize<MatrixColumnType>(),
            BrightDataType.Tensor3D          => GetTypeAndSize<Tensor3DColumnType>(),
            BrightDataType.Tensor4D          => GetTypeAndSize<Tensor4DColumnType>(),
            BrightDataType.TimeOnly          => GetTypeAndSize<TimeOnly>(),
            BrightDataType.DateOnly          => GetTypeAndSize<DateOnly>(),
            _                                => throw new ArgumentOutOfRangeException(nameof(dataType), dataType, null)
        };

        /// <summary>
        /// Converts from a Type to a ColumnType
        /// </summary>
        /// <param name="dataType"></param>
        /// <returns></returns>
        public static BrightDataType GetTableDataType(this Type dataType)
        {
            var typeCode = Type.GetTypeCode(dataType);
            if (typeCode == TypeCode.Boolean)
                return BrightDataType.Boolean;

            if (typeCode == TypeCode.SByte)
                return BrightDataType.SByte;

            if (typeCode == TypeCode.DateTime)
                return BrightDataType.Date;

            if (typeCode == TypeCode.Double)
                return BrightDataType.Double;

            if (typeCode == TypeCode.Decimal)
                return BrightDataType.Decimal;

            if (typeCode == TypeCode.Single)
                return BrightDataType.Float;

            if (typeCode == TypeCode.Int16)
                return BrightDataType.Short;

            if (typeCode == TypeCode.Int32)
                return BrightDataType.Int;

            if (typeCode == TypeCode.Int64)
                return BrightDataType.Long;

            if (typeCode == TypeCode.String)
                return BrightDataType.String;

            if (dataType == typeof(IndexList))
                return BrightDataType.IndexList;

            if (dataType == typeof(WeightedIndexList))
                return BrightDataType.WeightedIndexList;

            if (dataType == typeof(IVectorData))
                return BrightDataType.Vector;

            if (dataType == typeof(IMatrixData))
                return BrightDataType.Matrix;

            if (dataType == typeof(ITensor3DData))
                return BrightDataType.Tensor3D;

            if (dataType == typeof(ITensor4DData))
                return BrightDataType.Tensor4D;

            if (dataType == typeof(BinaryData))
                return BrightDataType.BinaryData;

            throw new ArgumentException($"{dataType} has no corresponding table data type");
        }

        public static ICompositeBuffer GetCompositeBuffer(this Type type,
            IProvideTempData? tempStreams = null, 
            int blockSize = Consts.DefaultBlockSize, 
            uint? maxInMemoryBlocks = null,
            uint? maxDistinctItems = null
        ) => GetCompositeBuffer(GetTableDataType(type), tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems);

        public static ICompositeBuffer GetCompositeBuffer(this BrightDataType type,
            IProvideTempData? tempStreams = null,
            int blockSize = Consts.DefaultBlockSize,
            uint? maxInMemoryBlocks = null,
            uint? maxDistinctItems = null
        ) => type switch 
        {
            BrightDataType.BinaryData        => CreateCompositeBuffer<BinaryData>(x => new(x), tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems),
            BrightDataType.Boolean           => CreateCompositeBuffer<bool>(tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems),
            BrightDataType.SByte             => CreateCompositeBuffer<sbyte>(tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems),
            BrightDataType.Short             => CreateCompositeBuffer<short>(tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems),
            BrightDataType.Int               => CreateCompositeBuffer<int>(tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems),
            BrightDataType.Long              => CreateCompositeBuffer<long>(tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems),
            BrightDataType.Float             => CreateCompositeBuffer<float>(tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems),
            BrightDataType.Double            => CreateCompositeBuffer<double>(tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems),
            BrightDataType.Decimal           => CreateCompositeBuffer<decimal>(tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems),
            BrightDataType.String            => CreateCompositeBuffer<uint>(tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems),
            BrightDataType.Date              => CreateCompositeBuffer<DateTime>(tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems),
            BrightDataType.IndexList         => CreateCompositeBuffer<IndexList>(x => new(x), tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems),
            BrightDataType.WeightedIndexList => CreateCompositeBuffer<WeightedIndexList>(x => new(x), tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems),
            BrightDataType.Vector            => CreateCompositeBuffer<ReadOnlyVector>(x => new(x), tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems),
            BrightDataType.Matrix            => CreateCompositeBuffer<ReadOnlyMatrix>(x => new(x), tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems),
            BrightDataType.Tensor3D          => CreateCompositeBuffer<ReadOnlyTensor3D>(x => new(x), tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems),
            BrightDataType.Tensor4D          => CreateCompositeBuffer<ReadOnlyTensor4D>(x => new(x), tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems),
            BrightDataType.TimeOnly          => CreateCompositeBuffer<TimeOnly>(tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems),
            BrightDataType.DateOnly          => CreateCompositeBuffer<DateOnly>(tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems),
            _                                => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown table data type")
        };

        public static async Task<T> GetItem<T>(this IReadOnlyBuffer<T> buffer, uint index) where T: notnull
        {
            var blockIndex = index / buffer.BlockSize;
            var blockMemory = await buffer.GetBlock(blockIndex);
            return blockMemory.Span[(int)(index % buffer.BlockSize)];
        }

        public static async Task<T[]> GetItems<T>(this IReadOnlyBuffer<T> buffer, uint[] indices) where T: notnull
        {
            var blocks = indices.Select((x, i) => (Index: x, BlockIndex: x / buffer.BlockSize, SourceIndex: (uint)i))
                .GroupBy(x => x.BlockIndex)
                .OrderBy(x => x.Key)
            ;
            var ret = new T[indices.Length];
            foreach (var block in blocks) {
                var blockMemory = await buffer.GetBlock(block.Key);
                Add(blockMemory, block, ret);
            }
            return ret;

            static void Add(ReadOnlyMemory<T> data, IEnumerable<(uint Index, uint BlockIndex, uint SourceIndex)> list, T[] output)
            {
                var span = data.Span;
                foreach (var (index, _, sourceIndex) in list)
                    output[sourceIndex] = span[(int)index];
            }
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
        public static async Task<ReadOnlySequence<T>> AsReadOnlySequence<T>(this IReadOnlyBuffer<T> buffer) where T : notnull
        {
            if(buffer.BlockCount == 0)
                return ReadOnlySequence<T>.Empty;

            var first = new MemorySegment<T>(await buffer.GetBlock(0));
            var last = first;
            for(var i = 1; i < buffer.BlockCount; i++)
                last = last.Append(await buffer.GetBlock(1));
            return new ReadOnlySequence<T>(first, 0, last, last.Memory.Length);
        }

        /// <summary>
        /// Creates a column analyser
        /// </summary>
        /// <param name="buffer">Buffer to analyse</param>
        /// <param name="maxMetaDataWriteCount">Maximum count to write to meta data</param>
        /// <returns></returns>
        public static IDataAnalyser GetAnalyser(this IReadOnlyBuffer buffer, MetaData metaData, uint maxMetaDataWriteCount = Consts.MaxMetaDataWriteCount)
        {
            var dataType = buffer.DataType.GetBrightDataType();
            var columnType = ColumnTypeClassifier.GetClass(dataType, metaData);
            if (columnType.HasFlag(ColumnClass.Categorical)) {
                if (dataType == BrightDataType.String)
                    return StaticAnalysers.CreateStringAnalyser(maxMetaDataWriteCount);
                return StaticAnalysers.CreateFrequencyAnalyser(buffer.DataType, maxMetaDataWriteCount);
            }

            if (columnType.HasFlag(ColumnClass.IndexBased))
                return StaticAnalysers.CreateIndexAnalyser(maxMetaDataWriteCount);

            if (columnType.HasFlag(ColumnClass.Tensor))
                return StaticAnalysers.CreateDimensionAnalyser();

            return dataType switch
            {
                BrightDataType.Double     => StaticAnalysers.CreateNumericAnalyser(maxMetaDataWriteCount),
                BrightDataType.Float      => StaticAnalysers.CreateNumericAnalyser<float>(maxMetaDataWriteCount),
                BrightDataType.Decimal    => StaticAnalysers.CreateNumericAnalyser<decimal>(maxMetaDataWriteCount),
                BrightDataType.SByte      => StaticAnalysers.CreateNumericAnalyser<sbyte>(maxMetaDataWriteCount),
                BrightDataType.Int        => StaticAnalysers.CreateNumericAnalyser<int>(maxMetaDataWriteCount),
                BrightDataType.Long       => StaticAnalysers.CreateNumericAnalyser<long>(maxMetaDataWriteCount),
                BrightDataType.Short      => StaticAnalysers.CreateNumericAnalyser<short>(maxMetaDataWriteCount),
                BrightDataType.Date       => StaticAnalysers.CreateDateAnalyser(),
                BrightDataType.BinaryData => StaticAnalysers.CreateFrequencyAnalyser<BinaryData>(maxMetaDataWriteCount),
                BrightDataType.DateOnly   => StaticAnalysers.CreateFrequencyAnalyser<DateOnly>(maxMetaDataWriteCount),
                BrightDataType.TimeOnly   => StaticAnalysers.CreateFrequencyAnalyser<TimeOnly>(maxMetaDataWriteCount),
                _                         => throw new NotImplementedException()
            };
        }
        public static IOperation Analyse(this IReadOnlyBuffer buffer, MetaData metaData, bool force)
        {
            if (force || !metaData.Get(Consts.HasBeenAnalysed, false)) {
                var analyser = buffer.GetAnalyser(metaData);
                return GenericActivator.Create<IOperation>(typeof(BufferScan<>).MakeGenericType(buffer.DataType), buffer, analyser, () => analyser.WriteTo(metaData));
            }
            return new NopOperation();
        }
        
        public static async Task<ICompositeBuffer> ToNumeric(this IReadOnlyBuffer buffer, 
            IProvideTempData? tempStreams = null, 
            int blockSize = Consts.DefaultBlockSize, 
            uint? maxInMemoryBlocks = null,
            uint? maxDistinctItems = null
        ) {
            if(Type.GetTypeCode(buffer.DataType) is TypeCode.DBNull or TypeCode.Empty or TypeCode.Object)
                throw new NotSupportedException();

            var analysis = GenericActivator.Create<ICastToNumericAnalysis>(typeof(CastToNumericAnalysis<>).MakeGenericType(buffer.DataType), buffer);
            await analysis.Process();

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

            var output = GenericActivator.Create<ICompositeBuffer>(typeof(UnmanagedCompositeBuffer<>).MakeGenericType(toType.GetDataType()), tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems);
            var conversion = GenericActivator.Create<IOperation>(typeof(UnmanagedConversion<,>).MakeGenericType(buffer.DataType, toType.GetDataType()), buffer, output);
            await conversion.Process();
            return output;
        }

        static readonly HashSet<string> TrueStrings = new() { "Y", "YES", "TRUE", "T", "1" };
        public static async Task<ICompositeBuffer<bool>> ToBoolean(this IReadOnlyBuffer buffer,
            IProvideTempData? tempStreams = null, 
            int blockSize = Consts.DefaultBlockSize, 
            uint? maxInMemoryBlocks = null,
            uint? maxDistinctItems = null
            ) {
            var output = CreateCompositeBuffer<bool>(tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems);
            var conversion = (buffer.DataType == typeof(string))
                ? new CustomConversion<string, bool>(StringToBool, (IReadOnlyBuffer<string>)buffer, output)
                : GenericActivator.Create<IOperation>(typeof(UnmanagedConversion<,>).MakeGenericType(buffer.DataType, typeof(bool)), buffer, output)
            ;
            await conversion.Process();
            return output;
            static bool StringToBool(string str) => TrueStrings.Contains(str.ToUpperInvariant());
        }

        public static async Task<ICompositeBuffer<string>> ToString(this IReadOnlyBuffer buffer,
            IProvideTempData? tempStreams = null, 
            int blockSize = Consts.DefaultBlockSize, 
            uint? maxInMemoryBlocks = null,
            uint? maxDistinctItems = null
        ) {
            var output = CreateCompositeBuffer(tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems);
            var conversion = GenericActivator.Create<IOperation>(typeof(ToStringConversion<>).MakeGenericType(buffer.DataType), buffer, output);
            await conversion.Process();
            return output;
        }

        public static async Task<ICompositeBuffer<DateTime>> ToDateTime(this IReadOnlyBuffer buffer,
            IProvideTempData? tempStreams = null, 
            int blockSize = Consts.DefaultBlockSize, 
            uint? maxInMemoryBlocks = null,
            uint? maxDistinctItems = null
        ) {
            var output = CreateCompositeBuffer<DateTime>(tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems);
            var conversion = (buffer.DataType == typeof(string))
                ? new CustomConversion<string, DateTime>(StringToDate, (IReadOnlyBuffer<string>)buffer, output)
                : GenericActivator.Create<IOperation>(typeof(UnmanagedConversion<,>).MakeGenericType(buffer.DataType, typeof(bool)), buffer, output)
            ;
            await conversion.Process();
            return output;

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
        }

        public static async Task<ICompositeBuffer<DateOnly>> ToDate(this IReadOnlyBuffer buffer,
            IProvideTempData? tempStreams = null, 
            int blockSize = Consts.DefaultBlockSize, 
            uint? maxInMemoryBlocks = null,
            uint? maxDistinctItems = null
        ) {
            var output = CreateCompositeBuffer<DateOnly>(tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems);
            var conversion = (buffer.DataType == typeof(string))
                ? new CustomConversion<string, DateOnly>(StringToDate, (IReadOnlyBuffer<string>)buffer, output)
                : GenericActivator.Create<IOperation>(typeof(UnmanagedConversion<,>).MakeGenericType(buffer.DataType, typeof(bool)), buffer, output)
            ;
            await conversion.Process();
            return output;

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

        public static async Task<ICompositeBuffer<TimeOnly>> ToTime(this IReadOnlyBuffer buffer,
            IProvideTempData? tempStreams = null, 
            int blockSize = Consts.DefaultBlockSize, 
            uint? maxInMemoryBlocks = null,
            uint? maxDistinctItems = null
        ) {
            var output = CreateCompositeBuffer<TimeOnly>(tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems);
            var conversion = (buffer.DataType == typeof(string))
                ? new CustomConversion<string, TimeOnly>(StringToTime, (IReadOnlyBuffer<string>)buffer, output)
                : GenericActivator.Create<IOperation>(typeof(UnmanagedConversion<,>).MakeGenericType(buffer.DataType, typeof(bool)), buffer, output)
            ;
            await conversion.Process();
            return output;

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

        public static async Task<ICompositeBuffer<int>> ToCategoricalIndex(this IReadOnlyBuffer buffer,
            IProvideTempData? tempStreams = null, 
            int blockSize = Consts.DefaultBlockSize, 
            uint? maxInMemoryBlocks = null,
            uint? maxDistinctItems = null
        ) {
            
            var output = CreateCompositeBuffer<int>(tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems);
            var conversion = GenericActivator.Create<IOperation>(typeof(ToCategoricalIndexConversion<>).MakeGenericType(buffer.DataType), buffer, output);
            await conversion.Process();
            return output;
        }

        public static async Task<ICompositeBuffer<IndexList>> ToIndexList(this IReadOnlyBuffer buffer,
            IProvideTempData? tempStreams = null, 
            int blockSize = Consts.DefaultBlockSize, 
            uint? maxInMemoryBlocks = null,
            uint? maxDistinctItems = null
        ) {
            var output = CreateCompositeBuffer<IndexList>(x => new(x), tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems);
            IOperation conversion;
            if (buffer.DataType == typeof(IndexList))
                conversion = new NopConversion<IndexList>((IReadOnlyBuffer<IndexList>)buffer, output);
            else if (buffer.DataType == typeof(WeightedIndexList))
                conversion = new CustomConversion<WeightedIndexList, IndexList>(WeightedIndexListToIndexList, (IReadOnlyBuffer<WeightedIndexList>)buffer, output);
            else if (buffer.DataType == typeof(ReadOnlyVector))
                conversion = new CustomConversion<ReadOnlyVector, IndexList>(VectorToIndexList, (IReadOnlyBuffer<ReadOnlyVector>)buffer, output);
            else
                throw new NotSupportedException("Only weighted index lists and vectors can be converted to index lists");
            await conversion.Process();
            return output;

            static IndexList VectorToIndexList(ReadOnlyVector vector) => vector.ReadOnlySegment.ToSparse().AsIndexList();
            static IndexList WeightedIndexListToIndexList(WeightedIndexList weightedIndexList) => weightedIndexList.AsIndexList();
        }

        public static async Task<ICompositeBuffer<ReadOnlyVector>> ToVector(this IReadOnlyBuffer buffer,
            IProvideTempData? tempStreams = null, 
            int blockSize = Consts.DefaultBlockSize, 
            uint? maxInMemoryBlocks = null,
            uint? maxDistinctItems = null
        ) {
            var output = CreateCompositeBuffer<ReadOnlyVector>(x => new(x), tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems);
            IOperation conversion;
            if (buffer.DataType == typeof(ReadOnlyVector))
                conversion = new NopConversion<ReadOnlyVector>((IReadOnlyBuffer<ReadOnlyVector>)buffer, output);
            else if (buffer.DataType == typeof(WeightedIndexList))
                conversion = new CustomConversion<WeightedIndexList, ReadOnlyVector>(WeightedIndexListToVector, (IReadOnlyBuffer<WeightedIndexList>)buffer, output);
            else if (buffer.DataType == typeof(IndexList))
                conversion = new CustomConversion<IndexList, ReadOnlyVector>(IndexListToVector, (IReadOnlyBuffer<IndexList>)buffer, output);
            else
                throw new NotSupportedException("Only weighted index lists, index lists and vectors can be converted to vectors");
            await conversion.Process();
            return output;

            static ReadOnlyVector WeightedIndexListToVector(WeightedIndexList weightedIndexList) => weightedIndexList.AsDense();
            static ReadOnlyVector IndexListToVector(IndexList indexList) => indexList.AsDense();
        }

        public static async Task<ICompositeBuffer<WeightedIndexList>> ToWeightedIndexList(this IReadOnlyBuffer buffer,
            IProvideTempData? tempStreams = null, 
            int blockSize = Consts.DefaultBlockSize, 
            uint? maxInMemoryBlocks = null,
            uint? maxDistinctItems = null
        ) {
            var output = CreateCompositeBuffer<WeightedIndexList>(x => new(x), tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems);
            IOperation conversion;
            if (buffer.DataType == typeof(WeightedIndexList))
                conversion = new NopConversion<WeightedIndexList>((IReadOnlyBuffer<WeightedIndexList>)buffer, output);
            else if (buffer.DataType == typeof(ReadOnlyVector))
                conversion = new CustomConversion<ReadOnlyVector, WeightedIndexList>(VectorToWeightedIndexList, (IReadOnlyBuffer<ReadOnlyVector>)buffer, output);
            else if (buffer.DataType == typeof(IndexList))
                conversion = new CustomConversion<IndexList, WeightedIndexList>(IndexListToWeightedIndexList, (IReadOnlyBuffer<IndexList>)buffer, output);
            else
                throw new NotSupportedException("Only weighted index lists, index lists and vectors can be converted to vectors");
            await conversion.Process();
            return output;

            static WeightedIndexList IndexListToWeightedIndexList(IndexList indexList) => indexList.AsWeightedIndexList();
            static WeightedIndexList VectorToWeightedIndexList(ReadOnlyVector vector) => vector.ToSparse();
        }

        class AggregateNotification : INotifyUser
        {
            readonly int _count;
            readonly Guid _id = Guid.NewGuid();
            readonly Dictionary<Guid, float> _taskProgress = new();
            readonly HashSet<Guid> _completed = new();
            readonly INotifyUser _notify;
            readonly CancellationToken _ct;
            readonly string? _msg;
            int _progressNotifications = 0;

            public AggregateNotification(int count, INotifyUser notify, string? msg, CancellationToken ct)
            {
                _count = count;
                _notify = notify;
                _msg = msg;
                _ct = ct;
            }

            public void OnStartOperation(Guid operationId, string? msg = null)
            {
                _taskProgress.Add(operationId, 0f);
                if(_taskProgress.Count == _count)
                    _notify.OnStartOperation(_id, _msg);
            }

            public void OnOperationProgress(Guid operationId, float progressPercent)
            {
                _taskProgress[operationId] = progressPercent;
                if (++_progressNotifications % _count == 0) {
                    var aggregateProgress = _taskProgress.Values.Average();
                    _notify.OnOperationProgress(_id, aggregateProgress);
                }
            }

            public void OnCompleteOperation(Guid operationId, bool wasCancelled)
            {
                _taskProgress[operationId] = 1f;
                if(_completed.Add(operationId) && _completed.Count == _count)
                    _notify.OnCompleteOperation(_id, _ct.IsCancellationRequested);
            }

            public void OnMessage(string msg)
            {
                // ignore
            }
        }
        public static Task Process(this IOperation[] operations, INotifyUser? notify = null, string? msg = null, CancellationToken ct = default)
        {
            if (operations.Length == 1)
                return operations[0].Process(notify, msg, ct);
            if (operations.Length > 1) {
                if (notify is not null)
                    notify = new AggregateNotification(operations.Length, notify, msg, ct);
                return Task.WhenAll(operations.Select(x => x.Process(notify, null, ct)));
            }
            return Task.CompletedTask;
        }
    }
}
