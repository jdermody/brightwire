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

		void _TestNode(INode node, IMatrix forwardInput, IMatrix expectedForwardOutput, IMatrix backwardInput, IMatrix expectedBackwardOutput)
		{
			var context = new TestingContext(_cpu);
			var matrix = forwardInput.AsIndexable();
			context.Data = matrix.AsGraphData();
			node.ExecuteForward(context, 0);

			var output = context.Forward.First();
			var outputMatrix = output.Item1.Data.GetMatrix();
			FloatingPointHelper.AssertEqual(outputMatrix.AsIndexable(), expectedForwardOutput.AsIndexable());

			output.Item2.Backward(null, backwardInput.Clone().AsGraphData(), context, new[] { node });
			var bpOutput = context.Backward.First().Item1.GetMatrix();
			FloatingPointHelper.AssertEqual(bpOutput.AsIndexable(), expectedBackwardOutput.AsIndexable());
		}

		[TestMethod]
		public void TestInputSquared()
		{
			var forwardInput = _cpu.CreateMatrix(2, 2, (i, j) => (i + 1) * (j + 1)).AsIndexable();
			var expectedOutput = _cpu.CreateMatrix(2, 2, (i, j) => (float)Math.Pow(forwardInput[i, j], 2));

			var identity = _cpu.CreateIdentityMatrix(2).AsIndexable();
			var expectedBpOutput = _cpu.CreateMatrix(2, 2, (i, j) => identity[i, j] * 2f);

			_TestNode(_factory.GraphOperation.InputSquared(), forwardInput, expectedOutput, identity, expectedBpOutput);
		}
	}
}
