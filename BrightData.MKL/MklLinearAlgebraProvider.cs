using BrightData.LinearAlgebra;
using MKLNET;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using BrightData.Helper;

// ReSharper disable BitwiseOperatorOnEnumWithoutFlags
namespace BrightData.MKL
{
    static class Helper
    {
        /// <summary>
        /// Returns an array from a tensor segment
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float[] GetLocalOrNewArray(this INumericSegment<float> segment)
        {
            var (array, offset, stride) = segment.GetUnderlyingArray();
            if (array is not null && stride == 1) {
                if (offset == 0)
                    return array;

                var ret = new float[segment.Size];
                Array.Copy(array, offset, ret, 0, segment.Size);
                return ret;
            }

            return segment.ToNewArray();
        }
    }

    /// <summary>
    /// Linear algebra provider that uses the Intel MKL library
    /// </summary>
    public unsafe class MklLinearAlgebraProvider : LinearAlgebraProvider<float>
    {
        /// <inheritdoc />
        public MklLinearAlgebraProvider(BrightDataContext context) : base(context)
        {
            Vml.SetMode(VmlMode.EP | VmlMode.FTZDAZ_ON | VmlMode.ERRMODE_EXCEPT);
        }

        /// <inheritdoc />
        public override string ProviderName => "mkl";

        /// <inheritdoc />
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)]
        public override Type VectorType { get; } = typeof(MklVector);

        /// <inheritdoc />
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)]
        public override Type MatrixType { get; } = typeof(MklMatrix);

        /// <inheritdoc />
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)]
        public override Type Tensor3DType { get; } = typeof(MklTensor3D);

        /// <inheritdoc />
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)]
        public override Type Tensor4DType { get; } = typeof(MklTensor4D);

        /// <inheritdoc />
        public override IVector<float> CreateVector(INumericSegment<float> data) => new MklVector(data, this);

        /// <inheritdoc />
        public override IMatrix<float> CreateMatrix(uint rowCount, uint columnCount, INumericSegment<float>  data) => new MklMatrix(data, rowCount, columnCount, this);

        /// <inheritdoc />
        public override ITensor3D<float> CreateTensor3D(uint depth, uint rowCount, uint columnCount, INumericSegment<float>  data) => new MklTensor3D(data, depth, rowCount, columnCount, this);

        /// <inheritdoc />
        public override ITensor4D<float> CreateTensor4D(uint count, uint depth, uint rowCount, uint columnCount, INumericSegment<float>  data) => new MklTensor4D(data, count, depth, rowCount, columnCount, this);

        internal delegate void ApplyNewSizeCallback(float* a, float* b, float* r);
        internal INumericSegment<float> ApplyWithNewSize(INumericSegment<float> tensor, INumericSegment<float> tensor2, uint resultSize, bool initialiseToZero, ApplyNewSizeCallback callback)
        {
            var result = CreateSegment(resultSize, initialiseToZero);
            tensor.ApplyReadOnlySpans(tensor2, (a, b) => {
                fixed(float* p1 = a)
                fixed (float* p2 = b)
                fixed(float* r = result.GetUnderlyingArray().Array!) {
                    callback(p1, p2, r);
                }
            });
            return result;
        }

        INumericSegment<float> Clone(IReadOnlyNumericSegment<float> tensor, float coefficient)
        {
            var result = CreateSegment(tensor.Size, false);
            tensor.CopyTo(result);
            if (Math.Abs(coefficient - 1) > Math<float>.AlmostZero) {
                fixed (float* ptr = result.Contiguous!.ReadOnlySpan) {
                    Blas.Unsafe.scal((int)tensor.Size, coefficient, ptr, 1);
                }
            }
            return result;
        }

        delegate RT TraverseCallback<out RT>(int size, float* x, int incX);
        static RT Traverse<RT>(IReadOnlyNumericSegment<float> x, TraverseCallback<RT> callback)
        {
            if (x is INumericSegment<float> x2)
                return Traverse(x2, callback);
            return x.ApplyReadOnlySpan(a => {
                fixed(float* p1 = a) {
                    return callback(a.Length, p1, 1);
                }
            });
        }
        static RT Traverse<RT>(INumericSegment<float> x, TraverseCallback<RT> callback)
        {
            var (xa, xo, xs) = x.GetUnderlyingArray();
            if (xa is null)
                throw new Exception("Expected segments with underlying arrays");

            fixed(float* p1 = xa) {
                return callback((int)x.Size, p1 + xo, (int)xs);
            }
        }

        delegate RT DoubleTraverseCallback<out RT>(int size, float* x, int incX, float* y, int incY);
        static RT Traverse<RT>(IReadOnlyNumericSegment<float> x, IReadOnlyNumericSegment<float> y, DoubleTraverseCallback<RT> callback)
        {
            if (x is INumericSegment<float> x2 && y is INumericSegment<float> y2)
                return Traverse(x2, y2, callback);
            return x.ReduceReadOnlySpans(y, (a, b) => {
                fixed(float* p1 = a)
                fixed (float* p2 = b) {
                    return callback(a.Length, p1, 1, p2, 1);
                }
            });
        }
        static RT Traverse<RT>(INumericSegment<float> x, INumericSegment<float> y, DoubleTraverseCallback<RT> callback)
        {
            var (xa, xo, xs) = x.GetUnderlyingArray();
            var (ya, yo, ys) = y.GetUnderlyingArray();
            if (xa is null || ya is null)
                throw new Exception("Expected segments with underlying arrays");

            fixed(float* p1 = xa)
            fixed (float* p2 = ya) {
                return callback((int)x.Size, p1 + xo, (int)xs, p2 + yo, (int)ys);
            }
        }
        delegate void DoubleZipCallback(int size, float* x, float* y, float* r);
        INumericSegment<float> Zip(IReadOnlyNumericSegment<float> x, IReadOnlyNumericSegment<float> y, bool initialiseToZero, DoubleZipCallback callback)
        {
            var result = CreateSegment(x.Size, initialiseToZero);
            x.ApplyReadOnlySpans(y, (a, b) => {
                fixed(float* p1 = a)
                fixed (float* p2 = b)
                fixed(float* r = result.GetUnderlyingArray().Array!) {
                    callback(a.Length, p1, p2, r);
                }
            });
            return result;
        }
        internal delegate void CopyCallback(int size, float* x, float* r);
        internal INumericSegment<float> Copy(IReadOnlyNumericSegment<float> tensor, bool initialiseToZero, CopyCallback callback)
        {
            var result = CreateSegment(tensor.Size, initialiseToZero);
            tensor.ApplyReadOnlySpan(a => {
                fixed(float* ap = a)
                fixed(float* r = result.GetUnderlyingArray().Array!) {
                    callback(a.Length, ap, r);
                }
            });
            return result;
        }

        /// <inheritdoc />
        public override float DotProduct(IReadOnlyNumericSegment<float> tensor, IReadOnlyNumericSegment<float> tensor2) => Traverse(tensor, tensor2, Blas.Unsafe.dot);

        /// <inheritdoc />
        public override INumericSegment<float> Add(IReadOnlyNumericSegment<float> tensor, IReadOnlyNumericSegment<float> tensor2) => Zip(tensor, tensor2, false, Vml.Unsafe.Add);

        /// <inheritdoc />
        public override INumericSegment<float> Abs(IReadOnlyNumericSegment<float> tensor) => Copy(tensor, false, Vml.Unsafe.Abs);

        /// <inheritdoc />
        public override INumericSegment<float> Add(IReadOnlyNumericSegment<float> tensor, IReadOnlyNumericSegment<float> tensor2, float coefficient1, float coefficient2)
        {
            var ret = Clone(tensor);
            AddInPlace(ret, tensor2, coefficient1, coefficient2);
            return ret;
        }

        /// <inheritdoc />
        public override INumericSegment<float> Multiply(IReadOnlyNumericSegment<float> target, float scalar) => Clone(target, scalar);

        /// <inheritdoc />
        public override void MultiplyInPlace(INumericSegment<float> target, float scalar) => Traverse(target, (n, a, i) => {
            Blas.Unsafe.scal(n, scalar, a, i);
            return true;
        });

        /// <inheritdoc />
        public override float L1Norm(IReadOnlyNumericSegment<float> segment) => Traverse(segment, Blas.Unsafe.asum);

        /// <inheritdoc />
        public override float L2Norm(IReadOnlyNumericSegment<float> segment) => Traverse(segment, Blas.Unsafe.nrm2);

        /// <inheritdoc />
        public override INumericSegment<float> Exp(IReadOnlyNumericSegment<float> tensor) => Copy(tensor, false, Vml.Unsafe.Exp);

        /// <inheritdoc />
        public override INumericSegment<float> Tanh(IReadOnlyNumericSegment<float> tensor) => Copy(tensor, false, Vml.Unsafe.Tanh);

        /// <inheritdoc />
        public override INumericSegment<float> PointwiseMultiply(IReadOnlyNumericSegment<float> tensor1, IReadOnlyNumericSegment<float> tensor2) => Zip(tensor1, tensor2, false, Vml.Unsafe.Mul);

        /// <inheritdoc />
        public override INumericSegment<float> PointwiseDivide(IReadOnlyNumericSegment<float> tensor1, IReadOnlyNumericSegment<float> tensor2) => Zip(tensor1, tensor2, false, Vml.Unsafe.Div);

        /// <inheritdoc />
        public override INumericSegment<float> Log(IReadOnlyNumericSegment<float> tensor) => Copy(tensor, false, Vml.Unsafe.Ln);

        /// <inheritdoc />
        public override INumericSegment<float> Subtract(IReadOnlyNumericSegment<float> tensor1, IReadOnlyNumericSegment<float> tensor2) => Zip(tensor1, tensor2, false, Vml.Unsafe.Sub);

        /// <inheritdoc />
        public override INumericSegment<float> Squared(IReadOnlyNumericSegment<float> tensor) => Copy(tensor, false, Vml.Unsafe.Sqr);

        /// <inheritdoc />
        public override INumericSegment<float> Add(IReadOnlyNumericSegment<float> tensor, float scalar) => Copy(tensor, false, (n, a, r) => Vml.Unsafe.LinearFrac(n, a, a, 1, scalar, 0, 1, r));

        /// <inheritdoc />
        public override void AddInPlace(INumericSegment<float> target, IReadOnlyNumericSegment<float> other)
        {
            using var ret = Zip(target, other, false, Vml.Unsafe.Add);
            ret.CopyTo(target);
        }

        /// <inheritdoc />
        public override void AddInPlace(INumericSegment<float> target, IReadOnlyNumericSegment<float> other, float coefficient1, float coefficient2)
        {
            Traverse(target, other, (n, x, incX, y, incY) => {
                Blas.Unsafe.axpby(n, coefficient2, y, incY, coefficient1, x, incX);
                return true;
            });
        }

        /// <inheritdoc />
        public override void AddInPlace(INumericSegment<float> target, float scalar)
        {
            using var ret = Copy(target, false, (n, a, r) => Vml.Unsafe.LinearFrac(n, a, a, 1, scalar, 0, 1, r));
            ret.CopyTo(target);
        }

        /// <inheritdoc />
        public override void PointwiseDivideInPlace(INumericSegment<float> target, IReadOnlyNumericSegment<float> other)
        {
            using var temp = PointwiseDivide(target, other);
            temp.CopyTo(target);
        }

        /// <inheritdoc />
        public override void PointwiseMultiplyInPlace(INumericSegment<float> target, IReadOnlyNumericSegment<float> other)
        {
            using var temp = PointwiseMultiply(target, other);
            temp.CopyTo(target);
        }

        /// <inheritdoc />
        public override INumericSegment<float> Pow(IReadOnlyNumericSegment<float> tensor, float power) => Copy(tensor, false, (n, a, r) => Vml.Unsafe.Powx(n, a, power, r));
        
        /// <inheritdoc />
        public override INumericSegment<float> Subtract(IReadOnlyNumericSegment<float> tensor1, IReadOnlyNumericSegment<float> tensor2, float coefficient1, float coefficient2)
        {
            var ret = Clone(tensor1);
            SubtractInPlace(ret, tensor2, coefficient1, coefficient2);
            return ret;
        }

        /// <inheritdoc />
        public override void SubtractInPlace(INumericSegment<float> target, IReadOnlyNumericSegment<float> other)
        {
            using var temp = Subtract(target, other);
            temp.CopyTo(target);
        }

        /// <inheritdoc />
        public override void SubtractInPlace(INumericSegment<float>  target, IReadOnlyNumericSegment<float>  other, float coefficient1, float coefficient2)
        {
            Traverse(target, other, (n, x, incX, y, incY) => {
                Blas.Unsafe.axpby(n, coefficient2 * -1, y, incY, coefficient1, x, incX);
                return true;
            });
        }

        ///// <inheritdoc />
        //public override ITensor3D Multiply(ITensor3D tensor, IMatrix other)
        //{
        //    // TODO: gemm_batch_strided
        //    return base.Multiply(tensor, other);
        //}

        ///// <inheritdoc />
        //public override ITensor3D TransposeFirstAndMultiply(ITensor3D tensor, IMatrix other)
        //{
        //    // TODO: gemm_batch_strided
        //    return base.TransposeFirstAndMultiply(tensor, other);
        //}
    }
}
