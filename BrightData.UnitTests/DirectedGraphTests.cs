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
            var builder = new DirectedGraphBuilder<GraphNodeIndex>();
            builder.Add(new(100));
            builder.Add(new(200));
            builder.AddEdge(200, 100);

            var graph = builder.Build();
            graph.Size.Should().Be(2);
            graph.TryGetValue(100, out _).Should().BeTrue();
            graph.TryGetValue(200, out _).Should().BeTrue();
            graph.TryGetValue(300, out _).Should().BeFalse();
            graph.EnumerateConnectedNodes(200).Should().HaveCount(1);
            graph.EnumerateConnectedNodes(100).Should().BeEmpty();
        }
    }
}
