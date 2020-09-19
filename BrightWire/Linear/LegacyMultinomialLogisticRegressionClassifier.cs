using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightData;
using BrightData.Helper;
using BrightTable;
using BrightWire.Models;

namespace BrightWire.Linear
{
    public class LegacyMultinomialLogisticRegressionClassifier : IRowClassifier
    {
        readonly MultinomialLogisticRegression _model;
        readonly List<LegacyLogisticRegressionPredictor> _classifier;

        public LegacyMultinomialLogisticRegressionClassifier(ILinearAlgebraProvider lap, MultinomialLogisticRegression model)
        {
            _model = model;
            _classifier = _model.Model.Select(c => new LegacyLogisticRegressionPredictor(lap, lap.CreateVector(c.Theta))).ToList();
        }

        IEnumerable<Tuple<int, float>> _Classify(IConvertibleRow row)
        {
            var featureCount = _model.FeatureColumn.Length;
            var features = new float[featureCount];
            for (var i = 0; i < featureCount; i++)
                features[i] = row.GetField<float>(_model.FeatureColumn[i]);

            return _classifier.Select((m, i) => Tuple.Create(i, m.Predict(features)));
        }

        public (string Label, float Weight)[] Classify(IConvertibleRow row)
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
                var exp = FloatMath.Exp(item.Item2);
                total += exp;
                softmax.Add(Tuple.Create(item.Item1, exp));
            }

            return softmax
                .Select(r => (r.Item1, r.Item2 / total))
                .ToArray()
            ;
        }
    }
}