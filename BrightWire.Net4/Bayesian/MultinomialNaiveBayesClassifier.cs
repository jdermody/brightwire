using BrightWire.Models;
using BrightWire.Models.Bayesian;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace BrightWire.Bayesian
{
    internal class MultinomialNaiveBayesClassifier : IRowClassifier
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
                foreach (var item in stringIndexList) {
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

        public IReadOnlyList<(string Label, float Weight)> Classify(IRow row)
        {
            Debug.Assert(row.Table.Columns.FirstOrDefault()?.Type == ColumnType.IndexList);
            var indexList = row.GetField<IndexList>(0);
            return GetWeightedClassifications(indexList.Index);
        }
    }
}
