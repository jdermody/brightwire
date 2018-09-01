using BrightWire.Cuda.Helper;
using BrightWire.Models;
using ManagedCuda;
using ManagedCuda.BasicTypes;
using ManagedCuda.CudaBlas;
using ManagedCuda.CudaSolve;
using ManagedCuda.VectorTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using BrightWire.CUDA.Source.Helper;
using BrightWire.Helper;

namespace BrightWire.LinearAlgebra
{
    /// <summary>
    /// Manages the bright wire cuda kernels and implements the cuda linear algebra provider
    /// </summary>
    internal class CudaProvider : ILinearAlgebraProvider, IGpuLinearAlgebraProvider
    {
        const int BLOCK_DIM = 16;
        const int BLOCK_DIM2 = BLOCK_DIM * BLOCK_DIM;
	    const int PTR_SIZE = 8;

        class KernelExecution
        {
            readonly CUfunction _function;
            readonly dim3 _block;
            readonly dim3 _thread;

            public KernelExecution(CUfunction function, dim3 block, dim3 thread)
            {
                _function = function;
                _block = block;
                _thread = thread;
            }

            public void Run(uint sharedMemSize, object[] param)
            {
                var paramList = new IntPtr[param.Length];
                var handleList = new GCHandle[param.Length];

                //Get pointers to kernel parameters
                for (int i = 0; i < param.Length; i++) {
                    handleList[i] = GCHandle.Alloc(param[i], GCHandleType.Pinned);
                    paramList[i] = handleList[i].AddrOfPinnedObject();
                }

                DriverAPINativeMethods.Launch.cuLaunchKernel(_function,
                    _block.x, _block.y, _block.z,
                    _thread.x, _thread.y, _thread.z,
                    sharedMemSize,
                    new CUstream(),
                    paramList,
                    null
                );

                // free the handles
                for (int i = 0; i < param.Length; i++)
                    handleList[i].Free();
            }
        }

        class KernelModule
        {
            //readonly CudaContext _context;
            readonly CUmodule _module;

            public KernelModule(CudaContext context, string path)
            {
                //_context = context;
                _module = context.LoadModule(path);
            }

            public CUfunction LoadFunction(string name)
            {
                CUfunction ret = new CUfunction();
                if (DriverAPINativeMethods.ModuleManagement.cuModuleGetFunction(ref ret, _module, name) != CUResult.Success)
                    throw new ArgumentException("Function not found", name);
                return ret;
            }

            public KernelExecution CreateExecution(CUfunction function, dim3 block, dim3 thread)
            {
                return new KernelExecution(function, block, thread);
            }
        }

	    readonly CudaContext _cuda;
        readonly CudaBlas _blas;
        readonly Lazy<CudaSolveDense> _solver = new Lazy<CudaSolveDense>();
        readonly KernelModule _kernel;
        readonly ILinearAlgebraProvider _numerics = BrightWireProvider.CreateLinearAlgebra();
        readonly DeviceMemory _cache;
	    readonly int _multiProcessorCount, _maxThreadsPerBlock, _warpSize, _numRegistersPerBlock, _sharedMemoryPerBlock;
	    readonly dim3 _maxGridDim, _maxBlockDim;
        readonly CUfunction
            _pointwiseMultiply,
            _addInPlace,
            _subtractInPlace,
            _addToEachRow,
            _addToEachColumn,
            _tanh,
            _tanhDerivative,
            _sigmoid,
            _sigmoidDerivative,
            _sumRows,
            _relu,
            _reluDerivative,
            _leakyRelu,
            _leakyReluDerivative,
            _memClear,
            _sumColumns,
            _pointwiseDivide,
            _sqrt,
            _findMinAndMax,
            _findSum,
            _findStdDev,
            _constrain,
            _pow,
            _diagonal,
            _l1Regularisation,
            _pointwiseDivideRows,
            _pointwiseDivideColumns,
            _splitRows,
            _splitColumns,
            _concatRows,
            _concatColumns,
            _euclideanDistance,
            _manhattanDistance,
            _abs,
            _normalise,
            _softmaxVector,
            _multiEuclidean,
            _multiManhattan,
            _log,
            _vectorAdd,
            _vectorCopyRandom,
            _copyToMatrix,
            _vectorSplit,
            _tensorConvertToVector,
            _tensorConvertToMatrix,
            _tensorAddPadding,
            _tensorRemovePadding,
            _tensorIm2Col,
            _softmaxDerivative,
            _reverse,
            _rotate,
            _tensorMaxPool,
            _tensorReverseMaxPool,
            _tensorReverseIm2Col
        ;
        bool _disposed = false;

        public CudaProvider(string cudaKernelPath, bool stochastic, int memoryCacheSize)
        {
            IsStochastic = stochastic;
            _cache = new DeviceMemory(memoryCacheSize);
            _cuda = new CudaContext();
            _kernel = new KernelModule(_cuda, cudaKernelPath);
            _blas = new CudaBlas(AtomicsMode.Allowed);

	        var deviceInfo = _cuda.GetDeviceInfo();
	        _multiProcessorCount = deviceInfo.MultiProcessorCount;
	        _maxThreadsPerBlock = deviceInfo.MaxThreadsPerBlock;
	        _maxGridDim = deviceInfo.MaxGridDim;
	        _maxBlockDim = deviceInfo.MaxBlockDim;
	        _warpSize = deviceInfo.WarpSize;
	        _numRegistersPerBlock = deviceInfo.RegistersPerBlock;
	        _sharedMemoryPerBlock = deviceInfo.SharedMemoryPerBlock;
            _cuda.SetCurrent();

            _pointwiseMultiply = _kernel.LoadFunction("PointwiseMultiply");
            _addInPlace = _kernel.LoadFunction("AddInPlace");
            _subtractInPlace = _kernel.LoadFunction("SubtractInPlace");
            _addToEachRow = _kernel.LoadFunction("AddToEachRow");
            _addToEachColumn = _kernel.LoadFunction("AddToEachColumn");
            _tanh = _kernel.LoadFunction("TanH");
            _tanhDerivative = _kernel.LoadFunction("TanHDerivative");
            _sigmoid = _kernel.LoadFunction("Sigmoid");
            _sigmoidDerivative = _kernel.LoadFunction("SigmoidDerivative");
            _sumRows = _kernel.LoadFunction("SumRows");
            _relu = _kernel.LoadFunction("RELU");
            _reluDerivative = _kernel.LoadFunction("RELUDerivative");
            _memClear = _kernel.LoadFunction("MemClear");
            _sumColumns = _kernel.LoadFunction("SumColumns");
            _pointwiseDivide = _kernel.LoadFunction("PointwiseDivide");
            _sqrt = _kernel.LoadFunction("Sqrt");
            _findMinAndMax = _kernel.LoadFunction("FindMinAndMax");
            _findSum = _kernel.LoadFunction("FindSum");
            _findStdDev = _kernel.LoadFunction("FindStdDev");
            _constrain = _kernel.LoadFunction("Constrain");
            _pow = _kernel.LoadFunction("Pow");
            _diagonal = _kernel.LoadFunction("Diagonal");
            _l1Regularisation = _kernel.LoadFunction("L1Regularisation");
            _leakyRelu = _kernel.LoadFunction("LeakyRELU");
            _leakyReluDerivative = _kernel.LoadFunction("LeakyRELUDerivative");
            _pointwiseDivideRows = _kernel.LoadFunction("PointwiseDivideRows");
            _pointwiseDivideColumns = _kernel.LoadFunction("PointwiseDivideColumns");
            _splitRows = _kernel.LoadFunction("SplitRows");
            _splitColumns = _kernel.LoadFunction("SplitColumns");
            _concatRows = _kernel.LoadFunction("ConcatRows");
            _concatColumns = _kernel.LoadFunction("ConcatColumns");
            _euclideanDistance = _kernel.LoadFunction("EuclideanDistance");
            _manhattanDistance = _kernel.LoadFunction("ManhattanDistance");
            _abs = _kernel.LoadFunction("Abs");
            _normalise = _kernel.LoadFunction("Normalise");
            _softmaxVector = _kernel.LoadFunction("SoftmaxVector");
            _multiEuclidean = _kernel.LoadFunction("MultiEuclideanDistance");
            _multiManhattan = _kernel.LoadFunction("MultiManhattanDistance");
            _log = _kernel.LoadFunction("Log");
            _vectorAdd = _kernel.LoadFunction("VectorAdd");
            _vectorCopyRandom = _kernel.LoadFunction("VectorCopyRandom");
            _copyToMatrix = _kernel.LoadFunction("CopyToMatrix");
            _vectorSplit = _kernel.LoadFunction("VectorSplit");
            _tensorConvertToVector = _kernel.LoadFunction("TensorConvertToVector");
            _tensorConvertToMatrix = _kernel.LoadFunction("TensorConvertToMatrix");
            _tensorAddPadding = _kernel.LoadFunction("TensorAddPadding");
            _tensorRemovePadding = _kernel.LoadFunction("TensorRemovePadding");
            _tensorIm2Col = _kernel.LoadFunction("TensorIm2Col");
            _softmaxDerivative = _kernel.LoadFunction("SoftmaxDerivative");
            _reverse = _kernel.LoadFunction("Reverse");
            _rotate = _kernel.LoadFunction("Rotate");
            _tensorMaxPool = _kernel.LoadFunction("TensorMaxPool");
            _tensorReverseMaxPool = _kernel.LoadFunction("TensorReverseMaxPool");
            _tensorReverseIm2Col = _kernel.LoadFunction("TensorReverseIm2Col"); 
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_disposed) {
                _blas.Dispose();
                _cuda.Dispose();
                _cache.Dispose();
                //if(_solver.IsValueCreated)
                //    _solver.Value.Dispose();
                _numerics.Dispose();
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public ILinearAlgebraProvider NumericsProvider => _numerics;
        public bool IsStochastic { get; }

	    public bool IsGpu => true;
        internal CudaContext Context => _cuda;
        internal CudaBlas Blas => _blas;
	    public CudaSolveDense Solver => _solver.Value;
	    public long TotalMemory => _cuda.GetTotalDeviceMemorySize();
	    public long FreeMemory => _cuda.GetFreeDeviceMemorySize();

		public void Register(IDisposable disposable) => _cache.Add(disposable);

	    int _GetBlockCount(int size, int blockSize)
        {
            return ((size / blockSize) + 1);
        }

        void _Invoke2(CUfunction function, int rows, int columns, params object[] param)
        {
            var x = _GetBlockCount(rows, BLOCK_DIM);
            var y = _GetBlockCount(columns, BLOCK_DIM);
	        var execution = _kernel.CreateExecution(function, new dim3(x, y), new dim3(BLOCK_DIM, BLOCK_DIM));
	        execution.Run(0, param);
	        //_cuda.Synchronize();
        }

        void _Invoke(CUfunction function, int size, params object[] param)
        {
            var x = _GetBlockCount(size, BLOCK_DIM2);
	        var execution = _kernel.CreateExecution(function, x, BLOCK_DIM2);
	        execution.Run(0, param);
        }

	    void _InvokeWithSharedMemory(CUfunction function, int size, uint sharedMemorySize, params object[] param)
	    {
		    var x = _GetBlockCount(size, BLOCK_DIM2);
		    var execution = _kernel.CreateExecution(function, x, BLOCK_DIM2);
		    execution.Run(sharedMemorySize, param);
	    }

        internal IDeviceMemoryPtr PointwiseMultiply(IDeviceMemoryPtr a, IDeviceMemoryPtr b, int size)
        {
            var ret = Allocate(size);
            ret.CopyToDevice(b);
	        _Invoke(_pointwiseMultiply, size, a.DevicePointer, ret.DevicePointer, size);
            return ret;
        }

        internal IDeviceMemoryPtr PointwiseDivide(IDeviceMemoryPtr a, IDeviceMemoryPtr b, int size)
        {
            var ret = Allocate(size);
            ret.CopyToDevice(b);
	        _Invoke(_pointwiseDivide, size, a.DevicePointer, ret.DevicePointer, size);
            return ret;
        }

        internal void AddInPlace(IDeviceMemoryPtr a, IDeviceMemoryPtr b, int size, float coefficient1, float coefficient2)
        {
	        _Invoke(_addInPlace, size, a.DevicePointer, b.DevicePointer, size, coefficient1, coefficient2);
        }

        internal void SubtractInPlace(IDeviceMemoryPtr a, IDeviceMemoryPtr b, int size, float coefficient1, float coefficient2)
        {
	        _Invoke(_subtractInPlace, size, a.DevicePointer, b.DevicePointer, size, coefficient1, coefficient2);
        }

        internal void AddToEachRow(IDeviceMemoryPtr matrix, IDeviceMemoryPtr vector, int rows, int columns)
        {
	        _Invoke2(_addToEachRow, rows, columns, matrix.DevicePointer, vector.DevicePointer, rows, columns);
        }

        internal void AddToEachColumn(IDeviceMemoryPtr matrix, IDeviceMemoryPtr vector, int rows, int columns)
        {
	        _Invoke2(_addToEachColumn, rows, columns, matrix.DevicePointer, vector.DevicePointer, rows, columns);
        }

        internal IDeviceMemoryPtr TanH(IDeviceMemoryPtr a, int size)
        {
            var ret = Allocate(size);
	        _Invoke(_tanh, size, a.DevicePointer, ret.DevicePointer, size);
            return ret;
        }

        internal IDeviceMemoryPtr TanHDerivative(IDeviceMemoryPtr a, int size)
        {
            var ret = Allocate(size);
	        _Invoke(_tanhDerivative, size, a.DevicePointer, ret.DevicePointer, size);
            return ret;
        }

        internal IDeviceMemoryPtr Sigmoid(IDeviceMemoryPtr a, int size)
        {
            var ret = Allocate(size);
	        _Invoke(_sigmoid, size, a.DevicePointer, ret.DevicePointer, size);
            return ret;
        }

        internal IDeviceMemoryPtr SigmoidDerivative(IDeviceMemoryPtr a, int size)
        {
            var ret = Allocate(size);
	        _Invoke(_sigmoidDerivative, size, a.DevicePointer, ret.DevicePointer, size);
            return ret;
        }

        internal IDeviceMemoryPtr RELU(IDeviceMemoryPtr a, int size)
        {
            var ret = Allocate(size);
	        _Invoke(_relu, size, a.DevicePointer, ret.DevicePointer, size);
            return ret;
        }

        internal IDeviceMemoryPtr RELUDerivative(IDeviceMemoryPtr a, int size)
        {
            var ret = Allocate(size);
	        _Invoke(_reluDerivative, size, a.DevicePointer, ret.DevicePointer, size);
            return ret;
        }

        internal IDeviceMemoryPtr LeakyRELU(IDeviceMemoryPtr a, int size)
        {
            var ret = Allocate(size);
	        _Invoke(_leakyRelu, size, a.DevicePointer, ret.DevicePointer, size);
            return ret;
        }

        internal IDeviceMemoryPtr LeakyRELUDerivative(IDeviceMemoryPtr a, int size)
        {
            var ret = Allocate(size);
	        _Invoke(_leakyReluDerivative, size, a.DevicePointer, ret.DevicePointer, size);
            return ret;
        }

        internal IDeviceMemoryPtr SumRows(IDeviceMemoryPtr a, int rows, int columns)
        {
            var ret = Allocate(rows);
	        _Invoke2(_sumRows, rows, columns, a.DevicePointer, ret.DevicePointer, rows, columns);
            return ret;
        }

        internal IDeviceMemoryPtr SumColumns(IDeviceMemoryPtr a, int rows, int columns)
        {
            var ret = Allocate(columns);
	        _Invoke(_sumColumns, columns, a.DevicePointer, ret.DevicePointer, rows, columns);
            return ret;
        }

        internal void MemClear(IDeviceMemoryPtr data, int count, int offset = 0, int increment = 1)
        {
	        _Invoke(_memClear, count, data.DevicePointer, count, offset, increment);
        }

        internal IDeviceMemoryPtr Sqrt(IDeviceMemoryPtr a, int size, float valueAdjustment)
        {
            var ret = Allocate(size);
	        _Invoke(_sqrt, size, a.DevicePointer, ret.DevicePointer, size, valueAdjustment);
            return ret;
        }

        internal IDeviceMemoryPtr Abs(IDeviceMemoryPtr a, int size)
        {
            var ret = Allocate(size);
	        _Invoke(_abs, size, a.DevicePointer, ret.DevicePointer, size);
            return ret;
        }

        internal IDeviceMemoryPtr Log(IDeviceMemoryPtr a, int size)
        {
            var ret = Allocate(size);
	        _Invoke(_log, size, a.DevicePointer, ret.DevicePointer, size);
            return ret;
        }

        internal void VectorAdd(IDeviceMemoryPtr a, int size, float scalar)
        {
	        _Invoke(_vectorAdd, size, a.DevicePointer, size, scalar);
        }

        internal IDeviceMemoryPtr VectorCopy(IDeviceMemoryPtr a, int size, int[] indexList)
        {
            var retSize = indexList.Length;
            var ret = Allocate(retSize);
            using (var indexGpu = new CudaDeviceVariable<int>(retSize)) {
                indexGpu.CopyToDevice(indexList);
	            _Invoke(_vectorCopyRandom, retSize, a.DevicePointer, ret.DevicePointer, indexGpu.DevicePointer, retSize);
                return ret;
            }
        }

        internal (float Min, float Max) FindMinAndMax(IDeviceMemoryPtr a, int size)
        {
            if (size > 0) {
                var ptr = a;
                while (size > BLOCK_DIM2) {
                    var bufferSize = (size / BLOCK_DIM2) + 1;
                    var minBlock = Allocate(bufferSize, true);
                    var maxBlock = Allocate(bufferSize, true);

                    try {
	                    _InvokeWithSharedMemory(_findMinAndMax, size, BLOCK_DIM2, ptr.DevicePointer, size, minBlock.DevicePointer, maxBlock.DevicePointer);
                        if (ptr != a)
                            ptr.Free();
                        //var minTest = new float[bufferSize];
                        //var maxText = new float[bufferSize];
                        //minBlock.CopyToHost(minTest);
                        //maxBlock.CopyToHost(maxText);
                        size = bufferSize * 2;
                        ptr = Allocate(size);
                        ptr.DeviceVariable.CopyToDevice(minBlock.DeviceVariable, 0, 0, bufferSize * sizeof(float));
                        ptr.DeviceVariable.CopyToDevice(maxBlock.DeviceVariable, 0, bufferSize * sizeof(float), bufferSize * sizeof(float));
                    }
                    finally {
                        minBlock.Free();
                        maxBlock.Free();
                    }
                }
                var data = new float[size];
                ptr.CopyToHost(data);
                float min = float.MaxValue, max = float.MinValue;
                for(var i = 0; i < size; i++) {
                    var val = data[i];
                    if (val > max)
                        max = val;
                    if (val < min)
                        min = val;
                }
                return (min, max);
            }
            return (0f, 0f);
        }

        internal float SumValues(IDeviceMemoryPtr a, int size)
        {
            var ptr = a;
            while (size > BLOCK_DIM2) {
                var bufferSize = (size / BLOCK_DIM2) + 1;
                var sumBlock = Allocate(bufferSize);
	            _InvokeWithSharedMemory(_findSum, size, BLOCK_DIM2, ptr.DevicePointer, size, sumBlock.DevicePointer);
                if (ptr != a)
                    ptr.Free();
                size = bufferSize;
                ptr = sumBlock;
            }
            var total = new float[size];
            ptr.CopyToHost(total);
            if (ptr != a)
                ptr.Free();
            return total.Sum();
        }

        internal float FindStdDev(IDeviceMemoryPtr a, int size, float mean)
        {
            var inputSize = size;
            if (size > 0) {
                var ptr = a;
                while(size > BLOCK_DIM2) {
                    var bufferSize = (size / BLOCK_DIM2) + 1;
                    var sumBlock = Allocate(bufferSize);
	                _InvokeWithSharedMemory(_findStdDev, size, BLOCK_DIM2, ptr.DevicePointer, size, mean, sumBlock.DevicePointer);
                    if (ptr != a)
                        ptr.Free();
                    size = bufferSize;
                    ptr = sumBlock;
                }
                var total = new float[size];
                ptr.CopyToHost(total);
                if (ptr != a)
                    ptr.Free();

                return Convert.ToSingle(Math.Sqrt(total.Sum() / inputSize));
            }
            return 0f;
        }

        internal void Constrain(IDeviceMemoryPtr a, int size, float min, float max)
        {
	        _Invoke(_constrain, size, a.DevicePointer, size, min, max);
        }

        internal IDeviceMemoryPtr Pow(IDeviceMemoryPtr a, int size, float power)
        {
            var ret = Allocate(size);
	        _Invoke(_pow, size, a.DevicePointer, ret.DevicePointer, size, power);
            return ret;
        }

        internal IDeviceMemoryPtr Diagonal(IDeviceMemoryPtr a, int rows, int columns)
        {
            var len = Math.Min(rows, columns);
            var ret = Allocate(len);
	        _Invoke(_diagonal, len, a.DevicePointer, ret.DevicePointer, rows, columns);
            return ret;
        }

        internal void L1Regularisation(IDeviceMemoryPtr a, int size, float coefficient)
        {
	        _Invoke(_l1Regularisation, size, a.DevicePointer, size, coefficient);
        }

        internal float EuclideanDistance(IDeviceMemoryPtr a, IDeviceMemoryPtr b, int size)
        {
            var ret = Allocate(size);
	        _Invoke(_euclideanDistance, size, a.DevicePointer, b.DevicePointer, ret.DevicePointer, size);
            return Convert.ToSingle(Math.Sqrt(SumValues(ret, size)));
        }

        internal float ManhattanDistance(IDeviceMemoryPtr a, IDeviceMemoryPtr b, int size)
        {
            var ret = Allocate(size);
	        _Invoke(_manhattanDistance, size, a.DevicePointer, b.DevicePointer, ret.DevicePointer, size);
            return SumValues(ret, size);
        }

        internal void Normalise(IDeviceMemoryPtr a, int size, float min, float range)
        {
	        _Invoke(_normalise, size, a.DevicePointer, size, min, range);
        }

        internal IDeviceMemoryPtr SoftmaxVector(IDeviceMemoryPtr a, int size, float max)
        {
            var ret = Allocate(size);
	        _Invoke(_softmaxVector, size, a.DevicePointer, ret.DevicePointer, size, max);
            return ret;
        }

        internal void VectorSplit(IDeviceMemoryPtr a, int size, int blockSize, CUdeviceptr output)
        {
	        _Invoke(_vectorSplit, size, a.DevicePointer, output, size, blockSize);
        }

        internal void PointwiseDivideRows(IDeviceMemoryPtr a, IDeviceMemoryPtr b, int rows, int columns)
        {
	        _Invoke2(_pointwiseDivideRows, rows, columns, a.DevicePointer, b.DevicePointer, rows, columns);
        }

        internal void PointwiseDivideColumns(IDeviceMemoryPtr a, IDeviceMemoryPtr b, int rows, int columns)
        {
	        _Invoke2(_pointwiseDivideColumns, rows, columns, a.DevicePointer, b.DevicePointer, rows, columns);
        }

        internal void SplitRows(IDeviceMemoryPtr a, IDeviceMemoryPtr b, IDeviceMemoryPtr c, int rows, int columns, int position)
        {
	        _Invoke2(_splitRows, rows, columns, a.DevicePointer, b.DevicePointer, c.DevicePointer, rows, columns, position);
        }

        internal void SplitColumns(IDeviceMemoryPtr a, IDeviceMemoryPtr b, IDeviceMemoryPtr c, int rows, int columns, int position)
        {
	        _Invoke2(_splitColumns, rows, columns, a.DevicePointer, b.DevicePointer, c.DevicePointer, rows, columns, position);
        }

        internal void ConcatRows(IDeviceMemoryPtr a, IDeviceMemoryPtr b, IDeviceMemoryPtr c, int rows, int columns, int leftColumnCount)
        {
	        _Invoke2(_concatRows, rows, columns, a.DevicePointer, b.DevicePointer, c.DevicePointer, rows, columns, leftColumnCount);
        }

        internal void ConcatColumns(IDeviceMemoryPtr a, IDeviceMemoryPtr b, IDeviceMemoryPtr c, int rows, int columns, int topRowCount, int bottomRowCount)
        {
	        _Invoke2(_concatColumns, rows, columns, a.DevicePointer, b.DevicePointer, c.DevicePointer, rows, columns, topRowCount, bottomRowCount);
        }

        internal IDeviceMemoryPtr MultiEuclideanDistance(IDeviceMemoryPtr vector, CUdeviceptr[] compareTo, int size)
        {
            IDeviceMemoryPtr ret = null;
            var buffer = Allocate(PTR_SIZE * compareTo.Length);
            try {
                _cuda.CopyToDevice(buffer.DevicePointer, compareTo);
                ret = Allocate(size * compareTo.Length);
	            _Invoke2(_multiEuclidean, size, compareTo.Length, vector.DevicePointer, buffer.DevicePointer, ret.DevicePointer, size, compareTo.Length);
            }
            finally {
	            buffer.Free();
            }
            return ret;
        }

        internal IDeviceMemoryPtr MultiManhattanDistance(IDeviceMemoryPtr vector, CUdeviceptr[] compareTo, int size)
        {
            IDeviceMemoryPtr ret = null;
            var buffer = Allocate(PTR_SIZE * compareTo.Length);
            try {
                _cuda.CopyToDevice(buffer.DevicePointer, compareTo);
                ret = Allocate(size * compareTo.Length);
	            _Invoke2(_multiManhattan, size, compareTo.Length, vector.DevicePointer, buffer.DevicePointer, ret.DevicePointer, size, compareTo.Length);
            }
            finally {
	            buffer.Free();
            }
            return ret;
        }

        internal IDeviceMemoryPtr TensorConvertToVector(IReadOnlyList<IDeviceMemoryPtr> matrixList, int matrixSize)
        {
            var outputSize = matrixList.Count * matrixSize;
            var ret = Allocate(outputSize);
            using (var ptrList = new DeviceMemoryPtrList(matrixList)) {
	            _Invoke(_tensorConvertToVector, outputSize, ptrList.DevicePointer, ret.DevicePointer, matrixSize, outputSize);
            }
            return ret;
        }

        internal void TensorConvertToMatrix(IReadOnlyList<IDeviceMemoryPtr> matrixList, int tensorRows, int tensorColumns, int matrixRows, int matrixColumns, IDeviceMemoryPtr ret)
        {
            using (var ptrList = new DeviceMemoryPtrList(matrixList)) {
	            _Invoke2(_tensorConvertToMatrix, matrixRows, matrixColumns, ptrList.DevicePointer, ret.DevicePointer, tensorRows, tensorColumns, matrixRows, matrixColumns);
            }
        }

        internal Tensor4DOutput TensorAddPadding(Tensor4DInput tensor, int padding)
        {
            var count = tensor.Count;
            var depth = tensor.Depth;
            var rows = tensor.Rows;
            var columns = tensor.Columns;
            var newRows = tensor.Rows + padding * 2;
            var newColumns = tensor.Columns + padding * 2;
            var ret = new Tensor4DOutput(this, newRows, newColumns, depth, count, true);

            try {
                using (var output = ret.GetDeviceMemoryPtr())
                using (var input = tensor.GetDeviceMemoryPtr()) {
	                var size = newRows * depth * count;
	                _Invoke2(_tensorAddPadding, newRows * depth * count, newColumns, size, input.DevicePointer, output.DevicePointer, count, rows, columns, newRows, newColumns, depth, padding);
                }
            }catch {
                ret.Release();
                throw;
            }
            return ret;
        }

        internal Tensor4DOutput TensorRemovePadding(Tensor4DInput tensor, int padding)
        {
            var count = tensor.Count;
            int depth = tensor.Depth;
            var rows = tensor.Rows;
            var columns = tensor.Columns;
            var newRows = tensor.Rows - padding * 2;
            var newColumns = tensor.Columns - padding * 2;
            var ret = new Tensor4DOutput(this, newRows, newColumns, depth, tensor.Count, false);

            try {
                using (var output = ret.GetDeviceMemoryPtr())
                using (var input = tensor.GetDeviceMemoryPtr()) {
	                var size = rows * depth * count;
	                _Invoke2(_tensorRemovePadding, size, columns, size, input.DevicePointer, output.DevicePointer, count, rows, columns, newRows, newColumns, depth, padding);
                }
            }catch {
                ret.Release();
                throw;
            }
            return ret;
        }

        internal IDeviceMemoryPtr VectorSoftmaxDerivative(IDeviceMemoryPtr a, int size)
        {
            var ret = Allocate(size * size);
	        _Invoke2(_softmaxDerivative, size, size, a.DevicePointer, ret.DevicePointer, size);
            return ret;
        }

        internal IDeviceMemoryPtr Reverse(IDeviceMemoryPtr a, int size)
        {
            var ret = Allocate(size);
	        _Invoke(_reverse, size, a.DevicePointer, ret.DevicePointer, size);
            return ret;
        }

        internal IDeviceMemoryPtr Rotate(IDeviceMemoryPtr a, int size, int blockCount)
        {
            var blockSize = size / blockCount;
            var ret = Allocate(size);
	        _Invoke(_rotate, size, a.DevicePointer, ret.DevicePointer, size, blockCount, blockSize);
            return ret;
        }

        internal IReadOnlyList<(IDeviceMemoryPtr, object, object)> TensorMaxPool(
            IReadOnlyList<IDeviceMemoryPtr> matrixList, 
            int rows, 
            int columns, 
            int filterWidth, 
            int filterHeight, 
            int stride,
            bool calculateIndex)
        {
            var newColumns = (columns - filterWidth) / stride + 1;
            var newRows = (rows - filterHeight) / stride + 1;
            var size = newColumns * newRows;
            var depth = matrixList.Count;
            var ret = new List<IDeviceMemoryPtr>();

            var xIndex = new List<CudaDeviceVariable<int>>();
            var yIndex = new List<CudaDeviceVariable<int>>();
            for (var i = 0; i < depth; i++) {
                ret.Add(Allocate(size));
                xIndex.Add(calculateIndex ? new CudaDeviceVariable<int>(size) : CudaDeviceVariable<int>.Null);
                yIndex.Add(calculateIndex ? new CudaDeviceVariable<int>(size) : CudaDeviceVariable<int>.Null);
            }
            using (var outputDevicePtr = new CudaDeviceVariable<CUdeviceptr>(depth))
            using (var inputDevicePtr = new CudaDeviceVariable<CUdeviceptr>(depth))
            using (var xIndexPtr = new CudaDeviceVariable<CUdeviceptr>(depth))
            using (var yIndexPtr = new CudaDeviceVariable<CUdeviceptr>(depth)) {
                inputDevicePtr.CopyToDevice(matrixList.Select(m => m.DevicePointer).ToArray());
                outputDevicePtr.CopyToDevice(ret.Select(m => m.DevicePointer).ToArray());
                xIndexPtr.CopyToDevice(xIndex.Select(m => m.DevicePointer).ToArray());
                yIndexPtr.CopyToDevice(yIndex.Select(m => m.DevicePointer).ToArray());
	            int size2 = newRows * depth;
	            _Invoke2(_tensorMaxPool, size2, newColumns, size2, inputDevicePtr.DevicePointer, outputDevicePtr.DevicePointer, xIndexPtr.DevicePointer, yIndexPtr.DevicePointer, rows, columns, depth, newRows, newColumns, filterWidth, filterHeight, stride);
            }
            if (calculateIndex) {
                return ret.Select((d, i) => {
                    return (d, (object)xIndex[i], (object)yIndex[i]);
                }).ToList();
            }else {
                return ret.Select((d, i) => {
                    return (d, (object)null, (object)null);
                }).ToList();
            }
        }

        internal IReadOnlyList<IDeviceMemoryPtr> TensorReverseMaxPool(IReadOnlyList<IDeviceMemoryPtr> matrixList, int rows, int columns, int newRows, int newColumns, IReadOnlyList<(object X, object Y)> indexList)
        {
            var size = newColumns * newRows;
            var depth = matrixList.Count;
            var ret = new List<IDeviceMemoryPtr>();
            var xIndex = new List<CudaDeviceVariable<int>>();
            var yIndex = new List<CudaDeviceVariable<int>>();
            for (var i = 0; i < depth; i++) {
                ret.Add(Allocate(size, true));
                var data = indexList[i];
                xIndex.Add((CudaDeviceVariable<int>)data.X);
                yIndex.Add((CudaDeviceVariable<int>)data.Y);
            }
            using (var outputDevicePtr = new CudaDeviceVariable<CUdeviceptr>(depth))
            using (var inputDevicePtr = new CudaDeviceVariable<CUdeviceptr>(depth))
            using (var xIndexPtr = new CudaDeviceVariable<CUdeviceptr>(depth))
            using (var yIndexPtr = new CudaDeviceVariable<CUdeviceptr>(depth)) {
                inputDevicePtr.CopyToDevice(matrixList.Select(m => m.DevicePointer).ToArray());
                outputDevicePtr.CopyToDevice(ret.Select(m => m.DevicePointer).ToArray());
                xIndexPtr.CopyToDevice(xIndex.Select(m => m.DevicePointer).ToArray());
                yIndexPtr.CopyToDevice(yIndex.Select(m => m.DevicePointer).ToArray());
	            int size2 = rows * depth;
	            _Invoke2(_tensorReverseMaxPool, size2, columns, size2, inputDevicePtr.DevicePointer, outputDevicePtr.DevicePointer, xIndexPtr.DevicePointer, yIndexPtr.DevicePointer, rows, columns, depth, newRows, newColumns);
            }
            for (var i = 0; i < depth; i++) {
                xIndex[i].Dispose();
                yIndex[i].Dispose();
            }
            return ret;
        }

	    internal IMatrix TensorIm2Col(Tensor3DInput tensor, int filterWidth, int filterHeight, int stride)
	    {
		    using(var tensorPtr = tensor.GetDeviceMemoryPtr())
		    using(var convolutions = new ConvolutionsData(this, ConvolutionHelper.Default(tensor.Columns, tensor.Rows, filterWidth, filterHeight, stride))) {
			    var filterSize = filterWidth * filterHeight;
			    var columns = filterSize * tensor.Depth;
			    var ret = (GpuMatrix)CreateZeroMatrix(convolutions.Count, columns);
			    var size = convolutions.Count * columns;
			    _Invoke(_tensorIm2Col, size, 
				    size,
				    tensorPtr.DevicePointer, 
				    ret.Memory.DevicePointer, 
				    convolutions.X.DevicePointer,
				    convolutions.Y.DevicePointer,
				    tensor.Rows,
				    convolutions.Count,
				    filterSize,
				    tensor.Depth,
				    filterWidth, 
				    filterHeight
			    );
			    return ret;
		    }
	    }

	    internal IMatrix TensorReverseIm2Col(Tensor3DInput tensor, IReadOnlyList<IReadOnlyList<IDeviceMemoryPtr>> filterList, int inputHeight, int inputWidth, int padding, int filterHeight, int filterWidth, int stride)
	    {
		    var outputRows = inputHeight + padding * 2;
		    var outputColumns = inputWidth + padding * 2;
		    var numFilters = filterList[0].Count;

		    var ret = new Tensor3DOutput(this, outputRows, outputColumns, numFilters * tensor.Depth, true);

		    using(var ptr = ret.GetDeviceMemoryPtr())
		    using(var tensorPtr = tensor.GetDeviceMemoryPtr())
		    using(var filterPtr = new DeviceMemoryPtrToPtrList(filterList))
		    using (var convolutions = new ConvolutionsData(this, ConvolutionHelper.Default(outputColumns, outputRows, filterWidth, filterHeight, stride))) {
			    var size = tensor.Depth * convolutions.Count * filterHeight * filterWidth * numFilters;
			    _Invoke(_tensorReverseIm2Col, size,
				    size,
				    tensorPtr.DevicePointer,
				    filterPtr.DevicePointer,
				    ptr.DevicePointer,
				    convolutions.X.DevicePointer,
				    convolutions.Y.DevicePointer,
				    tensor.Rows,
				    tensor.Columns,
				    convolutions.Count,
				    tensor.Depth,
				    filterWidth,
				    filterHeight,
				    stride,
				    numFilters,
				    outputRows,
				    outputColumns
			    );
		    }

		    return ret.GetAsMatrix();

	        //return ret.GetAsMatrix();
	        //   var rows = tensor.Rows;
         //   var columns = tensor.Columns;
         //   var count = tensor.Count;
         //   var newColumns = inputHeight + padding * 2;
         //   var newRows = inputWidth + padding * 2;
         //   var newSize = newColumns * newRows;
         //   var depth = tensor.Depth;
	        //var filterCount = filterList.Count;

         //   var ret = new Tensor4DOutput(this, newRows * newColumns, inputDepth, depth, count, true);
         //   var filterList2 = new List<CudaDeviceVariable<CUdeviceptr>>();
         //   for (var i = 0; i < filterCount; i++) {
         //       var filters = filterList[i];
         //       var filterDevicePtr = new CudaDeviceVariable<CUdeviceptr>(inputDepth);
         //       filterDevicePtr.CopyToDevice(filters.Select(m => m.DevicePointer).ToArray());
         //       filterList2.Add(filterDevicePtr);
         //   }
         //   using (var output = ret.GetDeviceMemoryPtr())
         //   using (var input = tensor.GetDeviceMemoryPtr())
         //   using (var filterDevicePtr = new CudaDeviceVariable<CUdeviceptr>(filterCount)) {
         //       filterDevicePtr.CopyToDevice(filterList2.Select(m => m.DevicePointer).ToArray());
	        //    _Invoke2(_tensorReverseIm2Col, rows * depth * inputDepth * count, columns, 
         //           input.DevicePointer, 
         //           filterDevicePtr.DevicePointer, 
         //           output.DevicePointer,
         //           count,
         //           rows,
         //           columns, 
         //           depth, 
         //           newRows,
         //           newSize,
         //           inputDepth,
         //           filterHeight, 
         //           filterWidth, 
         //           stride
         //       );
         //   }
         //   foreach (var item in filterList2)
         //       item.Dispose();
         //   return ret;
        }

        public IVector CreateVector(IEnumerable<float> data)
        {
            var list = data.ToList();
            return CreateVector(list.Count, i => list[i]);
        }

        public IVector CreateVector(int length, Func<int, float> init)
        {
            return new GpuVector(this, length, init);
        }

        public IMatrix CreateMatrix(IReadOnlyList<IVector> vectorData)
        {
            int rows = vectorData.Count;
            int columns = vectorData[0].Count;

            var ret = Allocate(rows * columns);
            using(var devicePtr = new CudaDeviceVariable<CUdeviceptr>(rows)) {
                devicePtr.CopyToDevice(vectorData.Cast<GpuVector>().Select(d => d.CudaDeviceVariable.DevicePointer).ToArray());
	            _Invoke2(_copyToMatrix, rows, columns, devicePtr.DevicePointer, ret.DevicePointer, rows, columns);
            }
            return new GpuMatrix(this, rows, columns, ret);
        }

        public IMatrix CreateMatrix(int rows, int columns, Func<int, int, float> init)
        {
            return new GpuMatrix(this, rows, columns, init);
        }

        public IMatrix CreateZeroMatrix(int rows, int columns)
        {
            var data = Allocate(rows * columns, true);
            return new GpuMatrix(this, rows, columns, data);
        }

        public IMatrix CreateMatrix(int rows, int columns)
        {
            var data = Allocate(rows * columns);
            return new GpuMatrix(this, rows, columns, data);
        }

        public void PushLayer()
        {
            _cache.PushLayer();
        }

        public void PopLayer()
        {
            _cache.PopLayer();
        }

        internal IDeviceMemoryPtr Allocate(int size, bool setToZero = false)
        {
            var ret = _cache.GetMemory(size);
            if (setToZero)
                ret.Clear();
            return ret;
        }

        public I3DTensor Create3DTensor(IReadOnlyList<IMatrix> data)
        {
            var first = data.First();
            return new Gpu3DTensor(this, first.RowCount, first.ColumnCount, data.Cast<GpuMatrix>().ToList());
        }

        public I4DTensor Create4DTensor(IReadOnlyList<FloatTensor> data)
        {
            return new Gpu4DTensor(this, data);
        }

        public I4DTensor Create4DTensor(IReadOnlyList<I3DTensor> tensorList)
        {
            return new Gpu4DTensor(this, tensorList);
        }

        public void BindThread()
        {
            _cuda.SetCurrent();
        }
    }
}
