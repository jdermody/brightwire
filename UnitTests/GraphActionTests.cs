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
	public class GraphActionTests
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

		void _TestAction(IAction action, IGraphData input, IGraphData expectedOutput)
		{
			var context = new TestingContext(_cpu);
			var output = action.Execute(input, context);

			FloatingPointHelper.AssertEqual(output.GetMatrix().AsIndexable(), expectedOutput.GetMatrix().AsIndexable());
		}

		[TestMethod]
		public void TestConstrainInput()
		{
			var input = _cpu.CreateVector(new[] {-1.5f, -1f, -0.5f, 0, 0.5f, 1f, 1.5f}).ReshapeAsMatrix(1, 7);
			var output = _cpu.CreateVector(new[] {-1f, -1f, -0.5f, 0, 0.5f, 1f, 1f}).ReshapeAsMatrix(1, 7);

			_TestAction(_factory.GraphAction.Constrain(-1f, 1f), input.AsGraphData(), output.AsGraphData());
		}
	}
}
