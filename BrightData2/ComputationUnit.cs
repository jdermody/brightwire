using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightData.Helper;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace BrightData2
{
    public class ComputationUnit : IDisposable
    {
        readonly HashSet<IDisposable> _disposable = new();

        public ComputationUnit(
            BrightDataContext2 context
        )
        {
            Context = context;
        }

        public BrightDataContext2 Context { get; }

        public void Dispose()
        {
            foreach(var item in _disposable)
                item.Dispose();
        }

        internal bool AddToScope(IDisposable obj) => _disposable.Add(obj);
        internal bool RemoveFromScope(IDisposable obj) => _disposable.Remove(obj);
        public virtual IDisposableTensorSegment CreateSegment(uint size) => new TensorSegment2(MemoryOwner<float>.Allocate((int)size));

        public virtual IDisposableTensorSegment Clone(ITensorSegment2 tensor)
        {
            var ret = CreateSegment(tensor.Size);
            ret.CopyFrom(tensor.GetSpan());
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
            var ret = CreateMatrix(columnCount, matrix.RowCount);
            Parallel.For(0, matrix.Segment.Size, ind => {
                var j = (uint)(ind / columnCount);
                var i = (uint)(ind % columnCount);
                ret[i, j] = matrix[j, i];
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
                segment.CopyFrom(ret.GetSpan());
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
                segment.CopyFrom(ret.GetSpan());
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
    }
}
