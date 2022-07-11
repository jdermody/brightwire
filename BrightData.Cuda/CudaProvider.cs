using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using BrightData.Cuda.Helper;
using BrightData.Helper;
using BrightData.LinearAlgebra;
using ManagedCuda;
using ManagedCuda.BasicTypes;
using ManagedCuda.CudaBlas;
using ManagedCuda.CudaSolve;
using ManagedCuda.VectorTypes;
using Microsoft.Toolkit.HighPerformance.Buffers;
using Math = ManagedCuda.CudaBlas.Math;

namespace BrightData.Cuda
{
	/// <summary>
	/// Manages the bright wire cuda kernels and implements the cuda linear algebra provider
	/// </summary>
    public class CudaProvider : IGpuLinearAlgebraProvider, IHaveDataContext, IDisposable
	{
        readonly BrightDataContext _context;
        const int BlockDim = 16;
		const int BlockDim2 = BlockDim * BlockDim;
		//const int PTR_SIZE = 8;
		internal const int FloatSize = sizeof(float);

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
				for (var i = 0; i < param.Length; i++) {
					handleList[i] = GCHandle.Alloc(param[i], GCHandleType.Pinned);
					paramList[i] = handleList[i].AddrOfPinnedObject();
				}

				var result = DriverAPINativeMethods.Launch.cuLaunchKernel(_function,
					_block.x, _block.y, _block.z,
					_thread.x, _thread.y, _thread.z,
					sharedMemSize,
					new CUstream(),
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

        internal readonly CUfunction
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
			_cosineDistance,
			_abs,
			_normalise,
			_softmaxVector,
            _multiCosine,
			_log,
			_vectorAdd,
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
			_roundInPlace
		;
		readonly ConcurrentDictionary<CUfunction, (int BlockSize, int MinGridSize)> _blockSize = new();
		bool _disposed = false;

		public CudaProvider(BrightDataContext context, string? cudaKernelPath, string? cudaDirectory, uint? memoryCacheSize)
        {
            _context = context;
            _cuda = new CudaContext();

            if (cudaKernelPath == null) {
                if (String.IsNullOrWhiteSpace(cudaDirectory))
                    throw new ArgumentException("No kernel path or directory was passed");

                var info = _cuda.GetDeviceInfo();
                var kernelName = $"brightwire_{info.ComputeCapability.Major}{info.ComputeCapability.Minor}.ptx";
                cudaKernelPath = Path.Combine(cudaDirectory, kernelName);

				// try the default kernel
                if (!File.Exists(cudaKernelPath))
                    cudaKernelPath = Path.Combine(cudaDirectory, "brightwire.ptx");
                if (!File.Exists(cudaKernelPath))
                    throw new Exception($"No default kernel was found in {cudaDirectory}");
            }

			// use most available CUDA memory for the cache by default
            var cacheSize = memoryCacheSize ?? 512 * 1048576;//((ulong)_cuda.GetTotalDeviceMemorySize() * 5 / 6);
            Memory = new DeviceMemory(cacheSize);

			_kernel = new KernelModule(_cuda, cudaKernelPath);
            _blas = new(() => {
                var ret = new CudaBlas(AtomicsMode.Allowed);
                //ret.MathMode = Math.TF32TensorOpMath;
                return ret;
            });
            _cuda.SetCurrent();

            _pointwiseMultiply      = _kernel.LoadFunction("PointwiseMultiply");
			_addInPlace             = _kernel.LoadFunction("AddInPlace");
			_subtractInPlace        = _kernel.LoadFunction("SubtractInPlace");
			_addToEachRow           = _kernel.LoadFunction("AddToEachRow");
			_addToEachColumn        = _kernel.LoadFunction("AddToEachColumn");
			_tanh                   = _kernel.LoadFunction("TanH");
			_tanhDerivative         = _kernel.LoadFunction("TanHDerivative");
			_sigmoid                = _kernel.LoadFunction("Sigmoid");
			_sigmoidDerivative      = _kernel.LoadFunction("SigmoidDerivative");
			_sumRows                = _kernel.LoadFunction("SumRows");
			_relu                   = _kernel.LoadFunction("RELU");
			_reluDerivative         = _kernel.LoadFunction("RELUDerivative");
			_memClear               = _kernel.LoadFunction("MemClear");
			_sumColumns             = _kernel.LoadFunction("SumColumns");
			_pointwiseDivide        = _kernel.LoadFunction("PointwiseDivide");
			_sqrt                   = _kernel.LoadFunction("Sqrt");
			_findMinAndMax          = _kernel.LoadFunction("FindMinAndMax");
			_findSum                = _kernel.LoadFunction("FindSum");
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
			_vectorAdd              = _kernel.LoadFunction("VectorAdd");
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
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing && !_disposed) {
				if(_blas.IsValueCreated)
				    _blas.Value.Dispose();
				if(_solver.IsValueCreated)
					_solver.Value.Dispose();
				_cuda.Dispose();
				Memory.Dispose();
                _disposed = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

        public string Name { get; } = "Cuda";
        BrightDataContext IHaveDataContext.Context => _context;
        public BrightDataContext DataContext => _context;
        internal CudaContext Context => _cuda;
		public CudaBlas Blas => _blas.Value;
		public CudaSolveDense Solver => _solver.Value;
		public long TotalMemory => _cuda.GetTotalDeviceMemorySize();
		public long FreeMemory => _cuda.GetFreeDeviceMemorySize();
        public DeviceMemory Memory { get; }

        public void Register(IDisposable disposable) => Memory.Add(disposable);

		static int GetBlockCount(int size, int blockSize)
		{
			return ((size / blockSize) + 1);
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

		void Invoke(CUfunction function, uint size, params object[] param)
		{
			if (!_blockSize.TryGetValue(function, out var data)) {
				int blockSize = 0, minGridSize = 0;
				DriverAPINativeMethods.Occupancy.cuOccupancyMaxPotentialBlockSize(ref minGridSize, ref blockSize, function, bs => 0, 0, 0);
				_blockSize.TryAdd(function, data = (blockSize, minGridSize));
			}
			var gridSize = (size + data.BlockSize - 1) / data.BlockSize;
			var execution = KernelModule.CreateExecution(function, (int)gridSize, data.BlockSize);
			execution.Run(0, param);
		}

        static void InvokeManual(CUfunction function, uint size, params object[] param)
		{
			var gridSize = GetBlockCount((int)size, BlockDim2);
			var execution = KernelModule.CreateExecution(function, gridSize, BlockDim2);
			execution.Run(0, param);
		}

        internal void InvokeMatrix(CUfunction function, uint rows, uint columns, params object[] param)
		{
			if (!_blockSize.TryGetValue(function, out var data)) {
				int blockSize = 0, minGridSize = 0;
				DriverAPINativeMethods.Occupancy.cuOccupancyMaxPotentialBlockSize(ref minGridSize, ref blockSize, function, bs => 0, 0, 0);
				_blockSize.TryAdd(function, data = (Convert.ToInt32(System.Math.Pow(blockSize, 1.0/2)), minGridSize));
			}
			var gridSizeRows = (rows + data.BlockSize - 1) / data.BlockSize;
			var gridSizeCols = (columns + data.BlockSize - 1) / data.BlockSize;
			var execution = KernelModule.CreateExecution(function, new dim3((int)gridSizeRows, (int)gridSizeCols), new dim3(data.BlockSize, data.BlockSize));
			
			execution.Run(0, param);
		}

        internal void InvokeTensor(CUfunction function, uint rows, uint columns, uint depth, params object[] param)
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
			execution.Run(0, param);
		}

		internal bool IsFinite(IDeviceMemoryPtr a, uint size)
		{
			var ret = Allocate(size);
			try {
				Invoke(_isFinite, size, a.DevicePointer, ret.DevicePointer, size);
				var sum = _blas.Value.AbsoluteSum(ret.DeviceVariable, 1);
				return FloatMath.IsZero(sum);
			}
			finally {
				ret.Release();
			}
		}

		internal IDeviceMemoryPtr PointwiseMultiply(IDeviceMemoryPtr a, IDeviceMemoryPtr b, uint size)
		{
			var ret = Allocate(size);
			ret.CopyToDevice(b);
			Invoke(_pointwiseMultiply, size, a.DevicePointer, ret.DevicePointer, size);
			return ret;
		}

		internal IDeviceMemoryPtr PointwiseDivide(IDeviceMemoryPtr a, IDeviceMemoryPtr b, uint size)
		{
			var ret = Allocate(size);
			ret.CopyToDevice(b);
			Invoke(_pointwiseDivide, size, a.DevicePointer, ret.DevicePointer, size);
			return ret;
		}

        internal void AddInPlace(IDeviceMemoryPtr a, IDeviceMemoryPtr b, uint size, float coefficient1, float coefficient2)
		{
			Invoke(_addInPlace, size, a.DevicePointer, b.DevicePointer, size, coefficient1, coefficient2);
		}

		internal void SubtractInPlace(IDeviceMemoryPtr a, IDeviceMemoryPtr b, uint size, float coefficient1, float coefficient2)
		{
			Invoke(_subtractInPlace, size, a.DevicePointer, b.DevicePointer, size, coefficient1, coefficient2);
		}

		internal void AddToEachRow(IDeviceMemoryPtr matrix, IDeviceMemoryPtr vector, uint rows, uint columns)
		{
			InvokeMatrix(_addToEachRow, rows, columns, matrix.DevicePointer, vector.DevicePointer, rows, columns);
		}

		internal void AddToEachColumn(IDeviceMemoryPtr matrix, IDeviceMemoryPtr vector, uint rows, uint columns)
		{
			InvokeMatrix(_addToEachColumn, rows, columns, matrix.DevicePointer, vector.DevicePointer, rows, columns);
		}

		internal IDeviceMemoryPtr TanH(IDeviceMemoryPtr a, uint size)
		{
			var ret = Allocate(size);
			Invoke(_tanh, size, a.DevicePointer, ret.DevicePointer, size);
			return ret;
		}

		internal IDeviceMemoryPtr TanHDerivative(IDeviceMemoryPtr a, uint size)
		{
			var ret = Allocate(size);
			Invoke(_tanhDerivative, size, a.DevicePointer, ret.DevicePointer, size);
			return ret;
		}

		internal IDeviceMemoryPtr Sigmoid(IDeviceMemoryPtr a, uint size)
		{
			var ret = Allocate(size);
			Invoke(_sigmoid, size, a.DevicePointer, ret.DevicePointer, size);
			return ret;
		}

		internal IDeviceMemoryPtr SigmoidDerivative(IDeviceMemoryPtr a, uint size)
		{
			var ret = Allocate(size);
			Invoke(_sigmoidDerivative, size, a.DevicePointer, ret.DevicePointer, size);
			return ret;
		}

		internal IDeviceMemoryPtr Relu(IDeviceMemoryPtr a, uint size)
		{
			var ret = Allocate(size);
			Invoke(_relu, size, a.DevicePointer, ret.DevicePointer, size);
			return ret;
		}

		internal IDeviceMemoryPtr ReluDerivative(IDeviceMemoryPtr a, uint size)
		{
			var ret = Allocate(size);
			Invoke(_reluDerivative, size, a.DevicePointer, ret.DevicePointer, size);
			return ret;
		}

		internal IDeviceMemoryPtr LeakyRelu(IDeviceMemoryPtr a, uint size)
		{
			var ret = Allocate(size);
			Invoke(_leakyRelu, size, a.DevicePointer, ret.DevicePointer, size);
			return ret;
		}

		internal IDeviceMemoryPtr LeakyReluDerivative(IDeviceMemoryPtr a, uint size)
		{
			var ret = Allocate(size);
			Invoke(_leakyReluDerivative, size, a.DevicePointer, ret.DevicePointer, size);
			return ret;
		}

		internal IDeviceMemoryPtr SumRows(IDeviceMemoryPtr a, uint rows, uint columns)
		{
			var ret = Allocate(rows, true);
			InvokeMatrix(_sumRows, rows, columns, a.DevicePointer, ret.DevicePointer, rows, columns);
			return ret;
		}

		internal IDeviceMemoryPtr SumColumns(IDeviceMemoryPtr a, uint rows, uint columns)
		{
			var ret = Allocate(columns, true);
			InvokeMatrix(_sumColumns, rows, columns, a.DevicePointer, ret.DevicePointer, rows, columns);
			return ret;
		}

		internal void MemClear(IDeviceMemoryPtr data, uint count, uint offset = 0, uint increment = 1)
		{
			Invoke(_memClear, count, data.DevicePointer, count, offset, increment);
		}

		internal IDeviceMemoryPtr Sqrt(IDeviceMemoryPtr a, uint size, float valueAdjustment)
		{
			var ret = Allocate(size);
			Invoke(_sqrt, size, a.DevicePointer, ret.DevicePointer, size, valueAdjustment);
			return ret;
		}

		internal IDeviceMemoryPtr Abs(IDeviceMemoryPtr a, uint size)
		{
			var ret = Allocate(size);
			Invoke(_abs, size, a.DevicePointer, ret.DevicePointer, size);
			return ret;
		}

		internal IDeviceMemoryPtr Log(IDeviceMemoryPtr a, uint size)
		{
			var ret = Allocate(size);
            InvokeManual(_log, size, a.DevicePointer, ret.DevicePointer, size);
			return ret;
		}

		internal void VectorAdd(IDeviceMemoryPtr a, uint size, float scalar)
		{
			Invoke(_vectorAdd, size, a.DevicePointer, size, scalar);
		}

		internal IDeviceMemoryPtr VectorCopy(IDeviceMemoryPtr a, uint[] indexList)
		{
			var retSize = (uint)indexList.Length;
			var ret = Allocate(retSize);
            using var indexGpu = new CudaDeviceVariable<uint>(retSize);
            indexGpu.CopyToDevice(indexList);
            Invoke(_vectorCopyRandom, retSize, a.DevicePointer, ret.DevicePointer, indexGpu.DevicePointer, retSize);
            return ret;
        }

		internal (float Min, float Max) FindMinAndMax(IDeviceMemoryPtr a, uint size)
		{
			if (size > 0) {
				var ptr = a;
				while (size > BlockDim2) {
					var bufferSize = (size / BlockDim2) + 1;
					var minBlock = Allocate(bufferSize, true);
					var maxBlock = Allocate(bufferSize, true);

					try {
                        InvokeManual(_findMinAndMax, size, ptr.DevicePointer, size, minBlock.DevicePointer, maxBlock.DevicePointer);
						if (ptr != a)
							ptr.Release();
						size = bufferSize * 2;
						ptr = Allocate(size);
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

		internal float SumValues(IDeviceMemoryPtr a, uint size)
		{
			var ptr = a;
			while (size > BlockDim2) {
				var bufferSize = (size / BlockDim2) + 1;
				var sumBlock = Allocate(bufferSize, true);
                InvokeManual(_findSum, size, ptr.DevicePointer, size, sumBlock.DevicePointer);
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

		internal float FindStdDev(IDeviceMemoryPtr a, uint size, float mean)
		{
			var inputSize = size;
			if (size > 0) {
				var ptr = a;
				while (size > BlockDim2) {
					var bufferSize = (size / BlockDim2) + 1;
					var sumBlock = Allocate(bufferSize, true);
                    InvokeManual(_findStdDev, size, ptr.DevicePointer, size, mean, sumBlock.DevicePointer);
					if (ptr != a)
						ptr.Release();
					size = bufferSize;
					ptr = sumBlock;
				}
				var total = new float[size];
				ptr.CopyToHost(total);
				if (ptr != a)
					ptr.Release();

				return Convert.ToSingle(System.Math.Sqrt(total.Sum() / inputSize));
			}
			return 0f;
		}

		internal void Constrain(IDeviceMemoryPtr a, uint size, float min, float max)
		{
			Invoke(_constrain, size, a.DevicePointer, size, min, max);
		}
        
        public void RoundInPlace(IDeviceMemoryPtr a, uint size, float lower, float upper, float mid)
        {
            Invoke(_roundInPlace, size, a.DevicePointer, size, lower, upper, mid);
        }

		internal IDeviceMemoryPtr Pow(IDeviceMemoryPtr a, uint size, float power)
		{
			var ret = Allocate(size);
			Invoke(_pow, size, a.DevicePointer, ret.DevicePointer, size, power);
			return ret;
		}

		internal IDeviceMemoryPtr Diagonal(IDeviceMemoryPtr a, uint rows, uint columns)
		{
			var len = System.Math.Min(rows, columns);
			var ret = Allocate(len);
			Invoke(_diagonal, len, a.DevicePointer, ret.DevicePointer, rows, columns);
			return ret;
		}

		internal void L1Regularisation(IDeviceMemoryPtr a, uint size, float coefficient)
		{
			Invoke(_l1Regularisation, size, a.DevicePointer, size, coefficient);
		}

        internal float EuclideanDistance(IDeviceMemoryPtr a, IDeviceMemoryPtr b, uint size)
		{
			var ret = Allocate(size);
			Invoke(_euclideanDistance, size, a.DevicePointer, b.DevicePointer, ret.DevicePointer, size);
			return Convert.ToSingle(System.Math.Sqrt(SumValues(ret, size)));
		}

		internal float ManhattanDistance(IDeviceMemoryPtr a, IDeviceMemoryPtr b, uint size)
		{
			var ret = Allocate(size);
			Invoke(_manhattanDistance, size, a.DevicePointer, b.DevicePointer, ret.DevicePointer, size);
			return SumValues(ret, size);
		}

		static float GetSingleValue(IDeviceMemoryPtr ptr)
		{
			var buffer = new float[1];
			ptr.CopyToHost(buffer);
			return buffer[0];
		}

		internal float CosineDistance(IDeviceMemoryPtr a, IDeviceMemoryPtr b, uint size)
		{
			var aaDevice = Allocate(1);
			var abDevice = Allocate(1);
			var bbDevice = Allocate(1);
			try {
				Invoke(_cosineDistance, size, a.DevicePointer, b.DevicePointer, aaDevice.DevicePointer, abDevice.DevicePointer, bbDevice.DevicePointer, size);

				float aa = GetSingleValue(aaDevice);
				float ab = GetSingleValue(abDevice);
				float bb = GetSingleValue(bbDevice);

				if (aa.Equals(0f))
					return bb.Equals(0f) ? 1.0f : 0.0f;
				else if (bb.Equals(0f))
					return 0.0f;
				else
					return 1f - (ab / (float)System.Math.Sqrt(aa) / (float)System.Math.Sqrt(bb));
			}
			finally {
				aaDevice.Release();
				abDevice.Release();
				bbDevice.Release();
			}
		}

		internal void Normalise(IDeviceMemoryPtr a, uint size, float min, float range)
		{
			Invoke(_normalise, size, a.DevicePointer, size, min, range);
		}

		internal IDeviceMemoryPtr SoftmaxVector(IDeviceMemoryPtr a, uint size, float max)
		{
			var ret = Allocate(size);
			Invoke(_softmaxVector, size, a.DevicePointer, ret.DevicePointer, size, max);
			return ret;
		}

		internal void PointwiseDivideRows(IDeviceMemoryPtr a, IDeviceMemoryPtr b, uint rows, uint columns)
		{
			InvokeMatrix(_pointwiseDivideRows, rows, columns, a.DevicePointer, b.DevicePointer, rows, columns);
		}

		internal void PointwiseDivideColumns(IDeviceMemoryPtr a, IDeviceMemoryPtr b, uint rows, uint columns)
		{
			InvokeMatrix(_pointwiseDivideColumns, rows, columns, a.DevicePointer, b.DevicePointer, rows, columns);
		}

		internal void SplitRows(IDeviceMemoryPtr a, IDeviceMemoryPtr b, IDeviceMemoryPtr c, uint rows, uint columns, uint position)
		{
			InvokeMatrix(_splitRows, rows, columns, a.DevicePointer, b.DevicePointer, c.DevicePointer, rows, columns, position);
		}

		internal void SplitColumns(IDeviceMemoryPtr a, IDeviceMemoryPtr b, IDeviceMemoryPtr c, uint rows, uint columns, uint position)
		{
			InvokeMatrix(_splitColumns, rows, columns, a.DevicePointer, b.DevicePointer, c.DevicePointer, rows, columns, position);
		}

		internal void ConcatRows(IDeviceMemoryPtr a, IDeviceMemoryPtr b, IDeviceMemoryPtr c, uint rows, uint columns, uint leftColumnCount)
		{
			InvokeMatrix(_concatRows, rows, columns, a.DevicePointer, b.DevicePointer, c.DevicePointer, rows, columns, leftColumnCount);
		}

		internal void ConcatColumns(IDeviceMemoryPtr a, IDeviceMemoryPtr b, IDeviceMemoryPtr c, uint rows, uint columns, uint topRowCount, uint bottomRowCount)
		{
			InvokeMatrix(_concatColumns, rows, columns, a.DevicePointer, b.DevicePointer, c.DevicePointer, rows, columns, topRowCount, bottomRowCount);
		}

        internal (IDeviceMemoryPtr Data, uint Rows, uint Columns) TensorAddPadding(
			IDeviceMemoryPtr tensor, 
			uint rows, 
			uint columns, 
			uint depth, 
			uint count, 
			uint padding
		) {
			var outputRows = rows + padding * 2;
			var outputColumns = columns + padding * 2;
			var ret = Allocate(outputRows * outputColumns * depth * count, true);

			Invoke(_tensorAddPadding, ret.Size,
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
			uint padding
		) {
			var outputRows = rows - padding * 2;
			var outputColumns = columns - padding * 2;
			var ret = Allocate(outputRows * outputColumns * depth * count);
			var size = rows * columns * depth * count;

			Invoke(_tensorRemovePadding, size,
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

		internal IDeviceMemoryPtr VectorSoftmaxDerivative(IDeviceMemoryPtr a, uint size)
		{
			var ret = Allocate(size * size);
			InvokeMatrix(_softmaxDerivative, size, size, a.DevicePointer, ret.DevicePointer, size);
			return ret;
		}

		internal IDeviceMemoryPtr Reverse(IDeviceMemoryPtr a, uint size)
		{
			var ret = Allocate(size);
			Invoke(_reverse, size, a.DevicePointer, ret.DevicePointer, size);
			return ret;
		}

		internal void RotateInPlace(IDeviceMemoryPtr a, uint size, uint blockCount)
		{
			var blockSize = size / blockCount;
			Invoke(_rotateInPlace, size, a.DevicePointer, size, blockCount, blockSize);
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
			bool saveIndices
		) {
			var outputColumns = (columns - filterWidth) / xStride + 1;
			var outputRows = (rows - filterHeight) / yStride + 1;
			var outputMatrixSize = outputColumns * outputRows;
			var ret = Allocate(outputMatrixSize * depth * count, true);
			var indices = saveIndices ? Allocate(outputMatrixSize * depth * count, true) : null;
			var convolutions = ConvolutionHelper.Default(columns, rows, filterWidth, filterHeight, xStride, yStride);
			var size = (uint)convolutions.Count * depth * count;

            using var convolutionData = new ConvolutionsData(this, convolutions);
            Invoke(_tensorMaxPool, size,
                size,
                tensor.DevicePointer,
                ret.DevicePointer,
                indices?.DevicePointer ?? new CUdeviceptr(),
                convolutionData.X.DevicePointer,
                convolutionData.Y.DevicePointer,
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

		internal IDeviceMemoryPtr TensorReverseMaxPool(IDeviceMemoryPtr tensor, IDeviceMemoryPtr indices, uint rows, uint columns, uint depth, uint count, uint outputRows, uint outputColumns, uint filterWidth, uint filterHeight, uint xStride, uint yStride)
		{
			var ret = Allocate(outputRows * outputColumns * depth * count, true);
			var size = rows * columns * depth * count;

			Invoke(_tensorReverseMaxPool, size,
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
			uint yStride
		) {
			var convolutions = ConvolutionHelper.Default(columns, rows, filterWidth, filterHeight, xStride, yStride);
			var filterSize = filterWidth * filterHeight;
			var outputRows = (uint)convolutions.Count;
			var outputColumns = filterSize * depth;
			var ret = Allocate(outputRows * outputColumns * count, true);

            using var convolutionData = new ConvolutionsData(this, convolutions);
            Invoke(_tensorIm2Col, ret.Size,
                ret.Size,
                tensor.DevicePointer,
                ret.DevicePointer,
                convolutionData.X.DevicePointer,
                convolutionData.Y.DevicePointer,
                rows,
                columns,
                depth,
                count,
                outputRows,
                outputColumns,
                convolutionData.Count,
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
			uint yStride
		) {
			var ret = Allocate(outputRows * outputColumns * outputDepth * count, true);

            using var convolutions = new ConvolutionsData(this, ConvolutionHelper.Default(outputColumns, outputRows, filterWidth, filterHeight, xStride, yStride));
            var size = depth * convolutions.Count * filterHeight * filterWidth * outputDepth * count;
            Invoke(_tensorReverseIm2Col, size,
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

        public void CopyToMatrixRows(uint rows, uint columns, CudaDeviceVariable<CUdeviceptr> from, IDeviceMemoryPtr to)
        {
            InvokeMatrix(_copyToMatrixRows, rows, columns, from.DevicePointer, to.DevicePointer, rows, columns);
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

        public void CopyToMatrixColumns(uint rows, uint columns, CudaDeviceVariable<CUdeviceptr> from, IDeviceMemoryPtr to)
        {
            InvokeMatrix(_copyToMatrixColumns, rows, columns, from.DevicePointer, to.DevicePointer, rows, columns);
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

        internal IDeviceMemoryPtr Allocate(uint size, bool setToZero = false)
		{
			var ret = Memory.GetMemory(size);
			if (setToZero)
				ret.Clear();
			return ret;
		}

		public void BindThread()
		{
			_cuda.SetCurrent();
		}

		public IDeviceMemoryPtr Offset(IDeviceMemoryPtr ptr, SizeT offsetByElements, SizeT size)
		{
			var offsetPtr = ptr.DevicePointer.Pointer + (offsetByElements * FloatSize);
			return new PtrToMemory(_cuda, ptr, new CUdeviceptr(offsetPtr), size * FloatSize);
		}

		public IDeviceMemoryPtr OffsetByBlock(IDeviceMemoryPtr ptr, SizeT offsetIndex, SizeT blockSize)
		{
			return Offset(ptr, blockSize * offsetIndex, blockSize);
		}
    }
}
