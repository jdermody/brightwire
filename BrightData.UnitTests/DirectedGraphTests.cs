using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightData.Types.Graph;
using Xunit;

namespace BrightData.UnitTests
{
    public class DirectedGraphTests
    {
        [Fact]
        public void SimpleTest()
        {
            var builder = new SparseGraphBuilder<GraphNodeIndex>();
            var firstNodeIndex = builder.Add(new(100));
            var secondNodeIndex = builder.Add(new(200));
            builder.AddEdge(secondNodeIndex, firstNodeIndex);

            var graph = builder.Build();
            graph.Size.Should().Be(2);
            graph.TryGetValue(firstNodeIndex, out var val).Should().BeTrue();
            val.Value.Index.Should().Be(100);
            graph.TryGetValue(secondNodeIndex, out val).Should().BeTrue();
            val.Value.Index.Should().Be(200);
            graph.TryGetValue(3, out _).Should().BeFalse();
            graph.EnumerateDirectlyConnectedNodes(secondNodeIndex).Should().HaveCount(1);
            graph.EnumerateDirectlyConnectedNodes(firstNodeIndex).Should().BeEmpty();
        }
    }
}
