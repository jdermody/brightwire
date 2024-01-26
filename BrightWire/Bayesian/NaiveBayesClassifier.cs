using BrightWire.Models.Bayesian;
using System;
using System.Collections.Generic;
using System.Linq;
using BrightData.DataTable.Rows;

namespace BrightWire.Bayesian
{
    /// <summary>
    /// Naive bayes classifier
    /// </summary>
    internal class NaiveBayesClassifier : IRowClassifier
    {
        interface IProbabilityProvider
        {
            double GetProbability(GenericTableRow row);
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

            public double GetProbability(GenericTableRow row)
            {
	            var val = row.Get<string>(_columnIndex);
                return _probability.GetValueOrDefault(val, _nullValue);
            }
        }
        class ContinuousColumn(NaiveBayes.Column column) : IProbabilityProvider
        {
            public double GetProbability(GenericTableRow row)
            {
                var x = row.Get<float>(column.ColumnIndex);
                var exponent = Math.Exp(-1 * Math.Pow(x - column.Mean, 2) / (2 * column.Variance));
                return Math.Log(1.0 / Math.Sqrt(2 * Math.PI * column.Variance) * exponent);
            }
        }
        readonly List<(string, double, List<IProbabilityProvider>)> _classProbability = [];

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
                _classProbability.Add((cls.Label, cls.LogPrior, list));
            }
        }

        IEnumerable<(string Classification, double Score)> GetClassificationScores(GenericTableRow row)
        {
            foreach (var cls in _classProbability) {
                var score = cls.Item2;
                foreach (var item in cls.Item3)
                    score += item.GetProbability(row);
                yield return (cls.Item1, score);
            }
        }

        /// <summary>
        /// Naive bayes classifications should only be used for ranking against each other and not for deriving an actual weighted probability
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public (string Label, float Weight)[] Classify(GenericTableRow row)
        {
            return GetClassificationScores(row)
                .OrderByDescending(kv => kv.Score)
                .Take(1)
                .Select((kv, _) => (kv.Classification, 1f))
                .ToArray()
            ;
        }
    }
}
