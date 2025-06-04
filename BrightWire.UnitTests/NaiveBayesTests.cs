using System.Linq;
using System.Threading.Tasks;
using BrightData;
using BrightData.Types;
using BrightData.UnitTests.Helper;
using BrightWire.TrainingData.Helper;
using FluentAssertions;
using Xunit;

namespace BrightWire.UnitTests
{
    public class NaiveBayesTests : CpuBase
    {
        [Fact]
        public async Task TestNaiveBayes()
        {
            var dataTable = _context.CreateTableBuilder();
            dataTable.CreateColumn(BrightDataType.Float, "height");
            dataTable.CreateColumn(BrightDataType.Int, "weight");
            dataTable.CreateColumn(BrightDataType.Int, "foot-size");
            dataTable.CreateColumn(BrightDataType.String, "gender").MetaData.SetTarget(true);

            // sample data from: https://en.wikipedia.org/wiki/Naive_Bayes_classifier
            dataTable.AddRow(6f, 180, 12, "male");
            dataTable.AddRow(5.92f, 190, 11, "male");
            dataTable.AddRow(5.58f, 170, 12, "male");
            dataTable.AddRow(5.92f, 165, 10, "male");
            dataTable.AddRow(5f, 100, 6, "female");
            dataTable.AddRow(5.5f, 150, 8, "female");
            dataTable.AddRow(5.42f, 130, 7, "female");
            dataTable.AddRow(5.75f, 150, 9, "female");
            var index = await dataTable.BuildInMemory();

            var testData = _context.CreateTableBuilder();
            testData.CreateColumnsFrom(index);
            testData.AddRow(6f, 130, 8, "?");
            var testDataTable = await testData.BuildInMemory();

            var model = await index.TrainNaiveBayes();
            var classifier = model.CreateClassifier();
            var row = await testDataTable[0];
            var classification = classifier.Classify(row);
            classification.First().Label.Should().Be("female");
        }

        public static IndexListWithLabel<string>[] GetSimpleChineseSet(BrightDataContext context, StringTableBuilder stringTableBuilder)
        {
            // sample data from: http://nlp.stanford.edu/IR-book/html/htmledition/naive-bayes-text-classification-1.html
            var data = new[] {
                (["Chinese", "Beijing", "Chinese"], true),
                (["Chinese", "Chinese", "Shanghai"], true),
                (["Chinese", "Macao"], true),
                (new[] { "Tokyo", "Japan", "Chinese" }, false),
            };

            return data.Select(r => new IndexListWithLabel<string>(r.Item2 ? "china" : "japan", IndexList.Create(r.Item1.Select(stringTableBuilder.GetIndex)))).ToArray();
        }

        static readonly string[] _stringTable = ["Chinese", "Chinese", "Chinese", "Tokyo", "Japan" ];
        public static IndexList GetTestRow(BrightDataContext context, StringTableBuilder stringTableBuilder)
        {
            return IndexList.Create(_stringTable.Select(stringTableBuilder.GetIndex));
        }

        [Fact]
        public void TestMultinomialNaiveBayes()
        {
            var stringTableBuilder = new StringTableBuilder();
            var data = GetSimpleChineseSet(_context, stringTableBuilder);
            var model = data.TrainMultinomialNaiveBayes();
            var classifier = model.CreateClassifier();
            var classification = classifier.Classify(GetTestRow(_context, stringTableBuilder));
            classification.MaxBy(c => c.Weight).Label.Should().Be("china");
        }

        [Fact]
        public void TestBernoulliNaiveBayes()
        {
            var stringTableBuilder = new StringTableBuilder();
            var data = GetSimpleChineseSet(_context, stringTableBuilder);
            var model = data.TrainBernoulliNaiveBayes();
            var classifier = model.CreateClassifier();
            var classification = classifier.Classify(GetTestRow(_context, stringTableBuilder));
            classification.MaxBy(c => c.Weight).Label.Should().Be("japan");
        }
    }
}
