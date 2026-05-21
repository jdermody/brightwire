using Parquet;
using BrightData.Types;
using Parquet.Data;
using Parquet.Schema;

namespace BrightData.Parquet
{
    public static class ExtensionMethods
    {
        public static async Task<IDataTable> CreateTableFromParquet(this BrightDataContext context, Stream inputStream, Stream? outputStream, CancellationToken ct = default)
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

            // read data from each row group
            for (var i = 0; i < reader.RowGroupCount; i++)
            {
                using var rowGroupReader = reader.OpenRowGroupReader(i);
                for (var j = 0; j < columnCount; j++)
                {
                    var field = fields[j];
                    var buffer = columns[j] ??= CreateColumn(rowGroupReader, field, columnMetaData[j], builder);
                    var columnType = columnTypes[j];
                    var rowCount = (int)rowGroupReader.RowCount;

                    // Read column data using explicit type dispatch
                    Array data;
                    if (columnType == typeof(string))
                    {
                        var stringData = new string[rowCount];
                        await rowGroupReader.ReadAsync(field, stringData, null, ct).ConfigureAwait(false);
                        data = stringData;
                    }
                    else if (columnType == typeof(bool))
                    {
                        var boolData = new bool[rowCount];
                        await rowGroupReader.ReadAsync<bool>(field, boolData, null, ct).ConfigureAwait(false);
                        data = boolData;
                    }
                    else if (columnType == typeof(sbyte))
                    {
                        var sbData = new sbyte[rowCount];
                        await rowGroupReader.ReadAsync<sbyte>(field, sbData, null, ct).ConfigureAwait(false);
                        data = sbData;
                    }
                    else if (columnType == typeof(short))
                    {
                        var shData = new short[rowCount];
                        await rowGroupReader.ReadAsync<short>(field, shData, null, ct).ConfigureAwait(false);
                        data = shData;
                    }
                    else if (columnType == typeof(int))
                    {
                        var intData = new int[rowCount];
                        await rowGroupReader.ReadAsync<int>(field, intData, null, ct).ConfigureAwait(false);
                        data = intData;
                    }
                    else if (columnType == typeof(long))
                    {
                        var longData = new long[rowCount];
                        await rowGroupReader.ReadAsync<long>(field, longData, null, ct).ConfigureAwait(false);
                        data = longData;
                    }
                    else if (columnType == typeof(float))
                    {
                        var floatData = new float[rowCount];
                        await rowGroupReader.ReadAsync<float>(field, floatData, null, ct).ConfigureAwait(false);
                        data = floatData;
                    }
                    else if (columnType == typeof(double))
                    {
                        var doubleData = new double[rowCount];
                        await rowGroupReader.ReadAsync<double>(field, doubleData, null, ct).ConfigureAwait(false);
                        data = doubleData;
                    }
                    else if (columnType == typeof(DateTime))
                    {
                        var dtData = new DateTime[rowCount];
                        await rowGroupReader.ReadAsync<DateTime>(field, dtData, null, ct).ConfigureAwait(false);
                        data = dtData;
                    }
                    else
                    {
                        throw new NotSupportedException($"Unsupported Parquet column type: {columnType.Name}");
                    }

                    if (field.IsNullable && columnType.IsValueType)
                    {
                        var defaultValue = Activator.CreateInstance(columnType)!;
                        foreach (var item in data)
                            buffer.AppendObject(item ?? defaultValue);
                    }
                    else
                    {
                        foreach (var item in data)
                            buffer.AppendObject(item);
                    }
                }
            }

            // write to stream - use MemoryStream that stays open for reading
            var memStream = new MemoryStream();
            await builder.WriteTo(memStream);
            memStream.Position = 0;

            if (outputStream != null)
            {
                await memStream.CopyToAsync(outputStream, ct);
                if (outputStream.CanSeek)
                    outputStream.Position = 0;
                return await context.LoadTableFromStream(outputStream);
            }

            return await context.LoadTableFromStream(memStream);
        }

        public static async Task WriteAsParquet(this IDataTable dataTable, Stream output, CancellationToken ct = default)
        {
            var dataFields = dataTable.ColumnMetaData
                .Zip(dataTable.ColumnTypes).Select((x, i) => new DataField(x.First.GetName($"Column {i + 1}"), x.Second.GetDataType(), false, false))
                .ToArray();
            var schema = new ParquetSchema(dataFields.Cast<Field>().ToArray());

            // create a parquet writer
            await using var writer = await ParquetWriter.CreateAsync(schema, output);

            // write the meta data
            writer.CustomMetadata = dataTable.MetaData.AllKeys
                .ToDictionary(x => x, x => dataTable.MetaData.Get(x)?.ToString() ?? "");

            var columns = dataTable.GetColumns();
            var blockCount = columns[0].BlockSizes.Length;

            // write each block
            for (uint i = 0; i < blockCount; i++)
            {
                using var rowGroupWriter = writer.CreateRowGroup();
                var columnIndex = 0;
                foreach (var column in columns)
                {
                    // Get block and write using explicit type dispatch
                    var block = await column.GetBlock(i);
                    var field = dataFields[columnIndex];
                    var colType = column.MetaData.GetColumnType();

                    if (colType == BrightDataType.String)
                    {
                        await rowGroupWriter.WriteAsync(field, (string[])block);
                    }
                    else if (colType == BrightDataType.Boolean)
                    {
                        await rowGroupWriter.WriteAsync<bool>(field, (bool[])block);
                    }
                    else if (colType == BrightDataType.SByte)
                    {
                        await rowGroupWriter.WriteAsync<sbyte>(field, (sbyte[])block);
                    }
                    else if (colType == BrightDataType.Short)
                    {
                        await rowGroupWriter.WriteAsync<short>(field, (short[])block);
                    }
                    else if (colType == BrightDataType.Int)
                    {
                        await rowGroupWriter.WriteAsync<int>(field, (int[])block);
                    }
                    else if (colType == BrightDataType.Long)
                    {
                        await rowGroupWriter.WriteAsync<long>(field, (long[])block);
                    }
                    else if (colType == BrightDataType.Float)
                    {
                        await rowGroupWriter.WriteAsync<float>(field, (float[])block);
                    }
                    else if (colType == BrightDataType.Double)
                    {
                        await rowGroupWriter.WriteAsync<double>(field, (double[])block);
                    }
                    else if (colType == BrightDataType.Date)
                    {
                        await rowGroupWriter.WriteAsync<DateTime>(field, (DateTime[])block);
                    }
                    else
                    {
                        throw new NotSupportedException($"Unsupported Parquet write type: {colType}");
                    }
                    ++columnIndex;
                }
            }
        }

        static ICompositeBuffer CreateColumn(ParquetRowGroupReader reader, DataField field, MetaData columnMetaData, IBuildDataTables builder)
        {
            return builder.CreateColumn(field.ClrType.GetBrightDataType(), columnMetaData);
        }

        public static async Task<IDataTable> LoadParquetDataTableFromFile(this BrightDataContext context, string path)
        {
            await using var stream = File.OpenRead(path);
            return await LoadParquetDataTableFromStream(context, stream);
        }

        public static async Task<IDataTable> LoadParquetDataTableFromStream(this BrightDataContext context, Stream stream)
        {
            return await ParquetDataTableAdaptor.Create(context, stream);
        }
    }
}