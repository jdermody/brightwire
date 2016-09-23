using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.Bayesian.Training
{
    // http://nlp.stanford.edu/IR-book/html/htmledition/the-bernoulli-model-1.html
    public class BernoulliNaiveBayesTrainer
    {
        readonly HashSet<uint> _vocabulary = new HashSet<uint>();
        readonly Dictionary<string, List<IReadOnlyList<uint>>> _documentClass = new Dictionary<string, List<IReadOnlyList<uint>>>();

        public void AddDocument(string documentClass, IReadOnlyList<uint> stringIndexList)
        {
            List<IReadOnlyList<uint>> temp;
            if (!_documentClass.TryGetValue(documentClass, out temp))
                _documentClass.Add(documentClass, temp = new List<IReadOnlyList<uint>>());

            foreach (var item in stringIndexList)
                _vocabulary.Add(item);
            temp.Add(stringIndexList);
        }

        public BernoulliNaiveBayes Train()
        {
            double numDocs = _documentClass.Sum(d => d.Value.Count);
            double numWords = _vocabulary.Count;

            HashSet<int> temp;
            var ret = new List<BernoulliNaiveBayes.Class>();
            foreach (var item in _documentClass) {
                var docTerm = new Dictionary<uint, HashSet<int>>();
                for(int i = 0; i < item.Value.Count; i++) {
                    foreach(var word in item.Value[i]) {
                        if (!docTerm.TryGetValue(word, out temp))
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
