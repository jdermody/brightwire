using System.Linq;
using FluentAssertions;
using BrightData.Types.Graph;
using Xunit;

namespace BrightData.UnitTests
{
    public class GraphTests
    {
        [Fact]
        public void UndirectedSparse()
        {
            var graph = new UndirectedSparseGraph<GraphNodeIndex>();
            var firstNodeIndex = graph.Add(new(100));
            var secondNodeIndex = graph.Add(new(200));
            graph.AddEdge(secondNodeIndex, firstNodeIndex);

            var readOnly = graph.AsReadOnly();
            readOnly.Size.Should().Be(2);
            readOnly.TryGetValue(firstNodeIndex, out var val).Should().BeTrue();
            val!.Value.Index.Should().Be(100);
            readOnly.TryGetValue(secondNodeIndex, out val).Should().BeTrue();
            val!.Value.Index.Should().Be(200);
            readOnly.TryGetValue(3, out _).Should().BeFalse();
            readOnly.EnumerateDirectlyConnectedNodes(secondNodeIndex).Should().HaveCount(1);
            readOnly.EnumerateDirectlyConnectedNodes(firstNodeIndex).Should().BeEmpty();
        }

        [Fact]
        public void DirectedGraph()
        {
            using var graph = new DirectedGraph<int, char>();
            graph.Add(10).Should().Be(0);
            graph.Add(20).Should().Be(1);
            graph.Add(30).Should().Be(2);
            graph.Size.Should().Be(3);

            graph.AddEdge(0, 1, 'a').Should().Be(0);
            graph.AddEdge(1, 2, 'b').Should().Be(1);
            graph.AddEdge(0, 2, 'c').Should().Be(2);

            graph.GetOutDegree(0).Should().Be(2);
            graph.GetInDegree(1).Should().Be(1);

            var edge = graph.GetEdge(0, 1);
            edge.Data.Should().Be('a');
            edge.Index.Should().Be(0);

            graph.Clear();
            graph.Size.Should().Be(0);
        }

        [Fact]
        public void TopologicalSort()
        {
            using var graph = new DirectedGraph<int, char>();
            graph.Add(10).Should().Be(0);
            graph.Add(20).Should().Be(1);
            graph.Add(30).Should().Be(2);
            graph.Add(40).Should().Be(3);
            graph.Size.Should().Be(4);

            graph.AddEdge(0, 1, 'a').Should().Be(0);
            graph.AddEdge(1, 2, 'b').Should().Be(1);
            graph.AddEdge(0, 2, 'c').Should().Be(2);
            graph.AddEdge(0, 3, 'd').Should().Be(3);

            var sort = graph.TopologicalSort().ToList();
            sort.Should().BeEquivalentTo([0, 1, 3, 2]);
        }

        [Fact]
        public void TopologicalSortWithCycles()
        {
            using var graph = new DirectedGraph<int, char>();
            graph.Add(10).Should().Be(0);
            graph.Add(20).Should().Be(1);
            graph.Add(30).Should().Be(2);
            graph.Size.Should().Be(3);

            graph.AddEdge(0, 1, 'a').Should().Be(0);
            graph.AddEdge(1, 2, 'b').Should().Be(1);
            graph.AddEdge(0, 2, 'c').Should().Be(2);
            graph.AddEdge(2, 0, 'd').Should().Be(3);

            var sort = graph.TopologicalSort().ToList();
            sort.Should().BeEmpty();
        }

        [Fact]
        public void DepthFirstSearch()
        {
            using var graph = new DirectedGraph<int, char>();
            graph.Add(10).Should().Be(0);
            graph.Add(20).Should().Be(1);
            graph.Add(30).Should().Be(2);
            graph.Add(40).Should().Be(3);
            graph.Size.Should().Be(4);

            graph.AddEdge(0, 1, 'a').Should().Be(0);
            graph.AddEdge(1, 2, 'b').Should().Be(1);
            graph.AddEdge(0, 2, 'c').Should().Be(2);
            graph.AddEdge(0, 3, 'd').Should().Be(3);

            var dfs = graph.DepthFirstSearch(0);
            dfs.Should().BeEquivalentTo([0, 3, 2, 1]);
        }

        [Fact]
        public void BreadthFirstSearch()
        {
            using var graph = new DirectedGraph<int, char>();
            graph.Add(10).Should().Be(0);
            graph.Add(20).Should().Be(1);
            graph.Add(30).Should().Be(2);
            graph.Add(40).Should().Be(3);
            graph.Size.Should().Be(4);

            graph.AddEdge(0, 1, 'a').Should().Be(0);
            graph.AddEdge(1, 2, 'b').Should().Be(1);
            graph.AddEdge(0, 2, 'c').Should().Be(2);
            graph.AddEdge(0, 3, 'd').Should().Be(3);

            var bfs = graph.BreadthFirstSearch(0);
            bfs.Should().BeEquivalentTo([0, 1, 2, 3]);
        }
    }
}
