using BrightWire.Connectionist;
using BrightWire.DimensionalityReduction;
using BrightWire.Ensemble;
using BrightWire.Helper;
using BrightWire.Models.Output;
using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.SampleCode
{
    partial class Program
    {
        //static IDataTable LoadAdult(string path, bool skipFirstLine)
        //{
        //    using(var streamReader = new StreamReader(path)) {
        //        if (skipFirstLine)
        //            streamReader.ReadLine();
        //        return streamReader.ParseCSV(',', false);
        //    }
        //}

        //static void Adult()
        //{
        //    var trainingTable = LoadAdult(@"d:\data\adult.data", false);
        //    var testTable = LoadAdult(@"d:\data\adult.test", true);
        //    var targetColumnIndex = testTable.TargetColumnIndex;
        //    var testTable2 = testTable.Project(row => {
        //        var label = row.GetField<string>(targetColumnIndex);
        //        var ret = row.Data.Take(targetColumnIndex).Concat(new[] { label.Substring(0, label.Length - 1) }).ToList();
        //        return ret;
        //    });

        //    var adaBoost = new AdaBoost(trainingTable.RowCount);
        //    var classifierList = new List<IRowClassifier>();

        //    for (var i = 0; i < 10; i++) {
        //        var samples = adaBoost.GetNextSamples();
        //        var iterationTable = trainingTable.CopyWithRows(samples);
        //        var decisionTree = iterationTable.TrainDecisionTree(null, 1);
        //        var classifier = decisionTree.CreateClassifier();
        //        classifierList.Add(classifier);
        //        var correct = trainingTable
        //            .Classify(classifier)
        //            .Select(r => r.Classification == r.Row.GetField<string>(targetColumnIndex))
        //            .ToList()
        //        ;
        //        adaBoost.AddClassifierResults(correct);
        //        Console.WriteLine("Classifier accuracy: {0:P}", correct
        //            .Select(r => r ? 1f : 0f)
        //            .Average()
        //        );
        //    }

        //    var classifierWeight = adaBoost.ClassifierWeight;
        //    var trainingRows = testTable2.GetRows(Enumerable.Range(0, testTable2.RowCount));
        //    int finalCorrect = 0, finalTotal = 0;
        //    for (var i = 0; i < trainingRows.Count; i++) {
        //        var row = trainingRows[i];
        //        var rowLabel = row.GetField<string>(targetColumnIndex);
        //        var results = classifierList
        //            .SelectMany((c, j) => c.GetWeightedClassifications(row)
        //                .Select(wc => new WeightedClassification(wc.Classification, wc.Weight * classifierWeight[j]))
        //            )
        //            .GroupBy(wc => wc.Classification)
        //            .Select(g => Tuple.Create(g.Key, g.Sum(wc => wc.Weight)))
        //            .OrderByDescending(d => d.Item2)
        //            .ToList()
        //        ;
        //        var bestClassification = results.First();
        //        if (bestClassification.Item1 == rowLabel)
        //            ++finalCorrect;
        //        ++finalTotal;
        //    }
        //    Console.WriteLine("Final accuracy: {0:P}", (float)finalCorrect / finalTotal);

            
        //}

        static void Main(string[] args)
        {
            //TestWord2Vec();
            //TestGlove();
            //TestCSV();

            //IrisClassification();
            //IrisClustering();
            //MarkovChains();
            //MNIST(@"D:\data\mnist\", @"D:\data\mnist\model_test.dat");
            //SentimentClassification(@"D:\data\sentiment labelled sentences\");
            //TextClustering(@"D:\data\[UCI] AAAI-14 Accepted Papers - Papers.csv", @"d:\temp\");
            //IntegerAddition();
            //Adult();
        }
    }
}
