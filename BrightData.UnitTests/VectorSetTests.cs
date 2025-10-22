using BrightData.UnitTests.Helper;
using System.Linq;
using BrightData.Helper.Vectors;
using BrightData.LinearAlgebra.VectorIndexing;
using BrightData.Types;
using BrightData.Types.Graph;
using FluentAssertions;
using Xunit;
using BrightData.Types.Graph.Helper;

namespace BrightData.UnitTests
{
    public class VectorSetTests : UnitTestBase
    {
        [Fact]
        public void Average()
        {
            using var set = VectorSet<float>.CreateFlat(4);
            set.Add(_context.CreateReadOnlyVector(0, 0, 0, 0));
            set.Add(_context.CreateReadOnlyVector(1, 1, 1, 1));
            var average = set.GetAverage(0U, 1U);
            average.Should().AllBeEquivalentTo(0.5f);
        }

        [Fact]
        public void Rank1()
        {
            using var set = VectorSet<float>.CreateFlat(4, DistanceMetric.Euclidean);
            set.Add(_context.CreateReadOnlyVector(0, 0, 0, 0));
            set.Add(_context.CreateReadOnlyVector(1, 1, 1, 1));
            var rank = set.Rank(_context.CreateReadOnlyVector(0.8f, 0.8f, 0.8f, 0.8f));
            rank.First().Should().Be(1);
        }

        [Fact]
        public void Rank2()
        {
            using var set = VectorSet<float>.CreateFlat(4, DistanceMetric.Euclidean);
            set.Add(_context.CreateReadOnlyVector(0, 0, 0, 0));
            set.Add(_context.CreateReadOnlyVector(1, 1, 1, 1));
            var rank = set.Rank(_context.CreateReadOnlyVector(0.45f, 0.45f, 0.45f, 0.45f));
            rank.First().Should().Be(0);
        }

        [Fact]
        public void Closest()
        {
            using var set = VectorSet<float>.CreateFlat(4, DistanceMetric.Euclidean);
            set.Add(_context.CreateReadOnlyVector(0, 0, 0, 0));
            set.Add(_context.CreateReadOnlyVector(1, 1, 1, 1));
            var score = set.Closest(
                _context.CreateReadOnlyVector(0.5f, 0.5f, 0.5f, 0.5f), // 0
                _context.CreateReadOnlyVector(0.9f, 0.9f, 0.9f, 0.9f), // 1
                _context.CreateReadOnlyVector(0.1f, 0.1f, 0.1f, 0.1f)  // 2
            );
            score[0].Should().Be(2);
            score[1].Should().Be(1);
        }

        [Fact]
        public void TestVectorGraphNode()
        {
            var node = new FixedSizeWeightedGraphNode<GraphNodeIndex, float, FixedSizeSortedAscending8Array<uint, float>>(new(1));
            node.Index.Should().Be(1);
            node.NeighbourIndices.Length.Should().Be(0);

            node.TryAddNeighbour(2, 0.9f);
            node.NeighbourIndices[0].Should().Be(2);
            node.NeighbourWeights[0].Should().Be(0.9f);

            node.TryAddNeighbour(3, 0.8f);
            node.NeighbourIndices[0].Should().Be(3);
            node.NeighbourWeights[0].Should().Be(0.8f);

            for(var i = 4U; i <= 10; i++)
                node.TryAddNeighbour(i, 1f - 0.1f * i);
            node.NeighbourIndices.Length.Should().Be(8);
            node.NeighbourIndices[0].Should().Be(10);
            node.NeighbourIndices[1].Should().Be(9);

            node.TryAddNeighbour(20, 0.5f).Should().BeTrue();
            node.TryAddNeighbour(20, 0.5f).Should().BeFalse();
        }

        [Fact]
        public void TestKDTree()
        {
            var vectors = VectorSet<float>.GetStorage(VectorStorageType.InMemory, 4, 5);
            vectors.Add([0.1f, 0.1f, 0.1f, 0.1f]);
            vectors.Add([0.2f, 0.2f, 0.2f, 0.2f]);
            vectors.Add([0.3f, 0.3f, 0.3f, 0.3f]);
            vectors.Add([0.4f, 0.4f, 0.4f, 0.4f]);
            vectors.Add([0.5f, 0.5f, 0.5f, 0.5f]);
            var tree = new VectorKDTree<float>(vectors);

            for (var i = 0U; i < 5; i++) {
                tree.GetNodeByVectorIndex(i).VectorIndex.Should().Be(i);
                tree.Search(vectors[i]).BestVectorIndex.Should().Be(i);
                tree.KnnSearch<FixedSizeSortedAscending5Array<uint, float>>(vectors[i])[0].Value.Should().Be(i);
            }
        }

        [Fact]
        public void TestBallTree()
        {
            var vectors = VectorSet<float>.GetStorage(VectorStorageType.InMemory, 4, 5);
            vectors.Add([0.1f, 0.1f, 0.1f, 0.1f]);
            vectors.Add([0.2f, 0.2f, 0.2f, 0.2f]);
            vectors.Add([0.3f, 0.3f, 0.3f, 0.3f]);
            vectors.Add([0.4f, 0.4f, 0.4f, 0.4f]);
            vectors.Add([0.5f, 0.5f, 0.5f, 0.5f]);
            var tree = new VectorBallTree<float>(vectors, DistanceMetric.Euclidean);

            for (var i = 0U; i < 5; i++) {
                tree.Search(vectors[i]).BestVectorIndex.Should().Be(i);
                tree.KnnSearch<FixedSizeSortedAscending5Array<uint, float>>(vectors[i])[0].Value.Should().Be(i);
            }
        }
    }
}
