using BrightData;
using BrightData.Helper;
using BrightData.UnitTests;
using BrightWire.ExecutionGraph;
using BrightWire.UnitTests.Helper;
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

        void CheckTestAction(IAction action, IGraphData input, IGraphData expectedOutput)
        {
            var context = new TestingContext(_cpu);
            var output = action.Execute(input, context);

            FloatMath.AreApproximatelyEqual(output.GetMatrix().AsIndexable(), expectedOutput.GetMatrix().AsIndexable()).Should().BeTrue();
        }

        [Fact]
        public void TestConstrainInput()
        {
            var input = _cpu.CreateVector(-1.5f, -1f, -0.5f, 0, 0.5f, 1f, 1.5f).ReshapeAsMatrix(1, 7);
            var output = _cpu.CreateVector(-1f, -1f, -0.5f, 0, 0.5f, 1f, 1f).ReshapeAsMatrix(1, 7);

            CheckTestAction(_factory.GraphAction.Constrain(), input.AsGraphData(), output.AsGraphData());
        }
	}
}
