using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using BrightData.Cuda.CudaToolkit;
using BrightData.Cuda.CudaToolkit.Types;
using BrightData.Cuda.Helper;
using BrightData.Helper;
using BrightData.LinearAlgebra;
using BrightData.LinearAlgebra.Segments;
using CommunityToolkit.HighPerformance.Buffers;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BrightData.Cuda
{
    /// <summary>
    /// CUDA linear algebra provider
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="context">Bright data context</param>
    /// <param name="cuda">CUDA provider</param>
    public unsafe class CudaLinearAlgebraProvider(BrightDataContext context, CudaProvider? cuda = null) : LinearAlgebraProvider<float>(context)
    {
        /// <inheritdoc />
        public override void Dispose()
        {
            GC.SuppressFinalize(this);
            base.Dispose();
#if DEBUG
            var trackedBlocks = new HashSet<IDeviceMemoryPtr>();
            foreach (var block in Scope) {
                foreach (var item in block.Keys)
                    trackedBlocks.Add(((CudaTensorSegment)((ITensor<float>)item).Segment).DeviceMemory);
            }
            DeviceMemoryBlockBase.FindLeakedBlocks(trackedBlocks);
#endif
            Provider.Dispose();
        }

        /// <summary>
        /// CUDA provider name
        /// </summary>
        public static string Name => "cuda";

        /// <inheritdoc />
        public override string ProviderName => Name;

        /// <inheritdoc />
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)]
        public override Type VectorType { get; } = typeof(CudaVector);

        /// <inheritdoc />
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)]
        public override Type MatrixType { get; } = typeof(CudaMatrix);

        /// <inheritdoc />
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)]
        public override Type Tensor3DType { get; } = typeof(CudaTensor3D);

        /// <inheritdoc />
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)]
        public override Type Tensor4DType { get; } = typeof(CudaTensor4D);

        /// <summary>
        /// CUDA provider
        /// </summary>
        public CudaProvider Provider { get; } = cuda ?? context.CreateCudaProvider();

        /// <inheritdoc />
        public override INumericSegment<float> CreateSegment(params float[] data)
        {
            var deviceMemory = Provider.Allocate((uint)data.Length);
            deviceMemory.CopyToDevice(data);
            return new CudaTensorSegment(deviceMemory, Provider);
        }

        /// <inheritdoc />
        public override INumericSegment<float> CreateSegment(IReadOnlyNumericSegment<float> segment)
        {
            var deviceMemory = Provider.Allocate(segment.Size);
            var temp = SpanOwner<float>.Empty;
            var wasTempUsed = false;
            try {
                var span = segment.GetSpan(ref temp, out wasTempUsed);
                deviceMemory.CopyToDevice(span, 0);
            }
            finally {
                if (wasTempUsed)
                    temp.Dispose();
            }
            return new CudaTensorSegment(deviceMemory, Provider);
        }

        internal CudaTensorSegment CreateCudaTensorSegment(IDeviceMemoryPtr ptr) => new(ptr, Provider);

        /// <inheritdoc />
        public override INumericSegment<float> CreateSegment(uint size, bool initialiseToZero) => CreateCudaTensorSegment(Provider.Allocate(size, null, initialiseToZero));

        /// <inheritdoc />
        public override INumericSegment<float> CreateSegment(uint size, Func<uint, float> initializer)
        {
            using var buffer = SpanOwner<float>.Allocate((int)size);
            var span = buffer.Span;
            fixed (float* ptr = span) {
                var p = ptr;
                for (uint i = 0; i < size; i++)
                    *p++ = initializer(i);
                var deviceMemory = Provider.Allocate(size);
                deviceMemory.CopyToDevice(ptr, 0, 0, size);
                return CreateCudaTensorSegment(deviceMemory);
            }
        }
                                                                                                                                                                
        /// <inheritdoc />
        public override IVector<float> CreateVector(INumericSegment<float> data) => new CudaVector(OptionallyCopyToDevice(data), this);                                                                                                                 

        /// <inheritdoc />
        public override IMatrix<float> CreateMatrix(uint rowCount, uint columnCount, INumericSegment<float> data) => new CudaMatrix(OptionallyCopyToDevice(data), rowCount, columnCount, this);

        /// <inheritdoc />
        public override ITensor3D<float> CreateTensor3D(uint depth, uint rowCount, uint columnCount, INumericSegment<float> data) => new CudaTensor3D(data, depth, rowCount, columnCount, this);

        /// <inheritdoc />
        public override ITensor4D<float> CreateTensor4D(uint count, uint depth, uint rowCount, uint columnCount, INumericSegment<float> data) => new CudaTensor4D(data, count, depth, rowCount, columnCount, this);

        /// <inheritdoc />
        public override IMatrix<float> CreateMatrix(uint rowCount, uint columnCount, Func<uint, uint, float> initializer)
        {
            var size = rowCount * columnCount;
            using var buffer = SpanOwner<float>.Allocate((int)size);
            fixed (float* ptr = buffer.Span) {
                var p = ptr;
                for (uint j = 0; j < columnCount; j++) {
                    for (uint i = 0; i < rowCount; i++)
                        *p++ = initializer(i, j);
                }

                var deviceMemory = Provider.Allocate(size);
                deviceMemory.CopyToDevice(ptr, 0, 0, size);
                return CreateMatrix(rowCount, columnCount, CreateCudaTensorSegment(deviceMemory));
            }
        }

        /// <inheritdoc />
        public override INumericSegment<float> Clone(IReadOnlyNumericSegment<float> segment)
        {
            if (CudaTensorSegment.IsCuda(segment, out var cudaSegment)) {
                var ret = (CudaTensorSegment)CreateSegment(segment.Size, false);
                ret.DeviceMemory.CopyToDevice(cudaSegment.DeviceMemory);
                return ret;
            }
            return CopyToDevice(segment);
        }

        CudaTensorSegment OptionallyCopyToDevice(INumericSegment<float> segment)
        {
            if (CudaTensorSegment.IsCuda(segment, out var ret))
                return ret;
            while (segment is MutableTensorSegmentWrapper<float> { Stride: 1 } wrapper) {
                segment = wrapper.UnderlyingSegment;
                if (segment is CudaTensorSegment cudaTensor) {
                    var offsetBlock = CudaProvider.Offset(cudaTensor.DeviceMemory, wrapper.Offset, wrapper.Size);
                    return CreateCudaTensorSegment(offsetBlock);
                }
            }
            return CopyToDevice(segment);
        }

        CudaTensorSegment CopyToDevice(IReadOnlyNumericSegment<float> segment)
        {
            var deviceMemory = Provider.Allocate(segment.Size);
            var temp = SpanOwner<float>.Empty;
            var span = segment.GetSpan(ref temp, out var wasTempUsed);
            try {
                deviceMemory.CopyToDevice(span, 0);
                return CreateCudaTensorSegment(deviceMemory);
            }
            finally {
                if (wasTempUsed)
                    temp.Dispose();
            }
        }

        /// <inheritdoc />
        public override float DotProduct(IReadOnlyNumericSegment<float> tensor, IReadOnlyNumericSegment<float> tensor2)
        {
            float ret = 0;
            CudaBlasNativeMethods.cublasSdot_v2_64(Provider.Blas, tensor.Size, tensor.GetDevicePointer(), 1, tensor2.GetDevicePointer(), 1, ref ret);
            return ret;
        }

        /// <inheritdoc />
        public override INumericSegment<float> Add(IReadOnlyNumericSegment<float> tensor, float scalar)
        {
            var ret = (CudaTensorSegment)CreateSegment(tensor.Size, false);
            Provider.MemSet(ret.DeviceMemory, scalar, ret.Size);
            Provider.AddInPlace(ret.DeviceMemory, tensor.GetDeviceMemoryPtr(), tensor.Size, 1f, 1f);
            return ret;
        }

        /// <inheritdoc />
        public override INumericSegment<float> Add(IReadOnlyNumericSegment<float> tensor, IReadOnlyNumericSegment<float> tensor2)
        {
            var size = GetSize(tensor, tensor2);
            var ret = (CudaTensorSegment)CreateSegment(size, false);
            ret.DeviceMemory.CopyToDevice(tensor2.GetDeviceMemoryPtr());

            float alpha = 1;
            CudaBlasNativeMethods.cublasSaxpy_v2_64(Provider.Blas, size, ref alpha, tensor.GetDevicePointer(), 1, ret.DeviceMemory.DevicePointer, 1);
            return ret;
        }

        /// <inheritdoc />
        public override void AddInPlace(INumericSegment<float> target, IReadOnlyNumericSegment<float> other)
        {
            var size = GetSize(target, other);
            Provider.AddInPlace(target.GetDeviceMemoryPtr(), other.GetDeviceMemoryPtr(), size, 1f, 1f);
        }

        /// <inheritdoc />
        public override void AddInPlace(INumericSegment<float> target, IReadOnlyNumericSegment<float> other, float coefficient1, float coefficient2)
        {
            var size = GetSize(target, other);
            Provider.AddInPlace(target.GetDeviceMemoryPtr(), other.GetDeviceMemoryPtr(), size, coefficient1, coefficient2);
        }

        /// <inheritdoc />
        public override INumericSegment<float> Add(IReadOnlyNumericSegment<float> tensor, IReadOnlyNumericSegment<float> tensor2, float coefficient1, float coefficient2)
        {
            var size = GetSize(tensor, tensor2);
            var ret = (CudaTensorSegment)CreateSegment(size, false);
            ret.DeviceMemory.CopyToDevice(tensor.GetDeviceMemoryPtr());
            Provider.AddInPlace(ret.DeviceMemory, tensor2.GetDeviceMemoryPtr(), size, coefficient1, coefficient2);
            return ret;
        }

        /// <inheritdoc />
        public override void AddInPlace(INumericSegment<float> target, float scalar)
        {
            Provider.VectorAddInPlace(target.GetDeviceMemoryPtr(), target.Size, scalar);
        }

        /// <inheritdoc />
        public override void ConstrainInPlace(INumericSegment<float> segment, float? minValue, float? maxValue)
        {
            var ptr = segment.GetDeviceMemoryPtr();
            Provider.Constrain(ptr, segment.Size, minValue ?? float.MinValue, maxValue ?? float.MaxValue);
        }

        /// <inheritdoc />
        public override float CosineDistance(IReadOnlyNumericSegment<float> tensor, IReadOnlyNumericSegment<float> other)
        {
            var size = GetSize(tensor, other);
            return Provider.CosineDistance(tensor.GetDeviceMemoryPtr(), other.GetDeviceMemoryPtr(), size);
        }

        /// <inheritdoc />
        public override INumericSegment<float> LeakyReluDerivative(IReadOnlyNumericSegment<float> tensor)
        {
            var ret = Provider.LeakyReluDerivative(tensor.GetDeviceMemoryPtr(), tensor.Size);
            return CreateCudaTensorSegment(ret);
        }

        /// <inheritdoc />
        public override INumericSegment<float> PointwiseMultiply(IReadOnlyNumericSegment<float> tensor1, IReadOnlyNumericSegment<float> tensor2)
        {
            var size = GetSize(tensor1, tensor2);
            var ret = CreateCudaTensorSegment(Provider.PointwiseMultiply(tensor1.GetDeviceMemoryPtr(), tensor2.GetDeviceMemoryPtr(), size));
            return ret;
        }

        /// <inheritdoc />
        public override void SubtractInPlace(INumericSegment<float> target, IReadOnlyNumericSegment<float> other)
        {
            var size = GetSize(target, other);
            Provider.SubtractInPlace(target.GetDeviceMemoryPtr(), other.GetDeviceMemoryPtr(), size, 1f, 1f);
        }

        /// <inheritdoc />
        public override void SubtractInPlace(INumericSegment<float> target, IReadOnlyNumericSegment<float> other, float coefficient1, float coefficient2)
        {
            var size = GetSize(target, other);
            Provider.SubtractInPlace(target.GetDeviceMemoryPtr(), other.GetDeviceMemoryPtr(), size, coefficient1, coefficient2);
        }

        /// <inheritdoc />
        public override INumericSegment<float> Multiply(IReadOnlyNumericSegment<float> target, float scalar)
        {
            var ret = (CudaTensorSegment)CreateSegment(target.Size, false);
            ret.DeviceMemory.CopyToDevice(target.GetDeviceMemoryPtr());
            Provider.ScaleInPlace(ret.DeviceMemory, ret.Size, scalar);
            return ret;
        }

        /// <inheritdoc />
        public override float L2Norm(IReadOnlyNumericSegment<float> segment) // => Provider.Blas.Norm2(GetDeviceVariable(segment), 1);
        {
            var result = 0f;
            CudaBlasNativeMethods.cublasSnrm2_v2(Provider.Blas, (int)segment.Size, segment.GetDevicePointer(), 1, ref result);
            return result;
        }

        /// <inheritdoc />
        public override INumericSegment<float> Subtract(IReadOnlyNumericSegment<float> tensor1, IReadOnlyNumericSegment<float> tensor2)
        {
            var size = GetSize(tensor1, tensor2);
            var ret = (CudaTensorSegment)CreateSegment(size, false);
            ret.DeviceMemory.CopyToDevice(tensor1.GetDeviceMemoryPtr());
            var alpha = -1f;
            CudaBlasNativeMethods.cublasSaxpy_v2(Provider.Blas, (int)size, ref alpha, tensor2.GetDevicePointer(), 1, ret.DeviceMemory.DevicePointer, 1);
            return ret;
        }

        /// <inheritdoc />
        public override INumericSegment<float> PointwiseDivide(IReadOnlyNumericSegment<float> tensor1, IReadOnlyNumericSegment<float> tensor2)
        {
            var size = GetSize(tensor1, tensor2);
            var ret = Provider.PointwiseDivide(tensor1.GetDeviceMemoryPtr(), tensor2.GetDeviceMemoryPtr(), size);
            return CreateCudaTensorSegment(ret);
        }

        /// <inheritdoc />
        public override INumericSegment<float> Subtract(IReadOnlyNumericSegment<float> tensor1, IReadOnlyNumericSegment<float> tensor2, float coefficient1, float coefficient2)
        {
            var size = GetSize(tensor1, tensor2);
            var ret = (CudaTensorSegment)CreateSegment(size, false);
            ret.DeviceMemory.CopyToDevice(tensor1.GetDeviceMemoryPtr());
            Provider.SubtractInPlace(ret.DeviceMemory, tensor2.GetDeviceMemoryPtr(), size, coefficient1, coefficient2);
            return ret;
        }

        /// <inheritdoc />
        public override float L1Norm(IReadOnlyNumericSegment<float> segment)
        {
            var abs = Abs(segment);
            try {
                return Provider.SumValues(abs.GetDeviceMemoryPtr(), segment.Size);
            }
            finally {
                abs.Release();
            }
        }

        /// <inheritdoc />
        public override bool IsEntirelyFinite(IReadOnlyNumericSegment<float> segment) => Provider.IsFinite(segment.GetDeviceMemoryPtr(), segment.Size);

        /// <inheritdoc />
        public override INumericSegment<float> Reverse(IReadOnlyNumericSegment<float> segment)
        {
            var ret = Provider.Reverse(segment.GetDeviceMemoryPtr(), segment.Size);
            return CreateCudaTensorSegment(ret);
        }

        /// <inheritdoc />
        public override float MeanSquaredDistance(IReadOnlyNumericSegment<float> tensor, IReadOnlyNumericSegment<float> other)
        {
            var size = GetSize(tensor, other);
            var temp = Subtract(tensor, other);
            try {
                var norm = L2Norm(temp);
                return norm * norm / size;
            }
            finally {
                temp.Release();
            }
        }

        /// <inheritdoc />
        public override float SquaredEuclideanDistance(IReadOnlyNumericSegment<float> tensor, IReadOnlyNumericSegment<float> other)
        {
            var temp = Subtract(tensor, other);
            try {
                var norm = L2Norm(temp);
                return norm * norm;
            }
            finally {
                temp.Release();
            }
        }

        /// <inheritdoc />
        public override float EuclideanDistance(IReadOnlyNumericSegment<float> tensor, IReadOnlyNumericSegment<float> other)
        {
            var size = GetSize(tensor, other);
            return Provider.EuclideanDistance(tensor.GetDeviceMemoryPtr(), other.GetDeviceMemoryPtr(), size);
        }

        /// <inheritdoc />
        public override float ManhattanDistance(IReadOnlyNumericSegment<float> tensor, IReadOnlyNumericSegment<float> other)
        {
            var size = GetSize(tensor, other);
            return Provider.ManhattanDistance(tensor.GetDeviceMemoryPtr(), other.GetDeviceMemoryPtr(), size);
        }

        /// <inheritdoc />
        public override float Average(IReadOnlyNumericSegment<float> segment) => Provider.SumValues(segment.GetDeviceMemoryPtr(), segment.Size) / segment.Size;

        /// <inheritdoc />
        public override INumericSegment<float> Abs(IReadOnlyNumericSegment<float> tensor) => CreateCudaTensorSegment(Provider.Abs(tensor.GetDeviceMemoryPtr(), tensor.Size));

        /// <inheritdoc />
        public override INumericSegment<float> Log(IReadOnlyNumericSegment<float> tensor) => CreateCudaTensorSegment(Provider.Log(tensor.GetDeviceMemoryPtr(), tensor.Size));

        /// <inheritdoc />
        public override INumericSegment<float> Squared(IReadOnlyNumericSegment<float> tensor) => PointwiseMultiply(tensor, tensor);

        /// <inheritdoc />
        public override INumericSegment<float> Sigmoid(IReadOnlyNumericSegment<float> tensor) => CreateCudaTensorSegment(Provider.Sigmoid(tensor.GetDeviceMemoryPtr(), tensor.Size));

        /// <inheritdoc />
        public override INumericSegment<float> SigmoidDerivative(IReadOnlyNumericSegment<float> tensor) => CreateCudaTensorSegment(Provider.SigmoidDerivative(tensor.GetDeviceMemoryPtr(), tensor.Size));

        /// <inheritdoc />
        public override INumericSegment<float> Tanh(IReadOnlyNumericSegment<float> tensor) => CreateCudaTensorSegment(Provider.TanH(tensor.GetDeviceMemoryPtr(), tensor.Size));

        /// <inheritdoc />
        public override INumericSegment<float> TanhDerivative(IReadOnlyNumericSegment<float> tensor) => CreateCudaTensorSegment(Provider.TanHDerivative(tensor.GetDeviceMemoryPtr(), tensor.Size));

        /// <inheritdoc />
        public override INumericSegment<float> Exp(IReadOnlyNumericSegment<float> tensor) => CreateCudaTensorSegment(Provider.Exp(tensor.GetDeviceMemoryPtr(), tensor.Size));

        /// <inheritdoc />
        public override INumericSegment<float> Relu(IReadOnlyNumericSegment<float> tensor) => CreateCudaTensorSegment(Provider.Relu(tensor.GetDeviceMemoryPtr(), tensor.Size));

        /// <inheritdoc />
        public override INumericSegment<float> ReluDerivative(IReadOnlyNumericSegment<float> tensor) => CreateCudaTensorSegment(Provider.ReluDerivative(tensor.GetDeviceMemoryPtr(), tensor.Size));

        /// <inheritdoc />
        public override INumericSegment<float> LeakyRelu(IReadOnlyNumericSegment<float> tensor) => CreateCudaTensorSegment(Provider.LeakyRelu(tensor.GetDeviceMemoryPtr(), tensor.Size));

        /// <inheritdoc />
        public override INumericSegment<float> Softmax(IReadOnlyNumericSegment<float> tensor) => Softmax(tensor.GetDeviceMemoryPtr(), 1);
        CudaTensorSegment Softmax(IDeviceMemoryPtr ptr, uint stride, CuStream* stream = null)
        {
            var max = Provider.FindMinAndMax(ptr, ptr.Size, stride, stream).Max;
            var softmax = Provider.SoftmaxVector(ptr, ptr.Size, max, stride, stream);
            var softmaxSum = Provider.SumValues(softmax, ptr.Size, 1, stream);
            if (Math<float>.IsNotZero(softmaxSum))
                Provider.ScaleInPlace(softmax, softmax.Size, 1f / softmaxSum, 1, stream);
            return CreateCudaTensorSegment(softmax);
        }

        /// <inheritdoc />
        public override IMatrix<float> SoftmaxDerivative(IReadOnlyNumericSegment<float> tensor) => SoftmaxDerivative(tensor.GetDeviceMemoryPtr(), 1);
        IMatrix<float> SoftmaxDerivative(IDeviceMemoryPtr ptr, uint stride)
        {
            var ret = Provider.VectorSoftmaxDerivative(ptr, ptr.Size, stride);
            return CreateMatrix(ptr.Size, ptr.Size, CreateCudaTensorSegment(ret));
        }

        /// <inheritdoc />
        public override INumericSegment<float> Pow(IReadOnlyNumericSegment<float> tensor, float power) => CreateCudaTensorSegment(Provider.Pow(tensor.GetDeviceMemoryPtr(), tensor.Size, power));

        /// <inheritdoc />
        public override INumericSegment<float> Sqrt(IReadOnlyNumericSegment<float> tensor, float? adjustment = null) => CreateCudaTensorSegment(Provider.Sqrt(tensor.GetDeviceMemoryPtr(), tensor.Size, adjustment ?? Math<float>.AlmostZero));

        /// <inheritdoc />
        public override void PointwiseDivideInPlace(INumericSegment<float> target, IReadOnlyNumericSegment<float> other)
        {
            var size = GetSize(target, other);
            var ptr = target.GetDeviceMemoryPtr();
            var temp = Provider.PointwiseDivide(ptr, other.GetDeviceMemoryPtr(), size);
            ptr.CopyToDevice(temp);
            temp.Release();
        }

        /// <inheritdoc />
        public override void RoundInPlace(INumericSegment<float> tensor, float lower, float upper)
        {
            Provider.RoundInPlace(tensor.GetDeviceMemoryPtr(), tensor.Size, lower, upper, lower + (upper - lower) / 2);
        }

        /// <inheritdoc />
        public override void PointwiseMultiplyInPlace(INumericSegment<float> target, IReadOnlyNumericSegment<float> other)
        {
            var size = GetSize(target, other);
            var ptr = target.GetDeviceMemoryPtr();
            var temp = Provider.PointwiseMultiply(ptr, other.GetDeviceMemoryPtr(), size);
            ptr.CopyToDevice(temp);
            temp.Release();
        }

        /// <inheritdoc />
        public override void MultiplyInPlace(INumericSegment<float> target, float scalar)
        {
            Provider.ScaleInPlace(target.GetDeviceMemoryPtr(), target.Size, scalar);
        }

        /// <inheritdoc />
        public override IEnumerable<IReadOnlyNumericSegment<float>> Split(IReadOnlyNumericSegment<float> segment, uint blockCount)
        {
            var blockSize = segment.Size / blockCount;
            var ptr = segment.GetDeviceMemoryPtr();

            return blockCount.AsRange().Select(i => {
                var ptr2 = CudaProvider.OffsetByBlock(ptr, i, blockSize);
                return CreateCudaTensorSegment(ptr2);
            });
        }

        /// <inheritdoc />
        public override float StdDev(IReadOnlyNumericSegment<float> tensor, float? mean)
        {
            return Provider.FindStdDev(tensor.GetDeviceMemoryPtr(), tensor.Size, mean ?? Average(tensor));
        }

        /// <inheritdoc />
        public override INumericSegment<float> CherryPickIndices(IReadOnlyNumericSegment<float> tensor, params uint[] indices)
        {
            var data = Provider.VectorCopy(tensor.GetDeviceMemoryPtr(), indices);
            return CreateCudaTensorSegment(data);
        }

        static CuDevicePtr[] GetDevicePointers(List<CudaTensorSegment> segments)
        {
            var len = segments.Count;
            var ret = new CuDevicePtr[len];
            for (var i = 0; i < len; i++) {
                var segment = segments[i];
                ret[i] = segment.DeviceMemory.DevicePointer;
            }
            return ret;
        }

        /// <inheritdoc />
        public override IMatrix<float> CreateMatrixFromColumns(IReadOnlyNumericSegment<float>[] vectorColumns)
        {
            var allAreCuda = true;
            var cudaSegmentList = new List<CudaTensorSegment>();
            foreach (var item in vectorColumns) {
                if (CudaTensorSegment.IsCuda(item, out var cudaSegment))
                    cudaSegmentList.Add(cudaSegment);
                else {
                    allAreCuda = false;
                    break;
                }
            }

            var columns = (uint)vectorColumns.Length;
            var rows = vectorColumns[0].Size;

            if (allAreCuda) {
                var devicePointers = GetDevicePointers(cudaSegmentList);
                var ret = (CudaTensorSegment)CreateSegment(rows * columns, false);
                using var devicePtr = new CudaDeviceVariable<CuDevicePtr>(columns);
                devicePtr.CopyToDevice(devicePointers);
                Provider.CopyToMatrixColumns(rows, columns, devicePtr, ret.DeviceMemory);
                return CreateMatrix(rows, columns, ret);
            }

            var size = rows * columns;
            using var buffer = SpanOwner<float>.Allocate((int)size);
            fixed (float* ptr = buffer.Span) {
                var p = ptr;
                for (uint j = 0; j < columns; j++)
                for (uint i = 0; i < rows; i++)
                    *p++ = vectorColumns[(int)j][i];
                var deviceMemory = Provider.Allocate(size);
                deviceMemory.CopyToDevice(ptr, 0, 0, size);
                return CreateMatrix(rows, columns, CreateCudaTensorSegment(deviceMemory));
            }
        }

        /// <inheritdoc />
        public override IMatrix<float> CreateMatrixFromColumns(ReadOnlySpan<IReadOnlyNumericSegment<float>> vectorColumns)
        {
            var allAreCuda = true;
            var cudaSegmentList = new List<CudaTensorSegment>();
            foreach (var item in vectorColumns) {
                if (CudaTensorSegment.IsCuda(item, out var cudaSegment))
                    cudaSegmentList.Add(cudaSegment);
                else {
                    allAreCuda = false;
                    break;
                }
            }

            var columns = (uint)vectorColumns.Length;
            var rows = vectorColumns[0].Size;

            if (allAreCuda) {
                var devicePointers = GetDevicePointers(cudaSegmentList);
                var ret = (CudaTensorSegment)CreateSegment(rows * columns, false);
                using var devicePtr = new CudaDeviceVariable<CuDevicePtr>(columns);
                devicePtr.CopyToDevice(devicePointers);
                Provider.CopyToMatrixColumns(rows, columns, devicePtr, ret.DeviceMemory);
                return CreateMatrix(rows, columns, ret);
            }

            var size = rows * columns;
            using var buffer = SpanOwner<float>.Allocate((int)size);
            fixed (float* ptr = buffer.Span) {
                var p = ptr;
                for (uint j = 0; j < columns; j++)
                for (uint i = 0; i < rows; i++)
                    *p++ = vectorColumns[(int)j][i];
                var deviceMemory = Provider.Allocate(size);
                deviceMemory.CopyToDevice(ptr, 0, 0, size);
                return CreateMatrix(rows, columns, CreateCudaTensorSegment(deviceMemory));
            }
        }

        /// <inheritdoc />
        public override IMatrix<float> CreateMatrixFromColumns(ReadOnlySpan<float[]> columnSpan)
        {
            var columns = (uint)columnSpan.Length;
            var rows = (uint)columnSpan[0].Length;
            var size = rows * columns;
            using var buffer = SpanOwner<float>.Allocate((int)size);
            fixed (float* ptr = buffer.Span) {
                var p = ptr;
                for (uint j = 0; j < columns; j++)
                for (uint i = 0; i < rows; i++)
                    *p++ = columnSpan[(int)j][i];
                var deviceMemory = Provider.Allocate(size);
                deviceMemory.CopyToDevice(ptr, 0, 0, size);
                return CreateMatrix(rows, columns, CreateCudaTensorSegment(deviceMemory));
            }
        }

        /// <inheritdoc />
        public override IMatrix<float> CreateMatrixFromRows(IReadOnlyNumericSegment<float>[] vectorRows)
        {
            var allAreCuda = true;
            var cudaSegmentList = new List<CudaTensorSegment>();
            foreach (var item in vectorRows) {
                if (CudaTensorSegment.IsCuda(item, out var cudaSegment))
                    cudaSegmentList.Add(cudaSegment);
                else {
                    allAreCuda = false;
                    break;
                }
            }

            var rows = (uint)vectorRows.Length;
            var columns = vectorRows[0].Size;

            if (allAreCuda) {
                var devicePointers = GetDevicePointers(cudaSegmentList);
                var ret = (CudaTensorSegment)CreateSegment(rows * columns, false);
                using var devicePtr = new CudaDeviceVariable<CuDevicePtr>(rows);
                devicePtr.CopyToDevice(devicePointers);
                Provider.CopyToMatrixRows(rows, columns, devicePtr, ret.DeviceMemory);
                return CreateMatrix(rows, columns, ret);
            }

            // allocate a single block
            var size = rows * columns;
            using var buffer = SpanOwner<float>.Allocate((int)(size));
            fixed (float* ptr = buffer.Span) {
                var p = ptr;
                for (uint j = 0; j < columns; j++)
                for (uint i = 0; i < rows; i++)
                    *p++ = vectorRows[(int)i][j];
                var deviceMemory = Provider.Allocate(size);
                deviceMemory.CopyToDevice(ptr, 0, 0, size);
                return CreateMatrix(rows, columns, CreateCudaTensorSegment(deviceMemory));
            }
        }

        /// <inheritdoc />
        public override IMatrix<float> CreateMatrixFromRows(ReadOnlySpan<IReadOnlyNumericSegment<float>> vectorRows)
        {
            var allAreCuda = true;
            var cudaSegmentList = new List<CudaTensorSegment>();
            foreach (var item in vectorRows) {
                if (CudaTensorSegment.IsCuda(item, out var cudaSegment))
                    cudaSegmentList.Add(cudaSegment);
                else {
                    allAreCuda = false;
                    break;
                }
            }

            var rows = (uint)vectorRows.Length;
            var columns = vectorRows[0].Size;

            if (allAreCuda) {
                var devicePointers = GetDevicePointers(cudaSegmentList);
                var ret = (CudaTensorSegment)CreateSegment(rows * columns, false);
                using var devicePtr = new CudaDeviceVariable<CuDevicePtr>(rows);
                devicePtr.CopyToDevice(devicePointers);
                Provider.CopyToMatrixRows(rows, columns, devicePtr, ret.DeviceMemory);
                return CreateMatrix(rows, columns, ret);
            }

            // allocate a single block
            var size = rows * columns;
            using var buffer = SpanOwner<float>.Allocate((int)(size));
            fixed (float* ptr = buffer.Span) {
                var p = ptr;
                for (uint j = 0; j < columns; j++)
                    for (uint i = 0; i < rows; i++)
                        *p++ = vectorRows[(int)i][j];
                var deviceMemory = Provider.Allocate(size);
                deviceMemory.CopyToDevice(ptr, 0, 0, size);
                return CreateMatrix(rows, columns, CreateCudaTensorSegment(deviceMemory));
            }
        }

        /// <inheritdoc />
        public override IMatrix<float> CreateMatrixFromRows(ReadOnlySpan<float[]> rowSpan)
        {
            var rows = (uint)rowSpan.Length;
            var columns = (uint)rowSpan[0].Length;
            var size = rows * columns;
            using var buffer = SpanOwner<float>.Allocate((int)(size));
            fixed (float* ptr = buffer.Span) {
                var p = ptr;
                for (uint j = 0; j < columns; j++)
                for (uint i = 0; i < rows; i++)
                    *p++ = rowSpan[(int)i][j];
                var deviceMemory = Provider.Allocate(size);
                deviceMemory.CopyToDevice(ptr, 0, 0, size);
                return CreateMatrix(rows, columns, CreateCudaTensorSegment(deviceMemory));
            }
        }

        /// <inheritdoc />
        public override void L1Regularisation(INumericSegment<float> segment, float coefficient)
        {
            Provider.L1Regularisation(segment.GetDeviceMemoryPtr(), segment.Size, coefficient);
        }

        /// <inheritdoc />
        public override IMatrix<float> CreateMatrix(uint rows, uint columns, bool initialiseToZero)
        {
            return new CudaMatrix(CreateSegment(rows * columns, initialiseToZero), rows, columns, this);
        }

        /// <inheritdoc />
        public override IMatrix<float> FindDistances(IReadOnlyList<IReadOnlyNumericSegment<float>> vectors, IReadOnlyList<IReadOnlyNumericSegment<float>> compareTo, DistanceMetric distanceMetric)
        {
            if (distanceMetric is not (DistanceMetric.Euclidean or DistanceMetric.Manhattan or DistanceMetric.Cosine))
                throw new NotImplementedException();

            var size = vectors[0].Size;
            var rows = (uint)compareTo.Count;
            var columns = (uint)vectors.Count;
            var ret = Provider.Allocate(rows * columns, null, true);

            using (var vectorPtr = new PtrToDeviceMemoryList(vectors.Cast<IHaveDeviceMemory>().ToArray()))
            using (var compareToPtr = new PtrToDeviceMemoryList(compareTo.Cast<IHaveDeviceMemory>().ToArray())) {
                if (distanceMetric == DistanceMetric.Cosine) {
                    var aa = Provider.Allocate(rows * columns, null, true);
                    var bb = Provider.Allocate(rows * columns, null, true);
                    Provider.MultiCosine(size, columns, rows,
                        vectorPtr.DevicePointer,
                        compareToPtr.DevicePointer,
                        aa.DevicePointer,
                        ret.DevicePointer,
                        bb.DevicePointer
                    );
                    using var ones = CreateMatrix(rows, columns, (_, _) => 1f);
                    using var vectorMagnitude = new CudaMatrix(CreateCudaTensorSegment(aa), rows, columns, this);
                    using var vectorSqrt = vectorMagnitude.Sqrt();
                    using var compareToMagnitude = new CudaMatrix(CreateCudaTensorSegment(bb), rows, columns, this);
                    using var compareToSqrt = compareToMagnitude.Sqrt();
                    using var norms = vectorSqrt.PointwiseMultiply(compareToSqrt);
                    using var result = new CudaMatrix(CreateCudaTensorSegment(ret), rows, columns, this);
                    using var distance = result.PointwiseDivide(norms);
                    return ones.Subtract(distance);
                }

                Provider.CalculateMultiDistances(size, columns, rows,
                    vectorPtr.DevicePointer,
                    compareToPtr.DevicePointer,
                    ret.DevicePointer,
                    distanceMetric
                );
            }

            IMatrix<float> matrix = new CudaMatrix(CreateCudaTensorSegment(ret), rows, columns, this);
            if (distanceMetric == DistanceMetric.Euclidean) {
                var sqrt = matrix.Sqrt();
                matrix.Dispose();
                matrix = sqrt;
            }

            return matrix;
        }

        public override IVector<float> FindDistances(IReadOnlyNumericSegment<float> vector, IReadOnlyList<IReadOnlyNumericSegment<float>> compareTo, DistanceMetric distanceMetric)
        {
            if (distanceMetric is not (DistanceMetric.Euclidean or DistanceMetric.Manhattan or DistanceMetric.Cosine))
                throw new NotImplementedException();

            var size = vector.Size;
            var numVectors = (uint)compareTo.Count;
            var ret = Provider.Allocate(numVectors, null, true);

            var vectorPtr = (IHaveDeviceMemory)vector;
            using (var compareToPtr = new PtrToDeviceMemoryList(compareTo.Cast<IHaveDeviceMemory>().ToArray())) {
                if (distanceMetric == DistanceMetric.Cosine) {
                    var aa = Provider.Allocate(numVectors, null, true);
                    var bb = Provider.Allocate(numVectors, null, true);
                    Provider.CosineDistances(size, numVectors,
                        vectorPtr.Memory.DevicePointer,
                        compareToPtr.DevicePointer,
                        aa.DevicePointer,
                        ret.DevicePointer,
                        bb.DevicePointer
                    );
                    using var ones = CreateVector(numVectors, _ => 1f);
                    using var vectorMagnitude = new CudaVector(CreateCudaTensorSegment(aa), this);
                    using var vectorSqrt = vectorMagnitude.Sqrt();
                    using var compareToMagnitude = new CudaVector(CreateCudaTensorSegment(bb), this);
                    using var compareToSqrt = compareToMagnitude.Sqrt();
                    using var norms = vectorSqrt.PointwiseMultiply(compareToSqrt);
                    using var result = new CudaVector(CreateCudaTensorSegment(ret), this);
                    using var distance = result.PointwiseDivide(norms);
                    return ones.Subtract(distance);
                }
                
                Provider.CalculateDistances(size, numVectors,
                    vectorPtr.Memory.DevicePointer,
                    compareToPtr.DevicePointer,
                    ret.DevicePointer,
                    distanceMetric
                );
            }

            IVector<float> matrix = new CudaVector(CreateCudaTensorSegment(ret), this);
            if (distanceMetric == DistanceMetric.Euclidean) {
                var sqrt = matrix.Sqrt();
                matrix.Dispose();
                matrix = sqrt;
            }

            return matrix;
        }

        /// <inheritdoc />
        public override void BindThread()
        {
            Provider.BindThread();
        }

        internal CudaTensorSegment GetNonContinuousSegment(INumericSegment<float> segment, uint offset, uint stride, uint size)
        {
            var ptr = segment.GetDeviceMemoryPtr();
            var ret = Provider.Allocate(size);
            var status = CudaBlasNativeMethods.cublasScopy_v2(Provider.Blas, (int)size, ptr.DevicePointer + (offset * CudaProvider.FloatSize), (int)stride, ret.DevicePointer, 1);
            if (status != CuBlasStatus.Success)
                throw new CudaBlasException(status);
            return new(ret, Provider);
        }

        /// <inheritdoc />
        public override float Sum(IReadOnlyNumericSegment<float> segment)
        {
            return Provider.SumValues(segment.GetDeviceMemoryPtr(), segment.Size);
        }

        /// <inheritdoc />
        public override INumericSegment<float>[] MultiSoftmax(ArraySegment<IReadOnlyNumericSegment<float>> segments)
        {
            var index = 0;
            var ret = new INumericSegment<float>[segments.Count];

            //var stream = new CuStream();
            //DriverApiNativeMethods.Streams.cuStreamCreate(ref stream, CuStreamFlags.Default).CheckResult();
            foreach (var segment in segments) {
                var (ptr, stride) = segment.GetDeviceMemory();
                ret[index++] = Softmax(ptr, stride/*, &stream*/);
            }

            //DriverApiNativeMethods.Streams.cuStreamSynchronize(stream);
            //DriverApiNativeMethods.Streams.cuStreamDestroy_v2(stream).CheckResult();
            return ret;
        }

        /// <inheritdoc />
        public override IMatrix<float>[] MultiSoftmaxDerivative(IReadOnlyNumericSegment<float>[] segments)
        {
            var index = 0;
            var ret = new IMatrix<float>[segments.Length];
            foreach (var segment in segments) {
                var (ptr, stride) = segment.GetDeviceMemory();
                ret[index++] = SoftmaxDerivative(ptr, stride);
            }
            return ret;
        }

        //public override IVector[] SoftmaxPerRow(IMatrix matrix)
        //{
        //    using var transpose = matrix.Transpose();
        //    var segment = GetDeviceMemoryPtr(transpose.Segment);
        //    var ptr = segment.DevicePointer;
        //    var rows = matrix.RowCount;
        //    var cols = matrix.ColumnCount;

        //    var ret = new IVector[rows];
        //    //for (uint i = 0; i < rows; i++) {
        //    Parallel.For(0, rows, i => {
        //        Provider.BindThread();
        //        using var wrapper = new StreamWrapper();
        //        var stream = wrapper.Stream;

        //        var rowPtr = ptr + (matrix.ColumnCount * i) * sizeof(float);
        //        var rowBlock = new PtrToMemory(segment, rowPtr, cols * sizeof(float));
        //        var max = Provider.FindMinAndMax(rowBlock, cols, &stream).Max;
        //        var softmax = Provider.SoftmaxVector(rowBlock, cols, max, &stream);
        //        var softmaxSum = Provider.SumValues(softmax, cols, &stream);
        //        if (FloatMath.IsNotZero(softmaxSum)) {
        //            Provider.ScaleInPlace(softmax, softmax.Size, 1f / softmaxSum, &stream);
        //        }
        //        var final = wrapper.CopyResultAndDispose(softmax, Provider);
        //        ret[i] = new CudaVector(CreateCudaTensorSegment(final), this);
        //    });
        //    //}

        //    return ret;
        //}
    }
}