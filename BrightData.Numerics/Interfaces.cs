using System;
using System.Collections.Generic;
using System.Text;

namespace BrightData.Numerics
{
    public interface IComputableVector : IDisposable
    {
        int Size { get; }
        float this[int index] { get; set; }
        float[] ToArray();
        float[] GetInternalArray();
        IComputableVector Add(IComputableVector vector);
        void AddInPlace(IComputableVector vector, float coefficient1 = 1.0f, float coefficient2 = 1.0f);
        float L1Norm();
        float L2Norm();
        int MaximumIndex();
        int MinimumIndex();
        void MultiplyInPlace(float scalar);
        IComputableVector Subtract(IComputableVector vector);
        void SubtractInPlace(IComputableVector vector, float coefficient1 = 1.0f, float coefficient2 = 1.0f);
        IComputableMatrix ReshapeAsSingleColumnMatrix();
        IComputableMatrix ReshapeAsSingleRowMatrix();
        Vector<float> ToVector(IBrightDataContext context);
        IComputableVector PointwiseMultiply(IComputableVector vector);
        float DotProduct(IComputableVector vector);
        IComputableVector GetNewVectorFromIndexes(IReadOnlyList<int> indexes);
        IComputableVector Abs();
        IComputableVector Sqrt();
        IComputableVector Clone();
        float EuclideanDistance(IComputableVector vector);
        float CosineDistance(IComputableVector vector);
        float ManhattanDistance(IComputableVector vector);
        float MeanSquaredDistance(IComputableVector vector);
        float SquaredEuclidean(IComputableVector vector);
        void CopyFrom(IComputableVector vector);
        (float Min, float Max) GetMinMax();
        float Average();
        float StdDev(float? mean);
        IComputableVector Softmax();
        IComputableMatrix SoftmaxDerivative();
        IComputableVector Log();
        IComputableVector Sigmoid();
        void AddInPlace(float scalar);
        IComputableMatrix ReshapeAsMatrix(int rows, int columns);
        IComputable3DTensor ReshapeAs3DTensor(int rows, int columns, int depth);
        IComputable4DTensor ReshapeAs4DTensor(int rows, int columns, int depth, int count);
        IReadOnlyList<IComputableVector> Split(int blockCount);
        void RotateInPlace(int blockCount);
        IComputableVector Reverse();
        bool IsEntirelyFinite();

    }

    public interface IComputableMatrix : IDisposable
    {
        int RowCount { get; }
        int ColumnCount { get; }
        float this[int rowY, int columnX] { get; set; }
        float[] GetInternalArray();
        Matrix<float> ToMatrix(IBrightDataContext context);
        IComputableVector Column(int index);
        IComputableMatrix Map(Func<float, float> mutator);
        IComputableMatrix MapIndexed(Func<int, int, float, float> mutator);
        IComputableMatrix Multiply(IComputableMatrix matrix);
        IComputableMatrix PointwiseMultiply(IComputableMatrix matrix);
        IComputableVector RowSums();
        IComputableVector ColumnSums();
        IComputableMatrix Add(IComputableMatrix matrix);
        IComputableMatrix Subtract(IComputableMatrix matrix);
        IComputableMatrix TransposeAndMultiply(IComputableMatrix matrix);
        IComputableMatrix TransposeThisAndMultiply(IComputableMatrix matrix);
        IComputableMatrix Transpose();
        void MultiplyInPlace(float scalar);
        void AddInPlace(IComputableMatrix matrix, float coefficient1 = 1.0f, float coefficient2 = 1.0f);
        void SubtractInPlace(IComputableMatrix matrix, float coefficient1 = 1.0f, float coefficient2 = 1.0f);
        IComputableMatrix ReluActivation();
        IComputableMatrix ReluDerivative();
        IComputableMatrix LeakyReluActivation();
        IComputableMatrix LeakyReluDerivative();
        IComputableMatrix SigmoidActivation();
        IComputableMatrix SigmoidDerivative();
        IComputableMatrix TanhActivation();
        IComputableMatrix TanhDerivative();
        IComputableMatrix SoftmaxActivation();
        void AddToEachRow(IComputableVector vector);
        void AddToEachColumn(IComputableVector vector);
        IComputableVector Row(int index);
        IEnumerable<IComputableVector> Rows { get; }
        IEnumerable<IComputableVector> Columns { get; }
        IComputableMatrix GetNewMatrixFromRows(IReadOnlyList<int> rowIndexes);
        IComputableMatrix GetNewMatrixFromColumns(IReadOnlyList<int> columnIndexes);
        void ClearRows(IReadOnlyList<int> indexes);
        void ClearColumns(IReadOnlyList<int> indexes);
        IComputableMatrix Clone();
        void Clear();
        IComputableMatrix ConcatColumns(IComputableMatrix bottom);
        IComputableMatrix ConcatRows(IComputableMatrix right);
        (IComputableMatrix Left, IComputableMatrix Right) SplitAtColumn(int columnIndex);
        (IComputableMatrix Top, IComputableMatrix Bottom) SplitAtRow(int rowIndex);
        IComputableMatrix Sqrt(float valueAdjustment = 1e-8f);
        IComputableMatrix PointwiseDivide(IComputableMatrix matrix);
        void L1Regularisation(float coefficient);
        void Constrain(float min, float max);
        IComputableVector ColumnL2Norm();
        IComputableVector RowL2Norm();
        void PointwiseDivideRows(IComputableVector vector);
        void PointwiseDivideColumns(IComputableVector vector);
        IComputableVector Diagonal();
        IComputableMatrix Pow(float power);
        IComputableVector GetRowSegment(int index, int columnIndex, int length);
        IComputableVector GetColumnSegment(int columnIndex, int rowIndex, int length);
        IComputableMatrix Multiply(IComputableVector vector);
        (IComputableMatrix U, IComputableVector S, IComputableMatrix VT) Svd();
        IComputableVector ReshapeAsVector();
        IComputable3DTensor ReshapeAs3DTensor(int rows, int columns);
        IComputable4DTensor ReshapeAs4DTensor(int rows, int columns, int depth);
        IReadOnlyList<IComputableVector> ColumnVectors();
        IReadOnlyList<IComputableVector> RowVectors();
        string AsXml { get; }
    }

    public interface IComputable3DTensor : IDisposable
    {
        int RowCount { get; }
        int ColumnCount { get; }
        int Depth { get; }
        float this[int row, int column, int depth] { get; set; }
        IComputableMatrix GetMatrixAt(int depth);
        float[] GetInternalArray();
        IComputable3DTensor AddPadding(int padding);
        IComputable3DTensor RemovePadding(int padding);
        IComputableVector ReshapeAsVector();
        IComputableMatrix ReshapeAsMatrix();
        IComputable4DTensor ReshapeAs4DTensor(int rows, int columns);

        (IComputable3DTensor Result, IComputable3DTensor Indices) MaxPool(
            int filterWidth,
            int filterHeight,
            int xStride,
            int yStride,
            bool saveIndices
        );

        IComputable3DTensor ReverseMaxPool(
            IComputable3DTensor indexList,
            int outputRows,
            int outputColumns,
            int filterWidth,
            int filterHeight,
            int xStride,
            int yStride
        );
        IComputableMatrix Im2Col(int filterWidth, int filterHeight, int xStride, int yStride);
        IComputable3DTensor ReverseIm2Col(
            IComputableMatrix filterMatrix,
            int outputRows,
            int outputColumns,
            int outputDepth,
            int filterWidth,
            int filterHeight,
            int xStride,
            int yStride
        );
        IComputableMatrix CombineDepthSlices();
        void AddInPlace(IComputable3DTensor tensor);
        IComputable3DTensor Multiply(IComputableMatrix matrix);
        void AddToEachRow(IComputableVector vector);
        IComputable3DTensor TransposeThisAndMultiply(IComputable4DTensor tensor);
        string AsXml { get; }
    }

    public interface IComputable4DTensor : IDisposable
    {
        int RowCount { get; }
        int ColumnCount { get; }
        int Depth { get; }
        int Count { get; }
        float[] GetInternalArray();
        IComputable3DTensor GetTensorAt(int index);
        IComputable4DTensor AddPadding(int padding);
        IComputable4DTensor RemovePadding(int padding);
        (IComputable4DTensor Result, IComputable4DTensor Indices) MaxPool(
            int filterWidth,
            int filterHeight,
            int xStride,
            int yStride,
            bool saveIndices
        );
        IComputable4DTensor ReverseMaxPool(
            IComputable4DTensor indices,
            int outputRows,
            int outputColumns,
            int filterWidth,
            int filterHeight,
            int xStride,
            int yStride
        );
        IComputable3DTensor Im2Col(int filterWidth, int filterHeight, int xStride, int yStride);
        IComputable4DTensor ReverseIm2Col(
            IComputableMatrix filters,
            int outputRows,
            int outputColumns,
            int outputDepth,
            int filterWidth,
            int filterHeight,
            int xStride,
            int yStride
        );
        IComputableMatrix ReshapeAsMatrix();
        IComputableVector ReshapeAsVector();
        IComputableVector ColumnSums();
        float this[int row, int column, int depth, int index] { get; set; }
        string AsXml { get; }
    }

    public interface IComputableFactory
    {
        IComputableVector Create(Vector<float> vector);
        IComputableVector Create(int size, Func<int, float> initializer);

        IComputableMatrix Create(Matrix<float> matrix);
        IComputableMatrix Create(int rows, int columns, Func<int, int, float> initializer);

        IComputable3DTensor Create(Tensor3D<float> tensor);
        IComputable4DTensor Create(Tensor4D<float> tensor);
    }
}
