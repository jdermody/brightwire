using BrightWire.Connectionist;
using BrightWire.Ensemble;
using BrightWire.Helper;
using BrightWire.Models;
using BrightWire.Models.Input;
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
                Classification = data.Select(d => new IndexedClassification {
                    Name = d.Item2,
                    Data = d.Item1.Select(str => stringTable.GetIndex(str)).ToArray()
                }).ToArray()
            };
        }

        static string[] _Tokenise(string str)
        {
            return SimpleTokeniser.JoinNegations(SimpleTokeniser.Tokenise(str).Select(s => s.ToLower())).ToArray();
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

            // convert the bags to sparse vectors
            var sentimentDataBag = _BuildClassificationBag(sentimentData, stringTable);
            var sentimentDataSet = sentimentDataBag.ConvertToSparseVectors(false);
            var sentimentDataTableSplit = sentimentDataSet.Split();

            using (var lap = GPUProvider.CreateLinearAlgebra(false)) {
                var maxIndex = sentimentDataSet.GetMaximumIndex() + 1;
                var trainingData = sentimentDataTableSplit.Training.CreateTrainingDataProvider(lap, maxIndex);
                var testData = sentimentDataTableSplit.Test.CreateTrainingDataProvider(lap, maxIndex);
                var classificationTable = sentimentDataSet.GetClassifications().ToDictionary(d => (int)d.Value, d => d.Key);

                // create the three classifiers
                var bernoulliClassifier = bernoulli.CreateClassifier();
                var multinomialClassifier = multinomial.CreateClassifier();
                var neuralClassifier = lap.NN.CreateFeedForward(lap.NN.CreateTrainingContext(ErrorMetricType.OneHot, learningRate: 0.1f, batchSize: 128)
                    .TrainNeuralNetwork(lap, trainingData, testData, new LayerDescriptor(0.1f) {
                        WeightUpdate = WeightUpdateType.Adam,
                        Activation = ActivationType.Relu,
                        WeightInitialisation = WeightInitialisationType.Xavier,
                        LayerTrainer = LayerTrainerType.Dropout
                    }, hiddenLayerSize:512, numEpochs:10)
                );

                // create the stacked training set
                Console.WriteLine("Creating model stack data set...");
                var modelStacker = new ModelStacker();
                foreach (var item in sentimentDataSet.Classification) {
                    var indexList = item.GetIndexList();
                    modelStacker.Add(new[] {
                        bernoulliClassifier.GetWeightedClassifications(indexList),
                        multinomialClassifier.GetWeightedClassifications(indexList),
                        neuralClassifier.GetWeightedClassifications(item.Vectorise(maxIndex), classificationTable)
                    }, item.Name);
                }

                // convert the stacked data to a data table and split it into training and test sets
                var sentimentDataTable = modelStacker.GetTable();
                var dataTableVectoriser = sentimentDataTable.GetVectoriser(true);
                var split = sentimentDataTable.Split();
                var trainingStack = lap.NN.CreateTrainingDataProvider(split.Training, dataTableVectoriser);
                var testStack = lap.NN.CreateTrainingDataProvider(split.Test, dataTableVectoriser);
                var targetColumnIndex = sentimentDataTable.TargetColumnIndex;

                // train a neural network on the test data
                var trainingContext = lap.NN.CreateTrainingContext(ErrorMetricType.OneHot, learningRate: 0.3f, batchSize: 8);
                trainingContext.ScheduleTrainingRateChange(10, 0.1f);
                var stackNN = lap.NN.CreateFeedForward(trainingContext.TrainNeuralNetwork(lap, trainingStack, testStack, new LayerDescriptor(0.1f) {
                    WeightUpdate = WeightUpdateType.RMSprop,
                    Activation = ActivationType.LeakyRelu,
                    WeightInitialisation = WeightInitialisationType.Xavier
                }, hiddenLayerSize:32, numEpochs:20));

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

                        var stackInput = modelStacker.Vectorise(new[] {
                            bernoulliClassifier.GetWeightedClassifications(indexList),
                            multinomialClassifier.GetWeightedClassifications(indexList),
                            neuralClassifier.GetWeightedClassifications(vector, classificationTable)
                        });
                        Console.WriteLine("Stack classification: " + dataTableVectoriser.GetOutputLabel(targetColumnIndex, stackNN.Execute(stackInput).MaximumIndex()));
                    }
                    else
                        Console.WriteLine("Sorry, none of those words have been seen before.");
                    Console.WriteLine();
                }
            }
            Console.WriteLine();
        }
    }
}
