using BrightWire;
using BrightWire.Helper;
using BrightWire.Models;
using BrightWire.TabularData;
using BrightWire.TrainingData;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightWire.TrainingData.Helper;

namespace UnitTests
{
    [TestClass]
    public class NaiveBayesTests
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
        public void TestNaiveBayes()
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

            var model = index.TrainNaiveBayes();
            var classifier = model.CreateClassifier();
            var classification = classifier.Classify(row);
            Assert.IsTrue(classification.First().Label == "female");
        }

        public static IReadOnlyList<(string Label, IndexList Data)> GetSimpleChineseSet(StringTableBuilder stringTableBuilder)
        {
            // sample data from: http://nlp.stanford.edu/IR-book/html/htmledition/naive-bayes-text-classification-1.html
            var data = new[] {
                (new[] { "Chinese", "Beijing", "Chinese" }, true),
                (new[] { "Chinese", "Chinese", "Shanghai" }, true),
                (new[] { "Chinese", "Macao" }, true),
                (new[] { "Tokyo", "Japan", "Chinese" }, false),
            };

            return data.Select(r => (r.Item2 ? "china" : "japan", new IndexList {
                Index = r.Item1.Select(s => stringTableBuilder.GetIndex(s)).ToArray()
            })).ToList();
        }

        public static IRow GetTestRow(StringTableBuilder stringTableBuilder)
        {
            var builder = BrightWireProvider.CreateDataTableBuilder();
            builder.AddColumn(ColumnType.IndexList, "String Index");
            return builder.Add(new IndexList {
                Index = new[] { "Chinese", "Chinese", "Chinese", "Tokyo", "Japan" }.Select(s => stringTableBuilder.GetIndex(s)).ToArray()
            });
        }

        [TestMethod]
        public void TestMultinomialNaiveBayes()
        {
            var stringTableBuilder = new StringTableBuilder();
            var data = GetSimpleChineseSet(stringTableBuilder);
            var model = data.TrainMultinomialNaiveBayes();
            var classifier = model.CreateClassifier();
            var classification = classifier.Classify(GetTestRow(stringTableBuilder));
            Assert.IsTrue(classification.OrderByDescending(c => c.Weight).First().Label == "china");
        }

        [TestMethod]
        public void TestBernoulliNaiveBayes()
        {
            var stringTableBuilder = new StringTableBuilder();
            var data = GetSimpleChineseSet(stringTableBuilder);
            var model = data.TrainBernoulliNaiveBayes();
            var classifier = model.CreateClassifier();
            var classification = classifier.Classify(GetTestRow(stringTableBuilder));
            Assert.IsTrue(classification.OrderByDescending(c => c.Weight).First().Label == "japan");
        }
    }
}
