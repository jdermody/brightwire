using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using BrightWire.Models.Output;

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

        public IEnumerable<string> Classify(IRow row)
        {
            return _Classify(row)
                .OrderByDescending(r => r.Item2)
                .Select(r => _model.Classification[r.Item1])
            ;
        }

        public IReadOnlyList<WeightedClassification> GetWeightedClassifications(IRow row)
        {
            return _Classify(row)
                .Select(r => new WeightedClassification(_model.Classification[r.Item1], r.Item2))
                .ToList()
            ;
        }
    }
}
