using BrightData.UnitTests.Helper;
using System.Linq;
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
            var set = new VectorSet(4);
            set.Add(_context.CreateReadOnlyVector(0, 0, 0, 0));
            set.Add(_context.CreateReadOnlyVector(1, 1, 1, 1));
            var average = set.GetAverage(new[] { 0U, 1U });
            average.Should().AllBeEquivalentTo(0.5f);
        }

        [Fact]
        public void Rank1()
        {
            var set = new VectorSet(4);
            set.Add(_context.CreateReadOnlyVector(0, 0, 0, 0));
            set.Add(_context.CreateReadOnlyVector(1, 1, 1, 1));
            var rank = set.Rank(_context.CreateReadOnlyVector(0.8f, 0.8f, 0.8f, 0.8f));
            rank.First().Should().Be(1);
        }

        [Fact]
        public void Rank2()
        {
            var set = new VectorSet(4);
            set.Add(_context.CreateReadOnlyVector(0, 0, 0, 0));
            set.Add(_context.CreateReadOnlyVector(1, 1, 1, 1));
            var rank = set.Rank(_context.CreateReadOnlyVector(0.45f, 0.45f, 0.45f, 0.45f));
            rank.First().Should().Be(0);
        }
    }
}
