using Parquet;
using BrightData.Types;
using Parquet.Data;
using Parquet.Schema;

namespace BrightData.Parquet
{
    public static class ExtensionMethods
    {
        public static async Task<IDataTable> CreateTableFromParquet(this BrightDataContext context, Stream inputStream, Stream? outputStream)
        {
            var reader = await ParquetReader.CreateAsync(inputStream);
            var fields = reader.Schema.DataFields;
            var columnCount = (uint)fields.Length;
            var columnTypes = fields.Select(x => x.ClrType).ToArray();
            var builder = context.CreateTableBuilder();
            var columnMetaData = columnCount.AsRange().Select(_ => new MetaData()).ToArray();
            var columns = new ICompositeBuffer?[columnCount];

            // read the metadata
            foreach (var (column, field) in columnMetaData.Zip(fields))
                column.SetName(field.Name);
            foreach (var (key, value) in reader.CustomMetadata)
                builder.TableMetaData.Set(key, value);

            // write the data
            for (var i = 0; i < reader.RowGroupCount; i++) {
                using var rowGroupReader = reader.OpenRowGroupReader(i);
                for (var j = 0; j < columnCount; j++) {
                    var field = fields[j];
                    var buffer = columns[j] ??= CreateColumn(rowGroupReader, field, columnMetaData[i], builder);
                    var columnType = columnTypes[j];
                    var column = await rowGroupReader.ReadColumnAsync(fields[j]);
                    var parquetData = column.Data;
                    if (field.IsNullable) {
                        if (columnType.IsValueType) {
                            var defaultValue = Activator.CreateInstance(columnType);
                            foreach (var item in parquetData)
                                buffer.AppendObject(item ?? defaultValue!);
                        }
                        else
                            throw new Exception($"Nullable non value types are not supported: {columnType}");
                    }
                    else {
                        foreach (var item in parquetData)
                            buffer.AppendObject(item!);
                    }

                }
            }

            // write to stream
            var output = outputStream ?? new MemoryStream();
            await builder.WriteTo(output);
            return await context.LoadTableFromStream(output);
        }

        public static async Task WriteAsParquet(this IDataTable dataTable, Stream output)
        {
            var dataFields = dataTable.ColumnMetaData
                .Zip(dataTable.ColumnTypes).Select((x, i) => new DataField(x.First.GetName($"Column {i + 1}"), x.Second.GetDataType(), false, false))
                .ToArray();
            var schema = new ParquetSchema(dataFields.Cast<Field>().ToArray());
            var columns = dataTable.GetColumns();
            var firstColumn = columns[0];

            // create a parquet writer
            await using var writer = await ParquetWriter.CreateAsync(schema, output);
            writer.CompressionMethod = CompressionMethod.Gzip;
            writer.CompressionLevel = System.IO.Compression.CompressionLevel.Optimal;

            // write the meta data
            writer.CustomMetadata = dataTable.MetaData.AllKeys.ToDictionary(x => x, x => dataTable.MetaData.Get(x)?.ToString() ?? "");

            // write each block
            for (uint i = 0, len = (uint)firstColumn.BlockSizes.Length; i < len; i++) {
                using var blockWriter = writer.CreateRowGroup();
                var columnIndex = 0;
                foreach (var column in columns) {
                    var metaData = column.MetaData.AllKeys.ToDictionary(x => x, x => column.MetaData.Get(x)?.ToString() ?? "");
                    var array = await column.GetBlock(i);
                    await blockWriter.WriteColumnAsync(new DataColumn(dataFields[columnIndex], array), metaData);
                    ++columnIndex;
                }
            }
        }

        static ICompositeBuffer CreateColumn(ParquetRowGroupReader reader, DataField field, MetaData columnMetaData, IBuildDataTables builder)
        {
            foreach(var (key, value) in reader.GetCustomMetadata(field))
                columnMetaData.Set(key, value);
            return builder.CreateColumn(field.ClrType.GetBrightDataType(), columnMetaData);
        }

        public static Task<IDataTable> LoadParquetDataTableFromFile(this BrightDataContext context, string path)
        {
            return LoadParquetDataTableFromStream(context, File.OpenRead(path));
        }

        public static async Task<IDataTable> LoadParquetDataTableFromStream(this BrightDataContext context, Stream stream)
        {
            return await ParquetDataTableAdaptor.Create(context, stream);
        }
    }
}
