using System.Linq;
using BrightData;
using BrightData.UnitTests.Helper;
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
                .ConvertToWeightedIndexList(false)
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
        public void TestNnmf()
        {
            var stringTableBuilder = new StringTableBuilder();
            var data = NaiveBayesTests.GetSimpleChineseSet(_context, stringTableBuilder)
                .ConvertToWeightedIndexList(false)
                .Vectorise(_context)
                .ToDictionary(d => _cpu.CreateVector(d.Data), d => d.Classification)
            ;
            var clusters = data
                .Select(d => d.Key)
                .ToList()
                .Nnmf(_cpu, 2)
            ;
            var clusterLabels = clusters.Select(d => d.Select(d2 => data[d2]).ToArray()).ToList();
        }
    }
}
