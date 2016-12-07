using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using BrightWire.Models.Output;

namespace BrightWire.Ensemble.Training
{
    class AdaBoostTrainer : IBoostedTrainer
    {
        readonly IDataTable _table;
        readonly int _classColumnIndex;
        readonly AdaBoostBuilder _modelBuilder;
        readonly List<IRowClassifier> _classifier = new List<IRowClassifier>();
        readonly List<string> _classificationList = new List<string>();

        public AdaBoostTrainer(IDataTable table)
        {
            _table = table;
            _classColumnIndex = table.TargetColumnIndex;
            _modelBuilder = new AdaBoostBuilder(table.RowCount);
        }

        public void AddClassifier(Func<IDataTable, IRowClassifier> classifierProvider)
        {
            var samples = _modelBuilder.GetNextSamples();
            var iterationTable = _table.CopyWithRows(samples);
            var classifier = classifierProvider(iterationTable);
            _classifier.Add(classifier);
            var correct = _table
                .Classify(classifier)
                .Select(r => r.Classification == r.Row.GetField<string>(_classColumnIndex))
                .ToList()
            ;
            _modelBuilder.AddClassifierResults(correct);
        }

        public void AddClassifiers(int count, Func<IDataTable, IRowClassifier> classifierProvider)
        {
            for (var i = 0; i < count; i++)
                AddClassifier(classifierProvider);
        }

        public IReadOnlyList<RowClassification> Classify(IDataTable testData)
        {
            var classifierWeight = _modelBuilder.ClassifierWeight;
            var trainingRows = testData.GetRows(Enumerable.Range(0, testData.RowCount));
            var ret = new List<RowClassification>();

            for (var i = 0; i < trainingRows.Count; i++) {
                var row = trainingRows[i];
                var results = _classifier
                    .SelectMany((c, j) => c.GetWeightedClassifications(row)
                        .Select(wc => WeightedClassification.Create(wc.Classification, wc.Weight * classifierWeight[j]))
                    )
                    .GroupBy(wc => wc.Classification)
                    .Select(g => Tuple.Create(g.Key, g.Sum(wc => wc.Weight)))
                    .OrderByDescending(d => d.Item2)
                    .ToList()
                ;
                var bestClassification = results.First();
                ret.Add(new RowClassification(row, bestClassification.Item1));
            }
            return ret;
        }
    }
}
