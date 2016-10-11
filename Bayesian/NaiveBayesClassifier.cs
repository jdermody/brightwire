using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.Bayesian
{
    internal class NaiveBayesClassifier : IRowClassifier
    {
        interface IProbabilityProvider
        {
            double GetProbability(IRow row);
        }
        class CategoricalColumn : IProbabilityProvider
        {
            readonly int _columnIndex;
            readonly Dictionary<string, double> _probability;
            readonly double _nullValue;

            public CategoricalColumn(NaiveBayes.CategorialColumn summary, double nullValue = 0)
            {
                _nullValue = nullValue;
                _columnIndex = summary.ColumnIndex;
                _probability = summary.Probability.ToDictionary(d => d.Category, d => d.LogProbability);
            }

            public double GetProbability(IRow row)
            {
                double ret;
                var val = row.GetField<string>(_columnIndex);
                if (_probability.TryGetValue(val, out ret))
                    return ret;
                return _nullValue;
            }
        }
        class ContinuousColumn : IProbabilityProvider
        {
            readonly NaiveBayes.ContinuousGaussianColumn _column;

            public ContinuousColumn(NaiveBayes.ContinuousGaussianColumn column)
            {
                _column = column;
            }

            public double GetProbability(IRow row)
            {
                double x = row.GetField<double>(_column.ColumnIndex);
                var exponent = Math.Exp(-1 * Math.Pow(x - _column.Mean, 2) / (2 * Math.Pow(_column.Variance, 2)));
                return Math.Log(1.0 / Math.Sqrt(2 * Math.PI * _column.Variance) * exponent);
            }
        }
        readonly List<Tuple<string, List<IProbabilityProvider>>> _classProbability = new List<Tuple<string, List<IProbabilityProvider>>>();

        public NaiveBayesClassifier(NaiveBayes model)
        {
            foreach (var cls in model.Class) {
                List<IProbabilityProvider> list = new List<IProbabilityProvider>();
                foreach (var col in cls.ColumnSummary) {
                    if (col.Type == NaiveBayes.ColumnType.Categorical)
                        list.Add(new CategoricalColumn(col as NaiveBayes.CategorialColumn));
                    else if (col.Type == NaiveBayes.ColumnType.ContinuousGaussian)
                        list.Add(new ContinuousColumn(col as NaiveBayes.ContinuousGaussianColumn));
                }
                _classProbability.Add(Tuple.Create(cls.Label, list));
            }
        }

        public IEnumerable<string> Classify(IRow row)
        {
            var ret = new Dictionary<string, double>();
            foreach(var cls in _classProbability) {
                double score = 0;
                foreach (var item in cls.Item2)
                    score += item.GetProbability(row);
                ret.Add(cls.Item1, score);
            }
            return ret.OrderByDescending(kv => kv.Value).Select(kv => kv.Key);
        }
    }
}
