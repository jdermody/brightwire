using BrightWire.Models;
using BrightWire.Models.Bayesian;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace BrightWire.Bayesian
{
    internal class BernoulliNaiveBayesClassifier : IRowClassifier
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
            public string Label { get { return _class.Label; } }
            public double GetScore(HashSet<uint> documentSet)
            {
                double ret = _class.Prior;

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
        readonly List<Classification> _classification = new List<Classification>();

        public BernoulliNaiveBayesClassifier(BernoulliNaiveBayes model)
        {
            _classification = model.ClassData.Select(c => new Classification(model.Vocabulary, c)).ToList();
        }

        List<(string, double)> _Classify(IReadOnlyList<uint> featureIndexList)
        {
            var documentSet = new HashSet<uint>(featureIndexList);
            var ret = new List<(string, double)>();
            foreach (var cls in _classification)
                ret.Add((cls.Label, cls.GetScore(documentSet)));
            return ret;
        }

        /// <summary>
        ///  Naive bayes values should only be used for ranking against each other
        /// </summary>
        /// <param name="featureIndexList"></param>
        /// <returns></returns>
        public IReadOnlyList<(string Classification, float Weight)> GetWeightedClassifications(IReadOnlyList<uint> featureIndexList)
        {
            return _Classify(featureIndexList)
                .OrderByDescending(kv => kv.Item2)
                .Take(1)
                .Select((d, i) => (d.Item1, 1f))
                .ToList()
            ;
        }

        public IReadOnlyList<(string Classification, float Weight)> Classify(IRow row)
        {
            Debug.Assert(row.Table.Columns.FirstOrDefault()?.Type == ColumnType.IndexList);
            var indexList = row.GetField<IndexList>(0);
            return GetWeightedClassifications(indexList.Index);
        }
    }
}
