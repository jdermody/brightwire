using BrightData;
using BrightWire.Models.Bayesian;
using System.Collections.Generic;
using System.Linq;

namespace BrightWire.Bayesian
{
    /// <summary>
    /// Bernoulli naive bayes classifier
    /// </summary>
    internal class BernoulliNaiveBayesClassifier : IIndexListClassifier
    {
        class Classification
        {
            readonly BernoulliNaiveBayes.Class _class;
            readonly List<uint> _excluded;

            public Classification(uint[] vocabulary, BernoulliNaiveBayes.Class cls)
            {
                _class = cls;

                var included = new HashSet<uint>();
                foreach (var item in cls.Index)
                    included.Add(item.StringIndex);
                _excluded = vocabulary.Where(w => !included.Contains(w)).ToList();
            }
            public string Label => _class.Label;

	        public double GetScore(HashSet<uint> documentSet)
            {
                var ret = _class.Prior;

                foreach (var item in _class.Index) {
                    if (documentSet.Contains(item.StringIndex))
                        ret += item.ConditionalProbability;
                    else
                        ret += item.InverseProbability;
                }

                int missingCount = 0, inverseMissingCount = 0;
                foreach (var word in _excluded) {
                    if (documentSet.Contains(word))
                        ++missingCount;
                    else
                        ++inverseMissingCount;
                }
                ret += (missingCount * _class.MissingProbability) + (inverseMissingCount * _class.InverseMissingProbability);
                return ret;
            }
        }
        readonly List<Classification> _classification;

        public BernoulliNaiveBayesClassifier(BernoulliNaiveBayes model)
        {
            _classification = model.ClassData.Select(c => new Classification(model.Vocabulary, c)).ToList();
        }

        IEnumerable<(string Classification, double Score)> Classify(IEnumerable<uint> featureIndexList)
        {
            var documentSet = new HashSet<uint>(featureIndexList);
            foreach (var cls in _classification)
                yield return (cls.Label, cls.GetScore(documentSet));
        }

        /// <summary>
        ///  Naive bayes values should only be used for ranking against each other
        /// </summary>
        public (string Label, float Weight)[] Classify(IndexList indexList)
        {
            return Classify(indexList.Indices)
                .OrderByDescending(kv => kv.Score)
                .Take(1)
                .Select((d, _) => (d.Classification, 1f))
                .ToArray()
            ;
        }
    }
}
