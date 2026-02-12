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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static unsafe void MatrixMultiplyChunked(T* a, T* b, int size, uint rows, uint cols, T* ret)
        {
            const int ChunkSize = 128;
            var vectorSize = Vector<T>.Count;
            var numVectors = size / vectorSize;
            var ceiling = numVectors * vectorSize;
            var totalSize = rows * cols;

            if (totalSize >= Consts.MinimumSizeForParallel)
                Parallel.For(0, totalSize / ChunkSize + 1, x => Multiply(x * ChunkSize));
            else {
                for (uint ind = 0; ind < totalSize; ind += ChunkSize)
                    Multiply(ind);
            }

            return;

            [MethodImpl(MethodImplOptions.AggressiveInlining)] void Multiply(long startIndex)
            {
                for (long index = startIndex, len = Math.Min(startIndex + ChunkSize, totalSize); index < len; index++) {
                    var i = (uint)(index % rows);
                    var j = (uint)(index / rows);

                    var xPtr = &a[i * size];
                    var yPtr = &b[j * size];
                    var xVectors = (Vector<T>*)xPtr;
                    var yVectors = (Vector<T>*)yPtr;

                    var vSum = Vector<T>.Zero;
                    for (var z = 0; z < numVectors; z++)
                        vSum += xVectors[z] * yVectors[z];

                    var sum = Vector.Dot(vSum, Vector<T>.One);
                    for (var z = ceiling; z < size; z++)
                        sum += xPtr[z] * yPtr[z];
                    ret[j * rows + i] = sum;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static unsafe void MatrixMultiplyTiled(T* a, T* b, int size, uint rows, uint cols, T* ret)
        {
            const int TileSize = 32; // Size of the tile, should be adjusted based on hardware cache sizes.
            var vectorSize = Vector<T>.Count;
            var numVectors = size / vectorSize;
            var ceiling = numVectors * vectorSize;
            var totalSize = rows * cols;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void MultiplyTile(uint rowStart, uint colStart)
            {
                for (uint i = rowStart; i < rowStart + TileSize && i < rows; i++) {
                    for (uint j = colStart; j < colStart + TileSize && j < cols; j++) {
                        var xPtr = &a[i * size];
                        var xSpan = new ReadOnlySpan<T>(xPtr, size);
                        var xVectors = MemoryMarshal.Cast<T, Vector<T>>(xSpan);

                        var yPtr = &b[j * size];
                        var ySpan = new ReadOnlySpan<T>(yPtr, size);
                        var yVectors = MemoryMarshal.Cast<T, Vector<T>>(ySpan);

                        var vSum = Vector<T>.Zero;
                        for (var z = 0; z < numVectors; z++)
                            vSum += xVectors[z] * yVectors[z];

                        var sum = Vector.Dot(vSum, Vector<T>.One);
                        for (var z = ceiling; z < size; z++)
                            sum += xPtr[z] * yPtr[z];
                        ret[j * rows + i] = sum;
                    }
                }
            }

            if (totalSize >= Consts.MinimumSizeForParallel) {
                Parallel.For(0, (int)Math.Ceiling((double)rows / TileSize), rowTile => {
                    for (uint colTile = 0; colTile < cols; colTile += TileSize) {
                        MultiplyTile((uint)rowTile * TileSize, colTile);
                    }
                });
            }
            else {
                for (uint rowTile = 0; rowTile < rows; rowTile += TileSize) {
                    for (uint colTile = 0; colTile < cols; colTile += TileSize) {
                        MultiplyTile(rowTile, colTile);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static unsafe void MatrixMultiplyTiled2(T* a, T* b, int size, uint rows, uint cols, T* ret)
        {
            const int L1BlockSize = 32;
            const int L2BlockSize = 64;
            var vectorSize = Vector<T>.Count;
            var numVectors = size / vectorSize;
            var ceiling = numVectors * vectorSize;
            var totalSize = rows * cols;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void MultiplyBlock(uint rowStart, uint colStart, uint rowEnd, uint colEnd)
            {
                for (uint i = rowStart; i < rowEnd && i < rows; i += L1BlockSize) {
                    for (uint j = colStart; j < colEnd && j < cols; j += L1BlockSize) {
                        for (uint ii = i; ii < i + L1BlockSize && ii < rowEnd && ii < rows; ii++) {
                            for (uint jj = j; jj < j + L1BlockSize && jj < colEnd && jj < cols; jj++) {
                                var xPtr = &a[ii * size];
                                var xSpan = new ReadOnlySpan<T>(xPtr, size);
                                var xVectors = MemoryMarshal.Cast<T, Vector<T>>(xSpan);

                                var yPtr = &b[jj * size];
                                var ySpan = new ReadOnlySpan<T>(yPtr, size);
                                var yVectors = MemoryMarshal.Cast<T, Vector<T>>(ySpan);

                                var vSum = Vector<T>.Zero;
                                for (var z = 0; z < numVectors; z++)
                                    vSum += xVectors[z] * yVectors[z];

                                var sum = Vector.Dot(vSum, Vector<T>.One);
                                for (var z = ceiling; z < size; z++)
                                    sum += xPtr[z] * yPtr[z];
                                ret[jj * rows + ii] = sum;
                            }
                        }
                    }
                }
            }

            if (totalSize >= Consts.MinimumSizeForParallel) {
                Parallel.For(0, (int)Math.Ceiling((double)rows / L2BlockSize), rowTile => {
                    for (uint colTile = 0; colTile < cols; colTile += L2BlockSize) {
                        MultiplyBlock((uint)rowTile * L2BlockSize, colTile, (uint)((rowTile + 1) * L2BlockSize), colTile + L2BlockSize);
                    }
                });
            }
            else {
                for (uint rowTile = 0; rowTile < rows; rowTile += L2BlockSize) {
                    for (uint colTile = 0; colTile < cols; colTile += L2BlockSize) {
                        MultiplyBlock(rowTile, colTile, rowTile + L2BlockSize, colTile + L2BlockSize);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static unsafe void MatrixMultiplyTiled3(T* a, T* b, int size, uint rows, uint cols, T* ret)
        {
            const int L1BlockSize = 32;
            const int L2BlockSize = 64;
            var vectorSize = Vector<T>.Count;
            var numVectors = size / vectorSize;
            var ceiling = numVectors * vectorSize;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void MultiplyBlock(uint rowStart, uint colStart, uint rowEnd, uint colEnd)
            {
                for (var i = rowStart; i < rowEnd; i += L1BlockSize) {
                    for (var j = colStart; j < colEnd; j += L1BlockSize) {
                        for (uint ii = i, iLen = Math.Min(i + L1BlockSize, rows); ii < iLen; ii++) {
                            var xPtr = &a[ii * size];
                            for (uint jj = j, jLen = Math.Min(j + L1BlockSize, cols); jj < jLen; jj++) {
                                var yPtr = &b[jj * size];
                                var vSum = Vector<T>.Zero;
                                //for (var z = 0; z < numVectors; z++)
                                //    vSum += Vector.Load(xPtr + z * vectorSize) * Vector.Load(yPtr + z * vectorSize);
                                var xVecs = MemoryMarshal.Cast<T, Vector<T>>(new ReadOnlySpan<T>(xPtr, size));
                                var yVecs = MemoryMarshal.Cast<T, Vector<T>>(new ReadOnlySpan<T>(yPtr, size));
                                for (var z = 0; z < numVectors; z++)
                                    vSum += xVecs[z] * yVecs[z];

                                var sum = Vector.Dot(vSum, Vector<T>.One);
                                for (var z = ceiling; z < size; z++)
                                    sum += xPtr[z] * yPtr[z];
                                ret[jj * rows + ii] = sum;
                            }
                        }
                    }
                }
            }

            if (rows * cols >= Consts.MinimumSizeForParallel) {
                Parallel.For(0, (int)Math.Ceiling((double)rows / L2BlockSize), rowTile => {
                    var rowStart = (uint)rowTile * L2BlockSize;
                    var rowEnd = rowStart + L2BlockSize;
                    for (var colTile = 0U; colTile < cols; colTile += L2BlockSize)
                        MultiplyBlock(rowStart, colTile, rowEnd, colTile + L2BlockSize);
                });
            }
            else {
                for (var rowTile = 0U; rowTile < rows; rowTile += L2BlockSize) {
                    for (var colTile = 0U; colTile < cols; colTile += L2BlockSize)
                        MultiplyBlock(rowTile, colTile, rowTile + L2BlockSize, colTile + L2BlockSize);
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

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        static unsafe void MatrixMultiplyFloat(float* a, float* b, int K, int M, int N, float* ret)
        {
            const int BLOCK_SIZE = 64;

            Parallel.For(0, (int)Math.Ceiling((double)N / BLOCK_SIZE), jBlockIndex =>
            {
                var jjStart = jBlockIndex * BLOCK_SIZE;
                var jjEnd = Math.Min(jjStart + BLOCK_SIZE, N);
                for (int iiStart = 0; iiStart < M; iiStart += BLOCK_SIZE) {
                    int iiEnd = Math.Min(iiStart + BLOCK_SIZE, M);
                    ProcessBlock(a, b, ret, K, M, iiStart, iiEnd, jjStart, jjEnd);
                }
            });

            [MethodImpl(MethodImplOptions.AggressiveOptimization)]
            static unsafe void ProcessBlock(float* a, float* b, float* ret, int K, int strideRet, int iStart, int iEnd, int jStart, int jEnd)
            {
                for (var jj = jStart; jj < jEnd; jj++) {
                    var ptrB = b + (long)jj * K;
                    var ii = iStart;

                    for (; ii < iEnd - 3; ii += 4) {
                        var ptrA0 = a + (long)ii * K;
                        var ptrA1 = a + (long)(ii + 1) * K;
                        var ptrA2 = a + (long)(ii + 2) * K;
                        var ptrA3 = a + (long)(ii + 3) * K;

                        var sum0 = Vector256<float>.Zero;
                        var sum1 = Vector256<float>.Zero;
                        var sum2 = Vector256<float>.Zero;
                        var sum3 = Vector256<float>.Zero;

                        var k = 0;
                        var kLimit = K - 15;
                        for (; k < kLimit; k += 16) {
                            var bVec1 = Avx.LoadVector256(ptrB + k);
                            var bVec2 = Avx.LoadVector256(ptrB + k + 8);

                            sum0 = Fma.MultiplyAdd(Avx.LoadVector256(ptrA0 + k), bVec1, sum0);
                            sum0 = Fma.MultiplyAdd(Avx.LoadVector256(ptrA0 + k + 8), bVec2, sum0);
                            sum1 = Fma.MultiplyAdd(Avx.LoadVector256(ptrA1 + k), bVec1, sum1);
                            sum1 = Fma.MultiplyAdd(Avx.LoadVector256(ptrA1 + k + 8), bVec2, sum1);
                            sum2 = Fma.MultiplyAdd(Avx.LoadVector256(ptrA2 + k), bVec1, sum2);
                            sum2 = Fma.MultiplyAdd(Avx.LoadVector256(ptrA2 + k + 8), bVec2, sum2);
                            sum3 = Fma.MultiplyAdd(Avx.LoadVector256(ptrA3 + k), bVec1, sum3);
                            sum3 = Fma.MultiplyAdd(Avx.LoadVector256(ptrA3 + k + 8), bVec2, sum3);
                        }
                        var s0 = HorizontalAdd(sum0);
                        var s1 = HorizontalAdd(sum1);
                        var s2 = HorizontalAdd(sum2);
                        var s3 = HorizontalAdd(sum3);

                        for (; k < K; k++) {
                            var bVal = ptrB[k];
                            s0 += ptrA0[k] * bVal;
                            s1 += ptrA1[k] * bVal;
                            s2 += ptrA2[k] * bVal;
                            s3 += ptrA3[k] * bVal;
                        }

                        var baseIdx = (long)jj * strideRet + ii;
                        ret[baseIdx] = s0;
                        ret[baseIdx + 1] = s1;
                        ret[baseIdx + 2] = s2;
                        ret[baseIdx + 3] = s3;
                    }

                    for (; ii < iEnd; ii++) {
                        var ptrA = a + (long)ii * K;
                        var vSum = Vector256<float>.Zero;
                        var k = 0;
                        for (; k <= K - 8; k += 8)
                            vSum = Fma.MultiplyAdd(Avx.LoadVector256(ptrA + k), Avx.LoadVector256(ptrB + k), vSum);

                        var sum = HorizontalAdd(vSum);
                        for (; k < K; k++) 
                            sum += ptrA[k] * ptrB[k];
                        ret[(long)jj * strideRet + ii] = sum;
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
