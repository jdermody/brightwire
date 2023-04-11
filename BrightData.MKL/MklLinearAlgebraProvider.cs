using BrightData.LinearAlgebra;
using MKLNET;

// ReSharper disable BitwiseOperatorOnEnumWithoutFlags
namespace BrightData.MKL
{
    /// <summary>
    /// Linear algebra provider that uses the Intel MKL library
    /// </summary>
    public class MklLinearAlgebraProvider : LinearAlgebraProvider
    {
        /// <inheritdoc />
        public MklLinearAlgebraProvider(BrightDataContext context) : base(context)
        {
            Vml.SetMode(VmlMode.EP | VmlMode.FTZDAZ_ON | VmlMode.ERRMODE_EXCEPT);
        }

        /// <inheritdoc />
        public override string ProviderName => "mkl";

        /// <inheritdoc />
        public override Type VectorType { get; } = typeof(MklVector);

        /// <inheritdoc />
        public override Type MatrixType { get; } = typeof(MklMatrix);

        /// <inheritdoc />
        public override Type Tensor3DType { get; } = typeof(MklTensor3D);

        /// <inheritdoc />
        public override Type Tensor4DType { get; } = typeof(MklTensor4D);

        /// <inheritdoc />
        public override IVector CreateVector(ITensorSegment data) => new MklVector(data, this);

        /// <inheritdoc />
        public override IMatrix CreateMatrix(uint rowCount, uint columnCount, ITensorSegment data) => new MklMatrix(data, rowCount, columnCount, this);

        /// <inheritdoc />
        public override ITensor3D CreateTensor3D(uint depth, uint rowCount, uint columnCount, ITensorSegment data) => new MklTensor3D(data, depth, rowCount, columnCount, this);

        /// <inheritdoc />
        public override ITensor4D CreateTensor4D(uint count, uint depth, uint rowCount, uint columnCount, ITensorSegment data) => new MklTensor4D(data, count, depth, rowCount, columnCount, this);

        ITensorSegment Apply(ITensorSegment tensor, ITensorSegment tensor2, bool initialiseToZero, Action<int, float[], float[], float[]> mkl)
        {
            var size = GetSize(tensor, tensor2);
            var result = CreateSegment(size, initialiseToZero);
            mkl((int)size, tensor.GetLocalOrNewArray(), tensor2.GetLocalOrNewArray(), result.GetArrayIfEasilyAvailable()!);
            return result;
        }
        ITensorSegment ApplyUnderlying(ITensorSegment tensor, ITensorSegment tensor2, bool initialiseToZero, Action<int, float[], int, int, float[], int, int, float[], int, int> mkl)
        {
            var size = GetSize(tensor, tensor2);
            var underlying = tensor.GetUnderlyingArray();
            var underlying2 = tensor2.GetUnderlyingArray();
            var result = CreateSegment(size, initialiseToZero);
            mkl((int)size, underlying.Array!, (int)underlying.Offset, (int)underlying.Stride, underlying2.Array!, (int)underlying2.Offset, (int)underlying2.Stride, result.GetArrayIfEasilyAvailable()!, 0, 1);
            return result;
        }

        ITensorSegment ApplyWithNewSize(ITensorSegment tensor, ITensorSegment tensor2, uint resultSize, bool initialiseToZero, Action<float[], float[], float[]> mkl)
        {
            var result = CreateSegment(resultSize, initialiseToZero);
            mkl(tensor.GetLocalOrNewArray(), tensor2.GetLocalOrNewArray(), result.GetArrayIfEasilyAvailable()!);
            return result;
        }

        ITensorSegment Apply(ITensorSegment tensor, bool initialiseToZero, Action<int, float[], float[]> mkl)
        {
            var result = CreateSegment(tensor.Size, initialiseToZero);
            mkl((int)tensor.Size, tensor.GetLocalOrNewArray(), result.GetArrayIfEasilyAvailable()!);
            return result;
        }
        ITensorSegment ApplyUnderlying(ITensorSegment tensor, bool initialiseToZero, Action<int, float[], int, int, float[], int, int> mkl)
        {
            var result = CreateSegment(tensor.Size, initialiseToZero);
            var underlying = tensor.GetUnderlyingArray();
            mkl((int)tensor.Size, underlying.Array!, (int)underlying.Offset, (int)underlying.Stride, result.GetArrayIfEasilyAvailable()!, 0, 1);
            return result;
        }

        static float Apply(ITensorSegment tensor, Func<int, float[], float> mkl)
        {
            return mkl((int)tensor.Size, tensor.GetLocalOrNewArray());
        }
        static float ApplyUnderlying(ITensorSegment tensor, Func<int, float[], int, int, float> mkl)
        {
            var underlying = tensor.GetUnderlyingArray();
            return mkl((int)tensor.Size, underlying.Array!, (int)underlying.Offset, (int)underlying.Stride);
        }

        static float Apply(ITensorSegment tensor, ITensorSegment tensor2, Func<int, float[], float[], float> mkl)
        {
            var size = GetSize(tensor, tensor2);
            return mkl((int)size, tensor.GetLocalOrNewArray(), tensor2.GetLocalOrNewArray());
        }
        static float ApplyUnderlying(ITensorSegment tensor, ITensorSegment tensor2, Func<int, float[], int, int, float[], int, int, float> mkl)
        {
            var size = GetSize(tensor, tensor2);
            var underlying = tensor.GetUnderlyingArray();
            var underlying2 = tensor2.GetUnderlyingArray();
            return mkl((int)size, underlying.Array!, (int)underlying.Offset, (int)underlying.Stride, underlying2.Array!, (int)underlying2.Offset, (int)underlying2.Stride);
        }

        static void Apply(ITensorSegment tensor, ITensorSegment tensor2, Action<int, float[], float[]> mkl)
        {
            var size = GetSize(tensor, tensor2);
            mkl((int)size, tensor.GetLocalOrNewArray(), tensor2.GetLocalOrNewArray());
        }
        static void ApplyUnderlying(ITensorSegment tensor, ITensorSegment tensor2, Action<int, float[], int, int, float[], int, int> mkl)
        {
            var size = GetSize(tensor, tensor2);
            var underlying = tensor.GetUnderlyingArray();
            var underlying2 = tensor2.GetUnderlyingArray();
            mkl((int)size, underlying.Array!, (int)underlying.Offset, (int)underlying.Stride, underlying2.Array!, (int)underlying2.Offset, (int)underlying2.Stride);
        }

        static void ApplyUnderlying(ITensorSegment tensor, Action<int, float[], int, int> mkl)
        {
            var underlying = tensor.GetUnderlyingArray();
            mkl((int)tensor.Size, underlying.Array!, (int)underlying.Offset, (int)underlying.Stride);
        }

        ITensorSegment Clone(ITensorSegment tensor, float coefficient)
        {
            var result = CreateSegment(tensor.Size, false);
            tensor.CopyTo(result);
            Blas.scal((int)tensor.Size, coefficient, result.GetArrayIfEasilyAvailable()!, 0, 1);
            return result;
        }

        /// <inheritdoc />
        public override float DotProduct(ITensorSegment tensor, ITensorSegment tensor2) => ApplyUnderlying(tensor, tensor2, Blas.dot);

        /// <inheritdoc />
        public override ITensorSegment Add(ITensorSegment tensor, ITensorSegment tensor2) => ApplyUnderlying(tensor, tensor2, false, Vml.Add);

        /// <inheritdoc />
        public override ITensorSegment Abs(ITensorSegment tensor) => ApplyUnderlying(tensor, false, Vml.Abs);

        /// <inheritdoc />
        public override ITensorSegment Add(ITensorSegment tensor, ITensorSegment tensor2, float coefficient1, float coefficient2)
        {
            var ret = Clone(tensor);
            AddInPlace(ret, tensor2, coefficient1, coefficient2);
            return ret;
        }

        /// <inheritdoc />
        public override ITensorSegment Multiply(ITensorSegment target, float scalar) => Clone(target, scalar);

        /// <inheritdoc />
        public override void MultiplyInPlace(ITensorSegment target, float scalar) => ApplyUnderlying(target, (n, a, o, s) => Blas.scal(n, scalar, a, o, s));

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
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
                s.GetArrayIfEasilyAvailable()!,
                u.GetArrayIfEasilyAvailable()!,
                rows,
                vt.GetArrayIfEasilyAvailable()!,
                cols,
                rWork.GetArrayIfEasilyAvailable()!
            );
            if (ret != 0)
                throw new Exception("Failed to compute SVD");

            return (
                CreateMatrix((uint)rows, (uint)rows, u),
                CreateVector(s),
                CreateMatrix((uint)cols, (uint)cols, vt)
            );
        }

        /// <inheritdoc />
        public override float L1Norm(ITensorSegment segment) => ApplyUnderlying(segment, Blas.asum);

        /// <inheritdoc />
        public override float L2Norm(ITensorSegment segment) => ApplyUnderlying(segment, Blas.nrm2);

        /// <inheritdoc />
        public override ITensorSegment Exp(ITensorSegment tensor) => ApplyUnderlying(tensor, false, Vml.Exp);

        /// <inheritdoc />
        public override ITensorSegment Tanh(ITensorSegment tensor) => ApplyUnderlying(tensor, false, Vml.Tanh);

        /// <inheritdoc />
        public override ITensorSegment PointwiseMultiply(ITensorSegment tensor1, ITensorSegment tensor2) => ApplyUnderlying(tensor1, tensor2, false, Vml.Mul);

        /// <inheritdoc />
        public override ITensorSegment PointwiseDivide(ITensorSegment tensor1, ITensorSegment tensor2) => ApplyUnderlying(tensor1, tensor2, false, Vml.Div);

        /// <inheritdoc />
        public override ITensorSegment Log(ITensorSegment tensor) => ApplyUnderlying(tensor, false, Vml.Ln);

        /// <inheritdoc />
        public override ITensorSegment Subtract(ITensorSegment tensor1, ITensorSegment tensor2) => ApplyUnderlying(tensor1, tensor2, false, Vml.Sub);

        /// <inheritdoc />
        public override ITensorSegment Squared(ITensorSegment tensor) => ApplyUnderlying(tensor, false, Vml.Sqr);

        /// <inheritdoc />
        public override ITensorSegment Add(ITensorSegment tensor, float scalar) => Apply(tensor, false, (n, a, r) => Vml.LinearFrac(n, a, a, 1, scalar, 0, 1, r));

        /// <inheritdoc />
        public override void AddInPlace(ITensorSegment target, ITensorSegment other)
        {
            using var ret = ApplyUnderlying(target, other, false, Vml.Add);
            ret.CopyTo(target);
        }

        /// <inheritdoc />
        public override void AddInPlace(ITensorSegment target, ITensorSegment other, float coefficient1, float coefficient2)
        {
            ApplyUnderlying(target, other, (n, x, xo, xs, y, yo, ys) => Blas.axpby(n, coefficient2, y, yo, ys, coefficient1, x, xo, xs));
        }

        /// <inheritdoc />
        public override void AddInPlace(ITensorSegment target, float scalar)
        {
            using var ret = Apply(target, false, (n, a, r) => Vml.LinearFrac(n, a, a, 1, scalar, 0, 1, r));
            ret.CopyTo(target);
        }

        /// <inheritdoc />
        public override void PointwiseDivideInPlace(ITensorSegment target, ITensorSegment other)
        {
            using var temp = PointwiseDivide(target, other);
            temp.CopyTo(target);
        }

        /// <inheritdoc />
        public override void PointwiseMultiplyInPlace(ITensorSegment target, ITensorSegment other)
        {
            using var temp = PointwiseMultiply(target, other);
            temp.CopyTo(target);
        }

        /// <inheritdoc />
        public override ITensorSegment Pow(ITensorSegment tensor, float power) => ApplyUnderlying(tensor, false, (n, x, xo, xs, r, ro, rs) => Vml.Powx(n, x, xo, xs, power, r, ro, rs));

        /// <inheritdoc />
        public override ITensorSegment Subtract(ITensorSegment tensor1, ITensorSegment tensor2, float coefficient1, float coefficient2)
        {
            var ret = Clone(tensor1);
            SubtractInPlace(ret, tensor2, coefficient1, coefficient2);
            return ret;
        }

        /// <inheritdoc />
        public override void SubtractInPlace(ITensorSegment target, ITensorSegment other)
        {
            using var temp = Subtract(target, other);
            temp.CopyTo(target);
        }

        /// <inheritdoc />
        public override void SubtractInPlace(ITensorSegment target, ITensorSegment other, float coefficient1, float coefficient2)
        {
            ApplyUnderlying(target, other, (n, x, xo, xs, y, yo, ys) => Blas.axpby(n, coefficient2 * -1, y, yo, ys, coefficient1, x, xo, xs));
        }

        /// <inheritdoc />
        public override ITensor3D Multiply(ITensor3D tensor, IMatrix other)
        {
            // TODO: gemm_batch_strided
            return base.Multiply(tensor, other);
        }

        /// <inheritdoc />
        public override ITensor3D TransposeFirstAndMultiply(ITensor3D tensor, IMatrix other)
        {
            // TODO: gemm_batch_strided
            return base.TransposeFirstAndMultiply(tensor, other);
        }
    }
}
