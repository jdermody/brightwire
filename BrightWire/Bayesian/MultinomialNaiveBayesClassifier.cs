using BrightWire.Models;
using BrightWire.Models.Bayesian;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BrightWire.Bayesian
{
    /// <summary>
    /// Multinomial naive bayes classifer
    /// </summary>
    class MultinomialNaiveBayesClassifier : IIndexListClassifier
    {
        class Classification
        {
	        readonly double _prior, _missing;
            readonly Dictionary<uint, double> _index;

            public Classification(MultinomialNaiveBayes.Class cls)
            {
                Label = cls.Label;
                _prior = cls.Prior;
                _missing = cls.MissingProbability;
                _index = cls.Index.ToDictionary(d => d.StringIndex, d => d.ConditionalProbability);
            }
            public string Label { get; }

	        public double GetScore(IReadOnlyList<uint> stringIndexList)
            {
                double score = _prior;
                foreach (var item in stringIndexList) {
                    if (_index.TryGetValue(item, out var temp))
                        score += temp;
                    else
                        score += _missing;
                }
                return score;
            }
        }
        readonly List<Classification> _classification;

        public MultinomialNaiveBayesClassifier(MultinomialNaiveBayes model)
        {
            _classification = model.ClassData.Select(c => new Classification(c)).ToList();
        }

        List<(string, double)> _Classify(IReadOnlyList<uint> stringIndexList)
        {
            var ret = new List<(string, double)>();
            foreach (var cls in _classification)
                ret.Add((cls.Label, cls.GetScore(stringIndexList)));
            return ret;
        }

        /// <summary>
        ///  Naive bayes values should only be used for ranking against each other
        /// </summary>
        public IReadOnlyList<(string Label, float Weight)> Classify(IndexList indexList)
        {
            return _Classify(indexList.Index)
                .OrderByDescending(kv => kv.Item2)
                .Take(1)
                .Select((d, i) => (d.Item1, 1f))
                .ToList()
            ;
        }
    }
}
