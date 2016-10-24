using BrightWire.Connectionist;
using BrightWire.Models;
using BrightWire.Models.Simple;
using ManagedCuda;
using ManagedCuda.BasicTypes;
using ManagedCuda.CudaBlas;
using ManagedCuda.CudaSolve;
using ManagedCuda.VectorTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.LinearAlgebra
{
    internal class CudaProvider : ILinearAlgebraProvider
    {
        const int BLOCK_DIM = 32;
        const int BLOCK_DIM2 = BLOCK_DIM * BLOCK_DIM;

        class AllocationLayer
        {
            readonly List<GpuMatrix> _matrix = new List<GpuMatrix>();
            readonly List<GpuVector> _vector = new List<GpuVector>();

            public void Add(GpuMatrix matrix) { _matrix.Add(matrix); }
            public void Add(GpuVector vector) { _vector.Add(vector); }
            public void Release()
            {
                foreach(var item in _matrix)
                    item.Dispose();
                foreach (var item in _vector)
                    item.Dispose();
                _matrix.Clear();
                _vector.Clear();
            }
        }

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
        readonly NumericsProvider _numerics = new NumericsProvider();
        readonly Stack<AllocationLayer> _allocationLayer = new Stack<AllocationLayer>();
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
            _sparseLoad,
            _sumColumns,
            _pointwiseDivide,
            _sqrt,
            _memCopy,
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
            _sigmoidVector,
            _vectorAdd
        ;
        bool _disposed = false;

        public CudaProvider(string cudaKernelPath, bool stochastic = true)
        {
            _stochastic = stochastic;
            _cuda = new CudaContext();
            _kernel = new KernelModule(_cuda, cudaKernelPath);
            _blas = new CudaBlas(AtomicsMode.Allowed);

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
            _sparseLoad = _kernel.LoadFunction("SparseLoad");
            _sumColumns = _kernel.LoadFunction("SumColumns");
            _pointwiseDivide = _kernel.LoadFunction("PointwiseDivide");
            _sqrt = _kernel.LoadFunction("Sqrt");
            _memCopy = _kernel.LoadFunction("MemCopy");
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
            _sigmoidVector = _kernel.LoadFunction("SigmoidVector");
            _vectorAdd = _kernel.LoadFunction("VectorAdd");
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_disposed) {
                _blas.Dispose();
                _cuda.Dispose();
                if(_solver.IsValueCreated)
                    _solver.Value.Dispose();
                _numerics.Dispose();
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

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

        void _Use(CUfunction function, int rows, int columns, Action<KernelExecution> callback)
        {
            callback(_kernel.CreateExecution(function, new dim3((rows / BLOCK_DIM) + 1, (columns / BLOCK_DIM) + 1), new dim3(BLOCK_DIM, BLOCK_DIM)));
        }

        void _Use(CUfunction function, int size, Action<KernelExecution> callback)
        {
            callback(_kernel.CreateExecution(function, (size / BLOCK_DIM2) + 1, BLOCK_DIM2));
        }

        internal CudaDeviceVariable<float> PointwiseMultiply(CudaDeviceVariable<float> a, CudaDeviceVariable<float> b, int size)
        {
            CudaDeviceVariable<float> ret = new CudaDeviceVariable<float>(size);
            ret.CopyToDevice(b);
            _Use(_pointwiseMultiply, size, k => k.Run(BLOCK_DIM2 * sizeof(float), a.DevicePointer, ret.DevicePointer, size));
            return ret;
        }

        internal CudaDeviceVariable<float> PointwiseDivide(CudaDeviceVariable<float> a, CudaDeviceVariable<float> b, int size)
        {
            CudaDeviceVariable<float> ret = new CudaDeviceVariable<float>(size);
            ret.CopyToDevice(b);
            _Use(_pointwiseDivide, size, k => k.Run(BLOCK_DIM2 * sizeof(float), a.DevicePointer, ret.DevicePointer, size));
            return ret;
        }

        internal void AddInPlace(CudaDeviceVariable<float> a, CudaDeviceVariable<float> b, int size, float coefficient1, float coefficient2)
        {
            _Use(_addInPlace, size, k => k.Run(BLOCK_DIM2 * sizeof(float), a.DevicePointer, b.DevicePointer, size, coefficient1, coefficient2));
        }

        internal void SubtractInPlace(CudaDeviceVariable<float> a, CudaDeviceVariable<float> b, int size, float coefficient1, float coefficient2)
        {
            _Use(_subtractInPlace, size, k => k.Run(BLOCK_DIM2 * sizeof(float), a.DevicePointer, b.DevicePointer, size, coefficient1, coefficient2));
        }

        internal void AddToEachRow(CudaDeviceVariable<float> matrix, CudaDeviceVariable<float> vector, int rows, int columns)
        {
            _Use(_addToEachRow, rows, columns, k => k.Run(0, matrix.DevicePointer, vector.DevicePointer, rows, columns));
        }

        internal void AddToEachColumn(CudaDeviceVariable<float> matrix, CudaDeviceVariable<float> vector, int rows, int columns)
        {
            _Use(_addToEachColumn, rows, columns, k => k.Run(0, matrix.DevicePointer, vector.DevicePointer, rows, columns));
        }

        internal CudaDeviceVariable<float> TanH(CudaDeviceVariable<float> a, int rows, int columns)
        {
            var ret = new CudaDeviceVariable<float>(rows * columns);
            _Use(_tanh, rows, columns, k => k.Run(0, a.DevicePointer, ret.DevicePointer, rows, columns));
            return ret;
        }

        internal CudaDeviceVariable<float> TanHDerivative(CudaDeviceVariable<float> a, int rows, int columns)
        {
            var ret = new CudaDeviceVariable<float>(rows * columns);
            _Use(_tanhDerivative, rows, columns, k => k.Run(0, a.DevicePointer, ret.DevicePointer, rows, columns));
            return ret;
        }

        internal CudaDeviceVariable<float> Sigmoid(CudaDeviceVariable<float> a, int rows, int columns)
        {
            var ret = new CudaDeviceVariable<float>(rows * columns);
            _Use(_sigmoid, rows, columns, k => k.Run(0, a.DevicePointer, ret.DevicePointer, rows, columns));
            return ret;
        }

        internal CudaDeviceVariable<float> Sigmoid(CudaDeviceVariable<float> a, int size)
        {
            var ret = new CudaDeviceVariable<float>(size);
            _Use(_sigmoidVector, size, k => k.Run(0, a.DevicePointer, ret.DevicePointer, size));
            return ret;
        }

        internal CudaDeviceVariable<float> SigmoidDerivative(CudaDeviceVariable<float> a, int rows, int columns)
        {
            var ret = new CudaDeviceVariable<float>(rows * columns);
            _Use(_sigmoidDerivative, rows, columns, k => k.Run(0, a.DevicePointer, ret.DevicePointer, rows, columns));
            return ret;
        }

        internal CudaDeviceVariable<float> RELU(CudaDeviceVariable<float> a, int rows, int columns)
        {
            var ret = new CudaDeviceVariable<float>(rows * columns);
            _Use(_relu, rows, columns, k => k.Run(0, a.DevicePointer, ret.DevicePointer, rows, columns));
            return ret;
        }

        internal CudaDeviceVariable<float> RELUDerivative(CudaDeviceVariable<float> a, int rows, int columns)
        {
            var ret = new CudaDeviceVariable<float>(rows * columns);
            _Use(_reluDerivative, rows, columns, k => k.Run(0, a.DevicePointer, ret.DevicePointer, rows, columns));
            return ret;
        }

        internal CudaDeviceVariable<float> LeakyRELU(CudaDeviceVariable<float> a, int rows, int columns)
        {
            var ret = new CudaDeviceVariable<float>(rows * columns);
            _Use(_leakyRelu, rows, columns, k => k.Run(0, a.DevicePointer, ret.DevicePointer, rows, columns));
            return ret;
        }

        internal CudaDeviceVariable<float> LeakyRELUDerivative(CudaDeviceVariable<float> a, int rows, int columns)
        {
            var ret = new CudaDeviceVariable<float>(rows * columns);
            _Use(_leakyReluDerivative, rows, columns, k => k.Run(0, a.DevicePointer, ret.DevicePointer, rows, columns));
            return ret;
        }

        internal CudaDeviceVariable<float> SumRows(CudaDeviceVariable<float> a, int rows, int columns)
        {
            var ret = new CudaDeviceVariable<float>(rows);
            _Use(_sumRows, rows, k => k.Run(0, a.DevicePointer, ret.DevicePointer, rows, columns));
            return ret;
        }

        internal CudaDeviceVariable<float> SumColumns(CudaDeviceVariable<float> a, int rows, int columns)
        {
            var ret = new CudaDeviceVariable<float>(columns);
            _Use(_sumColumns, columns, k => k.Run(0, a.DevicePointer, ret.DevicePointer, rows, columns));
            return ret;
        }

        internal void MemClear(CudaDeviceVariable<float> data, int count, int offset = 0, int increment = 1)
        {
            _Use(_memClear, count, k => k.Run(0, data.DevicePointer, count, offset, increment));
        }

        internal CudaDeviceVariable<float> MemCopy(CudaDeviceVariable<float> data, int count, int offset = 0, int increment = 1)
        {
            var ret = new CudaDeviceVariable<float>(count);
            _Use(_memCopy, count, k => k.Run(0, data.DevicePointer, ret.DevicePointer, count, offset, increment));
            return ret;
        }

        internal CudaDeviceVariable<float> Sqrt(CudaDeviceVariable<float> a, int size, float valueAdjustment)
        {
            var ret = new CudaDeviceVariable<float>(size);
            _Use(_sqrt, size, k => k.Run(BLOCK_DIM2 * sizeof(float), a.DevicePointer, ret.DevicePointer, size, valueAdjustment));
            return ret;
        }

        internal CudaDeviceVariable<float> Abs(CudaDeviceVariable<float> a, int size)
        {
            var ret = new CudaDeviceVariable<float>(size);
            _Use(_abs, size, k => k.Run(0, a.DevicePointer, ret.DevicePointer, size));
            return ret;
        }

        internal CudaDeviceVariable<float> Log(CudaDeviceVariable<float> a, int size)
        {
            var ret = new CudaDeviceVariable<float>(size);
            _Use(_log, size, k => k.Run(0, a.DevicePointer, ret.DevicePointer, size));
            return ret;
        }

        internal void VectorAdd(CudaDeviceVariable<float> a, int size, float scalar)
        {
            _Use(_vectorAdd, size, k => k.Run(0, a.DevicePointer, size, scalar));
        }

        internal MinMax FindMinAndMax(CudaDeviceVariable<float> a, int size)
        {
            if (size > 0) {
                var ptr = a;
                while (size > BLOCK_DIM2) {
                    var bufferSize = (size / BLOCK_DIM2) + 1;
                    using (var minBlock = new CudaDeviceVariable<float>(bufferSize))
                    using (var maxBlock = new CudaDeviceVariable<float>(bufferSize)) {
                        minBlock.Memset(0);
                        maxBlock.Memset(0);
                        _Use(_findMinAndMax, size, k => k.Run(BLOCK_DIM2, ptr.DevicePointer, size, minBlock.DevicePointer, maxBlock.DevicePointer));
                        if (ptr != a)
                            ptr.Dispose();
                        var minTest = new float[bufferSize];
                        var maxText = new float[bufferSize];
                        minBlock.CopyToHost(minTest);
                        maxBlock.CopyToHost(maxText);
                        size = bufferSize * 2;
                        ptr = new CudaDeviceVariable<float>(size);
                        ptr.CopyToDevice(minBlock, 0, 0, bufferSize * sizeof(float));
                        ptr.CopyToDevice(maxBlock, 0, bufferSize * sizeof(float), bufferSize * sizeof(float));
                        var test = new float[size];
                        ptr.CopyToHost(test);
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
                return new MinMax(min, max);
            }
            return MinMax.Empty;
        }

        internal float FindMean(CudaDeviceVariable<float> a, int size)
        {
            if (size > 0)
                return SumValues(a, size) / size;
            return 0f;
        }

        internal float SumValues(CudaDeviceVariable<float> a, int size)
        {
            var ptr = a;
            while (size > BLOCK_DIM2) {
                var bufferSize = (size / BLOCK_DIM2) + 1;
                var sumBlock = new CudaDeviceVariable<float>(bufferSize);
                _Use(_findSum, size, k => k.Run(BLOCK_DIM2, ptr.DevicePointer, size, sumBlock.DevicePointer));
                if (ptr != a)
                    ptr.Dispose();
                size = bufferSize;
                ptr = sumBlock;
            }
            var total = new float[size];
            ptr.CopyToHost(total);
            if (ptr != a)
                ptr.Dispose();
            return total.Sum();
        }

        internal float FindStdDev(CudaDeviceVariable<float> a, int size, float mean)
        {
            var inputSize = size;
            if (size > 0) {
                var ptr = a;
                while(size > BLOCK_DIM2) {
                    var bufferSize = (size / BLOCK_DIM2) + 1;
                    var sumBlock = new CudaDeviceVariable<float>(bufferSize);
                    _Use(_findStdDev, size, k => k.Run(BLOCK_DIM2, ptr.DevicePointer, size, mean, sumBlock.DevicePointer));
                    if (ptr != a)
                        ptr.Dispose();
                    size = bufferSize;
                    ptr = sumBlock;
                }
                var total = new float[size];
                ptr.CopyToHost(total);
                if (ptr != a)
                    ptr.Dispose();

                return Convert.ToSingle(Math.Sqrt(total.Sum() / inputSize));
            }
            return 0f;
        }

        internal void Constrain(CudaDeviceVariable<float> a, int size, float min, float max)
        {
            _Use(_constrain, size, k => k.Run(0, a.DevicePointer, size, min, max));
        }

        internal CudaDeviceVariable<float> Pow(CudaDeviceVariable<float> a, int size, float power)
        {
            var ret = new CudaDeviceVariable<float>(size);
            _Use(_pow, size, k => k.Run(0, a.DevicePointer, ret.DevicePointer, size, power));
            return ret;
        }

        internal CudaDeviceVariable<float> Diagonal(CudaDeviceVariable<float> a, int rows, int columns)
        {
            var len = Math.Min(rows, columns);
            var ret = new CudaDeviceVariable<float>(len);
            _Use(_diagonal, len, k => k.Run(0, a.DevicePointer, ret.DevicePointer, rows, columns));
            return ret;
        }

        internal void L1Regularisation(CudaDeviceVariable<float> a, int size, float coefficient)
        {
            _Use(_l1Regularisation, size, k => k.Run(0, a.DevicePointer, size, coefficient));
        }

        internal float EuclideanDistance(CudaDeviceVariable<float> a, CudaDeviceVariable<float> b, int size)
        {
            var ret = new CudaDeviceVariable<float>(size);
            _Use(_euclideanDistance, size, k => k.Run(0, a.DevicePointer, b.DevicePointer, ret.DevicePointer, size));
            return Convert.ToSingle(Math.Sqrt(SumValues(ret, size)));
        }

        internal float ManhattanDistance(CudaDeviceVariable<float> a, CudaDeviceVariable<float> b, int size)
        {
            var ret = new CudaDeviceVariable<float>(size);
            _Use(_manhattanDistance, size, k => k.Run(0, a.DevicePointer, b.DevicePointer, ret.DevicePointer, size));
            return SumValues(ret, size);
        }

        internal void Normalise(CudaDeviceVariable<float> a, int size, float min, float range)
        {
            _Use(_normalise, size, k => k.Run(0, a.DevicePointer, size, min, range));
        }

        internal CudaDeviceVariable<float> SoftmaxVector(CudaDeviceVariable<float> a, int size, float max)
        {
            var ret = new CudaDeviceVariable<float>(size);
            _Use(_softmaxVector, size, k => k.Run(0, a.DevicePointer, ret.DevicePointer, size, max));
            return ret;
        }

        internal void PointwiseDivideRows(CudaDeviceVariable<float> a, CudaDeviceVariable<float> b, int rows, int columns)
        {
            _Use(_pointwiseDivideRows, rows, columns, k => k.Run(0, a.DevicePointer, b.DevicePointer, rows, columns));
        }

        internal void PointwiseDivideColumns(CudaDeviceVariable<float> a, CudaDeviceVariable<float> b, int rows, int columns)
        {
            _Use(_pointwiseDivideColumns, rows, columns, k => k.Run(0, a.DevicePointer, b.DevicePointer, rows, columns));
        }

        internal void SplitRows(CudaDeviceVariable<float> a, CudaDeviceVariable<float> b, CudaDeviceVariable<float> c, int rows, int columns, int position)
        {
            _Use(_splitRows, rows, columns, k => k.Run(0, a.DevicePointer, b.DevicePointer, c.DevicePointer, rows, columns, position));
        }

        internal void SplitColumns(CudaDeviceVariable<float> a, CudaDeviceVariable<float> b, CudaDeviceVariable<float> c, int rows, int columns, int position)
        {
            _Use(_splitColumns, rows, columns, k => k.Run(0, a.DevicePointer, b.DevicePointer, c.DevicePointer, rows, columns, position));
        }

        internal void ConcatRows(CudaDeviceVariable<float> a, CudaDeviceVariable<float> b, CudaDeviceVariable<float> c, int rows, int columns, int leftColumnCount)
        {
            _Use(_concatRows, rows, columns, k => k.Run(0, a.DevicePointer, b.DevicePointer, c.DevicePointer, rows, columns, leftColumnCount));
        }

        internal void ConcatColumns(CudaDeviceVariable<float> a, CudaDeviceVariable<float> b, CudaDeviceVariable<float> c, int rows, int columns, int topRowCount, int bottomRowCount)
        {
            _Use(_concatColumns, rows, columns, k => k.Run(0, a.DevicePointer, b.DevicePointer, c.DevicePointer, rows, columns, topRowCount, bottomRowCount));
        }

        internal CudaDeviceVariable<float> MultiEuclideanDistance(CudaDeviceVariable<float> vector, CUdeviceptr[] compareTo, int size)
        {
            CudaDeviceVariable<float> ret = null;
            var buffer = _cuda.AllocateMemory(8 * compareTo.Length);
            try {
                _cuda.CopyToDevice(buffer, compareTo);
                ret = new CudaDeviceVariable<float>(size * compareTo.Length);
                _Use(_multiEuclidean, size, compareTo.Length, k => k.Run(0, vector.DevicePointer, buffer, ret.DevicePointer, size, compareTo.Length));
            }
            finally {
                _cuda.FreeMemory(buffer);
            }
            return ret;
        }

        internal CudaDeviceVariable<float> MultiManhattanDistance(CudaDeviceVariable<float> vector, CUdeviceptr[] compareTo, int size)
        {
            CudaDeviceVariable<float> ret = null;
            var buffer = _cuda.AllocateMemory(8 * compareTo.Length);
            try {
                _cuda.CopyToDevice(buffer, compareTo);
                ret = new CudaDeviceVariable<float>(size * compareTo.Length);
                _Use(_multiManhattan, size, compareTo.Length, k => k.Run(0, vector.DevicePointer, buffer, ret.DevicePointer, size, compareTo.Length));
            }
            finally {
                _cuda.FreeMemory(buffer);
            }
            return ret;
        }

        internal void SparseLoad(CudaDeviceVariable<float> data, int rows, int columns, IReadOnlyList<Tuple<int, float>[]> columnData)
        {
            var totalDataCount = columnData.Sum(d => d.Length);
            var offsetList = new int[columnData.Count];
            var lengthList = new int[columnData.Count];
            var destinationList = new int[totalDataCount];
            var dataList = new float[totalDataCount];
            int maxLength = int.MinValue;
            int offset = 0;

            for (var i = 0; i < columnData.Count; i++) {
                var column = columnData[i];
                offsetList[i] = offset;
                lengthList[i] = column.Length;
                foreach (var val in column) {
                    destinationList[offset] = val.Item1;
                    dataList[offset] = val.Item2;
                    ++offset;
                }
                if (column.Length > maxLength)
                    maxLength = column.Length;
            }

            using (var deviceOffset = new CudaDeviceVariable<int>(offsetList.Length))
            using (var deviceLength = new CudaDeviceVariable<int>(lengthList.Length))
            using (var deviceDestination = new CudaDeviceVariable<int>(destinationList.Length))
            using (var deviceData = new CudaDeviceVariable<float>(dataList.Length)) {
                deviceOffset.CopyToDevice(offsetList);
                deviceLength.CopyToDevice(lengthList);
                deviceDestination.CopyToDevice(destinationList);
                deviceData.CopyToDevice(dataList);
                _Use(_sparseLoad, rows, columns, k => k.Run(
                    0,
                    data.DevicePointer,
                    deviceOffset.DevicePointer,
                    deviceLength.DevicePointer,
                    deviceDestination.DevicePointer,
                    deviceData.DevicePointer,
                    rows,
                    columns
                ));
            }
        }

        internal CudaContext Context { get { return _cuda; } }
        internal CudaBlas Blas { get { return _blas; } }

        ConnectionistFactory _factory = null;
        public INeuralNetworkFactory NN
        {
            get
            {
                return _factory ?? (_factory = new ConnectionistFactory(this, _stochastic));
            }
        }

        public IVector Create(IEnumerable<float> data)
        {
            return Create(data.ToArray());
        }

        public IMatrix Create(IIndexableMatrix matrix)
        {
            return Create(matrix.RowCount, matrix.ColumnCount, (x, y) => matrix[x, y]);
        }

        public IVector Create(IIndexableVector vector)
        {
            return Create(vector.Count, x => vector[x]);
        }

        public IVector Create(float[] data)
        {
            return new GpuVector(this, data.Length, i => data[i]);
        }

        public IVector Create(int length, Func<int, float> init)
        {
            return new GpuVector(this, length, init);
        }

        public IVector Create(int length, float value)
        {
            return new GpuVector(this, length, i => value);
        }

        public IMatrix Create(IList<IIndexableVector> vectorData)
        {
            int columns = vectorData.Count;
            int rows = vectorData[0].Count;
            return Create(rows, columns, (x, y) => vectorData[y][x]);
        }

        public IMatrix Create(int rows, int columns, float value)
        {
            return Create(rows, columns, (x, y) => value);
        }

        public IMatrix Create(int rows, int columns, Func<int, int, float> init)
        {
            return new GpuMatrix(this, rows, columns, init);
        }

        public IMatrix CreateIdentity(int size)
        {
            return Create(size, size, (x, y) => x == y ? 1f : 0f);
        }

        public IMatrix CreateDiagonal(float[] values)
        {
            return Create(values.Length, values.Length, (x, y) => x == y ? values[x] : 0f);
        }

        public IIndexableVector CreateIndexable(int length)
        {
            return _numerics.CreateIndexable(length);
        }

        public IIndexableVector CreateIndexable(int length, Func<int, float> init)
        {
            return _numerics.CreateIndexable(length, init);
        }

        public IIndexableMatrix CreateIndexable(int rows, int columns)
        {
            return _numerics.CreateIndexable(rows, columns);
        }

        public IIndexableMatrix CreateIndexable(int rows, int columns, float value)
        {
            return _numerics.CreateIndexable(rows, columns, value);
        }

        public IIndexableMatrix CreateIndexable(int rows, int columns, Func<int, int, float> init)
        {
            return _numerics.CreateIndexable(rows, columns, init);
        }

        public IMatrix CreateMatrix(IReadOnlyList<FloatArray> data)
        {
            return Create((IIndexableMatrix)_numerics.CreateMatrix(data));
        }

        public IVector CreateVector(FloatArray data)
        {
            return Create((IIndexableVector)_numerics.CreateVector(data));
        }

        public void PushLayer()
        {
            _allocationLayer.Push(new AllocationLayer());
        }

        public void PopLayer()
        {
            var layer = _allocationLayer.Pop();
            layer.Release();
        }

        internal void Register(GpuVector vector)
        {
            if (_allocationLayer.Any()) {
                var layer = _allocationLayer.Peek();
                if (layer != null)
                    layer.Add(vector);
            }
        }

        internal void Register(GpuMatrix matrix)
        {
            if (_allocationLayer.Any()) {
                var layer = _allocationLayer.Peek();
                if (layer != null)
                    layer.Add(matrix);
            }
        }
    }
}
