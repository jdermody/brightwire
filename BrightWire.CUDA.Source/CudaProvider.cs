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
using BrightWire.LinearAlgebra.Helper;

namespace BrightWire.LinearAlgebra
{
	/// <summary>
	/// Manages the bright wire cuda kernels and implements the cuda linear algebra provider
	/// </summary>
	class CudaProvider : ILinearAlgebraProvider, IGpuLinearAlgebraProvider
	{
		const int BLOCK_DIM = 16;
		const int BLOCK_DIM2 = BLOCK_DIM * BLOCK_DIM;
		const int PTR_SIZE = 8;
		internal const int FLOAT_SIZE = sizeof(float);

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

				var result = DriverAPINativeMethods.Launch.cuLaunchKernel(_function,
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

				CheckForError(result);
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
			_calculateDistance
		;
		bool _disposed = false;

		public CudaProvider(string cudaKernelPath, bool stochastic, int memoryCacheSize)
		{
			IsStochastic = stochastic;
			_cuda = new CudaContext();
			_cache = new DeviceMemory(_cuda, memoryCacheSize);
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
			_copyToMatrixColumns = _kernel.LoadFunction("CopyToMatrixColumns");
			_copyToMatrixRows = _kernel.LoadFunction("CopyToMatrixRows");
			_tensorAddPadding = _kernel.LoadFunction("TensorAddPadding");
			_tensorRemovePadding = _kernel.LoadFunction("TensorRemovePadding");
			_tensorIm2Col = _kernel.LoadFunction("TensorIm2Col");
			_softmaxDerivative = _kernel.LoadFunction("SoftmaxDerivative");
			_reverse = _kernel.LoadFunction("Reverse");
			_rotateInPlace = _kernel.LoadFunction("RotateInPlace");
			_tensorMaxPool = _kernel.LoadFunction("TensorMaxPool");
			_tensorReverseMaxPool = _kernel.LoadFunction("TensorReverseMaxPool");
			_tensorReverseIm2Col = _kernel.LoadFunction("TensorReverseIm2Col");
			_isFinite = _kernel.LoadFunction("IsFinite");
			_calculateDistance = _kernel.LoadFunction("CalculateDistances");
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
		public float[,] CalculateDistances(IReadOnlyList<IVector> vectors, IReadOnlyList<IVector> compareTo, DistanceMetric distanceMetric)
		{
			throw new NotImplementedException();
		}

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

		internal static void CheckForError(CUResult result)
		{
			if (result != CUResult.Success) {
				string errorName = "", errorDesc = "";
				IntPtr errorNamePtr = IntPtr.Zero, errorDescPtr = IntPtr.Zero;
				if (DriverAPINativeMethods.ErrorHandling.cuGetErrorName(result, ref errorNamePtr) == CUResult.Success && errorNamePtr != IntPtr.Zero)
					errorName = Marshal.PtrToStringUni(errorNamePtr);
				if(DriverAPINativeMethods.ErrorHandling.cuGetErrorString(result, ref errorDescPtr) == CUResult.Success && errorDescPtr != IntPtr.Zero)
					errorDesc = Marshal.PtrToStringUni(errorDescPtr);
					
				throw new Exception($"{result.ToString()}: {errorName}-{errorDesc}");
			}
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

		void _Invoke2(CUfunction function, int rows, int columns, params object[] param)
		{
			var x = _GetBlockCount(rows, BLOCK_DIM);
			var y = _GetBlockCount(columns, BLOCK_DIM);
			var execution = _kernel.CreateExecution(function, new dim3(x, y), new dim3(BLOCK_DIM, BLOCK_DIM));
			execution.Run(0, param);
		}

		void _Invoke3(CUfunction function, int rows, int columns, int depth, params object[] param)
		{
			const int size = BLOCK_DIM / 2;
			var x = _GetBlockCount(rows, size);
			var y = _GetBlockCount(columns, size);
			var z = _GetBlockCount(depth, size);
			var execution = _kernel.CreateExecution(function, new dim3(x, y, z), new dim3(size, size, size));
			execution.Run(0, param);
		}

		internal bool IsFinite(IDeviceMemoryPtr a, int size)
		{
			var ret = Allocate(size);
			_Invoke(_isFinite, size, a.DevicePointer, ret.DevicePointer, size);
			var sum = _blas.AbsoluteSum(ret.DeviceVariable, 1);
			ret.Free();
			return BoundMath.IsZero(sum);
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
						_Invoke(_findMinAndMax, size, ptr.DevicePointer, size, minBlock.DevicePointer, maxBlock.DevicePointer);
						if (ptr != a)
							ptr.Free();
						size = bufferSize * 2;
						ptr = Allocate(size);
						ptr.DeviceVariable.CopyToDevice(minBlock.DeviceVariable, 0, 0, bufferSize * FLOAT_SIZE);
						ptr.DeviceVariable.CopyToDevice(maxBlock.DeviceVariable, 0, bufferSize * FLOAT_SIZE, bufferSize * FLOAT_SIZE);
					}
					finally {
						minBlock.Free();
						maxBlock.Free();
					}
				}
				var data = new float[size];
				ptr.CopyToHost(data);
				if (ptr != a)
					ptr.Free();
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

		internal float SumValues(IDeviceMemoryPtr a, int size)
		{
			var ptr = a;
			while (size > BLOCK_DIM2) {
				var bufferSize = (size / BLOCK_DIM2) + 1;
				var sumBlock = Allocate(bufferSize, true);
				_Invoke(_findSum, size, ptr.DevicePointer, size, sumBlock.DevicePointer);
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
				while (size > BLOCK_DIM2) {
					var bufferSize = (size / BLOCK_DIM2) + 1;
					var sumBlock = Allocate(bufferSize, true);
					_Invoke(_findStdDev, size, ptr.DevicePointer, size, mean, sumBlock.DevicePointer);
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

		internal (IDeviceMemoryPtr Data, int Rows, int Columns) TensorAddPadding(IDeviceMemoryPtr tensor, int rows, int columns, int depth, int count, int padding)
		{
			var outputRows = rows + padding * 2;
			var outputColumns = columns + padding * 2;
			var ret = Allocate(outputRows * outputColumns * depth * count, true);

			_Invoke(_tensorAddPadding, ret.Size,
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

		internal (IDeviceMemoryPtr Data, int Rows, int Columns) TensorRemovePadding(IDeviceMemoryPtr tensor, int rows, int columns, int depth, int count, int padding)
		{
			var outputRows = rows - padding * 2;
			var outputColumns = columns - padding * 2;
			var ret = Allocate(outputRows * outputColumns * depth * count);
			var size = rows * columns * depth * count;

			_Invoke(_tensorRemovePadding, size,
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

		internal void RotateInPlace(IDeviceMemoryPtr a, int size, int blockCount)
		{
			var blockSize = size / blockCount;
			_Invoke(_rotateInPlace, size, a.DevicePointer, size, blockCount, blockSize);
		}

		internal (IDeviceMemoryPtr Data, IDeviceMemoryPtr Indices, int Rows, int Columns) TensorMaxPool(
			IDeviceMemoryPtr tensor, 
			int rows, 
			int columns, 
			int depth, 
			int count,
			int filterWidth,
			int filterHeight,
			int stride,
			bool saveIndices
		) {
			var outputColumns = (columns - filterWidth) / stride + 1;
			var outputRows = (rows - filterHeight) / stride + 1;
			var outputMatrixSize = outputColumns * outputRows;
			var ret = Allocate(outputMatrixSize * depth * count, true);
			var indices = saveIndices ? Allocate(outputMatrixSize * depth * count, true) : null;
			var convolutions = ConvolutionHelper.Default(columns, rows, filterWidth, filterHeight, stride);
			var size = convolutions.Count * depth * count;

			using (var convolutionData = new ConvolutionsData(this, convolutions)) {
				_Invoke(_tensorMaxPool, size,
					size,
					tensor.DevicePointer,
					ret.DevicePointer,
					saveIndices ? indices.DevicePointer : new CUdeviceptr(),
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
					stride,
					saveIndices ? 1 : 0
				);

				return (ret, indices, outputRows, outputColumns);
			}
		}

		internal IDeviceMemoryPtr TensorReverseMaxPool(IDeviceMemoryPtr tensor, IDeviceMemoryPtr indices, int rows, int columns, int depth, int count, int outputRows, int outputColumns, int filterWidth, int filterHeight, int stride)
		{
			var ret = Allocate(outputRows * outputColumns * depth * count, true);
			var size = rows * columns * depth * count;

			_Invoke(_tensorReverseMaxPool, size,
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
				stride
			);

			return ret;
		}

		internal (IDeviceMemoryPtr Data, int Rows, int Columns, int Depth) TensorIm2Col(
			IDeviceMemoryPtr tensor, 
			int rows, 
			int columns, 
			int depth, 
			int count, 
			int filterWidth, 
			int filterHeight, 
			int stride
		) {
			var convolutions = ConvolutionHelper.Default(columns, rows, filterWidth, filterHeight, stride);
			var filterSize = filterWidth * filterHeight;
			var outputRows = convolutions.Count;
			var outputColumns = filterSize * depth;
			var ret = Allocate(outputRows * outputColumns * count, true);

			using (var convolutionData = new ConvolutionsData(this, convolutions)) {
				_Invoke(_tensorIm2Col, ret.Size,
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
					stride
				);
				return (ret, outputRows, outputColumns, count);
			}
		}

		internal (IDeviceMemoryPtr Data, int Rows, int Columns, int Depth, int Count) TensorReverseIm2Col(
			IDeviceMemoryPtr tensor,
			IDeviceMemoryPtr filters,
			int rows,
			int columns,
			int depth,
			int count,
			int outputRows, 
			int outputColumns,
			int outputDepth,
			int filterWidth,
			int filterHeight,
			int stride
		) {
			var ret = Allocate(outputRows * outputColumns * outputDepth * count, true);

			using (var convolutions = new ConvolutionsData(this, ConvolutionHelper.Default(outputColumns, outputRows, filterWidth, filterHeight, stride))) {
				var size = depth * convolutions.Count * filterHeight * filterWidth * outputDepth * count;
				_Invoke(_tensorReverseIm2Col, size,
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
					stride,
					outputRows,
					outputColumns,
					outputDepth
				);
			}

			return (ret, outputRows, outputColumns, outputDepth, count);

			// add up the per filter buffers
			//var matrix = new GpuMatrix(this, outputRows * outputColumns * outputDepth * count, depth, ret, false);
			//var collapsed = matrix.RowSums();
			//ret.Free();

			//return (((IHaveDeviceMemory)collapsed).Memory, outputRows, outputColumns, outputDepth, count);
		}

		public (IDeviceMemoryPtr Data, int Rows, int Columns) CalculateDistance(
			IReadOnlyList<IVector> compareTo, 
			IDeviceMemoryPtr a, 
			int rows, 
			int columns, 
			DistanceMetric distanceMetric
		) {
			var ret = Allocate(columns * compareTo.Count);

			using (var devicePtr = new CudaDeviceVariable<CUdeviceptr>(rows)) {
				devicePtr.CopyToDevice(compareTo.Cast<IHaveDeviceMemory>().Select(d => d.Memory.DevicePointer).ToArray());
				_Invoke3(_calculateDistance, rows, columns, compareTo.Count,
					a.DevicePointer,
					devicePtr.DevicePointer,
					ret.DevicePointer,
					rows,
					columns,
					compareTo.Count,
					(int) distanceMetric
				);
			}

			return (ret, compareTo.Count, columns);
		}

		public IVector CreateVector(int length, bool setToZero = false)
		{
			var data = Allocate(length, setToZero);
			return new GpuVector(this, data, true);
		}

		public IVector CreateVector(int length, Func<int, float> init)
		{
			var data = new float[length];
			for (var i = 0; i < length; i++)
				data[i] = init(i);
			var ptr = Allocate(length);
			ptr.CopyToDevice(data);

			return new GpuVector(this, ptr, true);
		}

		public IMatrix CreateMatrix(int rows, int columns, bool setToZero = false)
		{
			var data = Allocate(rows * columns, setToZero);
			return new GpuMatrix(this, rows, columns, data, true);
		}

		public IMatrix CreateMatrixFromRows(IReadOnlyList<IVector> vectorRows)
		{
			var rows = vectorRows.Count;
			var columns = vectorRows[0].Count;

			var ret = Allocate(rows * columns);
			using (var devicePtr = new CudaDeviceVariable<CUdeviceptr>(rows)) {
				devicePtr.CopyToDevice(vectorRows.Cast<IHaveDeviceMemory>().Select(d => d.Memory.DevicePointer).ToArray());
				_Invoke2(_copyToMatrixRows, rows, columns, devicePtr.DevicePointer, ret.DevicePointer, rows, columns);
			}
			return new GpuMatrix(this, rows, columns, ret, true);
		}

		public IMatrix CreateMatrixFromColumns(IReadOnlyList<IVector> vectorColumns)
		{
			var columns = vectorColumns.Count;
			var rows = vectorColumns[0].Count;

			var ret = Allocate(rows * columns);
			using (var devicePtr = new CudaDeviceVariable<CUdeviceptr>(columns)) {
				devicePtr.CopyToDevice(vectorColumns.Cast<IHaveDeviceMemory>().Select(d => d.Memory.DevicePointer).ToArray());
				_Invoke2(_copyToMatrixColumns, rows, columns, devicePtr.DevicePointer, ret.DevicePointer, rows, columns);
			}
			return new GpuMatrix(this, rows, columns, ret, true);
		}

		public IMatrix CreateMatrix(int rows, int columns, Func<int, int, float> init)
		{
			var size = rows * columns;
			var data = new float[size];
			for (var j = 0; j < columns; j++) {
				for (var i = 0; i < rows; i++) {
					data[j * rows + i] = init(i, j);
				}
			}
			var ptr = Allocate(size);
			ptr.CopyToDevice(data);
			return new GpuMatrix(this, rows, columns, ptr, true);
		}

		public I3DTensor Create3DTensor(int rows, int columns, int depth, bool setToZero = false)
		{
			var data = Allocate(rows * columns * depth, setToZero);
			return new Gpu3DTensor(this, rows, columns, depth, data, true);
		}

		public I3DTensor Create3DTensor(IReadOnlyList<IMatrix> matrices)
		{
			var depth = matrices.Count;
			var first = matrices[0];
			var rows = first.RowCount;
			var columns = first.ColumnCount;
			var outputRows = rows * columns;
			var outputColumns = depth;

			var ret = Allocate(rows * columns * depth);
			using (var devicePtr = new CudaDeviceVariable<CUdeviceptr>(depth)) {
				devicePtr.CopyToDevice(matrices.Cast<IHaveDeviceMemory>().Select(d => d.Memory.DevicePointer).ToArray());
				_Invoke2(_copyToMatrixColumns, outputRows, outputColumns, devicePtr.DevicePointer, ret.DevicePointer, outputRows, outputColumns);
			}
			return new Gpu3DTensor(this, rows, columns, depth, ret, true);
		}

		public I4DTensor Create4DTensor(int rows, int columns, int depth, int count, bool setToZero = false)
		{
			var data = Allocate(rows * columns * depth * count, setToZero);
			return new Gpu4DTensor(this, rows, columns, depth, count, data, true);
		}

		public I4DTensor Create4DTensor(IReadOnlyList<I3DTensor> tensors)
		{
			var count = tensors.Count;
			var first = tensors[0];
			var rows = first.RowCount;
			var columns = first.ColumnCount;
			var depth = first.Depth;
			var outputRows = rows * columns * depth;
			var outputColumns = count;

			var ret = Allocate(rows * columns * depth * count);
			using (var devicePtr = new CudaDeviceVariable<CUdeviceptr>(count)) {
				devicePtr.CopyToDevice(tensors.Cast<IHaveDeviceMemory>().Select(d => d.Memory.DevicePointer).ToArray());
				_Invoke2(_copyToMatrixColumns, outputRows, outputColumns, devicePtr.DevicePointer, ret.DevicePointer, outputRows, outputColumns);
			}
			return new Gpu4DTensor(this, rows, columns, depth, count, ret, true);
		}

		public I4DTensor Create4DTensor(IReadOnlyList<FloatTensor> tensors)
		{
			var first = tensors[0];
			var data = Allocate(first.RowCount * first.ColumnCount * first.Depth * tensors.Count);
			var ret = new Gpu4DTensor(this, first.RowCount, first.ColumnCount, first.Depth, tensors.Count, data, true);

			for (var i = 0; i < tensors.Count; i++)
				ret.GetTensorAt(i).Data = tensors[i];
			return ret;
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

		public void BindThread()
		{
			_cuda.SetCurrent();
		}

		public IDeviceMemoryPtr Offset(IDeviceMemoryPtr ptr, SizeT offsetByElements, SizeT size)
		{
			var offsetPtr = ptr.DevicePointer.Pointer + (offsetByElements * FLOAT_SIZE);
			return new PtrToMemory(_cuda, ptr, new CUdeviceptr(offsetPtr), size * FLOAT_SIZE);
		}

		public IDeviceMemoryPtr OffsetByBlock(IDeviceMemoryPtr ptr, SizeT offsetIndex, SizeT blockSize)
		{
			return Offset(ptr, blockSize * offsetIndex, blockSize);
		}
	}
}
