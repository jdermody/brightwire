using BrightData.Helper;
using BrightData.UnitTests.Helper;
using BrightWire.ExecutionGraph;
using BrightWire.UnitTests.Helper;
using FluentAssertions;
using Xunit;

namespace BrightWire.UnitTests
{
    public class GraphActionTests : CpuBase
    {
        readonly GraphFactory _factory;

        public GraphActionTests()
        {
            _factory = new GraphFactory(_cpu);
        }

        void CheckTestAction(IAction action, IGraphData input, IGraphData expectedOutput)
        {
            var context = new TestingContext(_cpu);
            var output = action.Execute(input, context, null!);

            Math<float>.AreApproximatelyEqual(output.GetMatrix(), expectedOutput.GetMatrix()).Should().BeTrue();
        }

        [Fact]
        public void TestConstrainInput()
        {
            using var input = _cpu.CreateVector(-1.5f, -1f, -0.5f, 0, 0.5f, 1f, 1.5f).Reshape(1, 7);
            using var output = _cpu.CreateVector(-1f, -1f, -0.5f, 0, 0.5f, 1f, 1f).Reshape(1, 7);

            CheckTestAction(_factory.GraphAction.Constrain(), input.AsGraphData(), output.AsGraphData());
        }
	}
}
