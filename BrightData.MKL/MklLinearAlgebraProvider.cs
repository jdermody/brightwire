using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using BrightData.LinearAlegbra2;
using Microsoft.Toolkit.HighPerformance.Buffers;
using MKLNET;

namespace BrightData.MKL
{
    public class MklLinearAlgebraProvider : LinearAlgebraProvider
    {
        public MklLinearAlgebraProvider(BrightDataContext context) : base(context)
        {
        }

        public override string Name => "mkl";
        public override Type VectorType { get; } = typeof(MklVector);
        public override Type MatrixType { get; } = typeof(MklMatrix);
        //public override Type Tensor3DType { get; } = typeof(Tensor3D2);
        //public override Type Tensor4DType { get; } = typeof(Tensor4D2);

        public override IVector CreateVector(ITensorSegment2 data) => new MklVector(data, this);
        public override IMatrix CreateMatrix(uint rowCount, uint columnCount, ITensorSegment2 data) => new MklMatrix(data, rowCount, columnCount, this);

        public override float DotProduct(ITensorSegment2 tensor, ITensorSegment2 tensor2)
        {
            return Blas.dot(tensor.GetLocalOrNewArray(), tensor2.GetLocalOrNewArray());
        }

        ITensorSegment2 Apply(ITensorSegment2 tensor, ITensorSegment2 tensor2, Action<int, float[], float[], float[]> mkl)
        {
            var size = GetSize(tensor, tensor2);
            var result = CreateSegment(size);
            mkl((int)size, tensor.GetLocalOrNewArray(), tensor2.GetLocalOrNewArray(), result.GetArrayForLocalUseOnly()!);
            return result;
        }

        ITensorSegment2 Apply(ITensorSegment2 tensor, Action<int, float[], float[]> mkl)
        {
            var result = CreateSegment(tensor.Size);
            mkl((int)tensor.Size, tensor.GetLocalOrNewArray(), result.GetArrayForLocalUseOnly()!);
            return result;
        }

        ITensorSegment2 Clone(ITensorSegment2 tensor, float coefficient)
        {
            var result = CreateSegment(tensor.Size);
            tensor.CopyTo(result);
            result.MutateInPlaceVectorised(
                (in System.Numerics.Vector<float> a, out System.Numerics.Vector<float> r) => r = a * coefficient, 
                a => a * coefficient
            );
            return result;
        }

        public override ITensorSegment2 Add(ITensorSegment2 tensor, ITensorSegment2 tensor2) => Apply(tensor, tensor2, Vml.Add);
        public override ITensorSegment2 Abs(ITensorSegment2 tensor) => Apply(tensor, Vml.Abs);
        public override ITensorSegment2 Add(ITensorSegment2 tensor, ITensorSegment2 tensor2, float coefficient1, float coefficient2)
        {
            // TODO: check timing of this
            using var t1 = Clone(tensor, coefficient1);
            using var t2 = Clone(tensor2, coefficient2);
            return Add(t1, t2);
        }

        public override ITensorSegment2 Multiply(ITensorSegment2 target, float scalar) => Clone(target, scalar);

        public override void MultiplyInPlace(ITensorSegment2 target, float scalar)
        {
            var local = target.GetArrayForLocalUseOnly();
            if(local is not null)
                Blas.scal(scalar, local);
            else
                base.MultiplyInPlace(target, scalar);
        }

        public override IMatrix Multiply(IMatrix matrix, IMatrix other)
        {
            int rowsA = (int)matrix.RowCount, columnsArowsB = (int)matrix.ColumnCount, columnsB = (int)other.ColumnCount;
            var ret = Apply(matrix.Segment, matrix.Segment, (size, a, b, r) => {
                Blas.gemm(
                    Layout.ColMajor,
                    Trans.No,
                    Trans.No,
                    rowsA,
                    columnsB,
                    columnsArowsB,
                    1f,
                    a,
                    rowsA,
                    b,
                    columnsArowsB,
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

            var ret = Apply(matrix.Segment, (size, a, r) => {
                Array.Copy(a, r, a.Length);
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
            int rowsA = (int)matrix.RowCount, columnsA = (int)matrix.ColumnCount, columnsB = (int)other.ColumnCount, rowsB = (int)other.RowCount;
            var ret = Apply(matrix.Segment, matrix.Segment, (size, a, b, r) => {
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
            int rowsA = (int)matrix.RowCount, columnsArowsB = (int)matrix.ColumnCount, rowsB = (int)other.RowCount;
            var ret = Apply(matrix.Segment, matrix.Segment, (size, a, b, r) => {
                Blas.gemm(
                    Layout.ColMajor,
                    Trans.No,
                    Trans.Yes,
                    rowsA,
                    rowsB,
                    columnsArowsB,
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

            var s = CreateSegment((uint)mn);
            var u = CreateSegment((uint)(rows * rows));
            var vt = CreateSegment((uint)(cols * cols));
            using var rWork = CreateSegment((uint)mn);

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

        public override float L2Norm(ITensorSegment2 segment) => Blas.nrm2(segment.GetLocalOrNewArray());
        public override ITensorSegment2 Exp(ITensorSegment2 tensor) => Apply(tensor, Vml.Exp);
        public override ITensorSegment2 Tanh(ITensorSegment2 tensor) => Apply(tensor, Vml.Tanh);
        //public override ITensorSegment2 PointwiseMultiply(ITensorSegment2 tensor1, ITensorSegment2 tensor2) => Apply(tensor1, tensor2, Vml.Mul);
        //public override ITensorSegment2 PointwiseDivide(ITensorSegment2 tensor1, ITensorSegment2 tensor2) => Apply(tensor1, tensor2, Vml.Div);
        public override ITensorSegment2 Sqrt(ITensorSegment2 tensor) => Apply(tensor, Vml.Sqrt);
        public override ITensorSegment2 Log(ITensorSegment2 tensor) => Apply(tensor, Vml.Ln);
        public override ITensorSegment2 Subtract(ITensorSegment2 tensor1, ITensorSegment2 tensor2) => Apply(tensor1, tensor2, Vml.Sub);
        public override ITensorSegment2 Squared(ITensorSegment2 tensor) => Apply(tensor, Vml.Sqr);
    }
}
