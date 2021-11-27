using BrightData;
using BrightWire.Models.Bayesian;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BrightWire.Bayesian.Training
{
    // http://nlp.stanford.edu/IR-book/html/htmledition/naive-bayes-text-classification-1.html
    internal class MultinomialNaiveBayesTrainer
    {
        readonly HashSet<uint> _vocabulary = new();
        readonly Dictionary<string, List<IndexList>> _documentClass = new();

        public void AddClassification(string documentClass, IndexList indexList)
        {
	        if (!_documentClass.TryGetValue(documentClass, out var temp))
                _documentClass.Add(documentClass, temp = new List<IndexList>());

            foreach (var item in indexList.Indices)
                _vocabulary.Add(item);
            temp.Add(indexList);
        }

        public MultinomialNaiveBayes Train()
        {
            double numDocs = _documentClass.Sum(d => d.Value.Count);
            double numWords = _vocabulary.Count;

            var ret = new List<MultinomialNaiveBayes.Class>();
            foreach (var item in _documentClass) {
                var indexData = new List<MultinomialNaiveBayes.StringIndexProbability>();
                var allClassToken = item.Value.SelectMany(d => d.Indices).ToList();
                double denominator = allClassToken.Count + numWords;
                foreach (var (key, count) in allClassToken.GroupBy(d => d).Select(d => (d.Key, Count: d.Count()))) {
                    indexData.Add(new MultinomialNaiveBayes.StringIndexProbability {
                        StringIndex = key,
                        ConditionalProbability = Math.Log((count + 1) / denominator)
                    });
                }
                ret.Add(new MultinomialNaiveBayes.Class {
                    Label = item.Key,
                    Prior = Math.Log(item.Value.Count / numDocs),
                    Index = indexData.ToArray(),
                    MissingProbability = Math.Log(1.0 / denominator)
                });
            }
            return new MultinomialNaiveBayes {
                ClassData = ret.ToArray()
            };
        }
    }
}
