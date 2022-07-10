using System;
using System.Linq;
using BrightData;
using BrightData.Helper;
using BrightData.UnitTests.Helper;
using BrightWire.ExecutionGraph;
using BrightWire.ExecutionGraph.Node;
using BrightWire.UnitTests.Helper;
using FluentAssertions;
using Xunit;

namespace BrightWire.UnitTests
{
    public class GraphOperationTests : CpuBase
    {
        readonly GraphFactory _factory;

        public GraphOperationTests()
        {
            _factory = new GraphFactory(_cpu);
        }

		void TestNode(NodeBase node, IMatrix forwardInput, IMatrix expectedForwardOutput, IMatrix backwardInput, IMatrix expectedBackwardOutput)
		{
			var context = new TestingContext(_cpu);
			var matrix = forwardInput;
            node.Forward(_factory.Context.CancellationToken, matrix.AsGraphData(), context);

			var output = context.Forward.First();
			var outputMatrix = output.Item1.Data.GetMatrix();
			FloatMath.AreApproximatelyEqual(outputMatrix, expectedForwardOutput).Should().BeTrue();

			var backward = output.Item2.Backward(backwardInput.Clone().AsGraphData(), context, new[] { node }).ToList();
			var bpOutput = backward.First().Signal.GetMatrix();
            FloatMath.AreApproximatelyEqual(bpOutput, expectedBackwardOutput).Should().BeTrue();
		}

		const uint Size = 2;

		float DefaultInit(uint i, uint j) => (i + 1) * (j + 1);

		[Fact]
		public void TestInputSquared()
		{
			var forwardInput = _cpu.CreateMatrix(Size, Size, DefaultInit);
			var expectedOutput = _cpu.CreateMatrix(Size, Size, (i, j) => (float)Math.Pow(forwardInput[i, j], 2));

			var backwardInput = _cpu.CreateMatrix(Size, Size, DefaultInit);
			var expectedBpOutput = _cpu.CreateMatrix(Size, Size, (i, j) => backwardInput[i, j] * 2f * forwardInput[i, j]);

			TestNode(_factory.GraphOperation.InputSquared(), forwardInput, expectedOutput, backwardInput, expectedBpOutput);
		}

		[Fact]
		public void TestOneDividedBy()
		{
			var forwardInput = _cpu.CreateMatrix(Size, Size, DefaultInit);
			var expectedOutput = _cpu.CreateMatrix(Size, Size, (i, j) => 1f / forwardInput[i, j]);

			var backwardInput = _cpu.CreateMatrix(Size, Size, DefaultInit);
			var expectedBpOutput = _cpu.CreateMatrix(Size, Size, (i, j) => -1f / (float)Math.Pow(forwardInput[i, j], 2) * backwardInput[i, j]);

			TestNode(_factory.GraphOperation.OneDividedBy(), forwardInput, expectedOutput, backwardInput, expectedBpOutput);
		}

		[Fact]
		public void TestOneMinus()
		{
			var forwardInput = _cpu.CreateMatrix(Size, Size, DefaultInit);
			var expectedOutput = _cpu.CreateMatrix(Size, Size, (i, j) => 1f - forwardInput[i, j]);

			var backwardInput = _cpu.CreateMatrix(Size, Size, DefaultInit);
			var expectedBpOutput = _cpu.CreateMatrix(Size, Size, (i, j) => -backwardInput[i, j]);

			TestNode(_factory.GraphOperation.OneMinusInput(), forwardInput, expectedOutput, backwardInput, expectedBpOutput);
		}

		[Fact]
		public void TestSquareRootOf()
		{
			var forwardInput = _cpu.CreateMatrix(Size, Size, DefaultInit);
			var expectedOutput = _cpu.CreateMatrix(Size, Size, (i, j) => (float)Math.Sqrt(forwardInput[i, j]));

			var backwardInput = _cpu.CreateMatrix(Size, Size, DefaultInit);
			var expectedBpOutput = _cpu.CreateMatrix(Size, Size, (i, j) => 0.5f * expectedOutput[i, j] * backwardInput[i, j]);

			TestNode(_factory.GraphOperation.SquareRootOf(), forwardInput, expectedOutput, backwardInput, expectedBpOutput);
		}

		[Fact]
		public void TestDropoutEmpty()
		{
			var forwardInput = _cpu.CreateMatrix(Size, Size, DefaultInit);
			var expectedOutput = _cpu.CreateMatrix(Size, Size, (_, _) => 0f);

			var backwardInput = _cpu.CreateMatrix(Size, Size, DefaultInit);
			var expectedBpOutput = _cpu.CreateMatrix(Size, Size, (_, _) => 0f);

			TestNode(_factory.CreateDropOut(1f), forwardInput, expectedOutput, backwardInput, expectedBpOutput);
		}

		[Fact]
		public void TestDropoutFull()
		{
			var forwardInput = _cpu.CreateMatrix(Size, Size, DefaultInit);
			var expectedOutput = _cpu.CreateMatrix(Size, Size, (i, j) => forwardInput[i, j]);

			var backwardInput = _cpu.CreateMatrix(Size, Size, DefaultInit);
			var expectedBpOutput = _cpu.CreateMatrix(Size, Size, (i, j) => forwardInput[i, j]);

			TestNode(_factory.CreateDropOut(0f), forwardInput, expectedOutput, backwardInput, expectedBpOutput);
		}

		[Fact]
		public void TestDropConnectEmpty()
		{
			var forwardInput = _cpu.CreateMatrix(Size, Size, DefaultInit);
			var expectedOutput = _cpu.CreateMatrix(Size, Size, (_, _) => 0f);

			var backwardInput = _cpu.CreateMatrix(Size, Size, DefaultInit);
			var expectedBpOutput = _cpu.CreateMatrix(Size, Size, (_, _) => 0f);

			TestNode(_factory.CreateDropConnect(1f, Size, Size), forwardInput, expectedOutput, backwardInput, expectedBpOutput);
		}
	}
}
