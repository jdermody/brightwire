using System;
using System.Linq;
using BrightData.DataTable;
using BrightData.Helper;
using BrightData.UnitTests.Helper;
using BrightWire;
using FluentAssertions;
using Xunit;
using BrightDataTable = BrightData.DataTable.BrightDataTable;

namespace BrightData.UnitTests
{
    public class DataTableTests : CpuBase
    {
        [Fact]
        public void TestColumnTypes()
        {
            var builder = _context.CreateTableBuilder();
            builder.AddColumn(BrightDataType.Boolean, "boolean");
            builder.AddColumn(BrightDataType.SByte, "byte");
            builder.AddColumn(BrightDataType.Date, "date");
            builder.AddColumn(BrightDataType.Double, "double");
            builder.AddColumn(BrightDataType.Float, "float");
            builder.AddColumn(BrightDataType.Int, "int");
            builder.AddColumn(BrightDataType.Long, "long");
            builder.AddColumn(BrightDataType.String, "string");

            var now = DateTime.Now;
            builder.AddRow(true, (sbyte)100, now, 1.0 / 3, 0.5f, int.MaxValue, long.MaxValue, "test");
            var dataTable = builder.BuildInMemory();

            var firstRow = dataTable.GetRow(0);
            firstRow.Get<bool>(0).Should().BeTrue();
            firstRow.Get<byte>(1).Should().Be(100);
            firstRow.Get<DateTime>(2).Should().Be(now);
            firstRow.Get<double>(3).Should().Be(1.0 / 3);
            firstRow.Get<float>(4).Should().Be(0.5f);
            firstRow.Get<int>(5).Should().Be(int.MaxValue);
            firstRow.Get<long>(6).Should().Be(long.MaxValue);
            firstRow.Get<string>(7).Should().Be("test");

        }

        static void CompareRows(BrightDataTableRow row1, BrightDataTableRow row2)
        {
            row1.Size.Should().Be(row2.Size);
            for (uint i = 0; i < row1.Size; i++)
                row1[i].Should().BeEquivalentTo(row2[i]);
        }

        static void CompareTables(BrightDataTable table1, BrightDataTable table2)
        {
            var rand = new Random();
            table1.ColumnCount.Should().Be(table2.ColumnCount);
            table1.RowCount.Should().Be(table2.RowCount);
            table1.GetTargetColumn().Should().Be(table2.GetTargetColumn());

            for (uint i = 0; i < 128; i++) {
                var index = (uint)rand.Next((int)table1.RowCount);
                var row1 = table1.GetRow(index);
                var row2 = table2.GetRow(index);
                CompareRows(row1, row2);
            }
        }

        static void RandomSample(uint rowCount, BrightDataTable table, Action<uint, BrightDataTableRow> callback)
        {
            var rand = new Random();
            for (var i = 0; i < 128; i++) {
                var index = (uint)rand.Next((int)rowCount);
                var row = table.GetRow(index);
                callback(index, row);
            }
        }

        static BrightDataTable CreateComplexTable(BrightDataContext context)
        {
            var builder = context.CreateTableBuilder();
            builder.AddColumn(BrightDataType.Boolean, "boolean");
            builder.AddColumn(BrightDataType.SByte, "byte");
            builder.AddColumn(BrightDataType.Date, "date");
            builder.AddColumn(BrightDataType.Double, "double");
            builder.AddColumn(BrightDataType.Float, "float");
            builder.AddColumn(BrightDataType.Int, "int");
            builder.AddColumn(BrightDataType.Long, "long");
            builder.AddColumn(BrightDataType.String, "string");

            for (var i = 1; i <= 10; i++)
                builder.AddRow(i % 2 == 0, (sbyte)i, DateTime.Now, (double)i, (float)i, i, (long)i, i.ToString());
            return builder.BuildInMemory();
        }

        [Fact]
        public void TestDataTableAnalysis()
        {
            var table = CreateComplexTable(_context);
            var analysis = table.AllColumnAnalysis();

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

        BrightDataTable GetSimpleTable()
        {
            var builder = _context.CreateTableBuilder();
            builder.AddColumn(BrightDataType.Int, "val");

            for (var i = 0; i < 10000; i++)
                builder.AddRow(i);
            return builder.BuildInMemory();
        }

        BrightDataTable GetSimpleTable2()
        {
            var table = GetSimpleTable();
            var table2 = table.Project(null, r => new object[] { Convert.ToDouble(r[0]) });
            table2.ColumnTypes[0].Should().Be(BrightDataType.Double);
            return table2;
        }

        [Fact]
        public void TestTableSlice()
        {
            var table = GetSimpleTable();
            var rows = table.CopyRowsToNewTable(null, Enumerable.Range(5000, 100).Select(i => (uint)i).ToArray()).GetRows().Select(r => r.Get<int>(0)).ToList();

            for (var i = 0; i < 100; i++)
                rows[i].Should().Be(5000 + i);
        }

        [Fact]
        public void TestTableSplit()
        {
            var table = GetSimpleTable();
            var (training, test) = table.Split(0.75);
            training.RowCount.Should().Be(7500);
            test.RowCount.Should().Be(2500);
        }

        [Fact]
        public void TestStandardNormalisation()
        {
            var table = GetSimpleTable2();
            var analysis = table.GetColumnAnalysis(0).GetNumericAnalysis();
            var mean = analysis.Mean;
            var stdDev = analysis.PopulationStdDev!.Value;
            var normalised = table.Normalize(NormalizationType.Standard);

            RandomSample(table.RowCount, normalised, (index, row) => {
                var val = row.Get<double>(0);
                var prevRow = table.GetRow(index);
                var prevVal = prevRow.Get<double>(0);
                var expected = (prevVal - mean) / stdDev;
                DoubleMath.AreApproximatelyEqual(val, expected, 1E-4f).Should().BeTrue();
            });
        }

        [Fact]
        public void TestDataTableClone()
        {
            using var table = CreateComplexTable(_context);
            using var clone = table.Clone(null);
            CompareTables(table, clone);
        }

        [Fact]
        public void ConcatTables()
        {
            using var table = CreateComplexTable(_context);
            using var table2 = CreateComplexTable(_context);
            var table3 = table.ConcatenateRows(table2);
            table3.ColumnCount.Should().Be(table.ColumnCount);
            table3.RowCount.Should().Be(table.RowCount + table2.RowCount);
        }

        [Fact]
        public void ShuffleTables()
        {
            using var table = CreateComplexTable(_context);
            using var shuffled = table.Shuffle(null);
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
        public void GroupTable()
        {
            using var table = CreateComplexTable(_context);
            foreach (var (_, dataTable) in table.GroupBy(0)) {
                dataTable.RowCount.Should().Be(table.RowCount / 2);
            }
        }

        [Fact]
        public void TestStandardNormalisation2()
        {
            var table = GetSimpleTable2();
            var analysis = table.AllColumnAnalysis()[0].GetNumericAnalysis();
            var normalised = table.Normalize(NormalizationType.Standard);

            RandomSample(table.RowCount, normalised, (index, row) => {
                var val = row.Get<double>(0);
                var prevRow = table.GetRow(index);
                var prevVal = (double)prevRow[0];
                var expected = (prevVal - analysis.Mean) / analysis.PopulationStdDev!;
                DoubleMath.AreApproximatelyEqual(val, expected, 1E-4f).Should().BeTrue();
            });
        }

        [Fact]
        public void TestFeatureScaleNormalisation()
        {
            var table = GetSimpleTable2();
            var analysis = table.AllColumnAnalysis()[0].GetNumericAnalysis();
            var normalised = table.Normalize(NormalizationType.FeatureScale);

            RandomSample(table.RowCount, normalised, (index, row) => {
                var val = row.Get<double>(0);
                var prevRow = table.GetRow(index);
                var prevVal = (double)prevRow[0];
                var expected = (prevVal - analysis.Min) / (analysis.Max - analysis.Min);
                DoubleMath.AreApproximatelyEqual(val, expected, 1E-4f).Should().BeTrue();
            });
        }

        [Fact]
        public void TestL2Normalisation()
        {
            var table = GetSimpleTable2();
            var analysis = table.AllColumnAnalysis()[0].GetNumericAnalysis();
            var normalised = table.Normalize(NormalizationType.Euclidean);

            var l2Norm = Math.Sqrt(table.GetColumn<double>(0).Values.Select(d => Math.Pow(d, 2)).Sum());
            analysis.L2Norm.Should().Be(l2Norm);

            RandomSample(table.RowCount, normalised, (index, row) => {
                var val = row.Get<double>(0);
                var prevRow = table.GetRow(index);
                var prevVal = prevRow.Get<double>(0);
                var expected = prevVal / analysis.L2Norm;
                DoubleMath.AreApproximatelyEqual(val, expected, 1E-4f).Should().BeTrue();
            });
        }

        [Fact]
        public void TestL1Normalisation()
        {
            var table = GetSimpleTable2();
            var analysis = table.AllColumnAnalysis()[0].GetNumericAnalysis();
            var normalised = table.Normalize(NormalizationType.Manhattan);

            var l1Norm = table.GetColumn<double>(0).Values.Select(Math.Abs).Sum();
            analysis.L1Norm.Should().Be(l1Norm);

            RandomSample(table.RowCount, normalised, (index, row) => {
                var val = row.Get<double>(0);
                var prevRow = table.GetRow(index);
                var prevVal = prevRow.Get<double>(0);
                var expected = prevVal / analysis.L1Norm;
                DoubleMath.AreApproximatelyEqual(val, expected, 1E-4f).Should().BeTrue();
            });
        }

        [Fact]
        public void TestTargetColumnIndex()
        {
            var builder = _context.CreateTableBuilder();
            builder.AddColumn(BrightDataType.String, "a");
            builder.AddColumn(BrightDataType.String, "b").MetaData.SetTarget(true);
            builder.AddColumn(BrightDataType.String, "c");
            builder.AddRow("a", "b", "c");
            var table = builder.BuildInMemory();

            table.GetTargetColumnOrThrow().Should().Be(1);
            table.RowCount.Should().Be(1);
            table.ColumnCount.Should().Be(3);
        }

        [Fact]
        public void AsMatrix()
        {
            var builder = _context.CreateTableBuilder();
            builder.AddColumn(BrightDataType.Float, "val1");
            builder.AddColumn(BrightDataType.Double, "val2");
            builder.AddColumn(BrightDataType.String, "cls").MetaData.SetTarget(true);

            builder.AddRow(0.5f, 1.1, "a");
            builder.AddRow(0.2f, 1.5, "b");
            builder.AddRow(0.7f, 0.5, "c");
            builder.AddRow(0.2f, 0.6, "d");

            var table = builder.BuildInMemory();
            var matrix = table.AsMatrix(0, 1);
            matrix[0, 0].Should().Be(0.5f);
            matrix[1, 0].Should().Be(0.2f);
            matrix[1, 1].Should().Be(1.5f);
        }

        [Fact]
        public void AsMatrix2()
        {
            var builder = _context.CreateTableBuilder();
            builder.AddColumn(BrightDataType.Float, "val1");
            builder.AddColumn(BrightDataType.Double, "val2");
            builder.AddColumn(BrightDataType.String, "cls").MetaData.SetTarget(true);

            builder.AddRow(0.5f, 1.1, "a");
            builder.AddRow(0.2f, 1.5, "b");
            builder.AddRow(0.7f, 0.5, "c");
            builder.AddRow(0.2f, 0.6, "d");

            var table = builder.BuildInMemory();
            var matrix = table.AsMatrix(0, 1);
            matrix[0, 0].Should().Be(0.5f);
            matrix[1, 0].Should().Be(0.2f);
            matrix[1, 1].Should().Be(1.5f);
        }

        [Fact]
        public void SelectColumns()
        {
            var builder = _context.CreateTableBuilder();

            builder.AddColumn(BrightDataType.Float, "val1");
            builder.AddColumn(BrightDataType.Double, "val2");
            builder.AddColumn(BrightDataType.String, "cls").MetaData.SetTarget(true);
            builder.AddColumn(BrightDataType.String, "cls2");

            builder.AddRow(0.5f, 1.1, "a", "a2");
            builder.AddRow(0.2f, 1.5, "b", "b2");
            builder.AddRow(0.7f, 0.5, "c", "c2");
            builder.AddRow(0.2f, 0.6, "d", "d2");

            var table = builder.BuildInMemory();
            var table2 = table.CopyColumnsToNewTable(null, 1, 2, 3);

            table2.GetTargetColumnOrThrow().Should().Be(1);
            table2.RowCount.Should().Be(4);
            table2.ColumnCount.Should().Be(3);

            using var column = table2.GetColumn(0).As<ITypedSegment<double>>();
            var columnValues = column.Values.ToArray();
            columnValues[0].Should().Be(1.1);
            columnValues[1].Should().Be(1.5);
        }

        [Fact]
        public void Fold()
        {
            var builder = _context.CreateTableBuilder();

            builder.AddColumn(BrightDataType.Float, "val1");
            builder.AddColumn(BrightDataType.Double, "val2");
            builder.AddColumn(BrightDataType.String, "cls").MetaData.SetTarget(true);

            builder.AddRow(0.5f, 1.1, "a");
            builder.AddRow(0.2f, 1.5, "b");
            builder.AddRow(0.7f, 0.5, "c");
            builder.AddRow(0.2f, 0.6, "d");

            var table = builder.BuildInMemory();
            var folds = table.Fold(4).ToList();
            folds.Count.Should().Be(4);
            foreach (var (training, validation) in folds) {
                training.RowCount.Should().Be(3);
                validation.RowCount.Should().Be(1);
            }
        }

        [Fact]
        public void TableFilter()
        {
            var builder = _context.CreateTableBuilder();

            builder.AddColumn(BrightDataType.Float, "val1");
            builder.AddColumn(BrightDataType.Double, "val2");
            builder.AddColumn(BrightDataType.String, "cls").MetaData.SetTarget(true);

            builder.AddRow(0.5f, 1.1, "a");
            builder.AddRow(0.2f, 1.5, "b");
            builder.AddRow(0.7f, 0.5, "c");
            builder.AddRow(0.2f, 0.6, "d");

            var table = builder.BuildInMemory();
            var projectedTable = table.Project(null, r => (string)r[2] == "b" ? null : r);

            projectedTable.ColumnCount.Should().Be(table.ColumnCount);
            projectedTable.RowCount.Should().Be(3);
        }

        [Fact]
        public void TableConfusionMatrix()
        {
            var builder = _context.CreateTableBuilder();

            builder.AddColumn(BrightDataType.String, "actual");
            builder.AddColumn(BrightDataType.String, "expected");

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
            var table = builder.BuildInMemory();
            var confusionMatrix = table.CreateConfusionMatrix(1, 0);

            confusionMatrix.GetCount("cat", "dog").Should().Be(catDog);
            confusionMatrix.GetCount("dog", "rabbit").Should().Be(dogRabbit);
            confusionMatrix.GetCount("rabbit", "rabbit").Should().Be(rabbitRabbit);
        }

        static void CheckTableConversion<T>(BrightDataTableBuilder builder, ColumnConversionOperation conversionType, BrightDataType columnType)
        {
            var table = builder.BuildInMemory();
            var converted = table.Convert(conversionType.ConvertColumn(0), conversionType.ConvertColumn(1));
            converted.ColumnTypes[0].Should().Be(columnType);
            converted.ColumnTypes[1].Should().Be(columnType);

            foreach (var (b1, b2) in converted.ForEachRow<T, T>())
                b1.Should().Be(b2);
        }

        [Fact]
        public void BooleanColumnConversion()
        {
            var builder = _context.CreateTableBuilder();
            builder.AddColumn(BrightDataType.String);
            builder.AddColumn(BrightDataType.Boolean);

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

            CheckTableConversion<bool>(builder, ColumnConversionOperation.ToBoolean, BrightDataType.Boolean);
        }

        [Fact]
        public void DateColumnConversion()
        {
            var builder = _context.CreateTableBuilder();
            builder.AddColumn(BrightDataType.String);
            builder.AddColumn(BrightDataType.Date);

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

            CheckTableConversion<DateTime>(builder, ColumnConversionOperation.ToDate, BrightDataType.Date);
        }

        [Fact]
        public void ByteColumnConversion()
        {
            var builder = _context.CreateTableBuilder();
            builder.AddColumn(BrightDataType.String);
            builder.AddColumn(BrightDataType.SByte);

            for (int i = 0, len = sbyte.MaxValue - sbyte.MinValue; i < len; i++) {
                var val = (sbyte)(sbyte.MinValue + i);
                builder.AddRow(val.ToString(), val);
            }
            CheckTableConversion<sbyte>(builder, ColumnConversionOperation.ToNumeric, BrightDataType.SByte);
        }

        [Fact]
        public void ShortColumnConversion()
        {
            var builder = _context.CreateTableBuilder();
            builder.AddColumn(BrightDataType.String);
            builder.AddColumn(BrightDataType.Short);

            foreach (var val in (short.MaxValue - short.MinValue).AsRange().Shuffle(_context.Random).Take(100).Select(o => short.MinValue + o))
                builder.AddRow(val.ToString(), (short)val);

            CheckTableConversion<short>(builder, ColumnConversionOperation.ToNumeric, BrightDataType.Short);
        }

        [Fact]
        public void IntColumnConversion()
        {
            var builder = _context.CreateTableBuilder();
            builder.AddColumn(BrightDataType.Int);
            builder.AddColumn(BrightDataType.String);

            builder.AddRow(int.MinValue, int.MinValue.ToString());
            builder.AddRow(0, "0");
            builder.AddRow(int.MaxValue, int.MaxValue.ToString());

            CheckTableConversion<int>(builder, ColumnConversionOperation.ToNumeric, BrightDataType.Int);
        }

        [Fact]
        public void ManyToVector()
        {
            var builder = _context.CreateTableBuilder();
            builder.AddColumn(BrightDataType.Float);
            builder.AddColumn(BrightDataType.Float);

            builder.AddRow(0.1f, 0.2f);
            builder.AddRow(0.3f, 0.4f);

            using var tempStreams = _context.CreateTempStreamProvider();
            var manyToOne = new uint[] { 0, 1 }.ReinterpretColumns(BrightDataType.Vector, "Data");

            var table = builder.BuildInMemory();
            var newTable = table.ReinterpretColumns(tempStreams, null, manyToOne);

            newTable.ColumnCount.Should().Be(1);
            var metaData = newTable.ColumnMetaData[0];
            metaData.GetColumnIndex().Should().Be(0);
            metaData.GetName().Should().Be("Data");
            metaData.GetColumnType().Should().Be(BrightDataType.Vector);
        }
    }
}
