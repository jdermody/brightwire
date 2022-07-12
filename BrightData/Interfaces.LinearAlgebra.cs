using BrightData.LinearAlgebra;

namespace BrightData
{
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

    public interface IHaveTensorSegment
    {
        ITensorSegment Segment { get; }
    }

    public interface IVectorInfo : ISerializable, IHaveSpan, IHaveSize, IHaveTensorSegment
    {
        float this[int index] { get; }
        float this[uint index] { get; }
        IVector Create(LinearAlgebraProvider lap);
    }

    public interface IMatrixInfo : ISerializable, IHaveSpan, IHaveSize, IHaveTensorSegment
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
    }

    public interface ITensor3DInfo : ISerializable, IHaveSpan, IHaveSize, IHaveTensorSegment
    {
        uint Depth { get; }
        uint RowCount { get; }
        uint ColumnCount { get; }
        uint MatrixSize { get; }
        float this[int depth, int rowY, int columnX] { get; }
        float this[uint depth, uint rowY, uint columnX] { get; }
        ITensor3D Create(LinearAlgebraProvider lap);
    }
    public interface ITensor4DInfo : ISerializable, IHaveSpan, IHaveSize, IHaveTensorSegment
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
}
