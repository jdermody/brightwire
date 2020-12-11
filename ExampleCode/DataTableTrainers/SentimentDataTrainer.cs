using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BrightData;
using BrightWire;
using BrightWire.Models.Bayesian;
using BrightWire.TrainingData.Helper;

namespace ExampleCode.DataTableTrainers
{
    class SentimentDataTrainer
    {
        private readonly (string Classification, IndexList Data)[] _indexedSentencesTraining;
        private readonly (string Classification, IndexList Data)[] _indexedSentencesTest;

        public SentimentDataTrainer(IBrightDataContext context, DirectoryInfo directory)
        {
            var files = new[]
            {
                "amazon_cells_labelled.txt",
                "imdb_labelled.txt",
                "yelp_labelled.txt"
            };
            var LINE_SEPARATOR = "\n".ToCharArray();
            var SEPARATOR = "\t".ToCharArray();
            var stringTable = new StringTableBuilder();

            var sentences = new List<(string[] Sentence, string Classification)>();
            foreach (var path in files.Select(f => Path.Combine(directory.FullName, "sentiment labelled sentences", f)))
            {
                var data = File.ReadAllText(path)
                    .Split(LINE_SEPARATOR)
                    .Where(l => !String.IsNullOrWhiteSpace(l))
                    .Select(l => l.Split(SEPARATOR))
                    .Select(s => (Sentence: _Tokenise(s[0]), Classification: s[1][0] == '1' ? "positive" : "negative"))
                    .Where(d => (d.Sentence ?? Array.Empty<string>()).Any());
                sentences.AddRange(data);
            }

            var trainingData = sentences.Shuffle(context.Random).ToArray().Split();
            _indexedSentencesTraining = _BuildIndexedClassifications(context, trainingData.Training, stringTable);
            _indexedSentencesTest = _BuildIndexedClassifications(context, trainingData.Test, stringTable);
        }

        public BernoulliNaiveBayes TrainBernoulli()
        {
            var bernoulli = _indexedSentencesTraining.TrainBernoulliNaiveBayes();
            Console.WriteLine("Bernoulli accuracy: {0:P}", _indexedSentencesTest
                .Classify(bernoulli.CreateClassifier())
                .Average(r => r.Score)
            );
            return bernoulli;
        }

        public MultinomialNaiveBayes TrainMultinomialNaiveBayes()
        {
            var multinomial = _indexedSentencesTraining.TrainMultinomialNaiveBayes();
            Console.WriteLine("Multinomial accuracy: {0:P}", _indexedSentencesTest
                .Classify(multinomial.CreateClassifier())
                .Average(r => r.Score)
            );
            return multinomial;
        }

        static string[] _Tokenise(string str)
        {
            return SimpleTokeniser.JoinNegations(SimpleTokeniser.Tokenise(str).Select(s => s.ToLower())).ToArray();
        }

        static (string Classification, IndexList Data)[] _BuildIndexedClassifications(IBrightDataContext context, (string[], string)[] data, StringTableBuilder stringTable)
        {
            return data
                .Select(d => (d.Item2, IndexList.Create(context, d.Item1.Select(str => stringTable.GetIndex(str)).ToArray())))
                .ToArray()
            ;
        }
    }
}
