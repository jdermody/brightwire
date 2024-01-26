using System;
using System.Linq;
using System.Threading.Tasks;
using BrightData.DataTable.Rows;
using BrightData.Helper;
using BrightData.UnitTests.Helper;
using BrightWire;
using FluentAssertions;
using Xunit;

namespace BrightData.UnitTests
{
    public class DataTableTests : CpuBase
    {
        [Fact]
        public async Task TestColumnTypes()
        {
            var builder = _context.CreateTableBuilder();
            builder.CreateColumn(BrightDataType.Boolean, "boolean");
            builder.CreateColumn(BrightDataType.SByte, "byte");
            builder.CreateColumn(BrightDataType.Date, "date");
            builder.CreateColumn(BrightDataType.Double, "double");
            builder.CreateColumn(BrightDataType.Float, "float");
            builder.CreateColumn(BrightDataType.Int, "int");
            builder.CreateColumn(BrightDataType.Long, "long");
            builder.CreateColumn(BrightDataType.String, "string");

            var now = DateTime.Now;
            builder.AddRow(true, (sbyte)100, now, 1.0 / 3, 0.5f, int.MaxValue, long.MaxValue, "test");
            var dataTable = await builder.BuildInMemory();

            var firstRow = dataTable[0];
            firstRow.Get<bool>(0).Should().BeTrue();
            firstRow.Get<sbyte>(1).Should().Be(100);
            firstRow.Get<DateTime>(2).Should().Be(now);
            firstRow.Get<double>(3).Should().Be(1.0 / 3);
            firstRow.Get<float>(4).Should().Be(0.5f);
            firstRow.Get<int>(5).Should().Be(int.MaxValue);
            firstRow.Get<long>(6).Should().Be(long.MaxValue);
            firstRow.Get<string>(7).Should().Be("test");
        }

        static void CompareRows(GenericTableRow row1, GenericTableRow row2)
        {
            row1.Size.Should().Be(row2.Size);
            for (uint i = 0; i < row1.Size; i++)
                row1.Values[i].Should().BeEquivalentTo(row2.Values[i]);
        }

        static async Task CompareTables(IDataTable table1, IDataTable table2)
        {
            var rand = new Random();
            table1.ColumnCount.Should().Be(table2.ColumnCount);
            table1.RowCount.Should().Be(table2.RowCount);
            table1.GetTargetColumn().Should().Be(table2.GetTargetColumn());

            var rowSample = 128.AsRange().Select(_ => (uint)rand.Next((int)table1.RowCount)).ToArray();
            var rows1 = await table1.GetRows(rowSample);
            var rows2 = await table2.GetRows(rowSample);
            foreach(var (r1, r2) in rows1.Zip(rows2))
                CompareRows(r1, r2);
        }

        static async Task RandomSample(uint rowCount, IDataTable table, Action<uint, GenericTableRow> callback)
        {
            var rand = new Random();
            var rowSample = 128.AsRange().Select(_ => (uint)rand.Next((int)rowCount)).ToArray();
            var rows = await table.GetRows(rowSample);
            foreach(var (row, index) in rows.Zip(rowSample))
                callback(index, row);
        }

        static Task<IDataTable> CreateComplexTable(BrightDataContext context)
        {
            var builder = context.CreateTableBuilder();
            builder.CreateColumn(BrightDataType.Boolean, "boolean");
            builder.CreateColumn(BrightDataType.SByte, "byte");
            builder.CreateColumn(BrightDataType.Date, "date");
            builder.CreateColumn(BrightDataType.Double, "double");
            builder.CreateColumn(BrightDataType.Float, "float");
            builder.CreateColumn(BrightDataType.Int, "int");
            builder.CreateColumn(BrightDataType.Long, "long");
            builder.CreateColumn(BrightDataType.String, "string");

            for (var i = 1; i <= 10; i++)
                builder.AddRow(i % 2 == 0, (sbyte)i, DateTime.Now, (double)i, (float)i, i, (long)i, i.ToString());
            return builder.BuildInMemory();
        }

        [Fact]
        public async Task TestDataTableAnalysis()
        {
            var table = await CreateComplexTable(_context);
            var analysis = await table.GetColumnAnalysis();

            var boolAnalysis = analysis[0].GetFrequencyAnalysis();
            boolAnalysis.NumDistinct.Should().Be(2);
            boolAnalysis.Frequency.Single(f => f.Label == "True").Value.Should().Be(0.5);
            boolAnalysis.Frequency.Single(f => f.Label == "False").Value.Should().Be(0.5);

            var numericAnalysis = new[] { 1, 3, 4, 5, 6 }.Select(i => analysis[i].GetNumericAnalysis()).ToList();
            numericAnalysis.All(a => a.NumDistinct == 10).Should().BeTrue();
            numericAnalysis.All(a => a.Min == 1.0).Should().BeTrue();
            numericAnalysis.All(a => a.Max == 10).Should().BeTrue();
            numericAnalysis.All(a => a.Mean == 5.5).Should().BeTrue();
            numericAnalysis.All(a => a.Median == 5.5).Should().BeTrue();
            numericAnalysis.All(a => Math.Round(a.SampleStdDev!.Value) == 3).Should().BeTrue();

            var stringAnalysis = analysis[7].GetStringAnalysis();
            stringAnalysis.NumDistinct.Should().Be(10);
            stringAnalysis.MaxLength.Should().Be(2);
        }

        Task<IDataTable> GetSimpleTable()
        {
            var builder = _context.CreateTableBuilder();
            builder.CreateColumn(BrightDataType.Int, "val");

            for (var i = 0; i < 10000; i++)
                builder.AddRow(i);
            return builder.BuildInMemory();
        }

        async Task<IDataTable> GetSimpleTable2()
        {
            using var table = await GetSimpleTable();
            var builder = await table.Project(r => [Convert.ToDouble(r[0])]);
            var table2 = await builder.BuildInMemory();
            table2.ColumnTypes[0].Should().Be(BrightDataType.Double);
            return table2;
        }

        [Fact]
        public async Task TestTableSlice()
        {
            using var table = await GetSimpleTable();
            using var table2 = await table.CopyRowsToNewTable(null, 100.AsRange(5000).ToArray());
            var rows = (await table2.GetRows()).Select(r => r.Get<int>(0)).ToList();

            for (var i = 0; i < 100; i++)
                rows[i].Should().Be(5000 + i);
        }

        [Fact]
        public async Task TestTableSplit()
        {
            var table = await GetSimpleTable();
            var (training, test) = await table.Split(0.75);
            training.RowCount.Should().Be(7500);
            test.RowCount.Should().Be(2500);
        }

        [Fact]
        public async Task TestStandardNormalisation()
        {
            using var table = await GetSimpleTable2();
            var analysis = (await table.GetColumnAnalysis(0))[0].GetNumericAnalysis();
            var mean = analysis.Mean;
            var stdDev = analysis.PopulationStdDev!.Value;
            using var normalised = await table.Normalize(NormalizationType.Standard);
            var existing = await table.GetColumn(0).ToArray<double>();

            await RandomSample(table.RowCount, normalised, (index, row) => {
                var val = row.Get<double>(0);
                var prevVal = existing[index];
                var expected = (prevVal - mean) / stdDev;
                DoubleMath.AreApproximatelyEqual(val, expected, 1E-4f).Should().BeTrue();
            });
        }

        [Fact]
        public async Task TestDataTableClone()
        {
            using var table = await CreateComplexTable(_context);
            using var clone = await table.Clone(null);
            await CompareTables(table, clone);
        }

        [Fact]
        public async Task ConcatTables()
        {
            using var table = await CreateComplexTable(_context);
            using var table2 = await CreateComplexTable(_context);
            var table3 = await table.ConcatenateRows(null, table2);
            table3.ColumnCount.Should().Be(table.ColumnCount);
            table3.RowCount.Should().Be(table.RowCount + table2.RowCount);
        }

        [Fact]
        public async Task ShuffleTables()
        {
            using var table = await CreateComplexTable(_context);
            using var shuffled = await table.Shuffle(null);
            shuffled.RowCount.Should().Be(table.RowCount);
        }

        //[Fact]
        //public void SortTable()
        //{
        //    using var table = CreateComplexTable(_context);
        //    using var sorted = table.Sort(1, false);
        //    sorted.FirstRow[1].Should().Be(table.LastRow[1]);
        //}

        [Fact]
        public async Task GroupTable()
        {
            using var table = await CreateComplexTable(_context);
            foreach (var (_, dataTable) in await table.GroupBy(0)) {
                dataTable.RowCount.Should().Be(table.RowCount / 2);
            }
        }

        [Fact]
        public async Task TestStandardNormalisation2()
        {
            var table = await GetSimpleTable2();
            var analysis = (await table.GetColumnAnalysis(0))[0].GetNumericAnalysis();
            var normalised = await table.Normalize(NormalizationType.Standard);
            var existing = await table.GetColumn(0).ToArray<double>();

            await RandomSample(table.RowCount, normalised, (index, row) => {
                var val = row.Get<double>(0);
                var prevVal = existing[index];
                var expected = (prevVal - analysis.Mean) / analysis.PopulationStdDev!;
                DoubleMath.AreApproximatelyEqual(val, expected, 1E-4f).Should().BeTrue();
            });
        }

        [Fact]
        public async Task TestFeatureScaleNormalisation()
        {
            var table = await GetSimpleTable2();
            var analysis = (await table.GetColumnAnalysis(0))[0].GetNumericAnalysis();
            var normalised = await table.Normalize(NormalizationType.FeatureScale);
            var existing = await table.GetColumn(0).ToArray<double>();

            await RandomSample(table.RowCount, normalised, (index, row) => {
                var val = row.Get<double>(0);
                var prevVal = existing[index];
                var expected = (prevVal - analysis.Min) / (analysis.Max - analysis.Min);
                DoubleMath.AreApproximatelyEqual(val, expected, 1E-4f).Should().BeTrue();
            });
        }

        [Fact]
        public async Task TestL2Normalisation()
        {
            var table = await GetSimpleTable2();
            var analysis = (await table.GetColumnAnalysis(0))[0].GetNumericAnalysis();
            var normalised = await table.Normalize(NormalizationType.Euclidean);
            var existing = await table.GetColumn(0).ToArray<double>();
            var l2Norm = Math.Sqrt(table.GetColumn<double>(0).EnumerateAllTyped().ToBlockingEnumerable().Select(d => Math.Pow(d, 2)).Sum());
            analysis.L2Norm.Should().Be(l2Norm);

            await RandomSample(table.RowCount, normalised, (index, row) => {
                var val = row.Get<double>(0);
                var prevVal = existing[index];
                var expected = prevVal / analysis.L2Norm;
                DoubleMath.AreApproximatelyEqual(val, expected, 1E-4f).Should().BeTrue();
            });
        }

        [Fact]
        public async Task TestL1Normalisation()
        {
            var table = await GetSimpleTable2();
            var analysis = (await table.GetColumnAnalysis(0))[0].GetNumericAnalysis();
            var normalised = await table.Normalize(NormalizationType.Manhattan);
            var existing = await table.GetColumn(0).ToArray<double>();
            var l1Norm = table.GetColumn<double>(0).EnumerateAllTyped().ToBlockingEnumerable().Select(Math.Abs).Sum();
            analysis.L1Norm.Should().Be(l1Norm);

            await RandomSample(table.RowCount, normalised, (index, row) => {
                var val = row.Get<double>(0);
                var prevVal = existing[index];
                var expected = prevVal / analysis.L1Norm;
                DoubleMath.AreApproximatelyEqual(val, expected, 1E-4f).Should().BeTrue();
            });
        }

        [Fact]
        public async Task TestTargetColumnIndex()
        {
            var builder = _context.CreateTableBuilder();
            builder.CreateColumn(BrightDataType.String, "a");
            builder.CreateColumn(BrightDataType.String, "b").MetaData.SetTarget(true);
            builder.CreateColumn(BrightDataType.String, "c");
            builder.AddRow("a", "b", "c");
            var table = await builder.BuildInMemory();

            table.GetTargetColumnOrThrow().Should().Be(1);
            table.RowCount.Should().Be(1);
            table.ColumnCount.Should().Be(3);
        }

        [Fact]
        public async Task AsMatrix()
        {
            var builder = _context.CreateTableBuilder();
            builder.CreateColumn(BrightDataType.Float, "val1");
            builder.CreateColumn(BrightDataType.Double, "val2");
            builder.CreateColumn(BrightDataType.String, "cls").MetaData.SetTarget(true);

            builder.AddRow(0.5f, 1.1, "a");
            builder.AddRow(0.2f, 1.5, "b");
            builder.AddRow(0.7f, 0.5, "c");
            builder.AddRow(0.2f, 0.6, "d");

            var table = await builder.BuildInMemory();
            var matrix = await table.AsMatrix(0, 1);
            matrix[0, 0].Should().Be(0.5f);
            matrix[0, 1].Should().Be(0.2f);
            matrix[1, 0].Should().Be(1.1f);
            matrix[1, 1].Should().Be(1.5f);
        }

        [Fact]
        public async Task SelectColumns()
        {
            var builder = _context.CreateTableBuilder();

            builder.CreateColumn(BrightDataType.Float, "val1");
            builder.CreateColumn(BrightDataType.Double, "val2");
            builder.CreateColumn(BrightDataType.String, "cls").MetaData.SetTarget(true);
            builder.CreateColumn(BrightDataType.String, "cls2");

            builder.AddRow(0.5f, 1.1, "a", "a2");
            builder.AddRow(0.2f, 1.5, "b", "b2");
            builder.AddRow(0.7f, 0.5, "c", "c2");
            builder.AddRow(0.2f, 0.6, "d", "d2");

            var table = await builder.BuildInMemory();
            var table2 = await table.CopyColumnsToNewTable(null, 1, 2, 3);

            table2.GetTargetColumnOrThrow().Should().Be(1);
            table2.RowCount.Should().Be(4);
            table2.ColumnCount.Should().Be(3);

            var column = table2.GetColumn(0);
            var columnValues = await column.ToArray<double>();
            columnValues[0].Should().Be(1.1);
            columnValues[1].Should().Be(1.5);
        }

        [Fact]
        public async Task Fold()
        {
            var builder = _context.CreateTableBuilder();

            builder.CreateColumn(BrightDataType.Float, "val1");
            builder.CreateColumn(BrightDataType.Double, "val2");
            builder.CreateColumn(BrightDataType.String, "cls").MetaData.SetTarget(true);

            builder.AddRow(0.5f, 1.1, "a");
            builder.AddRow(0.2f, 1.5, "b");
            builder.AddRow(0.7f, 0.5, "c");
            builder.AddRow(0.2f, 0.6, "d");

            var table = await builder.BuildInMemory();
            var folds = table.Fold(4).ToBlockingEnumerable().ToList();
            folds.Count.Should().Be(4);
            foreach (var (training, validation) in folds) {
                training.RowCount.Should().Be(3);
                validation.RowCount.Should().Be(1);
            }
        }

        [Fact]
        public async Task TableFilter()
        {
            var builder = _context.CreateTableBuilder();

            builder.CreateColumn(BrightDataType.Float, "val1");
            builder.CreateColumn(BrightDataType.Double, "val2");
            builder.CreateColumn(BrightDataType.String, "cls").MetaData.SetTarget(true);

            builder.AddRow(0.5f, 1.1, "a");
            builder.AddRow(0.2f, 1.5, "b");
            builder.AddRow(0.7f, 0.5, "c");
            builder.AddRow(0.2f, 0.6, "d");

            var table = await builder.BuildInMemory();
            var projectedTableBuilder = await table.Project(r => (string)r[2] == "b" ? null : r.Values);
            var projectedTable = await projectedTableBuilder.BuildInMemory();

            projectedTable.ColumnCount.Should().Be(table.ColumnCount);
            projectedTable.RowCount.Should().Be(3);
        }

        [Fact]
        public async Task TableConfusionMatrix()
        {
            var builder = _context.CreateTableBuilder();

            builder.CreateColumn(BrightDataType.String, "actual");
            builder.CreateColumn(BrightDataType.String, "expected");

            const int catCat = 5;
            const int catDog = 2;
            const int dogCat = 3;
            const int dogDog = 5;
            const int dogRabbit = 2;
            const int rabbitDog = 1;
            const int rabbitRabbit = 11;

            for (var i = 0; i < catCat; i++)
                builder.AddRow("cat", "cat");
            for (var i = 0; i < catDog; i++)
                builder.AddRow("cat", "dog");
            for (var i = 0; i < dogCat; i++)
                builder.AddRow("dog", "cat");
            for (var i = 0; i < dogDog; i++)
                builder.AddRow("dog", "dog");
            for (var i = 0; i < dogRabbit; i++)
                builder.AddRow("dog", "rabbit");
            for (var i = 0; i < rabbitDog; i++)
                builder.AddRow("rabbit", "dog");
            for (var i = 0; i < rabbitRabbit; i++)
                builder.AddRow("rabbit", "rabbit");
            var table = await builder.BuildInMemory();
            var confusionMatrix = await table.CreateConfusionMatrix(1, 0);

            confusionMatrix.GetCount("cat", "dog").Should().Be(catDog);
            confusionMatrix.GetCount("dog", "rabbit").Should().Be(dogRabbit);
            confusionMatrix.GetCount("rabbit", "rabbit").Should().Be(rabbitRabbit);
        }

        static async Task CheckTableConversion<T>(IBuildDataTables builder, ColumnConversion conversionType, BrightDataType columnType) where T: notnull
        {
            var table = await builder.BuildInMemory();
            var converted = await table.Convert(null, conversionType.ConvertColumn(0), conversionType.ConvertColumn(1));
            converted.ColumnTypes[0].Should().Be(columnType);
            converted.ColumnTypes[1].Should().Be(columnType);

            await foreach (var (b1, b2) in converted.Enumerate<T, T>())
                b1.Should().Be(b2);
        }

        [Fact]
        public Task BooleanColumnConversion()
        {
            var builder = _context.CreateTableBuilder();
            builder.CreateColumn(BrightDataType.String);
            builder.CreateColumn(BrightDataType.Boolean);

            builder.AddRow("True", true);
            builder.AddRow("Y", true);
            builder.AddRow("yes", true);
            builder.AddRow("t", true);
            builder.AddRow("1", true);
            builder.AddRow("False", false);
            builder.AddRow("N", false);
            builder.AddRow("no", false);
            builder.AddRow("f", false);
            builder.AddRow("0", false);

            return CheckTableConversion<bool>(builder, ColumnConversion.ToBoolean, BrightDataType.Boolean);
        }

        [Fact]
        public Task DateColumnConversion()
        {
            var builder = _context.CreateTableBuilder();
            builder.CreateColumn(BrightDataType.String);
            builder.CreateColumn(BrightDataType.Date);

            void AddRow(string dateStr) => builder.AddRow(dateStr, DateTime.Parse(dateStr));

            var date = DateTime.Now.Date;
            AddRow(date.ToLongDateString());
            AddRow(date.ToShortDateString());
            AddRow(date.ToString("D"));
            AddRow(date.ToString("f"));
            AddRow(date.ToString("F"));
            AddRow(date.ToString("g"));
            AddRow(date.ToString("G"));
            AddRow(date.ToString("O"));
            AddRow(date.ToString("R"));
            AddRow(date.ToString("s"));
            AddRow(date.ToString("t"));
            AddRow(date.ToString("T"));
            AddRow(date.ToString("u"));
            AddRow(date.ToString("U"));

            return CheckTableConversion<DateTime>(builder, ColumnConversion.ToDateTime, BrightDataType.Date);
        }

        [Fact]
        public Task ByteColumnConversion()
        {
            var builder = _context.CreateTableBuilder();
            builder.CreateColumn(BrightDataType.String);
            builder.CreateColumn(BrightDataType.SByte);

            for (int i = 0, len = sbyte.MaxValue - sbyte.MinValue; i < len; i++) {
                var val = (sbyte)(sbyte.MinValue + i);
                builder.AddRow(val.ToString(), val);
            }
            return CheckTableConversion<sbyte>(builder, ColumnConversion.ToNumeric, BrightDataType.SByte);
        }

        [Fact]
        public Task ShortColumnConversion()
        {
            var builder = _context.CreateTableBuilder();
            builder.CreateColumn(BrightDataType.String);
            builder.CreateColumn(BrightDataType.Short);

            foreach (var val in (short.MaxValue - short.MinValue).AsRange().Shuffle(_context.Random).Take(100).Select(o => short.MinValue + o))
                builder.AddRow(val.ToString(), (short)val);

            return CheckTableConversion<short>(builder, ColumnConversion.ToNumeric, BrightDataType.Short);
        }

        [Fact]
        public Task IntColumnConversion()
        {
            var builder = _context.CreateTableBuilder();
            builder.CreateColumn(BrightDataType.Int);
            builder.CreateColumn(BrightDataType.String);

            builder.AddRow(int.MinValue, int.MinValue.ToString());
            builder.AddRow(0, "0");
            builder.AddRow(int.MaxValue, int.MaxValue.ToString());

            return CheckTableConversion<int>(builder, ColumnConversion.ToNumeric, BrightDataType.Int);
        }

        [Fact]
        public async Task ManyToVector()
        {
            var builder = _context.CreateTableBuilder();
            builder.CreateColumn(BrightDataType.Float);
            builder.CreateColumn(BrightDataType.Float);

            builder.AddRow(0.1f, 0.2f);
            builder.AddRow(0.3f, 0.4f);

            using var tempStreams = _context.CreateTempDataBlockProvider();

            var table = await builder.BuildInMemory();
            var inputColumns = table.GetColumns(0, 1).Cast<IReadOnlyBuffer>().ToArray();
            var vectors = await inputColumns.Vectorise(tempStreams);

            vectors.Size.Should().Be(2);
            var vector = await vectors.GetItem(0);
            vector.Size.Should().Be(2);
        }
    }
}
