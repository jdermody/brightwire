using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightData;
using BrightData.Helper;
using BrightData.UnitTests;
using BrightWire.ExecutionGraph;
using FluentAssertions;
using Xunit;

namespace BrightWire.UnitTests
{
    class GraphOperationTests : NumericsBase
    {
        readonly GraphFactory _factory;

        public GraphOperationTests()
        {
            _factory = new GraphFactory(_cpu);
        }

		void _TestNode(INode node, IFloatMatrix forwardInput, IFloatMatrix expectedForwardOutput, IFloatMatrix backwardInput, IFloatMatrix expectedBackwardOutput, bool isTraining = true)
		{
			var context = new TestingContext(_cpu);
			var matrix = forwardInput.AsIndexable();
			context.Data = matrix.AsGraphData();
			context.IsTraining = isTraining;
			node.ExecuteForward(context, 0);

			var output = context.Forward.First();
			var outputMatrix = output.Item1.Data.GetMatrix();
			FloatMath.AreApproximatelyEqual(outputMatrix.AsIndexable(), expectedForwardOutput.AsIndexable()).Should().BeTrue();

			output.Item2.Backward(null, backwardInput.Clone().AsGraphData(), context, new[] { node });
			var bpOutput = context.Backward.First().Item1.GetMatrix();
            FloatMath.AreApproximatelyEqual(bpOutput.AsIndexable(), expectedBackwardOutput.AsIndexable()).Should().BeTrue();
		}

		const uint SIZE = 2;

		float _DefaultInit(uint i, uint j) => (i + 1) * (j + 1);

		[Fact]
		public void TestInputSquared()
		{
			var forwardInput = _cpu.CreateMatrix(SIZE, SIZE, _DefaultInit).AsIndexable();
			var expectedOutput = _cpu.CreateMatrix(SIZE, SIZE, (i, j) => (float)Math.Pow(forwardInput[i, j], 2));

			var backwardInput = _cpu.CreateMatrix(SIZE, SIZE, _DefaultInit).AsIndexable();
			var expectedBpOutput = _cpu.CreateMatrix(SIZE, SIZE, (i, j) => backwardInput[i, j] * 2f * forwardInput[i, j]);

			_TestNode(_factory.GraphOperation.InputSquared(), forwardInput, expectedOutput, backwardInput, expectedBpOutput);
		}

		[Fact]
		public void TestOneDividedBy()
		{
			var forwardInput = _cpu.CreateMatrix(SIZE, SIZE, _DefaultInit).AsIndexable();
			var expectedOutput = _cpu.CreateMatrix(SIZE, SIZE, (i, j) => 1f / forwardInput[i, j]);

			var backwardInput = _cpu.CreateMatrix(SIZE, SIZE, _DefaultInit).AsIndexable();
			var expectedBpOutput = _cpu.CreateMatrix(SIZE, SIZE, (i, j) => -1f / (float)Math.Pow(forwardInput[i, j], 2) * backwardInput[i, j]);

			_TestNode(_factory.GraphOperation.OneDividedBy(), forwardInput, expectedOutput, backwardInput, expectedBpOutput);
		}

		[Fact]
		public void TestOneMinus()
		{
			var forwardInput = _cpu.CreateMatrix(SIZE, SIZE, _DefaultInit).AsIndexable();
			var expectedOutput = _cpu.CreateMatrix(SIZE, SIZE, (i, j) => 1f - forwardInput[i, j]);

			var backwardInput = _cpu.CreateMatrix(SIZE, SIZE, _DefaultInit).AsIndexable();
			var expectedBpOutput = _cpu.CreateMatrix(SIZE, SIZE, (i, j) => -backwardInput[i, j]);

			_TestNode(_factory.GraphOperation.OneMinusInput(), forwardInput, expectedOutput, backwardInput, expectedBpOutput);
		}

		[Fact]
		public void TestSquareRootOf()
		{
			var forwardInput = _cpu.CreateMatrix(SIZE, SIZE, _DefaultInit).AsIndexable();
			var expectedOutput = _cpu.CreateMatrix(SIZE, SIZE, (i, j) => (float)Math.Sqrt(forwardInput[i, j])).AsIndexable();

			var backwardInput = _cpu.CreateMatrix(SIZE, SIZE, _DefaultInit).AsIndexable();
			var expectedBpOutput = _cpu.CreateMatrix(SIZE, SIZE, (i, j) => 0.5f * expectedOutput[i, j] * backwardInput[i, j]);

			_TestNode(_factory.GraphOperation.SquareRootOf(), forwardInput, expectedOutput, backwardInput, expectedBpOutput);
		}

		[Fact]
		public void TestDropoutEmpty()
		{
			var forwardInput = _cpu.CreateMatrix(SIZE, SIZE, _DefaultInit).AsIndexable();
			var expectedOutput = _cpu.CreateMatrix(SIZE, SIZE, (i, j) => 0f);

			var backwardInput = _cpu.CreateMatrix(SIZE, SIZE, _DefaultInit).AsIndexable();
			var expectedBpOutput = _cpu.CreateMatrix(SIZE, SIZE, (i, j) => 0f);

			_TestNode(_factory.CreateDropOut(1f), forwardInput, expectedOutput, backwardInput, expectedBpOutput);
		}

		[Fact]
		public void TestDropoutFull()
		{
			var forwardInput = _cpu.CreateMatrix(SIZE, SIZE, _DefaultInit).AsIndexable();
			var expectedOutput = _cpu.CreateMatrix(SIZE, SIZE, (i, j) => forwardInput[i, j]);

			var backwardInput = _cpu.CreateMatrix(SIZE, SIZE, _DefaultInit).AsIndexable();
			var expectedBpOutput = _cpu.CreateMatrix(SIZE, SIZE, (i, j) => forwardInput[i, j]);

			_TestNode(_factory.CreateDropOut(0f), forwardInput, expectedOutput, backwardInput, expectedBpOutput);
		}

		[Fact]
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
