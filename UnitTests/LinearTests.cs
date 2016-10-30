using BrightWire;
using BrightWire.Linear;
using BrightWire.Linear.Training;
using BrightWire.TabularData;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
    [TestClass]
    public class LinearTests
    {
        static ILinearAlgebraProvider _lap;

        [ClassInitialize]
        public static void Load(TestContext context)
        {
            _lap = Provider.CreateCPULinearAlgebra(false);
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            _lap.Dispose();
        }

        [TestMethod]
        public void TestRegression()
        {
            var dataTable = new DataTableBuilder();
            dataTable.AddColumn(ColumnType.Float, "value");
            dataTable.AddColumn(ColumnType.Float, "result", true);

            // simple linear relationship: result is twice value
            dataTable.AddRow(new object[] { 1f, 2f });
            dataTable.AddRow(new object[] { 2f, 4f });
            dataTable.AddRow(new object[] { 4f, 8f });
            dataTable.AddRow(new object[] { 8f, 16f });
            var index = dataTable.Build();

            var classifier = index.CreateLinearRegressionTrainer(_lap);
            //var theta = classifier.Solve();
            //var predictor = theta.CreatePredictor(_lap);

            //var prediction = predictor.Predict(3f);
            //Assert.IsTrue(Math.Round(prediction) == 6f);

            var theta = classifier.GradientDescent(20, 0.01f);
            var predictor = theta.CreatePredictor(_lap);
            var prediction = predictor.Predict(3f);
            Assert.IsTrue(Math.Round(prediction) == 6f);

            var prediction3 = predictor.Predict(new[] {
                new float[] { 10f },
                new float[] { 3f }
            });
            Assert.IsTrue(Math.Round(prediction3[1]) == 6f);
        }

        [TestMethod]
        public void TestLogisticRegression()
        {
            var dataTable = new DataTableBuilder();
            dataTable.AddColumn(ColumnType.Float, "hours");
            dataTable.AddColumn(ColumnType.Boolean, "pass", true);

            // sample data from: https://en.wikipedia.org/wiki/Logistic_regression
            dataTable.AddRow(new object[] { 0.5f, false });
            dataTable.AddRow(new object[] { 0.75f, false });
            dataTable.AddRow(new object[] { 1f, false });
            dataTable.AddRow(new object[] { 1.25f, false });
            dataTable.AddRow(new object[] { 1.5f, false });
            dataTable.AddRow(new object[] { 1.75f, false });
            dataTable.AddRow(new object[] { 1.75f, true });
            dataTable.AddRow(new object[] { 2f, false });
            dataTable.AddRow(new object[] { 2.25f, true });
            dataTable.AddRow(new object[] { 2.5f, false });
            dataTable.AddRow(new object[] { 2.75f, true });
            dataTable.AddRow(new object[] { 3f, false });
            dataTable.AddRow(new object[] { 3.25f, true });
            dataTable.AddRow(new object[] { 3.5f, false });
            dataTable.AddRow(new object[] { 4f, true });
            dataTable.AddRow(new object[] { 4.25f, true });
            dataTable.AddRow(new object[] { 4.5f, true });
            dataTable.AddRow(new object[] { 4.75f, true });
            dataTable.AddRow(new object[] { 5f, true });
            dataTable.AddRow(new object[] { 5.5f, true });
            var index = dataTable.Build();

            var trainer = index.CreateLogisticRegressionTrainer(_lap);
            var theta = trainer.GradientDescent(1000, 0.1f);
            var predictor = theta.CreatePredictor(_lap);
            var probability1 = predictor.Predict(2f);
            Assert.IsTrue(probability1 < 0.5f);

            var probability2 = predictor.Predict(4f);
            Assert.IsTrue(probability2 >= 0.5f);

            var probability3 = predictor.Predict(new[] {
                new float[] { 1f },
                new float[] { 2f },
                new float[] { 3f },
                new float[] { 4f },
                new float[] { 5f },
            });
            Assert.IsTrue(probability3[0] <= 0.5f);
            Assert.IsTrue(probability3[1] <= 0.5f);
            Assert.IsTrue(probability3[2] >= 0.5f);
            Assert.IsTrue(probability3[3] >= 0.5f);
            Assert.IsTrue(probability3[4] >= 0.5f);
        }

        [TestMethod]
        public void TestMultinomialLogisticRegression()
        {
            var dataTable = new DataTableBuilder();
            dataTable.AddColumn(ColumnType.Float, "height");
            dataTable.AddColumn(ColumnType.Int, "weight").IsContinuous = true;
            dataTable.AddColumn(ColumnType.Int, "foot-size").IsContinuous = true;
            dataTable.AddColumn(ColumnType.String, "gender", true);

            // sample data from: https://en.wikipedia.org/wiki/Naive_Bayes_classifier
            dataTable.Add(6f, 180, 12, "male");
            dataTable.Add(5.92f, 190, 11, "male");
            dataTable.Add(5.58f, 170, 12, "male");
            dataTable.Add(5.92f, 165, 10, "male");
            dataTable.Add(5f, 100, 6, "female");
            dataTable.Add(5.5f, 150, 8, "female");
            dataTable.Add(5.42f, 130, 7, "female");
            dataTable.Add(5.75f, 150, 9, "female");
            var index = dataTable.Build();

            var testData = new DataTableBuilder(dataTable.Columns);
            var row = testData.Add(6f, 130, 8, "?");

            var model = index.TrainMultinomialLogisticRegression(_lap, 100, 0.1f);
            var classifier = model.CreateClassifier(_lap);
            var classification = classifier.Classify(row);
            Assert.IsTrue(classification.First() == "female");
        }
    }
}
