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
            ).Shuffle(0)/*.Take(500)*/.ToList();
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

            using (var lap = BrightWireProvider.CreateLinearAlgebra(false)) {
                var graph = new GraphFactory(lap);
                var trainingData = graph.GetDataSource(sentimentDataSet.Training, vectoriser);
                var testData = graph.GetDataSource(sentimentDataSet.Test, vectoriser);
                var indexListEncoder = trainingData as IIndexListEncoder;

                // use a one hot encoding error metric, rmsprop gradient descent and xavier weight initialisation
                var errorMetric = graph.ErrorMetric.OneHotEncoding;
                var propertySet = graph.CurrentPropertySet
                    .Use(graph.GradientDescent.Adam)
                    .Use(graph.WeightInitialisation.Xavier)
                ;

                var engine = graph.CreateTrainingEngine(trainingData, 0.01f, 128);

                // train a neural network classifier
                var neuralNetworkWire = graph.Connect(engine)
                    .AddFeedForward(512)
                    .Add(graph.ReluActivation())
                    .AddDropOut(0.3f)
                    .AddFeedForward(trainingData.OutputSize)
                    .Add(graph.ReluActivation("output-node"))
                    .AddBackpropagation(errorMetric)
                ;

                // pre train the network
                Console.WriteLine("Training neural network classifier...");
                engine.Train(10, testData, errorMetric);

                // remove the backpropagation action from the graph
                var firstNeuralNetworkOutput = neuralNetworkWire.Find("output-node");
                //firstNeuralNetworkOutput.Output.Clear();

                var firstClassifierGraph = engine.Graph;
                var firstClassifier = graph.CreateEngine(firstClassifierGraph);

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
                var joined = graph.Join(multinomialWire, graph.Join(bernoulliWire, graph.Connect(trainingData.OutputSize, firstNeuralNetworkOutput)));

                // train an additional classifier on the output of the previous three classifiers
                joined
                    .AddFeedForward(32)
                    .Add(graph.ReluActivation())
                    .AddDropOut(0.3f)
                    .AddFeedForward(trainingData.OutputSize)
                    .Add(graph.TanhActivation())
                    .AddBackpropagation(errorMetric)
                ;

                // train the network again
                Console.WriteLine("Training stacked neural network classifier...");
                engine.Train(10, testData, errorMetric);

                uint stringIndex;
                Console.WriteLine("Enter some text to test the classifiers...");
                while (true) {
                    Console.Write(">");
                    var line = Console.ReadLine();
                    if (String.IsNullOrWhiteSpace(line))
                        break;

                    var tokens = _Tokenise(line);
                    var indexList = new List<uint>();
                    foreach (var token in tokens) {
                        if (stringTable.TryGetIndex(token, out stringIndex))
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
                        var stackedClassification = vectoriser.GetOutputLabel(1, (result.Output[0].Data[0] > result.Output[0].Data[1]) ? 0 : 1);
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
