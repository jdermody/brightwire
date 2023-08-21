using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using BrightData.LinearAlgebra.ReadOnly;
using BrightData.Table.Buffer;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData.Table
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Builds a table of the generic methods from a type
        /// </summary>
        /// <param name="type">Type to inspect</param>
        /// <param name="bindingFlags">Method flags</param>
        /// <returns></returns>
        public static Dictionary<string, MethodInfo> GetGenericMethods(
            this Type type, 
            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static
        ) {
            return type.GetMethods(bindingFlags)
                .Where(m => m.IsGenericMethod)
                .ToDictionary(m => m.Name)
            ;
        }

        public static ICompositeBuffer<string> CreateCompositeBuffer(
            IProvideTempStreams? tempStreams = null, 
            int blockSize = Consts.DefaultBlockSize, 
            uint? maxInMemoryBlocks = null,
            uint? maxDistinctItems = null
        ) => new StringCompositeBuffer(tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems);

        public static ICompositeBuffer<T> CreateCompositeBuffer<T>(
            CreateFromReadOnlyByteSpan<T> createItem,
            IProvideTempStreams? tempStreams = null, 
            int blockSize = Consts.DefaultBlockSize, 
            uint? maxInMemoryBlocks = null,
            uint? maxDistinctItems = null
        ) where T: IHaveDataAsReadOnlyByteSpan => new ManagedCompositeBuffer<T>(createItem, tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems);

        public static ICompositeBuffer<T> CreateCompositeBuffer<T>(
            IProvideTempStreams? tempStreams = null, 
            int blockSize = Consts.DefaultBlockSize, 
            uint? maxInMemoryBlocks = null,
            uint? maxDistinctItems = null
        ) where T: unmanaged => new UnmanagedCompositeBuffer<T>(tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems);

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
            IProvideTempStreams? tempStreams = null, 
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
        public static CompositeBufferIterator<T> Enumerate<T>(this ICompositeBuffer<T> buffer) where T: notnull => new(buffer);

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
            IProvideTempStreams? tempStreams = null, 
            int blockSize = Consts.DefaultBlockSize, 
            uint? maxInMemoryBlocks = null,
            uint? maxDistinctItems = null
        ) => GetCompositeBuffer(GetTableDataType(type), tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems);

        public static ICompositeBuffer GetCompositeBuffer(this BrightDataType type,
            IProvideTempStreams? tempStreams = null,
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
    }
}
