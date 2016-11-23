using BrightWire;
using BrightWire.Helper;
using BrightWire.Models;
using BrightWire.Models.Input;
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
    public class NaiveBayesTests
    {
        static ILinearAlgebraProvider _lap;

        [ClassInitialize]
        public static void Load(TestContext context)
        {
            _lap = Provider.CreateLinearAlgebra(false);
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            _lap.Dispose();
        }

        [TestMethod]
        public void TestNaiveBayes()
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

            var model = index.TrainNaiveBayes();
            var classifier = model.CreateClassifier();
            var classification = classifier.Classify(row);
            Assert.IsTrue(classification.First() == "female");
        }

        public static ClassificationBag GetSimpleChineseSet(StringTableBuilder stringTableBuilder)
        {
            // sample data from: http://nlp.stanford.edu/IR-book/html/htmledition/naive-bayes-text-classification-1.html
            return new ClassificationBag {
                Classification = new[] {
                    Tuple.Create(new[] { "Chinese", "Beijing", "Chinese" }, true),
                    Tuple.Create(new[] { "Chinese", "Chinese", "Shanghai" }, true),
                    Tuple.Create(new[] { "Chinese", "Macao" }, true),
                    Tuple.Create(new[] { "Tokyo", "Japan", "Chinese" }, false),
                }.Select(d => new IndexedClassification {
                    Name = d.Item2 ? "china" : "japan",
                    Data = d.Item1.Select(s => stringTableBuilder.GetIndex(s)).ToArray()
                }).ToArray()
            };
        }

        public static IReadOnlyList<uint> GetTestRow(StringTableBuilder stringTableBuilder)
        {
            return new[] { "Chinese", "Chinese", "Chinese", "Tokyo", "Japan" }.Select(s => stringTableBuilder.GetIndex(s)).ToArray();
        }

        [TestMethod]
        public void TestMultinomialNaiveBayes()
        {
            var stringTableBuilder = new StringTableBuilder();
            var data = GetSimpleChineseSet(stringTableBuilder);
            var model = data.TrainMultinomialNaiveBayes();
            var classifier = model.CreateClassifier();
            var classification = classifier.Classify(GetTestRow(stringTableBuilder));
            Assert.IsTrue(classification.First() == "china");
        }

        [TestMethod]
        public void TestBernoulliNaiveBayes()
        {
            var stringTableBuilder = new StringTableBuilder();
            var data = GetSimpleChineseSet(stringTableBuilder);
            var model = data.TrainBernoulliNaiveBayes();
            var classifier = model.CreateClassifier();
            var classification = classifier.Classify(GetTestRow(stringTableBuilder));
            Assert.IsTrue(classification.First() == "japan");
        }
    }
}
