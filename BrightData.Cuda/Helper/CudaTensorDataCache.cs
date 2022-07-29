using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightData.DataTable;
using ManagedCuda;
using ManagedCuda.BasicTypes;

namespace BrightData.Cuda.Helper
{
    public class CudaTensorDataCache : IDisposable
    {
        readonly IDeviceMemoryPtr _data;
        readonly List<IDisposable> _readers = new();

        class VectorReader : ICanRandomlyAccessData<IReadOnlyVector>
        {
            readonly CudaLinearAlgebraProvider _lap;
            readonly ICanRandomlyAccessUnmanagedData<DataRangeColumnType> _source;
            readonly IDeviceMemoryPtr _data;
            readonly CudaVector?[] _cache;

            public VectorReader(CudaLinearAlgebraProvider lap, ICanRandomlyAccessUnmanagedData<DataRangeColumnType> vectors, IDeviceMemoryPtr data)
            {
                _lap = lap;
                _source = vectors;
                _data = data;
                _cache = new CudaVector[vectors.Size];
            }

            public void Dispose()
            {
                _source.Dispose();
                foreach(var item in _cache)
                    item?.Dispose();
            }

            public uint Size => _source.Size;
            object ICanRandomlyAccessData.this[int index] => Get((uint)index);
            public IReadOnlyVector this[uint index] => Get(index);
            public IReadOnlyVector this[int index] => Get((uint)index);
            object ICanRandomlyAccessData.this[uint index] => Get(index);

            IReadOnlyVector Get(uint index)
            {
                var ret = _cache[index];
                if (ret is null) {
                    _source.Get(index, out var item);
                    var ptr = _data.Offset(item.StartIndex, item.Count);
                    ret = _cache[index] = new CudaVector(new CudaTensorSegment(ptr), _lap);
                    ret.Segment.AddRef();
                }
                return ret;
            }
        }
        class MatrixReader : ICanRandomlyAccessData<IReadOnlyMatrix>
        {
            readonly CudaLinearAlgebraProvider _lap;
            readonly ICanRandomlyAccessUnmanagedData<MatrixColumnType> _source;
            readonly IDeviceMemoryPtr _data;
            readonly CudaMatrix?[] _cache;

            public MatrixReader(CudaLinearAlgebraProvider lap, ICanRandomlyAccessUnmanagedData<MatrixColumnType> source, IDeviceMemoryPtr data)
            {
                _lap = lap;
                _source = source;
                _data = data;
                _cache = new CudaMatrix[source.Size];
            }

            public void Dispose()
            {
                _source.Dispose();
                foreach(var item in _cache)
                    item?.Dispose();
            }

            public uint Size => _source.Size;
            object ICanRandomlyAccessData.this[int index] => Get((uint)index);
            public IReadOnlyMatrix this[uint index] => Get(index);
            public IReadOnlyMatrix this[int index] => Get((uint)index);
            object ICanRandomlyAccessData.this[uint index] => Get(index);

            IReadOnlyMatrix Get(uint index)
            {
                var ret = _cache[index];
                if (ret is null) {
                    _source.Get(index, out var item);
                    var ptr = _data.Offset(item.StartIndex, item.RowCount * item.ColumnCount);
                    ret = _cache[index] = new CudaMatrix(new CudaTensorSegment(ptr), item.RowCount, item.ColumnCount, _lap);
                    ret.Segment.AddRef();
                }
                return ret;
            }
        }

        class Tensor3DReader : ICanRandomlyAccessData<IReadOnlyTensor3D>
        {
            readonly CudaLinearAlgebraProvider _lap;
            readonly ICanRandomlyAccessUnmanagedData<Tensor3DColumnType> _source;
            readonly IDeviceMemoryPtr _data;
            readonly CudaTensor3D?[] _cache;

            public Tensor3DReader(CudaLinearAlgebraProvider lap, ICanRandomlyAccessUnmanagedData<Tensor3DColumnType> source, IDeviceMemoryPtr data)
            {
                _lap = lap;
                _source = source;
                _data = data;
                _cache = new CudaTensor3D[source.Size];
            }

            public void Dispose()
            {
                _source.Dispose();
                foreach(var item in _cache)
                    item?.Dispose();
            }

            public uint Size => _source.Size;
            object ICanRandomlyAccessData.this[int index] => Get((uint)index);
            public IReadOnlyTensor3D this[uint index] => Get(index);
            public IReadOnlyTensor3D this[int index] => Get((uint)index);
            object ICanRandomlyAccessData.this[uint index] => Get(index);

            IReadOnlyTensor3D Get(uint index)
            {
                var ret = _cache[index];
                if (ret is null) {
                    _source.Get(index, out var item);
                    var ptr = _data.Offset(item.StartIndex, item.RowCount * item.ColumnCount * item.Depth);
                    ret = _cache[index] = new CudaTensor3D(new CudaTensorSegment(ptr), item.Depth, item.RowCount, item.ColumnCount, _lap);
                    ret.Segment.AddRef();
                }
                return ret;
            }
        }

        class Tensor4DReader : ICanRandomlyAccessData<IReadOnlyTensor4D>
        {
            readonly CudaLinearAlgebraProvider _lap;
            readonly ICanRandomlyAccessUnmanagedData<Tensor4DColumnType> _source;
            readonly IDeviceMemoryPtr _data;
            readonly CudaTensor4D?[] _cache;

            public Tensor4DReader(CudaLinearAlgebraProvider lap, ICanRandomlyAccessUnmanagedData<Tensor4DColumnType> source, IDeviceMemoryPtr data)
            {
                _lap = lap;
                _source = source;
                _data = data;
                _cache = new CudaTensor4D[source.Size];
            }

            public void Dispose()
            {
                _source.Dispose();
                foreach(var item in _cache)
                    item?.Dispose();
            }

            public uint Size => _source.Size;
            object ICanRandomlyAccessData.this[int index] => Get((uint)index);
            public IReadOnlyTensor4D this[uint index] => Get(index);
            public IReadOnlyTensor4D this[int index] => Get((uint)index);
            object ICanRandomlyAccessData.this[uint index] => Get(index);

            IReadOnlyTensor4D Get(uint index)
            {
                var ret = _cache[index];
                if (ret is null) {
                    _source.Get(index, out var item);
                    var ptr = _data.Offset(item.StartIndex, item.RowCount * item.ColumnCount * item.Depth * item.Count);
                    ret = _cache[index] = new CudaTensor4D(new CudaTensorSegment(ptr), item.Count, item.Depth, item.RowCount, item.ColumnCount, _lap);
                    ret.Segment.AddRef();
                }
                return ret;
            }
        }

        public CudaTensorDataCache(CudaLinearAlgebraProvider lap, BrightDataTable table)
        {
            // copy entire tensor block into CUDA device
            var span = table.TensorDataBlock.GetSpan(0, table.TensorDataBlock.Size);
            _data = new DeviceMemoryBlock(null, new CudaDeviceVariable<float>(span.Length));
            _data.CopyToDevice(span, 0);

            // replace any tensor column reader with a reader that reads from the cache
            var len = (uint)table.DefaultColumnReaders.Length;
            var columnReaders = new ICanRandomlyAccessData[len];
            var existingReaders = table.DefaultColumnReaders;
            for (uint i = 0; i < len; i++) {
                var type = table.ColumnTypes[i];
                if (type == BrightDataType.Vector)
                    _readers.Add(columnReaders[i] = new VectorReader(lap, table.GetRawColumnData<DataRangeColumnType>(i), _data));
                else if(type == BrightDataType.Matrix)
                    _readers.Add(columnReaders[i] = new MatrixReader(lap, table.GetRawColumnData<MatrixColumnType>(i), _data));
                else if(type == BrightDataType.Tensor3D)
                    _readers.Add(columnReaders[i] = new Tensor3DReader(lap, table.GetRawColumnData<Tensor3DColumnType>(i), _data));
                else if(type == BrightDataType.Tensor4D)
                    _readers.Add(columnReaders[i] = new Tensor4DReader(lap, table.GetRawColumnData<Tensor4DColumnType>(i), _data));
                else
                    columnReaders[i] = existingReaders[i];
            }
            table.SetCustomColumnReaders(columnReaders);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _data.Dispose();
            foreach(var item in _readers)
                item.Dispose();
        }
    }
}
