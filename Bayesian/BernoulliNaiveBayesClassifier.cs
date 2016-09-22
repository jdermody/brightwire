using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.Bayesian
{
    public class BernoulliNaiveBayesClassifier
    {
        class Classification
        {
            readonly IndexedNaiveBayes.Class _class;
            readonly uint[] _vocabulary;
            readonly HashSet<uint> _wordSet = new HashSet<uint>();

            public Classification(uint[] vocabulary, IndexedNaiveBayes.Class cls)
            {
                _vocabulary = vocabulary;
                _class = cls;
                foreach (var item in cls.Index)
                    _wordSet.Add(item.StringIndex);
            }
            public string Label { get { return _class.Label; } }
            public double GetScore(HashSet<uint> documentSet)
            {
                double ret = _class.Prior;

                foreach(var item in _class.Index) {
                    if (documentSet.Contains(item.StringIndex))
                        ret += item.ConditionalProbability;
                    else
                        ret += item.InverseProbability;
                }

                foreach (var word in _vocabulary.Where(w => !_wordSet.Contains(w))) {
                    if (documentSet.Contains(word))
                        ret += _class.MissingProbability;
                    else
                        ret += _class.InverseMissingProbability;
                    //if (documentSet.Contains(word)) {
                    //    if (_index.TryGetValue(word, out temp))
                    //        ret += temp.Item1;
                    //    else
                    //        ret += _missing;
                    //}
                    //else {
                    //    if (_index.TryGetValue(word, out temp))
                    //        ret += temp.Item2;
                    //    else
                    //        ret += _missingInverse;
                    //}
                }
                return ret;
            }
        }
        readonly List<Classification> _classification = new List<Classification>();

        public BernoulliNaiveBayesClassifier(IndexedNaiveBayes model)
        {
            if (model.Vocabulary == null)
                throw new ArgumentException();
            _classification = model.ClassData.Select(c => new Classification(model.Vocabulary, c)).ToList();
        }

        public IEnumerable<string> Classify(IReadOnlyList<uint> stringIndexList)
        {
            var documentSet = new HashSet<uint>(stringIndexList);
            var ret = new List<Tuple<string, double>>();
            foreach (var cls in _classification)
                ret.Add(Tuple.Create(cls.Label, cls.GetScore(documentSet)));
            return ret.OrderByDescending(kv => kv.Item2).Select(kv => kv.Item1);
        }
    }
}
