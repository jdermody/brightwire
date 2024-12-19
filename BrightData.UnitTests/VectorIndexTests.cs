using System.Linq;
using BrightData.LinearAlgebra.VectorIndexing;
using BrightData.UnitTests.Helper;
using FluentAssertions;
using Xunit;

namespace BrightData.UnitTests
{
    public class VectorIndexTests : UnitTestBase
    {
        [Fact]
        public void Average()
        {
            var index = VectorSet<float>.CreateFlat(4);
            index.Add([0, 1, 1, 1]);
            index.Add([0, 0, 0, 0]);
            index.Add([0, -1, -1, -1]);

            var average = index.GetAverage(0, 1, 2);
            average.Should().BeEquivalentTo([0, 0, 0, 0]);
        }

        [Fact]
        public void FlatIndex()
        {
            var index = VectorSet<float>.CreateFlat(4);
            index.Add([0, 1, 1, 1]);
            index.Add([0, 0, 0, 0]);
            index.Add([0, -1, -1, -1]);

            var rank = index.Rank([0, 2, 2, 2]).ToArray();
            rank.First().Should().Be(0);
            rank.Last().Should().Be(2);
        }

        [Fact]
        public void HNSW()
        {
            var index = VectorSet<float>.CreateHNSW(_context, 4);
            index.Add([0, 1, 1, 1]);
            index.Add([0, 0, 0, 0]);
            index.Add([0, -1, -1, -1]);

            var rank = index.Rank([0, 2, 2, 2]).ToArray();
            rank.First().Should().Be(0);
            rank.Last().Should().Be(2);
        }

        [Fact]
        public void RandomProjection()
        {
            var index = VectorSet<float>.CreateRandomProjection(_context.LinearAlgebraProvider, 4, 3);
            index.Add([0, 1, 1, 1]);
            index.Add([0, 0, 0, 0]);
            index.Add([0, -1, -1, -1]);

            var rank = index.Rank([0, 2, 2, 2]).ToArray();
            rank.First().Should().Be(0);
            rank.Last().Should().Be(2);
        }

        [Fact]
        public void BallTree()
        {
            var index = VectorSet<float>.CreateKnnSearch(4, x => x.BallTreeSearch());
            index.Add([0, 1, 1, 1]);
            index.Add([0, 0, 0, 0]);
            index.Add([0, -1, -1, -1]);

            var rank = index.Rank([0, 2, 2, 2]).ToArray();
            rank.First().Should().Be(0);
            rank.Last().Should().Be(2);
        }

        [Fact]
        public void KDTree()
        {
            var index = VectorSet<float>.CreateKnnSearch(4, x => x.KDTreeSearch());
            index.Add([0, 1, 1, 1]);
            index.Add([0, 0, 0, 0]);
            index.Add([0, -1, -1, -1]);

            var rank = index.Rank([0, 2, 2, 2]).ToArray();
            rank.First().Should().Be(0);
            rank.Last().Should().Be(2);
        }

        [Fact]
        public void BitCompression()
        {
            var index = VectorSet<float>.CreateBitCompressionIndex(4);
            index.Add([0, 1, 1, 1]);
            index.Add([0, 0, 0, 0]);
            index.Add([0, -1, -1, -1]);

            var rank = index.Rank([0, 2, 2, 2]).ToArray();
            rank.First().Should().Be(0);
            rank.Last().Should().Be(2);
        }
    }
}
