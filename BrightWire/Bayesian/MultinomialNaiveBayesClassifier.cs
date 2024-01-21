using System;
using BrightWire.Models.Bayesian;
using System.Collections.Generic;
using System.Linq;
using BrightData.Types;

namespace BrightWire.Bayesian
{
    /// <summary>
    /// Multinomial naive bayes classifier
    /// </summary>
    internal class MultinomialNaiveBayesClassifier(MultinomialNaiveBayes model) : IIndexListClassifier
    {
        class Classification(MultinomialNaiveBayes.Class cls)
        {
	        readonly double _prior = cls.Prior, _missing = cls.MissingProbability;
            readonly Dictionary<uint, double> _index = cls.Index.ToDictionary(d => d.StringIndex, d => d.ConditionalProbability);

            public string Label { get; } = cls.Label;

            public double GetScore(ReadOnlyMemory<uint> stringIndexList)
            {
                var score = _prior;
                foreach (var item in stringIndexList.Span) {
                    score += _index.GetValueOrDefault(item, _missing);
                }
                return score;
            }
        }
        readonly List<Classification> _classification = model.ClassData.Select(c => new Classification(c)).ToList();

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
