using System;
using BrightData;
using BrightWire.Models.Bayesian;
using System.Collections.Generic;
using System.Linq;

namespace BrightWire.Bayesian
{
    /// <summary>
    /// Multinomial naive bayes classifer
    /// </summary>
    internal class MultinomialNaiveBayesClassifier : IIndexListClassifier
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

	        public double GetScore(ReadOnlyMemory<uint> stringIndexList)
            {
                var score = _prior;
                foreach (var item in stringIndexList.Span) {
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

        IEnumerable<(string Classification, double Score)> Classify(ReadOnlyMemory<uint> stringIndexList)
        {
            foreach (var cls in _classification)
                yield return (cls.Label, cls.GetScore(stringIndexList));
        }

        /// <summary>
        ///  Naive bayes values should only be used for ranking against each other
        /// </summary>
        public (string Label, float Weight)[] Classify(IndexList indexList)
        {
            return Classify(indexList.ReadOnlyMemory)
                .OrderByDescending(kv => kv.Score)
                .Take(1)
                .Select((d, _) => (d.Classification, 1f))
                .ToArray();
        }
    }
}
