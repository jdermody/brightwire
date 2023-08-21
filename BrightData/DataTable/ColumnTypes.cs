using System;
using System.Runtime.CompilerServices;

namespace BrightData.DataTable
{
    /// <summary>
    /// Column type that references a range of other values
    /// </summary>
    public struct DataRangeColumnType
    {
        /// <summary>
        /// First inclusive index
        /// </summary>
        public uint StartIndex { get; set; }

        /// <summary>
        /// Number of items
        /// </summary>
        public uint Count { get; set; }
    }

    /// <summary>
    /// Column type that references a range of matrices
    /// </summary>
    public struct MatrixColumnType
    {
        /// <summary>
        /// First inclusive index
        /// </summary>
        public uint StartIndex { get; set; }

        /// <summary>
        /// Number of rows
        /// </summary>
        public uint RowCount { get; set; }

        /// <summary>
        /// Number of columns
        /// </summary>
        public uint ColumnCount { get; set; }

        /// <summary>
        /// Total number of values
        /// </summary>
        public readonly uint Size => RowCount * ColumnCount;
    }

    /// <summary>
    /// Column type that references a range of 3D tensors
    /// </summary>
    public struct Tensor3DColumnType
    {
        /// <summary>
        /// First inclusive index
        /// </summary>
        public uint StartIndex { get; set; }

        /// <summary>
        /// Number of matrices
        /// </summary>
        public uint Depth { get; set; }

        /// <summary>
        /// Number of rows
        /// </summary>
        public uint RowCount { get; set; }

        /// <summary>
        /// Number of columns
        /// </summary>
        public uint ColumnCount { get; set; }

        /// <summary>
        /// Total number of values
        /// </summary>
        public uint Size => Depth * RowCount * ColumnCount;
    }

    /// <summary>
    /// Column type that references a range of 4D tensors
    /// </summary>
    public struct Tensor4DColumnType
    {
        /// <summary>
        /// First inclusive index
        /// </summary>
        public uint StartIndex { get; set; }

        /// <summary>
        /// Number of 3D tensors
        /// </summary>
        public uint Count { get; set; }

        /// <summary>
        /// Number of matrices
        /// </summary>
        public uint Depth { get; set; }

        /// <summary>
        /// Number of rows
        /// </summary>
        public uint RowCount { get; set; }

        /// <summary>
        /// Number of columns
        /// </summary>
        public uint ColumnCount { get; set; }

        /// <summary>
        /// Total number of values
        /// </summary>
        public uint Size => Count * Depth * RowCount * ColumnCount;
    }

    /// <summary>
    /// Column type helpers
    /// </summary>
    public static class ColumnTypeHelper
    {
        static (Type, uint) GetTypeAndSize<T>() => (typeof(T), (uint)Unsafe.SizeOf<T>());

        /// <summary>
        /// Returns the .net type and its size to represent a bright data type within a column
        /// </summary>
        /// <param name="dataType">Bright data type</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static (Type Type, uint Size) GetColumnType(this BrightDataType dataType) => dataType switch 
        {
            BrightDataType.BinaryData        => GetTypeAndSize<DataRangeColumnType>(),
            BrightDataType.Boolean           => GetTypeAndSize<bool>(),
            BrightDataType.SByte             => GetTypeAndSize<sbyte>(),
            BrightDataType.Short             => GetTypeAndSize<short>(),
            BrightDataType.Int               => GetTypeAndSize<int>(),
            BrightDataType.Long              => GetTypeAndSize<long>(),
            BrightDataType.Float             => GetTypeAndSize<float>(),
            BrightDataType.Double            => GetTypeAndSize<double>(),
            BrightDataType.Decimal           => GetTypeAndSize<decimal>(),
            BrightDataType.String            => GetTypeAndSize<uint>(),
            BrightDataType.Date              => GetTypeAndSize<DateTime>(),
            BrightDataType.IndexList         => GetTypeAndSize<DataRangeColumnType>(),
            BrightDataType.WeightedIndexList => GetTypeAndSize<DataRangeColumnType>(),
            BrightDataType.Vector            => GetTypeAndSize<DataRangeColumnType>(),
            BrightDataType.Matrix            => GetTypeAndSize<MatrixColumnType>(),
            BrightDataType.Tensor3D          => GetTypeAndSize<Tensor3DColumnType>(),
            BrightDataType.Tensor4D          => GetTypeAndSize<Tensor4DColumnType>(),
            BrightDataType.TimeOnly          => GetTypeAndSize<TimeOnly>(),
            BrightDataType.DateOnly          => GetTypeAndSize<DateOnly>(),
            _                                => throw new ArgumentOutOfRangeException(nameof(dataType), dataType, null)
        };
    }
}
