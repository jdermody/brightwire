using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightData;
using BrightData.UnitTests;
using BrightTable;
using FluentAssertions;
using Xunit;

namespace BrightWire.UnitTests
{
    public class LinearTests : NumericsBase
    {
        [Fact]
        public void TestRegression()
        {
            var dataTable = _context.BuildTable();
            dataTable.AddColumn(ColumnType.Float, "value");
            dataTable.AddColumn(ColumnType.Float, "result").SetTargetColumn(true);

            // simple linear relationship: result is twice value
            dataTable.AddRow(1f, 2f);
            dataTable.AddRow(2f, 4f);
            dataTable.AddRow(4f, 8f);
            dataTable.AddRow(8f, 16f);
            var index = dataTable.Build();

            var classifier = index.CreateLinearRegressionTrainer(_cpu);
            //var theta = classifier.Solve();
            //var predictor = theta.CreatePredictor(_lap);

            //var prediction = predictor.Predict(3f);
            //Assert.IsTrue(Math.Round(prediction) == 6f);

            var theta = classifier.GradientDescent(20, 0.01f, 0.1f, cost => true);
            var predictor = theta.CreatePredictor(_cpu);
            var prediction = predictor.Predict(3f);
            Math.Round(prediction).Should().Be(6f);

            var prediction3 = predictor.Predict(new[] {
                new float[] { 10f },
                new float[] { 3f }
            });
            Math.Round(prediction3[1]).Should().Be(6f);
        }

        [Fact]
        public void TestLogisticRegression()
        {
            var dataTable = _context.BuildTable();
            dataTable.AddColumn(ColumnType.Float, "hours");
            dataTable.AddColumn(ColumnType.Boolean, "pass").SetTargetColumn(true);

            // sample data from: https://en.wikipedia.org/wiki/Logistic_regression
            dataTable.AddRow(0.5f, false);
            dataTable.AddRow(0.75f, false);
            dataTable.AddRow(1f, false);
            dataTable.AddRow(1.25f, false);
            dataTable.AddRow(1.5f, false);
            dataTable.AddRow(1.75f, false);
            dataTable.AddRow(1.75f, true);
            dataTable.AddRow(2f, false);
            dataTable.AddRow(2.25f, true);
            dataTable.AddRow(2.5f, false);
            dataTable.AddRow(2.75f, true);
            dataTable.AddRow(3f, false);
            dataTable.AddRow(3.25f, true);
            dataTable.AddRow(3.5f, false);
            dataTable.AddRow(4f, true);
            dataTable.AddRow(4.25f, true);
            dataTable.AddRow(4.5f, true);
            dataTable.AddRow(4.75f, true);
            dataTable.AddRow(5f, true);
            dataTable.AddRow(5.5f, true);
            var index = dataTable.Build();

            var trainer = index.CreateLogisticRegressionTrainer();
            var theta = trainer.GradientDescent(1000, 0.1f, 0.1f, cost => true);
            var predictor = theta.CreateClassifier(_cpu);
            var probability1 = predictor.Predict(_context.CreateMatrix(1, 1, 2f));
            probability1[0].Should().BeLessThan(0.5f);

            var probability2 = predictor.Predict(_context.CreateMatrix(1, 1, 4f));
            probability2[0].Should().BeGreaterOrEqualTo(0.5f);

            var probability3 = predictor.Predict(_context.CreateMatrixFromRows(new[] {
                _context.CreateVector(new[] { 1f }),
                _context.CreateVector(new[] { 2f }),
                _context.CreateVector(new[] { 3f }),
                _context.CreateVector(new[] { 4f }),
                _context.CreateVector(new[] { 5f })
            }));
            probability3[0].Should().BeLessOrEqualTo(0.5f);
            probability3[1].Should().BeGreaterOrEqualTo(0.5f);
            probability3[2].Should().BeGreaterOrEqualTo(0.5f);
            probability3[3].Should().BeGreaterOrEqualTo(0.5f);
            probability3[4].Should().BeGreaterOrEqualTo(0.5f);

            //var rowClassifier = predictor.ConvertToRowClassifier(new[] { 0 });
            //var rowClassifications = rowClassifier.Classifiy(index);
            //Assert.IsTrue(rowClassifications[0].Classification == "0");
            //Assert.IsTrue(rowClassifications[rowClassifications.Count - 1].Classification == "1");
        }

        [Fact]
        public void TestMultinomialLogisticRegression()
        {
            var dataTable = _context.BuildTable();
            dataTable.AddColumn(ColumnType.Float, "height");
            dataTable.AddColumn(ColumnType.Int, "weight");
            dataTable.AddColumn(ColumnType.Int, "foot-size");
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

            var testData = _context.BuildTable();
            testData.CopyColumnsFrom(index);
            testData.AddRow(6f, 130, 8, "?");
            var testDataTable = testData.Build();

            var model = index.TrainMultinomialLogisticRegression(100, 0.1f);
            var classifier = model.CreateClassifier(_context.LinearAlgebraProvider);
            var classification = classifier.Classify(testDataTable).Single();
            classification.Predictions.GetBestClassification().Should().Be("female");
        }
    }
}
