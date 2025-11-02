using BrightData.Buffer;
using BrightData.Types;
using System;
using CommunityToolkit.HighPerformance;

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
            _dataTypes = dataProvider.GetDataSpan(dataTypes).Cast<byte, DataType>().ToArray();

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

        ReadOnlySpan<byte> GetData(uint index)
        {
            var (type, offset, size) = _dataTypes.Span[(int)index];
            return _dataProvider.GetDataSpan(offset, size);
        }
    }
}
