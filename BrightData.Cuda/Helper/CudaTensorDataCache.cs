using System;
using System.Threading.Tasks;
using BrightData.Cuda.CudaToolkit.Types;
using BrightData.DataTable.Columns;
using BrightData.LinearAlgebra.ReadOnly;

namespace BrightData.Cuda.Helper
{
    /// <summary>
    /// Maintains a cache of CUDA tensors from a data table to avoid copying to device memory each time a tensor is used
    /// </summary>
    public class CudaTensorDataCache : IDisposable
    {
        readonly CudaLinearAlgebraProvider _lap;
        readonly IDeviceMemoryPtr          _data;

        CudaTensorDataCache(CudaLinearAlgebraProvider lap, IDataTable table, ReadOnlyMemory<float> tensorData)
        {
            // copy entire tensor block into CUDA device
            lap.BindThread();
            _data = new DeviceMemoryBlock(null, new CudaDeviceVariable<float>(tensorData.Length));
            _data.CopyToDevice(tensorData.Span, 0);
            _lap = lap;

            // update table to use new tensor mappers
            table.SetTensorMappers(GetVectors, GetMatrices, Get3DTensors, Get4DTensors);
        }

        /// <summary>
        /// Creates a CUDA tensor cache
        /// </summary>
        /// <param name="lap"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        public static async Task<CudaTensorDataCache> Create(CudaLinearAlgebraProvider lap, IDataTable table)
        {
            var tensorData = await table.GetTensorData();
            return new(lap, table, tensorData);
        }

        Task<ReadOnlyMemory<ReadOnlyVector<float>>> GetVectors(ReadOnlyMemory<DataRangeColumnType> block)
        {
            var index = 0;
            var ret = new ReadOnlyVector<float>[block.Length];
            foreach (ref readonly var item in block.Span) {
                var devicePtr = _data.Offset(item.StartIndex, item.Size);
                var segment = new CudaTensorSegment(devicePtr, _lap.Provider);
                ret[index++] = new(segment);
            }
            return Task.FromResult(new ReadOnlyMemory<ReadOnlyVector<float>>(ret));
        }

        Task<ReadOnlyMemory<ReadOnlyMatrix<float>>> GetMatrices(ReadOnlyMemory<MatrixColumnType> block)
        {
            var index = 0;
            var ret = new ReadOnlyMatrix<float>[block.Length];
            foreach (ref readonly var item in block.Span) {
                var devicePtr = _data.Offset(item.StartIndex, item.Size);
                var segment = new CudaTensorSegment(devicePtr, _lap.Provider);
                ret[index++] = new(segment, item.RowCount, item.ColumnCount);
            }
            return Task.FromResult(new ReadOnlyMemory<ReadOnlyMatrix<float>>(ret));
        }

        Task<ReadOnlyMemory<ReadOnlyTensor3D<float>>> Get3DTensors(ReadOnlyMemory<Tensor3DColumnType> block)
        {
            var index = 0;
            var ret = new ReadOnlyTensor3D<float>[block.Length];
            foreach (ref readonly var item in block.Span) {
                var devicePtr = _data.Offset(item.StartIndex, item.Size);
                var segment = new CudaTensorSegment(devicePtr, _lap.Provider);
                ret[index++] = new(segment, item.Depth, item.RowCount, item.ColumnCount);
            }
            return Task.FromResult(new ReadOnlyMemory<ReadOnlyTensor3D<float>>(ret));
        }

        Task<ReadOnlyMemory<ReadOnlyTensor4D<float>>> Get4DTensors(ReadOnlyMemory<Tensor4DColumnType> block)
        {
            var index = 0;
            var ret = new ReadOnlyTensor4D<float>[block.Length];
            foreach (ref readonly var item in block.Span) {
                var devicePtr = _data.Offset(item.StartIndex, item.Size);
                var segment = new CudaTensorSegment(devicePtr, _lap.Provider);
                ret[index++] = new(segment, item.Count, item.Depth, item.RowCount, item.ColumnCount);
            }
            return Task.FromResult(new ReadOnlyMemory<ReadOnlyTensor4D<float>>(ret));
        }

        /// <inheritdoc />
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _data.Dispose();
        }
    }
}
