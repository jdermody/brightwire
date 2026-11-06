using BrightData.Helper;
using BrightData.LinearAlgebra.ReadOnly;
using BrightData.LinearAlgebra.Segments;
using CommunityToolkit.HighPerformance.Buffers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Threading.Tasks;

namespace BrightData.LinearAlgebra
{
    /// <summary>
    /// Column major matrix type
    /// </summary>
    /// <typeparam name="LAP"></typeparam>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="data">Tensor segment</param>
    /// <param name="rows">Number of rows</param>
    /// <param name="columns">Number of columns</param>
    /// <param name="lap">Linear algebra provider</param>
    public class MutableMatrix<T, LAP>(INumericSegment<T> data, uint rows, uint columns, LAP lap) : MutableTensorBase<T, IReadOnlyMatrix<T>, IMatrix<T>, LAP>(data, lap), IMatrix<T>
        where T : unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
        where LAP : LinearAlgebraProvider<T>
    {
        /// <inheritdoc />
        public uint RowCount { get; private set; } = rows;

        /// <inheritdoc />
        public uint ColumnCount { get; private set; } = columns;

        /// <inheritdoc />
        public sealed override uint TotalSize { get; protected set; } = rows * columns;

        /// <inheritdoc />
        public sealed override uint[] Shape
        {
            get => [ColumnCount, RowCount];
            protected set
            {
                ColumnCount = value[0];
                RowCount = value[1];
                TotalSize = RowCount * ColumnCount;
            }
        }

        /// <inheritdoc cref="IMatrix{T}" />
        public T this[int rowY, int columnX]
        {
            get => Segment[columnX * RowCount + rowY];
            set => Segment[columnX * RowCount + rowY] = value;
        }

        /// <inheritdoc cref="IMatrix{T}" />
        public T this[uint rowY, uint columnX]
        {
            get => Segment[columnX * RowCount + rowY];
            set => Segment[columnX * RowCount + rowY] = value;
        }

        /// <inheritdoc />
        public T this[long rowY, long columnX]
        {
            get => Segment[columnX * RowCount + rowY];
            set => Segment[columnX * RowCount + rowY] = value;
        }

        /// <inheritdoc />
        public T this[ulong rowY, ulong columnX]
        {
            get => Segment[columnX * RowCount + rowY];
            set => Segment[columnX * RowCount + rowY] = value;
        }

        /// <inheritdoc />
        public INumericSegment<T> GetRow(uint index)
        {
            if (index >= RowCount)
                throw new ArgumentOutOfRangeException(nameof(index), $"Number of rows is {RowCount} but index {index} was requested");
            return new MutableTensorSegmentWrapper<T>(Segment, index, RowCount, ColumnCount);
        }

        /// <inheritdoc />
        public virtual INumericSegment<T> GetColumn(uint index)
        {
            if (index >= ColumnCount)
                throw new ArgumentOutOfRangeException(nameof(index), $"Number of columns is {ColumnCount} but index {index} was requested");
            return new MutableTensorSegmentWrapper<T>(Segment, index * RowCount, 1, RowCount);
        }

        /// <inheritdoc />
        public virtual IReadOnlyNumericSegment<T> GetReadOnlyRow(uint index)
        {
            if (index >= RowCount)
                throw new ArgumentOutOfRangeException(nameof(index), $"Number of rows is {RowCount} but index {index} was requested");
            return new ReadOnlyTensorSegmentWrapper<T>(Segment, index, RowCount, ColumnCount);
        }

        /// <inheritdoc />
        public virtual IReadOnlyNumericSegment<T> GetReadOnlyColumn(uint index)
        {
            if (index >= ColumnCount)
                throw new ArgumentOutOfRangeException(nameof(index), $"Number of columns is {ColumnCount} but index {index} was requested");
            return new ReadOnlyTensorSegmentWrapper<T>(Segment, index * RowCount, 1, RowCount);
        }

        /// <inheritdoc />
        public ReadOnlySpan<T> GetRowSpan(uint rowIndex, ref SpanOwner<T> temp) => GetReadOnlyRow(rowIndex).GetSpan(ref temp, out var _);

        /// <inheritdoc />
        public ReadOnlySpan<T> GetColumnSpan(uint columnIndex)
        {
            if (Segment.Contiguous is not null) {
                var ret = Segment.Contiguous.ReadOnlySpan;
                return ret.Slice((int)(columnIndex * RowCount), (int)RowCount);
            }
            using var column = GetReadOnlyColumn(columnIndex);
            return column.ToNewArray();
        }

        /// <inheritdoc />
        public override IMatrix<T> Create(INumericSegment<T> segment) => Lap.CreateMatrix(RowCount, ColumnCount, segment);

        /// <inheritdoc />
        public virtual IMatrix<T> Transpose()
        {
            var (buffer, rowCount, columnCount) = ReadOnlySegment.ApplyReadOnlySpan(x => x.Transpose(RowCount, ColumnCount));
            return new MutableMatrix<T, LAP>(new ArrayPoolTensorSegment<T>(buffer), rowCount, columnCount, Lap);
        }

        IReadOnlyMatrix<T> IReadOnlyMatrix<T>.Transpose()
        {
            var (segment, rowCount, columnCount) = ReadOnlySegment.Transpose(RowCount, ColumnCount);
            return new ReadOnlyMatrix<T>(segment, rowCount, columnCount);
        }

        /// <inheritdoc />
        public virtual IVector<T> GetDiagonal()
        {
            if (RowCount != ColumnCount)
                throw new InvalidOperationException("Diagonal can only be found from square matrices");
            return Lap.CreateVector(RowCount, i => this[i, i]);
        }

        /// <inheritdoc />
        public virtual IVector<T> RowSums()
        {
            var rows = this.AllRowsAsReadOnly(false);
            return Lap.CreateVector(RowCount, i => rows[i].Sum());
        }

        /// <inheritdoc />
        public virtual IVector<T> ColumnSums()
        {
            var columns = this.AllColumnsAsReadOnly(false);
            return Lap.CreateVector(ColumnCount, i => columns[i].Sum());
        }

        /// <inheritdoc />
        public virtual IVector<T> Multiply(IVector<T> vector)
        {
            using var temp = vector.Reshape(null, 1);
            using var temp2 = Multiply(temp);
            return temp2.Reshape();
        }

        /// <inheritdoc />
        public virtual IMatrix<T> Multiply(IMatrix<T> other)
        {
            if (ColumnCount != other.RowCount)
                throw new ArgumentException("Matrix sizes do not agree");

            // transpose so that we can get contiguous vectors
            using var transposedThis = Transpose();
            return MultiplyWithThisTransposed(Lap, transposedThis, other);
        }

        /// <inheritdoc />
        public virtual IMatrix<T> TransposeAndMultiply(IMatrix<T> other)
        {
            if (ColumnCount != other.ColumnCount)
                throw new ArgumentException("Matrix sizes do not agree");
            using var transposed = other.Transpose();
            return Multiply(transposed);
        }

        /// <inheritdoc />
        public virtual IMatrix<T> TransposeThisAndMultiply(IMatrix<T> other)
        {
            if (RowCount != other.RowCount)
                throw new ArgumentException("Matrix sizes do not agree");
            return MultiplyWithThisTransposed(Lap, this, other);
        }

        /// <inheritdoc />
        public void MapIndexedInPlace(Func<uint, uint, T, T> mutator)
        {
            var ret = Segment.MapParallel((ind, val) => {
                var i = ind % RowCount;
                var j = ind / RowCount;
                return mutator(i, j, val);
            });
            try {
                ret.CopyTo(Segment);
            }
            finally {
                ret.Release();
            }
        }

        /// <inheritdoc />
        public virtual (IMatrix<T> Left, IMatrix<T> Right) SplitAtColumn(uint columnIndex)
        {
            var ret1 = Lap.CreateMatrix(RowCount, columnIndex, (x, y) => this[x, y]);
            var ret2 = Lap.CreateMatrix(RowCount, ColumnCount - columnIndex, (x, y) => this[x, columnIndex + y]);
            return (ret1, ret2);
        }

        /// <inheritdoc />
        public virtual (IMatrix<T> Top, IMatrix<T> Bottom) SplitAtRow(uint rowIndex)
        {
            var ret1 = Lap.CreateMatrix(rowIndex, ColumnCount, (x, y) => this[x, y]);
            var ret2 = Lap.CreateMatrix(RowCount - rowIndex, ColumnCount, (x, y) => this[rowIndex + x, y]);
            return (ret1, ret2);
        }

        /// <inheritdoc />
        public virtual IMatrix<T> ConcatBelow(IMatrix<T> bottom)
        {
            Debug.Assert(ColumnCount == bottom.ColumnCount);
            return Lap.CreateMatrix(RowCount + bottom.RowCount, ColumnCount, (x, y) => {
                var m = x >= RowCount ? bottom : this;
                return m[x >= RowCount ? x - RowCount : x, y];
            });
        }

        /// <inheritdoc />
        public virtual IMatrix<T> ConcatRight(IMatrix<T> right)
        {
            Debug.Assert(RowCount == right.RowCount);
            return Lap.CreateMatrix(RowCount, ColumnCount + right.ColumnCount, (x, y) => {
                var m = y >= ColumnCount ? right : this;
                return m[x, y >= ColumnCount ? y - ColumnCount : y];
            });
        }

        /// <inheritdoc />
        public virtual (IMatrix<T> U, IVector<T> S, IMatrix<T> VT) Svd()
        {
            // For a matrix A, compute SVD: A = U * S * V^T
            // Using Jacobi eigenvalue decomposition on A^T * A to find V and singular values
            // Then U is computed from A * V * S^-1
            
            if (RowCount == 0 || ColumnCount == 0)
                throw new InvalidOperationException("Cannot compute SVD of empty matrix");

            uint eigenSize = Math.Min(RowCount, ColumnCount);
            
            // Compute A^T * A for eigenvectors using direct element access
            IMatrix<T>? AtA = null;
            try {
                AtA = Lap.CreateMatrix(ColumnCount, ColumnCount, true);
                
                // Directly compute A^T * A[i,j] = sum over k of A[k,i] * A[k,j]
                // For column-major: A[k,i] = data[i * RowCount + k]
                for (uint i = 0; i < ColumnCount; i++) {
                    for (uint j = 0; j <= i; j++) {
                        T sum = T.Zero;
                        for (uint k = 0; k < RowCount; k++) {
                            sum += this[k, i] * this[k, j];
                        }
                        AtA[j, i] = sum;
                        if (i != j) {
                            AtA[i, j] = sum; // Symmetric matrix
                        }
                    }
                }
            } catch {
                AtA?.Dispose();
                throw;
            }
            
            try {
                // Compute eigenvalues and eigenvectors of the symmetric matrix A^T * A
                var (eigenValues, eigenVectors) = ComputeEigenDecomposition(AtA);
                
                try {
                    // Singular values are square roots of eigenvalues
                    using var singularValues = Lap.CreateVector(eigenSize, false);
                    for (uint i = 0; i < eigenSize; i++) {
                        var ev = eigenValues[i];
                        singularValues[i] = Math<T>.Sqrt(Math<T>.Max(ev, T.Zero));
                    }
                    
                    // Sort singular values in descending order
                    var sortedIndices = Enumerable.Range(0, (int)eigenSize)
                        .OrderByDescending(i => singularValues[(uint)i])
                        .Select(i => (uint)i)
                        .ToArray();
                    
                    // Reorder singular values and eigenvectors (V)
                    var sortedSingularValues = Lap.CreateVector(eigenSize, false);
                    IMatrix<T> sortedEigenVectors;

                    using (eigenVectors) {
                        sortedEigenVectors = Lap.CreateMatrix(eigenSize, eigenSize, false);
                        for (uint i = 0; i < eigenSize; i++) {
                            var origIdx = sortedIndices[i];
                            // Use the singular values (square roots) for the reordered values
                            sortedSingularValues[i] = singularValues[origIdx];
                            using var col = eigenVectors.GetColumn(origIdx);
                            col.CopyTo(sortedEigenVectors.GetColumn(i));
                        }
                    }

                    // V is the matrix of eigenvectors (from A^T * A)
                    IMatrix<T> vMatrix = sortedEigenVectors;
                    IVector<T> singularValuesVec = sortedSingularValues;
                    
                    // Compute U from A * V * S^-1
                    // First compute AV = A * V, then divide each column by corresponding singular value
                    using var tempV = vMatrix;
                    IMatrix<T> av = Multiply(tempV);
                    
                    try {
                        var uMatrix = Lap.CreateMatrix(RowCount, eigenSize, false);
                        for (uint i = 0; i < eigenSize; i++) {
                            var sVal = singularValuesVec[i];
                            if (Math<T>.IsZero(sVal)) {
                                // Handle zero singular value
                                uMatrix.GetColumn(i).Clear();
                            } else {
                                using var col = av.GetColumn(i);
                                col.ApplySpan(true, x => x.MultiplyInPlace(T.One / sVal));
                                col.CopyTo(uMatrix.GetColumn(i));
                            }
                        }
                        
                        return (uMatrix, singularValuesVec, vMatrix.Transpose());
                    }
                    finally {
                        av.Dispose();
                    }
                }finally
                {
                    eigenValues.Dispose();
                }
            }
            finally {
                AtA.Dispose();
            }
        }

        /// <summary>
        /// Computes eigenvalue decomposition of a symmetric matrix using Jacobi method
        /// </summary>
        /// <param name="matrix">Symmetric matrix</param>
        /// <returns>Tuple of (eigenvalues, eigenvectors)</returns>
        (IVector<T> EigenValues, IMatrix<T> EigenVectors) ComputeEigenDecomposition(IMatrix<T> matrix)
        {
            var size = matrix.RowCount;
            
            if (size == 0)
                throw new ArgumentException("Matrix size must be greater than 0");
            
            if (matrix.ColumnCount != size)
                throw new ArgumentException("Matrix must be square");
            
            // Initialize eigenvector matrix as identity
            IMatrix<T> v = Lap.CreateMatrix(size, size, true);
            try {
                for (uint i = 0; i < size; i++)
                    v[i, i] = T.One;
                
                // Copy input matrix to work on it
                using var a = Lap.CreateMatrix(size, size, false);
                matrix.CopyTo(a);
                
                // Jacobi eigenvalue decomposition
                const int MaxIterations = 100;
                T Epsilon = ComputeTolerance(matrix);
                
                for (int iter = 0; iter < MaxIterations; iter++) {
                    // Find largest off-diagonal element
                    T maxVal = T.Zero;
                    uint p = 0, q = 0;
                    
                    for (uint i = 0; i < size - 1; i++) {
                        for (uint j = i + 1; j < size; j++) {
                            var val = Math<T>.Abs(a[i, j]);
                            if (val > maxVal) {
                                maxVal = val;
                                p = i;
                                q = j;
                            }
                        }
                    }
                    
                    // Check convergence
                    if (maxVal < Epsilon)
                        break;
                    
                    // Compute rotation angle
                    var app = a[p, p];
                    var aqq = a[q, q];
                    var apq = a[p, q];
                    
                    var phi = (aqq - app) / ((T.One + T.One) * apq);
                    var tau = Math<T>.Sign(phi) / (Math<T>.Abs(phi) + Math<T>.Sqrt(T.One + phi * phi));
                    var c = T.One / Math<T>.Sqrt(T.One + tau * tau);
                    var s = tau * c;
                    
                    // Apply Jacobi rotation
                    ApplyJacobiRotation(a, v, p, q, c, s, size);
                }
                
                // Extract eigenvalues from diagonal
                var eigenValues = Lap.CreateVector(size, false);
                for (uint i = 0; i < size; i++)
                    eigenValues[i] = a[i, i];
                
                return (eigenValues, v);
            }
            catch {
                v.Dispose();
                throw;
            }

            static T ComputeTolerance(IMatrix<T> matrix)
            {
                // Use the Frobenius norm of the off-diagonal elements as the tolerance
                var size = matrix.RowCount;
                T sumOfSquares = T.Zero;
                int count = 0;
                
                for (uint i = 0; i < size; i++)
                {
                    for (uint j = 0; j < size; j++)
                    {
                        if (i != j) // Off-diagonal elements
                        {
                            var val = matrix[i, j];
                            sumOfSquares += val * val;
                            count++;
                        }
                    }
                }
                
                if (count == 0) 
                    return T.Zero;
                
                // Root mean square of off-diagonal elements
                var rms = Math<T>.Sqrt(sumOfSquares / T.CreateSaturating(count));
                
                // Use a small multiple of machine epsilon scaled by the matrix norm
                return rms * Math<T>.AlmostZero * T.CreateSaturating(100);
            }
        }

        /// <summary>
        /// Applies Jacobi rotation to matrix
        /// </summary>
        static void ApplyJacobiRotation(IMatrix<T> a, IMatrix<T> v, uint p, uint q, T c, T s, uint size)
        {
            // Store affected elements before modification
            var app = a[p, p];
            var aqq = a[q, q];
            var apq = a[p, q];
            
            // Compute rotated diagonal elements using standard Jacobi formulas
            var sin2 = c * s;

            // new_app = c^2*app - 2*c*s*apq + s^2*aqq
            // new_aqq = s^2*app + 2*c*s*apq + c^2*aqq
            a[p, p] = app * c * c - apq * Math<T>.Two * sin2 + aqq * s * s;
            a[q, q] = app * s * s + apq * Math<T>.Two * sin2 + aqq * c * c;
            a[p, q] = T.Zero;
            a[q, p] = T.Zero;
            
            // Compute rotated off-diagonal elements
            for (uint i = 0; i < size; i++) {
                if (i != p && i != q) {
                    var ap_i = a[p, i];
                    var aq_i = a[q, i];
                    
                    a[p, i] = c * ap_i + s * aq_i;
                    a[q, i] = -s * ap_i + c * aq_i;
                    a[i, p] = a[p, i];
                    a[i, q] = a[q, i];
                }
            }
            
            // Update eigenvector matrix
            for (uint i = 0; i < size; i++) {
                var vip = v[i, p];
                var viq = v[i, q];
                
                v[i, p] = c * vip + s * viq;
                v[i, q] = -s * vip + c * viq;
            }
        }

        /// <inheritdoc />
        public virtual IMatrix<T> GetNewMatrixFromRows(IEnumerable<uint> rowIndices) => Lap.CreateMatrixFromRows(rowIndices.Select(GetRow).ToArray());

        /// <inheritdoc />
        public virtual IMatrix<T> GetNewMatrixFromColumns(IEnumerable<uint> columnIndices) => Lap.CreateMatrixFromColumns(columnIndices.Select(GetColumn).ToArray());

        /// <inheritdoc />
        public virtual void AddToEachRow(IReadOnlyNumericSegment<T> segment) => MapIndexedInPlace((_, k, v) => v + segment[k]);

        /// <inheritdoc />
        public virtual void AddToEachColumn(IReadOnlyNumericSegment<T> segment) => MapIndexedInPlace((j, _, v) => v + segment[j]);

        /// <inheritdoc />
        public virtual void MultiplyEachRowWith(IReadOnlyNumericSegment<T> segment) => MapIndexedInPlace((_, k, v) => v * segment[k]);

        /// <inheritdoc />
        public virtual void MultiplyEachColumnWith(IReadOnlyNumericSegment<T> segment) => MapIndexedInPlace((j, _, v) => v * segment[j]);

        /// <inheritdoc />
        public virtual void SubtractRowVector(IReadOnlyNumericSegment<T> rowVector)
        {
            // Column-major layout: data[col * RowCount + row]
            // Subtract rowVector[row] from each element in that row
            for (uint col = 0; col < ColumnCount; col++)
            {
                for (uint row = 0; row < RowCount; row++)
                {
                    this[row, col] -= rowVector[row];
                }
            }
        }

        /// <inheritdoc />
        public virtual INumericSegment<T>[] SoftmaxPerRow()
        {
            using var segments = SpanOwner<IReadOnlyNumericSegment<T>>.Allocate((int)RowCount);
            var ptr = segments.Span;
            for (var i = 0; i < RowCount; i++)
                ptr[i] = GetRow((uint)i);
            return Lap.MultiSoftmax(segments.DangerousGetArray());
        }

        /// <inheritdoc />
        public virtual INumericSegment<T>[] SoftmaxDerivativePerRow(IReadOnlyNumericSegment<T>[] rows)
        {
            var derivatives = Lap.MultiSoftmaxDerivative(rows);
            using var transposed = Transpose();

            var ret = new INumericSegment<T>[RowCount];
            for (uint i = 0; i < RowCount; i++) {
                using var derivative = derivatives[i];
                using var row = transposed.GetColumnVector(i);
                using var sm = derivative.Multiply(row);
                ret[i] = sm.Segment;
                sm.Segment.AddRef();
            }

            return ret;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static unsafe IMatrix<T> MultiplyWithThisTransposed(LinearAlgebraProvider<T> lap, IMatrix<T> transposedThis, IMatrix<T> other)
        {
            var lda = (int)transposedThis.RowCount;
            var ldb = (int)other.RowCount;
            if (lda != ldb)
                throw new ArgumentException("Expected matrix rows to be the same");

            var rowCount = transposedThis.ColumnCount;
            var columnCount = other.ColumnCount;
            var ret = lap.CreateMatrix(rowCount, columnCount, false);

            SpanOwner<T> matrixTemp = SpanOwner<T>.Empty, otherTemp = SpanOwner<T>.Empty;
            var matrixSpan = transposedThis.Segment.GetSpan(ref matrixTemp, out var wasMatrixTempUsed);
            var otherSpan = other.Segment.GetSpan(ref otherTemp, out var wasOtherTempUsed);
            try {
                var retSpan = ret.Segment.Contiguous!.ReadOnlySpan;
                fixed (T* matrixPtr = matrixSpan)
                fixed (T* otherPtr = otherSpan)
                fixed (T* retPtr = retSpan) {
                    if (typeof(T) == typeof(float) && Avx2.IsSupported && Fma.IsSupported)
                        MatrixMultiplyFloat((float*)matrixPtr, (float*)otherPtr, lda, (int)rowCount, (int)columnCount, (float*)retPtr);
                    else
                        MatrixMultiplyTiled3(matrixPtr, otherPtr, lda, rowCount, columnCount, retPtr);
                }
            }
            finally {
                if (wasMatrixTempUsed)
                    matrixTemp.Dispose();
                if (wasOtherTempUsed)
                    otherTemp.Dispose();
            }

            return ret;
        }

        /// <summary>
        /// Blocked vectorized GEMM for any unmanaged floating point type T.
        /// <para>a: rows x size, row i at a[i * size]</para>
        /// <para>b: cols x size, row j at b[j * size]</para>
        /// <para>ret: column-major rows x cols, ret[j * rows + i] = sum_k a[i * size + k] * b[j * size + k]</para>
        /// </summary>
        /// <remarks>
        /// Uses a 4 x (2 * vectorSize) microkernel: a small block of a-rows and b-rows is
        /// accumulated in vector registers while k is streamed once, so the b tile stays in
        /// cache and each output row has its own independent FMA dependency chain.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static unsafe void MatrixMultiplyTiled3(T* a, T* b, int size, uint rows, uint cols, T* ret)
        {
            const int MBlock = 4;
            var vectorSize = Vector<T>.Count;
            var nBlock = vectorSize * 2;
            var iBlocks = (int)((rows + MBlock - 1) / MBlock);
            var jBlocks = (int)((cols + nBlock - 1) / nBlock);
            var totalTasks = iBlocks * jBlocks;

            if (totalTasks > 1 && rows * cols >= Consts.MinimumSizeForParallel) {
                Parallel.For(0, totalTasks, task => {
                    var jb = task / iBlocks;
                    var ib = task - jb * iBlocks;
                    MultiplyBlock(a, b, ret, size, rows, cols, ib * MBlock, jb * nBlock, vectorSize);
                });
            }
            else {
                for (var jb = 0; jb < jBlocks; jb++)
                    for (var ib = 0; ib < iBlocks; ib++)
                        MultiplyBlock(a, b, ret, size, rows, cols, ib * MBlock, jb * nBlock, vectorSize);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static unsafe void MultiplyBlock(T* a, T* b, T* ret, int size, uint rows, uint cols, int i0, int j0, int vectorSize)
            {
                var iEnd = Math.Min(i0 + MBlock, (int)rows);
                var jEnd = Math.Min(j0 + vectorSize * 2, (int)cols);
                var mLen = iEnd - i0;
                var jLen = jEnd - j0;

                var a0 = a + (long)i0 * size;
                var a1 = a0 + size;
                var a2 = a1 + size;
                var a3 = a2 + size;
                var b0 = b + (long)j0 * size;

                if (jLen == vectorSize * 2) {
                    var acc0Lo = Vector<T>.Zero;
                    var acc0Hi = Vector<T>.Zero;
                    var acc1Lo = Vector<T>.Zero;
                    var acc1Hi = Vector<T>.Zero;
                    var acc2Lo = Vector<T>.Zero;
                    var acc2Hi = Vector<T>.Zero;
                    var acc3Lo = Vector<T>.Zero;
                    var acc3Hi = Vector<T>.Zero;

                    for (var k = 0; k < size; k++) {
                        var bVecLo = Vector.LoadUnsafe(ref *(b0 + k));
                        var bVecHi = Vector.LoadUnsafe(ref *(b0 + k + vectorSize));
                        if (mLen > 0) {
                            var av = a0[k];
                            acc0Lo += av * bVecLo;
                            acc0Hi += av * bVecHi;
                        }
                        if (mLen > 1) {
                            var av = a1[k];
                            acc1Lo += av * bVecLo;
                            acc1Hi += av * bVecHi;
                        }
                        if (mLen > 2) {
                            var av = a2[k];
                            acc2Lo += av * bVecLo;
                            acc2Hi += av * bVecHi;
                        }
                        if (mLen > 3) {
                            var av = a3[k];
                            acc3Lo += av * bVecLo;
                            acc3Hi += av * bVecHi;
                        }
                    }

                    var jPtr0 = ret + (long)j0 * rows + i0;
                    var jPtr1 = jPtr0 + 1;
                    var jPtr2 = jPtr1 + 1;
                    var jPtr3 = jPtr2 + 1;
                    for (var n = 0; n < vectorSize; n++) {
                        if (mLen > 0)
                            jPtr0[n] = acc0Lo[n];
                        if (mLen > 1)
                            jPtr1[n] = acc1Lo[n];
                        if (mLen > 2)
                            jPtr2[n] = acc2Lo[n];
                        if (mLen > 3)
                            jPtr3[n] = acc3Lo[n];
                        jPtr0 += rows;
                        jPtr1 += rows;
                        jPtr2 += rows;
                        jPtr3 += rows;
                    }
                    for (var n = 0; n < vectorSize; n++) {
                        if (mLen > 0)
                            jPtr0[n] = acc0Hi[n];
                        if (mLen > 1)
                            jPtr1[n] = acc1Hi[n];
                        if (mLen > 2)
                            jPtr2[n] = acc2Hi[n];
                        if (mLen > 3)
                            jPtr3[n] = acc3Hi[n];
                        jPtr0 += rows;
                        jPtr1 += rows;
                        jPtr2 += rows;
                        jPtr3 += rows;
                    }
                } else if (jLen == vectorSize) {
                    var acc0 = Vector<T>.Zero;
                    var acc1 = Vector<T>.Zero;
                    var acc2 = Vector<T>.Zero;
                    var acc3 = Vector<T>.Zero;

                    for (var k = 0; k < size; k++) {
                        var bVec = Vector.LoadUnsafe(ref *(b0 + k));
                        if (mLen > 0)
                            acc0 += a0[k] * bVec;
                        if (mLen > 1)
                            acc1 += a1[k] * bVec;
                        if (mLen > 2)
                            acc2 += a2[k] * bVec;
                        if (mLen > 3)
                            acc3 += a3[k] * bVec;
                    }

                    var jPtr0 = ret + (long)j0 * rows + i0;
                    var jPtr1 = jPtr0 + 1;
                    var jPtr2 = jPtr1 + 1;
                    var jPtr3 = jPtr2 + 1;
                    for (var n = 0; n < vectorSize; n++) {
                        if (mLen > 0)
                            jPtr0[n] = acc0[n];
                        if (mLen > 1)
                            jPtr1[n] = acc1[n];
                        if (mLen > 2)
                            jPtr2[n] = acc2[n];
                        if (mLen > 3)
                            jPtr3[n] = acc3[n];
                        jPtr0 += rows;
                        jPtr1 += rows;
                        jPtr2 += rows;
                        jPtr3 += rows;
                    }
                } else {
                    // Partial block: fall back to per-column dot products
                    for (var j = j0; j < jEnd; j++) {
                        var bCol = b + (long)j * size;
                        if (mLen > 0) {
                            var sum = default(T);
                            for (var k = 0; k < size; k++)
                                sum += a0[k] * bCol[k];
                            ret[(long)j * rows + i0] = sum;
                        }
                        if (mLen > 1) {
                            var sum = default(T);
                            for (var k = 0; k < size; k++)
                                sum += a1[k] * bCol[k];
                            ret[(long)j * rows + i0 + 1] = sum;
                        }
                        if (mLen > 2) {
                            var sum = default(T);
                            for (var k = 0; k < size; k++)
                                sum += a2[k] * bCol[k];
                            ret[(long)j * rows + i0 + 2] = sum;
                        }
                        if (mLen > 3) {
                            var sum = default(T);
                            for (var k = 0; k < size; k++)
                                sum += a3[k] * bCol[k];
                            ret[(long)j * rows + i0 + 3] = sum;
                        }
                    }
                }
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var preview = String.Join("|", Segment.Values.Take(Consts.DefaultPreviewSize));
            if (TotalSize > Consts.DefaultPreviewSize)
                preview += "|...";
            return $"Matrix (Rows: {RowCount}, Columns: {ColumnCount}) {preview}";
        }

        /// <inheritdoc />
        public IMatrix<T> Create(LinearAlgebraProvider<T> lap2) => lap2.CreateMatrix((IReadOnlyMatrix<T>)this);

        /// <inheritdoc />
        public override ReadOnlySpan<byte> DataAsBytes => ReadOnlyMatrix<T>.GetDataAsBytes(Segment, RowCount, ColumnCount);

        /// <inheritdoc />
        protected override IMatrix<T> Create(MemoryOwner<T> memory) => new MutableMatrix<T, LAP>(new ArrayPoolTensorSegment<T>(memory), RowCount, ColumnCount, Lap);

        /// <summary>
        /// AVX2+FMA blocked GEMM for single precision.
        /// a: M x K with row i at a[i * K], b: N x K with row j at b[j * K],
        /// ret: column-major M x N with ret[j * M + i] = dot(a[i], b[j]).
        /// </summary>
        /// <remarks>
        /// Uses a 4x4 microkernel with 2D blocking (64 x 32 outer tiles), so both the a and
        /// b tiles stay cache resident and the FMA dependency chains are independent per
        /// output row, allowing near-peak FMA throughput on AVX2+FMA hardware.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        static unsafe void MatrixMultiplyFloat(float* a, float* b, int K, int M, int N, float* ret)
        {
            const int I_BLOCK = 64;
            const int J_BLOCK = 32;
            var iBlocks = (M + I_BLOCK - 1) / I_BLOCK;
            var jBlocks = (N + J_BLOCK - 1) / J_BLOCK;
            var totalTasks = iBlocks * jBlocks;

            if (totalTasks > 1 && (long)M * N >= Consts.MinimumSizeForParallel) {
                Parallel.For(0, totalTasks, task => {
                    var jb = task / iBlocks;
                    var ib = task - jb * iBlocks;
                    ProcessBlock(a, b, ret, K, M, N, ib * I_BLOCK, jb * J_BLOCK);
                });
            }
            else {
                for (var jb = 0; jb < jBlocks; jb++)
                    for (var ib = 0; ib < iBlocks; ib++)
                        ProcessBlock(a, b, ret, K, M, N, ib * I_BLOCK, jb * J_BLOCK);
            }

            [MethodImpl(MethodImplOptions.AggressiveOptimization)]
            static unsafe void ProcessBlock(float* a, float* b, float* ret, int K, int M, int N, int i0, int j0)
            {
                var iEnd = Math.Min(i0 + I_BLOCK, M);
                var jEnd = Math.Min(j0 + J_BLOCK, N);

                for (var i = i0; i < iEnd; i += 4) {
                    var iLen = Math.Min(i + 4, iEnd) - i;
                    var ptrA0 = a + (long)i * K;
                    var ptrA1 = ptrA0 + K;
                    var ptrA2 = ptrA1 + K;
                    var ptrA3 = ptrA2 + K;

                    if (iLen == 4) {
                        for (var j = j0; j < jEnd; j += 4) {
                            var jLen = Math.Min(j + 4, jEnd) - j;
                            var ptrB0 = b + (long)j * K;
                            var ptrB1 = ptrB0 + K;
                            var ptrB2 = ptrB1 + K;
                            var ptrB3 = ptrB2 + K;

                            if (jLen == 4) {
                                var s00 = Vector256<float>.Zero;
                                var s01 = Vector256<float>.Zero;
                                var s02 = Vector256<float>.Zero;
                                var s03 = Vector256<float>.Zero;
                                var s10 = Vector256<float>.Zero;
                                var s11 = Vector256<float>.Zero;
                                var s12 = Vector256<float>.Zero;
                                var s13 = Vector256<float>.Zero;
                                var s20 = Vector256<float>.Zero;
                                var s21 = Vector256<float>.Zero;
                                var s22 = Vector256<float>.Zero;
                                var s23 = Vector256<float>.Zero;
                                var s30 = Vector256<float>.Zero;
                                var s31 = Vector256<float>.Zero;
                                var s32 = Vector256<float>.Zero;
                                var s33 = Vector256<float>.Zero;

                                var k = 0;
                                var kLimit = K - 7;
                                for (; k < kLimit; k += 8) {
                                    var a0v = Avx.LoadVector256(ptrA0 + k);
                                    var a1v = Avx.LoadVector256(ptrA1 + k);
                                    var a2v = Avx.LoadVector256(ptrA2 + k);
                                    var a3v = Avx.LoadVector256(ptrA3 + k);
                                    var b0v = Avx.LoadVector256(ptrB0 + k);
                                    var b1v = Avx.LoadVector256(ptrB1 + k);
                                    var b2v = Avx.LoadVector256(ptrB2 + k);
                                    var b3v = Avx.LoadVector256(ptrB3 + k);

                                    s00 = Fma.MultiplyAdd(a0v, b0v, s00);
                                    s01 = Fma.MultiplyAdd(a0v, b1v, s01);
                                    s02 = Fma.MultiplyAdd(a0v, b2v, s02);
                                    s03 = Fma.MultiplyAdd(a0v, b3v, s03);
                                    s10 = Fma.MultiplyAdd(a1v, b0v, s10);
                                    s11 = Fma.MultiplyAdd(a1v, b1v, s11);
                                    s12 = Fma.MultiplyAdd(a1v, b2v, s12);
                                    s13 = Fma.MultiplyAdd(a1v, b3v, s13);
                                    s20 = Fma.MultiplyAdd(a2v, b0v, s20);
                                    s21 = Fma.MultiplyAdd(a2v, b1v, s21);
                                    s22 = Fma.MultiplyAdd(a2v, b2v, s22);
                                    s23 = Fma.MultiplyAdd(a2v, b3v, s23);
                                    s30 = Fma.MultiplyAdd(a3v, b0v, s30);
                                    s31 = Fma.MultiplyAdd(a3v, b1v, s31);
                                    s32 = Fma.MultiplyAdd(a3v, b2v, s32);
                                    s33 = Fma.MultiplyAdd(a3v, b3v, s33);
                                }
                                var sum00 = HorizontalAdd(s00);
                                var sum01 = HorizontalAdd(s01);
                                var sum02 = HorizontalAdd(s02);
                                var sum03 = HorizontalAdd(s03);
                                var sum10 = HorizontalAdd(s10);
                                var sum11 = HorizontalAdd(s11);
                                var sum12 = HorizontalAdd(s12);
                                var sum13 = HorizontalAdd(s13);
                                var sum20 = HorizontalAdd(s20);
                                var sum21 = HorizontalAdd(s21);
                                var sum22 = HorizontalAdd(s22);
                                var sum23 = HorizontalAdd(s23);
                                var sum30 = HorizontalAdd(s30);
                                var sum31 = HorizontalAdd(s31);
                                var sum32 = HorizontalAdd(s32);
                                var sum33 = HorizontalAdd(s33);

                                for (; k < K; k++) {
                                    var a0k = ptrA0[k];
                                    var a1k = ptrA1[k];
                                    var a2k = ptrA2[k];
                                    var a3k = ptrA3[k];
                                    var b0k = ptrB0[k];
                                    var b1k = ptrB1[k];
                                    var b2k = ptrB2[k];
                                    var b3k = ptrB3[k];
                                    sum00 += a0k * b0k;
                                    sum01 += a0k * b1k;
                                    sum02 += a0k * b2k;
                                    sum03 += a0k * b3k;
                                    sum10 += a1k * b0k;
                                    sum11 += a1k * b1k;
                                    sum12 += a1k * b2k;
                                    sum13 += a1k * b3k;
                                    sum20 += a2k * b0k;
                                    sum21 += a2k * b1k;
                                    sum22 += a2k * b2k;
                                    sum23 += a2k * b3k;
                                    sum30 += a3k * b0k;
                                    sum31 += a3k * b1k;
                                    sum32 += a3k * b2k;
                                    sum33 += a3k * b3k;
                                }

                                var ret0 = ret + (long)j * M + i;
                                var ret1 = ret0 + M;
                                var ret2 = ret1 + M;
                                var ret3 = ret2 + M;
                                ret0[0] = sum00;
                                ret0[1] = sum10;
                                ret0[2] = sum20;
                                ret0[3] = sum30;
                                ret1[0] = sum01;
                                ret1[1] = sum11;
                                ret1[2] = sum21;
                                ret1[3] = sum31;
                                ret2[0] = sum02;
                                ret2[1] = sum12;
                                ret2[2] = sum22;
                                ret2[3] = sum32;
                                ret3[0] = sum03;
                                ret3[1] = sum13;
                                ret3[2] = sum23;
                                ret3[3] = sum33;
                            }
                            else {
                                for (var jj = 0; jj < jLen; jj++) {
                                    var ptrB = ptrB0 + jj * K;
                                    var sum0 = 0f;
                                    var sum1 = 0f;
                                    var sum2 = 0f;
                                    var sum3 = 0f;
                                    for (var k = 0; k < K; k++) {
                                        var bv = ptrB[k];
                                        sum0 += ptrA0[k] * bv;
                                        sum1 += ptrA1[k] * bv;
                                        sum2 += ptrA2[k] * bv;
                                        sum3 += ptrA3[k] * bv;
                                    }
                                    var ret0 = ret + (long)(j + jj) * M + i;
                                    ret0[0] = sum0;
                                    ret0[1] = sum1;
                                    ret0[2] = sum2;
                                    ret0[3] = sum3;
                                }
                            }
                        }
                    }
                    else {
                        // Trailing rows: dot products over the full j range
                        for (var j = j0; j < jEnd; j++) {
                            var ptrB = b + (long)j * K;
                            var ret0 = ret + (long)j * M + i;
                            for (var ii = 0; ii < iLen; ii++) {
                                var ptrA = ptrA0 + ii * K;
                                var sum = 0f;
                                for (var k = 0; k < K; k++)
                                    sum += ptrA[k] * ptrB[k];
                                ret0[ii] = sum;
                            }
                        }
                    }
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static float HorizontalAdd(Vector256<float> v)
            {
                var vLow = v.GetLower();
                var vHigh = Avx.ExtractVector128(v, 1);
                var v128 = Sse.Add(vLow, vHigh);
                v128 = Sse3.HorizontalAdd(v128, v128);
                v128 = Sse3.HorizontalAdd(v128, v128);
                return v128.ToScalar();
            }
        }
    }
}
