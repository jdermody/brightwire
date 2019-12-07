using System;
using System.Collections.Generic;
using System.IO;
using BrightData;

namespace BrightTable
{
    public enum DataTableOrientation
    {
        Unknown = 0,
        RowOriented,
        ColumnOriented
    }

    /// <summary>
    /// Data table column type
    /// </summary>
    public enum ColumnType
    {
        /// <summary>
        /// Nothing
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Boolean values
        /// </summary>
        Boolean,

        /// <summary>
        /// Byte values (-128 to 128)
        /// </summary>
        Byte,

        /// <summary>
        /// Short values
        /// </summary>
        Short,

        /// <summary>
        /// Integer values
        /// </summary>
        Int,

        /// <summary>
        /// Long values
        /// </summary>
        Long,

        /// <summary>
        /// Float values
        /// </summary>
        Float,

        /// <summary>
        /// Double values
        /// </summary>
        Double,

        /// <summary>
        /// Decimal values
        /// </summary>
        Decimal,

        /// <summary>
        /// String values
        /// </summary>
        String,

        /// <summary>
        /// Date values
        /// </summary>
        Date,

        /// <summary>
        /// List of indices
        /// </summary>
        IndexList,

        /// <summary>
        /// Weighted list of indices
        /// </summary>
        WeightedIndexList,

        /// <summary>
        /// Vector of floats
        /// </summary>
        Vector,

        /// <summary>
        /// Matrix of floats
        /// </summary>
        Matrix,

        /// <summary>
        /// 3D tensor of floats
        /// </summary>
        Tensor3D,

        /// <summary>
        /// 4D tensor of floats
        /// </summary>
        Tensor4D,

        /// <summary>
        /// Binary data
        /// </summary>
        BinaryData
    }

    public interface IStringIterator
    {
        char Next();
        long Position { get; }
        long ProgressPercent { get; }
    }

    public interface ISingleTypeTableSegment : IHaveMetaData, ICanWriteToBinaryWriter, IDisposable
    {
        ColumnType SingleType { get; }
        IEnumerable<object> Enumerate();
        uint Size { get; }
        bool IsEncoded { get; }
    }

    public interface IDataTableSegment
    {
        uint Size { get; }
        ColumnType[] Types { get; }
        object this[uint index] { get; }
    }

    public interface IDataTableSegment<out T> : ISingleTypeTableSegment
    {
        IEnumerable<T> EnumerateTyped();
    }

    public interface IDataTable : IHaveMetaData, IDisposable
    {
        IBrightDataContext Context { get; }
        uint RowCount { get; }
        uint ColumnCount { get; }
        IReadOnlyList<ColumnType> ColumnTypes { get; }
        DataTableOrientation Orientation { get; }
        IReadOnlyList<IMetaData> ColumnMetaData(params uint[] columnIndices);
        void ForEachRow(Action<object[], uint> callback);
        ISingleTypeTableSegment Column(uint columnIndex);
        IReadOnlyList<ISingleTypeTableSegment> Columns(params uint[] columnIndices);
        IDataTableSegment Row(uint rowIndex);
        IReadOnlyList<IDataTableSegment> Rows(params uint[] rowIndices);
    }

    public interface IColumnOrientedDataTable : IDataTable, IDisposable
    {
        IRowOrientedDataTable AsRowOriented(string filePath = null);
    }

    public interface IRowOrientedDataTable : IDataTable
    {
        IColumnOrientedDataTable AsColumnOriented(string filePath = null);
        IReadOnlyList<IDataTableSegment> Head { get; }
        IReadOnlyList<IDataTableSegment> Tail { get; }
        void ForEachRow(IEnumerable<uint> rowIndices, Action<object[]> callback);
    }

    interface IProvideStrings
    {
        uint Count { get; }
        void Reset();
        IEnumerable<string> All { get; }
    }

    public interface IEditableBuffer
    {
        void Set(uint index, object value);
        void Finalise();
    }

    public enum ColumnConversion
    {
        Unchanged = 0,
        ToBoolean,
        ToDate,
        ToNumeric,
        ToString,
        ToIndexList,
        ToWeightedIndexList,
        ToVector
    }

    public enum NormalisationType
    {
        Standard,
        Euclidean,
        Manhattan,
        FeatureScale
    }

    public interface IConvertColumn
    {
        ISingleTypeTableSegment Convert(IBrightDataContext context, ISingleTypeTableSegment segment);
    }

    public interface ITransformColumnOrientedDataTable
    {
        IColumnOrientedDataTable Transform(IColumnOrientedDataTable dataTable, string filePath = null);
    }

    public interface ITransformRowOrientedDataTable
    {
        IRowOrientedDataTable Transform(IRowOrientedDataTable dataTable, string filePath = null);
    }
}
