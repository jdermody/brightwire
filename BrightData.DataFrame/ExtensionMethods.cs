using Microsoft.Data.Analysis;

namespace BrightData.DataFrame
{
    public static class ExtensionMethods
    {
        public static Microsoft.Data.Analysis.DataFrame ToDataFrame(this IDataTable dataTable)
        {
            return new Microsoft.Data.Analysis.DataFrame(
                dataTable.ColumnTypes.Zip(dataTable.ColumnMetaData).Select((x, i) => {
                    var (dataType, metaData) = x;
                    var defaultName = $"Column {i + 1}";

                    return (DataFrameColumn)(dataType switch {
                        BrightDataType.Boolean => new BooleanDataFrameColumn(metaData.GetName(defaultName), dataTable.GetColumn<bool>((uint)i).Enumerate()),
                        BrightDataType.Date    => new DateTimeDataFrameColumn(metaData.GetName(defaultName), dataTable.GetColumn<DateTime>((uint)i).Enumerate()),
                        BrightDataType.Double  => new DoubleDataFrameColumn(metaData.GetName(defaultName), dataTable.GetColumn<double>((uint)i).Enumerate()),
                        BrightDataType.Decimal => new DecimalDataFrameColumn(metaData.GetName(defaultName), dataTable.GetColumn<decimal>((uint)i).Enumerate()),
                        BrightDataType.Float   => new SingleDataFrameColumn(metaData.GetName(defaultName), dataTable.GetColumn<float>((uint)i).Enumerate()),
                        BrightDataType.Int     => new Int32DataFrameColumn(metaData.GetName(defaultName), dataTable.GetColumn<int>((uint)i).Enumerate()),
                        BrightDataType.Long    => new Int64DataFrameColumn(metaData.GetName(defaultName), dataTable.GetColumn<long>((uint)i).Enumerate()),
                        BrightDataType.SByte   => new SByteDataFrameColumn(metaData.GetName(defaultName), dataTable.GetColumn<sbyte>((uint)i).Enumerate()),
                        BrightDataType.Short   => new Int16DataFrameColumn(metaData.GetName(defaultName), dataTable.GetColumn<short>((uint)i).Enumerate()),
                        BrightDataType.String  => new StringDataFrameColumn(metaData.GetName(defaultName), dataTable.GetColumn<string>((uint)i).Enumerate()),
                        _                      => throw new NotSupportedException($"{dataType} cannot be converted to data frame column")
                    });
                })
            );
        }

        public static IBuildDataTables ToDataTableBuilder(this Microsoft.Data.Analysis.DataFrame dataFrame, BrightDataContext context)
        {
            var builder = context.CreateTableBuilder();
            foreach (var column in dataFrame.Columns)
                builder.CreateColumn(column.DataType.GetBrightDataType(), column.Name);

            var numColumns = dataFrame.Columns.Count;
            var buffer = new object[numColumns];
            foreach (var row in dataFrame.Rows) {
                for(var i = 0; i < numColumns; i++)
                    buffer[i] = row[i];
                builder.AddRow(buffer);
            }

            return builder;
        }
    }
}
