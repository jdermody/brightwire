using BrightData.Buffer;
using BrightData.Buffer.ByteDataProviders;
using BrightData.Types;
using CommunityToolkit.HighPerformance;
using System;

namespace BrightData.Helper
{
    /// <summary>
    /// Stores data in indexed tables
    /// </summary>
    public class DataStore
    {
        interface IResolveData<out T>
        {
            T Value { get; }
        }
        ref struct ResolveString : IResolveData<string>
        {
            public string Value { get; }
        }

        public readonly record struct DataType(BrightDataType Type, uint Offset, uint Size) : IHaveOffset, IHaveSize;

        readonly IByteDataProvider              _dataProvider;
        readonly ReadOnlyMemory<DataType>       _dataTypes;
        ReadOnlyMemory<OffsetAndSize>?          _stringTable;
        ReadOnlyMemory<byte>?                   _stringData;
        ReadOnlyMemory<byte>?                   _binaryData;
        ReadOnlyMemory<float>?                  _tensors;
        ReadOnlyMemory<uint>?                   _indices;
        ReadOnlyMemory<WeightedIndexList.Item>? _weightedIndices;
        

        /// <summary>
        /// Constructor
        /// </summary>
        public DataStore(OffsetAndSize dataTypes, IByteDataProvider dataProvider)
        {
            _dataProvider = dataProvider;
            using var dataTypesBlock = dataProvider.GetDataSpan(dataTypes);
            _dataTypes = dataTypesBlock.ByteData.Cast<byte, DataType>().ToArray();

            // create the data types
            var index = 0;
            DataTypes = new BrightDataType[_dataTypes.Length];
            foreach(var (type, _, _) in _dataTypes.Span)
                DataTypes[index++] = type;
        }

        public BrightDataType[] DataTypes { get; }

        public T Get<T>(uint index)
            where T: notnull
        {
            return default;
        }

        ByteDataBlock GetData(uint index)
        {
            var (_, offset, size) = _dataTypes.Span[(int)index];
            return _dataProvider.GetDataSpan(offset, size);
        }
    }
}
