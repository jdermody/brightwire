using System;
using System.Linq;
using BrightData.UnitTests.Helper;
using FluentAssertions;
using Xunit;

namespace BrightData.UnitTests
{
    public class SpanTests : UnitTestBase
    {
        [Fact]
        public void ZipParallel()
        {
            var v1 = CreateFloatSpan(512);
            var v2 = CreateFloatSpan(512);
            using var output = v1.ZipParallel(v2, (x, y) => x + y);
            var outputSpan = output.Span;
            for (var i = 0; i < 512; i++) {
                outputSpan[i].Should().Be(v1[i] + v2[i]);
            }
        }

        [Fact]
        public void MapParallel()
        {
            var v1 = CreateFloatSpan(512);
            using var output = v1.MapParallel(x => x * 2);
            var outputSpan = output.Span;
            for (var i = 0; i < 512; i++) {
                outputSpan[i].Should().Be(v1[i] * 2);
            }
        }

        [Fact]
        public void MapParallel2()
        {
            var v1 = CreateFloatSpan(512);
            using var output = v1.MapParallel((i, x) => x * i);
            var outputSpan = output.Span;
            for (var i = 0; i < 512; i++) {
                outputSpan[i].Should().Be(v1[i] * i);
            }
        }

        [Fact]
        public void SearchSpan()
        {
            Span<float> span = stackalloc float[32];
            for (var i = 0; i < 32; i++)
                span[i] = -16 + i;
            var resultCount = 0;
            foreach (var item in span.AsReadOnly().Search(2)) {
                item.Should().Be(2);
                ++resultCount;
            }
            resultCount.Should().Be(1);
        }

        [Fact]
        public void GetRankedIndices()
        {
            Span<float> span = stackalloc float[32];
            for (var i = 0; i < 32; i++)
                span[i] = 16 - i;
            var indices = span.AsReadOnly().GetRankedIndices();
            indices.Length.Should().Be(32);
            indices.Should().ContainInConsecutiveOrder(32.AsRange().Select(i => 31 - i));
        }
    }
}
