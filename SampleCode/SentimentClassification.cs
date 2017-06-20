using BrightWire.ExecutionGraph;
using BrightWire.ExecutionGraph.Action;
using BrightWire.Models;
using BrightWire.TrainingData;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.SampleCode
{
    public partial class Program
    {
        static IReadOnlyList<(string Classification, IndexList Data)> _BuildIndexedClassifications(IReadOnlyList<Tuple<string[], string>> data, StringTableBuilder stringTable)
        {
            return data
                .Select(d => (d.Item2, new IndexList { Index = d.Item1.Select(str => stringTable.GetIndex(str)).ToArray() }))
                .ToList()
            ;
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
            var trainingClassificationBag = _BuildIndexedClassifications(splitSentimentData.Training, stringTable);
            var testClassificationBag = _BuildIndexedClassifications(splitSentimentData.Test, stringTable);

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

            // convert the index lists to vectors
            var sentimentDataBag = _BuildIndexedClassifications(sentimentData, stringTable);
            var sentimentDataTable = sentimentDataBag.ConvertToTable();
            var vectoriser = sentimentDataTable.GetVectoriser();
            var sentimentDataSet = sentimentDataTable.Split(0);

            using (var lap = BrightWireProvider.CreateLinearAlgebra()) {
                var graph = new GraphFactory(lap);
                var trainingData = graph.CreateDataSource(sentimentDataSet.Training, vectoriser);
                var testData = graph.CreateDataSource(sentimentDataSet.Test, vectoriser);
                var indexListEncoder = trainingData as IIndexListEncoder;

                // use a one hot encoding error metric, rmsprop gradient descent and xavier weight initialisation
                var errorMetric = graph.ErrorMetric.OneHotEncoding;
                var propertySet = graph.CurrentPropertySet
                    .Use(graph.GradientDescent.RmsProp)
                    .Use(graph.WeightInitialisation.Xavier)
                ;

                var engine = graph.CreateTrainingEngine(trainingData, 0.3f, 128);
                engine.LearningContext.ScheduleLearningRate(5, 0.1f);
                engine.LearningContext.ScheduleLearningRate(11, 1f);
                engine.LearningContext.ScheduleLearningRate(15, 0.3f);

                // train a neural network classifier
                var neuralNetworkWire = graph.Connect(engine)
                    .AddFeedForward(512, "layer1")
                    .Add(graph.ReluActivation())
                    .AddDropOut(0.5f)
                    .AddFeedForward(trainingData.OutputSize, "layer2")
                    .Add(graph.ReluActivation())
                    .AddBackpropagation(errorMetric, "first-network")
                ;

                // train the network
                Console.WriteLine("Training neural network classifier...");
                GraphModel bestNetwork = null;
                engine.Train(10, testData, errorMetric, network => bestNetwork = network);
                if (bestNetwork != null)
                    engine.LoadParametersFrom(bestNetwork.Graph);
                var firstClassifier = graph.CreateEngine(engine.Graph);

                // stop the backpropagation to the first neural network
                engine.LearningContext.EnableNodeUpdates(neuralNetworkWire.Find("layer1"), false);
                engine.LearningContext.EnableNodeUpdates(neuralNetworkWire.Find("layer2"), false);

                // create the bernoulli classifier wire
                var bernoulliClassifier = bernoulli.CreateClassifier();
                var bernoulliWire = graph.Connect(engine)
                    .AddClassifier(bernoulliClassifier, sentimentDataSet.Training, vectoriser.Analysis)
                ;

                // create the multinomial classifier wire
                var multinomialClassifier = multinomial.CreateClassifier();
                var multinomialWire = graph.Connect(engine)
                    .AddClassifier(multinomialClassifier, sentimentDataSet.Training, vectoriser.Analysis)
                ;

                // join the bernoulli, multinomial and neural network classification outputs
                var firstNetwork = neuralNetworkWire.Find("first-network");
                var joined = graph.Join(multinomialWire, graph.Join(bernoulliWire, graph.Connect(trainingData.OutputSize, firstNetwork)));

                // train an additional classifier on the output of the previous three classifiers
                joined
                    .AddFeedForward(32)
                    .Add(graph.ReluActivation())
                    .AddDropOut(0.5f)
                    .AddFeedForward(trainingData.OutputSize)
                    .Add(graph.ReluActivation())
                    .AddBackpropagation(errorMetric)
                ;

                // train the network again
                Console.WriteLine("Training stacked neural network classifier...");
                GraphModel bestStackedNetwork = null;
                engine.Train(10, testData, errorMetric, network => bestStackedNetwork = network);
                if (bestStackedNetwork != null)
                    engine.LoadParametersFrom(bestStackedNetwork.Graph);

                Console.WriteLine("Enter some text to test the classifiers...");
                while (true) {
                    Console.Write(">");
                    var line = Console.ReadLine();
                    if (String.IsNullOrWhiteSpace(line))
                        break;

                    var tokens = _Tokenise(line);
                    var indexList = new List<uint>();
                    foreach (var token in tokens) {
                        if (stringTable.TryGetIndex(token, out uint stringIndex))
                            indexList.Add(stringIndex);
                    }
                    if (indexList.Any()) {
                        var queryTokens = indexList.GroupBy(d => d).Select(g => Tuple.Create(g.Key, (float)g.Count())).ToList();
                        var vector = new float[trainingData.InputSize];
                        foreach (var token in queryTokens)
                            vector[token.Item1] = token.Item2;
                        var indexList2 = new IndexList {
                            Index = indexList.ToArray()
                        };
                        var encodedInput = indexListEncoder.Encode(indexList2);

                        Console.WriteLine("Bernoulli classification: " + bernoulliClassifier.Classify(indexList2).First().Label);
                        Console.WriteLine("Multinomial classification: " + multinomialClassifier.Classify(indexList2).First().Label);
                        var result = firstClassifier.Execute(encodedInput);
                        var classification = vectoriser.GetOutputLabel(1, (result.Output[0].Data[0] > result.Output[0].Data[1]) ? 0 : 1);
                        Console.WriteLine("Neural network classification: " + classification);

                        var stackedResult = engine.Execute(encodedInput);
                        var stackedClassification = vectoriser.GetOutputLabel(1, (stackedResult.Output[0].Data[0] > stackedResult.Output[0].Data[1]) ? 0 : 1);
                        Console.WriteLine("Stack classification: " + stackedClassification);
                    } else
                        Console.WriteLine("Sorry, none of those words have been seen before.");
                    Console.WriteLine();
                }
            }
            Console.WriteLine();
        }
    }
}
