using BrightWire.Connectionist;
using BrightWire.Helper;
using BrightWire.Models;
using BrightWire.Models.Simple;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.SampleCode
{
    partial class Program
    {
        static ClassificationBag _BuildClassificationBag(IReadOnlyList<Tuple<string[], string>> data, StringTableBuilder stringTable)
        {
            return new ClassificationBag {
                Classifications = data.Select(d => new ClassificationBag.Classification {
                    Name = d.Item2,
                    Data = d.Item1.Select(str => stringTable.GetIndex(str)).ToArray()
                }).ToArray()
            };
        }

        static string[] _Tokenise(string str)
        {
            return SimpleTokeniser.JoinNegations(SimpleTokeniser.Tokenise(str).Select(s => s.ToLower())).ToArray();
        }

        static FeedForwardNetwork _TrainNeuralNetwork(ILinearAlgebraProvider lap, ITrainingDataProvider trainingData, ITrainingDataProvider testData)
        {
            const int HIDDEN_SIZE = 512, BATCH_SIZE = 128, NUM_EPOCHS = 10;
            const float TRAINING_RATE = 0.1f;

            var errorMetric = ErrorMetricType.OneHot.Create();
            var layerTemplate = new LayerDescriptor(0.1f) {
                WeightUpdate = WeightUpdateType.Adam,
                Activation = ActivationType.Relu,
                WeightInitialisation = WeightInitialisationType.Xavier,
                LayerTrainer = LayerTrainerType.Dropout
            };

            Console.WriteLine($"Training a {trainingData.InputSize}x{HIDDEN_SIZE}x{trainingData.OutputSize} neural network...");
            FeedForwardNetwork bestModel = null;
            using (var trainer = lap.NN.CreateBatchTrainer(layerTemplate, trainingData.InputSize, HIDDEN_SIZE, trainingData.OutputSize)) {
                var trainingContext = lap.NN.CreateTrainingContext(TRAINING_RATE, BATCH_SIZE, errorMetric);
                float bestScore = 0;
                trainingContext.EpochComplete += c => {
                    var testError = trainer.Execute(testData).Select(d => errorMetric.Compute(d.Output, d.ExpectedOutput)).Average();
                    var flag = false;
                    if (testError > bestScore) {
                        bestScore = testError;
                        bestModel = trainer.NetworkInfo;
                        flag = true;
                    }
                    trainingContext.WriteScore(testError, errorMetric.DisplayAsPercentage, flag);
                };
                trainer.Train(trainingData, NUM_EPOCHS, trainingContext);
                Console.WriteLine("Final test score: {0:P}", bestScore);
            }
            return bestModel;
        }

        /// <summary>
        /// Classifies text into either positive or negative sentiment
        /// The data files can be downloaded from https://archive.ics.uci.edu/ml/datasets/Sentiment+Labelled+Sentences
        /// </summary>
        /// <param name="dataFilesPath">Path to extracted data files</param>
        public static void SentimentClassification(string dataFilesPath)
        {
            var files = new[] {
                "amazon_cells_labelled.txt",
                "imdb_labelled.txt",
                "yelp_labelled.txt"
            };
            var LINE_SEPARATOR = "\n".ToCharArray();
            var SEPARATOR = "\t".ToCharArray();
            var stringTable = new StringTableBuilder();
            var sentimentData = files.SelectMany(f => File.ReadAllText(dataFilesPath + f)
                .Split(LINE_SEPARATOR)
                .Where(l => !String.IsNullOrWhiteSpace(l))
                .Select(l => l.Split(SEPARATOR))
                .Select(s => Tuple.Create(_Tokenise(s[0]), s[1][0] == '1' ? "positive" : "negative"))
                .Where(d => d.Item1.Any())
            ).Shuffle(0).ToList();
            var splitSentimentData = sentimentData.Split();

            // build training and test classification bag
            var trainingClassificationBag = _BuildClassificationBag(splitSentimentData.Training, stringTable);
            var testClassificationBag = _BuildClassificationBag(splitSentimentData.Test, stringTable);

            // train a bernoulli naive bayes classifier
            var bernoulli = trainingClassificationBag.TrainBernoulliNaiveBayes();
            Console.WriteLine("Bernoulli accuracy: {0:P}", testClassificationBag
                .Classify(bernoulli.CreateClassifier())
                .Average(r => r.Score)
            );

            // train a multinomial naive bayes classifier
            var multinomial = trainingClassificationBag.TrainMultinomialNaiveBayes();
            Console.WriteLine("Multinomial accuracy: {0:P}", testClassificationBag
                .Classify(multinomial.CreateClassifier())
                .Average(r => r.Score)
            );

            // convert the bags to sets
            var sentimentDataBag = _BuildClassificationBag(sentimentData, stringTable);
            var sentimentDataSet = sentimentDataBag.ConvertToSet(false);
            var sentimentDataSetSplit = sentimentDataSet.Split();

            using (var lap = Provider.CreateGPULinearAlgebra(false)) {
                var maxIndex = sentimentDataSet.GetMaximumIndex();
                var classificationTable = sentimentDataSet.GetClassifications().ToDictionary(d => (int)d.Value, d => d.Key);

                var trainingData = sentimentDataSetSplit.Training.CreateTrainingDataProvider(lap, maxIndex);
                var testData = sentimentDataSetSplit.Test.CreateTrainingDataProvider(lap, maxIndex);

                // create the three classifiers
                var bernoulliClassifier = bernoulli.CreateClassifier();
                var multinomialClassifier = multinomial.CreateClassifier();
                var neuralClassifier = lap.NN.CreateFeedForward(_TrainNeuralNetwork(lap, trainingData, testData));

                uint stringIndex;
                Console.WriteLine("Enter some text to test the classifiers...");
                while (true) {
                    Console.Write(">");
                    var line = Console.ReadLine();
                    if (String.IsNullOrWhiteSpace(line))
                        break;

                    var tokens = _Tokenise(line);
                    var indexList = new List<uint>();
                    foreach(var token in tokens) {
                        if (stringTable.TryGetIndex(token, out stringIndex))
                            indexList.Add(stringIndex);
                    }
                    if (indexList.Any()) {
                        var queryTokens = indexList.GroupBy(d => d).Select(g => Tuple.Create(g.Key, (float)g.Count())).ToList();
                        var vector = new float[maxIndex];
                        foreach (var token in queryTokens)
                            vector[token.Item1] = token.Item2;

                        Console.WriteLine("Bernoulli classification: " + bernoulliClassifier.Classify(indexList).First());
                        Console.WriteLine("Multinomial classification: " + multinomialClassifier.Classify(indexList).First());
                        Console.WriteLine("Neural network classification: " + classificationTable[neuralClassifier.Execute(vector).MaximumIndex()]);
                    }
                    else
                        Console.WriteLine("Sorry, none of those words have been seen before.");
                }
            }
            Console.WriteLine();
        }
    }
}
