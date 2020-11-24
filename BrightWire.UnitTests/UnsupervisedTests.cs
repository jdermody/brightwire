using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightData;
using BrightData.UnitTests;
using BrightTable;
using BrightWire.TrainingData.Helper;
using Xunit;

namespace BrightWire.UnitTests
{
    public class UnsupervisedTests : NumericsBase
    {
        [Fact]
        public void TestKMeans()
        {
            var stringTableBuilder = new StringTableBuilder();
            var data = NaiveBayesTests.GetSimpleChineseSet(_context, stringTableBuilder)
                .ConvertToWeightedIndexList(_context, false)
                .Vectorise(_context)
                .ToDictionary(d => _cpu.CreateVector(d.Data), d => d.Classification)
            ;
            var clusters = data
                .Select(d => d.Key)
                .ToList()
                .KMeans(_context, 2)
            ;
            var clusterLabels = clusters
                .Select(d => d.Select(d2 => data[d2]).ToArray())
                .ToList()
            ;
        }

        [Fact]
        public void TestNNMF()
        {
            var stringTableBuilder = new StringTableBuilder();
            var data = NaiveBayesTests.GetSimpleChineseSet(_context, stringTableBuilder)
                .ConvertToWeightedIndexList(_context, false)
                .Vectorise(_context)
                .ToDictionary(d => _cpu.CreateVector(d.Data), d => d.Classification)
            ;
            var clusters = data
                .Select(d => d.Key)
                .ToList()
                .NNMF(_cpu, 2)
            ;
            var clusterLabels = clusters.Select(d => d.Select(d2 => data[d2]).ToArray()).ToList();
        }
    }
}
