using BrightTable;
using BrightWire.Models.Bayesian;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BrightWire.Bayesian
{
    /// <summary>
    /// Naive bayes classifier
    /// </summary>
    class NaiveBayesClassifier : IRowClassifier
    {
        interface IProbabilityProvider
        {
            double GetProbability(IConvertibleRow row);
        }
        class CategoricalColumn : IProbabilityProvider
        {
            readonly uint _columnIndex;
            readonly Dictionary<string, double> _probability;
            readonly double _nullValue;

            public CategoricalColumn(NaiveBayes.Column summary, double nullValue = 0)
            {
                _nullValue = nullValue;
                _columnIndex = summary.ColumnIndex;
                _probability = summary.Probability.ToDictionary(d => d.Category, d => d.LogProbability);
            }

            public double GetProbability(IConvertibleRow row)
            {
	            var val = row.GetTyped<string>(_columnIndex);
                if (_probability.TryGetValue(val, out var ret))
                    return ret;
                return _nullValue;
            }
        }
        class ContinuousColumn : IProbabilityProvider
        {
            readonly NaiveBayes.Column _column;

            public ContinuousColumn(NaiveBayes.Column column)
            {
                _column = column;
            }

            public double GetProbability(IConvertibleRow row)
            {
                double x = row.GetTyped<double>(_column.ColumnIndex);
                var exponent = Math.Exp(-1 * Math.Pow(x - _column.Mean, 2) / (2 * _column.Variance));
                return Math.Log(1.0 / Math.Sqrt(2 * Math.PI * _column.Variance) * exponent);
            }
        }
        readonly List<Tuple<string, double, List<IProbabilityProvider>>> _classProbability = new List<Tuple<string, double, List<IProbabilityProvider>>>();

        public NaiveBayesClassifier(NaiveBayes model)
        {
            foreach (var cls in model.Class) {
                var list = new List<IProbabilityProvider>();
                foreach (var col in cls.ColumnSummary) {
                    if (col.Type == NaiveBayes.ColumnType.Categorical)
                        list.Add(new CategoricalColumn(col));
                    else if (col.Type == NaiveBayes.ColumnType.ContinuousGaussian)
                        list.Add(new ContinuousColumn(col));
                }
                _classProbability.Add(Tuple.Create(cls.Label, cls.LogPrior, list));
            }
        }

        Dictionary<string, double> _Classify(IConvertibleRow row)
        {
            var ret = new Dictionary<string, double>();
            foreach (var cls in _classProbability) {
                var score = cls.Item2;
                foreach (var item in cls.Item3)
                    score += item.GetProbability(row);
                ret.Add(cls.Item1, score);
            }
            return ret;
        }

        /// <summary>
        /// Naive bayes classifications should only be used for ranking against each other and not for deriving an actual weighted probability
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public (string Label, float Weight)[] Classify(IConvertibleRow row)
        {
            return _Classify(row)
                .OrderByDescending(kv => kv.Value)
                .Take(1)
                .Select((kv, i) => (kv.Key, 1f))
                .ToArray()
            ;
        }
    }
}
