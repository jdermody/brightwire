using System;
using BrightData.UnitTests.Helper;
using System.Linq;
using BrightData.LinearAlgebra.VectorIndexing;
using BrightData.LinearAlgebra.VectorIndexing.Helper;
using BrightData.Types;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace BrightData.UnitTests
{
    public class VectorSetTests : UnitTestBase
    {
        readonly ITestOutputHelper _testOutputHelper;

        public VectorSetTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void Average()
        {
            using var set = new VectorSet<float>(4);
            set.Add(_context.CreateReadOnlyVector(0, 0, 0, 0));
            set.Add(_context.CreateReadOnlyVector(1, 1, 1, 1));
            var average = set.GetAverage([0U, 1U]);
            average.Should().AllBeEquivalentTo(0.5f);
        }

        [Fact]
        public void Rank1()
        {
            using var set = new VectorSet<float>(4);
            set.Add(_context.CreateReadOnlyVector(0, 0, 0, 0));
            set.Add(_context.CreateReadOnlyVector(1, 1, 1, 1));
            var rank = set.Rank(_context.CreateReadOnlyVector(0.8f, 0.8f, 0.8f, 0.8f));
            rank.First().Should().Be(1);
        }

        [Fact]
        public void Rank2()
        {
            using var set = new VectorSet<float>(4);
            set.Add(_context.CreateReadOnlyVector(0, 0, 0, 0));
            set.Add(_context.CreateReadOnlyVector(1, 1, 1, 1));
            var rank = set.Rank(_context.CreateReadOnlyVector(0.45f, 0.45f, 0.45f, 0.45f));
            rank.First().Should().Be(0);
        }

        [Fact]
        public void Closest()
        {
            using var set = new VectorSet<float>(4);
            set.Add(_context.CreateReadOnlyVector(0, 0, 0, 0));
            set.Add(_context.CreateReadOnlyVector(1, 1, 1, 1));
            var score = set.Closest([
                _context.CreateReadOnlyVector(0.5f, 0.5f, 0.5f, 0.5f), // 0
                _context.CreateReadOnlyVector(0.9f, 0.9f, 0.9f, 0.9f), // 1
                _context.CreateReadOnlyVector(0.1f, 0.1f, 0.1f, 0.1f), // 2
            ], DistanceMetric.Euclidean);
            score[0].Should().Be(2);
            score[1].Should().Be(1);
        }

        [Fact]
        public void TestVectorGraphNode()
        {
            var node = new IndexedFixedSizeGraphNode<float>(1);
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
    }
}
