using System;
using System.Globalization;
using System.Linq;
using BrightData.DataTable.Builders;
using BrightData.Helper;
using BrightData.UnitTests.Helper;
using BrightWire;
using FluentAssertions;
using Xunit;

namespace BrightData.UnitTests
{
    public class DataTableTests : NumericsBase
    {
        [Fact]
        public void TestColumnTypes()
        {
            var builder = _context.BuildTable();
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
            var dataTable = builder.BuildRowOriented();

            var firstRow = dataTable.AsConvertible().Row(0);
            firstRow.GetTyped<bool>(0).Should().BeTrue();
            firstRow.GetTyped<byte>(1).Should().Be(100);
            firstRow.GetTyped<DateTime>(2).Should().Be(now);
            firstRow.GetTyped<double>(3).Should().Be(1.0 / 3);
            firstRow.GetTyped<float>(4).Should().Be(0.5f);
            firstRow.GetTyped<int>(5).Should().Be(int.MaxValue);
            firstRow.GetTyped<long>(6).Should().Be(long.MaxValue);
            firstRow.GetTyped<string>(7).Should().Be("test");

        }

        static void CompareRows(IDataTableSegment row1, IDataTableSegment row2)
        {
            row1.Size.Should().Be(row2.Size);
            for (uint i = 0; i < row1.Size; i++)
                row1[i].Should().BeEquivalentTo(row2[i]);
        }

        static void CompareTables(IRowOrientedDataTable table1, IRowOrientedDataTable table2)
        {
            var rand = new Random();
            table1.ColumnCount.Should().Be(table2.ColumnCount);
            table1.RowCount.Should().Be(table2.RowCount);
            table1.GetTargetColumn().Should().Be(table2.GetTargetColumn());

            for (uint i = 0; i < 128; i++) {
                var index = (uint)rand.Next((int)table1.RowCount);
                var row1 = table1.Row(index);
                var row2 = table2.Row(index);
                CompareRows(row1, row2);
            }
        }

        static void RandomSample(IConvertibleTable table, Action<uint, IConvertibleRow> callback)
        {
            var rand = new Random();
            for (var i = 0; i < 128; i++) {
                var index = (uint)rand.Next((int)table.DataTable.RowCount);
                callback(index, table.Row(index));
            }
        }

        static IRowOrientedDataTable CreateComplexTable(IBrightDataContext context)
        {
            var builder = context.BuildTable();
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
            return builder.BuildRowOriented();
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

        IRowOrientedDataTable GetSimpleTable()
        {
            var builder = _context.BuildTable();
            builder.AddColumn(BrightDataType.Int, "val");

            for (var i = 0; i < 10000; i++)
                builder.AddRow(i);
            return builder.BuildRowOriented();
        }

        IRowOrientedDataTable GetSimpleTable2()
        {
            var table = GetSimpleTable();
            var table2 = table.Project(r => new object[] { Convert.ToDouble(r[0]) });
            table2!.ColumnTypes[0].Should().Be(BrightDataType.Double);
            return table2;
        }

        [Fact]
        public void TestTableSlice()
        {
            var table = GetSimpleTable();
            var rows = table.CopyRows(Enumerable.Range(5000, 100).Select(i => (uint)i).ToArray()).AsConvertible().Rows().Select(r => r.GetTyped<int>(0)).ToList();

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
            var convertible = table.AsConvertible();
            var analysis = table.ColumnAnalysis(0).Single().MetaData.GetNumericAnalysis();
            var mean = analysis.Mean;
            var stdDev = analysis.PopulationStdDev!.Value;
            var normalised = table.AsColumnOriented().Normalize(NormalizationType.Standard).AsRowOriented().AsConvertible();

            RandomSample(normalised, (index, row) => {
                var val = row.GetTyped<double>(0);
                var prevVal = convertible.Row(index).GetTyped<double>(0);
                var expected = (prevVal - mean) / stdDev;
                DoubleMath.AreApproximatelyEqual(val, expected, 1E-4f).Should().BeTrue();
            });
        }

        [Fact]
        public void TestDataTableClone()
        {
            using var table = CreateComplexTable(_context);
            using var clone = table.Clone();
            CompareTables(table, clone);
        }

        [Fact]
        public void TestDataTableClone2()
        {
            using var table = CreateComplexTable(_context).AsColumnOriented();
            using var clone = table.Clone();
            CompareTables(table.AsRowOriented(), clone.AsRowOriented());
        }

        [Fact]
        public void ConcatTables()
        {
            using var table = CreateComplexTable(_context);
            using var table2 = CreateComplexTable(_context);
            var table3 = table.Concat(table2);
            table3.ColumnCount.Should().Be(table.ColumnCount);
            table3.RowCount.Should().Be(table.RowCount + table2.RowCount);
        }

        [Fact]
        public void ShuffleTables()
        {
            using var table = CreateComplexTable(_context);
            using var shuffled = table.Shuffle();
            shuffled.RowCount.Should().Be(table.RowCount);
        }

        [Fact]
        public void SortTable()
        {
            using var table = CreateComplexTable(_context);
            using var sorted = table.Sort(1, false);
            sorted.FirstRow[1].Should().Be(table.LastRow[1]);
        }

        [Fact]
        public void GroupTable()
        {
            using var table = CreateComplexTable(_context);
            foreach (var (_, rowOrientedDataTable) in table.GroupBy(0)) {
                rowOrientedDataTable[0].Size.Should().Be(table.RowCount / 2);
            }
        }

        [Fact]
        public void TestStandardNormalisation2()
        {
            var table = GetSimpleTable2();
            var table2 = table.AsColumnOriented();
            var analysis = table2.AllColumnAnalysis()[0].GetNumericAnalysis();
            var normalised = table2.Normalize(NormalizationType.Standard).AsRowOriented().AsConvertible();

            RandomSample(normalised, (index, row) =>
            {
                var val = row.GetTyped<double>(0);
                var prevVal = (double)table.Row(index)[0];
                var expected = (prevVal - analysis.Mean) / analysis.PopulationStdDev!;
                DoubleMath.AreApproximatelyEqual(val, expected, 1E-4f).Should().BeTrue();
            });
        }

        [Fact]
        public void TestFeatureScaleNormalisation()
        {
            var table = GetSimpleTable2();
            var table2 = table.AsColumnOriented();
            var analysis = table2.AllColumnAnalysis()[0].GetNumericAnalysis();
            var normalised = table2.Normalize(NormalizationType.FeatureScale).AsRowOriented().AsConvertible();

            RandomSample(normalised, (index, row) =>
            {
                var val = row.GetTyped<double>(0);
                var prevVal = (double)table.Row(index)[0];
                var expected = (prevVal - analysis.Min) / (analysis.Max - analysis.Min);
                DoubleMath.AreApproximatelyEqual(val, expected, 1E-4f).Should().BeTrue();
            });
        }

        [Fact]
        public void TestL2Normalisation()
        {
            var table = GetSimpleTable2();
            var table2 = table.AsColumnOriented();
            var analysis = table2.AllColumnAnalysis()[0].GetNumericAnalysis();
            var normalised = table2.Normalize(NormalizationType.Euclidean).AsRowOriented().AsConvertible();

            var l2Norm = Math.Sqrt(table2.GetColumn<double>(0).EnumerateTyped().Select(d => Math.Pow(d, 2)).Sum());
            analysis.L2Norm.Should().Be(l2Norm);

            RandomSample(normalised, (index, row) =>
            {
                var val = row.GetTyped<double>(0);
                var prevVal = table.Row(index).Get<double>(0);
                var expected = prevVal / analysis.L2Norm;
                DoubleMath.AreApproximatelyEqual(val, expected, 1E-4f).Should().BeTrue();
            });
        }

        [Fact]
        public void TestL1Normalisation()
        {
            var table = GetSimpleTable2();
            var table2 = table.AsColumnOriented();
            var analysis = table2.AllColumnAnalysis()[0].GetNumericAnalysis();
            var normalised = table2.Normalize(NormalizationType.Manhattan).AsRowOriented().AsConvertible();

            var l1Norm = table2.GetColumn<double>(0).EnumerateTyped().Select(Math.Abs).Sum();
            analysis.L1Norm.Should().Be(l1Norm);

            RandomSample(normalised, (index, row) =>
            {
                var val = row.GetTyped<double>(0);
                var prevVal = table.Row(index).Get<double>(0);
                var expected = prevVal / analysis.L1Norm;
                DoubleMath.AreApproximatelyEqual(val, expected, 1E-4f).Should().BeTrue();
            });
        }

        [Fact]
        public void TestTargetColumnIndex()
        {
            var builder = _context.BuildTable();
            builder.AddColumn(BrightDataType.String, "a");
            builder.AddColumn(BrightDataType.String, "b").SetTarget(true);
            builder.AddColumn(BrightDataType.String, "c");
            builder.AddRow("a", "b", "c");
            var table = builder.BuildRowOriented();

            table.GetTargetColumnOrThrow().Should().Be(1);
            table.RowCount.Should().Be(1);
            table.ColumnCount.Should().Be(3);
        }

        [Fact]
        public void AsMatrix()
        {
            var builder = _context.BuildTable();
            builder.AddColumn(BrightDataType.Float, "val1");
            builder.AddColumn(BrightDataType.Double, "val2");
            builder.AddColumn(BrightDataType.String, "cls").SetTarget(true);

            builder.AddRow(0.5f, 1.1, "a");
            builder.AddRow(0.2f, 1.5, "b");
            builder.AddRow(0.7f, 0.5, "c");
            builder.AddRow(0.2f, 0.6, "d");

            var table = builder.BuildColumnOriented();
            var matrix = table.AsMatrix(0, 1);
            matrix[0, 0].Should().Be(0.5f);
            matrix[1, 0].Should().Be(0.2f);
            matrix[1, 1].Should().Be(1.5f);
        }

        [Fact]
        public void AsMatrix2()
        {
            var builder = _context.BuildTable();
            builder.AddColumn(BrightDataType.Float, "val1");
            builder.AddColumn(BrightDataType.Double, "val2");
            builder.AddColumn(BrightDataType.String, "cls").SetTarget(true);

            builder.AddRow(0.5f, 1.1, "a");
            builder.AddRow(0.2f, 1.5, "b");
            builder.AddRow(0.7f, 0.5, "c");
            builder.AddRow(0.2f, 0.6, "d");

            var table = builder.BuildRowOriented();
            var matrix = table.AsMatrix(0, 1);
            matrix[0, 0].Should().Be(0.5f);
            matrix[1, 0].Should().Be(0.2f);
            matrix[1, 1].Should().Be(1.5f);
        }

        [Fact]
        public void SelectColumns()
        {
            var builder = _context.BuildTable();

            builder.AddColumn(BrightDataType.Float, "val1");
            builder.AddColumn(BrightDataType.Double, "val2");
            builder.AddColumn(BrightDataType.String, "cls").SetTarget(true);
            builder.AddColumn(BrightDataType.String, "cls2");

            builder.AddRow(0.5f, 1.1, "a", "a2");
            builder.AddRow(0.2f, 1.5, "b", "b2");
            builder.AddRow(0.7f, 0.5, "c", "c2");
            builder.AddRow(0.2f, 0.6, "d", "d2");

            var table = builder.BuildColumnOriented();
            var table2 = table.CopyColumns(1, 2, 3);

            table2.GetTargetColumnOrThrow().Should().Be(1);
            table2.RowCount.Should().Be(4);
            table2.ColumnCount.Should().Be(3);

            var column = table2.Column(0).As<IDataTableSegment<double>>();
            var columnValues = column.EnumerateTyped().ToArray();
            columnValues[0].Should().Be(1.1);
            columnValues[1].Should().Be(1.5);
        }

        [Fact]
        public void Fold()
        {
            var builder = _context.BuildTable();

            builder.AddColumn(BrightDataType.Float, "val1");
            builder.AddColumn(BrightDataType.Double, "val2");
            builder.AddColumn(BrightDataType.String, "cls").SetTarget(true);

            builder.AddRow(0.5f, 1.1, "a");
            builder.AddRow(0.2f, 1.5, "b");
            builder.AddRow(0.7f, 0.5, "c");
            builder.AddRow(0.2f, 0.6, "d");

            var table = builder.BuildRowOriented();
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
            var builder = _context.BuildTable();

            builder.AddColumn(BrightDataType.Float, "val1");
            builder.AddColumn(BrightDataType.Double, "val2");
            builder.AddColumn(BrightDataType.String, "cls").SetTarget(true);

            builder.AddRow(0.5f, 1.1, "a");
            builder.AddRow(0.2f, 1.5, "b");
            builder.AddRow(0.7f, 0.5, "c");
            builder.AddRow(0.2f, 0.6, "d");

            var table = builder.BuildRowOriented();
            var projectedTable = table.Project(r => (string)r[2] == "b" ? null : r);

            projectedTable!.ColumnCount.Should().Be(table.ColumnCount);
            projectedTable.RowCount.Should().Be(3);
        }

        [Fact]
        public void TableConfusionMatrix()
        {
            var builder = _context.BuildTable();

            builder.AddColumn(BrightDataType.String, "actual");
            builder.AddColumn(BrightDataType.String, "expected");

            const int CAT_CAT = 5;
            const int CAT_DOG = 2;
            const int DOG_CAT = 3;
            const int DOG_DOG = 5;
            const int DOG_RABBIT = 2;
            const int RABBIT_DOG = 1;
            const int RABBIT_RABBIT = 11;

            for (var i = 0; i < CAT_CAT; i++)
                builder.AddRow("cat", "cat");
            for (var i = 0; i < CAT_DOG; i++)
                builder.AddRow("cat", "dog");
            for (var i = 0; i < DOG_CAT; i++)
                builder.AddRow("dog", "cat");
            for (var i = 0; i < DOG_DOG; i++)
                builder.AddRow("dog", "dog");
            for (var i = 0; i < DOG_RABBIT; i++)
                builder.AddRow("dog", "rabbit");
            for (var i = 0; i < RABBIT_DOG; i++)
                builder.AddRow("rabbit", "dog");
            for (var i = 0; i < RABBIT_RABBIT; i++)
                builder.AddRow("rabbit", "rabbit");
            var table = builder.BuildRowOriented();
            var confusionMatrix = table.AsConvertible().CreateConfusionMatrix(1, 0);

            confusionMatrix.GetCount("cat", "dog").Should().Be(CAT_DOG);
            confusionMatrix.GetCount("dog", "rabbit").Should().Be(DOG_RABBIT);
            confusionMatrix.GetCount("rabbit", "rabbit").Should().Be(RABBIT_RABBIT);
        }

        static void CheckTableConversion<T>(InMemoryTableBuilder builder, ColumnConversionType conversionType, BrightDataType columnType)
        {
            var table = builder.BuildColumnOriented();
            var converted = table.Convert(conversionType.ConvertColumn(0), conversionType.ConvertColumn(1)).AsRowOriented();
            converted.ColumnTypes[0].Should().Be(columnType);
            converted.ColumnTypes[1].Should().Be(columnType);
            converted.ForEachRow<T, T>((b1, b2) => b1.Should().Be(b2));
        }

        [Fact]
        public void BooleanColumnConversion()
        {
            var builder = _context.BuildTable();
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

            CheckTableConversion<bool>(builder, ColumnConversionType.ToBoolean, BrightDataType.Boolean);
        }

        [Fact]
        public void DateColumnConversion()
        {
            var builder = _context.BuildTable();
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

            CheckTableConversion<DateTime>(builder, ColumnConversionType.ToDate, BrightDataType.Date);
        }

        [Fact]
        public void ByteColumnConversion()
        {
            var builder = _context.BuildTable();
            builder.AddColumn(BrightDataType.String);
            builder.AddColumn(BrightDataType.SByte);

            for (int i = 0, len = sbyte.MaxValue - sbyte.MinValue; i < len; i++) {
                var val = (sbyte) (sbyte.MinValue + i);
                builder.AddRow(val.ToString(), val);
            }
            CheckTableConversion<sbyte>(builder, ColumnConversionType.ToNumeric, BrightDataType.SByte);
        }

        [Fact]
        public void ShortColumnConversion()
        {
            var builder = _context.BuildTable();
            builder.AddColumn(BrightDataType.String);
            builder.AddColumn(BrightDataType.Short);

            foreach(var val in (short.MaxValue - short.MinValue).AsRange().Shuffle(_context.Random).Take(100).Select(o => short.MinValue + o))
                builder.AddRow(val.ToString(), val);

            CheckTableConversion<short>(builder, ColumnConversionType.ToNumeric, BrightDataType.Short);
        }

        [Fact]
        public void IntColumnConversion()
        {
            var builder = _context.BuildTable();
            builder.AddColumn(BrightDataType.String);
            builder.AddColumn(BrightDataType.Int);

            builder.AddRow(int.MinValue, int.MinValue.ToString());
            builder.AddRow(0, "0");
            builder.AddRow(int.MaxValue, int.MaxValue.ToString());

            CheckTableConversion<int>(builder, ColumnConversionType.ToNumeric, BrightDataType.Int);
        }

        
    }
}
