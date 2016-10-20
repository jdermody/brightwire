using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

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

        public IEnumerable<string> Classify(IRow row)
        {
            var featureCount = _model.FeatureColumn.Length;
            var features = new float[featureCount];
            for (var i = 0; i < featureCount; i++)
                features[i] = row.GetField<float>(_model.FeatureColumn[i]);

            return _classifier.Select((m, i) => Tuple.Create(i, m.Predict(features)))
                .OrderByDescending(r => r.Item2)
                .Select(r => _model.Classification[r.Item1])
            ;
        }
    }
}
