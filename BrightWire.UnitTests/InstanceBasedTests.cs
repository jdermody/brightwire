using System;
using System.Collections.Generic;
using System.Text;
using BrightData.UnitTests;
using BrightTable;
using Xunit;

namespace BrightWire.UnitTests
{
    public class InstanceBasedTests : NumericsBase
    {
        [Fact]
        public void KNN()
        {
            var dataTable = _context.BuildTable();
            dataTable.AddColumn(ColumnType.Float, "height");
            dataTable.AddColumn(ColumnType.Int, "weight").IsContinuous = true;
            dataTable.AddColumn(ColumnType.Int, "foot-size").IsContinuous = true;
            dataTable.AddColumn(ColumnType.String, "gender").SetTargetColumn(true);

            // sample data from: https://en.wikipedia.org/wiki/Naive_Bayes_classifier
            dataTable.AddRow(6f, 180, 12, "male");
            dataTable.AddRow(5.92f, 190, 11, "male");
            dataTable.AddRow(5.58f, 170, 12, "male");
            dataTable.AddRow(5.92f, 165, 10, "male");
            dataTable.AddRow(5f, 100, 6, "female");
            dataTable.AddRow(5.5f, 150, 8, "female");
            dataTable.AddRow(5.42f, 130, 7, "female");
            dataTable.AddRow(5.75f, 150, 9, "female");
            var index = dataTable.Build();

            var testData = BrightWireProvider.CreateDataTableBuilder(dataTable.Columns);
            var row = testData.Add(6f, 130, 8, "?");

            var model = index.TrainKNearestNeighbours();
            var classifier = model.CreateClassifier(_lap, 2);
            var classification = classifier.Classify(row);
            Assert.IsTrue(classification.OrderByDescending(c => c.Weight).First().Label == "female");
        }
    }
}
