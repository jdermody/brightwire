using System;
using System.Collections.Generic;
using BrightData.LinearAlgebra;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace BrightData
{
    /// <summary>
    /// Gives access to a linear algebra provider
    /// </summary>
    public interface IHaveLinearAlgebraProvider
    {
        /// <summary>
        /// Linear algebra provider
        /// </summary>
        LinearAlgebraProvider LinearAlgebraProvider { get; }
    }

    /// <summary>
    /// Indicates that the type can set a linear algebra provider
    /// </summary>
    public interface ISetLinearAlgebraProvider
    {
        /// <summary>
        /// Linear algebra provider
        /// </summary>
        LinearAlgebraProvider LinearAlgebraProvider { set; }
    }

    /// <summary>
    /// Distance metrics
    /// </summary>
    public enum DistanceMetric
    {
        /// <summary>
        /// Euclidean Distance
        /// </summary>
        Euclidean,

        /// <summary>
        /// Cosine Distance Metric
        /// </summary>
        Cosine,

        /// <summary>
        /// Manhattan Distance
        /// </summary>
        Manhattan,

        /// <summary>
        /// Means Square Error
        /// </summary>
        MeanSquared,

        /// <summary>
        /// Square Euclidean
        /// </summary>
        SquaredEuclidean
    }

    public interface ITensorSegment : ICountReferences, IDisposable, IHaveSize
    {
        string SegmentType { get; }
        float this[int index] { get; set; }
        float this[uint index] { get; set; }
        float this[long index] { get; set; }
        float this[ulong index] { get; set; }
        IEnumerable<float> Values { get; }
        float[]? GetArrayForLocalUseOnly();
        float[] ToNewArray();
        void CopyFrom(ReadOnlySpan<float> span);
        void CopyTo(ITensorSegment segment);
        void CopyTo(Span<float> destination);
        unsafe void CopyTo(float* destination, int offset, int stride, int count);
        void Clear();
        ReadOnlySpan<float> GetSpan(ref SpanOwner<float> temp, out bool wasTempUsed);
        ReadOnlySpan<float> GetSpan();
        (float[] Array, uint Offset, uint Stride) GetUnderlyingArray();
    }

    public interface IHaveTensorSegment
    {
        ITensorSegment Segment { get; }
    }

    public interface IVectorInfo : IAmSerializable, IHaveSpan, IHaveSize, IHaveTensorSegment
    {
        float this[int index] { get; }
        float this[uint index] { get; }
        IVector Create(LinearAlgebraProvider lap);
    }

    public interface IMatrixInfo : IAmSerializable, IHaveSpan, IHaveSize, IHaveTensorSegment
    {
        uint RowCount { get; }
        uint ColumnCount { get; }
        float this[int rowY, int columnX] { get; }
        float this[uint rowY, uint columnX] { get; }
        IMatrix Create(LinearAlgebraProvider lap);
        IVectorInfo GetRow(uint rowIndex);
        IVectorInfo GetColumn(uint columnIndex);
        IVectorInfo[] AllRows();
        IVectorInfo[] AllColumns();
        IVectorInfo[] CopyAllRows();
        IVectorInfo[] CopyAllColumns();
    }

    public interface ITensor3DInfo : IAmSerializable, IHaveSpan, IHaveSize, IHaveTensorSegment
    {
        uint Depth { get; }
        uint RowCount { get; }
        uint ColumnCount { get; }
        uint MatrixSize { get; }
        float this[int depth, int rowY, int columnX] { get; }
        float this[uint depth, uint rowY, uint columnX] { get; }
        ITensor3D Create(LinearAlgebraProvider lap);
    }
    public interface ITensor4DInfo : IAmSerializable, IHaveSpan, IHaveSize, IHaveTensorSegment
    {
        uint Count { get; }
        uint Depth { get; }
        uint RowCount { get; }
        uint ColumnCount { get; }
        uint MatrixSize { get; }
        uint TensorSize { get; }
        float this[int count, int depth, int rowY, int columnX] { get; }
        float this[uint count, uint depth, uint rowY, uint columnX] { get; }
        ITensor4D Create(LinearAlgebraProvider lap);
    }

    public interface ITensor : IDisposable, IAmSerializable, IHaveSpan, IHaveSize, IHaveTensorSegment, IHaveLinearAlgebraProvider
    {
        BrightDataContext Context { get; }
        ITensorSegment Segment { get; }
        IVector Reshape();
        IMatrix Reshape(uint? rows, uint? columns);
        ITensor3D Reshape(uint? depth, uint? rows, uint? columns);
        ITensor4D Reshape(uint? count, uint? depth, uint? rows, uint? columns);
        void Clear();
        ITensor Clone();
        uint TotalSize { get; }
        uint[] Shape { get; }
        void AddInPlace(ITensor tensor);
        void AddInPlace(ITensor tensor, float coefficient1, float coefficient2);
        void AddInPlace(float scalar);
        void MultiplyInPlace(float scalar);
        void SubtractInPlace(ITensor tensor);
        void SubtractInPlace(ITensor tensor, float coefficient1, float coefficient2);
        void PointwiseMultiplyInPlace(ITensor tensor);
        void PointwiseDivideInPlace(ITensor tensor);
        float DotProduct(ITensor tensor);
        uint? Search(float value);
        void ConstrainInPlace(float? minValue, float? maxValue);
        float Average();
        float L1Norm();
        float L2Norm();
        (float Min, float Max, uint MinIndex, uint MaxIndex) GetMinAndMaxValues();
        uint GetMinIndex();
        uint GetMaxIndex();
        float GetMin();
        float GetMax();
        bool IsEntirelyFinite();
        float CosineDistance(ITensor other);
        float EuclideanDistance(ITensor other);
        float MeanSquaredDistance(ITensor other);
        float SquaredEuclideanDistance(ITensor other);
        float ManhattanDistance(ITensor other);
        float StdDev(float? mean);
        IMatrix SoftmaxDerivative();
        void RoundInPlace(float lower, float upper, float? mid);
        void MapInPlace(Func<float, float> mutator);
        void L1Regularisation(float coefficient);
        float Sum();
    }

    public interface ITensor<out T> : ITensor
        where T: ITensor
    {
        new T Clone();
        T Add(ITensor tensor);
        T Add(ITensor tensor, float coefficient1, float coefficient2);
        T Add(float scalar);
        T Multiply(float scalar);
        T Subtract(ITensor tensor);
        T Subtract(ITensor tensor, float coefficient1, float coefficient2);
        T PointwiseMultiply(ITensor tensor);
        T PointwiseDivide(ITensor tensor);
        T Sqrt();
        T Reverse();
        IEnumerable<T> Split(uint blockCount);
        T Abs();
        T Log();
        T Exp();
        T Squared();
        T Sigmoid();
        T SigmoidDerivative();
        T Tanh();
        T TanhDerivative();
        T Relu();
        T ReluDerivative();
        T LeakyRelu();
        T LeakyReluDerivative();
        T Softmax();
        T Pow(float power);
        T CherryPick(uint[] indices);
        T Map(Func<float, float> mutator);
    }

    public interface IVector : ITensor<IVector>, IVectorInfo
    {
        new float this[int index] { get; set; }
        new float this[uint index] { get; set; }
        float this[long index] { get; set; }
        float this[ulong index] { get; set; }
        IVector MapIndexed(Func<uint, float, float> mutator);
        void MapIndexedInPlace(Func<uint, float, float> mutator);
    }

    public interface IMatrix : ITensor<IMatrix>, IMatrixInfo
    {
        new float this[int rowY, int columnX] { get; set; }
        new float this[uint rowY, uint columnX] { get; set; }
        float this[long rowY, long columnX] { get; set; }
        float this[ulong rowY, ulong columnX] { get; set; }
        ReadOnlySpan<float> GetRowSpan(uint rowIndex, ref SpanOwner<float> temp);
        ReadOnlySpan<float> GetColumnSpan(uint columnIndex);
        IVector GetRowVector(uint index);
        IVector GetColumnVector(uint index);
        IMatrix Transpose();
        IMatrix Multiply(IMatrix other);
        IMatrix TransposeAndMultiply(IMatrix other);
        IMatrix TransposeThisAndMultiply(IMatrix other);
        IVector GetDiagonal();
        IVector RowSums();
        IVector ColumnSums();
        IMatrix Multiply(IVector vector);
        (IMatrix Left, IMatrix Right) SplitAtColumn(uint columnIndex);
        (IMatrix Top, IMatrix Bottom) SplitAtRow(uint rowIndex);
        IMatrix ConcatColumns(IMatrix bottom);
        IMatrix ConcatRows(IMatrix right);
        IMatrix MapIndexed(Func<uint, uint, float, float> mutator);
        void MapIndexedInPlace(Func<uint, uint, float, float> mutator);
        (IMatrix U, IVector S, IMatrix VT) Svd();
        IMatrix GetNewMatrixFromRows(IEnumerable<uint> rowIndices);
        IMatrix GetNewMatrixFromColumns(IEnumerable<uint> columnIndices);
        void AddToEachRow(ITensorSegment segment);
        void AddToEachColumn(ITensorSegment segment);
    }

    public interface IMatrixSegments
    {
        TensorSegmentWrapper Row(uint index, ITensorSegment? segment = null);
        TensorSegmentWrapper Column(uint index, ITensorSegment? segment = null);
    }

    public interface ITensor3D : ITensor<ITensor3D>, ITensor3DInfo
    {
        new float this[int depth, int rowY, int columnX] { get; set; }
        new float this[uint depth, uint rowY, uint columnX] { get; set; }
        float this[long depth, long rowY, long columnX] { get; set; }
        float this[ulong depth, ulong rowY, ulong columnX] { get; set; }
        IMatrix GetMatrix(uint index);
        ITensor3D AddPadding(uint padding);
        ITensor3D RemovePadding(uint padding);
        IMatrix Im2Col(uint filterWidth, uint filterHeight, uint xStride, uint yStride);
        (ITensor3D Result, ITensor3D? Indices) MaxPool(uint filterWidth, uint filterHeight, uint xStride, uint yStride, bool saveIndices);
        ITensor3D ReverseMaxPool(ITensor3D indices, uint outputRows, uint outputColumns, uint filterWidth, uint filterHeight, uint xStride, uint yStride);
        ITensor3D ReverseIm2Col(IMatrix filter, uint outputRows, uint outputColumns, uint outputDepth, uint filterWidth, uint filterHeight, uint xStride, uint yStride);
        IMatrix CombineDepthSlices();
        ITensor3D Multiply(IMatrix matrix);
        void AddToEachRow(IVector vector);
        ITensor3D TransposeThisAndMultiply(ITensor4D other);
    }

    public interface ITensor4D : ITensor<ITensor4D>, ITensor4DInfo
    {
        new float this[int count, int depth, int rowY, int columnX] { get; set; }
        new float this[uint count, uint depth, uint rowY, uint columnX] { get; set; }
        float this[long count, long depth, long rowY, long columnX] { get; set; }
        float this[ulong count, ulong depth, ulong rowY, ulong columnX] { get; set; }
        ITensor3D GetTensor(uint index);
        ITensor4D AddPadding(uint padding);
        ITensor4D RemovePadding(uint padding);
        (ITensor4D Result, ITensor4D? Indices) MaxPool(uint filterWidth, uint filterHeight, uint xStride, uint yStride, bool saveIndices);
        ITensor4D ReverseMaxPool(ITensor4D indices, uint outputRows, uint outputColumns, uint filterWidth, uint filterHeight, uint xStride, uint yStride);
        ITensor3D Im2Col(uint filterWidth, uint filterHeight, uint xStride, uint yStride);
        ITensor4D ReverseIm2Col(IMatrix filter, uint outputRows, uint outputColumns, uint outputDepth, uint filterWidth, uint filterHeight, uint xStride, uint yStride);
        IVector ColumnSums();
    }
}
