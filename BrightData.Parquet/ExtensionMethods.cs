using Parquet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using BrightData.Types;
using Parquet.Data;
using Parquet.Meta;
using Parquet.Schema;
using Type = Parquet.Meta.Type;
using System.IO;

namespace BrightData.Parquet
{
    public static class ExtensionMethods
    {
        public static async Task<IDataTable> LoadFromParquet(this BrightDataContext context, Stream inputStream, Stream? outputStream)
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
            var tasks = new Task[columnCount];
            for (var i = 0; i < reader.RowGroupCount; i++) {
                using var rowGroupReader = reader.OpenRowGroupReader(i);
                for (var j = 0; j < columnCount; j++) {
                    var field = fields[j];
                    var buffer = columns[i] ??= CreateColumn(rowGroupReader, field, columnMetaData[i], builder);
                    var columnType = columnTypes[j];
                    tasks[j] = rowGroupReader.ReadColumnAsync(fields[j]).ContinueWith(x => {
                        var parquetData = x.Result.Data;
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
                    });

                }
                await Task.WhenAll(tasks);
            }

            // write to stream
            var output = outputStream ?? new MemoryStream();
            await builder.WriteTo(output);
            return await context.LoadTableFromStream(output);
        }

        public static async Task WriteAsParquet(this IDataTable dataTable, Stream output)
        {
            var fields = dataTable.ColumnMetaData.Zip(dataTable.ColumnTypes).Select((x, i) => new DataField(x.First.GetName($"Column {i + 1}"), x.Second.GetDataType(), false, false));
            var schema = new ParquetSchema(fields);
            var columns = dataTable.GetColumns();
            var firstColumn = columns[0];

            // write each block
            using var writer = await ParquetWriter.CreateAsync(schema, output);
            writer.CompressionMethod = CompressionMethod.Gzip;
            writer.CompressionLevel = System.IO.Compression.CompressionLevel.Optimal;
            for (var i = 0; i < firstColumn.BlockCount; i++) {
                using ParquetRowGroupWriter blockWriter = writer.CreateRowGroup();
                foreach (var column in columns) {
                    
                }
                //await groupWriter.WriteColumnAsync(idColumn);
                //await groupWriter.WriteColumnAsync(cityColumn);
            }
        }

        //static DataColumn GetColumnBlock()

        static ICompositeBuffer CreateColumn(ParquetRowGroupReader reader, DataField field, MetaData columnMetaData, IBuildDataTables builder)
        {
            foreach(var (key, value) in reader.GetCustomMetadata(field))
                columnMetaData.Set(key, value);
            return builder.CreateColumn(field.ClrType.GetBrightDataType(), columnMetaData);
        }
    }
}
