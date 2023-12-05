using System;
using System.Linq;
using BrightData;
using BrightData.UnitTests.Helper;
using BrightWire.TrainingData.Helper;
using FluentAssertions;
using Xunit;

namespace BrightWire.UnitTests
{
    public class UnsupervisedTests : CpuBase
    {
        [Fact]
        public void TestKMeans()
        {
            var stringTableBuilder = new StringTableBuilder();
            var data = NaiveBayesTests.GetSimpleChineseSet(_context, stringTableBuilder)
                .ConvertToWeightedIndexList(false).AsSpan()
                .Vectorise(_context)
            ;
            var clusters = data
                .Select(d => d.Data)
                .ToArray()
                .KMeansCluster(_context, 2)
            ;
            var clusterLabels = clusters
                .Select(d => d.Select(d2 => data[d2].Classification).ToArray())
                .ToArray()
            ;
            clusterLabels.First().Length.Should().Be(3);
            clusterLabels.Last().Length.Should().Be(1);
        }

        [Fact]
        public void TestNnmf()
        {
            var lap = _context.LinearAlgebraProvider;
            var stringTableBuilder = new StringTableBuilder();
            var data = NaiveBayesTests.GetSimpleChineseSet(_context, stringTableBuilder)
                    .ConvertToWeightedIndexList(false).AsSpan()
                    .Vectorise(_context)
                ;
            var clusters = data
                .Select(d => d.Data)
                .ToArray()
                .Nnmf(lap, 2)
            ;
            var clusterLabels = clusters.Select(d => d.Select(d2 => data[d2].Classification).ToArray()).ToList();
            clusterLabels.First().Length.Should().Be(3);
            clusterLabels.Last().Length.Should().Be(1);
        }
    }
}
