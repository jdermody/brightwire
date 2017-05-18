using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using BrightWire.Helper;
using BrightWire.LinearAlgebra.Helper;

namespace BrightWire.Linear
{
    internal class MultinomialLogisticRegressionClassifier : IRowClassifier
    {
        readonly MultinomialLogisticRegression _model;
        readonly List<ILogisticRegressionClassifier> _classifier;

        public MultinomialLogisticRegressionClassifier(ILinearAlgebraProvider lap, MultinomialLogisticRegression model)
        {
            _model = model;
            _classifier = _model.Model.Select(c => c.CreatePredictor(lap)).ToList();
        }

        IEnumerable<Tuple<int, float>> _Classify(IRow row)
        {
            var featureCount = _model.FeatureColumn.Length;
            var features = new float[featureCount];
            for (var i = 0; i < featureCount; i++)
                features[i] = row.GetField<float>(_model.FeatureColumn[i]);

            return _classifier.Select((m, i) => Tuple.Create(i, m.Predict(features)));
        }

        //public IEnumerable<string> Classify(IRow row)
        //{
        //    return _Classify(row)
        //        .OrderByDescending(r => r.Item2)
        //        .Select(r => _model.Classification[r.Item1])
        //    ;
        //}

        public IReadOnlyList<(string Label, float Weight)> Classify(IRow row)
        {
            // calculate softmax over output value
            float max = float.MinValue, total = 0;
            var raw = new List<Tuple<string, float>>();
            foreach (var item in _Classify(row)) {
                var classification = _model.Classification[item.Item1];
                var score = item.Item2;
                if (score > max)
                    max = score;
                raw.Add(Tuple.Create(classification, score));
            }
            var softmax = new List<Tuple<string, float>>();
            foreach (var item in raw) {
                var exp = BoundMath.Exp(item.Item2);
                total += exp;
                softmax.Add(Tuple.Create(item.Item1, exp));
            }

            return softmax
                .Select(r => (r.Item1, r.Item2 / total))
                .ToList()
            ;
        }
    }
}
