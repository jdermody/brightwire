using System.Linq;
using BrightData;
using BrightData.UnitTests.Helper;
using FluentAssertions;
using Xunit;

namespace BrightWire.UnitTests
{
    public class InstanceBasedTests : NumericsBase
    {
        [Fact]
        public void Knn()
        {
            var dataTable = _context.BuildTable();
            dataTable.AddColumn(BrightDataType.Float, "height");
            dataTable.AddColumn(BrightDataType.Int, "weight");
            dataTable.AddColumn(BrightDataType.Int, "foot-size");
            dataTable.AddColumn(BrightDataType.String, "gender").MetaData.SetTarget(true);

            // sample data from: https://en.wikipedia.org/wiki/Naive_Bayes_classifier
            dataTable.AddRow(6f, 180, 12, "male");
            dataTable.AddRow(5.92f, 190, 11, "male");
            dataTable.AddRow(5.58f, 170, 12, "male");
            dataTable.AddRow(5.92f, 165, 10, "male");
            dataTable.AddRow(5f, 100, 6, "female");
            dataTable.AddRow(5.5f, 150, 8, "female");
            dataTable.AddRow(5.42f, 130, 7, "female");
            dataTable.AddRow(5.75f, 150, 9, "female");
            var index = dataTable.BuildInMemory();

            var testData = _context.BuildTable();
            testData.CopyColumnsFrom(index);
            testData.AddRow(6f, 130, 8, "?");
            var testDataTable = testData.BuildInMemory();

            var model = index.TrainKNearestNeighbours();
            var classifier = model.CreateClassifier(_context.LinearAlgebraProvider2, 2);
            using var row = testDataTable.GetRow(0);
            var classification = classifier.Classify(row);
            classification.MaxBy(c => c.Weight).Label.Should().Be("female");
        }
    }
}
