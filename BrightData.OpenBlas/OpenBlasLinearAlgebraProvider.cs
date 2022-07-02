using BrightData.LinearAlegbra2;
using OpenBLAS;

namespace BrightData.OpenBlas
{
    public class OpenBlasLinearAlgebraProvider : LinearAlgebraProvider
    {
        public OpenBlasLinearAlgebraProvider(BrightDataContext context) : base(context)
        {
            int num_threads = 2;
            BLAS.OpenblasSetNumThreads(ref num_threads);
        }

        public override string Name => "openblas";
        public override Type VectorType { get; } = typeof(OpenBlasVector);
        public override Type MatrixType { get; } = typeof(OpenBlasMatrix);
        public override IVector CreateVector(ITensorSegment2 data) => new OpenBlasVector(data, this);
        public override IMatrix CreateMatrix(uint rowCount, uint columnCount, ITensorSegment2 data) => new OpenBlasMatrix(data, rowCount, columnCount, this);

        ITensorSegment2 Apply(ITensorSegment2 tensor, Action<int, float[], float[]> blas)
        {
            var result = CreateSegment(tensor.Size);
            blas((int)tensor.Size, tensor.GetLocalOrNewArray(), result.GetArrayForLocalUseOnly()!);
            return result;
        }

        public override unsafe IMatrix Transpose(IMatrix matrix)
        {
            var rows = (int)matrix.RowCount;
            var cols = (int)matrix.ColumnCount;

            var ret = Apply(matrix.Segment, (size, a, r) => {
                var ordering = (sbyte)'c';
                var transpose = (sbyte)'t';
                var alpha = 1f;
                BLAS.Somatcopy(
                    &ordering, 
                    &transpose,
                    ref rows,
                    ref cols,
                    ref alpha, 
                    ref a[0],
                    ref cols,
                    ref r[0],
                    ref rows
                );
            });
            return CreateMatrix(matrix.ColumnCount, matrix.RowCount, ret);
        }
    }
}