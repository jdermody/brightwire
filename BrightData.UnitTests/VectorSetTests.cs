using BrightData.UnitTests.Helper;
using System.Linq;
using BrightData.LinearAlgebra.VectorIndexing;
using BrightData.Types;
using FluentAssertions;
using Xunit;

namespace BrightData.UnitTests
{
    public class VectorSetTests : UnitTestBase
    {
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
    }
}
