using BrightWire;
using BrightWire.ExecutionGraph;
using BrightWire.TabularData;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightWire.Models;

namespace UnitTests
{
    [TestClass]
    public class DataSourceTests
    {
        static ILinearAlgebraProvider _lap;
		static GraphFactory _factory;

        [ClassInitialize]
        public static void Load(TestContext context)
        {
            _lap = BrightWireProvider.CreateLinearAlgebra(false);
	        _factory = new GraphFactory(_lap);
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            _lap.Dispose();
        }

        [TestMethod]
        public void DefaultDataSource()
        {
            var builder = BrightWireProvider.CreateDataTableBuilder();
            builder.AddColumn(ColumnType.Float, "val1");
            builder.AddColumn(ColumnType.Double, "val2");
            builder.AddColumn(ColumnType.String, "val3");
            builder.AddColumn(ColumnType.String, "cls", true);

            builder.Add(0.5f, 1.1, "d", "a");
            builder.Add(0.2f, 1.5, "c", "b");
            builder.Add(0.7f, 0.5, "b", "c");
            builder.Add(0.2f, 0.6, "a", "d");

            var table = builder.Build();
            var vectoriser = table.GetVectoriser();
            var graph = new GraphFactory(_lap);
            var dataSource = graph.CreateDataSource(table, vectoriser);
            var miniBatch = dataSource.Get(null, new[] { 1 });
            var input = miniBatch.CurrentSequence.Input[0].GetMatrix().Row(0).AsIndexable();
            var expectedOutput = miniBatch.CurrentSequence.Target.GetMatrix().Row(0).AsIndexable();

            Assert.AreEqual(input[0], 0.2f);
            Assert.AreEqual(input[1], 1.5f);
            Assert.AreEqual(expectedOutput.Count, 4);

            Assert.AreEqual(vectoriser.GetOutputLabel(2, expectedOutput.MaximumIndex()), "b");
        }

	    float[] GetArray(int value, int size)
	    {
		    var ret = new float[size];
			for(var i = 0; i < size; i++)
				ret[i] = value;
			return ret;
	    }

	    [TestMethod]
	    public void VectorDataSource()
	    {
		    var vectors = Enumerable.Range(0, 10).Select(i => FloatVector.Create(GetArray(i, 10))).ToList();
			var dataSource = _factory.CreateDataSource(vectors);
		    var miniBatch = dataSource.Get(null, new[] {0, 1, 2});

		    var currentSequence = miniBatch.CurrentSequence;
		    var batchMatrix = currentSequence.Input[0].GetMatrix();
		    Assert.IsNull(currentSequence.Target);
		    Assert.IsTrue(batchMatrix.RowCount == 3);
		    Assert.IsTrue(batchMatrix.ColumnCount == 10);
		    Assert.AreEqual(batchMatrix.Row(0).GetAt(0), 0f);
		    Assert.AreEqual(batchMatrix.Row(1).GetAt(0), 1f);
	    }

	    [TestMethod]
	    public void MatrixDataSource()
	    {
		    var matrices = Enumerable.Range(0, 10).Select(j => FloatMatrix.Create(Enumerable.Range(0, 10).Select(i => FloatVector.Create(GetArray(i, 10))).ToArray())).ToList();
		    var dataSource = _factory.CreateDataSource(matrices);
		    var miniBatch = dataSource.Get(null, new[] {0, 1, 2});

		    var currentSequence = miniBatch.CurrentSequence;
		    var batchMatrix = currentSequence.Input[0].GetMatrix();
		    Assert.IsNull(currentSequence.Target);
		    Assert.IsTrue(batchMatrix.RowCount == 3);
		    Assert.IsTrue(batchMatrix.ColumnCount == 10);
		    Assert.AreEqual(batchMatrix.Row(0).GetAt(0), 0f);
	    }

	    [TestMethod]
	    public void TensorDataSource()
	    {
		    var tensors = Enumerable.Range(0, 10).Select(k => FloatTensor.Create(Enumerable.Range(0, 10).Select(j => FloatMatrix.Create(Enumerable.Range(0, 10).Select(i => FloatVector.Create(GetArray(i, 10))).ToArray())).ToArray())).ToList();
		    var dataSource = _factory.CreateDataSource(tensors);
		    var miniBatch = dataSource.Get(null, new[] {0, 1, 2});

		    var currentSequence = miniBatch.CurrentSequence;
		    var batchMatrix = currentSequence.Input[0].GetMatrix();
		    Assert.IsNull(currentSequence.Target);
		    Assert.IsTrue(batchMatrix.RowCount == 1000);
		    Assert.IsTrue(batchMatrix.ColumnCount == 3);
		    Assert.AreEqual(batchMatrix.Row(0).GetAt(0), 0f);
	    }
    }
}
