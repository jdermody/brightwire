using System;
using System.Reflection;
using System.Threading.Tasks;
using BrightData.Buffer.ReadOnly;
using BrightData.Buffer.ReadOnly.Converter;
using BrightData.Converter;
using BrightData.Helper;
using BrightData.LinearAlgebra.ReadOnly;

namespace BrightData.DataTable
{
    internal class ColumnOrientedDataTable : TableBase
    {
        const int ReaderBlockSize = 128;
        readonly IReadOnlyBufferWithMetaData[] _columnReader;

        public ColumnOrientedDataTable(BrightDataContext context, IByteBlockReader data) : base(context, data, DataTableOrientation.ColumnOriented)
        {
            var genericMethods              = GetType().GetGenericMethods();
            var createColumnReader          = genericMethods[nameof(CreateColumnReader)];

            // create column readers
            _columnReader = new IReadOnlyBufferWithMetaData[ColumnCount];
            var prevOffset = _header.DataOffset;
            for (uint i = 1; i < _columns.Length; i++) {
                var prevColumnType = ColumnTypes[i - 1];
                var (prevType, prevSize) = prevColumnType.GetColumnType();
                var nextOffset = prevOffset + prevSize * RowCount;
                CreateColumnReader(createColumnReader, prevColumnType, prevType, i - 1, prevOffset, nextOffset - prevOffset);
                prevOffset = nextOffset;
            }
            var lastColumnType = ColumnTypes[_columns.Length - 1];
            var (lastColumnDataType, _) = lastColumnType.GetColumnType();
            CreateColumnReader(createColumnReader, lastColumnType, lastColumnDataType, (uint)_columns.Length - 1, prevOffset, _header.DataOffset + _header.DataSizeBytes - prevOffset);
        }

        public override Task<T> Get<T>(uint columnIndex, uint rowIndex)
        {
            var column = _columnReader[columnIndex];
            if (column.DataType != typeof(T))
                throw new ArgumentException($"Column {columnIndex} is {column.DataType} but requested {typeof(T)}");
            var reader = (IReadOnlyBuffer<T>)column;
            return reader.GetItem(rowIndex);
        }

        public override Task<T[]> Get<T>(uint columnIndex, params uint[] rowIndices)
        {
            var column = _columnReader[columnIndex];
            if (column.DataType != typeof(T))
                throw new ArgumentException($"Column {columnIndex} is {column.DataType} but requested {typeof(T)}");
            var reader = (IReadOnlyBuffer<T>)column;
            return reader.GetItems(rowIndices);
        }

        void CreateColumnReader(MethodInfo createColumnReader, BrightDataType dataType, Type type, uint columnIndex, uint offset, uint size)
        {
            var reader = (IReadOnlyBufferWithMetaData)createColumnReader.MakeGenericMethod(type).Invoke(this, new object[] { columnIndex, offset, size })!;
            _columnReader[columnIndex] = dataType switch {
                BrightDataType.String            => new MappedReadOnlyBuffer<uint, string>((IReadOnlyBufferWithMetaData<uint>)reader, GetStrings),
                BrightDataType.BinaryData        => new MappedReadOnlyBuffer<DataRangeColumnType, BinaryData>((IReadOnlyBufferWithMetaData<DataRangeColumnType>)reader, GetBinaryData),
                BrightDataType.IndexList         => new MappedReadOnlyBuffer<DataRangeColumnType, IndexList>((IReadOnlyBufferWithMetaData<DataRangeColumnType>)reader, GetIndexLists),
                BrightDataType.WeightedIndexList => new MappedReadOnlyBuffer<DataRangeColumnType, WeightedIndexList>((IReadOnlyBufferWithMetaData<DataRangeColumnType>)reader, GetWeightedIndexLists),
                BrightDataType.Vector            => new MappedReadOnlyBuffer<DataRangeColumnType, ReadOnlyVector>((IReadOnlyBufferWithMetaData<DataRangeColumnType>)reader, GetVectors),
                BrightDataType.Matrix            => new MappedReadOnlyBuffer<MatrixColumnType, ReadOnlyMatrix>((IReadOnlyBufferWithMetaData<MatrixColumnType>)reader, GetMatrices),
                BrightDataType.Tensor3D          => new MappedReadOnlyBuffer<Tensor3DColumnType, ReadOnlyTensor3D>((IReadOnlyBufferWithMetaData<Tensor3DColumnType>)reader, GetTensors),
                BrightDataType.Tensor4D          => new MappedReadOnlyBuffer<Tensor4DColumnType, ReadOnlyTensor4D>((IReadOnlyBufferWithMetaData<Tensor4DColumnType>)reader, GetTensors),
                _                                => reader
            };
        }

        public override IReadOnlyBufferWithMetaData<T> GetColumn<T>(uint index)
        {
            var typeofT = typeof(T);
            var reader = _columnReader[index];
            var dataType = reader.DataType;

            if(dataType == typeofT)
                return (IReadOnlyBufferWithMetaData<T>)_columnReader[index];
            if (typeofT == typeof(object))
                return GenericActivator.Create<IReadOnlyBufferWithMetaData<T>>(typeof(ToObjectConverter<>).MakeGenericType(dataType), reader);
            if (typeofT == typeof(string))
                return GenericActivator.Create<IReadOnlyBufferWithMetaData<T>>(typeof(ToStringConverter<>).MakeGenericType(dataType), reader);
            if (typeofT.GetTypeInfo().IsAssignableFrom(dataType.GetTypeInfo()))
                return GenericActivator.Create<IReadOnlyBufferWithMetaData<T>>(typeof(CastConverter<,>).MakeGenericType(dataType, typeof(T)), reader);
            if (dataType.GetBrightDataType().IsNumeric() && typeofT.GetBrightDataType().IsNumeric()) {
                var converter = StaticConverters.GetConverterMethodInfo.MakeGenericMethod(dataType, typeof(T)).Invoke(null, null);
                return GenericActivator.Create<IReadOnlyBufferWithMetaData<T>>(typeof(TypeConverter<,>).MakeGenericType(dataType, typeof(T)), reader, converter);
            }

            throw new NotImplementedException($"Not able to create a column of type {typeof(T)} from {dataType}");
        }
        public override IReadOnlyBufferWithMetaData GetColumn(uint index) => _columnReader[index];

        IReadOnlyBuffer<T> CreateColumnReader<T>(uint columnIndex, uint offset, uint size) where T : unmanaged => 
            new BlockReaderReadOnlyBuffer<T>(_columnMetaData[columnIndex], _reader, offset, size, ReaderBlockSize);
    }
}