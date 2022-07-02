using System;
using System.Globalization;
using FluentAssertions;
using Xunit;

namespace BrightData.UnitTests
{
    public class AnalysisTests
    {
        [Fact]
        public void DateAnalysis()
        {
            var d1 = new DateTime(2020, 1, 1);
            var d2 = new DateTime(2020, 2, 1);
            var analysis = new[] { d1, d2, d2}.Analyze();

            analysis.MinDate.Should().Be(d1);
            analysis.MaxDate.Should().Be(d2);
            analysis.MostFrequent.Should().Be(d2.ToString(CultureInfo.InvariantCulture));
            analysis.NumDistinct.Should().Be(2);
            analysis.Total.Should().Be(3);
        }

        [Fact]
        public void DateAnalysisNoMostFrequent()
        {
            var d1 = new DateTime(2020, 1, 1);
            var d2 = new DateTime(2020, 2, 1);
            var analysis = new[] { d1, d2 }.Analyze();

            analysis.MinDate.Should().Be(d1);
            analysis.MaxDate.Should().Be(d2);
            analysis.MostFrequent.Should().BeNull();
            analysis.NumDistinct.Should().Be(2);
            analysis.Total.Should().Be(2);
        }

        [Fact]
        public void IntegerAnalysis()
        {
            var analysis = new[] {1, 2, 3}.Analyze();
            analysis.Min.Should().Be(1);
            analysis.Max.Should().Be(3);
            analysis.Median.Should().Be(2);
            analysis.NumDistinct.Should().Be(3);
            analysis.Total.Should().Be(3);
            analysis.SampleStdDev.Should().Be(1);
        }

        [Fact]
        public void IntegerAnalysis2()
        {
            var analysis = new[] { 1, 2, 2, 3 }.Analyze();
            analysis.Min.Should().Be(1);
            analysis.Max.Should().Be(3);
            analysis.Median.Should().Be(2);
            analysis.NumDistinct.Should().Be(3);
            analysis.Mode.Should().Be(2);
            analysis.Total.Should().Be(4);
        }

        [Fact]
        public void StringAnalysis()
        {
            var analysis = new[] {"a", "ab", "abc"}.Analyze();
            analysis.MinLength.Should().Be(1);
            analysis.MaxLength.Should().Be(3);
            analysis.NumDistinct.Should().Be(3);
            analysis.Total.Should().Be(3);
        }

        [Fact]
        public void StringAnalysis2()
        {
            var analysis = new[] { "a", "ab", "ab", "abc" }.Analyze();
            analysis.MinLength.Should().Be(1);
            analysis.MaxLength.Should().Be(3);
            analysis.NumDistinct.Should().Be(3);
            analysis.MostFrequent.Should().Be("ab");
            analysis.Total.Should().Be(4);
        }

        [Fact]
        public void IndexAnalysis()
        {
            using var context = new BrightDataContext();
            var analysis = new IHaveIndices [] {
                context.CreateIndexList(1, 2, 3),
                context.CreateIndexList(4, 5, 6),
            }.Analyze();
            analysis.MinIndex.Should().Be(1);
            analysis.MaxIndex.Should().Be(6);
            analysis.NumDistinct.Should().Be(6);
        }

        [Fact]
        public void IndexAnalysis2()
        {
            using var context = new BrightDataContext();
            var analysis = new IHaveIndices[] {
                context.CreateIndexList(1, 2, 3),
                context.CreateWeightedIndexList((4, 1f), (5, 2f), (6, 3f)),
            }.Analyze();
            analysis.MinIndex.Should().Be(1);
            analysis.MaxIndex.Should().Be(6);
            analysis.NumDistinct.Should().Be(6);
        }
    }
}
