using BrightWire.CUDA.Helper;
using BrightWire.Models;
using ManagedCuda;
using ManagedCuda.BasicTypes;
using ManagedCuda.CudaBlas;
using ManagedCuda.CudaSolve;
using ManagedCuda.VectorTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.LinearAlgebra
{
    internal class CudaProvider : ILinearAlgebraProvider, IGpuLinearAlgebraProvider
    {
        const int BLOCK_DIM = 16;
        const int BLOCK_DIM2 = BLOCK_DIM * BLOCK_DIM;

        //class AllocationLayer
        //{
        //    readonly List<GpuMatrix> _matrix = new List<GpuMatrix>();
        //    readonly List<GpuVector> _vector = new List<GpuVector>();

        //    public void Add(GpuMatrix matrix) { _matrix.Add(matrix); }
        //    public void Add(GpuVector vector) { _vector.Add(vector); }
        //    public void Release()
        //    {
        //        foreach(var item in _matrix)
        //            item?.Dispose();
        //        foreach (var item in _vector)
        //            item?.Dispose();
        //        _matrix.Clear();
        //        _vector.Clear();
        //    }
        //}

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

            public void Run(uint sharedMemSize, params object[] param)
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
            readonly CudaContext _context;
            readonly CUmodule _module;

            public KernelModule(CudaContext context, string path)
            {
                _context = context;
                _module = _context.LoadModule(path);
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

        readonly bool _stochastic;
        readonly CudaContext _cuda;
        readonly CudaBlas _blas;
        readonly Lazy<CudaSolveDense> _solver = new Lazy<CudaSolveDense>();
        readonly KernelModule _kernel;
        readonly ILinearAlgebraProvider _numerics = BrightWireProvider.CreateLinearAlgebra();
        //readonly Stack<AllocationLayer> _allocationLayer = new Stack<AllocationLayer>();
        readonly MemoryBlock _cache;
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
            _tensorCalculateWeightUpdate,
            _tensorMaxPool
        ;
        bool _disposed = false;

        public CudaProvider(string cudaKernelPath, bool stochastic = true)
        {
            _stochastic = stochastic;
            _cache = new MemoryBlock();
            _cuda = new CudaContext();
            _kernel = new KernelModule(_cuda, cudaKernelPath);
            _blas = new CudaBlas(AtomicsMode.Allowed);

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
            //_memCopy = _kernel.LoadFunction("MemCopy");
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
            _tensorCalculateWeightUpdate = _kernel.LoadFunction("TensorCalculateWeightUpdate");
            _tensorMaxPool = _kernel.LoadFunction("TensorMaxPool"); 
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
        public bool IsStochastic => _stochastic;
        public bool IsGpu => true;

        public CudaSolveDense Solver
        {
            get
            {
                return _solver.Value;
            }
        }
        
        public long TotalMemory
        {
            get { return _cuda.GetTotalDeviceMemorySize(); }
        }

        public long FreeMemory
        {
            get { return _cuda.GetFreeDeviceMemorySize(); }
        }

        int _GetBlockCount(int size, int blockSize)
        {
            return ((size / blockSize) + 1);
        }

        void _Use(CUfunction function, int rows, int columns, Action<KernelExecution> callback)
        {
            var x = _GetBlockCount(rows, BLOCK_DIM);
            var y = _GetBlockCount(columns, BLOCK_DIM);
            callback(_kernel.CreateExecution(function, new dim3(x, y), new dim3(BLOCK_DIM, BLOCK_DIM)));
            //_cuda.Synchronize();
        }

        void _Use(CUfunction function, int size, Action<KernelExecution> callback)
        {
            var x = _GetBlockCount(size, BLOCK_DIM2);
            callback(_kernel.CreateExecution(function, x, BLOCK_DIM2));
        }

        internal MemoryBlock.Ptr PointwiseMultiply(MemoryBlock.Ptr a, MemoryBlock.Ptr b, int size)
        {
            var ret = Allocate(size);
            ret.CopyToDevice(b);
            _Use(_pointwiseMultiply, size, k => k.Run(BLOCK_DIM2 * sizeof(float), a.DevicePointer, ret.DevicePointer, size));
            return ret;
        }

        internal MemoryBlock.Ptr PointwiseDivide(MemoryBlock.Ptr a, MemoryBlock.Ptr b, int size)
        {
            var ret = Allocate(size);
            ret.CopyToDevice(b);
            _Use(_pointwiseDivide, size, k => k.Run(BLOCK_DIM2 * sizeof(float), a.DevicePointer, ret.DevicePointer, size));
            return ret;
        }

        internal void AddInPlace(MemoryBlock.Ptr a, MemoryBlock.Ptr b, int size, float coefficient1, float coefficient2)
        {
            _Use(_addInPlace, size, k => k.Run(BLOCK_DIM2 * sizeof(float), a.DevicePointer, b.DevicePointer, size, coefficient1, coefficient2));
        }

        internal void SubtractInPlace(MemoryBlock.Ptr a, MemoryBlock.Ptr b, int size, float coefficient1, float coefficient2)
        {
            _Use(_subtractInPlace, size, k => k.Run(BLOCK_DIM2 * sizeof(float), a.DevicePointer, b.DevicePointer, size, coefficient1, coefficient2));
        }

        internal void AddToEachRow(MemoryBlock.Ptr matrix, MemoryBlock.Ptr vector, int rows, int columns)
        {
            _Use(_addToEachRow, rows, columns, k => k.Run(0, matrix.DevicePointer, vector.DevicePointer, rows, columns));
        }

        internal void AddToEachColumn(MemoryBlock.Ptr matrix, MemoryBlock.Ptr vector, int rows, int columns)
        {
            _Use(_addToEachColumn, rows, columns, k => k.Run(0, matrix.DevicePointer, vector.DevicePointer, rows, columns));
        }

        internal MemoryBlock.Ptr TanH(MemoryBlock.Ptr a, int size)
        {
            var ret = Allocate(size);
            _Use(_tanh, size, k => k.Run(BLOCK_DIM2 * sizeof(float), a.DevicePointer, ret.DevicePointer, size));
            return ret;
        }

        internal MemoryBlock.Ptr TanHDerivative(MemoryBlock.Ptr a, int size)
        {
            var ret = Allocate(size);
            _Use(_tanhDerivative, size, k => k.Run(BLOCK_DIM2 * sizeof(float), a.DevicePointer, ret.DevicePointer, size));
            return ret;
        }

        internal MemoryBlock.Ptr Sigmoid(MemoryBlock.Ptr a, int size)
        {
            var ret = Allocate(size);
            _Use(_sigmoid, size, k => k.Run(BLOCK_DIM2 * sizeof(float), a.DevicePointer, ret.DevicePointer, size));
            return ret;
        }

        internal MemoryBlock.Ptr SigmoidDerivative(MemoryBlock.Ptr a, int size)
        {
            var ret = Allocate(size);
            _Use(_sigmoidDerivative, size, k => k.Run(BLOCK_DIM2 * sizeof(float), a.DevicePointer, ret.DevicePointer, size));
            return ret;
        }

        internal MemoryBlock.Ptr RELU(MemoryBlock.Ptr a, int size)
        {
            var ret = Allocate(size);
            _Use(_relu, size, k => k.Run(BLOCK_DIM2 * sizeof(float), a.DevicePointer, ret.DevicePointer, size));
            return ret;
        }

        internal MemoryBlock.Ptr RELUDerivative(MemoryBlock.Ptr a, int size)
        {
            var ret = Allocate(size);
            _Use(_reluDerivative, size, k => k.Run(BLOCK_DIM2 * sizeof(float), a.DevicePointer, ret.DevicePointer, size));
            return ret;
        }

        internal MemoryBlock.Ptr LeakyRELU(MemoryBlock.Ptr a, int size)
        {
            var ret = Allocate(size);
            _Use(_leakyRelu, size, k => k.Run(BLOCK_DIM2 * sizeof(float), a.DevicePointer, ret.DevicePointer, size));
            return ret;
        }

        internal MemoryBlock.Ptr LeakyRELUDerivative(MemoryBlock.Ptr a, int size)
        {
            var ret = Allocate(size);
            _Use(_leakyReluDerivative, size, k => k.Run(BLOCK_DIM2 * sizeof(float), a.DevicePointer, ret.DevicePointer, size));
            return ret;
        }

        internal MemoryBlock.Ptr SumRows(MemoryBlock.Ptr a, int rows, int columns)
        {
            var ret = Allocate(rows);
            _Use(_sumRows, rows, columns, k => k.Run(0, a.DevicePointer, ret.DevicePointer, rows, columns));
            return ret;
        }

        internal MemoryBlock.Ptr SumColumns(MemoryBlock.Ptr a, int rows, int columns)
        {
            var ret = Allocate(columns);
            _Use(_sumColumns, columns, k => k.Run(0, a.DevicePointer, ret.DevicePointer, rows, columns));
            return ret;
        }

        internal void MemClear(MemoryBlock.Ptr data, int count, int offset = 0, int increment = 1)
        {
            _Use(_memClear, count, k => k.Run(0, data.DevicePointer, count, offset, increment));
        }

        internal MemoryBlock.Ptr Sqrt(MemoryBlock.Ptr a, int size, float valueAdjustment)
        {
            var ret = Allocate(size);
            _Use(_sqrt, size, k => k.Run(BLOCK_DIM2 * sizeof(float), a.DevicePointer, ret.DevicePointer, size, valueAdjustment));
            return ret;
        }

        internal MemoryBlock.Ptr Abs(MemoryBlock.Ptr a, int size)
        {
            var ret = Allocate(size);
            _Use(_abs, size, k => k.Run(0, a.DevicePointer, ret.DevicePointer, size));
            return ret;
        }

        internal MemoryBlock.Ptr Log(MemoryBlock.Ptr a, int size)
        {
            var ret = Allocate(size);
            _Use(_log, size, k => k.Run(0, a.DevicePointer, ret.DevicePointer, size));
            return ret;
        }

        internal void VectorAdd(MemoryBlock.Ptr a, int size, float scalar)
        {
            _Use(_vectorAdd, size, k => k.Run(0, a.DevicePointer, size, scalar));
        }

        internal MemoryBlock.Ptr VectorCopy(MemoryBlock.Ptr a, int size, int[] indexList)
        {
            var retSize = indexList.Length;
            var ret = Allocate(retSize);
            using (var indexGpu = new CudaDeviceVariable<int>(retSize)) {
                indexGpu.CopyToDevice(indexList);
                _Use(_vectorCopyRandom, retSize, k => k.Run(0, a.DevicePointer, ret.DevicePointer, indexGpu.DevicePointer, retSize));
                return ret;
            }
        }

        internal (float Min, float Max) FindMinAndMax(MemoryBlock.Ptr a, int size)
        {
            if (size > 0) {
                var ptr = a;
                while (size > BLOCK_DIM2) {
                    var bufferSize = (size / BLOCK_DIM2) + 1;
                    var minBlock = Allocate(bufferSize, true);
                    var maxBlock = Allocate(bufferSize, true);

                    try {
                        _Use(_findMinAndMax, size, k => k.Run(BLOCK_DIM2, ptr.DevicePointer, size, minBlock.DevicePointer, maxBlock.DevicePointer));
                        if (ptr != a)
                            ptr.Release();
                        var minTest = new float[bufferSize];
                        var maxText = new float[bufferSize];
                        minBlock.CopyToHost(minTest);
                        maxBlock.CopyToHost(maxText);
                        size = bufferSize * 2;
                        ptr = Allocate(size);
                        ptr.DeviceVariable.CopyToDevice(minBlock.DeviceVariable, 0, 0, bufferSize * sizeof(float));
                        ptr.DeviceVariable.CopyToDevice(maxBlock.DeviceVariable, 0, bufferSize * sizeof(float), bufferSize * sizeof(float));
                    }
                    finally {
                        minBlock.Release();
                        maxBlock.Release();
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

        internal float SumValues(MemoryBlock.Ptr a, int size)
        {
            var ptr = a;
            while (size > BLOCK_DIM2) {
                var bufferSize = (size / BLOCK_DIM2) + 1;
                var sumBlock = Allocate(bufferSize);
                _Use(_findSum, size, k => k.Run(BLOCK_DIM2, ptr.DevicePointer, size, sumBlock.DevicePointer));
                if (ptr != a)
                    ptr.Release();
                size = bufferSize;
                ptr = sumBlock;
            }
            var total = new float[size];
            ptr.CopyToHost(total);
            if (ptr != a)
                ptr.Release();
            return total.Sum();
        }

        internal float FindStdDev(MemoryBlock.Ptr a, int size, float mean)
        {
            var inputSize = size;
            if (size > 0) {
                var ptr = a;
                while(size > BLOCK_DIM2) {
                    var bufferSize = (size / BLOCK_DIM2) + 1;
                    var sumBlock = Allocate(bufferSize);
                    _Use(_findStdDev, size, k => k.Run(BLOCK_DIM2, ptr.DevicePointer, size, mean, sumBlock.DevicePointer));
                    if (ptr != a)
                        ptr.Release();
                    size = bufferSize;
                    ptr = sumBlock;
                }
                var total = new float[size];
                ptr.CopyToHost(total);
                if (ptr != a)
                    ptr.Release();

                return Convert.ToSingle(Math.Sqrt(total.Sum() / inputSize));
            }
            return 0f;
        }

        internal void Constrain(MemoryBlock.Ptr a, int size, float min, float max)
        {
            _Use(_constrain, size, k => k.Run(0, a.DevicePointer, size, min, max));
        }

        internal MemoryBlock.Ptr Pow(MemoryBlock.Ptr a, int size, float power)
        {
            var ret = Allocate(size);
            _Use(_pow, size, k => k.Run(0, a.DevicePointer, ret.DevicePointer, size, power));
            return ret;
        }

        internal MemoryBlock.Ptr Diagonal(MemoryBlock.Ptr a, int rows, int columns)
        {
            var len = Math.Min(rows, columns);
            var ret = Allocate(len);
            _Use(_diagonal, len, k => k.Run(0, a.DevicePointer, ret.DevicePointer, rows, columns));
            return ret;
        }

        internal void L1Regularisation(MemoryBlock.Ptr a, int size, float coefficient)
        {
            _Use(_l1Regularisation, size, k => k.Run(0, a.DevicePointer, size, coefficient));
        }

        internal float EuclideanDistance(MemoryBlock.Ptr a, MemoryBlock.Ptr b, int size)
        {
            var ret = Allocate(size);
            _Use(_euclideanDistance, size, k => k.Run(0, a.DevicePointer, b.DevicePointer, ret.DevicePointer, size));
            return Convert.ToSingle(Math.Sqrt(SumValues(ret, size)));
        }

        internal float ManhattanDistance(MemoryBlock.Ptr a, MemoryBlock.Ptr b, int size)
        {
            var ret = Allocate(size);
            _Use(_manhattanDistance, size, k => k.Run(0, a.DevicePointer, b.DevicePointer, ret.DevicePointer, size));
            return SumValues(ret, size);
        }

        internal void Normalise(MemoryBlock.Ptr a, int size, float min, float range)
        {
            _Use(_normalise, size, k => k.Run(0, a.DevicePointer, size, min, range));
        }

        internal MemoryBlock.Ptr SoftmaxVector(MemoryBlock.Ptr a, int size, float max)
        {
            var ret = Allocate(size);
            _Use(_softmaxVector, size, k => k.Run(0, a.DevicePointer, ret.DevicePointer, size, max));
            return ret;
        }

        internal void VectorSplit(MemoryBlock.Ptr a, int size, int blockSize, CUdeviceptr output)
        {
            _Use(_vectorSplit, size, k => k.Run(0, a.DevicePointer, output, size, blockSize));
        }

        internal void PointwiseDivideRows(MemoryBlock.Ptr a, MemoryBlock.Ptr b, int rows, int columns)
        {
            _Use(_pointwiseDivideRows, rows, columns, k => k.Run(0, a.DevicePointer, b.DevicePointer, rows, columns));
        }

        internal void PointwiseDivideColumns(MemoryBlock.Ptr a, MemoryBlock.Ptr b, int rows, int columns)
        {
            _Use(_pointwiseDivideColumns, rows, columns, k => k.Run(0, a.DevicePointer, b.DevicePointer, rows, columns));
        }

        internal void SplitRows(MemoryBlock.Ptr a, MemoryBlock.Ptr b, MemoryBlock.Ptr c, int rows, int columns, int position)
        {
            _Use(_splitRows, rows, columns, k => k.Run(0, a.DevicePointer, b.DevicePointer, c.DevicePointer, rows, columns, position));
        }

        internal void SplitColumns(MemoryBlock.Ptr a, MemoryBlock.Ptr b, MemoryBlock.Ptr c, int rows, int columns, int position)
        {
            _Use(_splitColumns, rows, columns, k => k.Run(0, a.DevicePointer, b.DevicePointer, c.DevicePointer, rows, columns, position));
        }

        internal void ConcatRows(MemoryBlock.Ptr a, MemoryBlock.Ptr b, MemoryBlock.Ptr c, int rows, int columns, int leftColumnCount)
        {
            _Use(_concatRows, rows, columns, k => k.Run(0, a.DevicePointer, b.DevicePointer, c.DevicePointer, rows, columns, leftColumnCount));
        }

        internal void ConcatColumns(MemoryBlock.Ptr a, MemoryBlock.Ptr b, MemoryBlock.Ptr c, int rows, int columns, int topRowCount, int bottomRowCount)
        {
            _Use(_concatColumns, rows, columns, k => k.Run(0, a.DevicePointer, b.DevicePointer, c.DevicePointer, rows, columns, topRowCount, bottomRowCount));
        }

        internal MemoryBlock.Ptr MultiEuclideanDistance(MemoryBlock.Ptr vector, CUdeviceptr[] compareTo, int size)
        {
            MemoryBlock.Ptr ret = null;
            var buffer = _cuda.AllocateMemory(8 * compareTo.Length);
            try {
                _cuda.CopyToDevice(buffer, compareTo);
                ret = Allocate(size * compareTo.Length);
                _Use(_multiEuclidean, size, compareTo.Length, k => k.Run(0, vector.DevicePointer, buffer, ret.DevicePointer, size, compareTo.Length));
            }
            finally {
                _cuda.FreeMemory(buffer);
            }
            return ret;
        }

        internal MemoryBlock.Ptr MultiManhattanDistance(MemoryBlock.Ptr vector, CUdeviceptr[] compareTo, int size)
        {
            MemoryBlock.Ptr ret = null;
            var buffer = _cuda.AllocateMemory(8 * compareTo.Length);
            try {
                _cuda.CopyToDevice(buffer, compareTo);
                ret = Allocate(size * compareTo.Length);
                _Use(_multiManhattan, size, compareTo.Length, k => k.Run(0, vector.DevicePointer, buffer, ret.DevicePointer, size, compareTo.Length));
            }
            finally {
                _cuda.FreeMemory(buffer);
            }
            return ret;
        }

        internal MemoryBlock.Ptr TensorConvertToVector(IReadOnlyList<MemoryBlock.Ptr> matrixList, int matrixSize)
        {
            var outputSize = matrixList.Count * matrixSize;
            var ret = Allocate(outputSize);
            using (var devicePtr = new CudaDeviceVariable<CUdeviceptr>(matrixList.Count)) {
                devicePtr.CopyToDevice(matrixList.Select(m => m.DevicePointer).ToArray());
                _Use(_tensorConvertToVector, outputSize, k => k.Run(0, devicePtr.DevicePointer, ret.DevicePointer, matrixSize, outputSize));
            }
            return ret;
        }

        internal void TensorConvertToMatrix(IReadOnlyList<MemoryBlock.Ptr> matrixList, int tensorRows, int tensorColumns, int matrixRows, int matrixColumns, MemoryBlock.Ptr ret)
        {
            using (var devicePtr = new CudaDeviceVariable<CUdeviceptr>(matrixList.Count)) {
                devicePtr.CopyToDevice(matrixList.Select(m => m.DevicePointer).ToArray());
                _Use(_tensorConvertToMatrix, matrixRows, matrixColumns, k => k.Run(0, devicePtr.DevicePointer, ret.DevicePointer, tensorRows, tensorColumns, matrixRows, matrixColumns));
            }
        }

        internal IReadOnlyList<MemoryBlock.Ptr> TensorAddPadding(IReadOnlyList<MemoryBlock.Ptr> matrixList, int rows, int columns, int padding)
        {
            int depth = matrixList.Count;
            var newRows = rows + padding * 2;
            var newColumns = columns + padding * 2;
            var ret = new List<MemoryBlock.Ptr>();
            for (var i = 0; i < depth; i++) {
                var buffer = Allocate(newRows * newColumns, true);
                ret.Add(buffer);
            }

            using (var outputDevicePtr = new CudaDeviceVariable<CUdeviceptr>(depth))
            using (var inputDevicePtr = new CudaDeviceVariable<CUdeviceptr>(depth)) {
                inputDevicePtr.CopyToDevice(matrixList.Select(m => m.DevicePointer).ToArray());
                outputDevicePtr.CopyToDevice(ret.Select(m => m.DevicePointer).ToArray());
                _Use(_tensorAddPadding, newRows, newColumns, k => k.Run(0, inputDevicePtr.DevicePointer, outputDevicePtr.DevicePointer, rows, columns, newRows, newColumns, depth, padding));
            }
            return ret;
        }

        internal IReadOnlyList<MemoryBlock.Ptr> TensorRemovePadding(IReadOnlyList<MemoryBlock.Ptr> matrixList, int rows, int columns, int padding)
        {
            int depth = matrixList.Count;
            var newRows = rows - padding * 2;
            var newColumns = columns - padding * 2;
            var ret = new List<MemoryBlock.Ptr>();
            for (var i = 0; i < depth; i++) {
                var buffer = Allocate(newRows * newColumns, true);
                ret.Add(buffer);
            }

            using (var outputDevicePtr = new CudaDeviceVariable<CUdeviceptr>(depth))
            using (var inputDevicePtr = new CudaDeviceVariable<CUdeviceptr>(depth)) {
                inputDevicePtr.CopyToDevice(matrixList.Select(m => m.DevicePointer).ToArray());
                outputDevicePtr.CopyToDevice(ret.Select(m => m.DevicePointer).ToArray());
                _Use(_tensorRemovePadding, rows, columns, k => k.Run(0, inputDevicePtr.DevicePointer, outputDevicePtr.DevicePointer, rows, columns, newRows, newColumns, depth, padding));
            }
            return ret;
        }

        internal Tuple<MemoryBlock.Ptr, int, int> TensorIm2Col(IReadOnlyList<MemoryBlock.Ptr> matrixList, int rows, int columns, int filterWidth, int filterHeight, int stride)
        {
            var depth = matrixList.Count;
            var xExtent = (columns - filterWidth) / stride + 1;
            var yExtent = (rows - filterHeight) / stride + 1;
            var filterSize = filterHeight * filterWidth;
            var newColumns = filterSize * depth;
            var newRows = xExtent * yExtent;

            var ret = Allocate(newColumns * newRows);
            using (var devicePtr = new CudaDeviceVariable<CUdeviceptr>(depth)) {
                devicePtr.CopyToDevice(matrixList.Select(m => m.DevicePointer).ToArray());
                _Use(_tensorIm2Col, newRows, newColumns, k => k.Run(0, devicePtr.DevicePointer, ret.DevicePointer, rows, columns, newRows, newColumns, depth, filterWidth, filterHeight, stride));
            }
            return Tuple.Create(ret, newRows, newColumns);
        }

        internal MemoryBlock.Ptr VectorSoftmaxDerivative(MemoryBlock.Ptr a, int size)
        {
            var ret = Allocate(size * size);
            _Use(_softmaxDerivative, size, size, k => k.Run(0, a.DevicePointer, ret.DevicePointer, size));
            return ret;
        }

        internal MemoryBlock.Ptr Reverse(MemoryBlock.Ptr a, int size)
        {
            var ret = Allocate(size);
            _Use(_reverse, size, k => k.Run(BLOCK_DIM2 * sizeof(float), a.DevicePointer, ret.DevicePointer, size));
            return ret;
        }

        internal MemoryBlock.Ptr Rotate(MemoryBlock.Ptr a, int size, int blockCount)
        {
            var blockSize = size / blockCount;
            var vectorList = Enumerable.Range(0, blockCount)
                .Select(i => Allocate(blockSize))
                .ToList()
            ;
            var ret = Allocate(size);
            using (var devicePtr = new CudaDeviceVariable<CUdeviceptr>(blockCount)) {
                devicePtr.CopyToDevice(vectorList.Select(p => p.DevicePointer).ToArray());
                VectorSplit(a, size, blockSize, devicePtr.DevicePointer);

                _Use(_rotate, size, k => k.Run(0, devicePtr.DevicePointer, ret.DevicePointer, size, blockCount, blockSize));
            }
            foreach (var item in vectorList)
                item.Release();
            return ret;
        }

        internal MemoryBlock.Ptr TensorCalculateWeightUpdate(IReadOnlyList<MemoryBlock.Ptr> matrixList, int rows, int columns)
        {
            var depth = matrixList.Count;
            var ret = Allocate(depth * rows * columns);

            return ret;
        }

        internal IReadOnlyList<(MemoryBlock.Ptr, int[], int[])> TensorMaxPool(IReadOnlyList<MemoryBlock.Ptr> matrixList, int rows, int columns, int filterWidth, int filterHeight, int stride)
        {
            var newColumns = (columns - filterWidth) / stride + 1;
            var newRows = (rows - filterHeight) / stride + 1;
            var size = newColumns * newRows;
            var depth = matrixList.Count;
            var ret = new List<MemoryBlock.Ptr>();
            var xIndex = new List<CudaDeviceVariable<int>>();
            var yIndex = new List<CudaDeviceVariable<int>>();
            for(var i = 0; i < depth; i++) {
                ret.Add(Allocate(size));
                xIndex.Add(new CudaDeviceVariable<int>(size));
                yIndex.Add(new CudaDeviceVariable<int>(size));
            }
            using (var outputDevicePtr = new CudaDeviceVariable<CUdeviceptr>(depth))
            using (var inputDevicePtr = new CudaDeviceVariable<CUdeviceptr>(depth))
            using (var xIndexPtr = new CudaDeviceVariable<CUdeviceptr>(depth))
            using (var yIndexPtr = new CudaDeviceVariable<CUdeviceptr>(depth)) {
                inputDevicePtr.CopyToDevice(matrixList.Select(m => m.DevicePointer).ToArray());
                outputDevicePtr.CopyToDevice(ret.Select(m => m.DevicePointer).ToArray());
                xIndexPtr.CopyToDevice(xIndex.Select(m => m.DevicePointer).ToArray());
                yIndexPtr.CopyToDevice(yIndex.Select(m => m.DevicePointer).ToArray());
                _Use(_tensorMaxPool, newRows, newColumns, k => k.Run(0, inputDevicePtr.DevicePointer, outputDevicePtr.DevicePointer, xIndexPtr.DevicePointer, yIndexPtr.DevicePointer, rows, columns, depth, newRows, newColumns, filterWidth, filterHeight, stride));
            }
            return ret.Select((d, i) => {
                var x = new int[size];
                var y = new int[size];
                xIndex[i].CopyToHost(x);
                yIndex[i].CopyToHost(y);
                xIndex[i].Dispose();
                yIndex[i].Dispose();
                return (d, x, y);
            }).ToList();
        }

        internal CudaContext Context { get { return _cuda; } }
        internal CudaBlas Blas { get { return _blas; } }

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
                _Use(_copyToMatrix, rows, columns, k => k.Run(0, devicePtr.DevicePointer, ret.DevicePointer, rows, columns));
            }
            return new GpuMatrix(this, rows, columns, ret);
        }

        public IMatrix CreateMatrix(int rows, int columns, Func<int, int, float> init)
        {
            return new GpuMatrix(this, rows, columns, init);
        }

        public void PushLayer()
        {
            _cache.PushLayer();
            //_allocationLayer.Push(new AllocationLayer());
        }

        public void PopLayer()
        {
            _cache.PopLayer();
            //var layer = _allocationLayer.Pop();
            //layer.Release();
        }

        internal MemoryBlock.Ptr Allocate(int size, bool setToZero = false)
        {
            var ret = _cache.GetMemory(size);
            if (setToZero)
                ret.Clear();
            return ret;
        }

        public I3DTensor CreateTensor(IReadOnlyList<IMatrix> data)
        {
            var first = data.First();
            return new Gpu3DTensor(this, first.RowCount, first.ColumnCount, data.Count, data.Cast<GpuMatrix>().ToList());
        }

        public void BindThread()
        {
            _cuda.SetCurrent();
        }
    }
}
