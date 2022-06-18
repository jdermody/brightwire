using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightData.DataTable2
{
    public partial class BrightDataTable
    {
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
                    var ret = new IndexListConverter(_context, _indices.Value);
                    return (IConvertStructsToObjects<CT, T>)ret;
                }

                if (dataType == typeof(WeightedIndexList)) {
                    var ret = new WeightedIndexListConverter(_context, _weightedIndices.Value);
                    return (IConvertStructsToObjects<CT, T>)ret;
                }

                if (dataType == typeof(IVector)) {
                    var ret = new VectorConverter(_context, _tensors.Value);
                    return (IConvertStructsToObjects<CT, T>)ret;
                }

                if (dataType == typeof(IMatrix)) {
                    var ret = new MatrixConverter(_context, _tensors.Value);
                    return (IConvertStructsToObjects<CT, T>)ret;
                }

                if (dataType == typeof(ITensor3D)) {
                    var ret = new Tensor3DConverter(_context, _tensors.Value);
                    return (IConvertStructsToObjects<CT, T>)ret;
                }

                if (dataType == typeof(ITensor4D)) {
                    var ret = new Tensor4DConverter(_context, _tensors.Value);
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
            readonly IBrightDataContext _context;
            readonly ICanRandomlyAccessData<uint> _indices;

            public IndexListConverter(IBrightDataContext context, ICanRandomlyAccessData<uint> indices)
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
            readonly IBrightDataContext _context;
            readonly ICanRandomlyAccessData<WeightedIndexList.Item> _indices;

            public WeightedIndexListConverter(IBrightDataContext context, ICanRandomlyAccessData<WeightedIndexList.Item> indices)
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
            readonly IBrightDataContext _context;
            readonly ICanRandomlyAccessData<float> _data;

            public VectorConverter(IBrightDataContext context, ICanRandomlyAccessData<float> data)
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
            readonly IBrightDataContext _context;
            readonly ICanRandomlyAccessData<float> _data;

            public MatrixConverter(IBrightDataContext context, ICanRandomlyAccessData<float> data)
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
                return cu.CreateMatrix(segment, item.RowCount, item.ColumnCount);
            }
        }

        class Tensor3DConverter : IConvertStructsToObjects<Tensor3DColumnType, ITensor3D>
        {
            readonly IBrightDataContext _context;
            readonly ICanRandomlyAccessData<float> _data;

            public Tensor3DConverter(IBrightDataContext context, ICanRandomlyAccessData<float> data)
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
                return cu.CreateTensor3D(segment, item.Depth, item.RowCount, item.ColumnCount);
            }
        }

        class Tensor4DConverter : IConvertStructsToObjects<Tensor4DColumnType, ITensor4D>
        {
            readonly IBrightDataContext _context;
            readonly ICanRandomlyAccessData<float> _data;

            public Tensor4DConverter(IBrightDataContext context, ICanRandomlyAccessData<float> data)
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
                return cu.CreateTensor4D(segment, item.Count, item.Depth, item.RowCount, item.ColumnCount);
            }
        }
    }
}
