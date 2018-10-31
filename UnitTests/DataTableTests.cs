using BrightWire;
using BrightWire.TabularData;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
    [TestClass]
    public class DataTableTests
    {
        static ILinearAlgebraProvider _lap;

        [ClassInitialize]
        public static void Load(TestContext context)
        {
            _lap = BrightWireProvider.CreateLinearAlgebra(false);
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            _lap.Dispose();
        }

        [TestMethod]
        public void TestColumnTypes()
        {
            var builder = BrightWireProvider.CreateDataTableBuilder();
            builder.AddColumn(ColumnType.Boolean, "boolean");
            builder.AddColumn(ColumnType.Byte, "byte");
            builder.AddColumn(ColumnType.Date, "date");
            builder.AddColumn(ColumnType.Double, "double");
            builder.AddColumn(ColumnType.Float, "float");
            builder.AddColumn(ColumnType.Int, "int");
            builder.AddColumn(ColumnType.Long, "long");
            builder.AddColumn(ColumnType.String, "string");

            var now = DateTime.Now;
            builder.Add(true, (sbyte)100, now, 1.0 / 3, 0.5f, int.MaxValue, long.MaxValue, "test");
            var dataTable = builder.Build();

            var firstRow = dataTable.GetRow(0);
            Assert.AreEqual(firstRow.GetField<bool>(0), true);
            Assert.AreEqual(firstRow.GetField<byte>(1), 100);
            Assert.AreEqual(firstRow.GetField<DateTime>(2), now);
            Assert.AreEqual(firstRow.GetField<double>(3), 1.0 / 3);
            Assert.AreEqual(firstRow.GetField<float>(4), 0.5f);
            Assert.AreEqual(firstRow.GetField<int>(5), int.MaxValue);
            Assert.AreEqual(firstRow.GetField<long>(6), long.MaxValue);
            Assert.AreEqual(firstRow.GetField<string>(7), "test");
        }

        void _CompareRows(IRow row1, IRow row2, Random rand)
        {
            Assert.AreEqual(row1.Data.Count, row2.Data.Count);
            for (var i = 0; i < row1.Data.Count; i++)
                Assert.AreEqual(row1.Data[i], row2.Data[i]);
        }

        void _CompareTables(IDataTable table1, IDataTable table2)
        {
            var rand = new Random();
            Assert.AreEqual(table1.ColumnCount, table2.ColumnCount);
            Assert.AreEqual(table1.RowCount, table2.RowCount);
            Assert.AreEqual(table1.TargetColumnIndex, table1.TargetColumnIndex);

            for (var i = 0; i < 128; i++) {
                var index = rand.Next(table1.RowCount);
                var row1 = table1.GetRow(index);
                var row2 = table2.GetRow(index);
                _CompareRows(row1, row2, rand);
            }
        }

        void _RandomSample(IDataTable table, Action<int, IRow> callback)
        {
            var rand = new Random();
            for(var i = 0; i < 128; i++) {
                var index = rand.Next(table.RowCount);
                callback(index, table.GetRow(index));
            }
        }

        [TestMethod]
        public void TestIndexHydration()
        {
            using (var dataStream = new MemoryStream())
            using (var indexStream = new MemoryStream()) {
                var builder = BrightWireProvider.CreateDataTableBuilder(dataStream);
                builder.AddColumn(ColumnType.Boolean, "target", true);
                builder.AddColumn(ColumnType.Int, "val");
                builder.AddColumn(ColumnType.String, "label");
                for (var i = 0; i < 33000; i++)
                    builder.Add(i % 2 == 0, i, i.ToString());

                var table = builder.Build();
                builder.WriteIndexTo(indexStream);

                dataStream.Seek(0, SeekOrigin.Begin);
                indexStream.Seek(0, SeekOrigin.Begin);
                var newTable = BrightWireProvider.CreateDataTable(dataStream, indexStream);
                _CompareTables(table, newTable);

                dataStream.Seek(0, SeekOrigin.Begin);
                var newTable2 = BrightWireProvider.CreateDataTable(dataStream, null);
                _CompareTables(table, newTable2);
            }
        }

	    static IDataTable _CreateComplexTable()
	    {
		    var builder = BrightWireProvider.CreateDataTableBuilder();
		    builder.AddColumn(ColumnType.Boolean, "boolean");
		    builder.AddColumn(ColumnType.Byte, "byte");
		    builder.AddColumn(ColumnType.Date, "date");
		    builder.AddColumn(ColumnType.Double, "double");
		    builder.AddColumn(ColumnType.Float, "float");
		    builder.AddColumn(ColumnType.Int, "int");
		    builder.AddColumn(ColumnType.Long, "long");
		    builder.AddColumn(ColumnType.String, "string");

		    for (var i = 1; i <= 10; i++)
			    builder.Add(i % 2 == 0, (sbyte)i, DateTime.Now, (double)i, (float)i, i, (long)i, i.ToString());
		    return builder.Build();
	    }

        [TestMethod]
        public void TestDataTableAnalysis()
        {
	        var table = _CreateComplexTable();
            var analysis = table.GetAnalysis();
            var xml = analysis.AsXml;

            var boolAnalysis = analysis[0] as INumericColumnInfo;
            Assert.IsTrue(boolAnalysis.NumDistinct == 2);
            Assert.IsTrue(boolAnalysis.Mean == 0.5);

            var numericAnalysis = new[] { 1, 3, 4, 5, 6 }.Select(i => analysis[i] as INumericColumnInfo).ToList();
            Assert.IsTrue(numericAnalysis.All(a => a.NumDistinct == 10));
            Assert.IsTrue(numericAnalysis.All(a => a.Min == 1));
            Assert.IsTrue(numericAnalysis.All(a => a.Max == 10));
            Assert.IsTrue(numericAnalysis.All(a => a.Mean == 5.5));
            Assert.IsTrue(numericAnalysis.All(a => a.Median.Value == 5));
            Assert.IsTrue(numericAnalysis.All(a => Math.Round(a.StdDev.Value) == 3));

            var stringAnalysis = analysis[7] as IStringColumnInfo;
            Assert.IsTrue(stringAnalysis.NumDistinct == 10);
            Assert.IsTrue(stringAnalysis.MaxLength == 2);
        }

        IDataTable _GetSimpleTable()
        {
            var builder = BrightWireProvider.CreateDataTableBuilder();
            builder.AddColumn(ColumnType.Int, "val");

            for (var i = 0; i < 10000; i++)
                builder.Add(i);
            return builder.Build();
        }

        IDataTable _GetSimpleTable2()
        {
            var table = _GetSimpleTable();
            var table2 = table.Project(r => new object[] { r.GetField<double>(0) });
            Assert.AreEqual(table2.Columns[0].Type, ColumnType.Double);
            return table2;
        }

        [TestMethod]
        public void TestTableSlice()
        {
            var table = _GetSimpleTable();
            var rows = table.GetSlice(5000, 100).Select(r => r.GetField<int>(0)).ToList();

            for(var i = 0; i < 100; i++)
                Assert.AreEqual(rows[i], 5000 + i);
        }

        [TestMethod]
        public void TestTableSplit()
        {
            var table = _GetSimpleTable();
            var split = table.Split(null, 0.75);
            Assert.AreEqual(split.Training.RowCount, 7500);
            Assert.AreEqual(split.Test.RowCount, 2500);
        }

        [TestMethod]
        public void TestStandardNormalisation()
        {
            var table = _GetSimpleTable2();
            var analysis = table.GetAnalysis()[0] as INumericColumnInfo;
            var normalised = table.Normalise(NormalisationType.Standard);

            _RandomSample(normalised, (index, row) => {
                var val = row.GetField<double>(0);
                var prevVal = table.GetRow(index).GetField<double>(0);
                var expected = (prevVal - analysis.Mean) / analysis.StdDev.Value;

                Assert.AreEqual(val, expected);
            });
        }

        [TestMethod]
        public void TestStandardNormalisation2()
        {
            var table = _GetSimpleTable2();
            var analysis = table.GetAnalysis()[0] as INumericColumnInfo;
            var model = table.GetNormalisationModel(NormalisationType.Standard);
            var normalised = table.Normalise(model);

            _RandomSample(normalised, (index, row) => {
                var val = row.GetField<double>(0);
                var prevVal = table.GetRow(index).GetField<double>(0);
                var expected = (prevVal - analysis.Mean) / analysis.StdDev.Value;

                Assert.AreEqual(val, expected);
            });
        }

        [TestMethod]
        public void TestFeatureScaleNormalisation()
        {
            var table = _GetSimpleTable2();
            var analysis = table.GetAnalysis()[0] as INumericColumnInfo;
            var normalised = table.Normalise(NormalisationType.FeatureScale);

            _RandomSample(normalised, (index, row) => {
                var val = row.GetField<double>(0);
                var prevVal = table.GetRow(index).GetField<double>(0);
                var expected = (prevVal - analysis.Min) / (analysis.Max - analysis.Min);

                Assert.AreEqual(val, expected);
            });
        }

        [TestMethod]
        public void TestFeatureScaleNormalisation2()
        {
            var table = _GetSimpleTable2();
            var analysis = table.GetAnalysis()[0] as INumericColumnInfo;
            var model = table.GetNormalisationModel(NormalisationType.FeatureScale);
            var normalised = table.Normalise(model);

            _RandomSample(normalised, (index, row) => {
                var val = row.GetField<double>(0);
                var prevVal = table.GetRow(index).GetField<double>(0);
                var expected = (prevVal - analysis.Min) / (analysis.Max - analysis.Min);

                Assert.AreEqual(val, expected);
            });
        }

        [TestMethod]
        public void TestL2Normalisation()
        {
            var table = _GetSimpleTable2();
            var analysis = table.GetAnalysis()[0] as INumericColumnInfo;
            var normalised = table.Normalise(NormalisationType.Euclidean);

            var l2Norm = Math.Sqrt(table.GetColumn<double>(0).Select(d => Math.Pow(d, 2)).Sum());
            Assert.AreEqual(analysis.L2Norm, l2Norm);

            _RandomSample(normalised, (index, row) => {
                var val = row.GetField<double>(0);
                var prevVal = table.GetRow(index).GetField<double>(0);
                var expected = prevVal / analysis.L2Norm;

                Assert.AreEqual(val, expected);
            });
        }

        [TestMethod]
        public void TestL2Normalisation2()
        {
            var table = _GetSimpleTable2();
            var analysis = table.GetAnalysis()[0] as INumericColumnInfo;
            var model = table.GetNormalisationModel(NormalisationType.Euclidean);
            var normalised = table.Normalise(model);

            var l2Norm = Math.Sqrt(table.GetColumn<double>(0).Select(d => Math.Pow(d, 2)).Sum());
            Assert.AreEqual(analysis.L2Norm, l2Norm);

            _RandomSample(normalised, (index, row) => {
                var val = row.GetField<double>(0);
                var prevVal = table.GetRow(index).GetField<double>(0);
                var expected = prevVal / analysis.L2Norm;

                Assert.AreEqual(val, expected);
            });
        }

        [TestMethod]
        public void TestL1Normalisation()
        {
            var table = _GetSimpleTable2();
            var analysis = table.GetAnalysis()[0] as INumericColumnInfo;
            var normalised = table.Normalise(NormalisationType.Manhattan);

            var l1Norm = table.GetColumn<double>(0).Select(d => Math.Abs(d)).Sum();
            Assert.AreEqual(analysis.L1Norm, l1Norm);

            _RandomSample(normalised, (index, row) => {
                var val = row.GetField<double>(0);
                var prevVal = table.GetRow(index).GetField<double>(0);
                var expected = prevVal / analysis.L1Norm;

                Assert.AreEqual(val, expected);
            });
        }

        [TestMethod]
        public void TestL1Normalisation2()
        {
            var table = _GetSimpleTable2();
            var analysis = table.GetAnalysis()[0] as INumericColumnInfo;
            var model = table.GetNormalisationModel(NormalisationType.Manhattan);
            var normalised = table.Normalise(model);

            var l1Norm = table.GetColumn<double>(0).Select(d => Math.Abs(d)).Sum();
            Assert.AreEqual(analysis.L1Norm, l1Norm);

            _RandomSample(normalised, (index, row) => {
                var val = row.GetField<double>(0);
                var prevVal = table.GetRow(index).GetField<double>(0);
                var expected = prevVal / analysis.L1Norm;

                Assert.AreEqual(val, expected);
            });
        }

	    [TestMethod]
	    public void TestReverseNormalisation()
	    {
		    var table = _CreateComplexTable();
		    var columnIndex = table.TargetColumnIndex = 3;
		    var model = table.GetNormalisationModel(NormalisationType.FeatureScale);
		    var normalised = table.Normalise(model);

		    var reverseNormalised = normalised.Project(row => new [] {model.ReverseNormaliseOutput(columnIndex, row.Data[columnIndex])});
		    var zipped = reverseNormalised.Zip(table.SelectColumns(new[] { columnIndex}));

		    zipped.ForEach(row => Assert.AreEqual(row.Data[0], row.Data[1]));
	    }

        [TestMethod]
        public void TestTargetColumnIndex()
        {
            var builder = BrightWireProvider.CreateDataTableBuilder();
            builder.AddColumn(ColumnType.String, "a");
            builder.AddColumn(ColumnType.String, "b", true);
            builder.AddColumn(ColumnType.String, "c");
            builder.Add("a", "b", "c");
            var table = builder.Build();

            Assert.AreEqual(table.TargetColumnIndex, 1);
            Assert.AreEqual(table.RowCount, 1);
            Assert.AreEqual(table.ColumnCount, 3);
        }

        [TestMethod]
        public void GetNumericRows()
        {
            var builder = BrightWireProvider.CreateDataTableBuilder();
            builder.AddColumn(ColumnType.Float, "val1");
            builder.AddColumn(ColumnType.Double, "val2");
            builder.AddColumn(ColumnType.String, "cls", true);

            builder.Add(0.5f, 1.1, "a");
            builder.Add(0.2f, 1.5, "b");
            builder.Add(0.7f, 0.5, "c");
            builder.Add(0.2f, 0.6, "d");

            var table = builder.Build();
            var rows = table.GetNumericRows(new[] { 1 }).Select(r => _lap.CreateVector(r)).Select(r => r.AsIndexable()).ToList();
            Assert.AreEqual(rows[0][0], 1.1f);
            Assert.AreEqual(rows[1][0], 1.5f);
        }

        [TestMethod]
        public void GetNumericRows2()
        {
            var builder = BrightWireProvider.CreateDataTableBuilder();
            builder.AddColumn(ColumnType.Float, "val1");
            builder.AddColumn(ColumnType.Double, "val2");
            builder.AddColumn(ColumnType.String, "cls", true);

            builder.Add(0.5f, 1.1, "a");
            builder.Add(0.2f, 1.5, "b");
            builder.Add(0.7f, 0.5, "c");
            builder.Add(0.2f, 0.6, "d");

            var table = builder.Build();
            var rows = table.GetNumericRows(new[] { 1 });
            Assert.AreEqual(rows[0][0], 1.1f);
            Assert.AreEqual(rows[1][0], 1.5f);
        }

        [TestMethod]
        public void GetNumericColumns()
        {
            var builder = BrightWireProvider.CreateDataTableBuilder();
            builder.AddColumn(ColumnType.Float, "val1");
            builder.AddColumn(ColumnType.Double, "val2");
            builder.AddColumn(ColumnType.String, "cls", true);

            builder.Add(0.5f, 1.1, "a");
            builder.Add(0.2f, 1.5, "b");
            builder.Add(0.7f, 0.5, "c");
            builder.Add(0.2f, 0.6, "d");

            var table = builder.Build();
            var column = table.GetNumericColumns(new[] { 1 }).Select(r => _lap.CreateVector(r)).First().AsIndexable();
            Assert.AreEqual(column[0], 1.1f);
            Assert.AreEqual(column[1], 1.5f);
        }

        [TestMethod]
        public void GetNumericColumns2()
        {
            var builder = BrightWireProvider.CreateDataTableBuilder();
            builder.AddColumn(ColumnType.Float, "val1");
            builder.AddColumn(ColumnType.Double, "val2");
            builder.AddColumn(ColumnType.String, "cls", true);

            builder.Add(0.5f, 1.1, "a");
            builder.Add(0.2f, 1.5, "b");
            builder.Add(0.7f, 0.5, "c");
            builder.Add(0.2f, 0.6, "d");

            var table = builder.Build();
            var column = table.GetNumericColumns(new[] { 1 }).First();
            Assert.AreEqual(column[0], 1.1f);
            Assert.AreEqual(column[1], 1.5f);
        }

        [TestMethod]
        public void SelectColumns()
        {
            var builder = BrightWireProvider.CreateDataTableBuilder();
            builder.AddColumn(ColumnType.Float, "val1");
            builder.AddColumn(ColumnType.Double, "val2");
            builder.AddColumn(ColumnType.String, "cls", true);
            builder.AddColumn(ColumnType.String, "cls2");

            builder.Add(0.5f, 1.1, "a", "a2");
            builder.Add(0.2f, 1.5, "b", "b2");
            builder.Add(0.7f, 0.5, "c", "c2");
            builder.Add(0.2f, 0.6, "d", "d2");

            var table = builder.Build();
            var table2 = table.SelectColumns(new[] { 1, 2, 3 });

            Assert.AreEqual(table2.TargetColumnIndex, 1);
            Assert.AreEqual(table2.RowCount, 4);
            Assert.AreEqual(table2.ColumnCount, 3);

            var column = table2.GetNumericColumns(new[] { 0 }).Select(r => _lap.CreateVector(r)).First().AsIndexable();
            Assert.AreEqual(column[0], 1.1f);
            Assert.AreEqual(column[1], 1.5f);
        }

        [TestMethod]
        public void Fold()
        {
            var builder = BrightWireProvider.CreateDataTableBuilder();
            builder.AddColumn(ColumnType.Float, "val1");
            builder.AddColumn(ColumnType.Double, "val2");
            builder.AddColumn(ColumnType.String, "cls", true);

            builder.Add(0.5f, 1.1, "a");
            builder.Add(0.2f, 1.5, "b");
            builder.Add(0.7f, 0.5, "c");
            builder.Add(0.2f, 0.6, "d");

            var table = builder.Build();
            var folds = table.Fold(4, 0, false).ToList();
            Assert.AreEqual(folds.Count, 4);
            Assert.IsTrue(folds.All(r => r.Training.RowCount == 3 && r.Validation.RowCount == 1));
        }

        [TestMethod]
        public void TableFilter()
        {
            var builder = BrightWireProvider.CreateDataTableBuilder();
            builder.AddColumn(ColumnType.Float, "val1");
            builder.AddColumn(ColumnType.Double, "val2");
            builder.AddColumn(ColumnType.String, "cls", true);

            builder.Add(0.5f, 1.1, "a");
            builder.Add(0.2f, 1.5, "b");
            builder.Add(0.7f, 0.5, "c");
            builder.Add(0.2f, 0.6, "d");

            var table = builder.Build();
            var projectedTable = table.Project(r => r.GetField<string>(2) == "b" ? null : r.Data);

            Assert.AreEqual(projectedTable.ColumnCount, table.ColumnCount);
            Assert.AreEqual(projectedTable.RowCount, 3);
        }

	    [TestMethod]
	    public void TableSummarise()
	    {
		    var builder = BrightWireProvider.CreateDataTableBuilder();
		    builder.AddColumn(ColumnType.Boolean, "boolean");
		    builder.AddColumn(ColumnType.Byte, "byte");
		    builder.AddColumn(ColumnType.Date, "date");
		    builder.AddColumn(ColumnType.Double, "double");
		    builder.AddColumn(ColumnType.Float, "float");
		    builder.AddColumn(ColumnType.Int, "int");
		    builder.AddColumn(ColumnType.Long, "long");
		    builder.AddColumn(ColumnType.String, "string");

		    var now = DateTime.Now;
		    builder.Add(true, (sbyte)100, now, 1.0 / 2, 0.5f, int.MaxValue, long.MaxValue, "test");
		    builder.Add(true, (sbyte)0, now, 0.0, 0f, int.MinValue, long.MinValue, "test");
		    var dataTable = builder.Build();

		    var summarisedRow = dataTable.Summarise(1).GetRow(0);
		    Assert.AreEqual(summarisedRow.GetField<bool>(0), true);
		    Assert.AreEqual(summarisedRow.GetField<sbyte>(1), (sbyte)50);
		    Assert.AreEqual(summarisedRow.GetField<double>(3), 0.25);
		    Assert.AreEqual(summarisedRow.GetField<string>(7), "test");
	    }

	    [TestMethod]
	    public void TableReverseVectorise()
	    {
		    var table = _CreateComplexTable();
		    var targetColumnIndex = table.TargetColumnIndex = table.ColumnCount - 1;
		    var targetColumnType = table.Columns[targetColumnIndex].Type;
		    var vectoriser = table.GetVectoriser(true);
		    var model = vectoriser.GetVectorisationModel();

		    var output = table.Map(row => model.GetOutput(row));
		    var reversedOutput = output.Select(vector => model.ReverseVectoriseOutput(vector, targetColumnType)).ToList();
		    var tuples = table.Map(row => (row.Data[targetColumnIndex], reversedOutput[row.Index]));
		    foreach (var tuple in tuples)
			    Assert.AreEqual(tuple.Item1, tuple.Item2);
	    }

	    [TestMethod]
	    public void TableConfusionMatrix()
	    {
		    var builder = BrightWireProvider.CreateDataTableBuilder();
		    builder.AddColumn(ColumnType.String, "actual");
		    builder.AddColumn(ColumnType.String, "expected");

		    const int CAT_CAT = 5;
		    const int CAT_DOG = 2;
		    const int DOG_CAT = 3;
		    const int DOG_DOG = 5;
		    const int DOG_RABBIT = 2;
		    const int RABBIT_DOG = 1;
		    const int RABBIT_RABBIT = 11;

			for(var i = 0; i < CAT_CAT; i++)
				builder.Add("cat", "cat");
		    for(var i = 0; i < CAT_DOG; i++)
			    builder.Add("cat", "dog");
		    for(var i = 0; i < DOG_CAT; i++)
			    builder.Add("dog", "cat");
		    for(var i = 0; i < DOG_DOG; i++)
			    builder.Add("dog", "dog");
		    for(var i = 0; i < DOG_RABBIT; i++)
			    builder.Add("dog", "rabbit");
		    for(var i = 0; i < RABBIT_DOG; i++)
			    builder.Add("rabbit", "dog");
		    for(var i = 0; i < RABBIT_RABBIT; i++)
			    builder.Add("rabbit", "rabbit");
		    var table = builder.Build();
		    var confusionMatrix = table.CreateConfusionMatrix(1, 0);
		    var xml = confusionMatrix.AsXml;

		    Assert.AreEqual((uint)CAT_DOG, confusionMatrix.GetCount("cat", "dog"));
		    Assert.AreEqual((uint)DOG_RABBIT, confusionMatrix.GetCount("dog", "rabbit"));
		    Assert.AreEqual((uint)RABBIT_RABBIT, confusionMatrix.GetCount("rabbit", "rabbit"));
	    }
    }
}
