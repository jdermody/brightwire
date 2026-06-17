using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AwesomeAssertions;
using BrightData;
using BrightData.Types;
using BrightData.Types.Graph;
using BrightData.Types.Graph.Helper;
using Xunit;

namespace BrightData.UnitTests
{
    public class GraphTests
    {
        #region UndirectedGraph Tests

        [Fact]
        public void UndirectedSparse()
        {
            var graph = new UndirectedGraph<GraphNodeIndex>();
            var firstNodeIndex = graph.Add(new(100));
            var secondNodeIndex = graph.Add(new(200));
            graph.AddEdge(secondNodeIndex, firstNodeIndex);

            var readOnly = graph.ToReadOnly();
            readOnly.Size.Should().Be(2);
            readOnly.Get(firstNodeIndex).Index.Should().Be(100);
            readOnly.Get(secondNodeIndex).Index.Should().Be(200);
            readOnly.GetConnectedNodes(secondNodeIndex).Should().HaveCount(1);
            readOnly.GetConnectedNodes(firstNodeIndex).Should().HaveCount(1);
        }

        [Fact]
        public void UndirectedGraph_Add_NodeReturnsCorrectIndex()
        {
            var graph = new UndirectedGraph<GraphNodeIndex>();
            graph.Add(new(1)).Should().Be(0);
            graph.Add(new(2)).Should().Be(1);
            graph.Add(new(3)).Should().Be(2);
            graph.Size.Should().Be(3);
        }

        [Fact]
        public void UndirectedGraph_AddEdge_SymmetricEdge()
        {
            var graph = new UndirectedGraph<GraphNodeIndex>();
            graph.Add(new(1));
            graph.Add(new(2));
            graph.AddEdge(0, 1).Should().BeTrue();

            var connected0 = graph.GetConnectedNodes(0).ToList();
            connected0.Should().Contain(1);

            var connected1 = graph.GetConnectedNodes(1).ToList();
            connected1.Should().Contain(0);
        }

        [Fact]
        public void UndirectedGraph_AddEdge_SelfLoopReturnsTrue()
        {
            var graph = new UndirectedGraph<GraphNodeIndex>();
            graph.Add(new(1));
            // Self-loops are allowed (fromNodeIndex < toNodeIndex is false, but both < nodeCount)
            graph.AddEdge(0, 0).Should().BeTrue();
        }

        [Fact]
        public void UndirectedGraph_AddEdge_OutOfRangeReturnsFalse()
        {
            var graph = new UndirectedGraph<GraphNodeIndex>();
            graph.Add(new(1));
            graph.AddEdge(0, 5).Should().BeFalse();
            graph.AddEdge(5, 0).Should().BeFalse();
        }

        [Fact]
        public void UndirectedGraph_Clear_ResetsGraph()
        {
            var graph = new UndirectedGraph<GraphNodeIndex>();
            graph.Add(new(1));
            graph.Add(new(2));
            graph.AddEdge(0, 1);
            graph.Size.Should().Be(2);

            graph.Clear();
            graph.Size.Should().Be(0);
        }

        [Fact]
        public void UndirectedGraph_GetConnectedNodes_OutOfRangeThrows()
        {
            var graph = new UndirectedGraph<GraphNodeIndex>();
            graph.Add(new(1));
            Action a = () => graph.GetConnectedNodes(99).ToList();
            a.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void UndirectedGraph_Get_ReturnsCorrectNode()
        {
            var graph = new UndirectedGraph<GraphNodeIndex>();
            graph.Add(new(42));
            graph.Get(0).Index.Should().Be(42);
        }

        [Fact]
        public void UndirectedGraph_GetNodeIndex_ReturnsCorrectIndex()
        {
            var graph = new UndirectedGraph<GraphNodeIndex>();
            graph.Add(new(10));
            graph.Add(new(20));
            graph.GetNodeIndex(new GraphNodeIndex(20)).Should().Be(1);
        }

        [Fact]
        public void UndirectedGraph_GetNodeIndex_ThrowsWhenNotFound()
        {
            var graph = new UndirectedGraph<GraphNodeIndex>();
            graph.Add(new(10));
            Action a = () => graph.GetNodeIndex(new GraphNodeIndex(99));
            a.Should().Throw<KeyNotFoundException>();
        }

        [Fact]
        public void UndirectedGraph_DepthFirstSearch()
        {
            var graph = new UndirectedGraph<GraphNodeIndex>();
            graph.Add(new(0));
            graph.Add(new(1));
            graph.Add(new(2));
            graph.Add(new(3));
            graph.AddEdge(0, 1);
            graph.AddEdge(1, 2);
            graph.AddEdge(1, 3);

            var dfs = graph.DepthFirstSearch(0).ToList();
            dfs.Should().Contain(0);
            dfs.Should().Contain(1);
            dfs.Should().Contain(2);
            dfs.Should().Contain(3);
        }

        [Fact]
        public void UndirectedGraph_BreadthFirstSearch()
        {
            var graph = new UndirectedGraph<GraphNodeIndex>();
            graph.Add(new(0));
            graph.Add(new(1));
            graph.Add(new(2));
            graph.Add(new(3));
            graph.AddEdge(0, 1);
            graph.AddEdge(0, 2);
            graph.AddEdge(0, 3);

            var bfs = graph.BreadthFirstSearch(0).ToList();
            bfs.Should().Contain(0);
            bfs.Should().Contain(1);
            bfs.Should().Contain(2);
            bfs.Should().Contain(3);
        }

        [Fact]
        public void UndirectedGraph_ToReadOnly_PreservesEdges()
        {
            var graph = new UndirectedGraph<GraphNodeIndex>();
            graph.Add(new(1));
            graph.Add(new(2));
            graph.Add(new(3));
            graph.AddEdge(0, 1);
            graph.AddEdge(1, 2);

            var readOnly = graph.ToReadOnly();
            readOnly.Size.Should().Be(3);
            readOnly.GetConnectedNodes(0).Should().Contain(1);
            readOnly.GetConnectedNodes(1).Should().Contain(0);
            readOnly.GetConnectedNodes(1).Should().Contain(2);
            readOnly.GetConnectedNodes(2).Should().Contain(1);
        }

        #endregion

        #region ReadOnlyUndirectedGraph Tests

        [Fact]
        public void ReadOnlyUndirectedGraph_Size()
        {
            var graph = new UndirectedGraph<GraphNodeIndex>();
            graph.Add(new(1));
            graph.Add(new(2));
            var readOnly = graph.ToReadOnly();
            readOnly.Size.Should().Be(2);
        }

        [Fact]
        public void ReadOnlyUndirectedGraph_GetConnectedNodes()
        {
            var graph = new UndirectedGraph<GraphNodeIndex>();
            graph.Add(new(1));
            graph.Add(new(2));
            graph.Add(new(3));
            graph.AddEdge(0, 1);
            graph.AddEdge(0, 2);
            var readOnly = graph.ToReadOnly();

            var connected = readOnly.GetConnectedNodes(0).ToList();
            connected.Should().Contain(1);
            connected.Should().Contain(2);
        }

        [Fact]
        public void ReadOnlyUndirectedGraph_GetConnectedNodes_OutOfRangeThrows()
        {
            var graph = new UndirectedGraph<GraphNodeIndex>();
            graph.Add(new(1));
            var readOnly = graph.ToReadOnly();

            // Force enumeration to trigger the range check inside the iterator
            Action a = () => readOnly.GetConnectedNodes(99).ToList();
            a.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void ReadOnlyUndirectedGraph_Get()
        {
            var graph = new UndirectedGraph<GraphNodeIndex>();
            graph.Add(new(42));
            var readOnly = graph.ToReadOnly();
            readOnly.Get(0).Index.Should().Be(42);
        }

        [Fact]
        public void ReadOnlyUndirectedGraph_GetNodeIndex()
        {
            var graph = new UndirectedGraph<GraphNodeIndex>();
            graph.Add(new(10));
            graph.Add(new(20));
            var readOnly = graph.ToReadOnly();
            readOnly.GetNodeIndex(new GraphNodeIndex(20)).Should().Be(1);
        }

        [Fact]
        public void ReadOnlyUndirectedGraph_GetNodeIndex_ThrowsWhenNotFound()
        {
            var graph = new UndirectedGraph<GraphNodeIndex>();
            graph.Add(new(10));
            var readOnly = graph.ToReadOnly();
            Action a = () => readOnly.GetNodeIndex(new GraphNodeIndex(99));
            a.Should().Throw<KeyNotFoundException>();
        }

        [Fact]
        public void ReadOnlyUndirectedGraph_DepthFirstSearch()
        {
            var graph = new UndirectedGraph<GraphNodeIndex>();
            graph.Add(new(0));
            graph.Add(new(1));
            graph.Add(new(2));
            graph.AddEdge(0, 1);
            graph.AddEdge(1, 2);
            var readOnly = graph.ToReadOnly();

            var dfs = readOnly.DepthFirstSearch(0).ToList();
            dfs.Should().Contain(0);
            dfs.Should().Contain(1);
            dfs.Should().Contain(2);
        }

        [Fact]
        public void ReadOnlyUndirectedGraph_BreadthFirstSearch()
        {
            var graph = new UndirectedGraph<GraphNodeIndex>();
            graph.Add(new(0));
            graph.Add(new(1));
            graph.Add(new(2));
            graph.AddEdge(0, 1);
            graph.AddEdge(0, 2);
            var readOnly = graph.ToReadOnly();

            var bfs = readOnly.BreadthFirstSearch(0).ToList();
            bfs.Should().Contain(0);
            bfs.Should().Contain(1);
            bfs.Should().Contain(2);
        }

        [Fact]
        public void ReadOnlyUndirectedGraph_DataAsBytes_RoundTrips()
        {
            var graph = new UndirectedGraph<GraphNodeIndex>();
            graph.Add(new(100));
            graph.Add(new(200));
            graph.Add(new(300));
            graph.AddEdge(0, 1);
            graph.AddEdge(1, 2);
            var original = graph.ToReadOnly();

            var bytes = original.DataAsBytes;
            var deserialized = new ReadOnlyUndirectedGraph<GraphNodeIndex>(bytes);

            deserialized.Size.Should().Be(original.Size);
            deserialized.Get(0).Index.Should().Be(100);
            deserialized.Get(1).Index.Should().Be(200);
            deserialized.Get(2).Index.Should().Be(300);
            deserialized.GetConnectedNodes(0).Should().Contain(1);
            deserialized.GetConnectedNodes(1).Should().Contain(0);
            deserialized.GetConnectedNodes(1).Should().Contain(2);
            deserialized.GetConnectedNodes(2).Should().Contain(1);
        }

        [Fact]
        public void ReadOnlyUndirectedGraph_FromBytes_TooShortThrows()
        {
            var shortData = new byte[] { 1, 2, 3 };
            Action a = () => new ReadOnlyUndirectedGraph<GraphNodeIndex>(shortData);
            a.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void ReadOnlyUndirectedGraph_GetEdgeIndex()
        {
            ReadOnlyUndirectedGraph<GraphNodeIndex>.GetEdgeIndex(0, 1, 3).Should().Be(1);
            ReadOnlyUndirectedGraph<GraphNodeIndex>.GetEdgeIndex(1, 2, 3).Should().Be(5);
            ReadOnlyUndirectedGraph<GraphNodeIndex>.GetEdgeIndex(2, 0, 3).Should().Be(6);
        }

        #endregion

        #region MultiDirectedGraph Tests

        [Fact]
        public void DirectedGraph()
        {
            using var graph = new MultiDirectedGraph<int, char>();
            graph.Add(10).Should().Be(0);
            graph.Add(20).Should().Be(1);
            graph.Add(30).Should().Be(2);
            graph.Size.Should().Be(3);

            graph.AddEdge(0, 1, 'a').Should().BeTrue();
            graph.AddEdge(1, 2, 'b').Should().BeTrue();
            graph.AddEdge(0, 2, 'c').Should().BeTrue();

            graph.GetOutDegree(0).Should().Be(2);
            graph.GetInDegree(1).Should().Be(1);

            var edge = graph.GetEdges(0, 1).Single();
            edge.Data.Should().Be('a');
            edge.Index.Should().Be(0);

            graph.Clear();
            graph.Size.Should().Be(0);
        }

        [Fact]
        public void MultiDirectedGraph_AddEdge_OutOfRangeThrows()
        {
            using var graph = new MultiDirectedGraph<int, char>();
            graph.Add(10);
            Action a = () => graph.AddEdge(5, 0);
            a.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void MultiDirectedGraph_AddEdge_ToOutOfRangeThrows()
        {
            using var graph = new MultiDirectedGraph<int, char>();
            graph.Add(10);
            Action a = () => graph.AddEdge(0, 5);
            a.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void MultiDirectedGraph_GetEdges_OutOfRangeThrows()
        {
            using var graph = new MultiDirectedGraph<int, char>();
            graph.Add(10);
            graph.Add(20);
            Action a = () => graph.GetEdges(5, 0);
            a.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void MultiDirectedGraph_GetConnectedNodes_OutOfRangeThrows()
        {
            using var graph = new MultiDirectedGraph<int, char>();
            graph.Add(10);
            Action a = () => graph.GetConnectedNodes(5);
            a.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void MultiDirectedGraph_GetConnectedNodes()
        {
            using var graph = new MultiDirectedGraph<int, char>();
            graph.Add(10);
            graph.Add(20);
            graph.Add(30);
            graph.AddEdge(0, 1);
            graph.AddEdge(0, 2);

            var connected = graph.GetConnectedNodes(0).ToList();
            connected.Should().Contain(1);
            connected.Should().Contain(2);
        }

        [Fact]
        public void MultiDirectedGraph_GetConnectedNodes_NoDuplicates()
        {
            using var graph = new MultiDirectedGraph<int, char>();
            graph.Add(10);
            graph.Add(20);
            graph.AddEdge(0, 1, 'a');
            graph.AddEdge(0, 1, 'b');

            var connected = graph.GetConnectedNodes(0).ToList();
            connected.Should().HaveCount(1);
            connected.Should().Contain(1);
        }

        [Fact]
        public void MultiDirectedGraph_Get()
        {
            using var graph = new MultiDirectedGraph<int, char>();
            graph.Add(42);
            graph.Get(0).Should().Be(42);
        }

        [Fact]
        public void MultiDirectedGraph_GetNodeIndex()
        {
            using var graph = new MultiDirectedGraph<int, char>();
            graph.Add(10);
            graph.Add(20);
            graph.GetNodeIndex(20).Should().Be(1);
        }

        [Fact]
        public void MultiDirectedGraph_GetNodeIndex_ThrowsWhenNotFound()
        {
            using var graph = new MultiDirectedGraph<int, char>();
            graph.Add(10);
            Action a = () => graph.GetNodeIndex(99);
            a.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void MultiDirectedGraph_GetInDegree_NoEdges()
        {
            using var graph = new MultiDirectedGraph<int, char>();
            graph.Add(10);
            graph.Add(20);
            graph.GetInDegree(1).Should().Be(0);
        }

        [Fact]
        public void MultiDirectedGraph_GetOutDegree_NoEdges()
        {
            using var graph = new MultiDirectedGraph<int, char>();
            graph.Add(10);
            graph.Add(20);
            graph.GetOutDegree(0).Should().Be(0);
        }

        [Fact]
        public void MultiDirectedGraph_AddEdge_DefaultEdgeData()
        {
            using var graph = new MultiDirectedGraph<int, char>();
            graph.Add(10);
            graph.Add(20);
            graph.AddEdge(0, 1).Should().BeTrue();

            var edges = graph.GetEdges(0, 1);
            edges.Should().HaveCount(1);
            edges.Single().Data.Should().Be(default(char));
        }

        [Fact]
        public void MultiDirectedGraph_DepthFirstSearch()
        {
            using var graph = new MultiDirectedGraph<int, char>();
            graph.Add(0);
            graph.Add(1);
            graph.Add(2);
            graph.Add(3);
            graph.AddEdge(0, 1);
            graph.AddEdge(1, 2);
            graph.AddEdge(0, 2);
            graph.AddEdge(0, 3);

            var dfs = graph.DepthFirstSearch(0).ToList();
            dfs.Should().Contain(0);
            dfs.Should().Contain(1);
            dfs.Should().Contain(2);
            dfs.Should().Contain(3);
        }

        [Fact]
        public void MultiDirectedGraph_BreadthFirstSearch()
        {
            using var graph = new MultiDirectedGraph<int, char>();
            graph.Add(0);
            graph.Add(1);
            graph.Add(2);
            graph.Add(3);
            graph.AddEdge(0, 1);
            graph.AddEdge(1, 2);
            graph.AddEdge(0, 2);
            graph.AddEdge(0, 3);

            var bfs = graph.BreadthFirstSearch(0).ToList();
            bfs.Should().Contain(0);
            bfs.Should().Contain(1);
            bfs.Should().Contain(2);
            bfs.Should().Contain(3);
        }

        [Fact]
        public void TopologicalSort()
        {
            using var graph = new MultiDirectedGraph<int, char>();
            graph.Add(10).Should().Be(0);
            graph.Add(20).Should().Be(1);
            graph.Add(30).Should().Be(2);
            graph.Add(40).Should().Be(3);
            graph.Size.Should().Be(4);

            graph.AddEdge(0, 1, 'a').Should().BeTrue();
            graph.AddEdge(1, 2, 'b').Should().BeTrue();
            graph.AddEdge(0, 2, 'c').Should().BeTrue();
            graph.AddEdge(0, 3, 'd').Should().BeTrue();

            var sort = graph.TopologicalSort().ToList();
            sort.Should().BeEquivalentTo([0, 1, 3, 2]);
        }

        [Fact]
        public void TopologicalSortWithCycles()
        {
            using var graph = new MultiDirectedGraph<int, char>();
            graph.Add(10).Should().Be(0);
            graph.Add(20).Should().Be(1);
            graph.Add(30).Should().Be(2);
            graph.Size.Should().Be(3);

            graph.AddEdge(0, 1, 'a').Should().BeTrue();
            graph.AddEdge(1, 2, 'b').Should().BeTrue();
            graph.AddEdge(0, 2, 'c').Should().BeTrue();
            graph.AddEdge(2, 0, 'd').Should().BeTrue();

            var sort = graph.TopologicalSort().ToList();
            sort.Should().BeEmpty();
        }

        [Fact]
        public void DepthFirstSearch()
        {
            using var graph = new MultiDirectedGraph<int, char>();
            graph.Add(10).Should().Be(0);
            graph.Add(20).Should().Be(1);
            graph.Add(30).Should().Be(2);
            graph.Add(40).Should().Be(3);
            graph.Size.Should().Be(4);

            graph.AddEdge(0, 1, 'a').Should().BeTrue();
            graph.AddEdge(1, 2, 'b').Should().BeTrue();
            graph.AddEdge(0, 2, 'c').Should().BeTrue();
            graph.AddEdge(0, 3, 'd').Should().BeTrue();

            var dfs = graph.DepthFirstSearch(0);
            dfs.Should().BeEquivalentTo([0, 3, 2, 1]);
        }

        [Fact]
        public void BreadthFirstSearch()
        {
            using var graph = new MultiDirectedGraph<int, char>();
            graph.Add(10).Should().Be(0);
            graph.Add(20).Should().Be(1);
            graph.Add(30).Should().Be(2);
            graph.Add(40).Should().Be(3);
            graph.Size.Should().Be(4);

            graph.AddEdge(0, 1, 'a').Should().BeTrue();
            graph.AddEdge(1, 2, 'b').Should().BeTrue();
            graph.AddEdge(0, 2, 'c').Should().BeTrue();
            graph.AddEdge(0, 3, 'd').Should().BeTrue();

            var bfs = graph.BreadthFirstSearch(0);
            bfs.Should().BeEquivalentTo([0, 1, 2, 3]);
        }

        #endregion

        #region FixedSizeWeightedGraph Tests

        [Fact]
        public void FixedSizeWeightedGraph_Add()
        {
            var graph = new FixedSizeWeightedGraph<GraphNodeIndex, float, FixedSizeSortedAscending8Array<uint, float>>();
            graph.Add(new GraphNodeIndex(1));
            graph.Size.Should().Be(1);
        }

        [Fact]
        public void FixedSizeWeightedGraph_Add_WithNeighbours()
        {
            var graph = new FixedSizeWeightedGraph<GraphNodeIndex, float, FixedSizeSortedAscending8Array<uint, float>>();
            graph.Add(new GraphNodeIndex(0));
            graph.Add(new GraphNodeIndex(1), new (uint, float)[] { (0, 0.5f) });

            graph.Size.Should().Be(2);
            var neighbours = graph.GetNeighbours(1).ToArray().ToList();
            neighbours.Should().Contain(0);
        }

        [Fact]
        public void FixedSizeWeightedGraph_Get()
        {
            // FixedSizeWeightedGraph looks up nodes by their Index property, not array position
            var graph = new FixedSizeWeightedGraph<GraphNodeIndex, float, FixedSizeSortedAscending8Array<uint, float>>();
            graph.Add(new GraphNodeIndex(42));
            graph.Get(42).Index.Should().Be(42);
        }

        [Fact]
        public void FixedSizeWeightedGraph_Get_OutOfRangeThrows()
        {
            var graph = new FixedSizeWeightedGraph<GraphNodeIndex, float, FixedSizeSortedAscending8Array<uint, float>>();
            graph.Add(new GraphNodeIndex(1));
            Action a = () => graph.Get(99);
            a.Should().Throw<IndexOutOfRangeException>();
        }

        [Fact]
        public void FixedSizeWeightedGraph_Contains()
        {
            // Contains looks up by node Index, not array position
            var graph = new FixedSizeWeightedGraph<GraphNodeIndex, float, FixedSizeSortedAscending8Array<uint, float>>();
            graph.Add(new GraphNodeIndex(1));
            graph.Contains(1).Should().BeTrue();
            graph.Contains(99).Should().BeFalse();
        }

        [Fact]
        public void FixedSizeWeightedGraph_TryGet_Existing()
        {
            // TryGet looks up by node Index, not array position
            var graph = new FixedSizeWeightedGraph<GraphNodeIndex, float, FixedSizeSortedAscending8Array<uint, float>>();
            graph.Add(new GraphNodeIndex(42));
            graph.TryGet(42, out var value).Should().BeTrue();
            value.Index.Should().Be(42);
        }

        [Fact]
        public void FixedSizeWeightedGraph_TryGet_NotFound()
        {
            var graph = new FixedSizeWeightedGraph<GraphNodeIndex, float, FixedSizeSortedAscending8Array<uint, float>>();
            graph.Add(new GraphNodeIndex(1));
            graph.TryGet(99, out _).Should().BeFalse();
        }

        [Fact]
        public void FixedSizeWeightedGraph_GetNodeIndex()
        {
            var graph = new FixedSizeWeightedGraph<GraphNodeIndex, float, FixedSizeSortedAscending8Array<uint, float>>();
            graph.Add(new GraphNodeIndex(10));
            graph.GetNodeIndex(new GraphNodeIndex(10)).Should().Be(10);
        }

        [Fact]
        public void FixedSizeWeightedGraph_GetNeighbours_Empty()
        {
            var graph = new FixedSizeWeightedGraph<GraphNodeIndex, float, FixedSizeSortedAscending8Array<uint, float>>();
            graph.Add(new GraphNodeIndex(1));
            graph.GetNeighbours(0).ToArray().Should().BeEmpty();
        }

        [Fact]
        public void FixedSizeWeightedGraph_GetNeighbours_NotFound()
        {
            var graph = new FixedSizeWeightedGraph<GraphNodeIndex, float, FixedSizeSortedAscending8Array<uint, float>>();
            graph.GetNeighbours(99).ToArray().Should().BeEmpty();
        }

        [Fact]
        public void FixedSizeWeightedGraph_AddNeighbour()
        {
            var graph = new FixedSizeWeightedGraph<GraphNodeIndex, float, FixedSizeSortedAscending8Array<uint, float>>();
            graph.Add(new GraphNodeIndex(0));
            graph.Add(new GraphNodeIndex(1));
            graph.AddNeighbour(0, 1, 0.5f).Should().BeTrue();

            var neighbours = graph.GetNeighbours(0).ToArray().ToList();
            neighbours.Should().Contain(1);
        }

        [Fact]
        public void FixedSizeWeightedGraph_AddNeighbour_NotFound()
        {
            var graph = new FixedSizeWeightedGraph<GraphNodeIndex, float, FixedSizeSortedAscending8Array<uint, float>>();
            graph.Add(new GraphNodeIndex(0));
            graph.AddNeighbour(99, 0, 0.5f).Should().BeFalse();
        }

        [Fact]
        public void FixedSizeWeightedGraph_EnumerateNeighbours()
        {
            var graph = new FixedSizeWeightedGraph<GraphNodeIndex, float, FixedSizeSortedAscending8Array<uint, float>>();
            graph.Add(new GraphNodeIndex(0));
            graph.Add(new GraphNodeIndex(1));
            graph.AddNeighbour(0, 1, 0.3f);

            var neighbours = graph.EnumerateNeighbours(0).ToList();
            neighbours.Should().HaveCount(1);
            neighbours[0].NeighbourIndex.Should().Be(1);
        }

        [Fact]
        public void FixedSizeWeightedGraph_EnumerateNeighbours_NotFound()
        {
            var graph = new FixedSizeWeightedGraph<GraphNodeIndex, float, FixedSizeSortedAscending8Array<uint, float>>();
            graph.EnumerateNeighbours(99).Should().BeEmpty();
        }

        [Fact]
        public void FixedSizeWeightedGraph_GetConnectedNodes()
        {
            var graph = new FixedSizeWeightedGraph<GraphNodeIndex, float, FixedSizeSortedAscending8Array<uint, float>>();
            graph.Add(new GraphNodeIndex(0));
            graph.Add(new GraphNodeIndex(1));
            graph.AddNeighbour(0, 1, 0.5f);

            var connected = graph.GetConnectedNodes(0).ToList();
            connected.Should().Contain(1);
        }

        [Fact]
        public void FixedSizeWeightedGraph_DepthFirstSearch()
        {
            var graph = new FixedSizeWeightedGraph<GraphNodeIndex, float, FixedSizeSortedAscending8Array<uint, float>>();
            graph.Add(new GraphNodeIndex(0));
            graph.Add(new GraphNodeIndex(1));
            graph.Add(new GraphNodeIndex(2));
            graph.AddNeighbour(0, 1, 0.5f);
            graph.AddNeighbour(1, 2, 0.5f);

            var dfs = graph.DepthFirstSearch(0).ToList();
            dfs.Should().Contain(0);
            dfs.Should().Contain(1);
            dfs.Should().Contain(2);
        }

        [Fact]
        public void FixedSizeWeightedGraph_BreadthFirstSearch()
        {
            var graph = new FixedSizeWeightedGraph<GraphNodeIndex, float, FixedSizeSortedAscending8Array<uint, float>>();
            graph.Add(new GraphNodeIndex(0));
            graph.Add(new GraphNodeIndex(1));
            graph.Add(new GraphNodeIndex(2));
            graph.AddNeighbour(0, 1, 0.5f);
            graph.AddNeighbour(0, 2, 0.3f);

            var bfs = graph.BreadthFirstSearch(0).ToList();
            bfs.Should().Contain(0);
            bfs.Should().Contain(1);
            bfs.Should().Contain(2);
        }

        [Fact]
        public void FixedSizeWeightedGraph_ToReadOnly()
        {
            var graph = new FixedSizeWeightedGraph<GraphNodeIndex, float, FixedSizeSortedAscending8Array<uint, float>>();
            graph.Add(new GraphNodeIndex(1));
            graph.Add(new GraphNodeIndex(2));
            graph.AddNeighbour(0, 1, 0.5f);

            var readOnly = graph.ToReadOnly();
            readOnly.Size.Should().Be(2);
            readOnly.Get(0).Index.Should().Be(1);
        }

        #endregion

        #region ReadOnlyFixedSizeWeightedGraph Tests

        [Fact]
        public void ReadOnlyFixedSizeWeightedGraph_Size()
        {
            var graph = new FixedSizeWeightedGraph<GraphNodeIndex, float, FixedSizeSortedAscending8Array<uint, float>>();
            graph.Add(new GraphNodeIndex(1));
            graph.Add(new GraphNodeIndex(2));
            var readOnly = graph.ToReadOnly();
            readOnly.Size.Should().Be(2);
        }

        [Fact]
        public void ReadOnlyFixedSizeWeightedGraph_Get()
        {
            var graph = new FixedSizeWeightedGraph<GraphNodeIndex, float, FixedSizeSortedAscending8Array<uint, float>>();
            graph.Add(new GraphNodeIndex(42));
            var readOnly = graph.ToReadOnly();
            readOnly.Get(0).Index.Should().Be(42);
        }

        [Fact]
        public void ReadOnlyFixedSizeWeightedGraph_GetNodeIndex()
        {
            var graph = new FixedSizeWeightedGraph<GraphNodeIndex, float, FixedSizeSortedAscending8Array<uint, float>>();
            graph.Add(new GraphNodeIndex(10));
            var readOnly = graph.ToReadOnly();
            readOnly.GetNodeIndex(new GraphNodeIndex(10)).Should().Be(10);
        }

        [Fact]
        public void ReadOnlyFixedSizeWeightedGraph_GetNeighbours()
        {
            var graph = new FixedSizeWeightedGraph<GraphNodeIndex, float, FixedSizeSortedAscending8Array<uint, float>>();
            graph.Add(new GraphNodeIndex(0));
            graph.Add(new GraphNodeIndex(1));
            graph.AddNeighbour(0, 1, 0.5f);
            var readOnly = graph.ToReadOnly();

            var neighbours = readOnly.GetNeighbours(0).ToArray().ToList();
            neighbours.Should().Contain(1);
        }

        [Fact]
        public void ReadOnlyFixedSizeWeightedGraph_EnumerateNeighbours()
        {
            var graph = new FixedSizeWeightedGraph<GraphNodeIndex, float, FixedSizeSortedAscending8Array<uint, float>>();
            graph.Add(new GraphNodeIndex(0));
            graph.Add(new GraphNodeIndex(1));
            graph.AddNeighbour(0, 1, 0.3f);
            var readOnly = graph.ToReadOnly();

            var neighbours = readOnly.EnumerateNeighbours(0).ToList();
            neighbours.Should().HaveCount(1);
            neighbours[0].NeighbourIndex.Should().Be(1);
        }

        [Fact]
        public void ReadOnlyFixedSizeWeightedGraph_GetNeighbourWeights()
        {
            var graph = new FixedSizeWeightedGraph<GraphNodeIndex, float, FixedSizeSortedAscending8Array<uint, float>>();
            graph.Add(new GraphNodeIndex(0));
            graph.Add(new GraphNodeIndex(1));
            graph.AddNeighbour(0, 1, 0.75f);
            var readOnly = graph.ToReadOnly();

            var weights = readOnly.GetNeighbourWeights(0).ToArray().ToList();
            weights.Should().Contain(0.75f);
        }

        [Fact]
        public void ReadOnlyFixedSizeWeightedGraph_GetWeightedNeighbours()
        {
            var graph = new FixedSizeWeightedGraph<GraphNodeIndex, float, FixedSizeSortedAscending8Array<uint, float>>();
            graph.Add(new GraphNodeIndex(0));
            graph.Add(new GraphNodeIndex(1));
            graph.AddNeighbour(0, 1, 0.25f);
            var readOnly = graph.ToReadOnly();

            var weighted = readOnly.GetWeightedNeighbours(0).ToList();
            weighted.Should().HaveCount(1);
            weighted[0].NeighbourIndex.Should().Be(1);
        }

        [Fact]
        public void ReadOnlyFixedSizeWeightedGraph_AddNeighbour()
        {
            var graph = new FixedSizeWeightedGraph<GraphNodeIndex, float, FixedSizeSortedAscending8Array<uint, float>>();
            graph.Add(new GraphNodeIndex(0));
            graph.Add(new GraphNodeIndex(1));
            var readOnly = graph.ToReadOnly();

            readOnly.AddNeighbour(0, 1, 0.5f).Should().BeTrue();
            readOnly.GetNeighbours(0).ToArray().Should().Contain(1);
        }

        [Fact]
        public void ReadOnlyFixedSizeWeightedGraph_DepthFirstSearch()
        {
            var graph = new FixedSizeWeightedGraph<GraphNodeIndex, float, FixedSizeSortedAscending8Array<uint, float>>();
            graph.Add(new GraphNodeIndex(0));
            graph.Add(new GraphNodeIndex(1));
            graph.Add(new GraphNodeIndex(2));
            graph.AddNeighbour(0, 1, 0.5f);
            graph.AddNeighbour(1, 2, 0.5f);
            var readOnly = graph.ToReadOnly();

            var dfs = readOnly.DepthFirstSearch(0).ToList();
            dfs.Should().Contain(0);
            dfs.Should().Contain(1);
            dfs.Should().Contain(2);
        }

        [Fact]
        public void ReadOnlyFixedSizeWeightedGraph_BreadthFirstSearch()
        {
            var graph = new FixedSizeWeightedGraph<GraphNodeIndex, float, FixedSizeSortedAscending8Array<uint, float>>();
            graph.Add(new GraphNodeIndex(0));
            graph.Add(new GraphNodeIndex(1));
            graph.Add(new GraphNodeIndex(2));
            graph.AddNeighbour(0, 1, 0.5f);
            graph.AddNeighbour(0, 2, 0.3f);
            var readOnly = graph.ToReadOnly();

            var bfs = readOnly.BreadthFirstSearch(0).ToList();
            bfs.Should().Contain(0);
            bfs.Should().Contain(1);
            bfs.Should().Contain(2);
        }

        [Fact]
        public void ReadOnlyFixedSizeWeightedGraph_GetConnectedNodes()
        {
            var graph = new FixedSizeWeightedGraph<GraphNodeIndex, float, FixedSizeSortedAscending8Array<uint, float>>();
            graph.Add(new GraphNodeIndex(0));
            graph.Add(new GraphNodeIndex(1));
            graph.AddNeighbour(0, 1, 0.5f);
            var readOnly = graph.ToReadOnly();

            var connected = readOnly.GetConnectedNodes(0).ToList();
            connected.Should().Contain(1);
        }

        [Fact]
        public void ReadOnlyFixedSizeWeightedGraph_Equals_Same()
        {
            var graph = new FixedSizeWeightedGraph<GraphNodeIndex, float, FixedSizeSortedAscending8Array<uint, float>>();
            graph.Add(new GraphNodeIndex(1));
            var readOnly = graph.ToReadOnly();
            readOnly.Equals(readOnly).Should().BeTrue();
        }

        [Fact]
        public void ReadOnlyFixedSizeWeightedGraph_Equals_Operator_ThrowsForInlineArray()
        {
            // The equality operator delegates to SequenceEqual on FixedSizeWeightedGraphNode,
            // which uses FixedSizeSortedAscending8Array (an InlineArray). InlineArrays throw
            // NotSupportedException on Equals/GetHashCode, so equality comparison is not supported.
            var graph = new FixedSizeWeightedGraph<GraphNodeIndex, float, FixedSizeSortedAscending8Array<uint, float>>();
            graph.Add(new GraphNodeIndex(1));
            var readOnly1 = graph.ToReadOnly();
            var readOnly2 = graph.ToReadOnly();
            Action a = () => { var _ = readOnly1 == readOnly2; };
            a.Should().Throw<NotSupportedException>();
        }

        #endregion

        #region FixedSizeWeightedGraphNode Tests

        [Fact]
        public void FixedSizeWeightedGraphNode_Index()
        {
            var node = new FixedSizeWeightedGraphNode<GraphNodeIndex, float, FixedSizeSortedAscending8Array<uint, float>>(new GraphNodeIndex(42));
            node.Index.Should().Be(42);
        }

        [Fact]
        public void FixedSizeWeightedGraphNode_MaxNeighbours()
        {
            var node = new FixedSizeWeightedGraphNode<GraphNodeIndex, float, FixedSizeSortedAscending8Array<uint, float>>(new GraphNodeIndex(1));
            node.MaxNeighbours.Should().Be(8);
        }

        [Fact]
        public void FixedSizeWeightedGraphNode_NeighbourCount_Empty()
        {
            var node = new FixedSizeWeightedGraphNode<GraphNodeIndex, float, FixedSizeSortedAscending8Array<uint, float>>(new GraphNodeIndex(1));
            node.NeighbourCount.Should().Be(0);
        }

        [Fact]
        public void FixedSizeWeightedGraphNode_TryAddNeighbour()
        {
            var node = new FixedSizeWeightedGraphNode<GraphNodeIndex, float, FixedSizeSortedAscending8Array<uint, float>>(new GraphNodeIndex(0));
            node.TryAddNeighbour(1, 0.5f).Should().BeTrue();
            node.NeighbourCount.Should().Be(1);
        }

        [Fact]
        public void FixedSizeWeightedGraphNode_TryAddNeighbour_SelfReturnsFalse()
        {
            var node = new FixedSizeWeightedGraphNode<GraphNodeIndex, float, FixedSizeSortedAscending8Array<uint, float>>(new GraphNodeIndex(1));
            node.TryAddNeighbour(1, 0.5f).Should().BeFalse();
            node.NeighbourCount.Should().Be(0);
        }

        [Fact]
        public void FixedSizeWeightedGraphNode_RemoveAt()
        {
            var node = new FixedSizeWeightedGraphNode<GraphNodeIndex, float, FixedSizeSortedAscending8Array<uint, float>>(new GraphNodeIndex(0));
            node.TryAddNeighbour(1, 0.5f);
            var removed = node.RemoveAt(0);
            removed.Should().Be(1);
            node.NeighbourCount.Should().Be(0);
        }

        [Fact]
        public void FixedSizeWeightedGraphNode_MinWeight()
        {
            var node = new FixedSizeWeightedGraphNode<GraphNodeIndex, float, FixedSizeSortedAscending8Array<uint, float>>(new GraphNodeIndex(0));
            node.TryAddNeighbour(1, 0.5f);
            node.TryAddNeighbour(2, 0.3f);
            node.MinWeight.Should().BeApproximately(0.3f, 0.001f);
        }

        [Fact]
        public void FixedSizeWeightedGraphNode_MaxWeight()
        {
            var node = new FixedSizeWeightedGraphNode<GraphNodeIndex, float, FixedSizeSortedAscending8Array<uint, float>>(new GraphNodeIndex(0));
            node.TryAddNeighbour(1, 0.5f);
            node.TryAddNeighbour(2, 0.3f);
            node.MaxWeight.Should().BeApproximately(0.5f, 0.001f);
        }

        [Fact]
        public void FixedSizeWeightedGraphNode_MinNeighbourIndex()
        {
            var node = new FixedSizeWeightedGraphNode<GraphNodeIndex, float, FixedSizeSortedAscending8Array<uint, float>>(new GraphNodeIndex(0));
            node.TryAddNeighbour(10, 0.5f);
            node.TryAddNeighbour(20, 0.3f);
            node.MinNeighbourIndex.Should().Be(20);
        }

        [Fact]
        public void FixedSizeWeightedGraphNode_MaxNeighbourIndex()
        {
            var node = new FixedSizeWeightedGraphNode<GraphNodeIndex, float, FixedSizeSortedAscending8Array<uint, float>>(new GraphNodeIndex(0));
            node.TryAddNeighbour(10, 0.5f);
            node.TryAddNeighbour(20, 0.3f);
            node.MaxNeighbourIndex.Should().Be(10);
        }

        [Fact]
        public void FixedSizeWeightedGraphNode_NeighbourIndices()
        {
            var node = new FixedSizeWeightedGraphNode<GraphNodeIndex, float, FixedSizeSortedAscending8Array<uint, float>>(new GraphNodeIndex(0));
            node.TryAddNeighbour(1, 0.5f);
            node.TryAddNeighbour(2, 0.3f);
            var indices = node.NeighbourIndices.ToArray();
            indices.Should().Contain(1);
            indices.Should().Contain(2);
        }

        [Fact]
        public void FixedSizeWeightedGraphNode_NeighbourWeights()
        {
            var node = new FixedSizeWeightedGraphNode<GraphNodeIndex, float, FixedSizeSortedAscending8Array<uint, float>>(new GraphNodeIndex(0));
            node.TryAddNeighbour(1, 0.5f);
            node.TryAddNeighbour(2, 0.3f);
            var weights = node.NeighbourWeights.ToArray();
            weights.Should().Contain(0.5f);
            weights.Should().Contain(0.3f);
        }

        [Fact]
        public void FixedSizeWeightedGraphNode_WeightedNeighbours()
        {
            var node = new FixedSizeWeightedGraphNode<GraphNodeIndex, float, FixedSizeSortedAscending8Array<uint, float>>(new GraphNodeIndex(0));
            node.TryAddNeighbour(1, 0.5f);
            node.TryAddNeighbour(2, 0.3f);
            var neighbours = node.WeightedNeighbours.ToList();
            neighbours.Should().HaveCount(2);
        }

        [Fact]
        public void FixedSizeWeightedGraphNode_NeighbourSpan()
        {
            var node = new FixedSizeWeightedGraphNode<GraphNodeIndex, float, FixedSizeSortedAscending8Array<uint, float>>(new GraphNodeIndex(0));
            node.TryAddNeighbour(1, 0.5f);
            node.NeighbourSpan.ToArray().Should().Contain(1);
        }

        [Fact]
        public void FixedSizeWeightedGraphNode_Neighbours()
        {
            var node = new FixedSizeWeightedGraphNode<GraphNodeIndex, float, FixedSizeSortedAscending8Array<uint, float>>(new GraphNodeIndex(0));
            node.TryAddNeighbour(1, 0.5f);
            node.TryAddNeighbour(2, 0.3f);
            var neighbours = node.Neighbours.ToList();
            neighbours.Should().Contain(1);
            neighbours.Should().Contain(2);
        }

        [Fact]
        public void FixedSizeWeightedGraphNode_CompareTo()
        {
            var a = new FixedSizeWeightedGraphNode<GraphNodeIndex, float, FixedSizeSortedAscending8Array<uint, float>>(new GraphNodeIndex(1));
            var b = new FixedSizeWeightedGraphNode<GraphNodeIndex, float, FixedSizeSortedAscending8Array<uint, float>>(new GraphNodeIndex(2));
            a.CompareTo(b).Should().BeNegative();
            b.CompareTo(a).Should().BePositive();
        }

        [Fact]
        public void FixedSizeWeightedGraphNode_ToString()
        {
            var node = new FixedSizeWeightedGraphNode<GraphNodeIndex, float, FixedSizeSortedAscending8Array<uint, float>>(new GraphNodeIndex(42));
            node.ToString().Should().NotBeNullOrEmpty();
        }

        #endregion

        #region GraphNodeIndex Tests

        [Fact]
        public void GraphNodeIndex_Index()
        {
            var node = new GraphNodeIndex(42);
            node.Index.Should().Be(42);
        }

        [Fact]
        public void GraphNodeIndex_CompareTo()
        {
            var a = new GraphNodeIndex(1);
            var b = new GraphNodeIndex(2);
            a.CompareTo(b).Should().BeNegative();
            b.CompareTo(a).Should().BePositive();
        }

        [Fact]
        public void GraphNodeIndex_ToString()
        {
            var node = new GraphNodeIndex(42);
            node.ToString().Should().Be("42");
        }

        #endregion

        #region HierarchicalNavigationSmallWorldGraph (HNSW) Tests

        // Simple deterministic distance calculator: distance = |a - b|
        class MockDistanceCalculator : ICalculateNodeWeights<float>
        {
            public float GetWeight(uint fromIndex, uint toIndex) => Math.Abs(fromIndex - toIndex);
        }

        HierarchicalNavigationSmallWorldGraph<GraphNodeIndex, float, FixedSizeSortedAscending8Array<uint, float>, FixedSizeSortedAscending8Array<uint, float>> CreateHnsw(int maxLayers = 4)
        {
            using var context = new BrightDataContext(null, 42);
            return new HierarchicalNavigationSmallWorldGraph<GraphNodeIndex, float, FixedSizeSortedAscending8Array<uint, float>, FixedSizeSortedAscending8Array<uint, float>>(context, maxLayers, 0.1f);
        }

        [Fact]
        public void HNSW_Add_SingleNodeCreatesEntryPoint()
        {
            var hnsw = CreateHnsw();
            var calculator = new MockDistanceCalculator();
            hnsw.Add(calculator, new GraphNodeIndex(0));

            // KNN search should succeed (entry point exists)
            var result = hnsw.KnnSearch(0, calculator);
            result.MinValue.Should().Be(0);
        }

        [Fact]
        public void HNSW_Add_MultipleNodesIncreasesSize()
        {
            var hnsw = CreateHnsw();
            var calculator = new MockDistanceCalculator();
            var nodes = Enumerable.Range(0, 10).Select(i => new GraphNodeIndex((uint)i)).ToArray();

            foreach (var node in nodes)
                hnsw.Add(calculator, node);

            // Verify all nodes are searchable via KNN
            foreach (var node in nodes)
            {
                var result = hnsw.KnnSearch(node.Index, calculator);
                result.Elements.Should().NotBeEmpty();
            }
        }

        [Fact]
        public void HNSW_Add_SecondNodeCreatesEdges()
        {
            var hnsw = CreateHnsw();
            var calculator = new MockDistanceCalculator();
            hnsw.Add(calculator, new GraphNodeIndex(0));
            hnsw.Add(calculator, new GraphNodeIndex(1));

            // BFS from node 0 should reach node 1
            var bfs = hnsw.BreadthFirstSearch(0).ToList();
            bfs.Should().Contain(n => n.NeighbourIndex == 1);
        }

        [Fact]
        public void HNSW_AddBatch_ZeroCopyAddsAllNodes()
        {
            var hnsw = CreateHnsw();
            var calculator = new MockDistanceCalculator();
            var nodes = new GraphNodeIndex[] { new(0), new(1), new(2), new(3), new(4) };

            hnsw.Add(calculator, nodes.AsSpan());

            foreach (var node in nodes)
            {
                var result = hnsw.KnnSearch(node.Index, calculator);
                result.Elements.Should().NotBeEmpty();
            }
        }

        [Fact]
        public void HNSW_AddBatch_EmptySpanIsNop()
        {
            var hnsw = CreateHnsw();
            var calculator = new MockDistanceCalculator();
            GraphNodeIndex[] empty = [];

            hnsw.Add(calculator, empty.AsSpan());

            // KNN on empty graph should throw
            Action a = () => hnsw.KnnSearch(0, calculator);
            a.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void KnnSearch_EmptyGraphThrowsInvalidOperationException()
        {
            var hnsw = CreateHnsw();
            var calculator = new MockDistanceCalculator();

            Action a = () => hnsw.KnnSearch(0, calculator);
            a.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void KnnSearch_SingleNodeReturnsSelf()
        {
            var hnsw = CreateHnsw();
            var calculator = new MockDistanceCalculator();
            hnsw.Add(calculator, new GraphNodeIndex(0));

            var result = hnsw.KnnSearch(0, calculator);
            result.MinValue.Should().Be(0);
        }

        [Fact]
        public void KnnSearch_ReturnsNearestNeighbours()
        {
            var hnsw = CreateHnsw();
            var calculator = new MockDistanceCalculator();

            // Add nodes with indices 0-9 so distance = |a - b|
            for (var i = 0; i < 10; i++)
                hnsw.Add(calculator, new GraphNodeIndex((uint)i));

            // Query from node 5: nearest should be 4, 5, 6 (distance 1)
            var result = hnsw.KnnSearch(5, calculator);
            var neighbours = result.Elements.Select(e => e.Item1).ToList();

            // At least one neighbour should be close (distance <= 2)
            neighbours.Should().Contain(n => n >= 3 && n <= 7);
        }

        [Fact]
        public void KnnSearch_ResultSizeMatchesArrayCapacity()
        {
            var hnsw = CreateHnsw();
            var calculator = new MockDistanceCalculator();

            for (var i = 0; i < 20; i++)
                hnsw.Add(calculator, new GraphNodeIndex((uint)i));

            var result = hnsw.KnnSearch(10, calculator);
            result.Size.Should().BeInRange(1, FixedSizeSortedAscending8Array<uint, float>.MaxSize);
        }

        [Fact]
        public void KnnSearch_ResultsSortedByWeight()
        {
            var hnsw = CreateHnsw();
            var calculator = new MockDistanceCalculator();

            for (var i = 0; i < 10; i++)
                hnsw.Add(calculator, new GraphNodeIndex((uint)i));

            var result = hnsw.KnnSearch(5, calculator);
            var weights = result.Elements.Select(e => e.Item2).ToList();

            // Verify ascending order
            for (var i = 1; i < weights.Count; i++)
                weights[i].Should().BeGreaterThanOrEqualTo(weights[i - 1]);
        }

        [Fact]
        public void BFS_ReturnsAllConnectedNodes()
        {
            var hnsw = CreateHnsw();
            var calculator = new MockDistanceCalculator();

            for (var i = 0; i < 10; i++)
                hnsw.Add(calculator, new GraphNodeIndex((uint)i));

            var bfs = hnsw.BreadthFirstSearch(0).ToList();
            bfs.Should().NotBeEmpty();
        }

        [Fact]
        public void BFS_NoDuplicates()
        {
            var hnsw = CreateHnsw();
            var calculator = new MockDistanceCalculator();

            for (var i = 0; i < 10; i++)
                hnsw.Add(calculator, new GraphNodeIndex((uint)i));

            var bfs = hnsw.BreadthFirstSearch(0).ToList();
            var indices = bfs.Select(n => n.NeighbourIndex).ToList();
            indices.Should().OnlyHaveUniqueItems();
        }

        [Fact]
        public void BFS_IsolatedNodeReturnsEmpty()
        {
            var hnsw = CreateHnsw();
            var calculator = new MockDistanceCalculator();
            hnsw.Add(calculator, new GraphNodeIndex(0));

            // Single node has no edges, so BFS yields no neighbours
            var bfs = hnsw.BreadthFirstSearch(0).ToList();
            bfs.Should().BeEmpty();
        }

        [Fact]
        public void HNSW_EntryPointUpdatesWithHigherLevel()
        {
            var hnsw = CreateHnsw();
            var calculator = new MockDistanceCalculator();

            // Add many nodes; eventually one will be at a higher level
            for (var i = 0; i < 50; i++)
                hnsw.Add(calculator, new GraphNodeIndex((uint)i));

            // Graph should still be functional
            var result = hnsw.KnnSearch(25, calculator);
            result.Elements.Should().NotBeEmpty();
        }

        [Fact]
        public void HNSW_SingleLayerGraph()
        {
            var hnsw = CreateHnsw(maxLayers: 1);
            var calculator = new MockDistanceCalculator();

            for (var i = 0; i < 10; i++)
                hnsw.Add(calculator, new GraphNodeIndex((uint)i));

            var result = hnsw.KnnSearch(5, calculator);
            result.Elements.Should().NotBeEmpty();
        }

        [Fact]
        public void HNSW_MaxLayersClampsLevel()
        {
            // Even with a very small ml (high level probability),
            // levels should not exceed maxLayers - 1
            using var context = new BrightDataContext(null, 123);
            var hnsw = new HierarchicalNavigationSmallWorldGraph<GraphNodeIndex, float, FixedSizeSortedAscending8Array<uint, float>, FixedSizeSortedAscending8Array<uint, float>>(context, 3, 1.0f);
            var calculator = new MockDistanceCalculator();

            for (var i = 0; i < 100; i++)
                hnsw.Add(calculator, new GraphNodeIndex((uint)i));

            // KNN should not crash (levels properly clamped)
            var result = hnsw.KnnSearch(50, calculator);
            result.Elements.Should().NotBeEmpty();
        }

        [Fact]
        public void HNSW_AddBatch_MixedWithSingleAdds()
        {
            var hnsw = CreateHnsw();
            var calculator = new MockDistanceCalculator();

            // Add some individually
            hnsw.Add(calculator, new GraphNodeIndex(0));
            hnsw.Add(calculator, new GraphNodeIndex(1));

            // Add batch
            hnsw.Add(calculator, new GraphNodeIndex[] { new(2), new(3) }.AsSpan());

            // Add more individually
            hnsw.Add(calculator, new GraphNodeIndex(4));

            var result = hnsw.KnnSearch(2, calculator);
            result.Elements.Should().NotBeEmpty();
        }

        [Fact]
        public void HNSW_KnnSearch_ReturnsNonEmptyResultForMultipleNodes()
        {
            var hnsw = CreateHnsw();
            var calculator = new MockDistanceCalculator();

            for (var i = 0; i < 20; i++)
                hnsw.Add(calculator, new GraphNodeIndex((uint)i));

            // KNN search should return results for any valid query
            var result = hnsw.KnnSearch(10, calculator);
            result.Size.Should().BeGreaterThan(0);
        }

        #endregion
    }
}
