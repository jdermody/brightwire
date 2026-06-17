using System.Collections.Frozen;
using BrightData.Types;
using Parquet;
using Parquet.Data;
using Parquet.Schema;

namespace BrightData.Parquet
{
    public static class ExtensionMethods
    {
        static readonly FrozenDictionary<Type, Func<ParquetRowGroupReader, DataField, Array, CancellationToken, Task>> _readers =
            new Dictionary<Type, Func<ParquetRowGroupReader, DataField, Array, CancellationToken, Task>>()
            {
                { typeof(string),   (r, f, d, ct) => r.ReadAsync(f, (string[])d, null, ct).AsTask() },
                { typeof(bool),     (r, f, d, ct) => r.ReadAsync<bool>(f, (bool[])d, null, ct).AsTask() },
                { typeof(sbyte),    (r, f, d, ct) => r.ReadAsync<sbyte>(f, (sbyte[])d, null, ct).AsTask() },
                { typeof(short),    (r, f, d, ct) => r.ReadAsync<short>(f, (short[])d, null, ct).AsTask() },
                { typeof(int),      (r, f, d, ct) => r.ReadAsync<int>(f, (int[])d, null, ct).AsTask() },
                { typeof(long),     (r, f, d, ct) => r.ReadAsync<long>(f, (long[])d, null, ct).AsTask() },
                { typeof(float),    (r, f, d, ct) => r.ReadAsync<float>(f, (float[])d, null, ct).AsTask() },
                { typeof(double),   (r, f, d, ct) => r.ReadAsync<double>(f, (double[])d, null, ct).AsTask() },
                { typeof(DateTime), (r, f, d, ct) => r.ReadAsync<DateTime>(f, (DateTime[])d, null, ct).AsTask() },
                { typeof(byte),     (r, f, d, ct) => r.ReadAsync<byte>(f, (byte[])d, null, ct).AsTask() },
                { typeof(ushort),   (r, f, d, ct) => r.ReadAsync<ushort>(f, (ushort[])d, null, ct).AsTask() },
                { typeof(uint),     (r, f, d, ct) => r.ReadAsync<uint>(f, (uint[])d, null, ct).AsTask() },
                { typeof(ulong),    (r, f, d, ct) => r.ReadAsync<ulong>(f, (ulong[])d, null, ct).AsTask() },
            }.ToFrozenDictionary();

        static readonly Dictionary<BrightDataType, Func<ParquetRowGroupWriter, DataField, Array, CancellationToken, Task>> _writers =
            new()
            {
                { BrightDataType.String,  (w, f, d, _) => w.WriteAsync(f, (string[])d) },
                { BrightDataType.Boolean, (w, f, d, ct) => w.WriteAsync<bool>(f, (bool[])d, null, null, ct) },
                { BrightDataType.SByte,   (w, f, d, ct) => w.WriteAsync<sbyte>(f, (sbyte[])d, null, null, ct) },
                { BrightDataType.Byte,    (w, f, d, ct) => w.WriteAsync<byte>(f, (byte[])d, null, null, ct) },
                { BrightDataType.Short,   (w, f, d, ct) => w.WriteAsync<short>(f, (short[])d, null, null, ct) },
                { BrightDataType.UShort,  (w, f, d, ct) => w.WriteAsync<ushort>(f, (ushort[])d, null, null, ct) },
                { BrightDataType.Int,     (w, f, d, ct) => w.WriteAsync<int>(f, (int[])d, null, null, ct) },
                { BrightDataType.UInt,    (w, f, d, ct) => w.WriteAsync<uint>(f, (uint[])d, null, null, ct) },
                { BrightDataType.Float,   (w, f, d, ct) => w.WriteAsync<float>(f, (float[])d, null, null, ct) },
                { BrightDataType.Long,    (w, f, d, ct) => w.WriteAsync<long>(f, (long[])d, null, null, ct) },
                { BrightDataType.ULong,   (w, f, d, ct) => w.WriteAsync<ulong>(f, (ulong[])d, null, null, ct) },
                { BrightDataType.Double,  (w, f, d, ct) => w.WriteAsync<double>(f, (double[])d, null, null, ct) },
                { BrightDataType.Date,    (w, f, d, ct) => w.WriteAsync<DateTime>(f, (DateTime[])d, null, null, ct) },
            };

        static Array CreateColumnArray(DataField field, int rowCount)
        {
            return field.ClrType switch
            {
                var t when t == typeof(string)   => new string[rowCount],
                var t when t == typeof(bool)     => new bool[rowCount],
                var t when t == typeof(sbyte)    => new sbyte[rowCount],
                var t when t == typeof(short)    => new short[rowCount],
                var t when t == typeof(int)      => new int[rowCount],
                var t when t == typeof(long)     => new long[rowCount],
                var t when t == typeof(float)    => new float[rowCount],
                var t when t == typeof(double)   => new double[rowCount],
                var t when t == typeof(DateTime) => new DateTime[rowCount],
                var t when t == typeof(byte)     => new byte[rowCount],
                var t when t == typeof(ushort)   => new ushort[rowCount],
                var t when t == typeof(uint)     => new uint[rowCount],
                var t when t == typeof(ulong)    => new ulong[rowCount],
                _ => throw new NotSupportedException($"Unsupported Parquet column type: {field.ClrType.Name}")
            };
        }

        public static async Task<IDataTable> LoadParquetDataTableFromStream(
            this BrightDataContext context,
            Stream inputStream,
            Stream? outputStream = null,
            CancellationToken ct = default)
        {
            var reader = await ParquetReader.CreateAsync(inputStream);
            var fields = reader.Schema.DataFields;
            var columnCount = (uint)fields.Length;
            var columnTypes = fields.Select(f => f.ClrType).ToArray();
            var builder = context.CreateTableBuilder();
            var columnMetaData = new MetaData[columnCount];
            for (int i = 0; i < columnCount; i++)
                columnMetaData[i] = new MetaData();
            var columns = new ICompositeBuffer?[columnCount];

            // Read metadata
            foreach (var (column, field) in columnMetaData.Zip(fields))
                column.SetName(field.Name);
            foreach (var (key, value) in reader.CustomMetadata)
                builder.TableMetaData.Set(key, value);

            // Read data from each row group
            for (var i = 0; i < reader.RowGroupCount; i++)
            {
                using var rowGroupReader = reader.OpenRowGroupReader(i);
                for (var j = 0; j < columnCount; j++)
                {
                    var field = fields[j];
                    var buffer = columns[j] ??= CreateColumn(rowGroupReader, field, columnMetaData[j], builder);
                    var columnType = columnTypes[j];
                    var rowCount = (int)rowGroupReader.RowCount;
                    if (!_readers.TryGetValue(columnType, out var readAsync))
                        throw new NotSupportedException($"Unsupported Parquet column type: {columnType.Name}");

                    var data = CreateColumnArray(field, rowCount);
                    await readAsync(rowGroupReader, field, data, ct).ConfigureAwait(false);
                    // Fix #1: Typed dispatch to avoid NRE with string columns
                    if (columnType == typeof(string))
                    {
                        var strData = (string[])data;
                        var strBuffer = (ICompositeBuffer<string>)buffer;
                        for (int k = 0; k < strData.Length; k++)
                            strBuffer.Append(strData[k] ?? string.Empty);
                    }
                    else
                    {
                        for (int k = 0; k < data.Length; k++)
                            buffer.AppendObject(data.GetValue(k)!);
                    }
                }
            }

            var memStream = new MemoryStream();
            await builder.WriteTo(memStream);
            memStream.Position = 0;

            if (outputStream != null) {
                await memStream.CopyToAsync(outputStream, ct);
                if (outputStream.CanSeek)
                    outputStream.Position = 0;
                return await context.LoadTableFromStream(outputStream);
            }

            return await context.LoadTableFromStream(memStream);
        }

        public static Task<IDataTable> CreateTableFromParquet(
            this BrightDataContext context,
            Stream inputStream,
            Stream? outputStream,
            CancellationToken ct = default)
        {
            return LoadParquetDataTableFromStream(context, inputStream, outputStream, ct);
        }

        public static async Task WriteAsParquet(this IDataTable dataTable, Stream output, CancellationToken ct = default)
        {
            var dataFields = dataTable.ColumnMetaData
                .Zip(dataTable.ColumnTypes)
                .Select((x, i) => new DataField(
                    x.First.GetName($"Column {i + 1}"),
                    x.Second.GetDataType(),
                    false,
                    false))
                .ToArray();
            var schema = new ParquetSchema(dataFields.Cast<Field>().ToArray());

            // Create a parquet writer
            await using var writer = await ParquetWriter.CreateAsync(schema, output);

            // Write the metadata
            writer.CustomMetadata = dataTable.MetaData.AllKeys
                .ToDictionary(k => k, k => dataTable.MetaData.Get(k)?.ToString() ?? "");

            var columns = dataTable.GetColumns();
            var blockCount = columns[0].BlockSizes.Length;

            // Write each block
            for (uint i = 0; i < blockCount; i++)
            {
                using var rowGroupWriter = writer.CreateRowGroup();
                var columnIndex = 0;
                foreach (var column in columns)
                {
                    var block = await column.GetBlock(i);
                    var field = dataFields[columnIndex];
                    var colType = column.MetaData.GetColumnType();

                    // Fix #1: Use shared dispatcher for write
                    if (!_writers.TryGetValue(colType, out var writeAsync))
                        throw new NotSupportedException($"Unsupported Parquet write type: {colType}");

                    await writeAsync(rowGroupWriter, field, block, ct);
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
    }
}
