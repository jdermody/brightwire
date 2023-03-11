using BrightData.UnitTests.Helper;

namespace BrightWire.UnitTests
{
    public class LinearTests : CpuBase
    {
        //[Fact]
        //public void TestRegression()
        //{
        //    var dataTable = _context.BuildTable();
        //    dataTable.AddColumn(BrightDataType.Float, "value");
        //    dataTable.AddColumn(BrightDataType.Float, "result").SetTarget(true);

        //    // simple linear relationship: result is twice value
        //    dataTable.AddRow(1f, 2f);
        //    dataTable.AddRow(2f, 4f);
        //    dataTable.AddRow(4f, 8f);
        //    dataTable.AddRow(8f, 16f);
        //    var index = dataTable.BuildRowOriented();

        //    var classifier = index.CreateLinearRegressionTrainer(_context.LinearAlgebraProvider2);

        //    var theta = classifier.GradientDescent(20, 0.01f, 0.1f, _ => true);
        //    var predictor = theta.CreatePredictor(_context.LinearAlgebraProvider2);
        //    var prediction = predictor.Predict(3f);
        //    Math.Round(prediction).Should().Be(6f);

        //    var prediction3 = predictor.Predict(new[] {
        //        new[] { 10f },
        //        new[] { 3f }
        //    });
        //    Math.Round(prediction3[1]).Should().Be(6f);
        //}

        //[Fact]
        //public void TestLogisticRegression()
        //{
        //    var dataTable = _context.BuildTable();
        //    dataTable.AddColumn(BrightDataType.Float, "hours");
        //    dataTable.AddColumn(BrightDataType.Boolean, "pass").SetTarget(true);

        //    // sample data from: https://en.wikipedia.org/wiki/Logistic_regression
        //    dataTable.AddRow(0.5f, false);
        //    dataTable.AddRow(0.75f, false);
        //    dataTable.AddRow(1f, false);
        //    dataTable.AddRow(1.25f, false);
        //    dataTable.AddRow(1.5f, false);
        //    dataTable.AddRow(1.75f, false);
        //    dataTable.AddRow(1.75f, true);
        //    dataTable.AddRow(2f, false);
        //    dataTable.AddRow(2.25f, true);
        //    dataTable.AddRow(2.5f, false);
        //    dataTable.AddRow(2.75f, true);
        //    dataTable.AddRow(3f, false);
        //    dataTable.AddRow(3.25f, true);
        //    dataTable.AddRow(3.5f, false);
        //    dataTable.AddRow(4f, true);
        //    dataTable.AddRow(4.25f, true);
        //    dataTable.AddRow(4.5f, true);
        //    dataTable.AddRow(4.75f, true);
        //    dataTable.AddRow(5f, true);
        //    dataTable.AddRow(5.5f, true);
        //    var index = dataTable.BuildRowOriented();

        //    var lap = _context.LinearAlgebraProvider2;
        //    var trainer = index.CreateLogisticRegressionTrainer();
        //    var theta = trainer.GradientDescent(1000, 0.1f, 0.1f, _ => true);
        //    var predictor = theta.CreateClassifier(lap);
        //    var probability1 = predictor.Predict(lap.CreateMatrix(1, 1, (i, j) => 2f));
        //    probability1[0].Should().BeLessThan(0.5f);

        //    var probability2 = predictor.Predict(lap.CreateMatrix(1, 1, (i, j) => 4f));
        //    probability2[0].Should().BeGreaterOrEqualTo(0.5f);

        //    var probability3 = predictor.Predict(lap.CreateMatrixFromRows(lap.CreateVector(1, 1f), lap.CreateVector(1, 2f), lap.CreateVector(1, 3f), lap.CreateVector(1, 4f), lap.CreateVector(1, 5f)));
        //    probability3[0].Should().BeLessOrEqualTo(0.5f);
        //    probability3[1].Should().BeLessOrEqualTo(0.5f);
        //    probability3[2].Should().BeGreaterOrEqualTo(0.5f);
        //    probability3[3].Should().BeGreaterOrEqualTo(0.5f);
        //    probability3[4].Should().BeGreaterOrEqualTo(0.5f);
        //}

        //[Fact]
        //public void TestMultinomialLogisticRegression()
        //{
        //    var dataTable = _context.BuildTable();
        //    dataTable.AddColumn(BrightDataType.Float, "height");
        //    dataTable.AddColumn(BrightDataType.Int, "weight");
        //    dataTable.AddColumn(BrightDataType.Int, "foot-size");
        //    dataTable.AddColumn(BrightDataType.String, "gender").SetTarget(true);

        //    // sample data from: https://en.wikipedia.org/wiki/Naive_Bayes_classifier
        //    dataTable.AddRow(6f, 180, 12, "male");
        //    dataTable.AddRow(5.92f, 190, 11, "male");
        //    dataTable.AddRow(5.58f, 170, 12, "male");
        //    dataTable.AddRow(5.92f, 165, 10, "male");
        //    dataTable.AddRow(5f, 100, 6, "female");
        //    dataTable.AddRow(5.5f, 150, 8, "female");
        //    dataTable.AddRow(5.42f, 130, 7, "female");
        //    dataTable.AddRow(5.75f, 150, 9, "female");
        //    var index = dataTable.BuildRowOriented();

        //    var testData = _context.BuildTable();
        //    testData.CopyColumnsFrom(index);
        //    testData.AddRow(6f, 130, 8, "?");
        //    var testDataTable = testData.BuildRowOriented();

        //    var model = index.TrainMultinomialLogisticRegression(100, 0.1f);
        //    var classifier = model.CreateClassifier(_context.LinearAlgebraProvider2);
        //    var (_, predictions) = classifier.Classify(testDataTable).Single();
        //    predictions.GetBestClassification().Should().Be("female");
        //}
    }
}
