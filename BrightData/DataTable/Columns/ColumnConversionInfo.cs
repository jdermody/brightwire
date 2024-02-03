using BrightData.Buffer.Operations.Conversion;
using BrightData.Helper;
using System;
using System.Threading.Tasks;
using BrightData.Buffer.Operations;
using BrightData.Converter;

namespace BrightData.DataTable.Columns
{
    /// <summary>
    /// Captures information about a data table column conversion
    /// </summary>
    /// <param name="ColumnIndex"></param>
    /// <param name="Conversion"></param>
    public record ColumnConversionInfo(uint ColumnIndex, ColumnConversion Conversion)
    {
        /// <summary>
        /// Converts a column from a data table
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        public virtual async Task<IReadOnlyBufferWithMetaData> Convert(IDataTable dataTable)
        {
            var column = dataTable.GetColumn(ColumnIndex);
            var ret = await column.Convert(Conversion);
            column.MetaData.CopyTo(ret.MetaData, Consts.Name, Consts.IsTarget, Consts.ColumnIndex);
            return ret;
        }
    }

    /// <summary>
    /// A custom data table column conversion
    /// </summary>
    /// <typeparam name="FT"></typeparam>
    /// <typeparam name="TT"></typeparam>
    /// <param name="ColumnIndex"></param>
    /// <param name="Converter"></param>
    public record CustomColumnConversionInfo<FT, TT>(uint ColumnIndex, Func<FT, TT> Converter) : ColumnConversionInfo(ColumnIndex, ColumnConversion.Custom) where FT : notnull where TT : notnull
    {
        /// <inheritdoc />
        public override async Task<IReadOnlyBufferWithMetaData> Convert(IDataTable dataTable)
        {
            var column = dataTable.GetColumn(ColumnIndex);
            if (column.DataType != typeof(FT))
                throw new Exception($"Expected column {ColumnIndex} to be of type {typeof(FT)} but found {column.DataType}");
            var output = (ICompositeBuffer<TT>)column.DataType.GetBrightDataType().CreateCompositeBuffer();
            var converter = (IReadOnlyBuffer<TT>)GenericTypeMapping.TypeConverter(typeof(TT), column, new CustomConversionFunction<FT, TT>(Converter));
            var conversion = new BufferCopyOperation<TT>(converter, output, null);
            await conversion.Execute();
            column.MetaData.CopyTo(output.MetaData, Consts.Name, Consts.IsTarget, Consts.ColumnIndex);
            return output;
        }
    }
}
