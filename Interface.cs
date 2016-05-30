using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icbld.BrightWire
{
    public interface ILinearAlgebraProvider : IDisposable
    {
        IVector Create(float[] data);
        IVector Create(IEnumerable<float> data);
        IVector Create(int length, float value);
        IVector Create(int length, Func<int, float> init);
        IVector Create(IIndexableVector vector);

        IMatrix Create(int rows, int columns, Func<int, int, float> init);
        IMatrix Create(int rows, int columns, float value);
        IMatrix Create(int rows, int columns, IList<IIndexableVector> vectorData);
        IMatrix Create(IIndexableMatrix matrix);

        IMatrix CreateMatrix(BinaryReader reader);
        IVector CreateVector(BinaryReader reader);

        IIndexableVector CreateIndexable(int length);
        IIndexableVector CreateIndexable(int length, Func<int, float> init);
        IIndexableMatrix CreateIndexable(int rows, int columns);
        IIndexableMatrix CreateIndexable(int rows, int columns, Func<int, int, float> init);
        IIndexableMatrix CreateIndexable(int rows, int columns, float value);
    }

    public enum NormalisationType
    {
        Standard,
        Manhattan,
        Euclidean,
        Infinity,
        FeatureScale,
    }

    public interface IVector : IDisposable
    {
        IMatrix ToColumnMatrix(int numCols = 1);
        IMatrix ToRowMatrix(int numRows = 1);
        int Count { get; }
        IVector Add(IVector vector);
        IVector Subtract(IVector vector);
        float L2Norm();
        int MaximumIndex();
        int MinimumIndex();
        object WrappedObject { get; }
        void Multiply(float scalar);
        void AddInPlace(IVector vector, float coefficient1 = 1.0f, float coefficient2 = 1.0f);
        void SubtractInPlace(IVector vector, float coefficient1 = 1.0f, float coefficient2 = 1.0f);
        void WriteTo(BinaryWriter writer);
        void ReadFrom(BinaryReader reader);
        IIndexableVector AsIndexable();
        IVector PointwiseMultiply(IVector vector);
        float DotProduct(IVector vector);
        IVector GetNewVectorFromIndexes(int[] indexes);
        IVector Clone();
        IVector Sqrt();
        void CopyFrom(IVector vector);
        float EuclideanDistance(IVector vector);
        float CosineDistance(IVector vector);
        float ManhattanDistance(IVector vector);
        float MeanSquaredDistance(IVector vector);
        float SquaredEuclidean(IVector vector);
    }

    public interface IIndexableVector : IVector
    {
        float this[int index] { get; set; }
        IEnumerable<float> Data { get; }
        float[] ToArray();
        IIndexableVector Softmax();
        IIndexableVector Softmax2();
        void Normalise(NormalisationType type);
    }

    public enum MatrixGrouping
    {
        ByRow,
        ByColumn,
    }

    public interface IMatrix : IDisposable
    {
        IMatrix Multiply(IMatrix matrix);
        int ColumnCount { get; }
        int RowCount { get; }
        IVector Column(int index);
        IVector Diagonal();
        IVector Row(int index);
        IMatrix Add(IMatrix matrix);
        IMatrix Subtract(IMatrix matrix);
        IMatrix PointwiseMultiply(IMatrix matrix);
        IMatrix TransposeAndMultiply(IMatrix matrix);
        IMatrix TransposeThisAndMultiply(IMatrix matrix);
        IVector RowSums(float coefficient = 1f);
        IVector ColumnSums(float coefficient = 1f);
        object WrappedObject { get; }
        IMatrix Transpose();
        void Multiply(float scalar);
        void AddInPlace(IMatrix matrix, float coefficient1 = 1.0f, float coefficient2 = 1.0f);
        void SubtractInPlace(IMatrix matrix, float coefficient1 = 1.0f, float coefficient2 = 1.0f);
        IMatrix SigmoidActivation();
        IMatrix SigmoidDerivative();
        IMatrix TanhActivation();
        IMatrix TanhDerivative();
        void AddToEachRow(IVector vector);
        void AddToEachColumn(IVector vector);
        void WriteTo(BinaryWriter writer);
        void ReadFrom(BinaryReader reader);
        IIndexableMatrix AsIndexable();
        IMatrix GetNewMatrixFromRows(int[] rowIndexes);
        IMatrix GetNewMatrixFromColumns(int[] columnIndexes);
        void ClearRows(int[] indexes);
        void ClearColumns(int[] indexes);
        IMatrix ReluActivation();
        IMatrix ReluDerivative();
        IMatrix LeakyReluActivation();
        IMatrix LeakyReluDerivative();
        IMatrix Clone();
        void Clear();
        IMatrix Sqrt(float valueAdjustment = 0);
        IMatrix Pow(float power);
        IMatrix PointwiseDivide(IMatrix matrix);
        void L1Regularisation(float coefficient);
        IVector ColumnL2Norm();
        IVector RowL2Norm();
        void PointwiseDivideRows(IVector vector);
        void PointwiseDivideColumns(IVector vector);
        void Constrain(float min, float max);
        void UpdateRow(int index, IIndexableVector vector, int columnIndex);
        void UpdateColumn(int index, IIndexableVector vector, int rowIndex);
        IVector GetRowSegment(int index, int columnIndex, int length);
        IVector GetColumnSegment(int index, int rowIndex, int length);
        IMatrix ConcatColumns(IMatrix bottom);
        IMatrix ConcatRows(IMatrix right);
        Tuple<IMatrix, IMatrix> SplitRows(int position);
        Tuple<IMatrix, IMatrix> SplitColumns(int position);
    }

    public interface IIndexableMatrix : IMatrix
    {
        float this[int row, int column] { get; set; }
        IEnumerable<IIndexableVector> Rows { get; }
        IEnumerable<IIndexableVector> Columns { get; }
        IEnumerable<float> Values { get; }
        void Normalise(MatrixGrouping group, NormalisationType type);
        IIndexableMatrix Map(Func<float, float> mutator);
        IIndexableMatrix MapIndexed(Func<int, int, float, float> mutator);
    }

    public interface ICostFunction
    {
        float Calculate(IIndexableVector output, IIndexableVector expectedOutput);
    }
}
