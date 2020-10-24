using System;
using System.Collections.Generic;
using System.Text;
using BrightData.UnitTests;
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
            var data = NaiveBayesTests.GetSimpleChineseSet(stringTableBuilder)
                    .ConvertToWeightedIndexList(false)
                    .Vectorise()
                    .ToDictionary(d => _cpu.CreateVector(d.Data), d => d.Classification)
                ;
            var clusters = data
                    .Select(d => d.Key)
                    .ToList()
                    .KMeans(_cpu, 2)
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
            var data = NaiveBayesTests.GetSimpleChineseSet(stringTableBuilder)
                    .ConvertToWeightedIndexList(false)
                    .Vectorise()
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
