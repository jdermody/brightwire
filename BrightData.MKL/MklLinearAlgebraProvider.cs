using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using BrightData.Helper;
using BrightData.LinearAlgebra;
using Microsoft.Toolkit.HighPerformance.Buffers;
using MKLNET;

namespace BrightData.MKL
{
    public class MklLinearAlgebraProvider : LinearAlgebraProvider
    {
        public MklLinearAlgebraProvider(BrightDataContext context) : base(context)
        {
            Vml.SetMode(VmlMode.EP | VmlMode.FTZDAZ_ON | VmlMode.ERRMODE_EXCEPT);
        }

        public override string ProviderName => "mkl";
        public override Type VectorType { get; } = typeof(MklVector);
        public override Type MatrixType { get; } = typeof(MklMatrix);
        public override Type Tensor3DType { get; } = typeof(MklTensor3D);
        public override Type Tensor4DType { get; } = typeof(MklTensor4D);

        public override IVector CreateVector(ITensorSegment data) => new MklVector(data, this);
        public override IMatrix CreateMatrix(uint rowCount, uint columnCount, ITensorSegment data) => new MklMatrix(data, rowCount, columnCount, this);
        public override ITensor3D CreateTensor3D(uint depth, uint rowCount, uint columnCount, ITensorSegment data) => new MklTensor3D(data, depth, rowCount, columnCount, this);
        public override ITensor4D CreateTensor4D(uint count, uint depth, uint rowCount, uint columnCount, ITensorSegment data) => new MklTensor4D(data, count, depth, rowCount, columnCount, this);

        ITensorSegment Apply(ITensorSegment tensor, ITensorSegment tensor2, bool initialiseToZero, Action<int, float[], float[], float[]> mkl)
        {
            var size = GetSize(tensor, tensor2);
            var result = CreateSegment(size, initialiseToZero);
            mkl((int)size, tensor.GetLocalOrNewArray(), tensor2.GetLocalOrNewArray(), result.GetArrayForLocalUseOnly()!);
            return result;
        }
        ITensorSegment ApplyUnderlying(ITensorSegment tensor, ITensorSegment tensor2, bool initialiseToZero, Action<int, float[], int, int, float[], int, int, float[], int, int> mkl)
        {
            var size = GetSize(tensor, tensor2);
            var underlying = tensor.GetUnderlyingArray();
            var underlying2 = tensor2.GetUnderlyingArray();
            var result = CreateSegment(size, initialiseToZero);
            mkl((int)size, underlying.Array!, (int)underlying.Offset, (int)underlying.Stride, underlying2.Array!, (int)underlying2.Offset, (int)underlying2.Stride, result.GetArrayForLocalUseOnly()!, 0, 1);
            return result;
        }

        ITensorSegment ApplyWithNewSize(ITensorSegment tensor, ITensorSegment tensor2, uint resultSize, bool initialiseToZero, Action<float[], float[], float[]> mkl)
        {
            var result = CreateSegment(resultSize, initialiseToZero);
            mkl(tensor.GetLocalOrNewArray(), tensor2.GetLocalOrNewArray(), result.GetArrayForLocalUseOnly()!);
            return result;
        }

        ITensorSegment Apply(ITensorSegment tensor, bool initialiseToZero, Action<int, float[], float[]> mkl)
        {
            var result = CreateSegment(tensor.Size, initialiseToZero);
            mkl((int)tensor.Size, tensor.GetLocalOrNewArray(), result.GetArrayForLocalUseOnly()!);
            return result;
        }
        ITensorSegment ApplyUnderlying(ITensorSegment tensor, bool initialiseToZero, Action<int, float[], int, int, float[], int, int> mkl)
        {
            var result = CreateSegment(tensor.Size, initialiseToZero);
            var underlying = tensor.GetUnderlyingArray();
            mkl((int)tensor.Size, underlying.Array!, (int)underlying.Offset, (int)underlying.Stride, result.GetArrayForLocalUseOnly()!, 0, 1);
            return result;
        }

        float Apply(ITensorSegment tensor, Func<int, float[], float> mkl)
        {
            return mkl((int)tensor.Size, tensor.GetLocalOrNewArray());
        }
        float ApplyUnderlying(ITensorSegment tensor, Func<int, float[], int, int, float> mkl)
        {
            var underlying = tensor.GetUnderlyingArray();
            return mkl((int)tensor.Size, underlying.Array!, (int)underlying.Offset, (int)underlying.Stride);
        }

        float Apply(ITensorSegment tensor, ITensorSegment tensor2, Func<int, float[], float[], float> mkl)
        {
            var size = GetSize(tensor, tensor2);
            return mkl((int)size, tensor.GetLocalOrNewArray(), tensor2.GetLocalOrNewArray());
        }
        float ApplyUnderlying(ITensorSegment tensor, ITensorSegment tensor2, Func<int, float[], int, int, float[], int, int, float> mkl)
        {
            var size = GetSize(tensor, tensor2);
            var underlying = tensor.GetUnderlyingArray();
            var underlying2 = tensor2.GetUnderlyingArray();
            return mkl((int)size, underlying.Array!, (int)underlying.Offset, (int)underlying.Stride, underlying2.Array!, (int)underlying2.Offset, (int)underlying2.Stride);
        }

        void Apply(ITensorSegment tensor, ITensorSegment tensor2, Action<int, float[], float[]> mkl)
        {
            var size = GetSize(tensor, tensor2);
            mkl((int)size, tensor.GetLocalOrNewArray(), tensor2.GetLocalOrNewArray());
        }
        void ApplyUnderlying(ITensorSegment tensor, ITensorSegment tensor2, Action<int, float[], int, int, float[], int, int> mkl)
        {
            var size = GetSize(tensor, tensor2);
            var underlying = tensor.GetUnderlyingArray();
            var underlying2 = tensor2.GetUnderlyingArray();
            mkl((int)size, underlying.Array!, (int)underlying.Offset, (int)underlying.Stride, underlying2.Array!, (int)underlying2.Offset, (int)underlying2.Stride);
        }

        void ApplyUnderlying(ITensorSegment tensor, Action<int, float[], int, int> mkl)
        {
            var underlying = tensor.GetUnderlyingArray();
            mkl((int)tensor.Size, underlying.Array!, (int)underlying.Offset, (int)underlying.Stride);
        }

        ITensorSegment Clone(ITensorSegment tensor, float coefficient)
        {
            var result = CreateSegment(tensor.Size, false);
            tensor.CopyTo(result);
            Blas.scal((int)tensor.Size, coefficient, result.GetArrayForLocalUseOnly()!, 0, 1);
            return result;
        }
        public override float DotProduct(ITensorSegment tensor, ITensorSegment tensor2) => ApplyUnderlying(tensor, tensor2, Blas.dot);
        public override ITensorSegment Add(ITensorSegment tensor, ITensorSegment tensor2) => ApplyUnderlying(tensor, tensor2, false, Vml.Add);
        public override ITensorSegment Abs(ITensorSegment tensor) => ApplyUnderlying(tensor, false, Vml.Abs);
        public override ITensorSegment Add(ITensorSegment tensor, ITensorSegment tensor2, float coefficient1, float coefficient2)
        {
            var ret = Clone(tensor);
            AddInPlace(ret, tensor2, coefficient1, coefficient2);
            return ret;
        }

        public override ITensorSegment Multiply(ITensorSegment target, float scalar) => Clone(target, scalar);
        public override void MultiplyInPlace(ITensorSegment target, float scalar) => ApplyUnderlying(target, (n, a, o, s) => Blas.scal(n, scalar, a, o, s));
        public override IMatrix Multiply(IMatrix matrix, IMatrix other)
        {
            int rowsA = (int)matrix.RowCount, 
                columnsARowsB = (int)matrix.ColumnCount, 
                columnsB = (int)other.ColumnCount
            ;
            var ret = ApplyWithNewSize(matrix.Segment, other.Segment, matrix.RowCount * other.ColumnCount, false, (a, b, r) => {
                Blas.gemm(
                    Layout.ColMajor,
                    Trans.No,
                    Trans.No,
                    rowsA,
                    columnsB,
                    columnsARowsB,
                    1f,
                    a,
                    rowsA,
                    b,
                    columnsARowsB,
                    0f,
                    r,
                    rowsA
                );
            });
            return CreateMatrix(matrix.RowCount, other.ColumnCount, ret);
        }

        public override IMatrix Transpose(IMatrix matrix)
        {
            var rows = (int)matrix.RowCount;
            var cols = (int)matrix.ColumnCount;

            var ret = Apply(matrix.Segment, false, (size, a, r) => {
                System.Buffer.BlockCopy(a, 0, r, 0, size * sizeof(float));
                Blas.imatcopy(
                    LayoutChar.ColMajor, 
                    TransChar.Yes,
                    rows,
                    cols,
                    1f, 
                    r,
                    rows,
                    cols
                );
            });
            return CreateMatrix(matrix.ColumnCount, matrix.RowCount, ret);
        }

        public override IMatrix TransposeFirstAndMultiply(IMatrix matrix, IMatrix other)
        {
            int rowsA = (int)matrix.RowCount, 
                columnsA = (int)matrix.ColumnCount, 
                columnsB = (int)other.ColumnCount, 
                rowsB = (int)other.RowCount
            ;
            var ret = ApplyWithNewSize(matrix.Segment, other.Segment, matrix.ColumnCount * other.ColumnCount, false, (a, b, r) => {
                Blas.gemm(
                    Layout.ColMajor,
                    Trans.Yes,
                    Trans.No,
                    columnsA,
                    columnsB,
                    rowsB,
                    1f,
                    a,
                    rowsA,
                    b,
                    rowsB,
                    0f,
                    r,
                    columnsA
                );
            });
            return CreateMatrix(matrix.ColumnCount, other.ColumnCount, ret);
        }

        public override IMatrix TransposeSecondAndMultiply(IMatrix matrix, IMatrix other)
        {
            int rowsA = (int)matrix.RowCount, 
                columnsARowsB = (int)matrix.ColumnCount, 
                rowsB = (int)other.RowCount
            ;
            var ret = ApplyWithNewSize(matrix.Segment, other.Segment, matrix.RowCount * other.RowCount, false, (a, b, r) => {
                Blas.gemm(
                    Layout.ColMajor,
                    Trans.No,
                    Trans.Yes,
                    rowsA,
                    rowsB,
                    columnsARowsB,
                    1f,
                    a,
                    rowsA,
                    b,
                    rowsB,
                    0f,
                    r,
                    rowsA
                );
            });
            return CreateMatrix(matrix.RowCount, other.RowCount, ret);
        }

        public override (IMatrix U, IVector S, IMatrix VT) Svd(IMatrix matrix)
        {
            var rows = (int)matrix.RowCount;
            var cols = (int)matrix.ColumnCount;
            var mn = Math.Min(rows, cols);
            var buffer = matrix.Segment.GetLocalOrNewArray();

            var s = CreateSegment((uint)mn, false);
            var u = CreateSegment((uint)(rows * rows), false);
            var vt = CreateSegment((uint)(cols * cols), false);
            using var rWork = CreateSegment((uint)mn, false);

            var ret = Lapack.gesvd(
                Layout.ColMajor,
                'A',
                'A',
                rows,
                cols,
                buffer,
                rows,
                s.GetArrayForLocalUseOnly()!,
                u.GetArrayForLocalUseOnly()!,
                rows,
                vt.GetArrayForLocalUseOnly()!,
                cols,
                rWork.GetArrayForLocalUseOnly()!
            );
            return (
                CreateMatrix((uint)rows, (uint)rows, u),
                CreateVector(s),
                CreateMatrix((uint)cols, (uint)cols, vt)
            );
        }

        public override float L1Norm(ITensorSegment segment) => ApplyUnderlying(segment, Blas.asum);
        public override float L2Norm(ITensorSegment segment) => ApplyUnderlying(segment, Blas.nrm2);
        public override ITensorSegment Exp(ITensorSegment tensor) => ApplyUnderlying(tensor, false, Vml.Exp);
        public override ITensorSegment Tanh(ITensorSegment tensor) => ApplyUnderlying(tensor, false, Vml.Tanh);
        public override ITensorSegment PointwiseMultiply(ITensorSegment tensor1, ITensorSegment tensor2) => ApplyUnderlying(tensor1, tensor2, false, Vml.Mul);
        public override ITensorSegment PointwiseDivide(ITensorSegment tensor1, ITensorSegment tensor2) => ApplyUnderlying(tensor1, tensor2, false, Vml.Div);

        public override ITensorSegment Sqrt(ITensorSegment tensor)
        {
            // need to adjust zeros
            return base.Sqrt(tensor);
        }

        public override ITensorSegment Log(ITensorSegment tensor) => ApplyUnderlying(tensor, false, Vml.Ln);
        public override ITensorSegment Subtract(ITensorSegment tensor1, ITensorSegment tensor2) => ApplyUnderlying(tensor1, tensor2, false, Vml.Sub);
        public override ITensorSegment Squared(ITensorSegment tensor) => ApplyUnderlying(tensor, false, Vml.Sqr);

        public override ITensorSegment Add(ITensorSegment tensor, float scalar) => Apply(tensor, false, (n, a, r) => Vml.LinearFrac(n, a, a, 1, scalar, 0, 1, r));

        public override void AddInPlace(ITensorSegment target, ITensorSegment other)
        {
            using var ret = ApplyUnderlying(target, other, false, Vml.Add);
            ret.CopyTo(target);
        }

        public override void AddInPlace(ITensorSegment target, ITensorSegment other, float coefficient1, float coefficient2)
        {
            ApplyUnderlying(target, other, (n, x, xo, xs, y, yo, ys) => Blas.axpby(n, coefficient2, y, yo, ys, coefficient1, x, xo, xs));
        }

        public override void AddInPlace(ITensorSegment target, float scalar)
        {
            using var ret = Apply(target, false, (n, a, r) => Vml.LinearFrac(n, a, a, 1, scalar, 0, 1, r));
            ret.CopyTo(target);
        }

        public override void PointwiseDivideInPlace(ITensorSegment target, ITensorSegment other)
        {
            using var temp = PointwiseDivide(target, other);
            temp.CopyTo(target);
        }

        public override void PointwiseMultiplyInPlace(ITensorSegment target, ITensorSegment other)
        {
            using var temp = PointwiseMultiply(target, other);
            temp.CopyTo(target);
        }

        public override ITensorSegment Pow(ITensorSegment tensor, float power) => ApplyUnderlying(tensor, false, (n, x, xo, xs, r, ro, rs) => Vml.Powx(n, x, xo, xs, power, r, ro, rs));

        public override ITensorSegment Subtract(ITensorSegment tensor1, ITensorSegment tensor2, float coefficient1, float coefficient2)
        {
            var ret = Clone(tensor1);
            SubtractInPlace(ret, tensor2, coefficient1, coefficient2);
            return ret;
        }

        public override void SubtractInPlace(ITensorSegment target, ITensorSegment other)
        {
            using var temp = Subtract(target, other);
            temp.CopyTo(target);
        }

        public override void SubtractInPlace(ITensorSegment target, ITensorSegment other, float coefficient1, float coefficient2)
        {
            ApplyUnderlying(target, other, (n, x, xo, xs, y, yo, ys) => Blas.axpby(n, coefficient2 * -1, y, yo, ys, coefficient1, x, xo, xs));
        }

        //public override ITensorSegment2 Softmax(ITensorSegment2 tensor)
        //{
        //    var max = GetMax(tensor);
        //    using var diff = Add(tensor, max * -1);
        //    var softmax = Apply(diff, Vml.Exp);
        //    var sum = softmax.Sum();
        //    if (FloatMath.IsNotZero(sum))
        //        softmax.MultiplyInPlace(1f / sum);
        //    return softmax;
        //}
    }
}
