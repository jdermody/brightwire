using BrightData;
using BrightWire.Models;
using BrightWire.Models.Bayesian;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BrightWire.Bayesian.Training
{
    // http://nlp.stanford.edu/IR-book/html/htmledition/the-bernoulli-model-1.html
    class BernoulliNaiveBayesTrainer
    {
        readonly HashSet<uint> _vocabulary = new HashSet<uint>();
        readonly Dictionary<string, List<IndexList>> _documentClass = new Dictionary<string, List<IndexList>>();

        public void AddClassification(string documentClass, IndexList indexList)
        {
	        if (!_documentClass.TryGetValue(documentClass, out var temp))
                _documentClass.Add(documentClass, temp = new List<IndexList>());

            foreach (var item in indexList.Indices)
                _vocabulary.Add(item);
            temp.Add(indexList);
        }

        public BernoulliNaiveBayes Train()
        {
            double numDocs = _documentClass.Sum(d => d.Value.Count);

	        var ret = new List<BernoulliNaiveBayes.Class>();
            foreach (var item in _documentClass) {
                var docTerm = new Dictionary<uint, HashSet<int>>();
                for (int i = 0; i < item.Value.Count; i++) {
                    foreach (var word in item.Value[i].Indices) {
                        if (!docTerm.TryGetValue(word, out var temp))
                            docTerm.Add(word, temp = new HashSet<int>());
                        temp.Add(i);
                    }
                }
                double denominator = item.Value.Count + 2;
                var indexData = new List<BernoulliNaiveBayes.StringIndexProbability>();
                foreach (var word in docTerm) {
                    var probability = (word.Value.Count + 1) / denominator;
                    indexData.Add(new BernoulliNaiveBayes.StringIndexProbability {
                        StringIndex = word.Key,
                        ConditionalProbability = Math.Log(probability),
                        InverseProbability = Math.Log(1.0 - probability)
                    });
                }
                var missingProbability = 1.0 / denominator;
                ret.Add(new BernoulliNaiveBayes.Class {
                    Label = item.Key,
                    Prior = Math.Log(item.Value.Count / numDocs),
                    Index = indexData.ToArray(),
                    MissingProbability = Math.Log(missingProbability),
                    InverseMissingProbability = Math.Log(1.0 - missingProbability)
                });
            }
            return new BernoulliNaiveBayes {
                ClassData = ret.ToArray(),
                Vocabulary = _vocabulary.OrderBy(d => d).ToArray()
            };
        }
    }
}
