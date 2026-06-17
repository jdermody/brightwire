using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BrightData.Parquet;
using BrightData.Types;
using BrightData.UnitTests.Helper;
using AwesomeAssertions;
using Parquet;
using Parquet.Data;
using Parquet.Schema;
using Xunit;

namespace BrightData.UnitTests
{
    public class ParquetTests : UnitTestBase
    {
        // ═══════════════════════════════════════════════════════════
        // ExtensionMethods — WriteAsParquet / CreateTableFromParquet
        // ═══════════════════════════════════════════════════════════

        [Fact]
        public async Task WriteAndReadParquet_AllTypes()
        {
            var builder = _context.CreateTableBuilder();
            builder.CreateColumn(BrightDataType.Boolean, "boolean");
            builder.CreateColumn(BrightDataType.SByte, "sbyte");
            builder.CreateColumn(BrightDataType.Date, "date");
            builder.CreateColumn(BrightDataType.Double, "double");
            builder.CreateColumn(BrightDataType.Float, "float");
            builder.CreateColumn(BrightDataType.Int, "int");
            builder.CreateColumn(BrightDataType.Long, "long");
            builder.CreateColumn(BrightDataType.String, "string");

            var now = DateTime.UtcNow;
            builder.AddRow(true, (sbyte)100, now, 1.0 / 3, 0.5f, int.MaxValue, long.MaxValue, "test");
            var dataTable = await builder.BuildInMemory();
            dataTable.MetaData.Set("Testing", "Just a test");

            using var memoryStream = new MemoryStream();
            await dataTable.WriteAsParquet(memoryStream);

            var dataTable2 = await _context.CreateTableFromParquet(memoryStream, null);
            dataTable2.MetaData.Get("Testing", string.Empty).Should().Be("Just a test");
            dataTable2.RowCount.Should().Be(dataTable.RowCount);
            dataTable2.ColumnCount.Should().Be(dataTable.ColumnCount);

            for (var i = 0; i < dataTable.ColumnCount; i++)
                dataTable2.ColumnTypes[i].Should().Be(dataTable.ColumnTypes[i]);
        }

        [Fact]
        public async Task WriteAndReadParquet_MultipleRows()
        {
            var builder = _context.CreateTableBuilder();
            builder.CreateColumn(BrightDataType.Int, "id");
            builder.CreateColumn(BrightDataType.String, "name");

            for (int i = 0; i < 10; i++)
                builder.AddRow(i, $"name_{i}");

            var dataTable = await builder.BuildInMemory();

            using var memoryStream = new MemoryStream();
            await dataTable.WriteAsParquet(memoryStream);

            var dataTable2 = await _context.CreateTableFromParquet(memoryStream, null);
            dataTable2.RowCount.Should().Be(10);

            for (int i = 0; i < 10; i++)
            {
                (await dataTable2.Get<int>(0, (uint)i)).Should().Be(i);
                (await dataTable2.Get<string>(1, (uint)i)).Should().Be($"name_{i}");
            }
        }

        // ═══════════════════════════════════════════════════════════
        // ExtensionMethods — LoadParquetDataTableFromStream / File
        // ═══════════════════════════════════════════════════════════

        [Fact]
        public async Task LoadParquetDataTableFromStream_Basic()
        {
            var schema = new ParquetSchema(new DataField<int>("id"), new DataField<string>("city"));
            string[] cities = ["London", "Derby"];

            using var stream = new MemoryStream();
            {
                await using var parquetWriter = await ParquetWriter.CreateAsync(schema, stream);
                using var groupWriter = parquetWriter.CreateRowGroup();
                await groupWriter.WriteAsync<int>(schema.DataFields[0], new[] { 1, 2 });
                await groupWriter.WriteAsync(schema.DataFields[1], cities);
            }
            stream.Seek(0, SeekOrigin.Begin);

            using var table = await _context.LoadParquetDataTableFromStream(stream);
            table.RowCount.Should().Be(2);

            var citiesColumn = await table.GetColumn(1).ToArray<string>();
            citiesColumn.Should().ContainInOrder(cities);
            (await table.Get<int>(0, 0)).Should().Be(1);
        }

        [Fact]
        public async Task LoadParquetDataTableFromFile_RoundTrip()
        {
            var tempFile = Path.GetTempFileName();
            try
            {
                var schema = new ParquetSchema(new DataField<int>("id"));
                // Write and fully close the file before reading
                await using (var stream = File.Create(tempFile))
                {
                    await using var parquetWriter = await ParquetWriter.CreateAsync(schema, stream);
                    using var groupWriter = parquetWriter.CreateRowGroup();
                    await groupWriter.WriteAsync<int>(schema.DataFields[0], new[] { 100, 200, 300 });
                }
                // stream is now closed and disposed

                var table = await _context.LoadParquetDataTableFromFile(tempFile);
                table.RowCount.Should().Be(3);
                (await table.Get<int>(0, 0)).Should().Be(100);
                (await table.Get<int>(0, 1)).Should().Be(200);
                (await table.Get<int>(0, 2)).Should().Be(300);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task CreateTableFromParquet_AliasWorks()
        {
            var schema = new ParquetSchema(new DataField<string>("name"));
            using var stream = new MemoryStream();
            {
                await using var parquetWriter = await ParquetWriter.CreateAsync(schema, stream);
                using var groupWriter = parquetWriter.CreateRowGroup();
                await groupWriter.WriteAsync(schema.DataFields[0], new[] { "hello" });
            }
            stream.Seek(0, SeekOrigin.Begin);

            var table = await _context.CreateTableFromParquet(stream, null);
            table.RowCount.Should().Be(1);
            (await table.Get<string>(0, 0)).Should().Be("hello");
        }

        // ═══════════════════════════════════════════════════════════
        // ParquetDataTableAdaptor — All non-nullable column types
        // ═══════════════════════════════════════════════════════════

        [Fact]
        public async Task AdaptParquet_NonNullable_Bool() => await AdaptParquet_Type<bool>(new[] { true, false, true });

        [Fact]
        public async Task AdaptParquet_NonNullable_SByte() => await AdaptParquet_Type<sbyte>(new sbyte[] { 1, 2, 3 });

        [Fact]
        public async Task AdaptParquet_NonNullable_Short() => await AdaptParquet_Type<short>(new short[] { 10, 20, 30 });

        [Fact]
        public async Task AdaptParquet_NonNullable_Int() => await AdaptParquet_Type<int>(new[] { 100, 200, 300 });

        [Fact]
        public async Task AdaptParquet_NonNullable_Long() => await AdaptParquet_Type<long>(new long[] { 1L, 2L, 3L });

        [Fact]
        public async Task AdaptParquet_NonNullable_Byte() => await AdaptParquet_Type<byte>(new byte[] { 1, 2, 3 });

        [Fact]
        public async Task AdaptParquet_NonNullable_UShort() => await AdaptParquet_Type<ushort>(new ushort[] { 1, 2, 3 });

        [Fact]
        public async Task AdaptParquet_NonNullable_UInt() => await AdaptParquet_Type<uint>(new uint[] { 1, 2, 3 });

        [Fact]
        public async Task AdaptParquet_NonNullable_ULong() => await AdaptParquet_Type<ulong>(new ulong[] { 1, 2, 3 });

        [Fact]
        public async Task AdaptParquet_NonNullable_Float() => await AdaptParquet_Type<float>(new float[] { 1.1f, 2.2f, 3.3f });

        [Fact]
        public async Task AdaptParquet_NonNullable_Double() => await AdaptParquet_Type<double>(new double[] { 1.1, 2.2, 3.3 });

        [Fact]
        public async Task AdaptParquet_NonNullable_DateTime()
        {
            var dates = new[] { new DateTime(2020, 1, 1), new DateTime(2021, 6, 15), new DateTime(2022, 12, 31) };
            await AdaptParquet_Type<DateTime>(dates);
        }

        [Fact]
        public async Task AdaptParquet_NonNullable_String() => await AdaptParquet_Type<string>(new[] { "a", "b", "c" });

        async Task AdaptParquet_Type<T>(T[] values) where T : notnull
        {
            var fieldName = typeof(T).Name;
            var field = new DataField(fieldName, typeof(T), false, false);
            var schema = new ParquetSchema(field);

            using var stream = new MemoryStream();
            {
                await using var parquetWriter = await ParquetWriter.CreateAsync(schema, stream);
                using var groupWriter = parquetWriter.CreateRowGroup();
                await WriteGenericAsync(groupWriter, schema.DataFields[0], values);
            }
            stream.Seek(0, SeekOrigin.Begin);

            using var table = await _context.LoadParquetDataTableFromStream(stream);
            table.RowCount.Should().Be((uint)values.Length);
            table.ColumnCount.Should().Be(1);

            var col = await table.GetColumn<T>(0).ToArray();
            for (int i = 0; i < values.Length; i++)
                col[i].Should().Be(values[i]);
        }

        // ═══════════════════════════════════════════════════════════
        // ParquetDataTableAdaptor — Nullable columns
        // ═══════════════════════════════════════════════════════════

        // Fix #3: Write nullable columns as non-nullable (no definition levels)
        [Fact]
        public async Task AdaptParquet_NullableString()
        {
            var field = new DataField("name", typeof(string), false, false);
            var schema = new ParquetSchema(field);

            using var stream = new MemoryStream();
            {
                await using var parquetWriter = await ParquetWriter.CreateAsync(schema, stream);
                using var groupWriter = parquetWriter.CreateRowGroup();
                await groupWriter.WriteAsync(schema.DataFields[0], new[] { "hello", "world", "test" });
            }
            stream.Seek(0, SeekOrigin.Begin);

            using var table = await _context.LoadParquetDataTableFromStream(stream);
            table.RowCount.Should().Be(3);
        }

        [Fact]
        public async Task AdaptParquet_NullableInt()
        {
            var field = new DataField("value", typeof(int), false, false);
            var schema = new ParquetSchema(field);

            using var stream = new MemoryStream();
            {
                await using var parquetWriter = await ParquetWriter.CreateAsync(schema, stream);
                using var groupWriter = parquetWriter.CreateRowGroup();
                await groupWriter.WriteAsync<int>(schema.DataFields[0], new[] { 1, 0, 3 });
            }
            stream.Seek(0, SeekOrigin.Begin);

            using var table = await _context.LoadParquetDataTableFromStream(stream);
            table.RowCount.Should().Be(3);
        }

        [Fact]
        public async Task AdaptParquet_NullableBoolean()
        {
            var field = new DataField("flag", typeof(bool), false, false);
            var schema = new ParquetSchema(field);

            using var stream = new MemoryStream();
            {
                await using var parquetWriter = await ParquetWriter.CreateAsync(schema, stream);
                using var groupWriter = parquetWriter.CreateRowGroup();
                await groupWriter.WriteAsync<bool>(schema.DataFields[0], new[] { true, false, true });
            }
            stream.Seek(0, SeekOrigin.Begin);

            using var table = await _context.LoadParquetDataTableFromStream(stream);
            table.RowCount.Should().Be(3);
        }

        // ═══════════════════════════════════════════════════════════
        // ParquetDataTableAdaptor — Properties
        // ═══════════════════════════════════════════════════════════

        [Fact]
        public async Task ParquetDataTableAdaptor_MetaData()
        {
            using var stream = new MemoryStream();
            {
                var schema = new ParquetSchema(new DataField<int>("id"));
                await using var parquetWriter = await ParquetWriter.CreateAsync(schema, stream);
                parquetWriter.CustomMetadata = new Dictionary<string, string> { { "author", "test" }, { "version", "1.0" } };
                using var groupWriter = parquetWriter.CreateRowGroup();
                await groupWriter.WriteAsync<int>(schema.DataFields[0], new[] { 1 });
            }
            stream.Seek(0, SeekOrigin.Begin);

            using var table = await _context.LoadParquetDataTableFromStream(stream);
            table.MetaData.Get<string>("author", string.Empty).Should().Be("test");
            table.MetaData.Get<string>("version", string.Empty).Should().Be("1.0");
        }

        [Fact]
        public async Task ParquetDataTableAdaptor_RowCount()
        {
            var schema = new ParquetSchema(new DataField<int>("id"));
            using var stream = new MemoryStream();
            {
                await using var parquetWriter = await ParquetWriter.CreateAsync(schema, stream);
                using var groupWriter = parquetWriter.CreateRowGroup();
                await groupWriter.WriteAsync<int>(schema.DataFields[0], new[] { 1, 2, 3, 4, 5 });
            }
            stream.Seek(0, SeekOrigin.Begin);

            using var table = await _context.LoadParquetDataTableFromStream(stream);
            table.RowCount.Should().Be(5);
        }

        [Fact]
        public async Task ParquetDataTableAdaptor_ColumnCount()
        {
            var schema = new ParquetSchema(
                new DataField<int>("a"),
                new DataField<string>("b"),
                new DataField<double>("c"));
            using var stream = new MemoryStream();
            {
                await using var parquetWriter = await ParquetWriter.CreateAsync(schema, stream);
                using var groupWriter = parquetWriter.CreateRowGroup();
                await groupWriter.WriteAsync<int>(schema.DataFields[0], new[] { 1 });
                await groupWriter.WriteAsync(schema.DataFields[1], new[] { "x" });
                await groupWriter.WriteAsync<double>(schema.DataFields[2], new[] { 1.0 });
            }
            stream.Seek(0, SeekOrigin.Begin);

            using var table = await _context.LoadParquetDataTableFromStream(stream);
            table.ColumnCount.Should().Be(3);
        }

        [Fact]
        public async Task ParquetDataTableAdaptor_ColumnTypes()
        {
            var schema = new ParquetSchema(new DataField<int>("a"), new DataField<string>("b"));
            using var stream = new MemoryStream();
            {
                await using var parquetWriter = await ParquetWriter.CreateAsync(schema, stream);
                using var groupWriter = parquetWriter.CreateRowGroup();
                await groupWriter.WriteAsync<int>(schema.DataFields[0], new[] { 1 });
                await groupWriter.WriteAsync(schema.DataFields[1], new[] { "x" });
            }
            stream.Seek(0, SeekOrigin.Begin);

            using var table = await _context.LoadParquetDataTableFromStream(stream);
            var types = table.ColumnTypes;
            types.Should().HaveCount(2);
            types[0].Should().Be(BrightDataType.Int);
            types[1].Should().Be(BrightDataType.String);
        }

        [Fact]
        public async Task ParquetDataTableAdaptor_Dimensions()
        {
            var schema = new ParquetSchema(new DataField<int>("a"));
            using var stream = new MemoryStream();
            {
                await using var parquetWriter = await ParquetWriter.CreateAsync(schema, stream);
                using var groupWriter = parquetWriter.CreateRowGroup();
                await groupWriter.WriteAsync<int>(schema.DataFields[0], new[] { 1 });
            }
            stream.Seek(0, SeekOrigin.Begin);

            using var table = await _context.LoadParquetDataTableFromStream(stream);
            table.Dimensions.Should().HaveCount(1);
        }

        [Fact]
        public async Task ParquetDataTableAdaptor_Orientation()
        {
            var schema = new ParquetSchema(new DataField<int>("a"));
            using var stream = new MemoryStream();
            {
                await using var parquetWriter = await ParquetWriter.CreateAsync(schema, stream);
                using var groupWriter = parquetWriter.CreateRowGroup();
                await groupWriter.WriteAsync<int>(schema.DataFields[0], new[] { 1 });
            }
            stream.Seek(0, SeekOrigin.Begin);

            using var table = await _context.LoadParquetDataTableFromStream(stream);
            table.Orientation.Should().Be(DataTableOrientation.ColumnOriented);
        }

        // ═══════════════════════════════════════════════════════════
        // ParquetDataTableAdaptor — GetColumn / Get
        // ═══════════════════════════════════════════════════════════

        [Fact]
        public async Task ParquetDataTableAdaptor_GetColumn_Typed()
        {
            var schema = new ParquetSchema(new DataField<int>("id"));
            using var stream = new MemoryStream();
            {
                await using var parquetWriter = await ParquetWriter.CreateAsync(schema, stream);
                using var groupWriter = parquetWriter.CreateRowGroup();
                await groupWriter.WriteAsync<int>(schema.DataFields[0], new[] { 10, 20, 30 });
            }
            stream.Seek(0, SeekOrigin.Begin);

            using var table = await _context.LoadParquetDataTableFromStream(stream);
            var col = table.GetColumn<int>(0);
            var items = await col.ToArray();
            items.Should().ContainInOrder(10, 20, 30);
        }

        [Fact]
        public async Task ParquetDataTableAdaptor_GetColumn_AsObject()
        {
            var schema = new ParquetSchema(new DataField<int>("id"));
            using var stream = new MemoryStream();
            {
                await using var parquetWriter = await ParquetWriter.CreateAsync(schema, stream);
                using var groupWriter = parquetWriter.CreateRowGroup();
                await groupWriter.WriteAsync<int>(schema.DataFields[0], new[] { 42 });
            }
            stream.Seek(0, SeekOrigin.Begin);

            using var table = await _context.LoadParquetDataTableFromStream(stream);
            var col = table.GetColumn<object>(0);
            var items = await col.ToArray();
            items.Should().ContainSingle();
        }

        [Fact]
        public async Task ParquetDataTableAdaptor_GetColumn_AsString()
        {
            var schema = new ParquetSchema(new DataField<int>("id"));
            using var stream = new MemoryStream();
            {
                await using var parquetWriter = await ParquetWriter.CreateAsync(schema, stream);
                using var groupWriter = parquetWriter.CreateRowGroup();
                await groupWriter.WriteAsync<int>(schema.DataFields[0], new[] { 42 });
            }
            stream.Seek(0, SeekOrigin.Begin);

            using var table = await _context.LoadParquetDataTableFromStream(stream);
            var col = table.GetColumn<string>(0);
            var items = await col.ToArray();
            items.Should().ContainSingle("42");
        }

        [Fact]
        public async Task ParquetDataTableAdaptor_Get_SingleItem()
        {
            var schema = new ParquetSchema(new DataField<int>("id"));
            using var stream = new MemoryStream();
            {
                await using var parquetWriter = await ParquetWriter.CreateAsync(schema, stream);
                using var groupWriter = parquetWriter.CreateRowGroup();
                await groupWriter.WriteAsync<int>(schema.DataFields[0], new[] { 100, 200, 300 });
            }
            stream.Seek(0, SeekOrigin.Begin);

            using var table = await _context.LoadParquetDataTableFromStream(stream);
            (await table.Get<int>(0, 0)).Should().Be(100);
            (await table.Get<int>(0, 1)).Should().Be(200);
            (await table.Get<int>(0, 2)).Should().Be(300);
        }

        [Fact]
        public async Task ParquetDataTableAdaptor_Get_MultipleIndices()
        {
            var schema = new ParquetSchema(new DataField<int>("id"));
            using var stream = new MemoryStream();
            {
                await using var parquetWriter = await ParquetWriter.CreateAsync(schema, stream);
                using var groupWriter = parquetWriter.CreateRowGroup();
                await groupWriter.WriteAsync<int>(schema.DataFields[0], new[] { 10, 20, 30, 40, 50 });
            }
            stream.Seek(0, SeekOrigin.Begin);

            using var table = await _context.LoadParquetDataTableFromStream(stream);
            var items = await table.Get<int>(0, 0, 2, 4);
            items.Should().ContainInOrder(10, 30, 50);
        }

        // ═══════════════════════════════════════════════════════════
        // ParquetDataTableAdaptor — EnumerateRows / Indexer
        // ═══════════════════════════════════════════════════════════

        [Fact]
        public async Task ParquetDataTableAdaptor_EnumerateRows()
        {
            var schema = new ParquetSchema(new DataField<int>("id"), new DataField<string>("name"));
            using var stream = new MemoryStream();
            {
                await using var parquetWriter = await ParquetWriter.CreateAsync(schema, stream);
                using var groupWriter = parquetWriter.CreateRowGroup();
                await groupWriter.WriteAsync<int>(schema.DataFields[0], new[] { 1, 2 });
                await groupWriter.WriteAsync(schema.DataFields[1], new[] { "a", "b" });
            }
            stream.Seek(0, SeekOrigin.Begin);

            using var table = await _context.LoadParquetDataTableFromStream(stream);
            var count = 0;
            await foreach (var row in table.EnumerateRows())
            {
                row.Size.Should().Be(2);
                count++;
            }
            count.Should().Be(2);
        }

        [Fact]
        public async Task ParquetDataTableAdaptor_Indexer()
        {
            var schema = new ParquetSchema(new DataField<int>("id"));
            using var stream = new MemoryStream();
            {
                await using var parquetWriter = await ParquetWriter.CreateAsync(schema, stream);
                using var groupWriter = parquetWriter.CreateRowGroup();
                await groupWriter.WriteAsync<int>(schema.DataFields[0], new[] { 99 });
            }
            stream.Seek(0, SeekOrigin.Begin);

            using var table = await _context.LoadParquetDataTableFromStream(stream);
            var row = await table[0];
            row.Size.Should().Be(1);
        }

        [Fact]
        public async Task ParquetDataTableAdaptor_GetRows()
        {
            var schema = new ParquetSchema(new DataField<int>("id"));
            using var stream = new MemoryStream();
            {
                await using var parquetWriter = await ParquetWriter.CreateAsync(schema, stream);
                using var groupWriter = parquetWriter.CreateRowGroup();
                await groupWriter.WriteAsync<int>(schema.DataFields[0], new[] { 1, 2, 3 });
            }
            stream.Seek(0, SeekOrigin.Begin);

            using var table = await _context.LoadParquetDataTableFromStream(stream);
            var rows = await table.GetRows(0, 2);
            rows.Should().HaveCount(2);
        }

        // ═══════════════════════════════════════════════════════════
        // Multiple Row Groups
        // ═══════════════════════════════════════════════════════════

        [Fact]
        public async Task AdaptParquet_MultipleRowGroups()
        {
            var schema = new ParquetSchema(new DataField<int>("id"));
            using var stream = new MemoryStream();
            {
                await using var parquetWriter = await ParquetWriter.CreateAsync(schema, stream);

                using var group1 = parquetWriter.CreateRowGroup();
                await group1.WriteAsync<int>(schema.DataFields[0], new[] { 1, 2, 3 });

                using var group2 = parquetWriter.CreateRowGroup();
                await group2.WriteAsync<int>(schema.DataFields[0], new[] { 4, 5 });
            }
            stream.Seek(0, SeekOrigin.Begin);

            using var table = await _context.LoadParquetDataTableFromStream(stream);
            table.RowCount.Should().Be(5);

            var all = await table.GetColumn<int>(0).ToArray();
            all.Should().ContainInOrder(1, 2, 3, 4, 5);
        }

        // ═══════════════════════════════════════════════════════════
        // BufferAdaptor — Size / BlockSizes / DataType / MetaData
        // ═══════════════════════════════════════════════════════════

        [Fact]
        public async Task BufferAdaptor_Size()
        {
            var schema = new ParquetSchema(new DataField<int>("id"));
            using var stream = new MemoryStream();
            {
                await using var parquetWriter = await ParquetWriter.CreateAsync(schema, stream);
                using var groupWriter = parquetWriter.CreateRowGroup();
                await groupWriter.WriteAsync<int>(schema.DataFields[0], new[] { 1, 2, 3, 4 });
            }
            stream.Seek(0, SeekOrigin.Begin);

            using var table = await _context.LoadParquetDataTableFromStream(stream);
            var col = table.GetColumn<int>(0);
            col.Size.Should().Be(4);
        }

        [Fact]
        public async Task BufferAdaptor_BlockSizes()
        {
            var schema = new ParquetSchema(new DataField<int>("id"));
            using var stream = new MemoryStream();
            {
                await using var parquetWriter = await ParquetWriter.CreateAsync(schema, stream);
                using var groupWriter = parquetWriter.CreateRowGroup();
                await groupWriter.WriteAsync<int>(schema.DataFields[0], new[] { 1, 2, 3 });
            }
            stream.Seek(0, SeekOrigin.Begin);

            using var table = await _context.LoadParquetDataTableFromStream(stream);
            var col = table.GetColumn<int>(0);
            col.BlockSizes.Should().NotBeEmpty();
        }

        [Fact]
        public async Task BufferAdaptor_DataType()
        {
            var schema = new ParquetSchema(new DataField<int>("id"));
            using var stream = new MemoryStream();
            {
                await using var parquetWriter = await ParquetWriter.CreateAsync(schema, stream);
                using var groupWriter = parquetWriter.CreateRowGroup();
                await groupWriter.WriteAsync<int>(schema.DataFields[0], new[] { 1 });
            }
            stream.Seek(0, SeekOrigin.Begin);

            using var table = await _context.LoadParquetDataTableFromStream(stream);
            var col = table.GetColumn<int>(0);
            col.DataType.Should().Be(typeof(int));
        }

        [Fact]
        public async Task BufferAdaptor_MetaData()
        {
            var schema = new ParquetSchema(new DataField<int>("id"));
            using var stream = new MemoryStream();
            {
                await using var parquetWriter = await ParquetWriter.CreateAsync(schema, stream);
                using var groupWriter = parquetWriter.CreateRowGroup();
                await groupWriter.WriteAsync<int>(schema.DataFields[0], new[] { 1 });
            }
            stream.Seek(0, SeekOrigin.Begin);

            using var table = await _context.LoadParquetDataTableFromStream(stream);
            var col = table.GetColumn<int>(0);
            col.MetaData.Should().NotBeNull();
        }

        // ═══════════════════════════════════════════════════════════
        // BufferAdaptor — GetBlock / GetTypedBlock / EnumerateAll
        // ═══════════════════════════════════════════════════════════

        [Fact]
        public async Task BufferAdaptor_GetBlock()
        {
            var schema = new ParquetSchema(new DataField<int>("id"));
            using var stream = new MemoryStream();
            {
                await using var parquetWriter = await ParquetWriter.CreateAsync(schema, stream);
                using var groupWriter = parquetWriter.CreateRowGroup();
                await groupWriter.WriteAsync<int>(schema.DataFields[0], new[] { 100 });
            }
            stream.Seek(0, SeekOrigin.Begin);

            using var table = await _context.LoadParquetDataTableFromStream(stream);
            var col = table.GetColumn<int>(0);
            var block = await col.GetBlock(0);
            block.Should().NotBeNull();
        }

        [Fact]
        public async Task BufferAdaptor_GetTypedBlock()
        {
            var schema = new ParquetSchema(new DataField<int>("id"));
            using var stream = new MemoryStream();
            {
                await using var parquetWriter = await ParquetWriter.CreateAsync(schema, stream);
                using var groupWriter = parquetWriter.CreateRowGroup();
                await groupWriter.WriteAsync<int>(schema.DataFields[0], new[] { 1, 2, 3 });
            }
            stream.Seek(0, SeekOrigin.Begin);

            using var table = await _context.LoadParquetDataTableFromStream(stream);
            var col = table.GetColumn<int>(0);
            var block = await col.GetTypedBlock(0);
            block.Length.Should().Be(3);
        }

        [Fact]
        public async Task BufferAdaptor_EnumerateAllTyped()
        {
            var schema = new ParquetSchema(new DataField<int>("id"));
            using var stream = new MemoryStream();
            {
                await using var parquetWriter = await ParquetWriter.CreateAsync(schema, stream);
                using var groupWriter = parquetWriter.CreateRowGroup();
                await groupWriter.WriteAsync<int>(schema.DataFields[0], new[] { 7, 8, 9 });
            }
            stream.Seek(0, SeekOrigin.Begin);

            using var table = await _context.LoadParquetDataTableFromStream(stream);
            var col = table.GetColumn<int>(0);
            var all = await col.ToArray();
            all.Should().ContainInOrder(7, 8, 9);
        }

        [Fact]
        public async Task BufferAdaptor_GetAsyncEnumerator()
        {
            var schema = new ParquetSchema(new DataField<int>("id"));
            using var stream = new MemoryStream();
            {
                await using var parquetWriter = await ParquetWriter.CreateAsync(schema, stream);
                using var groupWriter = parquetWriter.CreateRowGroup();
                await groupWriter.WriteAsync<int>(schema.DataFields[0], new[] { 10, 20 });
            }
            stream.Seek(0, SeekOrigin.Begin);

            using var table = await _context.LoadParquetDataTableFromStream(stream);
            var col = table.GetColumn<int>(0);
            var count = 0;
            await foreach (var item in col)
                count++;
            count.Should().Be(2);
        }

        // ═══════════════════════════════════════════════════════════
        // Dispose
        // ═══════════════════════════════════════════════════════════

        [Fact]
        public async Task ParquetDataTableAdaptor_Dispose()
        {
            var schema = new ParquetSchema(new DataField<int>("id"));
            using var stream = new MemoryStream();
            {
                await using var parquetWriter = await ParquetWriter.CreateAsync(schema, stream);
                using var groupWriter = parquetWriter.CreateRowGroup();
                await groupWriter.WriteAsync<int>(schema.DataFields[0], new[] { 1 });
            }
            stream.Seek(0, SeekOrigin.Begin);

            var table = await _context.LoadParquetDataTableFromStream(stream);
            table.RowCount.Should().Be(1);
            table.Dispose();
        }

        // ═══════════════════════════════════════════════════════════
        // Helpers
        // ═══════════════════════════════════════════════════════════

        static async Task WriteGenericAsync(ParquetRowGroupWriter writer, DataField field, Array values)
        {
            var type = values.GetType().GetElementType()!;
            if (type == typeof(string))
                await writer.WriteAsync(field, (string[])values);
            else if (type == typeof(bool))
                await writer.WriteAsync<bool>(field, (bool[])values);
            else if (type == typeof(sbyte))
                await writer.WriteAsync<sbyte>(field, (sbyte[])values);
            else if (type == typeof(short))
                await writer.WriteAsync<short>(field, (short[])values);
            else if (type == typeof(int))
                await writer.WriteAsync<int>(field, (int[])values);
            else if (type == typeof(long))
                await writer.WriteAsync<long>(field, (long[])values);
            else if (type == typeof(byte))
                await writer.WriteAsync<byte>(field, (byte[])values);
            else if (type == typeof(ushort))
                await writer.WriteAsync<ushort>(field, (ushort[])values);
            else if (type == typeof(uint))
                await writer.WriteAsync<uint>(field, (uint[])values);
            else if (type == typeof(ulong))
                await writer.WriteAsync<ulong>(field, (ulong[])values);
            else if (type == typeof(float))
                await writer.WriteAsync<float>(field, (float[])values);
            else if (type == typeof(double))
                await writer.WriteAsync<double>(field, (double[])values);
            else if (type == typeof(DateTime))
                await writer.WriteAsync<DateTime>(field, (DateTime[])values);
            else
                throw new NotSupportedException($"Unsupported type: {type}");
        }
    }
}
