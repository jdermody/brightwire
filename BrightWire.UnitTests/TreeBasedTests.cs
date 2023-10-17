using System;
using System.Linq;
using System.Threading.Tasks;
using BrightData;
using BrightData.UnitTests.Helper;
using BrightWire.TrainingData.Helper;
using FluentAssertions;
using Xunit;

namespace BrightWire.UnitTests
{
    public class TreeBasedTests : CpuBase
    {
        [Fact]
        public async Task TestDecisionTree()
        {
            var stringTableBuilder = new StringTableBuilder();
            var data = await NaiveBayesTests.GetSimpleChineseSet(_context, stringTableBuilder)
                .ConvertToWeightedIndexList(false).AsSpan()
                .ConvertToTable(_context)
            ;
            var model = data.TrainDecisionTree();
            var classifier = model.CreateClassifier();
            var testRows = (await data.GetRows()).ToArray();
            classifier.Classify(testRows[0]).GetBestClassification().Should().Be("china");
            classifier.Classify(testRows[1]).GetBestClassification().Should().Be("china");
        }

        [Fact]
        public async Task TestRandomForest()
        {
            var stringTableBuilder = new StringTableBuilder();
            var data = await NaiveBayesTests.GetSimpleChineseSet(_context, stringTableBuilder)
                .ConvertToWeightedIndexList(false).AsSpan()
                .ConvertToTable(_context);
            var model = await data.TrainRandomForest();
            var classifier = model.CreateClassifier();
            var testRows = (await data.GetRows()).ToArray();
            classifier.Classify(testRows[0]).GetBestClassification().Should().Be("china");
        }
    }
}
