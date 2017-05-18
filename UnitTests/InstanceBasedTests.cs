using BrightWire;
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
    public class InstanceBasedTests
    {
        static ILinearAlgebraProvider _lap;

        [ClassInitialize]
        public static void Load(TestContext context)
        {
            _lap = BrightWireProvider.CreateLinearAlgebra(false);
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            _lap.Dispose();
        }

        [TestMethod]
        public void KNN()
        {
            var dataTable = BrightWireProvider.CreateDataTableBuilder();
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

            var testData = BrightWireProvider.CreateDataTableBuilder(dataTable.Columns);
            var row = testData.Add(6f, 130, 8, "?");

            var model = index.TrainKNearestNeighbours();
            var classifier = model.CreateClassifier(_lap, 2);
            var classification = classifier.Classify(row);
            Assert.IsTrue(classification.OrderByDescending(c => c.Weight).First().Label == "female");
        }
    }
}
