using BrightWire.LinearAlgebra;
using ManagedCuda;
using ManagedCuda.BasicTypes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BrightWire.Cuda.Helper
{
    /// <summary>
    /// Helper class to represent a list of matrices that are the output of a cuda kernel
    /// </summary>
    class MatrixOutput
    {
        readonly List<IMatrix> _data = new List<IMatrix>();
        readonly int _rows, _columns;
        readonly CUdeviceptr[] _ptr;

        public MatrixOutput(CudaProvider cuda, int rows, int columns, int count, bool setToZero)
        {
            //_cuda = cuda;
            _rows = rows;
            _columns = columns;

            for (var i = 0; i < count; i++) {
                _data.Add(setToZero
                    ? cuda.CreateZeroMatrix(rows, columns)
                    : cuda.CreateMatrix(rows, columns)
                );
            }
            _ptr = _data.Cast<GpuMatrix>().Select(m => m.Memory.DevicePointer).ToArray();
        }

        public void Release()
        {
            foreach(var item in _data)
                item.Dispose();
        }

        public int Rows => _rows;
        public int Columns => _columns;
        public int Count => _data.Count;

        internal CudaDeviceVariable<CUdeviceptr> GetDeviceMemoryPtr()
        {
            var ret = new CudaDeviceVariable<CUdeviceptr>(Count);
            ret.CopyToDevice(_ptr);
            return ret;
        }

        public IMatrix Single()
        {
            return _data.Single();
        }

        public IReadOnlyList<IMatrix> GetAsTensor()
        {
            return _data;
        }
    }
}
