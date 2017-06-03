using BrightWire.Models;
using BrightWire.Models.Bayesian;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BrightWire.Bayesian.Training
{
    // http://nlp.stanford.edu/IR-book/html/htmledition/naive-bayes-text-classification-1.html
    internal class MultinomialNaiveBayesTrainer
    {
        readonly HashSet<uint> _vocabulary = new HashSet<uint>();
        readonly Dictionary<string, List<IndexList>> _documentClass = new Dictionary<string, List<IndexList>>();

        public void AddClassification(string documentClass, IndexList indexList)
        {
            List<IndexList> temp;
            if (!_documentClass.TryGetValue(documentClass, out temp))
                _documentClass.Add(documentClass, temp = new List<IndexList>());

            foreach (var item in indexList.Index)
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
                var allClassToken = item.Value.SelectMany(d => d.Index).ToList();
                double denominator = allClassToken.Count + numWords;
                foreach (var word in allClassToken.GroupBy(d => d).Select(d => Tuple.Create(d.Key, d.Count()))) {
                    indexData.Add(new MultinomialNaiveBayes.StringIndexProbability {
                        StringIndex = word.Item1,
                        ConditionalProbability = Math.Log((word.Item2 + 1) / denominator)
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
