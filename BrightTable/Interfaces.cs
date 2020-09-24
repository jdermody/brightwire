using System;
using System.Collections.Generic;
using BrightData;
using BrightData.Helper;
using BrightTable.Transformations;

namespace BrightTable
{
    public enum DataTableOrientation
    {
        Unknown = 0,
        RowOriented,
        ColumnOriented
    }

    /// <summary>
    /// Segment table column type
    /// </summary>
    public enum ColumnType : byte
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

    public interface ISingleTypeTableSegment : IHaveMetaData, ICanWriteToBinaryWriter, IDisposable
    {
        ColumnType SingleType { get; }
        IEnumerable<object> Enumerate();
        uint Size { get; }
    }

    public interface IDataTableSegment
    {
        uint Size { get; }
        ColumnType[] Types { get; }
        object this[uint index] { get; }
    }

    public interface IDataTableSegment<out T> : ISingleTypeTableSegment, IHaveDataContext
    {
        IEnumerable<T> EnumerateTyped();
    }

    public interface IDataTable : IHaveMetaData, IDisposable, IHaveDataContext
    {
        uint RowCount { get; }
        uint ColumnCount { get; }
        ColumnType[] ColumnTypes { get; }
        DataTableOrientation Orientation { get; }
        IEnumerable<IMetaData> ColumnMetaData(params uint[] columnIndices);
        void ForEachRow(Action<object[], uint> callback, uint maxRows = uint.MaxValue);
        IEnumerable<ISingleTypeTableSegment> Columns(params uint[] columnIndices);
        void ReadTyped(ITypedRowConsumer[] consumers, uint maxRows = uint.MaxValue);
        ISingleTypeTableSegment Column(uint columnIndex);
    }

    public interface IColumnOrientedDataTable : IDataTable
    {
        IRowOrientedDataTable AsRowOriented(string filePath = null);
        IColumnOrientedDataTable Convert(params ColumnConversion[] conversion);
        IColumnOrientedDataTable Convert(string filePath, params ColumnConversion[] conversion);
        IColumnOrientedDataTable Normalize(NormalizationType type, string filePath = null);
        IColumnOrientedDataTable Normalize(params ColumnNormalization[] conversion);
        IColumnOrientedDataTable Normalize(string filePath, params ColumnNormalization[] conversion);
        IColumnOrientedDataTable SelectColumns(params uint[] columnIndices);
        IColumnOrientedDataTable SelectColumns(string filePath, params uint[] columnIndices);
        IColumnOrientedDataTable ConcatColumns(params IColumnOrientedDataTable[] others);
        IColumnOrientedDataTable ConcatColumns(string filePath, params IColumnOrientedDataTable[] others);
        IColumnOrientedDataTable FilterRows(Predicate<object[]> predicate, string filePath = null);
    }

    public interface IRowOrientedDataTable : IDataTable
    {
        IColumnOrientedDataTable AsColumnOriented(string filePath = null);
        void ForEachRow(Action<object[]> callback);
        void ForEachRow(IEnumerable<uint> rowIndices, Action<object[]> callback);
        IRowOrientedDataTable Bag(uint sampleCount, int? randomSeed = null, string filePath = null);
        IDataTableSegment Row(uint rowIndex);
        IEnumerable<IDataTableSegment> Rows(params uint[] rowIndices);
        IRowOrientedDataTable Concat(params IRowOrientedDataTable[] others);
        IRowOrientedDataTable Concat(string filePath, params IRowOrientedDataTable[] others);
        IRowOrientedDataTable Mutate(Func<object[], object[]> projector, string filePath = null);
        IRowOrientedDataTable SelectRows(params uint[] rowIndices);
        IRowOrientedDataTable SelectRows(string filePath, params uint[] rowIndices);
        IRowOrientedDataTable Shuffle(string filePath = null);
        IRowOrientedDataTable Sort(bool ascending, uint columnIndex, string filePath = null);
        IEnumerable<(string Label, IRowOrientedDataTable Table)> GroupBy(uint columnIndex);
        string FirstRow { get; }
        string SecondRow { get; }
        string ThirdRow { get; }
        string LastRow { get; }
        //IRowOrientedDataTable Vectorise(string columnName, params uint[] vectorColumnIndices);
        //IRowOrientedDataTable Vectorise(string filePath, string columnName, params uint[] vectorColumnIndices);
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

    public interface IEditableBuffer<in T>
    {
        void Set(uint index, T value);
    }

    public enum ColumnConversionType
    {
        Unchanged = 0,
        ToBoolean,
        ToDate,
        ToNumeric,
        ToString,
        ToIndexList,
        ToWeightedIndexList,
        ToVector,
        ToCategoricalIndex
    }

    public interface IConvert<in TF, TT> : ICanConvert
    {
        bool Convert(TF input, IHybridBuffer<TT> buffer);
        void Finalise(IMetaData metaData);
    }

    public interface IColumnTransformation
    {
        uint Transform();
        IHybridBuffer Buffer { get; }
    }

    public interface ITransformColumnOrientedDataTable
    {
        IColumnOrientedDataTable Transform(IColumnOrientedDataTable dataTable, string filePath = null);
    }

    public interface ITransformRowOrientedDataTable
    {
        IRowOrientedDataTable Transform(IRowOrientedDataTable dataTable, string filePath = null);
    }

    public interface IColumnInfo : IHaveMetaData
    {
        public uint Index { get; }
        ColumnType ColumnType { get; }
    }

    public interface IColumnTransformationParam
    {
        public uint? Index { get; }
        public ICanConvert GetConverter(ColumnType fromType, ISingleTypeTableSegment column, TempStreamManager tempStreams, uint inMemoryRowCount = 32768);
    }

    public interface IConvertibleTable
    {
        IConvertibleRow GetRow(uint index);
        IEnumerable<IConvertibleRow> Rows(params uint[] rowIndices);
        IRowOrientedDataTable DataTable { get; }
        IEnumerable<T> Map<T>(Func<IConvertibleRow, T> rowMapper);
        void ForEachRow(Action<IConvertibleRow> action);
    }

    public interface IHaveDataTable
    {
        IDataTable DataTable { get; }
    }

    public interface IHaveDataContext
    {
        IBrightDataContext Context { get; }
    }

    public interface IConvertibleRow : IHaveDataTable
    {
        IDataTableSegment Segment { get; }
        T GetField<T>(uint index);
        uint RowIndex { get; }
    }

    public interface ITypedRowConsumer
    {
        uint ColumnIndex { get; }
        ColumnType ColumnType { get; }
    }

    public interface ITypedRowConsumer<in T> : ITypedRowConsumer, IEditableBuffer<T>
    {
    }
}
