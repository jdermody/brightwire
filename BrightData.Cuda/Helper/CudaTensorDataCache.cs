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
        readonly List<IDisposable> _vectorReaders = new();

        class VectorReader : ICanRandomlyAccessData<IReadOnlyVector>
        {
            readonly CudaLinearAlgebraProvider _lap;
            readonly ICanRandomlyAccessUnmanagedData<DataRangeColumnType> _vectors;
            readonly IDeviceMemoryPtr _data;
            readonly CudaVector?[] _cache;

            public VectorReader(CudaLinearAlgebraProvider lap, ICanRandomlyAccessUnmanagedData<DataRangeColumnType> vectors, IDeviceMemoryPtr data)
            {
                _lap = lap;
                _vectors = vectors;
                _data = data;
                _cache = new CudaVector[vectors.Size];
            }

            public void Dispose()
            {
                _vectors.Dispose();
                foreach(var item in _cache)
                    item?.Dispose();
            }

            public uint Size => _vectors.Size;
            object ICanRandomlyAccessData.this[int index] => Get((uint)index);
            public IReadOnlyVector this[uint index] => Get(index);
            public IReadOnlyVector this[int index] => Get((uint)index);
            object ICanRandomlyAccessData.this[uint index] => Get(index);

            IReadOnlyVector Get(uint index)
            {
                var ret = _cache[index];
                if (ret is null) {
                    _vectors.Get(index, out var item);
                    var ptr = _data.Offset(item.StartIndex, item.Count);
                    ret = _cache[index] = new CudaVector(new CudaTensorSegment(ptr), _lap);
                }
                return ret;
            }
        }

        public CudaTensorDataCache(CudaLinearAlgebraProvider lap, BrightDataTable table)
        {
            // copy entire tensor block into CUDA device
            var span = table.TensorDataBlock.GetSpan(0, table.TensorDataBlock.Size);
            _data = new DeviceMemoryBlock(null, new CudaDeviceVariable<float>(span.Length));
            _data.CopyToDevice(span);

            // replace any tensor column reader with a reader that reads from the cache
            var len = (uint)table.DefaultColumnReaders.Length;
            var columnReaders = new ICanRandomlyAccessData[len];
            var existingReaders = table.DefaultColumnReaders;
            for (uint i = 0; i < len; i++) {
                var type = table.ColumnTypes[i];
                if (type == BrightDataType.Vector)
                    _vectorReaders.Add(columnReaders[i] = new VectorReader(lap, table.GetRawColumnData<DataRangeColumnType>(i), _data));
                else
                    columnReaders[i] = existingReaders[i];
            }
            table.SetCustomColumnReaders(columnReaders);
        }

        public void Dispose()
        {
            _data.Dispose();
            foreach(var item in _vectorReaders)
                item.Dispose();
        }
    }
}
