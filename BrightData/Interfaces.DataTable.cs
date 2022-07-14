using System;
using System.Collections.Generic;
using System.Threading;

namespace BrightData
{
    /// <summary>
    /// Determines if the data table is oriented as either rows or columns
    /// </summary>
    public enum DataTableOrientation : byte
    {
        /// <summary>
        /// Pathological case
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Data table is stored as a series of rows
        /// </summary>
        RowOriented,

        /// <summary>
        /// Data table is stored as aa series of columns
        /// </summary>
        ColumnOriented
    }

    /// <summary>
    /// Data types enumeration
    /// </summary>
    public enum BrightDataType : byte
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
        /// Signed byte values (-128 to 128)
        /// </summary>
        SByte,

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
        BinaryData,

        /// <summary>
        /// Time only
        /// </summary>
        TimeOnly,

        /// <summary>
        /// Date only
        /// </summary>
        DateOnly
    }

    /// <summary>
    /// Column classifications
    /// </summary>
    [Flags]
    public enum ColumnClass : byte
    {
        /// <summary>
        /// Unknown category
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Forms a category - a set of possible values
        /// </summary>
        Categorical = 1,

        /// <summary>
        /// Numbers (float, int etc)
        /// </summary>
        Numeric = 2,

        /// <summary>
        /// Floating point numbers (float, double, decimal)
        /// </summary>
        Decimal = 4,

        /// <summary>
        /// Struct (blittable)
        /// </summary>
        Struct = 8,

        /// <summary>
        /// Tensor (vector, matrix etc)
        /// </summary>
        Tensor = 16,

        /// <summary>
        /// Has an index (index list, weighted index list)
        /// </summary>
        IndexBased = 32,

        /// <summary>
        /// Date and time
        /// </summary>
        DateTime = 64,

        /// <summary>
        /// Whole number
        /// </summary>
        Integer = 128
    }

    /// <summary>
    /// A segment (series of values) in a table of which each element has the same type
    /// </summary>
    public interface ISingleTypeTableSegment : IHaveMetaData, ICanWriteToBinaryWriter, IDisposable, IHaveSize, ICanEnumerate
    {
        /// <summary>
        /// The single type of the segment
        /// </summary>
        BrightDataType SingleType { get; }
    }

    /// <summary>
    /// A series of values from a data table
    /// </summary>
    public interface IDataTableSegment : IHaveSize
    {
        /// <summary>
        /// The column type of each value
        /// </summary>
        BrightDataType[] Types { get; }

        /// <summary>
        /// The value at each index (cast to object)
        /// </summary>
        /// <param name="index">Index to retrieve</param>
        object this[uint index] { get; }
    }

    /// <summary>
    /// Typed data table segment (all of the same type)
    /// </summary>
    /// <typeparam name="T">Data type of values within the segment</typeparam>
    public interface IDataTableSegment<out T> : ISingleTypeTableSegment, IHaveBrightDataContext
        where T : notnull
    {
        /// <summary>
        /// Enumerates the values
        /// </summary>
        /// <returns></returns>
        new IEnumerable<T> Values { get; }
    }

    /// <summary>
    /// Single column conversion options
    /// </summary>
    public enum ColumnConversionType
    {
        /// <summary>
        /// Leave the column unchanged (nop)
        /// </summary>
        Unchanged = 0,

        /// <summary>
        /// Convert to boolean
        /// </summary>
        ToBoolean,

        /// <summary>
        /// Convert to date
        /// </summary>
        ToDate,

        /// <summary>
        /// Convert to numeric (best numeric size will be automatically determined)
        /// </summary>
        ToNumeric,

        /// <summary>
        /// Convert to string
        /// </summary>
        ToString,

        /// <summary>
        /// Convert to index list
        /// </summary>
        ToIndexList,

        /// <summary>
        /// Convert to weighted index list
        /// </summary>
        ToWeightedIndexList,

        /// <summary>
        /// Convert to vector
        /// </summary>
        ToVector,

        /// <summary>
        /// Convert each value to an index within a dictionary
        /// </summary>
        ToCategoricalIndex,

        /// <summary>
        /// Convert to signed byte
        /// </summary>
        ToByte,

        /// <summary>
        /// Convert to short
        /// </summary>
        ToShort,

        /// <summary>
        /// Convert to int
        /// </summary>
        ToInt,

        /// <summary>
        /// Convert to long 
        /// </summary>
        ToLong,

        /// <summary>
        /// Convert to float
        /// </summary>
        ToFloat,

        /// <summary>
        /// Convert to double
        /// </summary>
        ToDouble,

        /// <summary>
        /// Convert to decimal
        /// </summary>
        ToDecimal
    }

    /// <summary>
    /// Transforms columns
    /// </summary>
    public interface IConvertColumn : ICanConvert
    {
        /// <summary>
        /// Complete the transformation
        /// </summary>
        /// <param name="metaData">Meta data store to receive transformation information</param>
        void Finalise(MetaData metaData);
    }

    /// <summary>
    /// Typed column transformer
    /// </summary>
    /// <typeparam name="TF"></typeparam>
    /// <typeparam name="TT"></typeparam>
    public interface IConvertColumn<in TF, TT> : IConvertColumn
        where TT : notnull
        where TF : notnull
    {
        /// <summary>
        /// Writes the converted input to the buffer
        /// </summary>
        /// <param name="input"></param>
        /// <param name="buffer"></param>
        /// <param name="index">Index within the buffer</param>
        /// <returns></returns>
        bool Convert(TF input, IHybridBuffer<TT> buffer, uint index);
    }


    /// <summary>
    /// Informtion about a column transformation
    /// </summary>
    public interface IColumnTransformationParam
    {
        /// <summary>
        /// Column index
        /// </summary>
        public uint? ColumnIndex { get; }

        /// <summary>
        /// Gets a column transformer
        /// </summary>
        /// <param name="fromType">Convert from column type</param>
        /// <param name="column">Column to convert</param>
        /// <param name="analysedMetaData">Function to produce analysed column meta data if needed</param>
        /// <param name="tempStreams">Temp stream provider</param>
        /// <param name="inMemoryRowCount">Number of rows to cache in memory</param>
        /// <returns></returns>
        public IConvertColumn? GetTransformer(
            BrightDataContext context, 
            BrightDataType fromType, 
            ISingleTypeTableSegment column, 
            Func<MetaData> analysedMetaData, 
            IProvideTempStreams tempStreams, 
            uint inMemoryRowCount = 32768
        );
    }

    /// <summary>
    /// Interface that 
    /// </summary>
    public interface IConsumeColumnData
    {
        /// <summary>
        /// Column index that will be consumed
        /// </summary>
        uint ColumnIndex { get; }

        /// <summary>
        /// Column type of incoming data
        /// </summary>
        BrightDataType ColumnType { get; }
    }

    /// <summary>
    /// Typed column consumer that writes to a buffer
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IConsumeColumnData<in T> : IConsumeColumnData, IAcceptSequentialTypedData<T>
        where T : notnull
    {
    }

    /// <summary>
    /// Data table vectoriser
    /// </summary>
    public interface IDataTableVectoriser : ICanWriteToBinaryWriter, IDisposable
    {
        /// <summary>
        /// Vectorise a table row
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        float[] Vectorise(object[] row);

        /// <summary>
        /// Vectorise a data table segment
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        float[] Vectorise(IDataTableSegment segment);

        /// <summary>
        /// Size of the output vectors
        /// </summary>
        uint OutputSize { get; }

        /// <summary>
        /// Returns the associated label from the one hot encoding dictionary
        /// </summary>
        /// <param name="vectorIndex">Index within one hot encoded vector</param>
        /// <param name="columnIndex">Data table column index</param>
        /// <returns></returns>
        string GetOutputLabel(uint vectorIndex, uint columnIndex = 0);

        /// <summary>
        /// Returns a sequence of vectorised table rows
        /// </summary>
        /// <returns></returns>
        IEnumerable<IVector> Enumerate();
    }

    /// <summary>
    /// Reinterpret columns parameters
    /// </summary>
    public interface IReinterpretColumns
    {
        /// <summary>
        /// Source column indices
        /// </summary>
        uint[] SourceColumnIndices { get; }

        /// <summary>
        /// Source column indices
        /// </summary>
        uint[] OutputColumnIndices { get; }

        /// <summary>
        /// Gets new column operations
        /// </summary>
        /// <param name="context">Bright data context</param>
        /// <param name="tempStreams">Temp stream provider</param>
        /// <param name="columns">Source column data</param>
        /// <returns></returns>
        IEnumerable<IOperation<ISingleTypeTableSegment?>> GetNewColumnOperations(BrightDataContext context, IProvideTempStreams tempStreams, uint rowCount, ICanEnumerateDisposable[] columns);
    }
}
