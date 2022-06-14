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
    public class MklComputationUnit : ComputationUnit
    {
        public MklComputationUnit(BrightDataContext2 context) : base(context)
        {
        }

        public override IVector CreateVector(ITensorSegment2 data) => new MklVector(data, this);

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

        IDisposableTensorSegment Clone(ITensorSegment2 tensor, float coefficient)
        {
            var result = CreateSegment(tensor.Size);
            tensor.CopyTo(result);
            result.MutateInPlaceVectorised(v => v * coefficient, v => v * coefficient);
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
                    1f,
                    r,
                    rowsA
                );
            });
            return CreateMatrix(ret, matrix.RowCount, other.ColumnCount);
        }
    }
}
