using System;
using System.Collections.Generic;
using System.Text;
using BrightData;
using BrightData.Helper;
using BrightData.UnitTests;
using BrightWire.ExecutionGraph;
using FluentAssertions;
using Xunit;

namespace BrightWire.UnitTests
{
    public class GraphActionTests : NumericsBase
    {
        readonly GraphFactory _factory;

        public GraphActionTests()
        {
            _factory = new GraphFactory(_cpu);
        }

        void _TestAction(IAction action, IGraphData input, IGraphData expectedOutput)
        {
            var context = new TestingContext(_cpu);
            var output = action.Execute(input, context);

            FloatMath.AreApproximatelyEqual(output.GetMatrix().AsIndexable(), expectedOutput.GetMatrix().AsIndexable()).Should().BeTrue();
        }

        [Fact]
        public void TestConstrainInput()
        {
            var input = _cpu.CreateVector(new[] { -1.5f, -1f, -0.5f, 0, 0.5f, 1f, 1.5f }).ReshapeAsMatrix(1, 7);
            var output = _cpu.CreateVector(new[] { -1f, -1f, -0.5f, 0, 0.5f, 1f, 1f }).ReshapeAsMatrix(1, 7);

            _TestAction(_factory.GraphAction.Constrain(-1f, 1f), input.AsGraphData(), output.AsGraphData());
        }
	}
}
