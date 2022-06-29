using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightData.DataTable2
{
    public partial class BrightDataTable
    {
        ICanEnumerateDisposable<T> GetColumnReader<T>(uint columnIndex, uint countToRead, Func<uint, uint>? offsetAdjuster = null) where T : notnull
        {
            ref readonly var column = ref _columns[columnIndex];
            if (column.DataType.GetDataType() != typeof(T))
                throw new ArgumentException($"Data types do not align - expected {column.DataType.GetDataType()} but received {typeof(T)}", nameof(T));
            
            var offset = _columnOffset[columnIndex];
            if(offsetAdjuster is not null)
                offset += offsetAdjuster(column.DataTypeSize);
            var (columnDataType, _) = column.DataType.GetColumnType();
            var sizeInBytes = countToRead * column.DataTypeSize;
            return (ICanEnumerateDisposable<T>)_getReader.MakeGenericMethod(columnDataType, typeof(T)).Invoke(this, new object[] { offset, sizeInBytes })!;
        }

        ICanEnumerateDisposable GetColumnReader(uint columnIndex, uint countToRead, Func<uint, uint>? offsetAdjuster = null)
        {
            ref readonly var column = ref _columns[columnIndex];

            var offset = _columnOffset[columnIndex];
            if(offsetAdjuster is not null)
                offset += offsetAdjuster(column.DataTypeSize);
            var (columnDataType, _) = column.DataType.GetColumnType();
            var dataType = column.DataType.GetDataType();
            var sizeInBytes = countToRead * column.DataTypeSize;
            return (ICanEnumerateDisposable)_getReader.MakeGenericMethod(columnDataType, dataType).Invoke(this, new object[] { offset, sizeInBytes })!;
        }

        ICanEnumerateDisposable[] GetColumnReaders(IEnumerable<uint> columnIndices)
        {
            var columnCount = _header.ColumnCount;
            var ret = new ICanEnumerateDisposable[columnCount];
            for (uint i = 0; i < columnCount; i++)
                ret[i] = GetColumnReader(i, _header.RowCount);
            return ret;
        }

        ICanEnumerate<T> GetReader<CT, T>(uint offset, uint sizeInBytes)
            where T: notnull
            where CT: unmanaged
        {
            IConvertStructsToObjects<CT, T> GetConverter()
            {
                var dataType = typeof(T);

                if (dataType == typeof(string)) {
                    var ret = new StringColumnConverter(_stringTable.Value);
                    return (IConvertStructsToObjects<CT, T>)ret;
                }

                if (dataType == typeof(IndexList)) {
                    var ret = new IndexListConverter(Context, _indices.Value);
                    return (IConvertStructsToObjects<CT, T>)ret;
                }

                if (dataType == typeof(WeightedIndexList)) {
                    var ret = new WeightedIndexListConverter(Context, _weightedIndices.Value);
                    return (IConvertStructsToObjects<CT, T>)ret;
                }

                if (dataType == typeof(IVector)) {
                    var ret = new VectorConverter(Context, _tensors.Value);
                    return (IConvertStructsToObjects<CT, T>)ret;
                }

                if (dataType == typeof(IMatrix)) {
                    var ret = new MatrixConverter(Context, _tensors.Value);
                    return (IConvertStructsToObjects<CT, T>)ret;
                }

                if (dataType == typeof(ITensor3D)) {
                    var ret = new Tensor3DConverter(Context, _tensors.Value);
                    return (IConvertStructsToObjects<CT, T>)ret;
                }

                if (dataType == typeof(ITensor4D)) {
                    var ret = new Tensor4DConverter(Context, _tensors.Value);
                    return (IConvertStructsToObjects<CT, T>)ret;
                }

                return new NopColumnConverter<CT, T>();
            }

            var block = _buffer.GetIterator<CT>(offset, sizeInBytes);
            var converter = GetConverter();
            return new ColumnReader<CT, T>(block.GetEnumerator(), converter, block);
        }

        class NopColumnConverter<CT, T> : IConvertStructsToObjects<CT, T>
            where CT : unmanaged
            where T: notnull
        {
            public T Convert(ref CT item)
            {
                return __refvalue(__makeref(item), T);
            }
        }

        class StringColumnConverter : IConvertStructsToObjects<uint, string>
        {
            readonly string[] _stringTable;

            public StringColumnConverter(string[] stringTable)
            {
                _stringTable = stringTable;
            }

            public string Convert(ref uint item) => _stringTable[item];
        }

        class IndexListConverter : IConvertStructsToObjects<DataRangeColumnType, IndexList>
        {
            readonly BrightDataContext _context;
            readonly ICanRandomlyAccessData<uint> _indices;

            public IndexListConverter(BrightDataContext context, ICanRandomlyAccessData<uint> indices)
            {
                _context = context;
                _indices = indices;
            }

            public IndexList Convert(ref DataRangeColumnType item)
            {
                var span = _indices.GetSpan(item.StartIndex, item.Count);
                return IndexList.Create(_context, span);
            }
        }

        class WeightedIndexListConverter : IConvertStructsToObjects<DataRangeColumnType, WeightedIndexList>
        {
            readonly BrightDataContext _context;
            readonly ICanRandomlyAccessData<WeightedIndexList.Item> _indices;

            public WeightedIndexListConverter(BrightDataContext context, ICanRandomlyAccessData<WeightedIndexList.Item> indices)
            {
                _context = context;
                _indices = indices;
            }

            public WeightedIndexList Convert(ref DataRangeColumnType item)
            {
                var span = _indices.GetSpan(item.StartIndex, item.Count);
                return WeightedIndexList.Create(_context, span);
            }
        }

        class VectorConverter : IConvertStructsToObjects<DataRangeColumnType, IVector>
        {
            readonly BrightDataContext _context;
            readonly ICanRandomlyAccessData<float> _data;

            public VectorConverter(BrightDataContext context, ICanRandomlyAccessData<float> data)
            {
                _context = context;
                _data = data;
            }

            public IVector Convert(ref DataRangeColumnType item)
            {
                var cu = _context.LinearAlgebraProvider2;
                var span = _data.GetSpan(item.StartIndex, item.Count);
                var segment = cu.CreateSegment(item.Count);
                segment.CopyFrom(span);
                return cu.CreateVector(segment);
            }
        }

        class MatrixConverter : IConvertStructsToObjects<MatrixColumnType, IMatrix>
        {
            readonly BrightDataContext _context;
            readonly ICanRandomlyAccessData<float> _data;

            public MatrixConverter(BrightDataContext context, ICanRandomlyAccessData<float> data)
            {
                _context = context;
                _data = data;
            }

            public IMatrix Convert(ref MatrixColumnType item)
            {
                var cu = _context.LinearAlgebraProvider2;
                var size = item.RowCount * item.ColumnCount;
                var span = _data.GetSpan(item.StartIndex, size);
                var segment = cu.CreateSegment(size);
                segment.CopyFrom(span);
                return cu.CreateMatrix(item.RowCount, item.ColumnCount, segment);
            }
        }

        class Tensor3DConverter : IConvertStructsToObjects<Tensor3DColumnType, ITensor3D>
        {
            readonly BrightDataContext _context;
            readonly ICanRandomlyAccessData<float> _data;

            public Tensor3DConverter(BrightDataContext context, ICanRandomlyAccessData<float> data)
            {
                _context = context;
                _data = data;
            }

            public ITensor3D Convert(ref Tensor3DColumnType item)
            {
                var cu = _context.LinearAlgebraProvider2;
                var size = item.RowCount * item.ColumnCount * item.Depth;
                var span = _data.GetSpan(item.StartIndex, size);
                var segment = cu.CreateSegment(size);
                segment.CopyFrom(span);
                return cu.CreateTensor3D(item.Depth, item.RowCount, item.ColumnCount, segment);
            }
        }

        class Tensor4DConverter : IConvertStructsToObjects<Tensor4DColumnType, ITensor4D>
        {
            readonly BrightDataContext _context;
            readonly ICanRandomlyAccessData<float> _data;

            public Tensor4DConverter(BrightDataContext context, ICanRandomlyAccessData<float> data)
            {
                _context = context;
                _data = data;
            }

            public ITensor4D Convert(ref Tensor4DColumnType item)
            {
                var cu = _context.LinearAlgebraProvider2;
                var size = item.RowCount * item.ColumnCount * item.Depth * item.Count;
                var span = _data.GetSpan(item.StartIndex, size);
                var segment = cu.CreateSegment(size);
                segment.CopyFrom(span);
                return cu.CreateTensor4D(item.Count, item.Depth, item.RowCount, item.ColumnCount, segment);
            }
        }
    }
}
