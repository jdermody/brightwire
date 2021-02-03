using System;
using System.Linq;
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
            builder.AddColumn(ColumnType.Boolean, "boolean");
            builder.AddColumn(ColumnType.Byte, "byte");
            builder.AddColumn(ColumnType.Date, "date");
            builder.AddColumn(ColumnType.Double, "double");
            builder.AddColumn(ColumnType.Float, "float");
            builder.AddColumn(ColumnType.Int, "int");
            builder.AddColumn(ColumnType.Long, "long");
            builder.AddColumn(ColumnType.String, "string");

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

        void CompareRows(IDataTableSegment row1, IDataTableSegment row2)
        {
            row1.Size.Should().Be(row2.Size);
            for (uint i = 0; i < row1.Size; i++)
                row1[i].Should().BeEquivalentTo(row2[i]);
        }

        void CompareTables(IRowOrientedDataTable table1, IRowOrientedDataTable table2)
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

        void RandomSample(IConvertibleTable table, Action<uint, IConvertibleRow> callback)
        {
            var rand = new Random();
            for (var i = 0; i < 128; i++) {
                var index = (uint)rand.Next((int)table.DataTable.RowCount);
                callback(index, table.Row(index));
            }
        }

        //[Fact]
        //public void TestIndexHydration()
        //{
        //    using var dataStream = new MemoryStream();
        //    using var indexStream = new MemoryStream();

        //    var builder = _context.BuildTable();
        //    builder.AddColumn(ColumnType.Boolean, "target").SetTargetColumn(true);
        //    builder.AddColumn(ColumnType.Int, "val");
        //    builder.AddColumn(ColumnType.String, "label");
        //    for (var i = 0; i < 33000; i++)
        //        builder.AddRow(i % 2 == 0, i, i.ToString());

        //    var table = builder.Build();
        //    builder.WriteIndexTo(indexStream);

        //    dataStream.Seek(0, SeekOrigin.Begin);
        //    indexStream.Seek(0, SeekOrigin.Begin);
        //    var newTable = _context.BuildTable(dataStream, indexStream);
        //    CompareTables(table, newTable);

        //    dataStream.Seek(0, SeekOrigin.Begin);
        //    var newTable2 = _context.BuildTable(dataStream, null);
        //    CompareTables(table, newTable2);
        //}

        static IDataTable CreateComplexTable(IBrightDataContext context)
        {
            var builder = context.BuildTable();
            builder.AddColumn(ColumnType.Boolean, "boolean");
            builder.AddColumn(ColumnType.Byte, "byte");
            builder.AddColumn(ColumnType.Date, "date");
            builder.AddColumn(ColumnType.Double, "double");
            builder.AddColumn(ColumnType.Float, "float");
            builder.AddColumn(ColumnType.Int, "int");
            builder.AddColumn(ColumnType.Long, "long");
            builder.AddColumn(ColumnType.String, "string");

            for (var i = 1; i <= 10; i++)
                builder.AddRow(i % 2 == 0, (sbyte)i, DateTime.Now, (double)i, (float)i, i, (long)i, i.ToString());
            return builder.BuildRowOriented();
        }

        [Fact]
        public void TestDataTableAnalysis()
        {
            var table = CreateComplexTable(_context);
            var analysis = table.GetColumnAnalysis();

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
            numericAnalysis.All(a => Math.Round(a.SampleStdDev.Value) == 3).Should().BeTrue();

            var stringAnalysis = analysis[7].GetStringAnalysis();
            stringAnalysis.NumDistinct.Should().Be(10);
            stringAnalysis.MaxLength.Should().Be(2);
        }

        IRowOrientedDataTable GetSimpleTable()
        {
            var builder = _context.BuildTable();
            builder.AddColumn(ColumnType.Int, "val");

            for (var i = 0; i < 10000; i++)
                builder.AddRow(i);
            return builder.BuildRowOriented();
        }

        IRowOrientedDataTable GetSimpleTable2()
        {
            var table = GetSimpleTable();
            var table2 = table.Project(r => new object[] { Convert.ToDouble(r[0]) });
            table2.ColumnTypes[0].Should().Be(ColumnType.Double);
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
            var analysis = table.GetColumnAnalysis()[0].GetNumericAnalysis();
            var mean = analysis.Mean;
            var stdDev = analysis.PopulationStdDev.Value;
            var normalised = table.AsColumnOriented().Normalize(NormalizationType.Standard).AsRowOriented().AsConvertible();

            RandomSample(normalised, (index, row) => {
                var val = row.GetTyped<double>(0);
                var prevVal = convertible.Row(index).GetTyped<double>(0);
                var expected = (prevVal - mean) / stdDev;
                val.Should().Be(expected);
            });
        }

        //[Fact]
        //public void TestStandardNormalisation2()
        //{
        //    var table = GetSimpleTable2();
        //    var analysis = table.GetColumnAnalysis()[0];
        //    var normalised = table.AsColumnOriented().Normalize(NormalizationType.Standard).AsRowOriented().AsConvertible();

        //    RandomSample(normalised, (index, row) => {
        //        var val = row.GetTyped<double>(0);
        //        var prevVal = (double)table.Row(index)[0];
        //        var expected = (prevVal - analysis.Mean) / analysis.StdDev.Value;

        //        Assert.AreEqual(val, expected);
        //    });
        //}

        //[Fact]
        //public void TestFeatureScaleNormalisation()
        //{
        //    var table = GetSimpleTable2();
        //    var analysis = table.GetAnalysis()[0] as INumericColumnInfo;
        //    var normalised = table.Normalize(NormalisationType.FeatureScale);

        //    RandomSample(normalised, (index, row) => {
        //        var val = row.GetTyped<double>(0);
        //        var prevVal = table.GetRow(index).GetField<double>(0);
        //        var expected = (prevVal - analysis.Min) / (analysis.Max - analysis.Min);

        //        Assert.AreEqual(val, expected);
        //    });
        //}

        //[Fact]
        //public void TestFeatureScaleNormalisation2()
        //{
        //    var table = GetSimpleTable2();
        //    var analysis = table.GetAnalysis()[0] as INumericColumnInfo;
        //    var model = table.GetNormalisationModel(NormalisationType.FeatureScale);
        //    var normalised = table.Normalize(model);

        //    RandomSample(normalised, (index, row) => {
        //        var val = row.GetTyped<double>(0);
        //        var prevVal = table.GetRow(index).GetField<double>(0);
        //        var expected = (prevVal - analysis.Min) / (analysis.Max - analysis.Min);

        //        Assert.AreEqual(val, expected);
        //    });
        //}

        //[Fact]
        //public void TestL2Normalisation()
        //{
        //    var table = GetSimpleTable2();
        //    var analysis = table.GetAnalysis()[0] as INumericColumnInfo;
        //    var normalised = table.Normalize(NormalisationType.Euclidean);

        //    var l2Norm = Math.Sqrt(table.GetColumn<double>(0).Select(d => Math.Pow(d, 2)).Sum());
        //    Assert.AreEqual(analysis.L2Norm, l2Norm);

        //    RandomSample(normalised, (index, row) => {
        //        var val = row.GetTyped<double>(0);
        //        var prevVal = table.GetRow(index).GetField<double>(0);
        //        var expected = prevVal / analysis.L2Norm;

        //        Assert.AreEqual(val, expected);
        //    });
        //}

        //[Fact]
        //public void TestL2Normalisation2()
        //{
        //    var table = GetSimpleTable2();
        //    var analysis = table.GetAnalysis()[0] as INumericColumnInfo;
        //    var model = table.GetNormalisationModel(NormalisationType.Euclidean);
        //    var normalised = table.Normalize(model);

        //    var l2Norm = Math.Sqrt(table.GetColumn<double>(0).Select(d => Math.Pow(d, 2)).Sum());
        //    Assert.AreEqual(analysis.L2Norm, l2Norm);

        //    RandomSample(normalised, (index, row) => {
        //        var val = row.GetTyped<double>(0);
        //        var prevVal = table.GetRow(index).GetField<double>(0);
        //        var expected = prevVal / analysis.L2Norm;

        //        Assert.AreEqual(val, expected);
        //    });
        //}

        //[Fact]
        //public void TestL1Normalisation()
        //{
        //    var table = GetSimpleTable2();
        //    var analysis = table.GetAnalysis()[0] as INumericColumnInfo;
        //    var normalised = table.Normalize(NormalisationType.Manhattan);

        //    var l1Norm = table.GetColumn<double>(0).Select(d => Math.Abs(d)).Sum();
        //    Assert.AreEqual(analysis.L1Norm, l1Norm);

        //    RandomSample(normalised, (index, row) => {
        //        var val = row.GetTyped<double>(0);
        //        var prevVal = table.GetRow(index).GetField<double>(0);
        //        var expected = prevVal / analysis.L1Norm;

        //        Assert.AreEqual(val, expected);
        //    });
        //}

        //[Fact]
        //public void TestL1Normalisation2()
        //{
        //    var table = GetSimpleTable2();
        //    var analysis = table.GetAnalysis()[0] as INumericColumnInfo;
        //    var model = table.GetNormalisationModel(NormalisationType.Manhattan);
        //    var normalised = table.Normalize(model);

        //    var l1Norm = table.GetColumn<double>(0).Select(d => Math.Abs(d)).Sum();
        //    Assert.AreEqual(analysis.L1Norm, l1Norm);

        //    RandomSample(normalised, (index, row) => {
        //        var val = row.GetTyped<double>(0);
        //        var prevVal = table.GetRow(index).GetField<double>(0);
        //        var expected = prevVal / analysis.L1Norm;

        //        Assert.AreEqual(val, expected);
        //    });
        //}

        //[Fact]
        //public void TestReverseNormalisation()
        //{
        //    var table = CreateComplexTable(_context);
        //    var columnIndex = table.TargetColumnIndex = 3;
        //    var model = table.GetNormalisationModel(NormalisationType.FeatureScale);
        //    var normalised = table.Normalize(model);

        //    var reverseNormalised = normalised.Project(row => new[] { model.ReverseNormaliseOutput(columnIndex, row.Data[columnIndex]) });
        //    var zipped = reverseNormalised.Zip(table.SelectColumns(new[] { columnIndex }));

        //    zipped.ForEach(row => Assert.AreEqual(row.Data[0], row.Data[1]));
        //}

        //[Fact]
        //public void TestTargetColumnIndex()
        //{
        //    var builder = _context.BuildTable();
        //    builder.AddColumn(ColumnType.String, "a");
        //    builder.AddColumn(ColumnType.String, "b").SetTargetColumn(true);
        //    builder.AddColumn(ColumnType.String, "c");
        //    builder.AddRow("a", "b", "c");
        //    var table = builder.Build();

        //    Assert.AreEqual(table.TargetColumnIndex, 1);
        //    Assert.AreEqual(table.RowCount, 1);
        //    Assert.AreEqual(table.ColumnCount, 3);
        //}

        //[Fact]
        //public void GetNumericRows()
        //{
        //    var builder = _context.BuildTable();
        //    builder.AddColumn(ColumnType.Float, "val1");
        //    builder.AddColumn(ColumnType.Double, "val2");
        //    builder.AddColumn(ColumnType.String, "cls").SetTargetColumn(true);

        //    builder.AddRow(0.5f, 1.1, "a");
        //    builder.AddRow(0.2f, 1.5, "b");
        //    builder.AddRow(0.7f, 0.5, "c");
        //    builder.AddRow(0.2f, 0.6, "d");

        //    var table = builder.Build();
        //    var rows = table.GetNumericRows(new[] { 1 }).Select(r => _lap.CreateVector(r)).Select(r => r.AsIndexable()).ToList();
        //    Assert.AreEqual(rows[0][0], 1.1f);
        //    Assert.AreEqual(rows[1][0], 1.5f);
        //}

        //[Fact]
        //public void GetNumericRows2()
        //{
        //    var builder = _context.BuildTable();
        //    builder.AddColumn(ColumnType.Float, "val1");
        //    builder.AddColumn(ColumnType.Double, "val2");
        //    builder.AddColumn(ColumnType.String, "cls").SetTargetColumn(true);

        //    builder.AddRow(0.5f, 1.1, "a");
        //    builder.AddRow(0.2f, 1.5, "b");
        //    builder.AddRow(0.7f, 0.5, "c");
        //    builder.AddRow(0.2f, 0.6, "d");

        //    var table = builder.Build();
        //    var rows = table.GetNumericRows(new[] { 1 });
        //    Assert.AreEqual(rows[0][0], 1.1f);
        //    Assert.AreEqual(rows[1][0], 1.5f);
        //}

        //[Fact]
        //public void GetNumericColumns()
        //{
        //    var builder = _context.BuildTable();
        //    builder.AddColumn(ColumnType.Float, "val1");
        //    builder.AddColumn(ColumnType.Double, "val2");
        //    builder.AddColumn(ColumnType.String, "cls").SetTargetColumn(true);

        //    builder.AddRow(0.5f, 1.1, "a");
        //    builder.AddRow(0.2f, 1.5, "b");
        //    builder.AddRow(0.7f, 0.5, "c");
        //    builder.AddRow(0.2f, 0.6, "d");

        //    var table = builder.Build();
        //    var column = table.GetNumericColumns(new[] { 1 }).Select(r => _lap.CreateVector(r)).First().AsIndexable();
        //    Assert.AreEqual(column[0], 1.1f);
        //    Assert.AreEqual(column[1], 1.5f);
        //}

        //[Fact]
        //public void GetNumericColumns2()
        //{
        //    var builder = _context.BuildTable();
        //    builder.AddColumn(ColumnType.Float, "val1");
        //    builder.AddColumn(ColumnType.Double, "val2");
        //    builder.AddColumn(ColumnType.String, "cls").SetTargetColumn(true);

        //    builder.AddRow(0.5f, 1.1, "a");
        //    builder.AddRow(0.2f, 1.5, "b");
        //    builder.AddRow(0.7f, 0.5, "c");
        //    builder.AddRow(0.2f, 0.6, "d");

        //    var table = builder.Build();
        //    var column = table.GetNumericColumns(new[] { 1 }).First();
        //    Assert.AreEqual(column[0], 1.1f);
        //    Assert.AreEqual(column[1], 1.5f);
        //}

        //[Fact]
        //public void SelectColumns()
        //{
        //    var builder = _context.BuildTable();

        //    builder.AddColumn(ColumnType.Float, "val1");
        //    builder.AddColumn(ColumnType.Double, "val2");
        //    builder.AddColumn(ColumnType.String, "cls").SetTargetColumn(true);
        //    builder.AddColumn(ColumnType.String, "cls2");

        //    builder.AddRow(0.5f, 1.1, "a", "a2");
        //    builder.AddRow(0.2f, 1.5, "b", "b2");
        //    builder.AddRow(0.7f, 0.5, "c", "c2");
        //    builder.AddRow(0.2f, 0.6, "d", "d2");

        //    var table = builder.Build();
        //    var table2 = table.SelectColumns(new[] { 1, 2, 3 });

        //    Assert.AreEqual(table2.TargetColumnIndex, 1);
        //    Assert.AreEqual(table2.RowCount, 4);
        //    Assert.AreEqual(table2.ColumnCount, 3);

        //    var column = table2.GetNumericColumns(new[] { 0 }).Select(r => _lap.CreateVector(r)).First().AsIndexable();
        //    Assert.AreEqual(column[0], 1.1f);
        //    Assert.AreEqual(column[1], 1.5f);
        //}

        //[Fact]
        //public void Fold()
        //{
        //    var builder = _context.BuildTable();

        //    builder.AddColumn(ColumnType.Float, "val1");
        //    builder.AddColumn(ColumnType.Double, "val2");
        //    builder.AddColumn(ColumnType.String, "cls").SetTargetColumn(true);

        //    builder.AddRow(0.5f, 1.1, "a");
        //    builder.AddRow(0.2f, 1.5, "b");
        //    builder.AddRow(0.7f, 0.5, "c");
        //    builder.AddRow(0.2f, 0.6, "d");

        //    var table = builder.Build();
        //    var folds = table.Fold(4, 0, false).ToList();
        //    Assert.AreEqual(folds.Count, 4);
        //    Assert.IsTrue(folds.All(r => r.Training.RowCount == 3 && r.Validation.RowCount == 1));
        //}

        //[Fact]
        //public void TableFilter()
        //{
        //    var builder = _context.BuildTable();

        //    builder.AddColumn(ColumnType.Float, "val1");
        //    builder.AddColumn(ColumnType.Double, "val2");
        //    builder.AddColumn(ColumnType.String, "cls").SetTargetColumn(true);

        //    builder.AddRow(0.5f, 1.1, "a");
        //    builder.AddRow(0.2f, 1.5, "b");
        //    builder.AddRow(0.7f, 0.5, "c");
        //    builder.AddRow(0.2f, 0.6, "d");

        //    var table = builder.Build();
        //    var projectedTable = table.Project(r => r.GetField<string>(2) == "b" ? null : r.Data);

        //    Assert.AreEqual(projectedTable.ColumnCount, table.ColumnCount);
        //    Assert.AreEqual(projectedTable.RowCount, 3);
        //}

        //[Fact]
        //public void TableSummarise()
        //{
        //    var builder = _context.BuildTable();

        //    builder.AddColumn(ColumnType.Boolean, "boolean");
        //    builder.AddColumn(ColumnType.Byte, "byte");
        //    builder.AddColumn(ColumnType.Date, "date");
        //    builder.AddColumn(ColumnType.Double, "double");
        //    builder.AddColumn(ColumnType.Float, "float");
        //    builder.AddColumn(ColumnType.Int, "int");
        //    builder.AddColumn(ColumnType.Long, "long");
        //    builder.AddColumn(ColumnType.String, "string");

        //    var now = DateTime.Now;
        //    builder.AddRow(true, (sbyte)100, now, 1.0 / 2, 0.5f, int.MaxValue, long.MaxValue, "test");
        //    builder.AddRow(true, (sbyte)0, now, 0.0, 0f, int.MinValue, long.MinValue, "test");
        //    var dataTable = builder.Build();

        //    var summarisedRow = dataTable.Summarise(1).GetRow(0);
        //    Assert.AreEqual(summarisedRow.GetField<bool>(0), true);
        //    Assert.AreEqual(summarisedRow.GetField<sbyte>(1), (sbyte)50);
        //    Assert.AreEqual(summarisedRow.GetField<double>(3), 0.25);
        //    Assert.AreEqual(summarisedRow.GetField<string>(7), "test");
        //}

        //[Fact]
        //public void TableReverseVectorise()
        //{
        //    var table = CreateComplexTable(_context);
        //    uint targetColumnIndex;
        //    table.SetTargetColumn(targetColumnIndex = table.ColumnCount - 1);
        //    var targetColumnType = table.Column(targetColumnIndex).SingleType;
        //    var vectoriser = table.GetVectoriser(true);
        //    var model = vectoriser.GetVectorisationModel();

        //    var output = table.Map(row => model.GetOutput(row));
        //    var reversedOutput = output.Select(vector => model.ReverseVectoriseOutput(vector, targetColumnType)).ToList();
        //    var tuples = table.Map(row => (row.Data[targetColumnIndex], reversedOutput[row.Index]));
        //    foreach (var tuple in tuples)
        //        Assert.AreEqual(tuple.Item1, tuple.Item2);
        //}

        //[Fact]
        //public void TableConfusionMatrix()
        //{
        //    var builder = _context.BuildTable();

        //    builder.AddColumn(ColumnType.String, "actual");
        //    builder.AddColumn(ColumnType.String, "expected");

        //    const int CAT_CAT = 5;
        //    const int CAT_DOG = 2;
        //    const int DOG_CAT = 3;
        //    const int DOG_DOG = 5;
        //    const int DOG_RABBIT = 2;
        //    const int RABBIT_DOG = 1;
        //    const int RABBIT_RABBIT = 11;

        //    for (var i = 0; i < CAT_CAT; i++)
        //        builder.AddRow("cat", "cat");
        //    for (var i = 0; i < CAT_DOG; i++)
        //        builder.AddRow("cat", "dog");
        //    for (var i = 0; i < DOG_CAT; i++)
        //        builder.AddRow("dog", "cat");
        //    for (var i = 0; i < DOG_DOG; i++)
        //        builder.AddRow("dog", "dog");
        //    for (var i = 0; i < DOG_RABBIT; i++)
        //        builder.AddRow("dog", "rabbit");
        //    for (var i = 0; i < RABBIT_DOG; i++)
        //        builder.AddRow("rabbit", "dog");
        //    for (var i = 0; i < RABBIT_RABBIT; i++)
        //        builder.AddRow("rabbit", "rabbit");
        //    var table = builder.Build();
        //    var confusionMatrix = table.CreateConfusionMatrix(1, 0);
        //    var xml = confusionMatrix.AsXml;

        //    Assert.AreEqual((uint)CAT_DOG, confusionMatrix.GetCount("cat", "dog"));
        //    Assert.AreEqual((uint)DOG_RABBIT, confusionMatrix.GetCount("dog", "rabbit"));
        //    Assert.AreEqual((uint)RABBIT_RABBIT, confusionMatrix.GetCount("rabbit", "rabbit"));
        //}
    }
}
