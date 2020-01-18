﻿using System;
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
        IReadOnlyList<ISingleTypeTableSegment> Columns(params uint[] columnIndices);
    }

    public interface IColumnOrientedDataTable : IDataTable, IDisposable
    {
        IRowOrientedDataTable AsRowOriented(string filePath = null);
        IColumnOrientedDataTable Convert(params ColumnConversion[] conversion);
        IColumnOrientedDataTable Convert(string filePath, params ColumnConversion[] conversion);
        ISingleTypeTableSegment Column(uint columnIndex);
        IColumnOrientedDataTable SelectColumns(params uint[] columnIndices);
        IColumnOrientedDataTable SelectColumns(string filePath, params uint[] columnIndices);
        IColumnOrientedDataTable Normalise(NormalisationType type, string filePath = null);
        IColumnOrientedDataTable OneHotEncode(params uint[] columnIndices);
        IColumnOrientedDataTable OneHotEncode(bool writeToMetadata, params uint[] columnIndices);
        IColumnOrientedDataTable OneHotEncode(string filePath, bool writeToMetadata, params uint[] columnIndices);
        IMutateColumns CreateMutateContext();
        IColumnOrientedDataTable Concat(params IColumnOrientedDataTable[] others);
        IColumnOrientedDataTable Concat(string filePath, params IColumnOrientedDataTable[] others);
    }

    public interface IRowOrientedDataTable : IDataTable
    {
        IColumnOrientedDataTable AsColumnOriented(string filePath = null);
        IReadOnlyList<IDataTableSegment> Head { get; }
        IReadOnlyList<IDataTableSegment> Tail { get; }
        void ForEachRow(IEnumerable<uint> rowIndices, Action<object[]> callback);
        IRowOrientedDataTable Bag(uint sampleCount, int? randomSeed = null, string filePath = null);
        IDataTableSegment Row(uint rowIndex);
        IReadOnlyList<IDataTableSegment> Rows(params uint[] rowIndices);
        IRowOrientedDataTable Concat(params IRowOrientedDataTable[] others);
        IRowOrientedDataTable Concat(string filePath, params IRowOrientedDataTable[] others);
        IRowOrientedDataTable Mutate(Func<object[], object[]> projector, string filePath = null);
        IRowOrientedDataTable SelectRows(params uint[] rowIndices);
        IRowOrientedDataTable SelectRows(string filePath, params uint[] rowIndices);
        IRowOrientedDataTable Shuffle(int? randomSeed = null, string filePath = null);
        IRowOrientedDataTable Sort(bool ascending, uint columnIndex, string filePath = null);
        IRowOrientedDataTable Vectorise(string columnName, params uint[] vectorColumnIndices);
        IRowOrientedDataTable Vectorise(string filePath, string columnName, params uint[] vectorColumnIndices);
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

    public interface IMutateColumns
    {
        IMutateColumns Add<T>(uint index, Func<T, T> mutator);
        IColumnOrientedDataTable Mutate(string filePath = null);
    }
}
