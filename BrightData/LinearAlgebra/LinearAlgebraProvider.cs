using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using BrightData.Helper;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace BrightData.LinearAlgebra
{
    public class LinearAlgebraProvider : IDisposable
    {
        readonly Stack<HashSet<IDisposable>> _scope = new();

        public LinearAlgebraProvider(
            BrightDataContext context
        )
        {
            Context = context;
            PushScope();
        }

        public virtual void Dispose()
        {
            GC.SuppressFinalize(this);
            foreach (var set in _scope) {
                foreach(var item in set)
                    item.Dispose();
            }
        }

        public BrightDataContext Context { get; }
        public virtual string Name => "default";

        public virtual Type VectorType { get; } = typeof(Vector2);
        public virtual Type MatrixType { get; } = typeof(Matrix2);
        public virtual Type Tensor3DType { get; } = typeof(Tensor3D2);
        public virtual Type Tensor4DType { get; } = typeof(Tensor4D2);

        // scope
        public void PushScope() => _scope.Push(new());
        public void PopScope()
        {
            var popped = _scope.Pop();
            foreach(var item in popped)
                item.Dispose();
        }
        internal bool AddToScope(IDisposable obj) => _scope.First().Add(obj);
        internal bool RemoveFromScope(IDisposable obj) => _scope.First().Remove(obj);

        // segment creation
        public virtual ITensorSegment2 CreateSegment(uint size) => new ArrayPoolTensorSegment(MemoryOwner<float>.Allocate((int)size, AllocationMode.Clear));
        public virtual ITensorSegment2 CreateSegment(uint size, Func<uint, float> initializer)
        {
            var ret = MemoryOwner<float>.Allocate((int)size, AllocationMode.Clear);
            var ptr = ret.Span;
            for (var i = 0; i < ptr.Length; i++)
                ptr[i] = initializer((uint)i);
            return new ArrayPoolTensorSegment(ret);
        }
        public virtual ITensorSegment2 Clone(ITensorSegment2 segment)
        {
            var ret = CreateSegment(segment.Size);
            segment.CopyTo(ret);
            return ret;
        }

        // vector creation
        public virtual IVector CreateVector(ITensorSegment2 data) => new Vector2(data, this);
        public IVector CreateVector(uint size) => CreateVector(CreateSegment(size));
        public IVector CreateVector(params float[] data) => CreateVector((uint)data.Length, i => data[i]);
        public IVector CreateVector(uint size, float value) => CreateVector(size, i => value);
        public IVector CreateVector(Span<float> span)
        {
            var segment = CreateSegment((uint)span.Length);
            segment.CopyFrom(span);
            return CreateVector(segment);
        }
        public IVector CreateVector(uint size, Func<uint, float> initializer) => CreateVector(CreateSegment(size, initializer));
        public IVector CreateVector(IVector vector) => CreateVector(Clone(vector.Segment));
        public IVector CreateVector(IVectorInfo vector) => vector.Create(this);

        // matrix creation
        public virtual IMatrix CreateMatrix(uint rowCount, uint columnCount, ITensorSegment2 data) => new Matrix2(data, rowCount, columnCount, this);
        public IMatrix CreateMatrix(uint rowCount, uint columnCount) => CreateMatrix(rowCount, columnCount, CreateSegment(rowCount * columnCount));
        public virtual IMatrix CreateMatrix(uint rowCount, uint columnCount, Func<uint, uint, float> initializer)
        {
            var segment = CreateSegment(rowCount * columnCount);
            var array = segment.GetArrayForLocalUseOnly()!;
            for (uint i = 0, len = segment.Size; i < len; i++)
                array[i] = initializer(i % rowCount, i / rowCount);
            return CreateMatrix(rowCount, columnCount, segment);
        }
        public IMatrix CreateMatrix(IMatrix matrix) => CreateMatrix(matrix.RowCount, matrix.ColumnCount, Clone(matrix.Segment));
        public IMatrix CreateMatrix(IMatrixInfo matrix) => matrix.Create(this);

        // create from rows
        public IMatrix CreateMatrixFromRows(params IVector[] rows) => CreateMatrixFromRows(rows.Select(v => v.Segment).ToArray());
        public IMatrix CreateMatrixFromRowsAndThenDisposeInput(params IVector[] rows)
        {
            try {
                return CreateMatrixFromRows(rows.Select(v => v.Segment).ToArray());
            }
            finally {
                foreach(var row in rows)
                    row.Dispose();
            }
        }
        public IMatrix CreateMatrixFromRows(params ITensorSegment2[] rows) => CreateMatrixFromRows(rows.AsSpan());
        public virtual IMatrix CreateMatrixFromRows(ReadOnlySpan<ITensorSegment2> rows)
        {
            var columns = rows[0].Size;
            var ret = CreateMatrix((uint)rows.Length, columns);
            var matrix = (IMatrixSegments)ret;
            for(var i = 0; i < rows.Length; i++)
                rows[i].CopyTo(matrix.Row((uint)i));
            return ret;
        }
        public IMatrix CreateMatrixFromRows(ReadOnlySpan<IVectorInfo> rows)
        {
            var columns = rows[0].Size;
            var ret = CreateMatrix((uint)rows.Length, columns);
            var matrix = (IMatrixSegments)ret;
            for (var i = 0; i < rows.Length; i++) {
                var sourceRow = rows[i];
                var targetRow = matrix.Row((uint)i);
                for (var j = 0; j < columns; j++)
                    targetRow[j] = sourceRow[j];
            } 
            return ret;
        }

        // create from columns
        public IMatrix CreateMatrixFromColumns(params IVector[] columns) => CreateMatrixFromColumns(columns.Select(v => v.Segment).ToArray());
        public IMatrix CreateMatrixFromColumnsAndThenDisposeInput(params IVector[] columns)
        {
            try {
                return CreateMatrixFromColumns(columns.Select(v => v.Segment).ToArray());
            }
            finally {
                foreach (var column in columns)
                    column.Dispose();
            }
        }
        public IMatrix CreateMatrixFromColumns(params ITensorSegment2[] columns) => CreateMatrixFromColumns(columns.AsSpan());
        public virtual IMatrix CreateMatrixFromColumns(ReadOnlySpan<ITensorSegment2> columns)
        {
            var rows = columns[0].Size;
            var ret = CreateMatrix(rows, (uint)columns.Length);
            var matrix = (IMatrixSegments)ret;
            for(var i = 0; i < columns.Length; i++)
                columns[i].CopyTo(matrix.Column((uint)i));
            return ret;
        }
        public IMatrix CreateMatrixFromColumns(ReadOnlySpan<IVectorInfo> columns)
        {
            var rows = columns[0].Size;
            var ret = CreateMatrix(rows, (uint)columns.Length);
            var matrix = (IMatrixSegments)ret;
            for (var i = 0; i < columns.Length; i++) {
                var sourceColumn = columns[i];
                var targetColumn = matrix.Column((uint)i);
                for (var j = 0; j < rows; j++)
                    targetColumn[j] = sourceColumn[j];
            } 
            return ret;
        }

        // 3D tensor creation
        public virtual ITensor3D CreateTensor3D(uint depth, uint rowCount, uint columnCount, ITensorSegment2 data) => new Tensor3D2(data, depth, rowCount, columnCount, this);
        public ITensor3D CreateTensor3D(uint depth, uint rowCount, uint columnCount) => CreateTensor3D(depth, rowCount, columnCount, CreateSegment(depth * rowCount * columnCount));
        public ITensor3D CreateTensor3D(params IMatrix[] matrices) => CreateTensor3D(matrices.AsSpan());
        public ITensor3D CreateTensor3D(ITensor3D tensor) => CreateTensor3D(tensor.Depth, tensor.RowCount, tensor.ColumnCount, Clone(tensor.Segment));
        public ITensor3D CreateTensor3D(ITensor3DInfo tensor) => tensor.Create(this);
        public ITensor3D CreateTensor3DAndThenDisposeInput(params IMatrix[] matrices)
        {
            try {
                return CreateTensor3D(matrices.AsSpan());
            }
            finally {
                foreach(var item in matrices)
                    item.Dispose();
            }
        }
        public ITensor3D CreateTensor3DAndThenDisposeInput(Span<IMatrix> matrices)
        {
            try {
                return CreateTensor3D(matrices);
            }
            finally {
                foreach(var item in matrices)
                    item.Dispose();
            }
        }
        public virtual ITensor3D CreateTensor3D(Span<IMatrix> matrices)
        {
            var first = matrices[0];
            var depth = (uint)matrices.Length;
            var rows = first.RowCount;
            var columns = first.ColumnCount;

            var data = CreateSegment(depth * rows * columns);
            var ret = CreateTensor3D(depth, rows, columns, data);
            var allSame = true;
            for (uint i = 0; i < ret.Depth; i++) {
                using var t = ret.GetMatrix(i);
                var s = matrices[(int)i];
                if (s.RowCount == t.RowCount && s.ColumnCount == t.ColumnCount)
                    s.Segment.CopyTo(t.Segment);
                else {
                    allSame = false;
                    break;
                }
            }

            if (!allSame) {
                throw new ArgumentException("Input matrices had different sizes");
            }

            return ret;
        }

        // 4D tensor creation
        public virtual ITensor4D CreateTensor4D(uint count, uint depth, uint rowCount, uint columnCount, ITensorSegment2 data) => new Tensor4D2(data, count, depth, rowCount, columnCount, this);
        public ITensor4D CreateTensor4D(uint count, uint depth, uint rowCount, uint columnCount) => CreateTensor4D(count, depth, rowCount, columnCount, CreateSegment(count * depth * rowCount * columnCount));
        public ITensor4D CreateTensor4D(params ITensor3D[] tensors) => CreateTensor4D(tensors.AsSpan());
        public ITensor4D CreateTensor4D(ITensor4D tensor) => CreateTensor4D(tensor.Count, tensor.Depth, tensor.RowCount, tensor.ColumnCount, Clone(tensor.Segment));
        public ITensor4D CreateTensor4D(ITensor4DInfo tensor) => tensor.Create(this);
        public ITensor4D CreateTensor4DAndThenDisposeInput(params ITensor3D[] tensors)
        {
            try {
                return CreateTensor4D(tensors.AsSpan());
            }
            finally {
                foreach(var item in tensors)
                    item.Dispose();
            }
        }
        public ITensor4D CreateTensor4DAndThenDisposeInput(Span<ITensor3D> tensors)
        {
            try {
                return CreateTensor4D(tensors);
            }
            finally {
                foreach(var item in tensors)
                    item.Dispose();
            }
        }
        public virtual ITensor4D CreateTensor4D(Span<ITensor3D> tensors)
        {
            var first = tensors[0];
            var count = (uint)tensors.Length;
            var rows = first.RowCount;
            var columns = first.ColumnCount;
            var depth = first.Depth;

            var data = CreateSegment(depth * rows * columns * count);
            var ret = CreateTensor4D(count, depth, rows, columns, data);
            var allSame = true;
            for (uint i = 0; i < ret.Depth; i++) {
                using var t = ret.GetTensor(i);
                var s = tensors[(int)i];
                if (s.RowCount == t.RowCount && s.ColumnCount == t.ColumnCount && s.Depth == t.Depth)
                    s.Segment.CopyTo(t.Segment);
                else {
                    allSame = false;
                    break;
                }
            }

            if (!allSame)
                throw new ArgumentException("Input tensors had different sizes");
            return ret;
        }

        protected static uint GetSize(ITensorSegment2 tensor, ITensorSegment2 tensor2)
        {
            if (tensor.Size != tensor2.Size)
                throw new Exception("Expected tensors to have same size");
            return tensor.Size;
        }

        public virtual ITensorSegment2 Add(ITensorSegment2 tensor, ITensorSegment2 tensor2) => tensor.Add(tensor2);
        public virtual ITensorSegment2 Add(ITensorSegment2 tensor, ITensorSegment2 tensor2, float coefficient1, float coefficient2) => tensor.Add(tensor2, coefficient1, coefficient2);
        public virtual ITensorSegment2 Add(ITensorSegment2 tensor, float scalar) => tensor.Add(scalar);
        public virtual void AddInPlace(ITensorSegment2 target, ITensorSegment2 other) => target.AddInPlace(other);
        public virtual void AddInPlace(ITensorSegment2 target, ITensorSegment2 other, float coefficient1, float coefficient2) => target.AddInPlace(other, coefficient1, coefficient2);
        public virtual void AddInPlace(ITensorSegment2 target, float scalar) => target.AddInPlace(scalar);
        public virtual void MultiplyInPlace(ITensorSegment2 target, float scalar) => target.MultiplyInPlace(scalar);
        public virtual ITensorSegment2 Multiply(ITensorSegment2 target, float scalar) => target.Multiply(scalar);
        public virtual ITensorSegment2 Subtract(ITensorSegment2 tensor1, ITensorSegment2 tensor2) => tensor1.Subtract(tensor2);
        public virtual ITensorSegment2 Subtract(ITensorSegment2 tensor1, ITensorSegment2 tensor2, float coefficient1, float coefficient2) => tensor1.Subtract(tensor2, coefficient1, coefficient2);
        public virtual void SubtractInPlace(ITensorSegment2 target, ITensorSegment2 other) => target.SubtractInPlace(other);
        public virtual void SubtractInPlace(ITensorSegment2 target, ITensorSegment2 other, float coefficient1, float coefficient2) => target.SubtractInPlace(other, coefficient1, coefficient2);
        public virtual ITensorSegment2 PointwiseMultiply(ITensorSegment2 tensor1, ITensorSegment2 tensor2) => tensor1.PointwiseMultiply(tensor2);
        public virtual void PointwiseMultiplyInPlace(ITensorSegment2 target, ITensorSegment2 other) => target.PointwiseMultiplyInPlace(other);
        public virtual ITensorSegment2 PointwiseDivide(ITensorSegment2 tensor1, ITensorSegment2 tensor2) => tensor1.PointwiseDivide(tensor2);
        public virtual void PointwiseDivideInPlace(ITensorSegment2 target, ITensorSegment2 other) => target.PointwiseDivideInPlace(other);
        public virtual float DotProduct(ITensorSegment2 tensor, ITensorSegment2 tensor2) => tensor.DotProduct(tensor2);
        public virtual ITensorSegment2 Sqrt(ITensorSegment2 tensor) => tensor.Sqrt();
        public virtual uint? Search(ITensorSegment2 segment, float value) => segment.Search(value);
        public virtual void ConstrainInPlace(ITensorSegment2 segment, float? minValue, float? maxValue) => segment.ConstrainInPlace(minValue, maxValue);
        public virtual float Average(ITensorSegment2 segment) => segment.Average();
        public virtual float L1Norm(ITensorSegment2 segment) => segment.L1Norm();
        public virtual float L2Norm(ITensorSegment2 segment) => segment.L2Norm();
        public virtual (float Min, float Max, uint MinIndex, uint MaxIndex) GetMinAndMaxValues(ITensorSegment2 segment) => segment.GetMinAndMaxValues();
        public virtual uint GetMinIndex(ITensorSegment2 segment) => GetMinAndMaxValues(segment).MinIndex;
        public virtual uint GetMaxIndex(ITensorSegment2 segment) => GetMinAndMaxValues(segment).MaxIndex;
        public virtual float GetMin(ITensorSegment2 segment) => GetMinAndMaxValues(segment).Min;
        public virtual float GetMax(ITensorSegment2 segment) => GetMinAndMaxValues(segment).Max;
        public virtual bool IsEntirelyFinite(ITensorSegment2 segment) => segment.IsEntirelyFinite();
        public virtual ITensorSegment2 Reverse(ITensorSegment2 segment) => segment.Reverse();
        public virtual IEnumerable<ITensorSegment2> Split(ITensorSegment2 segment, uint blockCount) => segment.Split(blockCount);
        public virtual float CosineDistance(ITensorSegment2 tensor, ITensorSegment2 other) => tensor.CosineDistance(other);
        public virtual float EuclideanDistance(ITensorSegment2 tensor, ITensorSegment2 other) => tensor.EuclideanDistance(other);
        public virtual float MeanSquaredDistance(ITensorSegment2 tensor, ITensorSegment2 other) => tensor.MeanSquaredDistance(other);
        public virtual float SquaredEuclideanDistance(ITensorSegment2 tensor, ITensorSegment2 other) => tensor.SquaredEuclideanDistance(other);
        public virtual float ManhattanDistance(ITensorSegment2 tensor, ITensorSegment2 other) => tensor.ManhattanDistance(other);
        public virtual ITensorSegment2 Abs(ITensorSegment2 tensor) => tensor.Abs();
        public virtual ITensorSegment2 Log(ITensorSegment2 tensor) => tensor.Log();
        public virtual ITensorSegment2 Exp(ITensorSegment2 tensor) => tensor.Exp();
        public virtual ITensorSegment2 Squared(ITensorSegment2 tensor) => tensor.Squared();
        public virtual float StdDev(ITensorSegment2 tensor, float? mean) => tensor.StdDev(mean);
        public virtual ITensorSegment2 Sigmoid(ITensorSegment2 tensor) => tensor.Sigmoid();
        public virtual ITensorSegment2 SigmoidDerivative(ITensorSegment2 tensor) => tensor.SigmoidDerivative();
        public virtual ITensorSegment2 Tanh(ITensorSegment2 tensor) => tensor.Tanh();
        public virtual ITensorSegment2 TanhDerivative(ITensorSegment2 tensor) => tensor.TanhDerivative();
        public virtual ITensorSegment2 Relu(ITensorSegment2 tensor) => tensor.Relu();
        public virtual ITensorSegment2 ReluDerivative(ITensorSegment2 tensor) => tensor.ReluDerivative();
        public virtual ITensorSegment2 LeakyRelu(ITensorSegment2 tensor) => tensor.LeakyRelu();
        public virtual ITensorSegment2 LeakyReluDerivative(ITensorSegment2 tensor) => tensor.LeakyReluDerivative();
        public virtual ITensorSegment2 Softmax(ITensorSegment2 tensor) => tensor.Softmax();
        public virtual IMatrix SoftmaxDerivative(ITensorSegment2 tensor) => tensor.SoftmaxDerivative(this);
        public virtual ITensorSegment2 Pow(ITensorSegment2 tensor, float power) => tensor.Pow(power);
        public virtual void RoundInPlace(ITensorSegment2 tensor, float lower, float upper, float? mid) => tensor.RoundInPlace(lower, upper, mid);
        public virtual ITensorSegment2 CherryPickIndices(ITensorSegment2 tensor, uint[] indices) => tensor.CherryPickIndices(indices);

        public virtual unsafe IMatrix Transpose(IMatrix matrix)
        {
            var columnCount = matrix.ColumnCount;
            var rowCount = matrix.RowCount;
            var ret = CreateMatrix(columnCount, rowCount);
            var temp = SpanOwner<float>.Empty;
            fixed (float* matrixPtr = &MemoryMarshal.GetReference(matrix.Segment.GetSpan(ref temp, out var wasTempUsed)))
            fixed (float* retPtr = &MemoryMarshal.GetReference(ret.Segment.GetSpan())) {
                CacheTranspose(matrixPtr, matrix.RowCount, matrix.ColumnCount, 0, matrix.RowCount, 0, matrix.ColumnCount, retPtr);
                //Parallel.For(0, matrix.Segment.Size, ind => {
                //    var i = (uint)(ind % rowCount);
                //    var j = (uint)(ind / rowCount);
                //    ret[j, i] = matrix[i, j];
                //});
                if (wasTempUsed)
                    temp.Dispose();
            }
            return ret;
        }

        static unsafe void CacheTranspose(float* from, uint rows, uint columns, uint rb, uint re, uint cb, uint ce, float* to) {
            uint r = re - rb, c = ce - cb;
            if (r <= 16 && c <= 16) {
                for (var i = rb; i < re; i++) {
                    for (var j = cb; j < ce; j++) {
                        to[i * columns + j] = from[j * rows + i];
                    }
                }
            } else if (r >= c) {
                CacheTranspose(from, rows, columns, rb, rb + (r / 2), cb, ce, to);
                CacheTranspose(from, rows, columns, rb + (r / 2), re, cb, ce, to);
            } else {
                CacheTranspose(from, rows, columns, rb, re, cb, cb + (c / 2), to);
                CacheTranspose(from, rows, columns, rb, re, cb + (c / 2), ce, to);
            }
        }

        //public virtual IMatrix OldMultiply(IMatrix matrix, IMatrix other)
        //{
        //    var rowCount = matrix.RowCount;
        //    var ret = CreateMatrix(rowCount, other.ColumnCount);
        //    var rows = matrix.AllRows();
        //    var columns = other.AllColumns();
            
        //    //for (uint ind = 0; ind < matrix.RowCount * other.ColumnCount; ind++) {
        //    Parallel.For(0, matrix.RowCount * other.ColumnCount, ind => {
        //        var i = (uint)(ind % rowCount);
        //        var j = (uint)(ind / rowCount);
        //        var row = rows[i];
        //        var column = columns[j];
        //        var val = row.DotProduct(column);
        //        ret[i, j] = val;
        //    });
        //    //}

        //    // don't need to dispose the wrappers
        //    return ret;
        //}

        public virtual IMatrix Multiply(IMatrix matrix, IMatrix other)
        {
            if (matrix.ColumnCount != other.RowCount)
                throw new Exception("Matrix sizes do not agree");

            // transpose so that we can can get contiguous vectors
            using var transposedThis = Transpose(matrix);
            return MultiplyWithThisTransposed(transposedThis, other);
            //return OldMultiply(matrix, other);
        }

        unsafe IMatrix MultiplyWithThisTransposed(IMatrix transposedThis, IMatrix other)
        {
            var size = (int)other.RowCount;
            var vectorSize = ExtensionMethods.NumericsVectorSize;
            var numVectors = size / vectorSize;
            var ceiling = numVectors * vectorSize;

            var rowCount = transposedThis.ColumnCount;
            var columnCount = other.ColumnCount;
            var ret = CreateMatrix(rowCount, columnCount);

            SpanOwner<float> matrixTemp = SpanOwner<float>.Empty, otherTemp = SpanOwner<float>.Empty;
            var matrixSpan = transposedThis.Segment.GetSpan(ref matrixTemp, out var wasMatrixTempUsed);
            var otherSpan = other.Segment.GetSpan(ref otherTemp, out var wasOtherTempUsed);
            try {
                var retSpan = ret.Segment.GetSpan();
                var lda = (int)transposedThis.RowCount;
                var ldb = (int)other.RowCount;
                fixed (float* matrixPtr = &MemoryMarshal.GetReference(matrixSpan))
                fixed (float* otherPtr = &MemoryMarshal.GetReference(otherSpan))
                fixed (float* retPtr = &MemoryMarshal.GetReference(retSpan)) {
                    var matrixPtr2 = matrixPtr;
                    var otherPtr2 = otherPtr;
                    var retPtr2 = retPtr;
                    //for (int ind = 0; ind < rowCount * columnCount; ind++) {
                    Parallel.For(0, rowCount * columnCount, ind => {
                        var i = (uint)(ind % rowCount);
                        var j = (uint)(ind / rowCount);

                        var xPtr = &matrixPtr2[i * lda];
                        var xSpan = new ReadOnlySpan<float>(xPtr, lda);
                        var xVectors = MemoryMarshal.Cast<float, System.Numerics.Vector<float>>(xSpan);

                        var yPtr = &otherPtr2[j * ldb];
                        var ySpan = new ReadOnlySpan<float>(yPtr, ldb);
                        var yVectors = MemoryMarshal.Cast<float, System.Numerics.Vector<float>>(ySpan);

                        var sum = 0f;
                        for (var z = 0; z < numVectors; z++) {
                            var temp = Vector.Multiply(xVectors[z], yVectors[z]);
                            sum += Vector.Sum(temp);
                        }

                        for (var z = ceiling; z < size; z++)
                            sum += xSpan[z] * ySpan[z];
                        retPtr2[j * rowCount + i] = sum;
                    });
                }
            }
            finally {
                if(wasMatrixTempUsed)
                    matrixTemp.Dispose();
                if(wasOtherTempUsed)
                    otherTemp.Dispose();
            }


            //Parallel.For(0, rowCount * columnCount, ind => {
            //for (int ind = 0; ind < rowCount * columnCount; ind++) {
            //    var i = (uint)(ind % rowCount);
            //    var j = (uint)(ind / rowCount);
            //    var leftPtr = transposedThis.GetColumnSpan(i);
            //    var rightPtr = other.GetColumnSpan(j);
            //    var leftVec = MemoryMarshal.Cast<float, Vector<float>>(leftPtr);
            //    var rightVec = MemoryMarshal.Cast<float, Vector<float>>(rightPtr);

            //    var sum = 0f;
            //    for (var x = 0; x < numVectors; x++) {
            //        var result = Vector.Multiply(leftVec[x], rightVec[x]);
            //        sum += Vector.Sum(result);
            //    }

            //    for (var y = ceiling; y < size; y++)
            //        sum += leftPtr[y] * rightPtr[y];
            //    ret[i, j] = sum;
            //}
            //});
            return ret;
        }

        //protected unsafe IMatrix MultiplyWithOtherTransposed(IMatrix matrix, IMatrix transposedOther)
        //{
        //    var size = (int)matrix.ColumnCount;
        //    var vectorSize = ExtensionMethods.NumericsVectorSize;
        //    var numVectors = size / vectorSize;
        //    var ceiling = numVectors * vectorSize;

        //    var rowCount = matrix.RowCount;
        //    var columnCount = transposedOther.RowCount;
        //    var ret = CreateMatrix(rowCount, columnCount);

        //    SpanOwner<float> matrixTemp = SpanOwner<float>.Empty, otherTemp = SpanOwner<float>.Empty;
        //    var matrixSpan = matrix.Segment.GetSpan(ref matrixTemp, out var wasMatrixTempUsed);
        //    var otherSpan = transposedOther.Segment.GetSpan(ref otherTemp, out var wasOtherTempUsed);
        //    try {
        //        var retSpan = ret.Segment.GetSpan();
        //        var matrixColumnCount = (int)matrix.ColumnCount;
        //        var otherColumnCount = (int)transposedOther.ColumnCount;
        //        fixed (float* matrixPtr = &MemoryMarshal.GetReference(matrixSpan))
        //        fixed (float* otherPtr = &MemoryMarshal.GetReference(otherSpan))
        //        fixed (float* retPtr = &MemoryMarshal.GetReference(retSpan)) {
        //            var matrixPtr2 = matrixPtr;
        //            var otherPtr2 = otherPtr;
        //            var retPtr2 = retPtr;
        //            Parallel.For(0, rowCount, i => {
        //                var xPtr = &matrixPtr2[i * matrixColumnCount];
        //                var xSpan = new ReadOnlySpan<float>(xPtr, matrixColumnCount);
        //                var xVectors = MemoryMarshal.Cast<float, Vector<float>>(xSpan);
        //                for (var j = 0; j < columnCount; j++) {
        //                    var yPtr = &otherPtr2[j * otherColumnCount];
        //                    var ySpan = new ReadOnlySpan<float>(yPtr, otherColumnCount);
        //                    var yVectors = MemoryMarshal.Cast<float, Vector<float>>(ySpan);

        //                    var sum = 0f;
        //                    for (var z = 0; z < numVectors; z++) {
        //                        var temp = Vector.Multiply(xVectors[z], yVectors[z]);
        //                        sum += Vector.Sum(temp);
        //                    }

        //                    for (var z = ceiling; z < size; z++)
        //                        sum += xSpan[z] * ySpan[z];
        //                    retPtr2[i * columnCount + j] = sum;
        //                }
        //            });
        //        }
        //    }
        //    finally {
        //        if(wasMatrixTempUsed)
        //            matrixTemp.Dispose();
        //        if(wasOtherTempUsed)
        //            otherTemp.Dispose();
        //    }

        //    return ret;
        //}

        public virtual IMatrix TransposeSecondAndMultiply(IMatrix matrix, IMatrix other)
        {
            if (matrix.ColumnCount != other.ColumnCount)
                throw new Exception("Matrix sizes do not agree");
            using var transposed = other.Transpose();
            return Multiply(matrix, transposed);
        }

        public virtual IMatrix TransposeFirstAndMultiply(IMatrix matrix, IMatrix other)
        {
            if (matrix.RowCount != other.RowCount)
                throw new Exception("Matrix sizes do not agree");
            return MultiplyWithThisTransposed(matrix, other);
        }

        public virtual IVector GetDiagonal(IMatrix matrix)
        {
            if(matrix.RowCount != matrix.ColumnCount)
                throw new Exception("Diagonal can only be found from square matrices");
            return CreateVector(matrix.RowCount, i => matrix[i, i]);
        }

        public virtual IVector RowSums(IMatrix matrix)
        {
            var rows = matrix.AllRows();
            return CreateVector(matrix.RowCount, i => rows[i].Segment.Sum());
        }

        public virtual IVector ColumnSums(IMatrix matrix)
        {
            var columns = matrix.AllColumns();
            return CreateVector(matrix.ColumnCount, i => columns[i].Segment.Sum());
        }

        public virtual IVector ColumnSums(ITensor4D tensor)
        {
            IVector? ret = null;
            for (uint i = 0, count = tensor.Count; i < count; i++) {
                using var subTensor = tensor.GetTensor(i);
                using var tensorAsMatrix = subTensor.Reshape(subTensor.RowCount * subTensor.ColumnCount, subTensor.Depth);
                var columnSums = tensorAsMatrix.ColumnSums();
                if (ret == null)
                    ret = columnSums;
                else {
                    ret.AddInPlace(columnSums);
                    columnSums.Dispose();
                }
            }
            return ret ?? tensor.LinearAlgebraProvider.CreateVector(tensor.ColumnCount);
        }

        public virtual (IMatrix U, IVector S, IMatrix VT) Svd(IMatrix matrix)
        {
            throw new NotImplementedException();
        }

        public ITensorSegment2 MapParallel(ITensorSegment2 segment, Func<float, float> mapper)
        {
            var ret = CreateSegment(segment.Size);
            // ReSharper disable once AccessToDisposedClosure
            Parallel.For(0, (int)segment.Size, i => ret[i] = mapper(segment[i]));
            return ret;
        }

        public void MapParallelInPlace(ITensorSegment2 segment, Func<float, float> mapper)
        {
            var ret = CreateSegment(segment.Size);
            try {
                // ReSharper disable once AccessToDisposedClosure
                Parallel.For(0, (int)segment.Size, i => ret[i] = mapper(segment[i]));
                ret.CopyTo(segment);
            }
            finally {
                ret.Release();
            }
        }

        public ITensorSegment2 MapParallel(ITensorSegment2 segment, Func<uint, float, float> mapper)
        {
            var ret = CreateSegment(segment.Size);
            // ReSharper disable once AccessToDisposedClosure
            Parallel.For(0, (int)segment.Size, i => ret[i] = mapper((uint)i, segment[i]));
            return ret;
        }

        public void MapParallelInPlace(ITensorSegment2 segment, Func<uint, float, float> mapper)
        {
            var ret = CreateSegment(segment.Size);
            try {
                // ReSharper disable once AccessToDisposedClosure
                Parallel.For(0, (int)segment.Size, i => ret[i] = mapper((uint)i, segment[i]));
                ret.CopyTo(segment);
            }
            finally {
                ret.Release();
            }
        }

        public virtual void FeatureScaleNormalization(ITensorSegment2 segment)
        {
            var (min, max, _, _) = GetMinAndMaxValues(segment);
            var range = max - min;
            if (FloatMath.IsNotZero(range))
                MapParallelInPlace(segment, v => (v - min) / range);
        }

        public virtual void StandardNormalization(ITensorSegment2 segment)
        {
            var mean = Average(segment);
            var stdDev = StdDev(segment, mean);
            if (FloatMath.IsNotZero(stdDev))
                MapParallelInPlace(segment, v => (v - mean) / stdDev);
        }

        public virtual void EuclideanNormalization(ITensorSegment2 segment)
        {
            var norm = L2Norm(segment);
            if (FloatMath.IsNotZero(norm))
                MapParallelInPlace(segment, v => v / norm);
        }

        public virtual void ManhattanNormalization(ITensorSegment2 segment)
        {
            var norm = L1Norm(segment);
            if (FloatMath.IsNotZero(norm))
                MapParallelInPlace(segment, v => v / norm);
        }

        public ITensorSegment2 Batch(ITensorSegment2 segment, ITensorSegment2[] others, Func<ITensorSegment2, ITensorSegment2, float> getValue)
        {
            var ret = CreateSegment((uint)others.Length);
            Parallel.ForEach(others, (vec, _, ind) => ret[ind] = getValue(segment, vec));
            return ret;
        }

        public virtual void L1Regularisation(ITensorSegment2 segment, float coefficient) => segment.L1Regularisation(coefficient);

        public virtual (IMatrix Left, IMatrix Right) SplitAtColumn(IMatrix matrix, uint columnIndex)
        {
            var ret1 = CreateMatrix(matrix.RowCount, columnIndex, (x, y) => matrix[x, y]);
            var ret2 = CreateMatrix(matrix.RowCount, matrix.ColumnCount - columnIndex, (x, y) => matrix[x, columnIndex + y]);
            return (ret1, ret2);
        }

        public virtual (IMatrix Top, IMatrix Bottom) SplitAtRow(IMatrix matrix, uint rowIndex)
        {
            var ret1 = CreateMatrix(rowIndex, matrix.ColumnCount, (x, y) => matrix[x, y]);
            var ret2 = CreateMatrix(matrix.RowCount - rowIndex, matrix.ColumnCount, (x, y) => matrix[rowIndex + x, y]);
            return (ret1, ret2);
        }

        public virtual IMatrix ConcatColumns(IMatrix top, IMatrix bottom)
        {
            Debug.Assert(top.ColumnCount == bottom.ColumnCount);
            return CreateMatrix(top.RowCount + bottom.RowCount, top.ColumnCount, (x, y) => {
                var m = x >= top.RowCount ? bottom : top;
                return m[x >= top.RowCount ? x - top.RowCount : x, y];
            });
        }

        public virtual IMatrix ConcatRows(IMatrix left, IMatrix right)
        {
            Debug.Assert(left.RowCount == right.RowCount);
            return CreateMatrix(left.RowCount, left.ColumnCount + right.ColumnCount, (x, y) => {
                var m = y >= left.ColumnCount ? right : left;
                return m[x, y >= left.ColumnCount ? y - left.ColumnCount : y];
            });
        }

        public virtual ITensor3D AddPadding(ITensor3D tensor, uint padding)
        {
            var newRows = tensor.RowCount + padding * 2;
            var newColumns = tensor.ColumnCount + padding * 2;
            var ret = CreateTensor3D(tensor.Depth, newRows, newColumns);

            for (uint k = 0; k < tensor.Depth; k++) {
                for (uint i = 0; i < newRows; i++) {
                    for (uint j = 0; j < newColumns; j++) {
                        if (i < padding || j < padding)
                            continue;
                        if (i >= newRows - padding || j >= newColumns - padding)
                            continue;
                        ret[i, j, k] = tensor[i - padding, j - padding, k];
                    }
                }
            }
            return ret;
        }

        public virtual ITensor3D RemovePadding(ITensor3D tensor, uint padding)
        {
            var newRows = tensor.RowCount - padding * 2;
            var newColumns = tensor.ColumnCount - padding * 2;
            var ret = CreateTensor3D(tensor.Depth, newRows, newColumns);
            for (uint k = 0; k < tensor.Depth; k++) {
                for (uint i = 0; i < newRows; i++) {
                    for (uint j = 0; j < newColumns; j++) {
                        ret[i, j, k] = tensor[i + padding, j + padding, k];
                    }
                }
            }

            return ret;
        }

        public virtual IMatrix Im2Col(ITensor3D tensor, uint filterWidth, uint filterHeight, uint xStride, uint yStride)
        {
            var convolutions = ConvolutionHelper.Default(tensor.ColumnCount, tensor.RowCount, filterWidth, filterHeight, xStride, yStride);
            var filterSize = filterWidth * filterHeight;
            var ret = CreateMatrix((uint)convolutions.Count, filterSize * tensor.Depth, (_, _) => 0f);

            for(int i = 0; i < convolutions.Count; i++) {
                var (offsetX, offsetY) = convolutions[i];
                for (uint k = 0; k < tensor.Depth; k++) {
                    var filterOffset = k * filterSize;
                    for (uint y = 0; y < filterHeight; y++) {
                        for (uint x = 0; x < filterWidth; x++) {
                            // write in column major format
                            var filterIndex = filterOffset + (x * filterHeight + y);
                            ret[(uint)i, filterIndex] = tensor[offsetY + y, offsetX + x, k];
                        }
                    }
                }
            }

            return ret;
        }

        public virtual (ITensor3D Result, ITensor3D? Indices) MaxPool(ITensor3D tensor, uint filterWidth, uint filterHeight, uint xStride, uint yStride, bool saveIndices)
        {
            var newColumns = (tensor.ColumnCount - filterWidth) / xStride + 1;
            var newRows = (tensor.RowCount - filterHeight) / yStride + 1;
            using var matrixList = SpanOwner<IMatrix>.Allocate((int)tensor.Depth, AllocationMode.Clear);
            var ptr = matrixList.Span;
            var indexList = saveIndices ? new IMatrix[tensor.Depth] : null;
            var convolutions = ConvolutionHelper.Default(tensor.ColumnCount, tensor.RowCount, filterWidth, filterHeight, xStride, yStride);

            for (uint k = 0; k < tensor.Depth; k++) {
                var indices = saveIndices ? CreateMatrix(newRows, newColumns) : null;
                var layer = CreateMatrix(newRows, newColumns);

                foreach (var (cx, cy) in convolutions) {
                    var targetX = cx / xStride;
                    var targetY = cy / yStride;
                    var maxVal = float.MinValue;
                    var bestOffset = -1;
                    var offset = 0;

                    for (uint x = 0; x < filterWidth; x++) {
                        for (uint y = 0; y < filterHeight; y++) {
                            var val = tensor[cy + y, cx + x, k];
                            if (val > maxVal || bestOffset == -1) {
                                bestOffset = offset;
                                maxVal = val;
                            }

                            ++offset;
                        }
                    }

                    if (indices != null)
                        indices[targetY, targetX] = bestOffset;
                    layer[targetY, targetX] = maxVal;
                }

                ptr[(int)k] = layer;
                if (indexList != null && indices != null)
                    indexList[k] = indices;
            }

            return (
                CreateTensor3DAndThenDisposeInput(ptr),
                indexList != null ? CreateTensor3DAndThenDisposeInput(indexList) : null
            );
        }

        public virtual ITensor3D ReverseMaxPool(ITensor3D tensor, ITensor3D indices, uint outputRows, uint outputColumns, uint filterWidth, uint filterHeight, uint xStride, uint yStride)
        {
            using var matrixList = SpanOwner<IMatrix>.Allocate((int)tensor.Depth, AllocationMode.Clear);
            var ptr = matrixList.Span;

            for (uint k = 0; k < tensor.Depth; k++) {
                using var source = tensor.GetMatrix(k);
                var sourceRows = source.RowCount;
                var sourceColumns = source.ColumnCount;
                using var index = indices.GetMatrix(k);
                var target = ptr[(int)k] = CreateMatrix(outputRows, outputColumns);

                for (uint j = 0; j < sourceColumns; j++) {
                    for (uint i = 0; i < sourceRows; i++) {
                        var value = source[i, j];
                        var offset = index[i, j];
                        var offsetRow = (uint)offset % filterHeight;
                        var offsetColumn = (uint)offset / filterHeight;
                        target[(int)(i * yStride + offsetRow), (int)(j * xStride + offsetColumn)] = value;
                    }
                }
            }

            return CreateTensor3DAndThenDisposeInput(ptr);
        }

        public virtual ITensor3D ReverseIm2Col(ITensor3D tensor, IMatrix filter, uint outputRows, uint outputColumns, uint outputDepth, uint filterWidth, uint filterHeight, uint xStride, uint yStride)
        {
            var convolutions = ConvolutionHelper.Default(outputColumns, outputRows, filterWidth, filterHeight, xStride, yStride);
            using var output = SpanOwner<IMatrix>.Allocate((int)outputDepth, AllocationMode.Clear);
            var ptr = output.Span;

            for (var i = 0; i < outputDepth; i++)
                ptr[i] = CreateMatrix(outputRows, outputColumns);
            for (uint k = 0; k < tensor.Depth; k++) {
                using var slice = tensor.GetMatrix(k);
                var filters = filter.GetColumn(k).Segment.Split(outputDepth).ToArray();

                foreach (var (cx, cy) in convolutions) {
                    var errorY = cy / xStride;
                    var errorX = cx / yStride;
                    if (errorX < slice.ColumnCount && errorY < slice.RowCount) {
                        var error = slice[errorY, errorX];
                        for (uint y = 0; y < filterHeight; y++) {
                            for (uint x = 0; x < filterWidth; x++) {
                                var filterIndex = (filterWidth - x - 1) * filterHeight + (filterHeight - y - 1);
                                for (uint z = 0; z < outputDepth; z++)
                                    ptr[(int)z][cy + y, cx + x] += filters[z][filterIndex] * error;
                            }
                        }
                    }
                }
            }

            return CreateTensor3DAndThenDisposeInput(ptr);
        }

        public virtual IMatrix CombineDepthSlices(ITensor3D tensor)
        {
            var ret = CreateMatrix(tensor.RowCount, tensor.ColumnCount);

            for (uint i = 0; i < tensor.Depth; i++) {
                using var matrix = tensor.GetMatrix(i);
                ret.AddInPlace(matrix);
            }

            return ret;
        }

        public virtual ITensor3D Multiply(ITensor3D tensor, IMatrix other)
        {
            using var ret = SpanOwner<IMatrix>.Allocate((int)tensor.Depth, AllocationMode.Clear);
            var ptr = ret.Span;

            for (uint i = 0; i < tensor.Depth; i++) {
                using var matrix = tensor.GetMatrix(i);
                ptr[(int)i] = matrix.Multiply(other);
            }
            return CreateTensor3DAndThenDisposeInput(ptr);
        }

        public virtual void AddToEachRow(ITensor3D tensor, IVector vector)
        {
            for (uint k = 0; k < tensor.Depth; k++) {
                for (uint j = 0; j < tensor.ColumnCount; j++) {
                    for (uint i = 0; i < tensor.RowCount; i++)
                        tensor[i, j, k] += vector[j];
                }
            }
        }

        public virtual ITensor3D TransposeFirstAndMultiply(ITensor3D tensor, ITensor4D other)
        {
            Debug.Assert(other.Count == tensor.Depth);
            using var ret = SpanOwner<IMatrix>.Allocate((int)other.Count, AllocationMode.Clear);
            var ptr = ret.Span;

            for (uint i = 0; i < other.Count; i++) {
                using var item = other.GetTensor(i);
                using var multiplyWith = item.Reshape(other.MatrixSize, other.Count);
                using var slice = tensor.GetMatrix(i);
                ptr[(int)i] = slice.TransposeThisAndMultiply(multiplyWith);
            }

            return CreateTensor3DAndThenDisposeInput(ptr);
        }

        public virtual ITensor4D AddPadding(ITensor4D tensor, uint padding)
        {
            using var ret = SpanOwner<ITensor3D>.Allocate((int)tensor.Count, AllocationMode.Clear);
            var ptr = ret.Span;

            for (uint i = 0; i < tensor.Count; i++) {
                using var subTensor = tensor.GetTensor(i);
                ptr[(int)i] = subTensor.AddPadding(padding);
            }

            return CreateTensor4DAndThenDisposeInput(ptr);
        }

        public virtual ITensor4D RemovePadding(ITensor4D tensor, uint padding)
        {
            using var ret = SpanOwner<ITensor3D>.Allocate((int)tensor.Count, AllocationMode.Clear);
            var ptr = ret.Span;

            for (uint i = 0; i < tensor.Count; i++) {
                using var subTensor = tensor.GetTensor(i);
                ptr[(int)i] = subTensor.RemovePadding(padding);
            }

            return CreateTensor4DAndThenDisposeInput(ptr);
        }

        public virtual (ITensor4D Result, ITensor4D? Indices) MaxPool(ITensor4D tensor, uint filterWidth, uint filterHeight, uint xStride, uint yStride, bool saveIndices)
        {
            var indexList = saveIndices 
                ? new ITensor3D[tensor.Count]
                : null;
            using var ret = SpanOwner<ITensor3D>.Allocate((int)tensor.Count, AllocationMode.Clear);
            var ptr = ret.Span;
            
            for (uint i = 0; i < tensor.Count; i++) {
                using var subTensor = tensor.GetTensor(i);
                var (result, indices) = subTensor.MaxPool(filterWidth, filterHeight, xStride, yStride, saveIndices);
                ptr[(int)i] = result;
                if (indexList != null && indices != null)
                    indexList[i] = indices;
            }

            return (CreateTensor4DAndThenDisposeInput(ptr), indexList != null ? CreateTensor4DAndThenDisposeInput(indexList) : null);
        }

        public virtual ITensor4D ReverseMaxPool(ITensor4D tensor, ITensor4D indices, uint outputRows, uint outputColumns, uint filterWidth, uint filterHeight, uint xStride, uint yStride)
        {
            using var ret = SpanOwner<ITensor3D>.Allocate((int)tensor.Count, AllocationMode.Clear);
            var ptr = ret.Span;

            for (uint i = 0; i < tensor.Count; i++) {
                using var subTensor = tensor.GetTensor(i);
                using var indexTensor = indices.GetTensor(i);
                var result = subTensor.ReverseMaxPool(indexTensor, outputRows, outputColumns, filterWidth, filterHeight, xStride, yStride);
                ptr[(int)i] = result;
            }

            return CreateTensor4DAndThenDisposeInput(ptr);
        }

        public virtual ITensor3D Im2Col(ITensor4D tensor, uint filterWidth, uint filterHeight, uint xStride, uint yStride)
        {
            var ret = SpanOwner<IMatrix>.Allocate((int)tensor.Count, AllocationMode.Clear);
            var ptr = ret.Span;
            
            for (uint i = 0; i < tensor.Count; i++) {
                using var subTensor = tensor.GetTensor(i);
                ptr[(int)i] = subTensor.Im2Col(filterWidth, filterHeight, xStride, yStride);
            }

            return CreateTensor3DAndThenDisposeInput(ptr);
        }

        public virtual ITensor4D ReverseIm2Col(ITensor4D tensor, IMatrix filter, uint outputRows, uint outputColumns, uint outputDepth, uint filterWidth, uint filterHeight, uint xStride, uint yStride)
        {
            var ret = SpanOwner<ITensor3D>.Allocate((int)tensor.Count, AllocationMode.Clear);
            var ptr = ret.Span;
            
            for (uint i = 0; i < tensor.Count; i++) {
                using var subTensor = tensor.GetTensor(i);
                ptr[(int)i] = subTensor.ReverseIm2Col(filter, outputRows, outputColumns, outputDepth, filterWidth, filterHeight, xStride, yStride);
            }

            return CreateTensor4DAndThenDisposeInput(ptr);
        }

        public virtual IMatrix GetMatrix(ITensor3D tensor, uint index)
        {
            var segment = new TensorSegmentWrapper(tensor.Segment, index * tensor.MatrixSize, 1, tensor.MatrixSize);
            return CreateMatrix(tensor.RowCount, tensor.ColumnCount, segment);
        }

        public virtual ITensor3D GetTensor(ITensor4D tensor, uint index)
        {
            var segment = new TensorSegmentWrapper(tensor.Segment, index * tensor.TensorSize, 1, tensor.TensorSize);
            return CreateTensor3D(tensor.Depth, tensor.RowCount, tensor.ColumnCount, segment);
        }

        public virtual IMatrix GetNewMatrixFromRows(IMatrix matrix, IEnumerable<uint> rowIndices)
        {
            var ret = CreateMatrixFromRows(rowIndices.Select(matrix.GetRow).ToArray());
            return ret;
        }

        public virtual IMatrix GetNewMatrixFromColumns(IMatrix matrix, IEnumerable<uint> columnIndices)
        {
            var ret = CreateMatrixFromColumns(columnIndices.Select(matrix.GetColumn).ToArray());
            return ret;
        }

        public virtual void AddToEachRow(IMatrix matrix, ITensorSegment2 vector)
        {
            matrix.MapIndexedInPlace((_, k, v) => v + vector[k]);
        }

        public virtual void AddToEachColumn(IMatrix matrix, ITensorSegment2 vector)
        {
            matrix.MapIndexedInPlace((j, _, v) => v + vector[j]);
        }

        public virtual IMatrix FindDistances(IVector[] vectors, IReadOnlyList<IVector> compareTo, DistanceMetric distanceMetric)
        {
            var rows = (uint)compareTo.Count;
            var columns = (uint)vectors.Length;
            var ret = CreateMatrix(rows, columns);

            Parallel.For(0, rows * columns, ind => {
                var i = (uint) (ind % rows);
                var j = (uint) (ind / rows);
                var distance = compareTo[(int)i].FindDistance(vectors[(int)j], distanceMetric);;
                ret[i, j] = distance;
            });

            return ret;
        }

        public ITensor2 CreateTensor(uint[] shape, ITensorSegment2 segment)
        {
            if (shape.Length == 1) {
                if (shape[0] != segment.Size)
                    throw new ArgumentException("Shape does not match segment size");
                return CreateVector(segment);
            }

            if (shape.Length == 2) {
                if(shape[0] * shape[1] != segment.Size)
                    throw new ArgumentException("Shape does not match segment size");
                return CreateMatrix(shape[1], shape[0], segment);
            }

            if (shape.Length == 3) {
                if(shape[0] * shape[1] * shape[2] != segment.Size)
                    throw new ArgumentException("Shape does not match segment size");
                return CreateTensor3D(shape[2], shape[1], shape[0], segment);
            }

            if (shape.Length == 4) {
                if(shape[0] * shape[1] * shape[2] * shape[3] != segment.Size)
                    throw new ArgumentException("Shape does not match segment size");
                return CreateTensor4D(shape[3], shape[2], shape[1], shape[0], segment);
            }

            throw new NotImplementedException();
        }

        public virtual void BindThread()
        {
            // nop
        }
    }
}
