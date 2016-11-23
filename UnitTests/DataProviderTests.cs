using BrightWire;
using BrightWire.TabularData;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
    [TestClass]
    public class DataProviderTests
    {
        static ILinearAlgebraProvider _lap;

        [ClassInitialize]
        public static void Load(TestContext context)
        {
            _lap = Provider.CreateLinearAlgebra(false);
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            _lap.Dispose();
        }

        [TestMethod]
        public void DataTableProvider()
        {
            var builder = new DataTableBuilder();
            builder.AddColumn(ColumnType.Float, "val1");
            builder.AddColumn(ColumnType.Double, "val2");
            builder.AddColumn(ColumnType.String, "val3");
            builder.AddColumn(ColumnType.String, "cls", true);

            builder.Add(0.5f, 1.1, "d", "a");
            builder.Add(0.2f, 1.5, "c", "b");
            builder.Add(0.7f, 0.5, "b", "c");
            builder.Add(0.2f, 0.6, "a", "d");

            var table = builder.Build();
            var vectoriser = table.GetVectoriser(true);
            var dataProvider = _lap.NN.CreateTrainingDataProvider(table, vectoriser);
            var miniBatch = dataProvider.GetTrainingData(new[] { 1 });
            var input = miniBatch.Input.Row(0).AsIndexable();
            var expectedOutput = miniBatch.ExpectedOutput.Row(0).AsIndexable();

            Assert.AreEqual(input[0], 0.2f);
            Assert.AreEqual(input[1], 1.5f);
            Assert.AreEqual(expectedOutput.Count, 4);

            Assert.AreEqual(vectoriser.GetOutputLabel(2, expectedOutput.MaximumIndex()), "b");
        }
    }
}
