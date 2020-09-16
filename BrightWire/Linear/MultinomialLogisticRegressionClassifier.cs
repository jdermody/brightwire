using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using BrightData;
using BrightData.Helper;
using BrightTable;

namespace BrightWire.Linear
{
    /// <summary>
    /// Makes predictions from a previously trained model
    /// </summary>
    class MultinomialLogisticRegressionClassifier : ITableClassifier
    {
        readonly MultinomialLogisticRegression _model;
        readonly List<ILogisticRegressionClassifier> _classifier;

        public MultinomialLogisticRegressionClassifier(ILinearAlgebraProvider lap, MultinomialLogisticRegression model)
        {
            _model = model;
            _classifier = _model.Model.Select(c => c.CreateClassifier(lap)).ToList();
        }

        //IEnumerable<Tuple<int, float>> _Classify(IConvertibleRow row)
        //{
        //    var featureCount = _model.FeatureColumn.Length;
        //    var features = new float[featureCount];
        //    for (var i = 0; i < featureCount; i++)
        //        features[i] = row.GetField<float>(_model.FeatureColumn[i]);

        //    return _classifier.Select((m, i) => Tuple.Create(i, m.Predict(features)));
        //}

        //public (string Label, float Weight)[] Classify(IConvertibleRow row)
        //{
        //    // calculate softmax over output value
        //    float max = float.MinValue, total = 0;
        //    var raw = new List<Tuple<string, float>>();
        //    foreach (var item in _Classify(row)) {
        //        var classification = _model.Classification[item.Item1];
        //        var score = item.Item2;
        //        if (score > max)
        //            max = score;
        //        raw.Add(Tuple.Create(classification, score));
        //    }
        //    var softmax = new List<Tuple<string, float>>();
        //    foreach (var item in raw) {
        //        var exp = FloatMath.Exp(item.Item2);
        //        total += exp;
        //        softmax.Add(Tuple.Create(item.Item1, exp));
        //    }

        //    return softmax
        //        .Select(r => (r.Item1, r.Item2 / total))
        //        .ToArray()
        //    ;
        //}
        public IEnumerable<(uint RowIndex, (string Classification, float Weight)[] Predictions)> Classify(IDataTable table)
        {
            var matrix = table.AsMatrix(_model.FeatureColumn);
            var output = _classifier.Select(c => c.Predict(matrix)).ToArray();
            for (uint i = 0; i < matrix.RowCount; i++) {
                var rowOutput = table.Context.CreateVector(output.Select(v => v[i]).ToArray());
                var classification = _model.Classification.Zip(rowOutput.Softmax().Values).ToArray();
                yield return (i, classification);
            }
        }
    }
}
