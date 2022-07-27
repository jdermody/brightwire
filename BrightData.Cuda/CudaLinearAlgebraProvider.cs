using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using BrightData.Cuda.Helper;
using BrightData.Helper;
using BrightData.LinearAlgebra;
using ManagedCuda;
using ManagedCuda.BasicTypes;
using ManagedCuda.CudaBlas;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace BrightData.Cuda
{
    public unsafe class CudaLinearAlgebraProvider : LinearAlgebraProvider
    {
        public CudaLinearAlgebraProvider(BrightDataContext context, CudaProvider? cuda = null) : base(context)
        {
            Provider = cuda ?? ExtensionMethods.CreateCudaProvider(context);
        }

        public override void Dispose()
        {
            GC.SuppressFinalize(this);
            base.Dispose();
#if DEBUG
            var trackedBlocks = new HashSet<IDeviceMemoryPtr>();
            foreach (var block in _scope) {
                foreach (var item in block.Keys)
                    trackedBlocks.Add(((CudaTensorSegment)((ITensor)item).Segment).DeviceMemory);
            }
            DeviceMemoryBlock.FindLeakedBlocks(trackedBlocks);
#endif
            Provider.Dispose();
        }

        public static string Name => "cuda";
        public override string ProviderName => Name;
        public override Type VectorType { get; } = typeof(CudaVector);
        public override Type MatrixType { get; } = typeof(CudaMatrix);
        public override Type Tensor3DType { get; } = typeof(CudaTensor3D);
        public override Type Tensor4DType { get; } = typeof(CudaTensor4D);
        public CudaProvider Provider { get; }

        public override ITensorSegment CreateSegment(uint size, bool initialiseToZero) => new CudaTensorSegment(Provider.Allocate(size, null, initialiseToZero));
        public override unsafe ITensorSegment CreateSegment(uint size, Func<uint, float> initializer)
        {
            using var buffer = SpanOwner<float>.Allocate((int)size);
            var span = buffer.Span;
            fixed (float* ptr = span) {
                var p = ptr;
                for (uint i = 0; i < size; i++)
                    *p++ = initializer(i);
                var deviceMemory = Provider.Allocate(size);
                deviceMemory.CopyToDevice(ptr, 0, 0, size);
                return new CudaTensorSegment(deviceMemory);
            }
        }
        public override IVector CreateVector(ITensorSegment data) => new CudaVector(OptionallyCopyToDevice(data), this);
        public override IMatrix CreateMatrix(uint rowCount, uint columnCount, ITensorSegment data) => new CudaMatrix(OptionallyCopyToDevice(data), rowCount, columnCount, this);
        public override ITensor3D CreateTensor3D(uint depth, uint rowCount, uint columnCount, ITensorSegment data) => new CudaTensor3D(data, depth, rowCount, columnCount, this);
        public override ITensor4D CreateTensor4D(uint count, uint depth, uint rowCount, uint columnCount, ITensorSegment data) => new CudaTensor4D(data, count, depth, rowCount, columnCount, this);

        public override IMatrix CreateMatrix(uint rowCount, uint columnCount, Func<uint, uint, float> initializer)
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
                return CreateMatrix(rowCount, columnCount, new CudaTensorSegment(deviceMemory));
            }
        }

        public override ITensorSegment Clone(ITensorSegment segment)
        {
            if (CudaTensorSegment.IsCuda(segment, out var cudaSegment)) {
                var ret = (CudaTensorSegment)CreateSegment(segment.Size, false);
                ret.DeviceMemory.CopyToDevice(cudaSegment.DeviceMemory);
                return ret;
            }
            return CopyToDevice(segment);
        }

        ITensorSegment OptionallyCopyToDevice(ITensorSegment segment)
        {
            if (CudaTensorSegment.IsCuda(segment, out var ret))
                return ret;
            while (segment is TensorSegmentWrapper { Stride: 1 } wrapper) {
                segment = wrapper.UnderlyingSegment;
                if (segment is CudaTensorSegment cudaTensor) {
                    var offsetBlock = Provider.Offset(cudaTensor.DeviceMemory, wrapper.Offset, wrapper.Size);
                    return new CudaTensorSegment(offsetBlock);
                }
            }
            return CopyToDevice(segment);
        }

        ITensorSegment CopyToDevice(ITensorSegment segment)
        {
            var deviceMemory = Provider.Allocate(segment.Size);
            var temp = SpanOwner<float>.Empty;
            var span = segment.GetSpan(ref temp, out var wasTempUsed);
            try {
                deviceMemory.CopyToDevice(span);
                return new CudaTensorSegment(deviceMemory);
            }
            finally {
                if (wasTempUsed)
                    temp.Dispose();
            }
        }

        public override float DotProduct(ITensorSegment tensor, ITensorSegment tensor2)
        {
            return Provider.Blas.Dot(
                GetDeviceVariable(tensor),
                1,
                GetDeviceVariable(tensor2),
                1
            );
        }

        public override ITensorSegment Add(ITensorSegment tensor, float scalar)
        {
            // TODO: create new cuda method?
            using var buffer = SpanOwner<float>.Allocate((int)tensor.Size);
            Array.Fill(buffer.DangerousGetArray().Array!, scalar);
            var ret = (CudaTensorSegment)CreateSegment(tensor.Size, false);
            ret.CopyFrom(buffer.Span);

            Provider.AddInPlace(ret.DeviceMemory, GetDeviceMemoryPtr(tensor), tensor.Size, 1f, 1f);
            return ret;
        }

        public override ITensorSegment Add(ITensorSegment tensor, ITensorSegment tensor2)
        {
            var size = GetSize(tensor, tensor2);
            var ret = (CudaTensorSegment)CreateSegment(size, false);
            ret.DeviceMemory.CopyToDevice(GetDeviceMemoryPtr(tensor2));
            Provider.Blas.Axpy(1.0f, GetDeviceVariable(tensor), 1, ret.DeviceMemory.DeviceVariable, 1);
            return ret;
        }

        public override void AddInPlace(ITensorSegment target, ITensorSegment other)
        {
            var size = GetSize(target, other);
            Provider.AddInPlace(GetDeviceMemoryPtr(target), GetDeviceMemoryPtr(other), size, 1f, 1f);
        }

        public override void AddInPlace(ITensorSegment target, ITensorSegment other, float coefficient1, float coefficient2)
        {
            var size = GetSize(target, other);
            Provider.AddInPlace(GetDeviceMemoryPtr(target), GetDeviceMemoryPtr(other), size, coefficient1, coefficient2);
        }

        public override ITensorSegment Add(ITensorSegment tensor, ITensorSegment tensor2, float coefficient1, float coefficient2)
        {
            var size = GetSize(tensor, tensor2);
            var ret = (CudaTensorSegment)CreateSegment(size, false);
            ret.DeviceMemory.CopyToDevice(GetDeviceMemoryPtr(tensor));
            Provider.AddInPlace(ret.DeviceMemory, GetDeviceMemoryPtr(tensor2), size, coefficient1, coefficient2);
            return ret;
        }

        public override void AddInPlace(ITensorSegment target, float scalar)
        {
            Provider.VectorAddInPlace(GetDeviceMemoryPtr(target), target.Size, scalar);
        }

        public override void ConstrainInPlace(ITensorSegment segment, float? minValue, float? maxValue)
        {
            var ptr = GetDeviceMemoryPtr(segment);
            Provider.Constrain(ptr, segment.Size, minValue ?? float.MinValue, maxValue ?? float.MaxValue);
        }

        public override float CosineDistance(ITensorSegment tensor, ITensorSegment other)
        {
            var size = GetSize(tensor, other);
            return Provider.CosineDistance(GetDeviceMemoryPtr(tensor), GetDeviceMemoryPtr(other), size);
        }

        public override ITensorSegment LeakyReluDerivative(ITensorSegment tensor)
        {
            var ret = Provider.LeakyReluDerivative(GetDeviceMemoryPtr(tensor), tensor.Size);
            return new CudaTensorSegment(ret);
        }

        public override ITensorSegment PointwiseMultiply(ITensorSegment tensor1, ITensorSegment tensor2)
        {
            var size = GetSize(tensor1, tensor2);
            var ret = new CudaTensorSegment(Provider.PointwiseMultiply(GetDeviceMemoryPtr(tensor1), GetDeviceMemoryPtr(tensor2), size));
            return ret;
        }

        public override void SubtractInPlace(ITensorSegment target, ITensorSegment other)
        {
            var size = GetSize(target, other);
            Provider.SubtractInPlace(GetDeviceMemoryPtr(target), GetDeviceMemoryPtr(other), size, 1f, 1f);
        }

        public override void SubtractInPlace(ITensorSegment target, ITensorSegment other, float coefficient1, float coefficient2)
        {
            var size = GetSize(target, other);
            Provider.SubtractInPlace(GetDeviceMemoryPtr(target), GetDeviceMemoryPtr(other), size, coefficient1, coefficient2);
        }

        public override ITensorSegment Multiply(ITensorSegment target, float scalar)
        {
            var ret = (CudaTensorSegment)CreateSegment(target.Size, false);
            ret.DeviceMemory.CopyToDevice(GetDeviceMemoryPtr(target));
            Provider.ScaleInPlace(ret.DeviceMemory, ret.Size, scalar);
            return ret;
        }

        public override float L2Norm(ITensorSegment segment) => Provider.Blas.Norm2(GetDeviceVariable(segment), 1);

        public override ITensorSegment Subtract(ITensorSegment tensor1, ITensorSegment tensor2)
        {
            var size = GetSize(tensor1, tensor2);
            var ret = (CudaTensorSegment)CreateSegment(size, false);
            ret.DeviceMemory.CopyToDevice(GetDeviceMemoryPtr(tensor1));
            Provider.Blas.Axpy(-1.0f, GetDeviceVariable(tensor2), 1, ret.DeviceMemory.DeviceVariable, 1);
            return ret;
        }

        public override ITensorSegment PointwiseDivide(ITensorSegment tensor1, ITensorSegment tensor2)
        {
            var size = GetSize(tensor1, tensor2);
            var ret = Provider.PointwiseDivide(GetDeviceMemoryPtr(tensor1), GetDeviceMemoryPtr(tensor2), size);
            return new CudaTensorSegment(ret);
        }

        public override ITensorSegment Subtract(ITensorSegment tensor1, ITensorSegment tensor2, float coefficient1, float coefficient2)
        {
            var size = GetSize(tensor1, tensor2);
            var ret = (CudaTensorSegment)CreateSegment(size, false);
            ret.DeviceMemory.CopyToDevice(GetDeviceMemoryPtr(tensor1));
            Provider.SubtractInPlace(ret.DeviceMemory, GetDeviceMemoryPtr(tensor2), size, coefficient1, coefficient2);
            return ret;
        }

        public override float L1Norm(ITensorSegment segment)
        {
            var abs = Abs(segment);
            try {
                return Provider.SumValues(GetDeviceMemoryPtr(abs), segment.Size);
            }
            finally {
                abs.Release();
            }
        }

        public override (float Min, float Max, uint MinIndex, uint MaxIndex) GetMinAndMaxValues(ITensorSegment segment)
        {
            //var buffer = MemoryOwner<float>.Allocate((int)ptr.Size);
            //using var temp = new ArrayPoolTensorSegment(buffer);
            //ptr.CopyToHost(buffer.DangerousGetArray());
            //return segment.GetMinAndMaxValues();

            // TODO: fix absolute values regarding indices
            var ptr = GetDeviceMemoryPtr(segment);
            var (min, max) = Provider.FindMinAndMax(ptr, segment.Size);
            var maxIndex = (uint)Provider.Blas.Max(ptr.DeviceVariable, 1) - 1;
            var minIndex = (uint)Provider.Blas.Min(GetDeviceVariable(segment), 1) - 1;
            return (min, max, minIndex, maxIndex);
        }
        public override float GetMin(ITensorSegment segment) => Provider.FindMinAndMax(GetDeviceMemoryPtr(segment), segment.Size).Min;
        public override float GetMax(ITensorSegment segment) => Provider.FindMinAndMax(GetDeviceMemoryPtr(segment), segment.Size).Max;
        //public override uint GetMinIndex(ITensorSegment2 segment) => (uint)_cuda.Blas.Min(GetDeviceVariable(segment), 1) - 1;
        //public override uint GetMaxIndex(ITensorSegment2 segment) => (uint)_cuda.Blas.Max(GetDeviceVariable(segment), 1) - 1;

        public override bool IsEntirelyFinite(ITensorSegment segment) => Provider.IsFinite(GetDeviceMemoryPtr(segment), segment.Size);

        public override ITensorSegment Reverse(ITensorSegment segment)
        {
            var ret = Provider.Reverse(GetDeviceMemoryPtr(segment), segment.Size);
            return new CudaTensorSegment(ret);
        }

        public override float MeanSquaredDistance(ITensorSegment tensor, ITensorSegment other)
        {
            var size = GetSize(tensor, other);
            var temp = Subtract(tensor, other);
            try {
                var norm = temp.L2Norm();
                return norm * norm / size;
            }
            finally {
                temp.Release();
            }
        }

        public override float SquaredEuclideanDistance(ITensorSegment tensor, ITensorSegment other)
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

        public override float EuclideanDistance(ITensorSegment tensor, ITensorSegment other)
        {
            var size = GetSize(tensor, other);
            return Provider.EuclideanDistance(GetDeviceMemoryPtr(tensor), GetDeviceMemoryPtr(other), size);
        }

        public override float ManhattanDistance(ITensorSegment tensor, ITensorSegment other)
        {
            var size = GetSize(tensor, other);
            return Provider.ManhattanDistance(GetDeviceMemoryPtr(tensor), GetDeviceMemoryPtr(other), size);
        }

        public override float Average(ITensorSegment segment) => Provider.SumValues(GetDeviceMemoryPtr(segment), segment.Size) / segment.Size;
        public override ITensorSegment Abs(ITensorSegment tensor) => new CudaTensorSegment(Provider.Abs(GetDeviceMemoryPtr(tensor), tensor.Size));
        public override ITensorSegment Log(ITensorSegment tensor) => new CudaTensorSegment(Provider.Log(GetDeviceMemoryPtr(tensor), tensor.Size));
        public override ITensorSegment Squared(ITensorSegment tensor) => PointwiseMultiply(tensor, tensor);
        public override ITensorSegment Sigmoid(ITensorSegment tensor) => new CudaTensorSegment(Provider.Sigmoid(GetDeviceMemoryPtr(tensor), tensor.Size));
        public override ITensorSegment SigmoidDerivative(ITensorSegment tensor) => new CudaTensorSegment(Provider.SigmoidDerivative(GetDeviceMemoryPtr(tensor), tensor.Size));
        public override ITensorSegment Tanh(ITensorSegment tensor) => new CudaTensorSegment(Provider.TanH(GetDeviceMemoryPtr(tensor), tensor.Size));
        public override ITensorSegment TanhDerivative(ITensorSegment tensor) => new CudaTensorSegment(Provider.TanHDerivative(GetDeviceMemoryPtr(tensor), tensor.Size));

        public override ITensorSegment Exp(ITensorSegment tensor)
        {
            return base.Exp(tensor);
        }

        public override ITensorSegment Relu(ITensorSegment tensor) => new CudaTensorSegment(Provider.Relu(GetDeviceMemoryPtr(tensor), tensor.Size));
        public override ITensorSegment ReluDerivative(ITensorSegment tensor) => new CudaTensorSegment(Provider.ReluDerivative(GetDeviceMemoryPtr(tensor), tensor.Size));
        public override ITensorSegment LeakyRelu(ITensorSegment tensor) => new CudaTensorSegment(Provider.LeakyRelu(GetDeviceMemoryPtr(tensor), tensor.Size));

        public override ITensorSegment Softmax(ITensorSegment tensor) => Softmax(GetDeviceMemoryPtr(tensor), 1);
        ITensorSegment Softmax(IDeviceMemoryPtr ptr, uint stride)
        {
            var max = Provider.FindMinAndMax(ptr, ptr.Size, stride).Max;
            var softmax = Provider.SoftmaxVector(ptr, ptr.Size, max, stride);
            var softmaxSum = Provider.SumValues(softmax, ptr.Size);
            if (FloatMath.IsNotZero(softmaxSum))
                Provider.ScaleInPlace(softmax, softmax.Size, 1f / softmaxSum);
            return new CudaTensorSegment(softmax);
        }

        public override IMatrix SoftmaxDerivative(ITensorSegment tensor) => SoftmaxDerivative(GetDeviceMemoryPtr(tensor), 1);
        IMatrix SoftmaxDerivative(IDeviceMemoryPtr ptr, uint stride)
        {
            var ret = Provider.VectorSoftmaxDerivative(ptr, ptr.Size, stride);
            return CreateMatrix(ptr.Size, ptr.Size, new CudaTensorSegment(ret));
        }

        public override ITensorSegment Pow(ITensorSegment tensor, float power) => new CudaTensorSegment(Provider.Pow(GetDeviceMemoryPtr(tensor), tensor.Size, power));
        public override ITensorSegment Sqrt(ITensorSegment tensor) => new CudaTensorSegment(Provider.Sqrt(GetDeviceMemoryPtr(tensor), tensor.Size, 1e-8f));
        public override void PointwiseDivideInPlace(ITensorSegment target, ITensorSegment other)
        {
            var size = GetSize(target, other);
            var ptr = GetDeviceMemoryPtr(target);
            var temp = Provider.PointwiseDivide(ptr, GetDeviceMemoryPtr(other), size);
            ptr.CopyToDevice(temp);
            temp.Release();
        }

        public override void RoundInPlace(ITensorSegment tensor, float lower, float upper, float? mid)
        {
            Provider.RoundInPlace(GetDeviceMemoryPtr(tensor), tensor.Size, lower, upper, mid ?? lower + (upper - lower) / 2);
        }

        public override void PointwiseMultiplyInPlace(ITensorSegment target, ITensorSegment other)
        {
            var size = GetSize(target, other);
            var ptr = GetDeviceMemoryPtr(target);
            var temp = Provider.PointwiseMultiply(ptr, GetDeviceMemoryPtr(other), size);
            ptr.CopyToDevice(temp);
            temp.Release();
        }

        public override void MultiplyInPlace(ITensorSegment target, float scalar)
        {
            Provider.ScaleInPlace(GetDeviceMemoryPtr(target), target.Size, scalar);
        }

        public override IEnumerable<ITensorSegment> Split(ITensorSegment segment, uint blockCount)
        {
            var blockSize = segment.Size / blockCount;
            var ptr = GetDeviceMemoryPtr(segment);

            return blockCount.AsRange().Select(i => {
                var ptr2 = Provider.OffsetByBlock(ptr, i, blockSize);
                return new CudaTensorSegment(ptr2);
            });
        }

        public override float StdDev(ITensorSegment tensor, float? mean)
        {
            return Provider.FindStdDev(GetDeviceMemoryPtr(tensor), tensor.Size, mean ?? Average(tensor));
        }

        public override uint? Search(ITensorSegment segment, float value)
        {
            return base.Search(segment, value);
        }

        public override IVector ColumnSums(IMatrix matrix)
        {
            var ret = Provider.SumColumns(GetDeviceMemoryPtr(matrix.Segment), matrix.RowCount, matrix.ColumnCount);
            return CreateVector(new CudaTensorSegment(ret));
        }

        public override IVector GetDiagonal(IMatrix matrix)
        {
            var ret = Provider.Diagonal(GetDeviceMemoryPtr(matrix.Segment), matrix.RowCount, matrix.ColumnCount);
            return CreateVector(new CudaTensorSegment(ret));
        }

        public override IMatrix Multiply(IMatrix matrix, IMatrix other)
        {
            var ret = Provider.Allocate(matrix.RowCount * other.ColumnCount);
            int rowsA = (int)matrix.RowCount, columnsArowsB = (int)matrix.ColumnCount, columnsB = (int)other.ColumnCount;

            float alpha = 1.0f, beta = 0.0f;
            CudaBlasNativeMethods.cublasSgemm_v2(Provider.Blas.CublasHandle,
                Operation.NonTranspose,
                Operation.NonTranspose,
                rowsA,
                columnsB,
                columnsArowsB,
                ref alpha,
                GetDeviceMemoryPtr(matrix.Segment).DevicePointer,
                rowsA,
                GetDeviceMemoryPtr(other.Segment).DevicePointer,
                columnsArowsB,
                ref beta,
                ret.DevicePointer,
                rowsA
            );
            return CreateMatrix(matrix.RowCount, other.ColumnCount, new CudaTensorSegment(ret));
        }

        public override IVector RowSums(IMatrix matrix)
        {
            var ret = Provider.SumRows(GetDeviceMemoryPtr(matrix.Segment), matrix.RowCount, matrix.ColumnCount);
            return CreateVector(new CudaTensorSegment(ret));
        }

        public override IMatrix Transpose(IMatrix matrix)
        {
            var ret = Provider.Allocate(matrix.RowCount * matrix.ColumnCount);
            float alpha = 1.0f, beta = 0.0f;
            CudaBlasNativeMethods.cublasSgeam(Provider.Blas.CublasHandle,
                Operation.Transpose,
                Operation.NonTranspose,
                (int)matrix.ColumnCount,
                (int)matrix.RowCount,
                ref alpha,
                GetDeviceMemoryPtr(matrix.Segment).DevicePointer,
                (int)matrix.RowCount,

                ref beta,
                new CUdeviceptr(0),
                (int)matrix.ColumnCount,
                ret.DevicePointer,
                (int)matrix.ColumnCount
            );
            return CreateMatrix(matrix.ColumnCount, matrix.RowCount, new CudaTensorSegment(ret));
        }

        public override IMatrix TransposeSecondAndMultiply(IMatrix matrix, IMatrix other)
        {
            var ret = Provider.Allocate(matrix.RowCount * other.RowCount);
            int rowsA = (int)matrix.RowCount, columnsArowsB = (int)matrix.ColumnCount, rowsB = (int)other.RowCount;

            float alpha = 1.0f, beta = 0.0f;
            CudaBlasNativeMethods.cublasSgemm_v2(Provider.Blas.CublasHandle,
                Operation.NonTranspose,
                Operation.Transpose,
                rowsA,
                rowsB,
                columnsArowsB,
                ref alpha,
                GetDeviceMemoryPtr(matrix.Segment).DevicePointer,
                rowsA,
                GetDeviceMemoryPtr(other.Segment).DevicePointer,
                rowsB,
                ref beta,
                ret.DevicePointer,
                rowsA
            );
            return CreateMatrix(matrix.RowCount, other.RowCount, new CudaTensorSegment(ret));
        }

        public override IMatrix TransposeFirstAndMultiply(IMatrix matrix, IMatrix other)
        {
            var ret = Provider.Allocate(matrix.ColumnCount * other.ColumnCount);
            int rowsA = (int)matrix.RowCount, columnsA = (int)matrix.ColumnCount, columnsB = (int)other.ColumnCount, rowsB = (int)other.RowCount;

            float alpha = 1.0f, beta = 0.0f;
            CudaBlasNativeMethods.cublasSgemm_v2(Provider.Blas.CublasHandle,
                Operation.Transpose,
                Operation.NonTranspose,
                columnsA,
                columnsB,
                rowsB,
                ref alpha,
                GetDeviceMemoryPtr(matrix.Segment).DevicePointer,
                rowsA,
                GetDeviceMemoryPtr(other.Segment).DevicePointer,
                rowsB,
                ref beta,
                ret.DevicePointer,
                columnsA
            );
            return CreateMatrix(matrix.ColumnCount, other.ColumnCount, new CudaTensorSegment(ret));
        }

        public override (IMatrix U, IVector S, IMatrix VT) Svd(IMatrix matrix)
        {
            var solver = Provider.Solver;
            var rows = matrix.RowCount;
            var columns = matrix.ColumnCount;

            // find the size of the required buffer
            var bufferSize = solver.GesvdBufferSizeFloat((int)rows, (int)columns);
            var mn = System.Math.Min(rows, columns);

            // allocate output buffers
            var s = Provider.Allocate(mn);
            var u = Provider.Allocate(rows * rows);
            var vt = Provider.Allocate(columns * columns);

            // call cusolver to find the SVD
            try {
                var buffer = Provider.Allocate((uint)bufferSize);
                var rwork = Provider.Allocate(mn);
                var a = Provider.Allocate(rows * columns);
                try {
                    using var devInfo = new CudaDeviceVariable<int>(1);
                    a.CopyToDevice(GetDeviceMemoryPtr(matrix.Segment));
                    solver.Gesvd(
                        'A',
                        'A',
                        (int)rows,
                        (int)columns,
                        a.DeviceVariable,
                        (int)rows,
                        s.DeviceVariable,
                        u.DeviceVariable,
                        (int)rows,
                        vt.DeviceVariable,
                        (int)columns,
                        buffer.DeviceVariable,
                        bufferSize,
                        rwork.DeviceVariable,
                        devInfo
                    );
                    return (
                        CreateMatrix(rows, rows, new CudaTensorSegment(u)),
                        CreateVector(new CudaTensorSegment(s)),
                        CreateMatrix(columns, columns, new CudaTensorSegment(vt))
                    );
                }
                finally {
                    buffer.Release();
                    rwork.Release();
                    a.Release();
                }
            }
            catch {
                s.Release();
                u.Release();
                vt.Release();
                throw;
            }
        }

        public override ITensorSegment CherryPickIndices(ITensorSegment tensor, uint[] indices)
        {
            var data = Provider.VectorCopy(GetDeviceMemoryPtr(tensor), indices);
            return new CudaTensorSegment(data);
        }

        public override void EuclideanNormalization(ITensorSegment segment)
        {
            var norm = L2Norm(segment);
            if (FloatMath.IsNotZero(norm))
                Multiply(segment, 1f / norm);
        }

        public override void FeatureScaleNormalization(ITensorSegment segment)
        {
            var ptr = GetDeviceMemoryPtr(segment);
            var (min, max) = Provider.FindMinAndMax(ptr, segment.Size);
            var range = max - min;
            if (FloatMath.IsNotZero(range))
                Provider.Normalise(ptr, segment.Size, min, range);
        }

        public override void ManhattanNormalization(ITensorSegment segment)
        {
            var norm = L1Norm(segment);
            if (FloatMath.IsNotZero(norm))
                Multiply(segment, 1f / norm);
        }

        public override void StandardNormalization(ITensorSegment segment)
        {
            var mean = Average(segment);
            var stdDev = StdDev(segment, mean);
            if (FloatMath.IsNotZero(stdDev))
                Provider.Normalise(GetDeviceMemoryPtr(segment), segment.Size, mean, stdDev);
        }

        public override (IMatrix Left, IMatrix Right) SplitAtColumn(IMatrix matrix, uint columnIndex)
        {
            var size = matrix.ColumnCount - columnIndex;
            var ret1 = Provider.Allocate(matrix.RowCount * columnIndex);
            var ret2 = Provider.Allocate(matrix.RowCount * size);
            Provider.SplitRows(GetDeviceMemoryPtr(matrix.Segment), ret1, ret2, matrix.RowCount, matrix.ColumnCount, columnIndex);
            return (CreateMatrix(matrix.RowCount, columnIndex, new CudaTensorSegment(ret1)), CreateMatrix(matrix.RowCount, size, new CudaTensorSegment(ret2)));
        }

        public override (IMatrix Top, IMatrix Bottom) SplitAtRow(IMatrix matrix, uint rowIndex)
        {
            var size = matrix.RowCount - rowIndex;
            var ret1 = Provider.Allocate(rowIndex * matrix.ColumnCount);
            var ret2 = Provider.Allocate(size * matrix.ColumnCount);
            Provider.SplitColumns(GetDeviceMemoryPtr(matrix.Segment), ret1, ret2, matrix.RowCount, matrix.ColumnCount, rowIndex);
            return (CreateMatrix(rowIndex, matrix.ColumnCount, new CudaTensorSegment(ret1)), CreateMatrix(size, matrix.ColumnCount, new CudaTensorSegment(ret2)));
        }

        public override IMatrix ConcatColumns(IMatrix top, IMatrix bottom)
        {
            Debug.Assert(top.ColumnCount == bottom.ColumnCount);
            var size = top.RowCount + bottom.RowCount;
            var ret = Provider.Allocate(size * top.ColumnCount);
            Provider.ConcatColumns(GetDeviceMemoryPtr(top.Segment), GetDeviceMemoryPtr(bottom.Segment), ret, size, top.ColumnCount, top.RowCount, bottom.RowCount);
            return CreateMatrix(size, top.ColumnCount, new CudaTensorSegment(ret));
        }

        public override IMatrix ConcatRows(IMatrix left, IMatrix right)
        {
            Debug.Assert(left.RowCount == right.RowCount);
            var size = left.ColumnCount + right.ColumnCount;
            var ret = Provider.Allocate(left.RowCount * size);
            Provider.ConcatRows(GetDeviceMemoryPtr(left.Segment), GetDeviceMemoryPtr(right.Segment), ret, left.RowCount, size, left.ColumnCount);
            return CreateMatrix(left.RowCount, size, new CudaTensorSegment(ret));
        }

        public override IMatrix GetNewMatrixFromColumns(IMatrix matrix, IEnumerable<uint> columnIndices)
        {
            uint offset = 0;
            var indices = columnIndices.ToList();
            var ret = Provider.Allocate(matrix.RowCount * (uint)indices.Count);
            foreach (var item in indices) {
                ret.DeviceVariable.CopyToDevice(GetDeviceVariable(matrix.Segment), item * matrix.RowCount * CudaProvider.FloatSize, offset * CudaProvider.FloatSize, matrix.RowCount * CudaProvider.FloatSize);
                offset += matrix.RowCount;
            }
            return CreateMatrix(matrix.RowCount, (uint)indices.Count, new CudaTensorSegment(ret));
        }

        public override IMatrix GetNewMatrixFromRows(IMatrix matrix, IEnumerable<uint> rowIndices)
        {
            int offset = 0;
            var indices = rowIndices.ToList();
            var ret = Provider.Allocate(matrix.ColumnCount * (uint)indices.Count);
            foreach (var item in indices) {
                CudaBlasNativeMethods.cublasScopy_v2(Provider.Blas.CublasHandle,
                    n: (int)matrix.ColumnCount,
                    x: GetDeviceMemoryPtr(matrix.Segment).DevicePointer + (item * CudaProvider.FloatSize),
                    incx: (int)matrix.RowCount,
                    y: ret.DevicePointer + (offset * CudaProvider.FloatSize),
                    incy: indices.Count
                );
                offset += 1;
            }
            return CreateMatrix((uint)indices.Count, matrix.ColumnCount, new CudaTensorSegment(ret));
        }

        CUdeviceptr[] GetDevicePointers(List<CudaTensorSegment> segments)
        {
            var len = segments.Count;
            var ret = new CUdeviceptr[len];
            for (var i = 0; i < len; i++) {
                var segment = segments[i];
                ret[i] = segment.DeviceMemory.DevicePointer;
            }
            return ret;
        }

        public override IMatrix CreateMatrixFromColumns(ReadOnlySpan<ITensorSegment> vectorColumns)
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
                using var devicePtr = new CudaDeviceVariable<CUdeviceptr>(columns);
                devicePtr.CopyToDevice(devicePointers);
                Provider.CopyToMatrixColumns(rows, columns, devicePtr, ret.DeviceMemory);
                return CreateMatrix(rows, columns, ret);
                //var ret = (CudaTensorSegment)CreateSegment(rows * columns, false);
                //return CreateMatrix(rows, columns, ret);
            }
            else {
                var size = rows * columns;
                using var buffer = SpanOwner<float>.Allocate((int)size);
                fixed (float* ptr = buffer.Span) {
                    var p = ptr;
                    for (uint j = 0; j < columns; j++)
                        for (uint i = 0; i < rows; i++)
                            *p++ = vectorColumns[(int)j][i];
                    var deviceMemory = Provider.Allocate(size);
                    deviceMemory.CopyToDevice(ptr, 0, 0, size);
                    return CreateMatrix(rows, columns, new CudaTensorSegment(deviceMemory));
                }
            }
        }

        public override IMatrix CreateMatrixFromRows(ReadOnlySpan<ITensorSegment> vectorRows)
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
                using var devicePtr = new CudaDeviceVariable<CUdeviceptr>(rows);
                devicePtr.CopyToDevice(devicePointers);
                Provider.CopyToMatrixRows(rows, columns, devicePtr, ret.DeviceMemory);
                return CreateMatrix(rows, columns, ret);
                //var ret = (CudaTensorSegment)CreateSegment(rows * columns, false);
                //return CreateMatrix(rows, columns, ret);
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
                return CreateMatrix(rows, columns, new CudaTensorSegment(deviceMemory));
            }
        }

        public override void L1Regularisation(ITensorSegment segment, float coefficient)
        {
            Provider.L1Regularisation(GetDeviceMemoryPtr(segment), segment.Size, coefficient);
        }

        public override void AddToEachColumn(IMatrix matrix, ITensorSegment segment)
        {
            Provider.AddToEachColumn(GetDeviceMemoryPtr(matrix.Segment), GetDeviceMemoryPtr(segment), matrix.RowCount, matrix.ColumnCount);
        }

        public override void AddToEachRow(IMatrix matrix, ITensorSegment segment)
        {
            Provider.AddToEachRow(GetDeviceMemoryPtr(matrix.Segment), GetDeviceMemoryPtr(segment), matrix.RowCount, matrix.ColumnCount);
        }

        static CudaDeviceVariable<float> GetDeviceVariable(ITensorSegment segment) => GetDeviceMemoryPtr(segment).DeviceVariable;
        internal static IDeviceMemoryPtr GetDeviceMemoryPtr(ITensorSegment segment)
        {
            if (segment is CudaTensorSegment cudaSegment) {
                if (!segment.IsValid)
                    throw new Exception("CUDA tensor was not valid");
                return cudaSegment.DeviceMemory;
            }

            throw new Exception("CUDA tensors can only be used with other CUDA tensors");
        }
        internal static (IDeviceMemoryPtr Ptr, uint Stride) GetDeviceMemory(ITensorSegment segment)
        {
            var foundWrapper = false;
            uint offset = 0, stride = 0, size = uint.MaxValue;
            while (segment is TensorSegmentWrapper wrapper) {
                offset += wrapper.Offset;
                stride += wrapper.Stride;
                size = wrapper.Size;
                foundWrapper = true;
                segment = wrapper.UnderlyingSegment;
            }
            if (segment is CudaTensorSegment cudaSegment) {
                if (!segment.IsValid)
                    throw new Exception("CUDA tensor was not valid");

                var ptr = cudaSegment.DeviceMemory;
                if(offset > 0 || (foundWrapper && size != segment.Size))
                    ptr = ptr.Offset(offset, size);
                return (ptr, foundWrapper ? stride : 1);
            }

            throw new Exception("CUDA tensors can only be used with other CUDA tensors");
        }

        public IMatrix CreateMatrix(uint rows, uint columns, bool initialiseToZero)
        {
            return new CudaMatrix(CreateSegment(rows * columns, initialiseToZero), rows, columns, this);
        }

        public override IMatrix FindDistances(IVector[] vectors, IReadOnlyList<IVector> compareTo, DistanceMetric distanceMetric)
        {
            if (distanceMetric is not (DistanceMetric.Euclidean or DistanceMetric.Manhattan or DistanceMetric.Cosine))
                throw new NotImplementedException();

            var size = vectors[0].Size;
            var rows = (uint)compareTo.Count;
            var columns = (uint)vectors.Length;
            var ret = Provider.Allocate(rows * columns, null, true);

            using (var vectorPtr = new PtrToDeviceMemoryList(vectors.Cast<IHaveDeviceMemory>().ToArray()))
            using (var compareToPtr = new PtrToDeviceMemoryList(compareTo.Cast<IHaveDeviceMemory>().ToArray())) {
                if (distanceMetric == DistanceMetric.Cosine) {
                    var aa = Provider.Allocate(rows * columns, null, true);
                    var bb = Provider.Allocate(rows * columns, null, true);
                    Provider.InvokeTensor(Provider._multiCosine, null, size, columns, rows,
                        vectorPtr.DevicePointer,
                        compareToPtr.DevicePointer,
                        aa.DevicePointer,
                        ret.DevicePointer,
                        bb.DevicePointer,
                        rows,
                        columns,
                        size
                    );
                    using var ones = CreateMatrix(rows, columns, (i, j) => 1f);
                    using var vectorMagnitude = new CudaMatrix(new CudaTensorSegment(aa), rows, columns, this);
                    using var vectorSqrt = vectorMagnitude.Sqrt();
                    using var compareToMagnitude = new CudaMatrix(new CudaTensorSegment(bb), rows, columns, this);
                    using var compareToSqrt = compareToMagnitude.Sqrt();
                    using var norms = vectorSqrt.PointwiseMultiply(compareToSqrt);
                    using var result = new CudaMatrix(new CudaTensorSegment(ret), rows, columns, this);
                    using var distance = result.PointwiseDivide(norms);
                    return ones.Subtract(distance);
                }

                Provider.InvokeTensor(Provider._calculateDistance, null, size, columns, rows,
                    vectorPtr.DevicePointer,
                    compareToPtr.DevicePointer,
                    ret.DevicePointer,
                    rows,
                    columns,
                    size,
                    (uint)distanceMetric
                );
            }

            IMatrix matrix = new CudaMatrix(new CudaTensorSegment(ret), rows, columns, this);
            if (distanceMetric == DistanceMetric.Euclidean) {
                var sqrt = matrix.Sqrt();
                matrix.Dispose();
                matrix = sqrt;
            }

            return matrix;
        }

        public override void BindThread()
        {
            Provider.BindThread();
        }

        internal CudaTensorSegment GetNonContinuousSegment(ITensorSegment segment, uint offset, uint stride, uint size)
        {
            var ptr = GetDeviceMemoryPtr(segment);
            var ret = Provider.Allocate(size);
            var status = CudaBlasNativeMethods.cublasScopy_v2(Provider.Blas.CublasHandle, (int)size, ptr.DevicePointer + (offset * CudaProvider.FloatSize), (int)stride, ret.DevicePointer, 1);
            if (status != CublasStatus.Success)
                throw new CudaBlasException(status);
            return new(ret);
        }

        public override float Sum(ITensorSegment segment)
        {
            return Provider.SumValues(GetDeviceMemoryPtr(segment), segment.Size);
        }

        public override IMatrix AddMatrices(ITensor3D tensor)
        {
            return base.AddMatrices(tensor);
        }

        public override ITensor3D AddPadding(ITensor3D tensor, uint padding)
        {
            var (ret, rows, cols) = Provider.TensorAddPadding(GetDeviceMemoryPtr(tensor.Segment), tensor.RowCount, tensor.ColumnCount, tensor.Depth, 1, padding);
            return new CudaTensor3D(new CudaTensorSegment(ret), tensor.Depth, rows, cols, this);
        }

        public override ITensor4D AddPadding(ITensor4D tensor, uint padding)
        {
            var (ret, rows, cols) = Provider.TensorAddPadding(GetDeviceMemoryPtr(tensor.Segment), tensor.RowCount, tensor.ColumnCount, tensor.Depth, tensor.Count, padding);
            return new CudaTensor4D(new CudaTensorSegment(ret), tensor.Count, tensor.Depth, rows, cols, this);
        }

        public override void AddToEachRow(ITensor3D tensor, IVector vector)
        {
            for (uint i = 0, len = tensor.Depth; i < len; i++) {
                using var matrix = tensor.GetMatrix(i);
                matrix.AddToEachRow(vector.Segment);
            }
        }

        public override IVector ColumnSums(ITensor4D tensor)
        {
            return base.ColumnSums(tensor);
        }

        public override IMatrix GetMatrix(ITensor3D tensor, uint index)
        {
            var ptr = Provider.OffsetByBlock(GetDeviceMemoryPtr(tensor.Segment), index, tensor.MatrixSize);
            return new CudaMatrix(new CudaTensorSegment(ptr), tensor.RowCount, tensor.ColumnCount, this);
        }

        public override ITensor3D GetTensor(ITensor4D tensor, uint index)
        {
            var ptr = Provider.OffsetByBlock(GetDeviceMemoryPtr(tensor.Segment), index, tensor.TensorSize);
            return new CudaTensor3D(new CudaTensorSegment(ptr), tensor.Depth, tensor.RowCount, tensor.ColumnCount, this);
        }

        public override IMatrix Im2Col(ITensor3D tensor, uint filterWidth, uint filterHeight, uint xStride, uint yStride)
        {
            var (ptr, rows, columns, _) = Provider.TensorIm2Col(GetDeviceMemoryPtr(tensor.Segment), tensor.RowCount, tensor.ColumnCount, tensor.Depth, 1, filterWidth, filterHeight, xStride, yStride);
            return new CudaMatrix(new CudaTensorSegment(ptr), rows, columns, this);
        }

        public override ITensor3D Im2Col(ITensor4D tensor, uint filterWidth, uint filterHeight, uint xStride, uint yStride)
        {
            var (ptr, rows, columns, depth) = Provider.TensorIm2Col(GetDeviceMemoryPtr(tensor.Segment), tensor.RowCount, tensor.ColumnCount, tensor.Depth, tensor.Count, filterWidth, filterHeight, xStride, yStride);
            return new CudaTensor3D(new CudaTensorSegment(ptr), depth, rows, columns, this);
        }

        public override (ITensor3D Result, ITensor3D? Indices) MaxPool(ITensor3D tensor, uint filterWidth, uint filterHeight, uint xStride, uint yStride, bool saveIndices)
        {
            var (ptr, indices, rows, cols) = Provider.TensorMaxPool(GetDeviceMemoryPtr(tensor.Segment), tensor.RowCount, tensor.ColumnCount, tensor.Depth, 1, filterWidth, filterHeight, xStride, yStride, saveIndices);
            var ret = new CudaTensor3D(new CudaTensorSegment(ptr), tensor.Depth, rows, cols, this);
            var indexTensor = indices is null ? null : new CudaTensor3D(new CudaTensorSegment(indices), tensor.Depth, rows, cols, this);
            return (ret, indexTensor);
        }

        public override (ITensor4D Result, ITensor4D? Indices) MaxPool(ITensor4D tensor, uint filterWidth, uint filterHeight, uint xStride, uint yStride, bool saveIndices)
        {
            var (ptr, indices, rows, cols) = Provider.TensorMaxPool(GetDeviceMemoryPtr(tensor.Segment), tensor.RowCount, tensor.ColumnCount, tensor.Depth, tensor.Count, filterWidth, filterHeight, xStride, yStride, saveIndices);
            var ret = new CudaTensor4D(new CudaTensorSegment(ptr), tensor.Count, tensor.Depth, rows, cols, this);
            var indexTensor = indices is null ? null : new CudaTensor4D(new CudaTensorSegment(indices), tensor.Count, tensor.Depth, rows, cols, this);
            return (ret, indexTensor);
        }

        public override ITensor3D Multiply(ITensor3D tensor, IMatrix matrix)
        {
            var ptr = GetDeviceMemoryPtr(tensor.Segment);
            uint rowsA = tensor.RowCount, columnsArowsB = tensor.ColumnCount, columnsB = matrix.ColumnCount;
            float alpha = 1.0f, beta = 0.0f;
            var outputPtr = Provider.Allocate(tensor.RowCount * columnsB * tensor.Depth);
            var output = new CudaTensor3D(new CudaTensorSegment(outputPtr), tensor.Depth, tensor.RowCount, columnsB, this);

            var status = CudaBlasNativeMethods.cublasSgemmStridedBatched(Provider.Blas.CublasHandle,
                Operation.NonTranspose,
                Operation.NonTranspose,
                (int)rowsA,
                (int)columnsB,
                (int)columnsArowsB,
                ref alpha,
                ptr.DevicePointer,
                (int)rowsA,
                tensor.MatrixSize,
                GetDeviceMemoryPtr(matrix.Segment).DevicePointer,
                (int)columnsArowsB,
                0,
                ref beta,
                outputPtr.DevicePointer,
                (int)rowsA,
                tensor.RowCount * columnsB,
                (int)tensor.Depth
            );
            if (status != CublasStatus.Success)
                throw new CudaBlasException(status);
            return output;
        }

        public override ITensor3D RemovePadding(ITensor3D tensor, uint padding)
        {
            var (ptr, rows, cols) = Provider.TensorRemovePadding(GetDeviceMemoryPtr(tensor.Segment), tensor.RowCount, tensor.ColumnCount, tensor.Depth, 1, padding);
            return new CudaTensor3D(new CudaTensorSegment(ptr), tensor.Depth, rows, cols, this);
        }

        public override ITensor4D RemovePadding(ITensor4D tensor, uint padding)
        {
            var (ptr, rows, cols) = Provider.TensorRemovePadding(GetDeviceMemoryPtr(tensor.Segment), tensor.RowCount, tensor.ColumnCount, tensor.Depth, tensor.Count, padding);
            return new CudaTensor4D(new CudaTensorSegment(ptr), tensor.Count, tensor.Depth, rows, cols, this);
        }

        public override ITensor3D ReverseIm2Col(ITensor3D tensor, IMatrix filter, uint outputRows, uint outputColumns, uint outputDepth, uint filterWidth, uint filterHeight, uint xStride, uint yStride)
        {
            var (ptr, rows, cols, depth, _) = Provider.TensorReverseIm2Col(GetDeviceMemoryPtr(tensor.Segment), GetDeviceMemoryPtr(filter.Segment), tensor.RowCount, tensor.ColumnCount, tensor.Depth, 1, outputRows, outputColumns, outputDepth, filterWidth, filterHeight, xStride, yStride);
            return new CudaTensor3D(new CudaTensorSegment(ptr), depth, rows, cols, this);
        }

        public override ITensor4D ReverseIm2Col(ITensor4D tensor, IMatrix filter, uint outputRows, uint outputColumns, uint outputDepth, uint filterWidth, uint filterHeight, uint xStride, uint yStride)
        {
            var (ptr, rows, cols, depth, count) = Provider.TensorReverseIm2Col(GetDeviceMemoryPtr(tensor.Segment), GetDeviceMemoryPtr(filter.Segment), tensor.RowCount, tensor.ColumnCount, tensor.Depth, tensor.Count, outputRows, outputColumns, outputDepth, filterWidth, filterHeight, xStride, yStride);
            return new CudaTensor4D(new CudaTensorSegment(ptr), count, depth, rows, cols, this);
        }

        public override ITensor3D ReverseMaxPool(ITensor3D tensor, ITensor3D indices, uint outputRows, uint outputColumns, uint filterWidth, uint filterHeight, uint xStride, uint yStride)
        {
            var ptr = Provider.TensorReverseMaxPool(GetDeviceMemoryPtr(tensor.Segment), GetDeviceMemoryPtr(indices.Segment), tensor.RowCount, tensor.ColumnCount, tensor.Depth, 1, outputRows, outputColumns, filterWidth, filterHeight, xStride, yStride);
            return new CudaTensor3D(new CudaTensorSegment(ptr), tensor.Depth, outputRows, outputColumns, this);
        }

        public override ITensor4D ReverseMaxPool(ITensor4D tensor, ITensor4D indices, uint outputRows, uint outputColumns, uint filterWidth, uint filterHeight, uint xStride, uint yStride)
        {
            var ptr = Provider.TensorReverseMaxPool(GetDeviceMemoryPtr(tensor.Segment), GetDeviceMemoryPtr(indices.Segment), tensor.RowCount, tensor.ColumnCount, tensor.Depth, tensor.Count, outputRows, outputColumns, filterWidth, filterHeight, xStride, yStride);
            return new CudaTensor4D(new CudaTensorSegment(ptr), tensor.Count, tensor.Depth, outputRows, outputColumns, this);
        }

        public override ITensor3D TransposeFirstAndMultiply(ITensor3D tensor, ITensor4D other)
        {
            var ptr = GetDeviceMemoryPtr(tensor.Segment);
            var ptr2 = GetDeviceMemoryPtr(other.Segment);
            uint rowsA = tensor.RowCount, columnsA = tensor.ColumnCount, columnsB = other.Depth, rowsB = other.RowCount * other.ColumnCount, blockSize2 = columnsB * rowsB;
            float alpha = 1.0f, beta = 0.0f;
            var outputPtr = Provider.Allocate(tensor.ColumnCount * columnsB * tensor.Depth);
            var output = new CudaTensor3D(new CudaTensorSegment(outputPtr), tensor.Depth, columnsB, tensor.ColumnCount, this);

            var status = CudaBlasNativeMethods.cublasSgemmStridedBatched(Provider.Blas.CublasHandle,
                Operation.Transpose,
                Operation.NonTranspose,
                (int)columnsA,
                (int)columnsB,
                (int)rowsB,
                ref alpha,
                ptr.DevicePointer,
                (int)rowsA,
                tensor.MatrixSize,
                ptr2.DevicePointer,
                (int)rowsB,
                blockSize2,
                ref beta,
                outputPtr.DevicePointer,
                (int)columnsA,
                tensor.ColumnCount * columnsB,
                (int)tensor.Depth
            );
            if (status != CublasStatus.Success)
                throw new CudaBlasException(status);

            return output;
        }

        public override ITensorSegment[] MultiSoftmax(ArraySegment<ITensorSegment> segments)
        {
            var index = 0;
            var ret = new ITensorSegment[segments.Count];
            foreach (var segment in segments) {
                var (ptr, stride) = GetDeviceMemory(segment);
                ret[index++] = Softmax(ptr, stride);
            }
            return ret;
        }

        public override IMatrix[] MultiSoftmaxDerivative(ITensorSegment[] segments)
        {
            var index = 0;
            var ret = new IMatrix[segments.Length];
            foreach (var segment in segments) {
                var (ptr, stride) = GetDeviceMemory(segment);
                ret[index++] = SoftmaxDerivative(ptr, stride);
            }
            return ret;
        }

        public override ITensorSegment[] SoftmaxDerivativePerRow(IMatrix matrix, ITensorSegment[] rows)
        {
            var size = (int)rows[0].Size;
            var derivatives = MultiSoftmaxDerivative(rows);
            var segments = (IMatrixSegments)matrix;

            var ret = new ITensorSegment[matrix.RowCount];
            for (uint i = 0; i < matrix.RowCount; i++) {
                using var derivative = derivatives[i];
                var derivativePtr = GetDeviceMemoryPtr(derivative.Segment);
                var (ptr, stride) = GetDeviceMemory(segments.Row(i));

                float alpha = 1.0f, beta = 0f;
                var result = Provider.Allocate((uint)size);
                CudaBlasNativeMethods.cublasSgemv_v2(Provider.Blas.CublasHandle,
                    Operation.NonTranspose,
                    size,
                    size,
                    ref alpha,
                    derivativePtr.DevicePointer,
                    size,
                    ptr.DevicePointer,
                    (int)stride,
                    ref beta,
                    result.DevicePointer,
                    1
                ).CheckResult();
                ret[i] = new CudaTensorSegment(result);
            }

            return ret;
        }

        public override IVector Multiply(IMatrix matrix, IVector vector)
        {
            var (ptr, stride) = GetDeviceMemory(vector.Segment);
            var matrixPtr = GetDeviceMemoryPtr(matrix.Segment);
            var size = (int)ptr.Size;
            float alpha = 1.0f, beta = 0f;

            var result = Provider.Allocate((uint)size);
            CudaBlasNativeMethods.cublasSgemv_v2(Provider.Blas.CublasHandle,
                Operation.NonTranspose,
                size,
                size,
                ref alpha,
                matrixPtr.DevicePointer,
                size,
                ptr.DevicePointer,
                (int)stride,
                ref beta,
                result.DevicePointer,
                1
            ).CheckResult();
            return new CudaVector(new CudaTensorSegment(result), this);
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
        //        ret[i] = new CudaVector(new CudaTensorSegment(final), this);
        //    });
        //    //}

        //    return ret;
        //}
    }
}