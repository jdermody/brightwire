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
        [TestMethod]
        public void TestColumnTypes()
        {
            var builder = new DataTableBuilder();
            builder.AddColumn(ColumnType.Boolean, "boolean");
            builder.AddColumn(ColumnType.Byte, "byte");
            builder.AddColumn(ColumnType.Date, "date");
            builder.AddColumn(ColumnType.Double, "double");
            builder.AddColumn(ColumnType.Float, "float");
            builder.AddColumn(ColumnType.Int, "int");
            builder.AddColumn(ColumnType.Long, "long");
            builder.AddColumn(ColumnType.Null, "null");
            builder.AddColumn(ColumnType.String, "string");

            var now = DateTime.Now;
            builder.Add(true, (byte)100, now, 1.0 / 3, 0.5f, int.MaxValue, long.MaxValue, null, "test");
            var dataTable = builder.Build();

            var firstRow = dataTable.GetRow(0);
            Assert.AreEqual(firstRow.GetField<bool>(0), true);
            Assert.AreEqual(firstRow.GetField<byte>(1), 100);
            Assert.AreEqual(firstRow.GetField<DateTime>(2), now);
            Assert.AreEqual(firstRow.GetField<double>(3), 1.0 / 3);
            Assert.AreEqual(firstRow.GetField<float>(4), 0.5f);
            Assert.AreEqual(firstRow.GetField<int>(5), int.MaxValue);
            Assert.AreEqual(firstRow.GetField<long>(6), long.MaxValue);
            Assert.AreEqual(firstRow.GetField<object>(7), null);
            Assert.AreEqual(firstRow.GetField<string>(8), "test");
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
            var builder = new DataTableBuilder();
            builder.AddColumn(ColumnType.Boolean, "target", true);
            builder.AddColumn(ColumnType.Int, "val");
            builder.AddColumn(ColumnType.String, "label");
            for (var i = 0; i < 33000; i++)
                builder.Add(i % 2 == 0, i, i.ToString());

            using (var dataStream = new MemoryStream())
            using (var indexStream = new MemoryStream()) {
                var table = builder.Build(dataStream);
                table.WriteIndexTo(indexStream);

                dataStream.Seek(0, SeekOrigin.Begin);
                indexStream.Seek(0, SeekOrigin.Begin);
                var newTable = Provider.CreateDataTable(dataStream, indexStream);
                _CompareTables(table, newTable);

                dataStream.Seek(0, SeekOrigin.Begin);
                var newTable2 = Provider.CreateDataTable(dataStream, null);
                _CompareTables(table, newTable2);
            }
        }

        [TestMethod]
        public void TestDataTableAnalysis()
        {
            var builder = new DataTableBuilder();
            builder.AddColumn(ColumnType.Boolean, "boolean");
            builder.AddColumn(ColumnType.Byte, "byte");
            builder.AddColumn(ColumnType.Date, "date");
            builder.AddColumn(ColumnType.Double, "double");
            builder.AddColumn(ColumnType.Float, "float");
            builder.AddColumn(ColumnType.Int, "int");
            builder.AddColumn(ColumnType.Long, "long");
            builder.AddColumn(ColumnType.Null, "null");
            builder.AddColumn(ColumnType.String, "string");

            for (var i = 1; i <= 10; i++)
                builder.Add(i % 2 == 0, (byte)i, DateTime.Now, (double)i, (float)i, i, (long)i, null, i.ToString());
            var table = builder.Build();
            var analysis = table.Analysis;

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

            var stringAnalysis = analysis[8] as IStringColumnInfo;
            Assert.IsTrue(stringAnalysis.NumDistinct == 10);
            Assert.IsTrue(stringAnalysis.MaxLength == 2);
        }

        IDataTable _GetSimpleTable()
        {
            var builder = new DataTableBuilder();
            builder.AddColumn(ColumnType.Int, "val");

            for (var i = 0; i < 10000; i++)
                builder.Add(i);
            return builder.Build();
        }

        IDataTable _GetSimpleTable2()
        {
            var table = _GetSimpleTable();
            var table2 = table.Project(r => new object[] { Convert.ToDouble(r[0]) });
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
            Assert.AreEqual(split.Item1.RowCount, 7500);
            Assert.AreEqual(split.Item2.RowCount, 2500);
        }

        [TestMethod]
        public void TestStandardNormalisation()
        {
            var table = _GetSimpleTable2();
            var analysis = table.Analysis[0] as INumericColumnInfo;
            var normalised = table.Normalise(NormalisationType.Standard);

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
            var analysis = table.Analysis[0] as INumericColumnInfo;
            var normalised = table.Normalise(NormalisationType.FeatureScale);

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
            var analysis = table.Analysis[0] as INumericColumnInfo;
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
        public void TestL1Normalisation()
        {
            var table = _GetSimpleTable2();
            var analysis = table.Analysis[0] as INumericColumnInfo;
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
    }
}
