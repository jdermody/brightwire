using System;
using BrightData.Converter;
using BrightData.Helper;

namespace BrightData.DataTable.Meta
{
    /// <summary>
    /// A data dimension from a table
    /// </summary>
    /// <param name="dataType"></param>
    /// <param name="buffer"></param>
    public class TableDataDimension(BrightDataType dataType, IReadOnlyBuffer buffer) : IDataDimension
    {
        /// <inheritdoc />
        public BrightDataType DataType => dataType;

        /// <inheritdoc />
        public IReadOnlyBuffer<T> GetBuffer<T>() where T : notnull
        {
            var typeofT = typeof(T);
            var bufferType = buffer.DataType;

            if (bufferType == typeofT)
                return (IReadOnlyBuffer<T>)buffer;

            if (typeofT == typeof(object))
                return (IReadOnlyBuffer<T>)buffer.ToObjectBuffer();

            if (typeofT == typeof(string))
            {
                return (IReadOnlyBuffer<T>)buffer.ToStringBuffer();
            }

            if (bufferType.GetBrightDataType().IsNumeric() && typeofT.GetBrightDataType().IsNumeric())
            {
                var converter = StaticConverters.GetConverter(bufferType, typeof(T));
                return (IReadOnlyBuffer<T>)GenericTypeMapping.TypeConverter(typeof(T), buffer, converter);
            }

            throw new NotImplementedException("");
        }

        /// <inheritdoc />
        public IDataTypeSpecification? Specification => null;

        /// <inheritdoc />
        public uint Size => buffer.Size;
    }
}
