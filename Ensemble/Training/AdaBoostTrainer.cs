using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using BrightWire.Models.Output;
using MathNet.Numerics.Distributions;
using BrightWire.Helper;

namespace BrightWire.Ensemble.Training
{
    class AdaBoostTrainer : IBoostedTrainer
    {
        readonly IDataTable _table;
        readonly int _classColumnIndex;
        readonly float[] _rowWeight;
        readonly List<float> _classifierWeight = new List<float>();
        readonly List<IRowClassifier> _classifier = new List<IRowClassifier>();
        readonly List<string> _classificationList = new List<string>();

        public AdaBoostTrainer(IDataTable table)
        {
            _table = table;
            _classColumnIndex = table.TargetColumnIndex;
            _classificationList = table.GetAnalysis()[_classColumnIndex].DistinctValues.Select(v => v.ToString()).ToList();

            var rowCount = table.RowCount;
            var weight = 1f / rowCount;
            _rowWeight = new float[rowCount];
            for (var i = 0; i < rowCount; i++)
                _rowWeight[i] = weight;
        }

        public void AddClassifier(Func<IDataTable, IRowClassifier> classifierProvider)
        {
            var samples = _GetNextSamples();
            var iterationTable = _table.CopyWithRows(samples);
            var classifier = classifierProvider(iterationTable);
            _classifier.Add(classifier);
            var correct = _table
                .Classify(classifier)
                .Select(r => r.Classification == r.Row.GetField<string>(_classColumnIndex))
                .ToList()
            ;
            _AddClassifierResults(correct);
        }

        public void AddClassifiers(int count, Func<IDataTable, IRowClassifier> classifierProvider)
        {
            for (var i = 0; i < count; i++)
                AddClassifier(classifierProvider);
        }

        string _Classify(IRow row)
        {
            float temp;
            var scoreTable = new Dictionary<string, float>();
            for(int i = 0, len = _classifier.Count; i < len; i++) {
                var classifier = _classifier[i];
                var classification = classifier.Classify(row).First();
                var weight = _classifierWeight[i];
                if (scoreTable.TryGetValue(classification, out temp))
                    scoreTable[classification] = temp + weight;
                else
                    scoreTable.Add(classification, weight);
            }
            return scoreTable.OrderByDescending(kv => kv.Value).Select(kv => kv.Key).First();
        }

        public IReadOnlyList<RowClassification> Classify(IDataTable testData)
        {
            var trainingRows = testData.GetRows(Enumerable.Range(0, testData.RowCount));
            var ret = new List<RowClassification>();

            for (var i = 0; i < trainingRows.Count; i++) {
                var row = trainingRows[i];
                var classification = _Classify(row);
                ret.Add(new RowClassification(row, classification));
            }
            return ret;
        }

        float _AddClassifierResults(IReadOnlyList<bool> rowClassificationWasCorrect)
        {
            int mistakes = 0, total = 0;
            foreach (var wasCorrect in rowClassificationWasCorrect) {
                if (!wasCorrect)
                    ++mistakes;
                ++total;
            }
            if (total != _rowWeight.Length)
                throw new ArgumentException();

            var weightedError = (float)mistakes / total;
            Console.WriteLine("Classifier error: " + weightedError);
            var classifierWeight = BoundMath.Log((1 - weightedError) / weightedError) * 0.5f;

            float weightTotal = 0;
            for (var i = 0; i < _rowWeight.Length; i++) {
                var wasCorrect = rowClassificationWasCorrect[i];
                float delta;
                if (wasCorrect)
                    delta = BoundMath.Exp(-classifierWeight);
                else
                    delta = BoundMath.Exp(classifierWeight);

                weightTotal += (_rowWeight[i] = _rowWeight[i] * delta);
            }
            for (var i = 0; i < _rowWeight.Length; i++)
                _rowWeight[i] /= weightTotal;

            _classifierWeight.Add(classifierWeight);
            return classifierWeight;
        }

        IReadOnlyList<int> _GetNextSamples()
        {
            var distribution = new Categorical(_rowWeight.Select(d => Convert.ToDouble(d)).ToArray());
            var ret = new List<int>();
            for (int i = 0, len = _rowWeight.Length; i < len; i++)
                ret.Add(distribution.Sample());
            return ret.OrderBy(v => v).ToList();
        }
    }
}
