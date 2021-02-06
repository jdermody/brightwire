﻿using System;
using System.Collections.Generic;
using System.Linq;
using BrightData;
using BrightWire;
using BrightWire.Models.Bayesian;
using BrightWire.Models.InstanceBased;
using BrightWire.Models.TreeBased;

namespace ExampleCode.DataTableTrainers
{
    internal class DataTableTrainer : IDisposable
    {
        public DataTableTrainer(IRowOrientedDataTable table)
        {
            TargetColumn = table.GetTargetColumnOrThrow();
            Table = table;
            var (training, test) = table.Split();
            Training = training;
            Test = test;
        }

        public DataTableTrainer(IRowOrientedDataTable? table, IRowOrientedDataTable training, IRowOrientedDataTable test)
        {
            TargetColumn = training.GetTargetColumnOrThrow();
            Training = training;
            Test = test;
            Table = table ?? training.Concat(test);
        }

        public void Dispose()
        {
            Table.Dispose();
            Training.Dispose();
            Test.Dispose();
        }

        public uint TargetColumn { get; }
        public IRowOrientedDataTable Table { get; }
        public IRowOrientedDataTable Training { get; }
        public IRowOrientedDataTable Test { get; }

        public IEnumerable<string> KMeans(uint k) => AggregateLabels(Table.KMeans(k));
        public IEnumerable<string> HierarchicalCluster(uint k) => AggregateLabels(Table.HierarchicalCluster(k));
        public IEnumerable<string> NonNegativeMatrixFactorisation(uint k) => AggregateLabels(Table.NonNegativeMatrixFactorisation(k));

        IEnumerable<string> AggregateLabels(IEnumerable<(uint RowIndex, string? Label)[]> clusters) => clusters
            .Select(c => String.Join(';', c
                .Select(r => r.Label)
                .GroupBy(d => d)
                .Select(g => (Label: g.Key, Count: g.Count()))
                .OrderByDescending(g => g.Count)
                .ThenBy(g => g.Label)
                .Select(g => $"{g.Label ?? "<<NULL>>"} ({g.Count})")
            ));

        public NaiveBayes TrainNaiveBayes(bool writeResults = true)
        {
            var ret = Training.TrainNaiveBayes();
            if (writeResults)
                WriteResults("Naive bayes", ret.CreateClassifier());
            return ret;
        }

        public DecisionTree TrainDecisionTree(bool writeResults = true)
        {
            var ret = Training.TrainDecisionTree();
            if (writeResults)
                WriteResults("Decision tree", ret.CreateClassifier());
            return ret;
        }

        public RandomForest TrainRandomForest(uint numTrees, uint? baggedRowCount = null, bool writeResults = true)
        {
            var ret = Training.TrainRandomForest(numTrees, baggedRowCount);
            if(writeResults)
                WriteResults("Random forest", ret.CreateClassifier());
            return ret;
        }

        public KNearestNeighbours TrainKNearestNeighbours(uint k, bool writeResults = true)
        {
            var ret = Training.TrainKNearestNeighbours();
            if (writeResults)
                WriteResults("K nearest neighbours", ret.CreateClassifier(Table.Context.LinearAlgebraProvider, k));
            return ret;
        }

        public void TrainMultinomialLogisticRegression(uint iterations, float trainingRate, float lambda, bool writeResults = true)
        {
            var ret = Training.TrainMultinomialLogisticRegression(iterations, trainingRate, lambda);
            if (writeResults)
                WriteResults("Multinomial logistic regression", ret.CreateClassifier(Table.Context.LinearAlgebraProvider));
        }

        void WriteResults(string type, IRowClassifier classifier)
        {
            var results = Test.Classify(classifier).ToList();
            var score = results
                .Average(d => d.Row.GetTyped<string>(TargetColumn) == d.Classification.OrderByDescending(c => c.Weight).First().Label ? 1.0 : 0.0);

            Console.WriteLine($"{type} accuracy: {score:P}");
        }

        void WriteResults(string type, ITableClassifier classifier)
        {
            var results = classifier.Classify(Test).ToList();
            var expectedLabels = Test.Column(TargetColumn).Enumerate().Select(o => o.ToString()).ToArray();
            var score = results
                .Average(d => expectedLabels[d.RowIndex] == d.Predictions.OrderByDescending(c => c.Weight).First().Classification ? 1.0 : 0.0);

            Console.WriteLine($"{type} accuracy: {score:P}");
        }

        //void _WriteClusters(IFloatVector[][] clusters, Dictionary<IVector, string> labelTable)
        //{
        //    foreach (var cluster in clusters) {
        //        foreach (var item in cluster)
        //            Console.WriteLine(labelTable[item]);
        //        Console.WriteLine("---------------------------------------------");
        //    }
        //}
    }
}
