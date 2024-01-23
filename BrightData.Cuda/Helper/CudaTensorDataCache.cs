using System;
using System.Collections.Generic;
using BrightData.Cuda.CudaToolkit.Types;
using BrightData.DataTable;
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

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="lap"></param>
        /// <param name="table"></param>
        public CudaTensorDataCache(CudaLinearAlgebraProvider lap, IDataTable table)
        {
            lap.BindThread();

            // copy entire tensor block into CUDA device
            var tensorData = table.GetTensorData();
            _data = new DeviceMemoryBlock(null, new CudaDeviceVariable<float>(tensorData.Length));
            _data.CopyToDevice(tensorData.Span, 0);
            _lap = lap;

            // update table to use new tensor mappers
            table.SetTensorMappers(GetVectors, GetMatrices, Get3DTensors, Get4DTensors);
        }

        ReadOnlyMemory<ReadOnlyVector> GetVectors(ReadOnlySpan<DataRangeColumnType> span)
        {
            var index = 0;
            var ret = new ReadOnlyVector[span.Length];
            foreach (ref readonly var item in span) {
                var devicePtr = _data.Offset(item.StartIndex, item.Size);
                var segment = new CudaTensorSegment(devicePtr);
                ret[index++] = new(segment);
            }
            return ret;
        }

        ReadOnlyMemory<ReadOnlyMatrix> GetMatrices(ReadOnlySpan<MatrixColumnType> span)
        {
            var index = 0;
            var ret = new ReadOnlyMatrix[span.Length];
            foreach (ref readonly var item in span) {
                var devicePtr = _data.Offset(item.StartIndex, item.Size);
                var segment = new CudaTensorSegment(devicePtr);
                ret[index++] = new(segment, item.RowCount, item.ColumnCount);
            }
            return ret;
        }

        ReadOnlyMemory<ReadOnlyTensor3D> Get3DTensors(ReadOnlySpan<Tensor3DColumnType> span)
        {
            var index = 0;
            var ret = new ReadOnlyTensor3D[span.Length];
            foreach (ref readonly var item in span) {
                var devicePtr = _data.Offset(item.StartIndex, item.Size);
                var segment = new CudaTensorSegment(devicePtr);
                ret[index++] = new(segment, item.Depth, item.RowCount, item.ColumnCount);
            }
            return ret;
        }

        ReadOnlyMemory<ReadOnlyTensor4D> Get4DTensors(ReadOnlySpan<Tensor4DColumnType> span)
        {
            var index = 0;
            var ret = new ReadOnlyTensor4D[span.Length];
            foreach (ref readonly var item in span) {
                var devicePtr = _data.Offset(item.StartIndex, item.Size);
                var segment = new CudaTensorSegment(devicePtr);
                ret[index++] = new(segment, item.Count, item.Depth, item.RowCount, item.ColumnCount);
            }
            return ret;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _data.Dispose();
        }
    }
}
