using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using BrightData.Helper;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace BrightData.LinearAlegbra2
{
    public class ComputationUnit : IDisposable
    {
        readonly List<HashSet<IDisposable>> _scope = new();

        public ComputationUnit(
            BrightDataContext context
        )
        {
            Context = context;
            PushScope();
        }

        public BrightDataContext Context { get; }

        public void Dispose()
        {
            foreach (var set in _scope) {
                foreach(var item in set)
                    item.Dispose();
            }
        }

        public void PushScope()
        {
            _scope.Add(new());
        }

        public void PopScope()
        {
            var popped = _scope.Last();
            _scope.RemoveAt(_scope.Count-1);
            foreach(var item in popped)
                item.Dispose();
        }
        internal bool AddToScope(IDisposable obj) => _scope.Last().Add(obj);
        internal bool RemoveFromScope(IDisposable obj) => _scope.Last().Remove(obj);
        public virtual IDisposableTensorSegment CreateSegment(uint size) => new TensorSegment2(MemoryOwner<float>.Allocate((int)size, AllocationMode.Clear));

        public virtual IDisposableTensorSegment Clone(ITensorSegment2 tensor)
        {
            var ret = CreateSegment(tensor.Size);
            tensor.CopyTo(ret);
            return ret;
        }

        // vector creation
        public virtual IVector CreateVector(ITensorSegment2 data) => new Vector2(data, this);
        public IVector CreateVector(uint size) => CreateVector(CreateSegment(size));
        public virtual IVector CreateVector(uint size, Func<uint, float> initializer)
        {
            var segment = CreateSegment(size);
            var array = segment.GetArrayForLocalUseOnly()!;
            for (uint i = 0, len = (uint)array.Length; i < len; i++)
                array[i] = initializer(i);
            return CreateVector(segment);
        }


        // matrix creation
        public virtual IMatrix CreateMatrix(ITensorSegment2 data, uint rowCount, uint columnCount) => new Matrix2(data, rowCount, columnCount, this);
        public IMatrix CreateMatrix(uint rowCount, uint columnCount) => CreateMatrix(CreateSegment(rowCount * columnCount), rowCount, columnCount);
        public virtual IMatrix CreateMatrix(uint rowCount, uint columnCount, Func<uint, uint, float> initializer)
        {
            var segment = CreateSegment(rowCount * columnCount);
            var array = segment.GetArrayForLocalUseOnly()!;
            for (uint i = 0, len = (uint)array.Length; i < len; i++)
                array[i] = initializer(i / columnCount, i % columnCount);
            return CreateMatrix(CreateSegment(rowCount * columnCount), rowCount, columnCount);
        }
        public virtual IMatrix CreateMatrixFromRows(params IVector[] rows)
        {
            var columns = rows[0].Size;
            return CreateMatrix((uint)rows.Length, columns, (j, i) => rows[j][i]);
        }
        public virtual IMatrix CreateMatrixFromColumns(params IVector[] columns)
        {
            var rows = columns[0].Size;
            return CreateMatrix(rows, (uint)columns.Length, (j, i) => columns[i][j]);
        }

        // 3D tensor creation
        public virtual ITensor3D CreateTensor3D(ITensorSegment2 data, uint depth, uint rowCount, uint columnCount) => new Tensor3D2(data, depth, rowCount, columnCount, this);
        public ITensor3D CreateTensor3D(uint depth, uint rowCount, uint columnCount) => CreateTensor3D(CreateSegment(depth * rowCount * columnCount), depth, rowCount, columnCount);
        public ITensor3D CreateTensor3D(params IMatrix[] matrices) => CreateTensor3D(matrices.AsSpan());
        public virtual ITensor3D CreateTensor3D(Span<IMatrix> matrices)
        {
            var first = matrices[0];
            var depth = (uint)matrices.Length;
            var rows = first.RowCount;
            var columns = first.ColumnCount;

            var data = CreateSegment(depth * rows * columns);
            var ret = CreateTensor3D(data, depth, rows, columns);
            var allSame = true;
            for (uint i = 0; i < ret.Depth; i++) {
                using var t = ret.Matrix(i);
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
        public virtual ITensor4D CreateTensor4D(ITensorSegment2 data, uint count, uint depth, uint rowCount, uint columnCount) => new Tensor4D2(data, count, depth, rowCount, columnCount, this);
        public ITensor4D CreateTensor4D(uint count, uint depth, uint rowCount, uint columnCount) => CreateTensor4D(CreateSegment(count * depth * rowCount * columnCount), count, depth, rowCount, columnCount);
        public ITensor4D CreateTensor4D(params ITensor3D[] tensors) => CreateTensor4D(tensors.AsSpan());
        public virtual ITensor4D CreateTensor4D(Span<ITensor3D> tensors)
        {
            var first = tensors[0];
            var count = (uint)tensors.Length;
            var rows = first.RowCount;
            var columns = first.ColumnCount;
            var depth = first.Depth;

            var data = CreateSegment(depth * rows * columns * count);
            var ret = CreateTensor4D(data, count, depth, rows, columns);
            var allSame = true;
            for (uint i = 0; i < ret.Depth; i++) {
                using var t = ret.Tensor(i);
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

        public virtual IMatrix Transpose(IMatrix matrix)
        {
            var columnCount = matrix.ColumnCount;
            var rowCount = matrix.RowCount;
            var ret = CreateMatrix(columnCount, rowCount);
            //for (var i = 0; i < rowCount; i++) {
            //    for(var j = 0; j < columnCount; j++)
            //        ret[j, i] = matrix[i, j];
            //}
            Parallel.For(0, matrix.Segment.Size, ind => {
                var i = (uint)(ind / columnCount);
                var j = (uint)(ind % columnCount);
                ret[j, i] = matrix[i, j];
            });
            return ret;
        }

        public virtual IMatrix Multiply(IMatrix matrix, IMatrix other)
        {
            var rowCount = matrix.RowCount;
            var ret = CreateMatrix(rowCount, other.ColumnCount);
            var columns = other.Columns();
            var rows = matrix.Rows();
            Parallel.For(0, matrix.RowCount * other.ColumnCount, ind => {
                var i = (uint) (ind % rowCount);
                var j = (uint) (ind / rowCount);
                var column = columns[j];
                var row = rows[i];
                var val = row.DotProduct(column);
                ret[i, j] = val;
            });

            // don't need to dispose the wrappers
            return ret;
        }

        public virtual IMatrix TransposeSecondAndMultiply(IMatrix matrix, IMatrix other)
        {
            using var transpose = Transpose(other);
            return Multiply(matrix, transpose);
        }

        public virtual IMatrix TransposeFirstAndMultiply(IMatrix matrix, IMatrix other)
        {
            using var transpose = Transpose(matrix);
            return Multiply(transpose, other);
        }

        public virtual IVector GetDiagonal(IMatrix matrix)
        {
            if(matrix.RowCount != matrix.ColumnCount)
                throw new Exception("Diagonal can only be found from square matrices");
            return CreateVector(matrix.RowCount, i => matrix[i, i]);
        }

        public virtual IVector RowSums(IMatrix matrix)
        {
            var rows = matrix.Rows();
            return CreateVector(matrix.RowCount, i => rows[i].Sum());
        }

        public virtual IVector ColumnSums(IMatrix matrix)
        {
            var columns = matrix.Columns();
            return CreateVector(matrix.ColumnCount, i => columns[i].Sum());
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
            try {
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
                    CreateTensor3D(ptr),
                    indexList != null ? CreateTensor3D(indexList) : null
                );
            }
            finally {
                foreach(var item in ptr)
                    item?.Dispose();
                if (indexList is not null) {
                    foreach(var item in indexList)
                        item?.Dispose();
                }
            }
        }

        public virtual ITensor3D ReverseMaxPool(ITensor3D tensor, ITensor3D indices, uint outputRows, uint outputColumns, uint filterWidth, uint filterHeight, uint xStride, uint yStride)
        {
            using var matrixList = SpanOwner<IMatrix>.Allocate((int)tensor.Depth, AllocationMode.Clear);
            var ptr = matrixList.Span;
            try {
                for (uint k = 0; k < tensor.Depth; k++) {
                    using var source = tensor.Matrix(k);
                    var sourceRows = source.RowCount;
                    var sourceColumns = source.ColumnCount;
                    using var index = indices.Matrix(k);
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

                return CreateTensor3D(ptr);
            }
            finally {
                foreach(var item in ptr)
                    item?.Dispose();
            }
        }

        public virtual ITensor3D ReverseIm2Col(ITensor3D tensor, IMatrix filter, uint outputRows, uint outputColumns, uint outputDepth, uint filterWidth, uint filterHeight, uint xStride, uint yStride)
        {
            var convolutions = ConvolutionHelper.Default(outputColumns, outputRows, filterWidth, filterHeight, xStride, yStride);
            using var output = SpanOwner<IMatrix>.Allocate((int)outputDepth, AllocationMode.Clear);
            var ptr = output.Span;
            for (var i = 0; i < outputDepth; i++)
                ptr[i] = CreateMatrix(outputRows, outputColumns);

            try {
                for (uint k = 0; k < tensor.Depth; k++) {
                    using var slice = tensor.Matrix(k);
                    var filters = filter.Column(k).Split(outputDepth).ToArray();

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

                return CreateTensor3D(ptr);
            }
            finally {
                foreach(var item in ptr)
                    item?.Dispose();
            }
        }

        public virtual IMatrix CombineDepthSlices(ITensor3D tensor)
        {
            var ret = CreateMatrix(tensor.RowCount, tensor.ColumnCount);
            ret.Clear();

            for (uint i = 0; i < tensor.Depth; i++) {
                using var matrix = tensor.Matrix(i);
                ret.AddInPlace(matrix);
            }

            return ret;
        }

        public virtual ITensor3D Multiply(ITensor3D tensor, IMatrix other)
        {
            using var ret = SpanOwner<IMatrix>.Allocate((int)tensor.Depth, AllocationMode.Clear);
            var ptr = ret.Span;
            try {
                for (uint i = 0; i < tensor.Depth; i++) {
                    using var matrix = tensor.Matrix(i);
                    ptr[(int)i] = matrix.Multiply(other);
                }
                return CreateTensor3D(ptr);
            }
            finally {
                foreach(var item in ptr)
                    item?.Dispose();
            }
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
            try {
                for (uint i = 0; i < other.Count; i++) {
                    using var item = other.Tensor(i);
                    using var multiplyWith = item.Reshape(other.MatrixSize, other.Count);
                    using var slice = tensor.Matrix(i);
                    ptr[(int)i] = slice.TransposeThisAndMultiply(multiplyWith);
                }

                return CreateTensor3D(ptr);
            }
            finally {
                foreach(var item in ptr)
                    item?.Dispose();
            }
        }

        public virtual ITensor4D AddPadding(ITensor4D tensor, uint padding)
        {
            using var ret = SpanOwner<ITensor3D>.Allocate((int)tensor.Count, AllocationMode.Clear);
            var ptr = ret.Span;
            try {
                for (uint i = 0; i < tensor.Count; i++) {
                    using var subTensor = tensor.Tensor(i);
                    ptr[(int)i] = subTensor.AddPadding(padding);
                }

                return CreateTensor4D(ptr);
            }
            finally {
                foreach(var item in ptr)
                    item?.Dispose();
            }
        }

        public virtual ITensor4D RemovePadding(ITensor4D tensor, uint padding)
        {
            using var ret = SpanOwner<ITensor3D>.Allocate((int)tensor.Count, AllocationMode.Clear);
            var ptr = ret.Span;
            try {
                for (uint i = 0; i < tensor.Count; i++) {
                    using var subTensor = tensor.Tensor(i);
                    ptr[(int)i] = subTensor.RemovePadding(padding);
                }

                return CreateTensor4D(ptr);
            }
            finally {
                foreach(var item in ptr)
                    item?.Dispose();
            }
        }

        public virtual (ITensor4D Result, ITensor4D? Indices) MaxPool(ITensor4D tensor, uint filterWidth, uint filterHeight, uint xStride, uint yStride, bool saveIndices)
        {
            var indexList = saveIndices 
                ? new ITensor3D[tensor.Count]
                : null;
            using var ret = SpanOwner<ITensor3D>.Allocate((int)tensor.Count, AllocationMode.Clear);
            var ptr = ret.Span;
            try {
                for (uint i = 0; i < tensor.Count; i++) {
                    using var subTensor = tensor.Tensor(i);
                    var (result, indices) = subTensor.MaxPool(filterWidth, filterHeight, xStride, yStride, saveIndices);
                    ptr[(int)i] = result;
                    if (indexList != null && indices != null)
                        indexList[i] = indices;
                }

                return (CreateTensor4D(ptr), indexList != null ? CreateTensor4D(indexList) : null);
            }
            finally {
                foreach(var item in ptr)
                    item?.Dispose();
                if (indexList is not null) {
                    foreach(var item in indexList)
                        item?.Dispose();
                }
            }
        }

        public virtual ITensor4D ReverseMaxPool(ITensor4D tensor, ITensor4D indices, uint outputRows, uint outputColumns, uint filterWidth, uint filterHeight, uint xStride, uint yStride)
        {
            using var ret = SpanOwner<ITensor3D>.Allocate((int)tensor.Count, AllocationMode.Clear);
            var ptr = ret.Span;
            try {
                for (uint i = 0; i < tensor.Count; i++) {
                    using var subTensor = tensor.Tensor(i);
                    using var indexTensor = indices.Tensor(i);
                    var result = subTensor.ReverseMaxPool(indexTensor, outputRows, outputColumns, filterWidth, filterHeight, xStride, yStride);
                    ptr[(int)i] = result;
                }

                return CreateTensor4D(ptr);
            }
            finally {
                foreach (var item in ptr)
                    item?.Dispose();
            }
        }

        public virtual ITensor3D Im2Col(ITensor4D tensor, uint filterWidth, uint filterHeight, uint xStride, uint yStride)
        {
            var ret = SpanOwner<IMatrix>.Allocate((int)tensor.Count, AllocationMode.Clear);
            var ptr = ret.Span;
            try {
                for (uint i = 0; i < tensor.Count; i++) {
                    using var subTensor = tensor.Tensor(i);
                    ptr[(int)i] = subTensor.Im2Col(filterWidth, filterHeight, xStride, yStride);
                }

                return CreateTensor3D(ptr);
            }
            finally {
                foreach(var item in ptr)
                    item?.Dispose();
            }
        }

        public virtual ITensor4D ReverseIm2Col(ITensor4D tensor, IMatrix filter, uint outputRows, uint outputColumns, uint outputDepth, uint filterWidth, uint filterHeight, uint xStride, uint yStride)
        {
            var ret = SpanOwner<ITensor3D>.Allocate((int)tensor.Count, AllocationMode.Clear);
            var ptr = ret.Span;
            try {
                for (uint i = 0; i < tensor.Count; i++) {
                    using var subTensor = tensor.Tensor(i);
                    ptr[(int)i] = subTensor.ReverseIm2Col(filter, outputRows, outputColumns, outputDepth, filterWidth, filterHeight, xStride, yStride);
                }

                return CreateTensor4D(ptr);
            }
            finally {
                foreach(var item in ptr)
                    item?.Dispose();
            }
        }

        public virtual IMatrix GetMatrix(ITensor3D tensor, uint index)
        {
            var segment = new TensorSegmentWrapper2(tensor.Segment, index * tensor.MatrixSize, 1, tensor.MatrixSize);
            return CreateMatrix(segment, tensor.RowCount, tensor.ColumnCount);
        }

        public virtual ITensor3D GetTensor(ITensor4D tensor, uint index)
        {
            var segment = new TensorSegmentWrapper2(tensor.Segment, index * tensor.TensorSize, 1, tensor.TensorSize);
            return CreateTensor3D(segment, tensor.Depth, tensor.RowCount, tensor.ColumnCount);
        }
    }
}
