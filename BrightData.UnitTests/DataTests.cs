using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightData.Helper;
using FluentAssertions;
using Xunit;

namespace BrightData.UnitTests
{
    public class DataTests
    {
        [Fact]
        public void FindContiguousRanges()
        {
            var items = new uint[] { 1, 2, 3, 6, 6, 7, 9, 10 };
            var ranges = items.FindDistinctContiguousRanges().ToArray();
            ranges.Length.Should().Be(3);

            ranges[0].First.Should().Be(1);
            ranges[0].Last.Should().Be(3);

            ranges[1].First.Should().Be(6);
            ranges[1].Last.Should().Be(7);

            ranges[2].First.Should().Be(9);
            ranges[2].Last.Should().Be(10);
        }

        [Fact]
        public void FindDistinctContiguousRanges()
        {
            // Test with empty input
            var empty = new uint[] {};
            var emptyRanges = empty.FindDistinctContiguousRanges();
            emptyRanges.Should().BeEmpty();

            // Test with input containing a single value
            var single = new uint[] { 1 };
            var singleRanges = single.FindDistinctContiguousRanges();
            singleRanges.Should().ContainSingle(range => range.First == 1 && range.Last == 1);

            // Test with input containing a single range
            var singleRange = new uint[] { 1, 2, 3, 4 };
            var singleRangeRanges = singleRange.FindDistinctContiguousRanges();
            singleRangeRanges.Should().ContainSingle(range => range.First == 1 && range.Last == 4);

            // Test with input containing multiple ranges
            var multipleRanges = new uint[] { 1, 2, 3, 4, 10, 11, 12, 13, 20, 21, 22, 23 };
            var multipleRangesRanges = multipleRanges.FindDistinctContiguousRanges();
            multipleRangesRanges.Should()
                .ContainInOrder(
                    (First: 1, Last: 4),
                    (First: 10, Last: 13),
                    (First: 20, Last: 23)
                );

            // Test with input containing repeated values
            var repeated = new uint[] { 1, 1, 2, 3, 3, 3, 4, 4, 5, 5, 5, 5 };
            var repeatedRanges = repeated.FindDistinctContiguousRanges();
            repeatedRanges.Should().ContainSingle(range => range.First == 1 && range.Last == 5);
        }

        [Fact]
        public void FindAllPairs()
        {
            var items = new uint[] { 1, 2, 3 };
            var pairs = items.FindAllPairs().ToArray();
            pairs.Length.Should().Be(3);

            pairs[0].First.Should().Be(1);
            pairs[0].Second.Should().Be(2);

            pairs[1].First.Should().Be(1);
            pairs[1].Second.Should().Be(3);

            pairs[2].First.Should().Be(2);
            pairs[2].Second.Should().Be(3);
        }

        [Fact]
        public void FindAllPairs2()
        {
            var input = new[] { 1, 2, 3, 4 };
            var expectedPairs = new[]
            {
                (1, 2), (1, 3), (1, 4),
                (2, 3), (2, 4),
                (3, 4)
            };

            var pairs = input.FindAllPairs().ToArray();
            pairs.Should().BeEquivalentTo(expectedPairs);
        }

        [Fact]
        public void FindPermutations()
        {
            var input = new[] { 1, 2, 3 };
            var permutations = input.FindPermutations().ToArray();
            permutations.Length.Should().Be(4);
            permutations[0].Should().BeEquivalentTo(ImmutableList.Create(1, 2));
            permutations[1].Should().BeEquivalentTo(ImmutableList.Create(1, 3));
            permutations[2].Should().BeEquivalentTo(ImmutableList.Create(1, 2, 3));
            permutations[3].Should().BeEquivalentTo(ImmutableList.Create(2, 3));
        }

        [Fact]
        public void CosineSimilarity_SameVectors()
        {
            var v1 = new[] { 1f, 2f, 3f, 4f };
            var v2 = new[] { 1f, 2f, 3f, 4f };
            var result = v1.CosineDistance(v2);
            FloatMath.AreApproximatelyEqual(result, 0f, 30).Should().BeTrue();
        }

        [Fact]
        public void CosineSimilarity_OrthogonalVectors()
        {
            var v1 = new[] { 1f, 0f, 0f };
            var v2 = new[] { 0f, 1f, 0f };
            var result = v1.CosineDistance(v2);
            Assert.Equal(1f, result);
        }

        [Fact]
        public void CosineSimilarity_OppositeVectors()
        {
            var v1 = new[] { 1f, 2f, 3f, 4f };
            var v2 = new[] { -1f, -2f, -3f, -4f };
            var result = v1.CosineDistance(v2);
            Assert.Equal(2f, result);
        }

        [Fact]
        public void CosineSimilarity_DifferentVectorLengths()
        {
            var v1 = new[] { 1f, 2f, 3f };
            var v2 = new[] { 1f, 2f, 3f, 4f };
            Assert.Throws<ArgumentException>(() => v1.CosineDistance(v2));
        }
    }
}
