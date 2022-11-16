using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using BrightData.Cuda.Helper;
using BrightData.Helper;
using ManagedCuda;
using ManagedCuda.BasicTypes;
using ManagedCuda.CudaBlas;
using ManagedCuda.CudaSolve;
using ManagedCuda.VectorTypes;

namespace BrightData.Cuda
{
	/// <summary>
	/// Manages the bright wire cuda kernels and implements the cuda linear algebra provider
	/// </summary>
    public unsafe class CudaProvider : IGpuLinearAlgebraProvider, IHaveBrightDataContext, IDisposable
	{
        readonly BrightDataContext _context;
        readonly Dictionary<List<(uint X, uint Y)>, ConvolutionsData> _convolutionDataCache = new();
        const int BlockSize = 32;
		const int N = BlockSize * BlockSize;
        internal const int FloatSize = sizeof(float);
        internal const int UIntSize = sizeof(uint);

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

			public void Run(CUstream stream, uint sharedMemSize, object[] param)
			{
				var paramList = new IntPtr[param.Length];
				var handleList = new GCHandle[param.Length];

				//Get pointers to kernel parameters
				for (var i = 0; i < param.Length; i++) {
					handleList[i] = GCHandle.Alloc(param[i], GCHandleType.Pinned);
					paramList[i] = handleList[i].AddrOfPinnedObject();
				}

				var result = DriverAPINativeMethods.Launch.cuLaunchKernel(_function,
					_block.x, _block.y, _block.z,
					_thread.x, _thread.y, _thread.z,
					sharedMemSize,
                    stream,
					paramList,
					null
				);

				// free the handles
				for (var i = 0; i < param.Length; i++)
					handleList[i].Free();

				CheckForError(result);
			}
		}

		class KernelModule
		{
			readonly CUmodule _module;

			public KernelModule(CudaContext context, string path)
			{
				_module = context.LoadModule(path);
			}

			public CUfunction LoadFunction(string name)
			{
				var ret = new CUfunction();
				if (DriverAPINativeMethods.ModuleManagement.cuModuleGetFunction(ref ret, _module, name) != CUResult.Success)
					throw new ArgumentException("Function not found", name);
				return ret;
			}

			public static KernelExecution CreateExecution(CUfunction function, dim3 block, dim3 thread)
			{
				return new(function, block, thread);
			}
		}

		readonly CudaContext _cuda;
		readonly Lazy<CudaBlas> _blas;
		readonly Lazy<CudaSolveDense> _solver = new();
		readonly KernelModule _kernel;
        readonly CudaStream _defaultStream;
        readonly MemoryPool _memoryPool;

        internal readonly CUfunction
			_pointwiseMultiply,
			_addInPlace,
			_subtractInPlace,
			_addToEachRow,
			_addToEachColumn,
            _multiplyByEachRow,
            _multiplyByEachColumn,
			_tanh,
			_tanhDerivative,
			_sigmoid,
			_sigmoidDerivative,
			_sumRows,
			_relu,
			_reluDerivative,
			_leakyRelu,
			_leakyReluDerivative,
			_memSet,
			_sumColumns,
			_pointwiseDivide,
			_sqrt,
			_findMinAndMax,
            _sumValues,
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
			_cosineDistance,
			_abs,
			_normalise,
			_softmaxVector,
            _multiCosine,
			_log,
            _exp,
            _vectorAddInPlace,
			_vectorCopyRandom,
			_copyToMatrixColumns,
			_copyToMatrixRows,
			_tensorAddPadding,
			_tensorRemovePadding,
			_tensorIm2Col,
			_softmaxDerivative,
			_reverse,
			_rotateInPlace,
			_tensorMaxPool,
			_tensorReverseMaxPool,
			_tensorReverseIm2Col,
			_isFinite,
			_calculateDistance,
			_roundInPlace,
			_scale
		;
		readonly ConcurrentDictionary<CUfunction, (int BlockSize, int MinGridSize)> _blockSize = new();
		bool _disposed = false;

		public CudaProvider(BrightDataContext context, string? cudaKernelPath, string? cudaDirectory)
        {
            _context = context;
            _cuda = new CudaContext();

            if (cudaKernelPath == null) {
                if (String.IsNullOrWhiteSpace(cudaDirectory))
                    throw new ArgumentException("No kernel path or directory was passed");

                var info = _cuda.GetDeviceInfo();
                var kernelName = $"brightwire_{info.ComputeCapability.Major}{info.ComputeCapability.Minor}.ptx";
                //var kernelName = $"brightwire.ptx";
                cudaKernelPath = Path.Combine(cudaDirectory, kernelName);

				// try the default kernel
                if (!File.Exists(cudaKernelPath))
                    cudaKernelPath = Path.Combine(cudaDirectory, "brightwire.ptx");
                if (!File.Exists(cudaKernelPath))
                    throw new Exception($"No default kernel was found in {cudaDirectory}");
            }

            _memoryPool = new MemoryPool();
            _defaultStream = new CudaStream(CUStreamFlags.Default);

            _kernel = new KernelModule(_cuda, cudaKernelPath);
            _blas = new(() => {
                var ret = new CudaBlas(_defaultStream.Stream, AtomicsMode.Allowed) {
                    MathMode = ManagedCuda.CudaBlas.Math.TF32TensorOpMath
                };
                return ret;
            });
            _cuda.SetCurrent();

            _pointwiseMultiply      = _kernel.LoadFunction("PointwiseMultiply");
			_addInPlace             = _kernel.LoadFunction("AddInPlace");
			_subtractInPlace        = _kernel.LoadFunction("SubtractInPlace");
			_addToEachRow           = _kernel.LoadFunction("AddToEachRow");
			_addToEachColumn        = _kernel.LoadFunction("AddToEachColumn");
            _multiplyByEachRow      = _kernel.LoadFunction("MultiplyByEachRow");
            _multiplyByEachColumn   = _kernel.LoadFunction("MultiplyByEachColumn");
			_tanh                   = _kernel.LoadFunction("TanH");
			_tanhDerivative         = _kernel.LoadFunction("TanHDerivative");
			_sigmoid                = _kernel.LoadFunction("Sigmoid");
			_sigmoidDerivative      = _kernel.LoadFunction("SigmoidDerivative");
			_sumRows                = _kernel.LoadFunction("SumRows");
			_relu                   = _kernel.LoadFunction("RELU");
			_reluDerivative         = _kernel.LoadFunction("RELUDerivative");
			_memSet                 = _kernel.LoadFunction("MemSet");
			_sumColumns             = _kernel.LoadFunction("SumColumns");
			_pointwiseDivide        = _kernel.LoadFunction("PointwiseDivide");
			_sqrt                   = _kernel.LoadFunction("Sqrt");
			_findMinAndMax          = _kernel.LoadFunction("FindMinAndMax");
            _sumValues              = _kernel.LoadFunction("SumValues");
            _findStdDev             = _kernel.LoadFunction("FindStdDev");
			_constrain              = _kernel.LoadFunction("Constrain");
			_pow                    = _kernel.LoadFunction("Pow");
			_diagonal               = _kernel.LoadFunction("Diagonal");
			_l1Regularisation       = _kernel.LoadFunction("L1Regularisation");
			_leakyRelu              = _kernel.LoadFunction("LeakyRELU");
			_leakyReluDerivative    = _kernel.LoadFunction("LeakyRELUDerivative");
			_pointwiseDivideRows    = _kernel.LoadFunction("PointwiseDivideRows");
			_pointwiseDivideColumns = _kernel.LoadFunction("PointwiseDivideColumns");
			_splitRows              = _kernel.LoadFunction("SplitRows");
			_splitColumns           = _kernel.LoadFunction("SplitColumns");
			_concatRows             = _kernel.LoadFunction("ConcatRows");
			_concatColumns          = _kernel.LoadFunction("ConcatColumns");
			_euclideanDistance      = _kernel.LoadFunction("EuclideanDistance");
			_manhattanDistance      = _kernel.LoadFunction("ManhattanDistance");
			_cosineDistance         = _kernel.LoadFunction("CosineDistance");
			_abs                    = _kernel.LoadFunction("Abs");
			_normalise              = _kernel.LoadFunction("Normalise");
			_softmaxVector          = _kernel.LoadFunction("SoftmaxVector");
            _multiCosine            = _kernel.LoadFunction("MultiCosineDistance");
			_log                    = _kernel.LoadFunction("Log");
            _exp                    = _kernel.LoadFunction("Exp");
			_vectorAddInPlace       = _kernel.LoadFunction("VectorAddInPlace");
			_vectorCopyRandom       = _kernel.LoadFunction("VectorCopyRandom");
			_copyToMatrixColumns    = _kernel.LoadFunction("CopyToMatrixColumns");
			_copyToMatrixRows       = _kernel.LoadFunction("CopyToMatrixRows");
			_tensorAddPadding       = _kernel.LoadFunction("TensorAddPadding");
			_tensorRemovePadding    = _kernel.LoadFunction("TensorRemovePadding");
			_tensorIm2Col           = _kernel.LoadFunction("TensorIm2Col");
			_softmaxDerivative      = _kernel.LoadFunction("SoftmaxDerivative");
			_reverse                = _kernel.LoadFunction("Reverse");
			_rotateInPlace          = _kernel.LoadFunction("RotateInPlace");
			_tensorMaxPool          = _kernel.LoadFunction("TensorMaxPool");
			_tensorReverseMaxPool   = _kernel.LoadFunction("TensorReverseMaxPool");
			_tensorReverseIm2Col    = _kernel.LoadFunction("TensorReverseIm2Col");
			_isFinite               = _kernel.LoadFunction("IsFinite");
			_calculateDistance      = _kernel.LoadFunction("CalculateDistances");
            _roundInPlace           = _kernel.LoadFunction("RoundInPlace");
            _scale                  = _kernel.LoadFunction("Scale");
        }

		protected virtual void Dispose(bool disposing)
		{
			if (disposing && !_disposed) {
				foreach(var item in _convolutionDataCache)
					item.Value.Dispose();
                _convolutionDataCache.Clear();

				if(_blas.IsValueCreated)
				    _blas.Value.Dispose();
				if(_solver.IsValueCreated)
					_solver.Value.Dispose();
                _defaultStream.Dispose();
                _memoryPool.Dispose();
				_cuda.Dispose();
                _disposed = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
        }

        public string Name { get; } = "Cuda";
        BrightDataContext IHaveBrightDataContext.Context => _context;
        public BrightDataContext DataContext => _context;
        internal CudaContext Context => _cuda;
        public CudaBlas Blas => _blas.Value;
        public CudaSolveDense Solver => _solver.Value;
		public long TotalMemory => _cuda.GetTotalDeviceMemorySize();
		public long FreeMemory => _cuda.GetFreeDeviceMemorySize();

        static int GetBlockCount(int size, int blockSize)
		{
			return (size / blockSize) + 1;
		}

		internal static void CheckForError(CUResult result)
		{
			if (result != CUResult.Success) {
				string errorName = "", errorDesc = "";
				IntPtr errorNamePtr = IntPtr.Zero, errorDescPtr = IntPtr.Zero;
				if (DriverAPINativeMethods.ErrorHandling.cuGetErrorName(result, ref errorNamePtr) == CUResult.Success && errorNamePtr != IntPtr.Zero)
					errorName = Marshal.PtrToStringUni(errorNamePtr) ?? "Unknown error";
				if(DriverAPINativeMethods.ErrorHandling.cuGetErrorString(result, ref errorDescPtr) == CUResult.Success && errorDescPtr != IntPtr.Zero)
					errorDesc = Marshal.PtrToStringUni(errorDescPtr) ?? "??";
					
				throw new Exception($"{result}: {errorName}-{errorDesc}");
			}
		}

		void Invoke(CUfunction function, CUstream* stream, uint size, params object[] param)
		{
			if (!_blockSize.TryGetValue(function, out var data)) {
				int blockSize = 0, minGridSize = 0;
				DriverAPINativeMethods.Occupancy.cuOccupancyMaxPotentialBlockSize(ref minGridSize, ref blockSize, function, bs => 0, 0, 0);
				_blockSize.TryAdd(function, data = (blockSize, minGridSize));
			}
			var gridSize = (size + data.BlockSize - 1) / data.BlockSize;
			var execution = KernelModule.CreateExecution(function, (int)gridSize, data.BlockSize);
			execution.Run(stream is not null ? *stream : _defaultStream.Stream, 0, param);
		}

        void InvokeManual(CUfunction function, CUstream* stream, uint size, params object[] param)
		{
			var gridSize = GetBlockCount((int)size, N);
			var execution = KernelModule.CreateExecution(function, gridSize, N);
			execution.Run(stream is not null ? *stream : _defaultStream.Stream, 0, param);
		}

        internal void InvokeMatrix(CUfunction function, CUstream* stream, uint rows, uint columns, params object[] param)
		{
			if (!_blockSize.TryGetValue(function, out var data)) {
				int blockSize = 0, minGridSize = 0;
				DriverAPINativeMethods.Occupancy.cuOccupancyMaxPotentialBlockSize(ref minGridSize, ref blockSize, function, bs => 0, 0, 0);
				_blockSize.TryAdd(function, data = (Convert.ToInt32(System.Math.Pow(blockSize, 1.0/2)), minGridSize));
			}
			var gridSizeRows = (rows + data.BlockSize - 1) / data.BlockSize;
			var gridSizeCols = (columns + data.BlockSize - 1) / data.BlockSize;
			var execution = KernelModule.CreateExecution(function, new dim3((int)gridSizeRows, (int)gridSizeCols), new dim3(data.BlockSize, data.BlockSize));
			
			execution.Run(stream is not null ? *stream : _defaultStream.Stream, 0, param);
		}

        internal void InvokeTensor(CUfunction function, CUstream* stream, uint rows, uint columns, uint depth, params object[] param)
		{
			if (!_blockSize.TryGetValue(function, out var data)) {
				int blockSize = 0, minGridSize = 0;
				DriverAPINativeMethods.Occupancy.cuOccupancyMaxPotentialBlockSize(ref minGridSize, ref blockSize, function, bs => 0, 0, 0);
				_blockSize.TryAdd(function, data = (Convert.ToInt32(System.Math.Pow(blockSize, 1.0/3)), minGridSize));
			}
			var gridSizeRows = (rows + data.BlockSize - 1) / data.BlockSize;
			var gridSizeCols = (columns + data.BlockSize - 1) / data.BlockSize;
			var gridSizeDepth = (depth + data.BlockSize - 1) / data.BlockSize;
			var execution = KernelModule.CreateExecution(function, new dim3((int)gridSizeRows, (int)gridSizeCols, (int)gridSizeDepth), new dim3(data.BlockSize, data.BlockSize, data.BlockSize));
			execution.Run(stream is not null ? *stream : _defaultStream.Stream, 0, param);
		}

		internal bool IsFinite(IDeviceMemoryPtr a, uint size, uint ai = 1, CUstream* stream = null)
		{
			var temp = Allocate(size, stream);
			try {
				Invoke(_isFinite, stream, size, a.DevicePointer, temp.DevicePointer, size, ai);
				var sum = Blas.AbsoluteSum(temp.DeviceVariable, 1);
				return FloatMath.IsZero(sum);
			}
			finally {
				temp.Release();
			}
		}

		internal IDeviceMemoryPtr PointwiseMultiply(IDeviceMemoryPtr a, IDeviceMemoryPtr b, uint size, uint ai = 1, uint bi = 1, CUstream* stream = null)
		{
			var ret = Allocate(size, stream);
			ret.CopyToDevice(b);
			Invoke(_pointwiseMultiply, stream, size, a.DevicePointer, ret.DevicePointer, size, ai, bi);
			return ret;
		}

		internal IDeviceMemoryPtr PointwiseDivide(IDeviceMemoryPtr a, IDeviceMemoryPtr b, uint size, uint ai = 1, uint bi = 1, CUstream* stream = null)
		{
			var ret = Allocate(size, stream);
			ret.CopyToDevice(b);
			Invoke(_pointwiseDivide, stream, size, a.DevicePointer, ret.DevicePointer, size, ai, bi);
			return ret;
		}

        internal void AddInPlace(IDeviceMemoryPtr a, IDeviceMemoryPtr b, uint size, float coefficient1, float coefficient2, uint ai = 1, uint bi = 1, CUstream* stream = null)
		{
			Invoke(_addInPlace, stream, size, a.DevicePointer, b.DevicePointer, size, coefficient1, coefficient2, ai, bi);
		}

		internal void SubtractInPlace(IDeviceMemoryPtr a, IDeviceMemoryPtr b, uint size, float coefficient1, float coefficient2, uint ai = 1, uint bi = 1, CUstream* stream = null)
		{
			Invoke(_subtractInPlace, stream, size, a.DevicePointer, b.DevicePointer, size, coefficient1, coefficient2, ai, bi);
		}

		internal void AddToEachRow(IDeviceMemoryPtr matrix, IDeviceMemoryPtr vector, uint rows, uint columns, CUstream* stream = null)
		{
			InvokeMatrix(_addToEachRow, stream, rows, columns, matrix.DevicePointer, vector.DevicePointer, rows, columns);
		}

		internal void AddToEachColumn(IDeviceMemoryPtr matrix, IDeviceMemoryPtr vector, uint rows, uint columns, CUstream* stream = null)
		{
			InvokeMatrix(_addToEachColumn, stream, rows, columns, matrix.DevicePointer, vector.DevicePointer, rows, columns);
		}

        internal void MultiplyByEachRow(IDeviceMemoryPtr matrix, IDeviceMemoryPtr vector, uint rows, uint columns, CUstream* stream = null)
        {
            InvokeMatrix(_multiplyByEachRow, stream, rows, columns, matrix.DevicePointer, vector.DevicePointer, rows, columns);
        }

        internal void MultiplyByEachColumn(IDeviceMemoryPtr matrix, IDeviceMemoryPtr vector, uint rows, uint columns, CUstream* stream = null)
        {
            InvokeMatrix(_multiplyByEachColumn, stream, rows, columns, matrix.DevicePointer, vector.DevicePointer, rows, columns);
        }

		internal IDeviceMemoryPtr TanH(IDeviceMemoryPtr a, uint size, uint ai = 1, uint bi = 1, CUstream* stream = null)
		{
			var ret = Allocate(size, stream);
			Invoke(_tanh, stream, size, a.DevicePointer, ret.DevicePointer, size, ai, bi);
			return ret;
		}

		internal IDeviceMemoryPtr TanHDerivative(IDeviceMemoryPtr a, uint size, uint ai = 1, uint bi = 1, CUstream* stream = null)
		{
			var ret = Allocate(size, stream);
			Invoke(_tanhDerivative, stream, size, a.DevicePointer, ret.DevicePointer, size, ai, bi);
			return ret;
		}

		internal IDeviceMemoryPtr Sigmoid(IDeviceMemoryPtr a, uint size, uint ai = 1, uint bi = 1, CUstream* stream = null)
		{
			var ret = Allocate(size, stream);
			Invoke(_sigmoid, stream, size, a.DevicePointer, ret.DevicePointer, size, ai, bi);
			return ret;
		}

		internal IDeviceMemoryPtr SigmoidDerivative(IDeviceMemoryPtr a, uint size, uint ai = 1, uint bi = 1, CUstream* stream = null)
		{
			var ret = Allocate(size, stream);
			Invoke(_sigmoidDerivative, stream, size, a.DevicePointer, ret.DevicePointer, size, ai, bi);
			return ret;
		}

		internal IDeviceMemoryPtr Relu(IDeviceMemoryPtr a, uint size, uint ai = 1, uint bi = 1, CUstream* stream = null)
		{
			var ret = Allocate(size, stream);
			Invoke(_relu, stream, size, a.DevicePointer, ret.DevicePointer, size, ai, bi);
			return ret;
		}

		internal IDeviceMemoryPtr ReluDerivative(IDeviceMemoryPtr a, uint size, uint ai = 1, uint bi = 1, CUstream* stream = null)
		{
			var ret = Allocate(size, stream);
			Invoke(_reluDerivative, stream, size, a.DevicePointer, ret.DevicePointer, size, ai, bi);
			return ret;
		}

		internal IDeviceMemoryPtr LeakyRelu(IDeviceMemoryPtr a, uint size, uint ai = 1, uint bi = 1, CUstream* stream = null)
		{
			var ret = Allocate(size, stream);
			Invoke(_leakyRelu, stream, size, a.DevicePointer, ret.DevicePointer, size, ai, bi);
			return ret;
		}

		internal IDeviceMemoryPtr LeakyReluDerivative(IDeviceMemoryPtr a, uint size, uint ai = 1, uint bi = 1, CUstream* stream = null)
		{
			var ret = Allocate(size, stream);
			Invoke(_leakyReluDerivative, stream, size, a.DevicePointer, ret.DevicePointer, size, ai, bi);
			return ret;
		}

		internal IDeviceMemoryPtr SumRows(IDeviceMemoryPtr a, uint rows, uint columns, IDeviceMemoryPtr? ret, CUstream* stream = null)
		{
			ret ??= Allocate(rows, stream, true);
			InvokeMatrix(_sumRows, stream, rows, columns, a.DevicePointer, ret.DevicePointer, rows, columns);
			return ret;
		}

		internal IDeviceMemoryPtr SumColumns(IDeviceMemoryPtr a, uint rows, uint columns, IDeviceMemoryPtr? ret, CUstream* stream = null)
		{
			ret ??= Allocate(columns, stream, true);
			InvokeMatrix(_sumColumns, stream, rows, columns, a.DevicePointer, ret.DevicePointer, rows, columns);
			return ret;
		}

		internal void MemSet(IDeviceMemoryPtr data, float value, uint count, CUstream* stream = null, uint offset = 0, uint increment = 1)
		{
			Invoke(_memSet, stream, count, data.DevicePointer, value, count, offset, increment);
		}

		internal IDeviceMemoryPtr Sqrt(IDeviceMemoryPtr a, uint size, float valueAdjustment, uint ai = 1, uint bi = 1, CUstream* stream = null)
		{
			var ret = Allocate(size, stream);
			Invoke(_sqrt, stream, size, a.DevicePointer, ret.DevicePointer, size, valueAdjustment, ai, bi);
			return ret;
		}

		internal IDeviceMemoryPtr Abs(IDeviceMemoryPtr a, uint size, uint ai = 1, uint bi = 1, CUstream* stream = null)
		{
			var ret = Allocate(size, stream);
			Invoke(_abs, stream, size, a.DevicePointer, ret.DevicePointer, size, ai, bi);
			return ret;
		}

		internal IDeviceMemoryPtr Log(IDeviceMemoryPtr a, uint size, uint ai = 1, uint bi = 1, CUstream* stream = null)
		{
			var ret = Allocate(size, stream);
            InvokeManual(_log, stream, size, a.DevicePointer, ret.DevicePointer, size, ai, bi);
			return ret;
		}

        internal IDeviceMemoryPtr Exp(IDeviceMemoryPtr a, uint size, uint ai = 1, uint bi = 1, CUstream* stream = null)
        {
            var ret = Allocate(size, stream);
            InvokeManual(_exp, stream, size, a.DevicePointer, ret.DevicePointer, size, ai, bi);
            return ret;
        }

		internal void VectorAddInPlace(IDeviceMemoryPtr a, uint size, float scalar, uint ai = 1, CUstream* stream = null)
		{
			Invoke(_vectorAddInPlace, stream, size, a.DevicePointer, size, scalar, ai);
		}

		internal IDeviceMemoryPtr VectorCopy(IDeviceMemoryPtr a, uint[] indexList, uint ai = 1, uint bi = 1, CUstream* stream = null)
		{
			var retSize = (uint)indexList.Length;
			var ret = Allocate(retSize, stream);
            using var indexGpu = new CudaDeviceVariable<uint>(retSize);
            indexGpu.CopyToDevice(indexList);
            Invoke(_vectorCopyRandom, stream, retSize, a.DevicePointer, ret.DevicePointer, indexGpu.DevicePointer, retSize, ai, bi);
            return ret;
        }

        internal (float Min, float Max) FindMinAndMax(IDeviceMemoryPtr a, uint size, uint ai = 1, CUstream* stream = null)
        {
            if (size > 0) {
                var ptr = a;
                while (size >= N) {
                    var bufferSize = (size / N) + 1;
                    var minBlock = Allocate(bufferSize, stream, true);
                    var maxBlock = Allocate(bufferSize, stream, true);

                    try {
                        InvokeManual(_findMinAndMax, stream, size, ptr.DevicePointer, size, minBlock.DevicePointer, maxBlock.DevicePointer, ai);
                        if (ptr != a)
                            ptr.Release();

                        ai = 1;
                        size = bufferSize * 2;
                        ptr = Allocate(size, stream);
                        ptr.DeviceVariable.CopyToDevice(minBlock.DeviceVariable, 0, 0, bufferSize * FloatSize);
                        ptr.DeviceVariable.CopyToDevice(maxBlock.DeviceVariable, 0, bufferSize * FloatSize, bufferSize * FloatSize);
                    }
                    finally {
                        minBlock.Release();
                        maxBlock.Release();
                    }
                }
                var data = new float[size];
                ptr.CopyToHost(data);
                if (ptr != a)
                    ptr.Release();
                float min = float.MaxValue, max = float.MinValue;
                for (var i = 0; i < size; i++) {
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

		internal float SumValues(IDeviceMemoryPtr a, uint size, uint ai = 1, CUstream* stream = null)
		{
			var ptr = a;
			while (size >= N) {
				var bufferSize = (size / N) + 1;
				var sumBlock = Allocate(bufferSize, stream, true);
                InvokeManual(_sumValues, stream, size, ptr.DevicePointer, size, sumBlock.DevicePointer, ai);
				if (ptr != a)
					ptr.Release();
				size = bufferSize;
				ptr = sumBlock;
                ai = 1;
            }
			var total = new float[size];
			ptr.CopyToHost(total);
			if (ptr != a)
				ptr.Release();
			return total.Sum();
		}

		internal float FindStdDev(IDeviceMemoryPtr a, uint size, float mean, uint ai = 1, CUstream* stream = null)
		{
			var inputSize = size;
			if (size > 0) {
				var ptr = a;
				while (size >= N) {
					var bufferSize = (size / N) + 1;
					var sumBlock = Allocate(bufferSize, stream, true);
                    InvokeManual(_findStdDev, stream, size, ptr.DevicePointer, size, mean, sumBlock.DevicePointer, ai);
					if (ptr != a)
						ptr.Release();
					size = bufferSize;
					ptr = sumBlock;
                    ai = 1;
                }
				var total = new float[size];
				ptr.CopyToHost(total);
				if (ptr != a)
					ptr.Release();

				return Convert.ToSingle(System.Math.Sqrt(total.Sum() / inputSize));
			}
			return 0f;
		}

		internal void Constrain(IDeviceMemoryPtr a, uint size, float min, float max, uint ai = 1, CUstream* stream = null)
		{
			Invoke(_constrain, stream, size, a.DevicePointer, size, min, max, ai);
		}
        
        public void RoundInPlace(IDeviceMemoryPtr a, uint size, float lower, float upper, float mid, uint ai = 1, CUstream* stream = null)
        {
            Invoke(_roundInPlace, stream, size, a.DevicePointer, size, lower, upper, mid, ai);
        }

		internal IDeviceMemoryPtr Pow(IDeviceMemoryPtr a, uint size, float power, uint ai = 1, uint bi = 1, CUstream* stream = null)
		{
			var ret = Allocate(size, stream);
			Invoke(_pow, stream, size, a.DevicePointer, ret.DevicePointer, size, power, ai, bi);
			return ret;
		}

		internal IDeviceMemoryPtr Diagonal(IDeviceMemoryPtr a, uint rows, uint columns, uint ai = 1, uint bi = 1, CUstream* stream = null)
		{
			var size = System.Math.Min(rows, columns);
			var ret = Allocate(size, stream);
			Invoke(_diagonal, stream, size, a.DevicePointer, ret.DevicePointer, rows, columns, ai, bi);
			return ret;
		}

		internal void L1Regularisation(IDeviceMemoryPtr a, uint size, float coefficient, uint ai = 1, CUstream* stream = null)
		{
			Invoke(_l1Regularisation, stream, size, a.DevicePointer, size, coefficient, ai);
		}

        internal float EuclideanDistance(IDeviceMemoryPtr a, IDeviceMemoryPtr b, uint size, uint ai = 1, uint bi = 1, uint ci = 1, CUstream* stream = null)
		{
			var ret = Allocate(size, stream);
			Invoke(_euclideanDistance, stream, size, a.DevicePointer, b.DevicePointer, ret.DevicePointer, size, ai, bi, ci);
			return Convert.ToSingle(System.Math.Sqrt(SumValues(ret, size)));
		}

		internal float ManhattanDistance(IDeviceMemoryPtr a, IDeviceMemoryPtr b, uint size, uint ai = 1, uint bi = 1, uint ci = 1, CUstream* stream = null)
		{
			var ret = Allocate(size, stream);
			Invoke(_manhattanDistance, stream, size, a.DevicePointer, b.DevicePointer, ret.DevicePointer, size, ai, bi, ci);
			return SumValues(ret, size);
		}

        internal float CosineDistance(IDeviceMemoryPtr a, IDeviceMemoryPtr b, uint size, uint ai = 1, uint bi = 1, CUstream* stream = null)
		{
			var buffer = Allocate(3, stream);
            var aaDevice = buffer.DevicePointer;
			var abDevice = aaDevice + sizeof(float);
			var bbDevice = abDevice + sizeof(float);
			try {
				Invoke(_cosineDistance, stream, size, a.DevicePointer, b.DevicePointer, aaDevice, abDevice, bbDevice, size, ai, bi);
                var local = new float[3];
				buffer.CopyToHost(local);
				var aa = local[0];
                var ab = local[1];
                var bb = local[2];

				if (aa.Equals(0f))
					return bb.Equals(0f) ? 1.0f : 0.0f;
				else if (bb.Equals(0f))
					return 0.0f;
				else
					return 1f - (ab / (float)System.Math.Sqrt(aa) / (float)System.Math.Sqrt(bb));
			}
			finally {
                buffer.Release();
            }
		}

		internal void Normalise(IDeviceMemoryPtr a, uint size, float min, float range, uint ai = 1, CUstream* stream = null)
		{
			Invoke(_normalise, stream, size, a.DevicePointer, size, min, range, ai);
		}

        internal void ScaleInPlace(IDeviceMemoryPtr a, uint size, float scaleBy, uint ai = 1, CUstream* stream = null)
        {
            Invoke(_scale, stream, size, a.DevicePointer, size, scaleBy, ai);
        }

		internal IDeviceMemoryPtr SoftmaxVector(IDeviceMemoryPtr a, uint size, float max, uint ai = 1, CUstream* stream = null)
		{
			var ret = Allocate(size, stream);
			Invoke(_softmaxVector, stream, size, a.DevicePointer, ret.DevicePointer, size, max, ai);
			return ret;
		}

		internal void PointwiseDivideRows(IDeviceMemoryPtr a, IDeviceMemoryPtr b, uint rows, uint columns, CUstream* stream = null)
		{
			InvokeMatrix(_pointwiseDivideRows, stream, rows, columns, a.DevicePointer, b.DevicePointer, rows, columns);
		}

		internal void PointwiseDivideColumns(IDeviceMemoryPtr a, IDeviceMemoryPtr b, uint rows, uint columns, CUstream* stream = null)
		{
			InvokeMatrix(_pointwiseDivideColumns, stream, rows, columns, a.DevicePointer, b.DevicePointer, rows, columns);
		}

		internal void SplitRows(IDeviceMemoryPtr a, IDeviceMemoryPtr b, IDeviceMemoryPtr c, uint rows, uint columns, uint position, CUstream* stream = null)
		{
			InvokeMatrix(_splitRows, stream, rows, columns, a.DevicePointer, b.DevicePointer, c.DevicePointer, rows, columns, position);
		}

		internal void SplitColumns(IDeviceMemoryPtr a, IDeviceMemoryPtr b, IDeviceMemoryPtr c, uint rows, uint columns, uint position, CUstream* stream = null)
		{
			InvokeMatrix(_splitColumns, stream, rows, columns, a.DevicePointer, b.DevicePointer, c.DevicePointer, rows, columns, position);
		}

		internal void ConcatRows(IDeviceMemoryPtr a, IDeviceMemoryPtr b, IDeviceMemoryPtr c, uint rows, uint columns, uint leftColumnCount, CUstream* stream = null)
		{
			InvokeMatrix(_concatRows, stream, rows, columns, a.DevicePointer, b.DevicePointer, c.DevicePointer, rows, columns, leftColumnCount);
		}

		internal void ConcatColumns(IDeviceMemoryPtr a, IDeviceMemoryPtr b, IDeviceMemoryPtr c, uint rows, uint columns, uint topRowCount, uint bottomRowCount, CUstream* stream = null)
		{
			InvokeMatrix(_concatColumns, stream, rows, columns, a.DevicePointer, b.DevicePointer, c.DevicePointer, rows, columns, topRowCount, bottomRowCount);
		}

        internal (IDeviceMemoryPtr Data, uint Rows, uint Columns) TensorAddPadding(
            IDeviceMemoryPtr tensor, 
			uint rows, 
			uint columns, 
			uint depth, 
			uint count, 
			uint padding,
            CUstream* stream = null
		) {
			var outputRows = rows + padding * 2;
			var outputColumns = columns + padding * 2;
			var ret = Allocate(outputRows * outputColumns * depth * count, stream, true);

			Invoke(_tensorAddPadding, stream, ret.Size,
				ret.Size,
				tensor.DevicePointer,
				ret.DevicePointer, 
				rows, 
				columns, 
				depth, 
				count,
				outputRows, 
				outputColumns, 
				padding
			);

			return (ret, outputRows, outputColumns);
		}

		internal (IDeviceMemoryPtr Data, uint Rows, uint Columns) TensorRemovePadding(
            IDeviceMemoryPtr tensor, 
			uint rows, 
			uint columns, 
			uint depth, 
			uint count, 
			uint padding,
            CUstream* stream = null
		) {
			var outputRows = rows - padding * 2;
			var outputColumns = columns - padding * 2;
			var ret = Allocate(outputRows * outputColumns * depth * count, stream);
			var size = rows * columns * depth * count;

			Invoke(_tensorRemovePadding, stream, size,
				size,
				tensor.DevicePointer, 
				ret.DevicePointer, 
				rows, 
				columns, 
				depth, 
				count,
				outputRows, 
				outputColumns, 
				padding
			);

			return (ret, outputRows, outputColumns);
		}

		internal IDeviceMemoryPtr VectorSoftmaxDerivative(IDeviceMemoryPtr a, uint size, uint ai = 1, CUstream* stream = null)
		{
			var ret = Allocate(size * size, stream);
			InvokeMatrix(_softmaxDerivative, stream, size, size, a.DevicePointer, ret.DevicePointer, size, ai);
			return ret;
		}

		internal IDeviceMemoryPtr Reverse(IDeviceMemoryPtr a, uint size, CUstream* stream = null)
		{
			var ret = Allocate(size, stream);
			Invoke(_reverse, stream, size, a.DevicePointer, ret.DevicePointer, size);
			return ret;
		}

		internal void RotateInPlace(IDeviceMemoryPtr a, uint size, uint blockCount, CUstream* stream = null)
		{
			var blockSize = size / blockCount;
			Invoke(_rotateInPlace, stream, size, a.DevicePointer, size, blockCount, blockSize);
		}

		internal (IDeviceMemoryPtr Data, IDeviceMemoryPtr? Indices, uint Rows, uint Columns) TensorMaxPool(
            IDeviceMemoryPtr tensor, 
			uint rows, 
			uint columns, 
			uint depth, 
			uint count,
			uint filterWidth,
			uint filterHeight,
			uint xStride,
			uint yStride,
			bool saveIndices, 
            CUstream* stream = null
		) {
			var outputColumns = (columns - filterWidth) / xStride + 1;
			var outputRows = (rows - filterHeight) / yStride + 1;
			var outputMatrixSize = outputColumns * outputRows;
			var ret = Allocate(outputMatrixSize * depth * count, stream, true);
			var indices = saveIndices ? Allocate(outputMatrixSize * depth * count, stream, true) : null;
			var convolutions = GetConvolutions(rows, columns, filterWidth, filterHeight, xStride, yStride);
			var size = (uint)convolutions.Count * depth * count;

            Invoke(_tensorMaxPool, stream, size,
                size,
                tensor.DevicePointer,
                ret.DevicePointer,
                indices?.DevicePointer ?? new CUdeviceptr(),
                convolutions.X.DevicePointer,
                convolutions.Y.DevicePointer,
                convolutions.Count,
                rows,
                columns,
                depth,
                count,
                outputRows,
                outputColumns,
                filterWidth,
                filterHeight,
                xStride,
                yStride,
                saveIndices ? 1 : 0
            );

            return (ret, indices, outputRows, outputColumns);
        }

		internal IDeviceMemoryPtr TensorReverseMaxPool(
            IDeviceMemoryPtr tensor, 
            IDeviceMemoryPtr indices, 
            uint rows, 
            uint columns, 
            uint depth, 
            uint count, 
            uint outputRows, 
            uint outputColumns, 
            uint filterWidth, 
            uint filterHeight, 
            uint xStride, 
            uint yStride, 
            CUstream* stream = null
        )
		{
			var ret = Allocate(outputRows * outputColumns * depth * count, stream, true);
			var size = rows * columns * depth * count;

			Invoke(_tensorReverseMaxPool, stream, size,
				size,
				tensor.DevicePointer, 
				indices.DevicePointer,
				ret.DevicePointer, 
				rows, 
				columns, 
				depth, 
				count,
				outputRows, 
				outputColumns, 
				filterWidth, 
				filterHeight,
				xStride,
				yStride
            );

			return ret;
		}

		internal (IDeviceMemoryPtr Data, uint Rows, uint Columns, uint Depth) TensorIm2Col(
            IDeviceMemoryPtr tensor, 
			uint rows, 
			uint columns, 
			uint depth, 
			uint count, 
			uint filterWidth, 
			uint filterHeight, 
			uint xStride, 
			uint yStride, 
            CUstream* stream = null
		) {
			var convolutions = GetConvolutions(rows, columns, filterWidth, filterHeight, xStride, yStride);
			var filterSize = filterWidth * filterHeight;
			var outputRows = (uint)convolutions.Count;
			var outputColumns = filterSize * depth;
			var ret = Allocate(outputRows * outputColumns * count, stream, true);

            Invoke(_tensorIm2Col, stream, ret.Size,
                ret.Size,
                tensor.DevicePointer,
                ret.DevicePointer,
                convolutions.X.DevicePointer,
                convolutions.Y.DevicePointer,
                rows,
                columns,
                depth,
                count,
                outputRows,
                outputColumns,
                convolutions.Count,
                filterWidth,
                filterHeight,
                xStride,
                yStride
            );
            return (ret, outputRows, outputColumns, count);
        }

		internal (IDeviceMemoryPtr Data, uint Rows, uint Columns, uint Depth, uint Count) TensorReverseIm2Col(
            IDeviceMemoryPtr tensor,
			IDeviceMemoryPtr filters,
			uint rows,
			uint columns,
			uint depth,
			uint count,
			uint outputRows, 
			uint outputColumns,
			uint outputDepth,
			uint filterWidth,
			uint filterHeight,
			uint xStride, 
			uint yStride, 
            CUstream* stream = null
		) {
			var ret = Allocate(outputRows * outputColumns * outputDepth * count, stream, true);

            var convolutions = GetConvolutions(outputRows, outputColumns, filterWidth, filterHeight, xStride, yStride);
            var size = depth * convolutions.Count * filterHeight * filterWidth * outputDepth * count;
            Invoke(_tensorReverseIm2Col, stream, size,
                size,
                tensor.DevicePointer,
                filters.DevicePointer,
                ret.DevicePointer,
                convolutions.X.DevicePointer,
                convolutions.Y.DevicePointer,
                rows,
                columns,
                depth,
                count,
                convolutions.Count,
                filterWidth,
                filterHeight,
                xStride,
                yStride,
                outputRows,
                outputColumns,
                outputDepth
            );

            return (ret, outputRows, outputColumns, outputDepth, count);
		}

        ConvolutionsData GetConvolutions(
            uint outputRows, 
            uint outputColumns,
            uint filterWidth,
            uint filterHeight,
            uint xStride, 
            uint yStride)
        {
            var list = ConvolutionHelper.Default(outputColumns, outputRows, filterWidth, filterHeight, xStride, yStride);
            if (!_convolutionDataCache.TryGetValue(list, out var ret))
                _convolutionDataCache.Add(list, ret = new ConvolutionsData(this, list));
            return ret;
        }

        //      public IFloatVector CreateVector(ITensorSegment<float> data) => CreateVector(data.Size, i => data[i]);

        //      public IFloatVector CreateVector(uint length, bool setToZero = false)
        //{
        //	var data = Allocate(length, setToZero);
        //	return new CudaVector(this, data, true);
        //}

        //public IFloatVector CreateVector(uint length, Func<uint, float> init)
        //{
        //	using var data = MemoryOwner<float>.Allocate((int)length);
        //          var dataArray = data.DangerousGetArray().Array!;
        //	for (uint i = 0; i < length; i++)
        //              dataArray[i] = init(i);
        //	var ptr = Allocate(length);
        //	ptr.CopyToDevice(dataArray);

        //	return new CudaVector(this, ptr, true);
        //}

        

        //public IFloatMatrix CreateMatrixFromRows(IFloatVector[] vectorRows)
        //{
        //	var rows = (uint)vectorRows.Length;
        //	var columns = vectorRows[0].Count;

        //	var ret = Allocate(rows * columns);
        //	using (var devicePtr = new CudaDeviceVariable<CUdeviceptr>(rows)) {
        //		devicePtr.CopyToDevice(vectorRows.Cast<IHaveDeviceMemory>().Select(d => d.Memory.DevicePointer).ToArray());
        //              CopyToMatrixRows(rows, columns, devicePtr, ret);
        //          }
        //	return new CudaMatrix(this, rows, columns, ret, true);
        //}

        public void CopyToMatrixRows(uint rows, uint columns, CudaDeviceVariable<CUdeviceptr> from, IDeviceMemoryPtr to, CUstream* stream = null)
        {
            InvokeMatrix(_copyToMatrixRows, stream, rows, columns, from.DevicePointer, to.DevicePointer, rows, columns);
        }

        //public IFloatMatrix CreateMatrixFromColumns(IFloatVector[] vectorColumns)
        //{
        //	var columns = (uint)vectorColumns.Length;
        //	var rows = vectorColumns[0].Count;

        //	var ret = Allocate(rows * columns);
        //	using (var devicePtr = new CudaDeviceVariable<CUdeviceptr>(columns)) {
        //		devicePtr.CopyToDevice(vectorColumns.Cast<IHaveDeviceMemory>().Select(d => d.Memory.DevicePointer).ToArray());
        //              CopyToMatrixColumns(rows, columns, devicePtr, ret);
        //          }
        //	return new CudaMatrix(this, rows, columns, ret, true);
        //}

        public void CopyToMatrixColumns(uint rows, uint columns, CudaDeviceVariable<CUdeviceptr> from, IDeviceMemoryPtr to, CUstream* stream = null)
        {
            InvokeMatrix(_copyToMatrixColumns, stream, rows, columns, from.DevicePointer, to.DevicePointer, rows, columns);
        }

        //public IFloatMatrix CreateMatrix(uint rows, uint columns, Func<uint, uint, float> init)
        //{
        //	var size = rows * columns;
        //	using var data = SpanOwner<float>.Allocate((int)size);
        //          var dataArray = data.DangerousGetArray().Array!;
        //	for (uint j = 0; j < columns; j++) {
        //		for (uint i = 0; i < rows; i++) {
        //                  dataArray[j * rows + i] = init(i, j);
        //		}
        //	}
        //	var ptr = Allocate(size);
        //	ptr.CopyToDevice(dataArray);
        //	return new CudaMatrix(this, rows, columns, ptr, true);
        //}

        //public I3DFloatTensor Create3DTensor(uint rows, uint columns, uint depth, bool setToZero = false)
        //{
        //	var data = Allocate(rows * columns * depth, setToZero);
        //	return new Cuda3DTensor(this, rows, columns, depth, data, true);
        //}

        //public I3DFloatTensor Create3DTensor(params IFloatMatrix[] matrices)
        //{
        //	var depth = (uint)matrices.Length;
        //	var first = matrices[0];
        //	var rows = first.RowCount;
        //	var columns = first.ColumnCount;
        //	var outputRows = rows * columns;
        //	var outputColumns = depth;

        //	var ret = Allocate(rows * columns * depth);
        //	using (var devicePtr = new CudaDeviceVariable<CUdeviceptr>(depth)) {
        //		devicePtr.CopyToDevice(matrices.Cast<IHaveDeviceMemory>().Select(d => d.Memory.DevicePointer).ToArray());
        //		InvokeMatrix(_copyToMatrixColumns, outputRows, outputColumns, devicePtr.DevicePointer, ret.DevicePointer, outputRows, outputColumns);
        //	}
        //	return new Cuda3DTensor(this, rows, columns, depth, ret, true);
        //}

        //public I4DFloatTensor Create4DTensor(uint rows, uint columns, uint depth, uint count, bool setToZero = false)
        //{
        //	var data = Allocate(rows * columns * depth * count, setToZero);
        //	return new Cuda4DTensor(this, rows, columns, depth, count, data, true);
        //}

        //public I4DFloatTensor Create4DTensor(params I3DFloatTensor[] tensors)
        //{
        //	var count = (uint)tensors.Length;
        //	var first = tensors[0];
        //	var rows = first.RowCount;
        //	var columns = first.ColumnCount;
        //	var depth = first.Depth;
        //	var outputRows = rows * columns * depth;
        //	var outputColumns = count;

        //	var ret = Allocate(rows * columns * depth * count);
        //	using (var devicePtr = new CudaDeviceVariable<CUdeviceptr>(count)) {
        //		devicePtr.CopyToDevice(tensors.Cast<IHaveDeviceMemory>().Select(d => d.Memory.DevicePointer).ToArray());
        //		InvokeMatrix(_copyToMatrixColumns, outputRows, outputColumns, devicePtr.DevicePointer, ret.DevicePointer, outputRows, outputColumns);
        //	}
        //	return new Cuda4DTensor(this, rows, columns, depth, count, ret, true);
        //}

        //public I4DFloatTensor Create4DTensor(params Tensor3D<float>[] tensors)
        //{
        //	var first = tensors[0];
        //	var data = Allocate(first.RowCount * first.ColumnCount * first.Depth * (uint)tensors.Length);
        //	var ret = new Cuda4DTensor(this, first.RowCount, first.ColumnCount, first.Depth, (uint)tensors.Length, data, true);

        //	for (int i = 0; i < tensors.Length; i++)
        //		ret.GetTensorAt((uint)i).Data = tensors[i];
        //	return ret;
        //}

        public IDeviceMemoryPtr Allocate(uint size, CUstream* stream = null, bool setToZero = false)
        {
            IDeviceMemoryPtr ret;
            if (stream is null) {
                var devicePtr = _memoryPool.GetPtr(size * FloatSize);
                var ptr = new CudaDeviceVariable<float>(devicePtr, false);
                ret = new DeviceMemoryBlock(_memoryPool, ptr);
            }
            else
                ret = new StreamDeviceMemoryBlock(*stream, size, false);

            if (setToZero) {
				//MemClear(ret, size);
                ret.Clear();
            }

            return ret;
		}

		public void BindThread()
		{
			_cuda.SetCurrent();
		}

		public IDeviceMemoryPtr Offset(IDeviceMemoryPtr ptr, SizeT offsetByElements, SizeT size)
		{
			var offsetPtr = ptr.DevicePointer.Pointer + (offsetByElements * FloatSize);
			return new PtrToMemory(ptr, new CUdeviceptr(offsetPtr), size * FloatSize);
		}

		public IDeviceMemoryPtr OffsetByBlock(IDeviceMemoryPtr ptr, SizeT offsetIndex, SizeT blockSize)
		{
			return Offset(ptr, blockSize * offsetIndex, blockSize);
		}
    }
}
