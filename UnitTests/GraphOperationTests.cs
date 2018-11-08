using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightWire;
using BrightWire.ExecutionGraph;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests.Helper;

namespace UnitTests
{
	[TestClass]
	public class GraphOperationTests
	{
		static ILinearAlgebraProvider _cpu;
		static GraphFactory _factory;

		[ClassInitialize]
		public static void Load(TestContext context)
		{
			_cpu = BrightWireProvider.CreateLinearAlgebra(false);
			_factory = new GraphFactory(_cpu);
		}

		[ClassCleanup]
		public static void Cleanup()
		{
			_cpu.Dispose();
		}

		void _TestNode(INode node, IMatrix forwardInput, IMatrix expectedForwardOutput, IMatrix backwardInput, IMatrix expectedBackwardOutput, bool isTraining = true)
		{
			var context = new TestingContext(_cpu);
			var matrix = forwardInput.AsIndexable();
			context.Data = matrix.AsGraphData();
			context.IsTraining = isTraining;
			node.ExecuteForward(context, 0);

			var output = context.Forward.First();
			var outputMatrix = output.Item1.Data.GetMatrix();
			FloatingPointHelper.AssertEqual(outputMatrix.AsIndexable(), expectedForwardOutput.AsIndexable());

			output.Item2.Backward(null, backwardInput.Clone().AsGraphData(), context, new[] { node });
			var bpOutput = context.Backward.First().Item1.GetMatrix();
			FloatingPointHelper.AssertEqual(bpOutput.AsIndexable(), expectedBackwardOutput.AsIndexable());
		}

		const int SIZE = 2;

		float _DefaultInit(int i, int j) => (i + 1) * (j + 1);

		[TestMethod]
		public void TestInputSquared()
		{
			var forwardInput = _cpu.CreateMatrix(SIZE, SIZE, _DefaultInit).AsIndexable();
			var expectedOutput = _cpu.CreateMatrix(SIZE, SIZE, (i, j) => (float)Math.Pow(forwardInput[i, j], 2));

			var backwardInput = _cpu.CreateMatrix(SIZE, SIZE, _DefaultInit).AsIndexable();
			var expectedBpOutput = _cpu.CreateMatrix(SIZE, SIZE, (i, j) => backwardInput[i, j] * 2f * forwardInput[i, j]);

			_TestNode(_factory.GraphOperation.InputSquared(), forwardInput, expectedOutput, backwardInput, expectedBpOutput);
		}

		[TestMethod]
		public void TestOneDividedBy()
		{
			var forwardInput = _cpu.CreateMatrix(SIZE, SIZE, _DefaultInit).AsIndexable();
			var expectedOutput = _cpu.CreateMatrix(SIZE, SIZE, (i, j) => 1f / forwardInput[i, j]);

			var backwardInput = _cpu.CreateMatrix(SIZE, SIZE, _DefaultInit).AsIndexable();
			var expectedBpOutput = _cpu.CreateMatrix(SIZE, SIZE, (i, j) => -1f / (float)Math.Pow(forwardInput[i, j], 2) * backwardInput[i, j]);

			_TestNode(_factory.GraphOperation.OneDividedBy(), forwardInput, expectedOutput, backwardInput, expectedBpOutput);
		}

		[TestMethod]
		public void TestOneMinus()
		{
			var forwardInput = _cpu.CreateMatrix(SIZE, SIZE, _DefaultInit).AsIndexable();
			var expectedOutput = _cpu.CreateMatrix(SIZE, SIZE, (i, j) => 1f - forwardInput[i, j]);

			var backwardInput = _cpu.CreateMatrix(SIZE, SIZE, _DefaultInit).AsIndexable();
			var expectedBpOutput = _cpu.CreateMatrix(SIZE, SIZE, (i, j) => -backwardInput[i,j]);

			_TestNode(_factory.GraphOperation.OneMinusInput(), forwardInput, expectedOutput, backwardInput, expectedBpOutput);
		}

		[TestMethod]
		public void TestSquareRootOf()
		{
			var forwardInput = _cpu.CreateMatrix(SIZE, SIZE, _DefaultInit).AsIndexable();
			var expectedOutput = _cpu.CreateMatrix(SIZE, SIZE, (i, j) => (float)Math.Sqrt(forwardInput[i, j])).AsIndexable();

			var backwardInput = _cpu.CreateMatrix(SIZE, SIZE, _DefaultInit).AsIndexable();
			var expectedBpOutput = _cpu.CreateMatrix(SIZE, SIZE, (i, j) => 0.5f * expectedOutput[i,j] * backwardInput[i, j]);

			_TestNode(_factory.GraphOperation.SquareRootOf(), forwardInput, expectedOutput, backwardInput, expectedBpOutput);
		}

		[TestMethod]
		public void TestDropoutEmpty()
		{
			var forwardInput = _cpu.CreateMatrix(SIZE, SIZE, _DefaultInit).AsIndexable();
			var expectedOutput = _cpu.CreateMatrix(SIZE, SIZE, (i, j) => 0f);

			var backwardInput = _cpu.CreateMatrix(SIZE, SIZE, _DefaultInit).AsIndexable();
			var expectedBpOutput = _cpu.CreateMatrix(SIZE, SIZE, (i, j) => 0f);

			_TestNode(_factory.CreateDropOut(1f), forwardInput, expectedOutput, backwardInput, expectedBpOutput);
		}

		[TestMethod]
		public void TestDropoutFull()
		{
			var forwardInput = _cpu.CreateMatrix(SIZE, SIZE, _DefaultInit).AsIndexable();
			var expectedOutput = _cpu.CreateMatrix(SIZE, SIZE, (i, j) => forwardInput[i, j]);

			var backwardInput = _cpu.CreateMatrix(SIZE, SIZE, _DefaultInit).AsIndexable();
			var expectedBpOutput = _cpu.CreateMatrix(SIZE, SIZE, (i, j) => forwardInput[i, j]);

			_TestNode(_factory.CreateDropOut(0f), forwardInput, expectedOutput, backwardInput, expectedBpOutput);
		}

		[TestMethod]
		public void TestDropConnectEmpty()
		{
			var forwardInput = _cpu.CreateMatrix(SIZE, SIZE, _DefaultInit).AsIndexable();
			var expectedOutput = _cpu.CreateMatrix(SIZE, SIZE, (i, j) => 0f);

			var backwardInput = _cpu.CreateMatrix(SIZE, SIZE, _DefaultInit).AsIndexable();
			var expectedBpOutput = _cpu.CreateMatrix(SIZE, SIZE, (i, j) => 0f);

			_TestNode(_factory.CreateDropConnect(1f, SIZE, SIZE), forwardInput, expectedOutput, backwardInput, expectedBpOutput);
		}
	}
}
