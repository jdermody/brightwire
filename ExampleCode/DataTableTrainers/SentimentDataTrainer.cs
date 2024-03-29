﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BrightData;
using BrightData.Helper;
using BrightWire;
using BrightWire.ExecutionGraph;
using BrightWire.Models;
using BrightWire.Models.Bayesian;
using BrightWire.TrainingData.Helper;

namespace ExampleCode.DataTableTrainers
{
    internal class SentimentDataTrainer
    {
        readonly (string Classification, IndexList Data)[] _indexedSentencesTraining;
        readonly (string Classification, IndexList Data)[] _indexedSentencesTest;
        readonly IBrightDataContext _context;
        readonly StringTableBuilder _stringTable;
        readonly uint _maxIndex;

        public SentimentDataTrainer(IBrightDataContext context, DirectoryInfo directory)
        {
            var files = new[]
            {
                "amazon_cells_labelled.txt",
                "imdb_labelled.txt",
                "yelp_labelled.txt"
            };
            var lineSeparator = "\n".ToCharArray();
            var separator = "\t".ToCharArray();
            _stringTable = new StringTableBuilder();

            var sentences = new List<(string[] Sentence, string Classification)>();
            foreach (var path in files.Select(f => Path.Combine(directory.FullName, "sentiment labelled sentences", f)))
            {
                var data = File.ReadAllText(path)
                    .Split(lineSeparator)
                    .Where(l => !String.IsNullOrWhiteSpace(l))
                    .Select(l => l.Split(separator))
                    .Select(s => (Sentence: Tokenise(s[0]), Classification: s[1][0] == '1' ? "positive" : "negative"))
                    .Where(d => d.Sentence.Any());
                sentences.AddRange(data);
            }

            var (training, test) = sentences.Shuffle(context.Random).ToArray().Split();
            _indexedSentencesTraining = BuildIndexedClassifications(context, training, _stringTable);
            _indexedSentencesTest = BuildIndexedClassifications(context, test, _stringTable);
            _maxIndex = _indexedSentencesTraining.Concat(_indexedSentencesTest).Max(d => d.Data.Indices.Max());
            _context = context;
        }

        public StringTable StringTable => _stringTable.StringTable;

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

        public (IGraphExecutionEngine, WireBuilder, IGraphTrainingEngine) TrainNeuralNetwork(uint numIterations)
        {
            var indexer = GetIndexer();
            var trainingTable = GetTable(_context, _maxIndex, indexer, _indexedSentencesTraining);
            var testTable = GetTable(_context, _maxIndex, indexer, _indexedSentencesTest);
            var graph = _context.CreateGraphFactory();

            var trainingData = graph.CreateDataSource(trainingTable);
            var testData = graph.CreateDataSource(testTable);

            // use rmsprop gradient descent and xavier weight initialisation
            var errorMetric = graph.ErrorMetric.OneHotEncoding;
            graph.CurrentPropertySet
                .Use(graph.GradientDescent.RmsProp)
                .Use(graph.WeightInitialisation.Xavier)
            ;

            var engine = graph.CreateTrainingEngine(trainingData, errorMetric, 0.3f);
            engine.LearningContext.ScheduleLearningRate(5, 0.1f);

            var neuralNetworkWire = graph.Connect(engine)
                .AddFeedForward(512, "layer1")
                //.AddBatchNormalisation()
                .Add(graph.ReluActivation("layer1-activation"))
                //.AddDropOut(0.5f)
                .AddFeedForward(trainingData.GetOutputSizeOrThrow(), "layer2")
                .Add(graph.SoftMaxActivation("layer2-activation"))
                .AddBackpropagation("nn-bp")
            ;

            Console.WriteLine("Training neural network classifier...");
            GraphModel? bestNetwork = null;
            engine.Train(numIterations, testData, network => bestNetwork = network);
            return (engine.CreateExecutionEngine(bestNetwork?.Graph), neuralNetworkWire, engine);
        }

        public IGraphEngine StackClassifiers(IGraphTrainingEngine engine, WireBuilder neuralNetworkWire, IIndexListClassifier bernoulli, IIndexListClassifier multinomial)
        {
            // create combined data tables with both index lists and encoded vectors
            var graph = engine.LearningContext.GraphFactory;
            var context = graph.Context;
            var maxIndex = _indexedSentencesTraining.Concat(_indexedSentencesTest).Max(d => d.Data.Indices.Max());
            var indexer = GetIndexer();
            var training = CreateCombinedDataTable(context, maxIndex, indexer, _indexedSentencesTraining);
            var test = CreateCombinedDataTable(context, maxIndex, indexer, _indexedSentencesTest);
            var trainingData = graph.CreateDataSource(training, 0);
            var testData = trainingData.CloneWith(test);
            var outputSize = trainingData.GetOutputSizeOrThrow();

            // remove the last layer
            var last = neuralNetworkWire.Find("layer1-activation")!;
            var bp = neuralNetworkWire.Find("layer2")!;
            last.RemoveDirectDescendant(bp);

            // stop the backpropagation to the first neural network
            engine.LearningContext.EnableNodeUpdates(neuralNetworkWire.Find("layer1")!, false);
            engine.LearningContext.EnableNodeUpdates(neuralNetworkWire.Find("layer2")!, false);

            // create the bernoulli classifier wire
            var bernoulliWireToNode = graph.Connect(engine)
                .AddClassifier(bernoulli.AsRowClassifier(1, indexer), training, "bernoulli")
            ;

            // create the multinomial classifier wire
            var multinomialWire = graph.Connect(engine)
                .AddClassifier(multinomial.AsRowClassifier(1, indexer), training, "multinomial")
            ;

            // join the bernoulli, multinomial and neural network classification outputs
            var firstNetwork = graph.Connect(512, last);

            // train an additional classifier on the output of the previous three classifiers
            graph.Join(multinomialWire, bernoulliWireToNode, firstNetwork)
                .AddFeedForward(outputSize: 512, "layer3")
                .Add(graph.ReluActivation("layer3-activation"))
                //.AddDropOut(dropOutPercentage: 0.5f)
                .AddFeedForward(outputSize, "layer4")
                .Add(graph.SoftMaxActivation("layer4-activation"))
                .AddBackpropagation("stack-bp")
            ;

            // train the network again
            Console.WriteLine("Training stacked neural network classifier...");
            GraphModel? bestStackedNetwork = null;
            engine.Reset();
            engine.Train(20, testData, network => bestStackedNetwork = network);
            if (bestStackedNetwork != null)
                engine.LoadParametersFrom(graph, bestStackedNetwork.Graph);

            return graph.CreateExecutionEngine(engine.Graph);
        }

        static IRowOrientedDataTable GetTable(IBrightDataContext context, uint maxIndex, IIndexStrings indexer, (string Classification, IndexList Data)[] data)
        {
            var builder = context.BuildTable();
            var addColumns = true;

            var vector = new float[2];
            foreach (var (classification, indexList) in data) {
                var features = indexList.AsDense(maxIndex);
                vector[0] = vector[1] = 0f;
                vector[indexer.GetIndex(classification)] = 1f;
                if (addColumns) {
                    addColumns = false;
                    builder.AddFixedSizeVectorColumn(features.Size, "Features");
                    builder.AddFixedSizeVectorColumn((uint)vector.Length, "Target").SetTarget(true);
                }
                builder.AddRow(features, context.CreateVector(vector));
            }

            return builder.BuildRowOriented();
        }

        static string[] Tokenise(string str) => SimpleTokeniser.JoinNegations(SimpleTokeniser.Tokenise(str).Select(s => s.ToLower())).ToArray();

        static (string Classification, IndexList Data)[] BuildIndexedClassifications(IBrightDataContext context, (string[], string)[] data, StringTableBuilder stringTable)
        {
            return data
                .Select(d => (d.Item2, context.CreateIndexList(d.Item1.Select(stringTable.GetIndex).ToArray())))
                .ToArray()
            ;
        }

        static IRowOrientedDataTable CreateCombinedDataTable(IBrightDataContext context, uint maxIndex, IIndexStrings indexer, (string Classification, IndexList Data)[] data)
        {
            var builder = context.BuildTable();
            var addColumns = true;

            var vector = new float[2];
            foreach (var (classification, indexList) in data) {
                var features = indexList.AsDense(maxIndex);
                vector[0] = vector[1] = 0f;
                vector[indexer.GetIndex(classification)] = 1f;
                if (addColumns) {
                    addColumns = false;
                    builder.AddFixedSizeVectorColumn(features.Size, "Vector");
                    builder.AddColumn(BrightDataType.IndexList, "Index List");
                    builder.AddColumn(BrightDataType.String, "Target");
                    builder.AddFixedSizeVectorColumn((uint)vector.Length, "Vector Target").SetTarget(true);
                }
                builder.AddRow(features, indexList, classification, context.CreateVector(vector));
            }

            return builder.BuildRowOriented();
        }

        static IIndexStrings GetIndexer() => new StringIndexer("negative", "positive");

        public void TestClassifiers(IIndexListClassifier bernoulli, IIndexListClassifier multinomial, IGraphExecutionEngine neuralNetwork)
        {
            var empty = new float[102];
            Console.WriteLine("Enter some text to test the classifiers...");
            while (true)
            {
                Console.Write(">");
                var line = Console.ReadLine();
                if (String.IsNullOrWhiteSpace(line))
                    break;

                var tokens = Tokenise(line);
                var indices = new List<uint>();
                var embeddings = new List<float[]>();
                foreach (var token in tokens)
                {
                    if (_stringTable.TryGetIndex(token, out uint stringIndex))
                        indices.Add(stringIndex);
                    embeddings.Add(GetInputVector(0, 0, token) ?? empty);
                }
                if (indices.Any())
                {
                    var indexList = _context.CreateIndexList(indices);
                    var bc = bernoulli.Classify(indexList).First().Label;
                    var mc = multinomial.Classify(indexList).First().Label;
                    Console.WriteLine("Bernoulli classification: " + bc);
                    Console.WriteLine("Multinomial classification: " + mc);

                    // add the other classifier results into the embedding
                    foreach (var word in embeddings) {
                        word[100] = bc == "positive" ? 1f : 0f;
                        word[101] = mc == "positive" ? 1f : 0f;
                    }

                    foreach (var (token, result) in tokens.Zip(neuralNetwork.ExecuteSequential(embeddings.ToArray()), (t, r) => (Token: t, Result: r.Output.Single()))) {
                        var label = result.Softmax().MaximumIndex() == 0 ? "positive" : "negative";
                        Console.WriteLine($"{token}: {label}");
                    }
                }
                else
                    Console.WriteLine("Sorry, none of those words have been seen before.");
                Console.WriteLine();
            }
        }

        public IGraphExecutionEngine TrainBiLstm(IIndexListClassifier bernoulli, IIndexListClassifier multinomial)
        {
            var graph = _context.CreateGraphFactory();
            var trainingTable = CreateTable(_indexedSentencesTraining, bernoulli, multinomial);
            var testTable = CreateTable(_indexedSentencesTest, bernoulli, multinomial);
            var training = graph.CreateDataSource(trainingTable);
            var test = training.CloneWith(testTable);
            var errorMetric = graph.ErrorMetric.OneHotEncoding;
            var engine = graph.CreateTrainingEngine(training, errorMetric, learningRate: 0.1f, batchSize: 128);

            graph.CurrentPropertySet
                .Use(graph.Adam())
                .Use(graph.WeightInitialisation.Xavier)
            ;

            // build the network
            const int HIDDEN_LAYER_SIZE = 100;

            var forward = graph.Connect(engine)
                .AddLstm(HIDDEN_LAYER_SIZE, "forward")
            ;
            var reverse = graph.Connect(engine)
                .ReverseSequence()
                .AddLstm(HIDDEN_LAYER_SIZE, "backward")
            ;
            graph.BidirectionalJoin(forward, reverse)
                .AddFeedForward(engine.DataSource.GetOutputSizeOrThrow(), "joined")
                .Add(graph.SigmoidActivation())
                .AddBackpropagationThroughTime()
            ;

            ExecutionGraphModel? bestGraph = null;
            engine.Train(10, test, bn => bestGraph = bn.Graph);
            return engine.CreateExecutionEngine(bestGraph);
        }

        IRowOrientedDataTable CreateTable((string Classification, IndexList Data)[] data, IIndexListClassifier bernoulli, IIndexListClassifier multinomial)
        {
            var builder = _context.BuildTable();
            builder.AddColumn(BrightDataType.Matrix);
            builder.AddColumn(BrightDataType.Matrix).SetTarget(true);

            var empty = new float[102];
            foreach (var (classification, indexList) in data) {
                var c1 = bernoulli.Classify(indexList).First().Label == "positive" ? 1f : 0f;
                var c2 = multinomial.Classify(indexList).First().Label == "positive" ? 1f : 0f;
                var input = indexList.Indices.Select(i => _context.CreateVector(GetInputVector(c1, c2, _stringTable.GetString(i)) ?? empty)).ToArray();
                var output = _context.CreateMatrix((uint)input.Length, 2, (_, j) => GetOutputValue(j, classification == "positive"));
                
                builder.AddRow(_context.CreateMatrixFromRows(input), output);
            }

            return builder.BuildRowOriented();
        }

        static float[]? GetInputVector(float c1, float c2, string word)
        {
            var embedding = Data.Embeddings.Get(word);
            if(embedding == null && word.StartsWith("not_"))
                embedding = Data.Embeddings.Get(word[4..]);
            if (embedding != null) {
                var ret = new float[embedding.Length + 2];
                Array.Copy(embedding, ret, embedding.Length);
                ret[embedding.Length] = c1;
                ret[embedding.Length + 1] = c2;
                return ret;
            }

            return null;
        }

        static float GetOutputValue(uint columnIndex, bool isPositive) => columnIndex switch {
            0 when isPositive => 1f,
            1 when !isPositive => 1f,
            _ => 0f
        };
    }
}
