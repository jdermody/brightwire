using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using BrightData.Cuda.CudaToolkit;
using BrightData.Cuda.CudaToolkit.Types;
using BrightData.Cuda.Helper;
using BrightData.Helper;
using Math = BrightData.Cuda.CudaToolkit.Math;

namespace BrightData.Cuda
{
    /// <summary>
    /// Manages the bright wire cuda kernels and implements the cuda linear algebra provider
    /// </summary>
    public unsafe class CudaProvider : IGpuLinearAlgebraProvider, IHaveBrightDataContext, IDisposable
	{
        readonly Dictionary<List<(uint X, uint Y)>, ConvolutionsData> _convolutionDataCache = [];
        const int BlockSize = 32;
		const int N = BlockSize * BlockSize;
        internal const int FloatSize = sizeof(float);

        class KernelExecution(CuFunction function, Dim3 block, Dim3 thread)
        {
            public void Run(CuStream stream, uint sharedMemSize, object[] param)
			{
				var paramList = new IntPtr[param.Length];
				var handleList = new GCHandle[param.Length];

				//Get pointers to kernel parameters
				for (var i = 0; i < param.Length; i++) {
					handleList[i] = GCHandle.Alloc(param[i], GCHandleType.Pinned);
					paramList[i] = handleList[i].AddrOfPinnedObject();
				}

				var result = DriverApiNativeMethods.Launch.cuLaunchKernel(function,
					block.X, block.Y, block.Z,
					thread.X, thread.Y, thread.Z,
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

		class KernelModule(CudaContext context, string path)
        {
			readonly CuModule _module = context.LoadModule(path);

            public CuFunction LoadFunction(string name)
			{
				var ret = new CuFunction();
				if (DriverApiNativeMethods.ModuleManagement.cuModuleGetFunction(ref ret, _module, name) != CuResult.Success)
					throw new ArgumentException("Function not found", name);
				return ret;
			}

			public static KernelExecution CreateExecution(CuFunction function, Dim3 block, Dim3 thread)
			{
				return new(function, block, thread);
			}
		}

		readonly CudaContext _cuda;
		readonly Lazy<CudaBlasHandle> _blas;
        readonly Lazy<CuSolverDnHandle> _solver;
		readonly KernelModule _kernel;
        readonly CuStream _defaultStream;
        readonly MemoryPool _memoryPool;

        readonly CuFunction
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
            _memCpy,
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
		readonly ConcurrentDictionary<CuFunction, (int BlockSize, int MinGridSize)> _blockSize = new();
		bool _disposed;

		/// <summary>
		/// Constructor - tries to find most relevant CUDA kernel if none specified
		/// </summary>
		/// <param name="context">Bright data context</param>
		/// <param name="cudaKernelPath">Path to CUDA kernel (optional)</param>
		/// <param name="cudaDirectory">Path to directory that contains CUDA kernel (optional)</param>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref="Exception"></exception>
		public CudaProvider(BrightDataContext context, string? cudaKernelPath, string? cudaDirectory)
        {
            DataContext = context;
            _cuda = new CudaContext();

            if (cudaKernelPath == null) {
                if (String.IsNullOrWhiteSpace(cudaDirectory))
                    throw new ArgumentException("No kernel path or directory was passed");

                int computeCapabilityMinor = 0, computeCapabilityMajor = 0;
                if(DriverApiNativeMethods.DeviceManagement.cuDeviceGetAttribute(ref computeCapabilityMajor, CuDeviceAttribute.ComputeCapabilityMajor, _cuda.Device) != CuResult.Success)
                    throw new Exception("Unable to get major version from CUDA device");
                if (DriverApiNativeMethods.DeviceManagement.cuDeviceGetAttribute(ref computeCapabilityMinor, CuDeviceAttribute.ComputeCapabilityMinor, _cuda.Device) != CuResult.Success)
                    throw new Exception("Unable to get minor version from CUDA device");

                var kernelName = $"brightwire_{computeCapabilityMajor}{computeCapabilityMinor}.ptx";
                cudaKernelPath = Path.Combine(cudaDirectory, kernelName);

				// try the default kernel
                if (!File.Exists(cudaKernelPath))
                    cudaKernelPath = Path.Combine(cudaDirectory, "brightwire.ptx");
                if (!File.Exists(cudaKernelPath))
                    throw new Exception($"No default kernel was found in {cudaDirectory}");
            }

            _memoryPool = new MemoryPool();
            if (DriverApiNativeMethods.Streams.cuStreamCreate(ref _defaultStream, CuStreamFlags.Default) != CuResult.Success)
                throw new Exception("Unable to create CUDA stream");

            _kernel = new KernelModule(_cuda, cudaKernelPath);
            _blas = new(() => {
                var ret = new CudaBlasHandle();
                if (CudaBlasNativeMethods.cublasCreate_v2(ref ret) != CuBlasStatus.Success)
                    throw new Exception("Unable to create CUDA BLAS");
                CudaBlasNativeMethods.cublasSetStream_v2(ret, _defaultStream);
                CudaBlasNativeMethods.cublasSetMathMode(ret, Math.Tf32TensorOpMath);
                CudaBlasNativeMethods.cublasSetAtomicsMode(ret, AtomicsMode.Allowed);
                return ret;
            });
            _solver = new(() => {
                var ret = new CuSolverDnHandle();
                if (CudaSolveNativeMethods.Dense.cusolverDnCreate(ref ret) != CuSolverStatus.Success)
                    throw new Exception("Unable to create CUDA solver");
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
            _memCpy                 = _kernel.LoadFunction("MemCpy");
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

		/// <summary>
		/// Dispose
		/// </summary>
		/// <param name="disposing"></param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing && !_disposed) {
				foreach(var item in _convolutionDataCache)
					item.Value.Dispose();
                _convolutionDataCache.Clear();

				if(_blas.IsValueCreated)
                    CudaBlasNativeMethods.cublasDestroy_v2(_blas.Value);
                if(_solver.IsValueCreated)
                    CudaSolveNativeMethods.Dense.cusolverDnDestroy(_solver.Value);
                DriverApiNativeMethods.Streams.cuStreamDestroy_v2(_defaultStream);
                _memoryPool.Dispose();
				_cuda.Dispose();
                _disposed = true;
			}
		}

        /// <inheritdoc />
        public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
        }

        BrightDataContext IHaveBrightDataContext.Context => DataContext;

        /// <summary>
        /// Bright data context
        /// </summary>
        public BrightDataContext DataContext { get; }

		/// <summary>
		/// CUDA context
		/// </summary>
        internal CudaContext Context => _cuda;


        /// <summary>
        /// CUDA BLAS provider
        /// </summary>
        public CudaBlasHandle Blas => _blas.Value;

		/// <summary>
		/// CUDA Solver
		/// </summary>
        public CuSolverDnHandle Solver => _solver.Value;

		/// <summary>
		/// Total device memory
		/// </summary>
		public long TotalMemory => _cuda.GetTotalDeviceMemorySize();

		/// <summary>
		/// Free device memory
		/// </summary>
		public long FreeMemory => _cuda.GetFreeDeviceMemorySize();

        static int GetBlockCount(int size, int blockSize)
		{
			return (size / blockSize) + 1;
		}

        /// <summary>
        /// Pushes a new CUDA context
        /// </summary>
        public void PushContext() => Context.PushContext();

		/// <summary>
		/// Pops a CUDA context
		/// </summary>
        public void PopContext() => Context.PopContext();

		internal static void CheckForError(CuResult result)
		{
			if (result != CuResult.Success) {
				string errorName = "", errorDesc = "";
				IntPtr errorNamePtr = IntPtr.Zero, errorDescPtr = IntPtr.Zero;
				if (DriverApiNativeMethods.ErrorHandling.cuGetErrorName(result, ref errorNamePtr) == CuResult.Success && errorNamePtr != IntPtr.Zero)
					errorName = Marshal.PtrToStringUni(errorNamePtr) ?? "Unknown error";
				if(DriverApiNativeMethods.ErrorHandling.cuGetErrorString(result, ref errorDescPtr) == CuResult.Success && errorDescPtr != IntPtr.Zero)
					errorDesc = Marshal.PtrToStringUni(errorDescPtr) ?? "??";
					
				throw new Exception($"{result}: {errorName}-{errorDesc}");
			}
		}

		void Invoke(CuFunction function, CuStream* stream, uint size, params object[] param)
		{
			if (!_blockSize.TryGetValue(function, out var data)) {
				int blockSize = 0, minGridSize = 0;
				DriverApiNativeMethods.Occupancy.cuOccupancyMaxPotentialBlockSize(ref minGridSize, ref blockSize, function, _ => 0, 0, 0);
				_blockSize.TryAdd(function, data = (blockSize, minGridSize));
			}
			var gridSize = (size + data.BlockSize - 1) / data.BlockSize;
			var execution = KernelModule.CreateExecution(function, (int)gridSize, data.BlockSize);
			execution.Run(stream is not null ? *stream : _defaultStream, 0, param);
		}

        void InvokeManual(CuFunction function, CuStream* stream, uint size, params object[] param)
		{
			var gridSize = GetBlockCount((int)size, N);
			var execution = KernelModule.CreateExecution(function, gridSize, N);
			execution.Run(stream is not null ? *stream : _defaultStream, 0, param);
		}

        internal void InvokeMatrix(CuFunction function, CuStream* stream, uint rows, uint columns, params object[] param)
		{
			if (!_blockSize.TryGetValue(function, out var data)) {
				int blockSize = 0, minGridSize = 0;
				DriverApiNativeMethods.Occupancy.cuOccupancyMaxPotentialBlockSize(ref minGridSize, ref blockSize, function, _ => 0, 0, 0);
				_blockSize.TryAdd(function, data = (Convert.ToInt32(System.Math.Pow(blockSize, 1.0/2)), minGridSize));
			}
			var gridSizeRows = (rows + data.BlockSize - 1) / data.BlockSize;
			var gridSizeCols = (columns + data.BlockSize - 1) / data.BlockSize;
			var execution = KernelModule.CreateExecution(function, new Dim3((int)gridSizeRows, (int)gridSizeCols), new Dim3(data.BlockSize, data.BlockSize));
			
			execution.Run(stream is not null ? *stream : _defaultStream, 0, param);
		}

        private void InvokeTensor(CuFunction function, CuStream* stream, uint rows, uint columns, uint depth, params object[] param)
		{
			if (!_blockSize.TryGetValue(function, out var data)) {
				int blockSize = 0, minGridSize = 0;
				DriverApiNativeMethods.Occupancy.cuOccupancyMaxPotentialBlockSize(ref minGridSize, ref blockSize, function, _ => 0, 0, 0);
				_blockSize.TryAdd(function, data = (Convert.ToInt32(System.Math.Pow(blockSize, 1.0/3)), minGridSize));
			}
			var gridSizeRows = (rows + data.BlockSize - 1) / data.BlockSize;
			var gridSizeCols = (columns + data.BlockSize - 1) / data.BlockSize;
			var gridSizeDepth = (depth + data.BlockSize - 1) / data.BlockSize;
			var execution = KernelModule.CreateExecution(function, new Dim3((int)gridSizeRows, (int)gridSizeCols, (int)gridSizeDepth), new Dim3(data.BlockSize, data.BlockSize, data.BlockSize));
			execution.Run(stream is not null ? *stream : _defaultStream, 0, param);
		}

		internal bool IsFinite(IDeviceMemoryPtr a, uint size, uint ai = 1, CuStream* stream = null)
		{
			var temp = Allocate(size, stream);
			try {
				Invoke(_isFinite, stream, size, a.DevicePointer, temp.DevicePointer, size, ai);
                float sum = 0;
                CudaBlasNativeMethods.cublasSasum_v2(_blas.Value, temp.DeviceVariable.Size, temp.DeviceVariable.DevicePointer, 1, ref sum);
                return FloatMath.IsZero(sum);
			}
			finally {
				temp.Release();
			}
		}

		internal IDeviceMemoryPtr PointwiseMultiply(IDeviceMemoryPtr a, IDeviceMemoryPtr b, uint size, uint ai = 1, uint bi = 1, CuStream* stream = null)
		{
			var ret = Allocate(size, stream);
			ret.CopyToDevice(b);
			Invoke(_pointwiseMultiply, stream, size, a.DevicePointer, ret.DevicePointer, size, ai, bi);
			return ret;
		}

		internal IDeviceMemoryPtr PointwiseDivide(IDeviceMemoryPtr a, IDeviceMemoryPtr b, uint size, uint ai = 1, uint bi = 1, CuStream* stream = null)
		{
			var ret = Allocate(size, stream);
			ret.CopyToDevice(b);
			Invoke(_pointwiseDivide, stream, size, a.DevicePointer, ret.DevicePointer, size, ai, bi);
			return ret;
		}

        internal void AddInPlace(IDeviceMemoryPtr a, IDeviceMemoryPtr b, uint size, float coefficient1, float coefficient2, uint ai = 1, uint bi = 1, CuStream* stream = null)
		{
			Invoke(_addInPlace, stream, size, a.DevicePointer, b.DevicePointer, size, coefficient1, coefficient2, ai, bi);
		}

		internal void SubtractInPlace(IDeviceMemoryPtr a, IDeviceMemoryPtr b, uint size, float coefficient1, float coefficient2, uint ai = 1, uint bi = 1, CuStream* stream = null)
		{
			Invoke(_subtractInPlace, stream, size, a.DevicePointer, b.DevicePointer, size, coefficient1, coefficient2, ai, bi);
		}

		internal void AddToEachRow(IDeviceMemoryPtr matrix, IDeviceMemoryPtr vector, uint rows, uint columns, CuStream* stream = null)
		{
			InvokeMatrix(_addToEachRow, stream, rows, columns, matrix.DevicePointer, vector.DevicePointer, rows, columns);
		}

		internal void AddToEachColumn(IDeviceMemoryPtr matrix, IDeviceMemoryPtr vector, uint rows, uint columns, CuStream* stream = null)
		{
			InvokeMatrix(_addToEachColumn, stream, rows, columns, matrix.DevicePointer, vector.DevicePointer, rows, columns);
		}

        internal void MultiplyByEachRow(IDeviceMemoryPtr matrix, IDeviceMemoryPtr vector, uint rows, uint columns, CuStream* stream = null)
        {
            InvokeMatrix(_multiplyByEachRow, stream, rows, columns, matrix.DevicePointer, vector.DevicePointer, rows, columns);
        }

        internal void MultiplyByEachColumn(IDeviceMemoryPtr matrix, IDeviceMemoryPtr vector, uint rows, uint columns, CuStream* stream = null)
        {
            InvokeMatrix(_multiplyByEachColumn, stream, rows, columns, matrix.DevicePointer, vector.DevicePointer, rows, columns);
        }

		internal IDeviceMemoryPtr TanH(IDeviceMemoryPtr a, uint size, uint ai = 1, uint bi = 1, CuStream* stream = null)
		{
			var ret = Allocate(size, stream);
			Invoke(_tanh, stream, size, a.DevicePointer, ret.DevicePointer, size, ai, bi);
			return ret;
		}

		internal IDeviceMemoryPtr TanHDerivative(IDeviceMemoryPtr a, uint size, uint ai = 1, uint bi = 1, CuStream* stream = null)
		{
			var ret = Allocate(size, stream);
			Invoke(_tanhDerivative, stream, size, a.DevicePointer, ret.DevicePointer, size, ai, bi);
			return ret;
		}

		internal IDeviceMemoryPtr Sigmoid(IDeviceMemoryPtr a, uint size, uint ai = 1, uint bi = 1, CuStream* stream = null)
		{
			var ret = Allocate(size, stream);
			Invoke(_sigmoid, stream, size, a.DevicePointer, ret.DevicePointer, size, ai, bi);
			return ret;
		}

		internal IDeviceMemoryPtr SigmoidDerivative(IDeviceMemoryPtr a, uint size, uint ai = 1, uint bi = 1, CuStream* stream = null)
		{
			var ret = Allocate(size, stream);
			Invoke(_sigmoidDerivative, stream, size, a.DevicePointer, ret.DevicePointer, size, ai, bi);
			return ret;
		}

		internal IDeviceMemoryPtr Relu(IDeviceMemoryPtr a, uint size, uint ai = 1, uint bi = 1, CuStream* stream = null)
		{
			var ret = Allocate(size, stream);
			Invoke(_relu, stream, size, a.DevicePointer, ret.DevicePointer, size, ai, bi);
			return ret;
		}

		internal IDeviceMemoryPtr ReluDerivative(IDeviceMemoryPtr a, uint size, uint ai = 1, uint bi = 1, CuStream* stream = null)
		{
			var ret = Allocate(size, stream);
			Invoke(_reluDerivative, stream, size, a.DevicePointer, ret.DevicePointer, size, ai, bi);
			return ret;
		}

		internal IDeviceMemoryPtr LeakyRelu(IDeviceMemoryPtr a, uint size, uint ai = 1, uint bi = 1, CuStream* stream = null)
		{
			var ret = Allocate(size, stream);
			Invoke(_leakyRelu, stream, size, a.DevicePointer, ret.DevicePointer, size, ai, bi);
			return ret;
		}

		internal IDeviceMemoryPtr LeakyReluDerivative(IDeviceMemoryPtr a, uint size, uint ai = 1, uint bi = 1, CuStream* stream = null)
		{
			var ret = Allocate(size, stream);
			Invoke(_leakyReluDerivative, stream, size, a.DevicePointer, ret.DevicePointer, size, ai, bi);
			return ret;
		}

		internal IDeviceMemoryPtr SumRows(IDeviceMemoryPtr a, uint rows, uint columns, IDeviceMemoryPtr? ret, CuStream* stream = null)
		{
			ret ??= Allocate(rows, stream, true);
			InvokeMatrix(_sumRows, stream, rows, columns, a.DevicePointer, ret.DevicePointer, rows, columns);
			return ret;
		}

		internal IDeviceMemoryPtr SumColumns(IDeviceMemoryPtr a, uint rows, uint columns, IDeviceMemoryPtr? ret, CuStream* stream = null)
		{
			ret ??= Allocate(columns, stream, true);
			InvokeMatrix(_sumColumns, stream, rows, columns, a.DevicePointer, ret.DevicePointer, rows, columns);
			return ret;
		}

		internal void MemSet(IDeviceMemoryPtr data, float value, uint count, CuStream* stream = null, uint offset = 0, uint increment = 1)
		{
			Invoke(_memSet, stream, count, data.DevicePointer, value, count, offset, increment);
		}

        internal void MemCpy(IDeviceMemoryPtr a, IDeviceMemoryPtr b, uint count, CuStream* stream = null, uint offsetA = 0, uint offsetB = 0, uint incrementA = 1, uint incrementB = 1)
        {
            Invoke(_memCpy, stream, count, a.DevicePointer, b.DevicePointer, count, offsetA, offsetB, incrementA, incrementB);
        }

		internal IDeviceMemoryPtr Sqrt(IDeviceMemoryPtr a, uint size, float valueAdjustment, uint ai = 1, uint bi = 1, CuStream* stream = null)
		{
			var ret = Allocate(size, stream);
			Invoke(_sqrt, stream, size, a.DevicePointer, ret.DevicePointer, size, valueAdjustment, ai, bi);
			return ret;
		}

		internal IDeviceMemoryPtr Abs(IDeviceMemoryPtr a, uint size, uint ai = 1, uint bi = 1, CuStream* stream = null)
		{
			var ret = Allocate(size, stream);
			Invoke(_abs, stream, size, a.DevicePointer, ret.DevicePointer, size, ai, bi);
			return ret;
		}

		internal IDeviceMemoryPtr Log(IDeviceMemoryPtr a, uint size, uint ai = 1, uint bi = 1, CuStream* stream = null)
		{
			var ret = Allocate(size, stream);
            InvokeManual(_log, stream, size, a.DevicePointer, ret.DevicePointer, size, ai, bi);
			return ret;
		}

        internal IDeviceMemoryPtr Exp(IDeviceMemoryPtr a, uint size, uint ai = 1, uint bi = 1, CuStream* stream = null)
        {
            var ret = Allocate(size, stream);
            InvokeManual(_exp, stream, size, a.DevicePointer, ret.DevicePointer, size, ai, bi);
            return ret;
        }

		internal void VectorAddInPlace(IDeviceMemoryPtr a, uint size, float scalar, uint ai = 1, CuStream* stream = null)
		{
			Invoke(_vectorAddInPlace, stream, size, a.DevicePointer, size, scalar, ai);
		}

		internal IDeviceMemoryPtr VectorCopy(IDeviceMemoryPtr a, uint[] indexList, uint ai = 1, uint bi = 1, CuStream* stream = null)
		{
			var retSize = (uint)indexList.Length;
			var ret = Allocate(retSize, stream);
            using var indexGpu = new CudaDeviceVariable<uint>(retSize);
            indexGpu.CopyToDevice(indexList);
            Invoke(_vectorCopyRandom, stream, retSize, a.DevicePointer, ret.DevicePointer, indexGpu.DevicePointer, retSize, ai, bi);
            return ret;
        }

        internal (float Min, float Max) FindMinAndMax(IDeviceMemoryPtr a, uint size, uint ai = 1, CuStream* stream = null)
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

		internal float SumValues(IDeviceMemoryPtr a, uint size, uint ai = 1, CuStream* stream = null)
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

		internal float FindStdDev(IDeviceMemoryPtr a, uint size, float mean, uint ai = 1, CuStream* stream = null)
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

		internal void Constrain(IDeviceMemoryPtr a, uint size, float min, float max, uint ai = 1, CuStream* stream = null)
		{
			Invoke(_constrain, stream, size, a.DevicePointer, size, min, max, ai);
		}
        
        internal void RoundInPlace(IDeviceMemoryPtr a, uint size, float lower, float upper, float mid, uint ai = 1, CuStream* stream = null)
        {
            Invoke(_roundInPlace, stream, size, a.DevicePointer, size, lower, upper, mid, ai);
        }

		internal IDeviceMemoryPtr Pow(IDeviceMemoryPtr a, uint size, float power, uint ai = 1, uint bi = 1, CuStream* stream = null)
		{
			var ret = Allocate(size, stream);
			Invoke(_pow, stream, size, a.DevicePointer, ret.DevicePointer, size, power, ai, bi);
			return ret;
		}

		internal IDeviceMemoryPtr Diagonal(IDeviceMemoryPtr a, uint rows, uint columns, uint ai = 1, uint bi = 1, CuStream* stream = null)
		{
			var size = System.Math.Min(rows, columns);
			var ret = Allocate(size, stream);
			Invoke(_diagonal, stream, size, a.DevicePointer, ret.DevicePointer, rows, columns, ai, bi);
			return ret;
		}

		internal void L1Regularisation(IDeviceMemoryPtr a, uint size, float coefficient, uint ai = 1, CuStream* stream = null)
		{
			Invoke(_l1Regularisation, stream, size, a.DevicePointer, size, coefficient, ai);
		}

        internal float EuclideanDistance(IDeviceMemoryPtr a, IDeviceMemoryPtr b, uint size, uint ai = 1, uint bi = 1, uint ci = 1, CuStream* stream = null)
		{
			var ret = Allocate(size, stream);
			Invoke(_euclideanDistance, stream, size, a.DevicePointer, b.DevicePointer, ret.DevicePointer, size, ai, bi, ci);
			return Convert.ToSingle(System.Math.Sqrt(SumValues(ret, size)));
		}

		internal float ManhattanDistance(IDeviceMemoryPtr a, IDeviceMemoryPtr b, uint size, uint ai = 1, uint bi = 1, uint ci = 1, CuStream* stream = null)
		{
			var ret = Allocate(size, stream);
			Invoke(_manhattanDistance, stream, size, a.DevicePointer, b.DevicePointer, ret.DevicePointer, size, ai, bi, ci);
			return SumValues(ret, size);
		}

        internal float CosineDistance(IDeviceMemoryPtr a, IDeviceMemoryPtr b, uint size, uint ai = 1, uint bi = 1, CuStream* stream = null)
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

		internal void Normalise(IDeviceMemoryPtr a, uint size, float min, float range, uint ai = 1, CuStream* stream = null)
		{
			Invoke(_normalise, stream, size, a.DevicePointer, size, min, range, ai);
		}

        internal void ScaleInPlace(IDeviceMemoryPtr a, uint size, float scaleBy, uint ai = 1, CuStream* stream = null)
        {
            Invoke(_scale, stream, size, a.DevicePointer, size, scaleBy, ai);
        }

		internal IDeviceMemoryPtr SoftmaxVector(IDeviceMemoryPtr a, uint size, float max, uint ai = 1, CuStream* stream = null)
		{
			var ret = Allocate(size, stream);
			Invoke(_softmaxVector, stream, size, a.DevicePointer, ret.DevicePointer, size, max, ai);
			return ret;
		}

		internal void PointwiseDivideRows(IDeviceMemoryPtr a, IDeviceMemoryPtr b, uint rows, uint columns, CuStream* stream = null)
		{
			InvokeMatrix(_pointwiseDivideRows, stream, rows, columns, a.DevicePointer, b.DevicePointer, rows, columns);
		}

		internal void PointwiseDivideColumns(IDeviceMemoryPtr a, IDeviceMemoryPtr b, uint rows, uint columns, CuStream* stream = null)
		{
			InvokeMatrix(_pointwiseDivideColumns, stream, rows, columns, a.DevicePointer, b.DevicePointer, rows, columns);
		}

		internal void SplitRows(IDeviceMemoryPtr a, IDeviceMemoryPtr b, IDeviceMemoryPtr c, uint rows, uint columns, uint position, CuStream* stream = null)
		{
			InvokeMatrix(_splitRows, stream, rows, columns, a.DevicePointer, b.DevicePointer, c.DevicePointer, rows, columns, position);
		}

		internal void SplitColumns(IDeviceMemoryPtr a, IDeviceMemoryPtr b, IDeviceMemoryPtr c, uint rows, uint columns, uint position, CuStream* stream = null)
		{
			InvokeMatrix(_splitColumns, stream, rows, columns, a.DevicePointer, b.DevicePointer, c.DevicePointer, rows, columns, position);
		}

		internal void ConcatRows(IDeviceMemoryPtr a, IDeviceMemoryPtr b, IDeviceMemoryPtr c, uint rows, uint columns, uint leftColumnCount, CuStream* stream = null)
		{
			InvokeMatrix(_concatRows, stream, rows, columns, a.DevicePointer, b.DevicePointer, c.DevicePointer, rows, columns, leftColumnCount);
		}

		internal void ConcatColumns(IDeviceMemoryPtr a, IDeviceMemoryPtr b, IDeviceMemoryPtr c, uint rows, uint columns, uint topRowCount, uint bottomRowCount, CuStream* stream = null)
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
            CuStream* stream = null
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
            CuStream* stream = null
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

		internal IDeviceMemoryPtr VectorSoftmaxDerivative(IDeviceMemoryPtr a, uint size, uint ai = 1, CuStream* stream = null)
		{
			var ret = Allocate(size * size, stream);
			InvokeMatrix(_softmaxDerivative, stream, size, size, a.DevicePointer, ret.DevicePointer, size, ai);
			return ret;
		}

		internal IDeviceMemoryPtr Reverse(IDeviceMemoryPtr a, uint size, CuStream* stream = null)
		{
			var ret = Allocate(size, stream);
			Invoke(_reverse, stream, size, a.DevicePointer, ret.DevicePointer, size);
			return ret;
		}

		internal void RotateInPlace(IDeviceMemoryPtr a, uint size, uint blockCount, CuStream* stream = null)
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
            CuStream* stream = null
		) {
			var outputColumns = (columns - filterWidth) / xStride + 1;
			var outputRows = (rows - filterHeight) / yStride + 1;
			var outputMatrixSize = outputColumns * outputRows;
			var ret = Allocate(outputMatrixSize * depth * count, stream, true);
			var indices = saveIndices ? Allocate(outputMatrixSize * depth * count, stream, true) : null;
			var convolutions = GetConvolutions(rows, columns, filterWidth, filterHeight, xStride, yStride);
			var size = convolutions.Count * depth * count;

            Invoke(_tensorMaxPool, stream, size,
                size,
                tensor.DevicePointer,
                ret.DevicePointer,
                indices?.DevicePointer ?? new CuDevicePtr(),
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
            CuStream* stream = null
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
            CuStream* stream = null
		) {
			var convolutions = GetConvolutions(rows, columns, filterWidth, filterHeight, xStride, yStride);
			var filterSize = filterWidth * filterHeight;
			var outputRows = convolutions.Count;
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
            CuStream* stream = null
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

        internal void MultiCosine(uint size, uint columns, uint rows, CuDevicePtr vectorPtr, CuDevicePtr compareToPtr, CuDevicePtr aa, CuDevicePtr ret, CuDevicePtr bb)
        {
            InvokeTensor(_multiCosine, null, size, columns, rows,
                vectorPtr,
                compareToPtr,
                aa,
                ret,
                bb,
                rows,
                columns,
                size
            );
        }

        internal void CalculateDistances(uint size, uint columns, uint rows, CuDevicePtr vectorPtr, CuDevicePtr compareToPtr, CuDevicePtr ret, DistanceMetric distanceMetric)
        {
            InvokeTensor(_calculateDistance, null, size, columns, rows,
                vectorPtr,
                compareToPtr,
                ret,
                rows,
                columns,
                size,
                (uint)distanceMetric
            );
        }

        internal void CopyToMatrixRows(uint rows, uint columns, CudaDeviceVariable<CuDevicePtr> from, IDeviceMemoryPtr to, CuStream* stream = null)
        {
            InvokeMatrix(_copyToMatrixRows, stream, rows, columns, from.DevicePointer, to.DevicePointer, rows, columns);
        }

        internal void CopyToMatrixColumns(uint rows, uint columns, CudaDeviceVariable<CuDevicePtr> from, IDeviceMemoryPtr to, CuStream* stream = null)
        {
            InvokeMatrix(_copyToMatrixColumns, stream, rows, columns, from.DevicePointer, to.DevicePointer, rows, columns);
        }

        internal IDeviceMemoryPtr Allocate(uint size, CuStream* stream = null, bool setToZero = false)
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

        /// <inheritdoc />
        public void BindThread()
		{
			_cuda.SetCurrent();
		}

        internal static IDeviceMemoryPtr Offset(IDeviceMemoryPtr ptr, SizeT offsetByElements, SizeT size)
		{
			var offsetPtr = ptr.DevicePointer.Pointer + (offsetByElements * FloatSize);
			return new PtrToMemory(ptr, new CuDevicePtr(offsetPtr), size * FloatSize);
		}

        internal static IDeviceMemoryPtr OffsetByBlock(IDeviceMemoryPtr ptr, SizeT offsetIndex, SizeT blockSize)
		{
			return Offset(ptr, blockSize * offsetIndex, blockSize);
		}
    }
}
