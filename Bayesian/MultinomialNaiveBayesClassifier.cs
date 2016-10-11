using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.Bayesian
{
    internal class MultinomialNaiveBayesClassifier : IIndexBasedClassifier
    {
        class Classification
        {
            readonly string _label;
            readonly double _prior, _missing;
            readonly Dictionary<uint, double> _index;

            public Classification(MultinomialNaiveBayes.Class cls)
            {
                _label = cls.Label;
                _prior = cls.Prior;
                _missing = cls.MissingProbability;
                _index = cls.Index.ToDictionary(d => d.StringIndex, d => d.ConditionalProbability);
            }
            public string Label { get { return _label; } }
            public double GetScore(IReadOnlyList<uint> stringIndexList)
            {
                double score = _prior, temp;
                foreach(var item in stringIndexList) {
                    if (_index.TryGetValue(item, out temp))
                        score += temp;
                    else
                        score += _missing;
                }
                return score;
            }
        }
        readonly List<Classification> _classification = new List<Classification>();

        public MultinomialNaiveBayesClassifier(MultinomialNaiveBayes model)
        {
            _classification = model.ClassData.Select(c => new Classification(c)).ToList();
        }

        public IEnumerable<string> Classify(IReadOnlyList<uint> stringIndexList)
        {
            var ret = new List<Tuple<string, double>>();
            foreach (var cls in _classification)
                ret.Add(Tuple.Create(cls.Label, cls.GetScore(stringIndexList)));
            return ret.OrderByDescending(kv => kv.Item2).Select(kv => kv.Item1);
        }
    }
}
