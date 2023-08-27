using System.Reflection;
using BrightData.LinearAlgebra.ReadOnly;
using BrightData.Table.Buffer.ReadOnly;
using BrightData.Table.Helper;

namespace BrightData.Table
{
    internal class ColumnOriented : TableBase
    {
        const int ReaderBlockSize = 8;
        readonly IReadOnlyBuffer[] _columnReader;

        public ColumnOriented(IByteBlockReader data) : base(data, DataTableOrientation.ColumnOriented)
        {
            var genericMethods              = GetType().GetGenericMethods();
            var createColumnReader          = genericMethods[nameof(CreateColumnReader)];

            // create column readers
            _columnReader = new IReadOnlyBuffer[ColumnCount];
            var prevOffset = _header.DataOffset;
            for (uint i = 1; i < _columns.Length; i++) {
                var (prevType, prevSize) = ColumnTypes[i - 1].GetColumnType();
                var nextOffset = prevOffset + prevSize * RowCount;
                CreateColumnReader(createColumnReader, i - 1, prevOffset, nextOffset - prevOffset);
                prevOffset = nextOffset;
            }
            CreateColumnReader(createColumnReader, (uint)_columns.Length - 1, prevOffset, _header.DataOffset + _header.DataSizeBytes - prevOffset);
        }

        void CreateColumnReader(MethodInfo createColumnReader, uint columnIndex, uint offset, uint size)
        {
            var columnType = ColumnTypes[columnIndex];
            var (prevType, prevSize) = columnType.GetColumnType();
            var reader = (IReadOnlyBuffer)createColumnReader.MakeGenericMethod(prevType).Invoke(this, new object[] { columnIndex, offset, size })!;
            _columnReader[columnIndex] = columnType switch {
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

        public override IReadOnlyBuffer<T> GetColumn<T>(uint index) => (IReadOnlyBuffer<T>)_columnReader[index];
        protected override IReadOnlyBuffer GetColumn(uint index) => _columnReader[index];

        IReadOnlyBuffer<T> CreateColumnReader<T>(uint columnIndex, uint offset, uint size) where T : unmanaged => 
            new BlockReaderReadOnlyBuffer<T>(_columnMetaData[columnIndex], _reader, offset, size, ReaderBlockSize);
    }
}