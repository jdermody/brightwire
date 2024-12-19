using System;
using System.Linq;
using BrightData.Types;
using FluentAssertions;
using Xunit;

namespace BrightData.UnitTests
{
    public class BitVectorUnitTests
    {
        [Fact]
        public void CountOfSetBits()
        {
            var v1 = new BitVector(8) {
                [0] = true,
                [2] = true,
                [5] = true
            };
            v1.CountOfSetBits().Should().Be(3);
        }

        [Fact]
        public void Range()
        {
            var v1 = new BitVector(8);
            for (var i = 2; i <= 6; i++)
                v1[i].Should().BeFalse();
            v1.SetBits(2..6);
            for (var i = 2; i < 6; i++)
                v1[i].Should().BeTrue();
        }

        [Fact]
        public void ContiguousRanges()
        {
            var v1 = new BitVector(8);
            v1.SetBits(2..4);
            v1.SetBits(6..8);
            var ranges = v1.GetContiguousRanges().ToList();
            ranges.Should().HaveCount(2);
            ranges[0].Should().Be(new Range(2, 4));
            ranges[1].Should().Be(new Range(6, 8));
        }

        [Fact]
        public void ContiguousRanges2()
        {
            var v1 = new BitVector(8);
            v1.SetBits(2..4);
            v1.SetBits(4..8);
            var ranges = v1.GetContiguousRanges().ToList();
            ranges.Should().HaveCount(1);
            ranges[0].Should().Be(new Range(2, 8));
        }

        [Fact]
        public void ClearBits()
        {
            var v1 = new BitVector(8);
            v1.SetBits(..8);
            v1.CountOfSetBits().Should().Be(8);
            v1.Clear();
            v1.CountOfSetBits().Should().Be(0);
        }

        [Fact]
        public void Xor()
        {
            var v1 = new BitVector(8) {
                [2] = true
            };
            var v2 = new BitVector(8) {
                [2] = true,
                [4] = true
            };
            var result = v1.XorWith(v2);
            result[2].Should().BeFalse();
            result[4].Should().BeTrue();
        }

        [Fact]
        public void UnionWith()
        {
            var v1 = new BitVector(8) {
                [2] = true
            };
            var v2 = new BitVector(8) {
                [4] = true
            };
            var result = v1.UnionWith(v2);
            result[2].Should().BeTrue();
            result[4].Should().BeTrue();
        }

        [Fact]
        public void IntersectionWith()
        {
            var v1 = new BitVector(8) {
                [2] = true
            };
            var v2 = new BitVector(8) {
                [2] = true,
                [4] = true
            };
            var result = v1.IntersectionWith(v2);
            result[2].Should().BeTrue();
            result[4].Should().BeFalse();
        }

        [Fact]
        public void HammingDistance()
        {
            var v1 = new BitVector(8) {
                [2] = true,
                [3] = true
            };
            var v2 = new BitVector(8) {
                [3] = true,
                [4] = true
            };

            var distance = v1.HammingDistance(v2);
            distance.Should().Be(2);
        }
    }
}
