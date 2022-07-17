using System.Runtime.CompilerServices;
using BrightData.DataTable.TensorData;

namespace BrightData.DataTable
{
    public partial class BrightDataTable
    {
        IConvertStructsToObjects<CT, T> GetConverter<CT, T>()
            where T: notnull
            where CT: unmanaged
        {
            var dataType = typeof(T);

            if (dataType == typeof(string)) {
                var ret = new StringColumnConverter(_stringTable.Value);
                return (IConvertStructsToObjects<CT, T>)ret;
            }

            if (dataType == typeof(IndexList)) {
                var ret = new IndexListConverter(_indices.Value);
                return (IConvertStructsToObjects<CT, T>)ret;
            }

            if (dataType == typeof(WeightedIndexList)) {
                var ret = new WeightedIndexListConverter(_weightedIndices.Value);
                return (IConvertStructsToObjects<CT, T>)ret;
            }

            if (dataType == typeof(IReadOnlyVector)) {
                var ret = new VectorInfoConverter(_tensors.Value);
                return (IConvertStructsToObjects<CT, T>)ret;
            }

            if (dataType == typeof(IReadOnlyMatrix)) {
                var ret = new MatrixInfoConverter(_tensors.Value);
                return (IConvertStructsToObjects<CT, T>)ret;
            }

            if (dataType == typeof(IReadOnlyTensor3D)) {
                var ret = new Tensor3DInfoConverter(_tensors.Value);
                return (IConvertStructsToObjects<CT, T>)ret;
            }

            if (dataType == typeof(IReadOnlyTensor4D)) {
                var ret = new Tensor4DInfoConverter(_tensors.Value);
                return (IConvertStructsToObjects<CT, T>)ret;
            }

            return new NopColumnConverter<CT, T>();
        }

        class NopColumnConverter<CT, T> : IConvertStructsToObjects<CT, T>
            where CT : unmanaged
            where T: notnull
        {
            public T Convert(in CT item)
            {
                return __refvalue(__makeref(Unsafe.AsRef(item)), T);
            }
        }

        class StringColumnConverter : IConvertStructsToObjects<uint, string>
        {
            readonly string[] _stringTable;

            public StringColumnConverter(string[] stringTable)
            {
                _stringTable = stringTable;
            }

            public string Convert(in uint item) => _stringTable[item];
        }

        class IndexListConverter : IConvertStructsToObjects<DataRangeColumnType, IndexList>
        {
            readonly ICanRandomlyAccessUnmanagedData<uint> _indices;

            public IndexListConverter(ICanRandomlyAccessUnmanagedData<uint> indices)
            {
                _indices = indices;
            }

            public IndexList Convert(in DataRangeColumnType item)
            {
                var span = _indices.GetSpan(item.StartIndex, item.Count);
                return IndexList.Create(span);
            }
        }

        class WeightedIndexListConverter : IConvertStructsToObjects<DataRangeColumnType, WeightedIndexList>
        {
            readonly ICanRandomlyAccessUnmanagedData<WeightedIndexList.Item> _indices;

            public WeightedIndexListConverter(ICanRandomlyAccessUnmanagedData<WeightedIndexList.Item> indices)
            {
                _indices = indices;
            }

            public WeightedIndexList Convert(in DataRangeColumnType item)
            {
                var span = _indices.GetSpan(item.StartIndex, item.Count);
                return WeightedIndexList.Create(span);
            }
        }

        class VectorInfoConverter : IConvertStructsToObjects<DataRangeColumnType, VectorData>
        {
            readonly ICanRandomlyAccessUnmanagedData<float> _data;

            public VectorInfoConverter(ICanRandomlyAccessUnmanagedData<float> data)
            {
                _data = data;
            }

            public VectorData Convert(in DataRangeColumnType item)
            {
                return new VectorData(_data, item.StartIndex, 1, item.Count);
            }
        }

        class MatrixInfoConverter : IConvertStructsToObjects<MatrixColumnType, MatrixData>
        {
            readonly ICanRandomlyAccessUnmanagedData<float> _data;

            public MatrixInfoConverter(ICanRandomlyAccessUnmanagedData<float> data)
            {
                _data = data;
            }

            public MatrixData Convert(in MatrixColumnType item)
            {
                return new MatrixData(_data, item.RowCount, item.ColumnCount, item.StartIndex);
            }
        }

        class Tensor3DInfoConverter : IConvertStructsToObjects<Tensor3DColumnType, Tensor3DData>
        {
            readonly ICanRandomlyAccessUnmanagedData<float> _data;

            public Tensor3DInfoConverter(ICanRandomlyAccessUnmanagedData<float> data)
            {
                _data = data;
            }

            public Tensor3DData Convert(in Tensor3DColumnType item)
            {
                return new Tensor3DData(_data, item.Depth, item.RowCount, item.ColumnCount, item.StartIndex);
            }
        }

        class Tensor4DInfoConverter : IConvertStructsToObjects<Tensor4DColumnType, Tensor4DData>
        {
            readonly ICanRandomlyAccessUnmanagedData<float> _data;

            public Tensor4DInfoConverter(ICanRandomlyAccessUnmanagedData<float> data)
            {
                _data = data;
            }

            public Tensor4DData Convert(in Tensor4DColumnType item)
            {
                return new Tensor4DData(_data, item.Count, item.Depth, item.RowCount, item.ColumnCount, item.StartIndex);
            }
        }
    }
}
