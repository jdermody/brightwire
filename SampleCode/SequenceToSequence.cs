using BrightWire.Connectionist;
using BrightWire.Connectionist.Helper;
using BrightWire.Models.Input;
using BrightWire.TrainingData.Artificial;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightWire.Models;
using BrightWire.Models.Output;
using BrightWire.Helper;
using System.IO;
using ProtoBuf;

namespace BrightWire.SampleCode
{
    partial class Program
    {
        public static void SequenceClassification()
        {
            var xor = XorData.Get();
            var data = xor.Select(d => Tuple.Create(d.Input.Select(i => {
                var ret = new float[2];
                if (i == 0f)
                    ret[0] = 1f;
                else
                    ret[1] = 1f;
                return ret;
            }).ToList(), d.Output)).Select(d => new SequenceClassificationTrainingExample {
                Input = new VectorSequence {
                    Sequence = d.Item1.Select(v => new VectorSequence.Vector { Data = v }).ToArray(),
                },
                Output = d.Item2
            }).ToList();

            var layerTemplate = new LayerDescriptor(0.1f) {
                Activation = ActivationType.Tanh,
                WeightInitialisation = WeightInitialisationType.Gaussian,
                WeightUpdate = WeightUpdateType.RMSprop,
            };

            var recurrentTemplate = layerTemplate.Clone();
            recurrentTemplate.WeightInitialisation = WeightInitialisationType.Gaussian;

            const int INPUT_SIZE = 2, HIDDEN_SIZE = 8, HIDDEN_SIZE2 = 4, BATCH_SIZE = 8, OUTPUT_SIZE = 1, NUM_EPOCHS = 30;
            const float TRAINING_RATE = 0.01f;

            using (var lap = Provider.CreateLinearAlgebra()) {
                var layers = new INeuralNetworkRecurrentLayer[] {
                    lap.NN.CreateSimpleRecurrentLayer(INPUT_SIZE, HIDDEN_SIZE, recurrentTemplate),
                };
                using (var sequenceTrainer = lap.NN.CreateRecurrentBatchTrainer(layers)) {
                    var memory = Enumerable.Range(0, HIDDEN_SIZE).Select(i => 0f).ToArray();
                    var trainingContext = lap.NN.CreateTrainingContext(ErrorMetricType.OneHot, TRAINING_RATE, BATCH_SIZE);
                    var recurrentContext = lap.NN.CreateRecurrentTrainingContext(trainingContext);
                    List<Stack<SequenceBackpropagationData>> backprop = new List<Stack<SequenceBackpropagationData>>();

                    var trainingDataProvider = new SequenceClassificationTrainingDataProvider(
                        lap, data, HIDDEN_SIZE,
                        mb => {
                            var ret = sequenceTrainer.FeedForward(mb, memory, recurrentContext);
                            backprop.Add(ret);
                            return ret.Last().Output;
                        },
                        err => {
                            for(int i = 0, len = backprop.Count; i < len; i++) {
                                var bp = backprop[i];
                                var em = err[i];
                                using (var expectedOutput = bp.Last().Output.Add(em)) {
                                    bp.Last().ExpectedOutput = expectedOutput;
                                    sequenceTrainer.Backpropagate(recurrentContext, memory, bp, null, null);
                                }
                            }
                        }
                    );

                    using (var trainer = lap.NN.CreateBatchTrainer(layerTemplate, HIDDEN_SIZE, HIDDEN_SIZE2, OUTPUT_SIZE)) {
                        float bestScore = 0;
                        trainingContext.EpochComplete += c => {
                            var testError = trainer.Execute(trainingDataProvider).Select(d => trainingContext.ErrorMetric.Compute(d.Output, d.ExpectedOutput)).Average();
                            var flag = false;
                            if (testError > bestScore) {
                                bestScore = testError;
                                flag = true;
                            }
                            trainingContext.WriteScore(testError, trainingContext.ErrorMetric.DisplayAsPercentage, flag);
                        };
                        trainer.Train(trainingDataProvider, NUM_EPOCHS, trainingContext);
                    }
                }
            }
        }

        static float[] _GetOneHotOutput(string classification, Dictionary<string, int> classificationTable)
        {
            var ret = new float[classificationTable.Count];
            ret[classificationTable[classification]] = 1f;
            return ret;
        }

        public static void SequenceClassification2()
        {
            const int SEQUENCE_LENGTH = 6, HIDDEN_SIZE = 256, BATCH_SIZE = 32, NUM_EPOCHS = 100;
            const float TRAINING_RATE = 0.01f;

            var errorMetric = ErrorMetricType.OneHot.Create();

            var layerTemplate = new LayerDescriptor(0.1f) {
                Activation = ActivationType.Relu,
                WeightInitialisation = WeightInitialisationType.Gaussian,
                WeightUpdate = WeightUpdateType.RMSprop,
            };
            var recurrentTemplate = layerTemplate.Clone();
            recurrentTemplate.WeightInitialisation = WeightInitialisationType.Gaussian;

            var grammar = new ReberGrammar(false);
            var sequences = grammar.Get(SEQUENCE_LENGTH).Take(100).ToList();
            var sequenceTable = sequences.GroupBy(s => s).Select((g, i) => Tuple.Create(g.Key, i)).ToDictionary(d => d.Item1, d => d.Item2);

            var input = new List<TrainingSequence>();
            var nullOutput = new float[sequenceTable.Count];
            for (int i = 0, len = sequences.Count; i < len; i++) {
                var sequence = sequences[i];
                var vector = Encode(sequence, SEQUENCE_LENGTH, true).Sequence.Select((t, j) => new TrainingExample {
                    Input = t.Data,
                    Output = (j == SEQUENCE_LENGTH - 1) ? _GetOneHotOutput(sequence, sequenceTable) : nullOutput
                }).ToList();
                input.Add(new TrainingSequence {
                    Sequence = vector.ToArray()
                });
            }
            using (var lap = Provider.CreateLinearAlgebra(false)) {
                // create training data providers
                var trainingDataProvider = lap.NN.CreateSequentialTrainingDataProvider(input);
                var testDataProvider = lap.NN.CreateSequentialTrainingDataProvider(input);
                var layers = new INeuralNetworkRecurrentLayer[] {
                    lap.NN.CreateSimpleRecurrentLayer(trainingDataProvider.InputSize, HIDDEN_SIZE, recurrentTemplate),
                    lap.NN.CreateFeedForwardRecurrentLayer(HIDDEN_SIZE, trainingDataProvider.OutputSize, layerTemplate)
                };

                // train the network
                RecurrentNetwork networkData = null;
                using (var trainer = lap.NN.CreateRecurrentBatchTrainer(layers)) {
                    var memory = Enumerable.Range(0, HIDDEN_SIZE).Select(i => 0f).ToArray();
                    var trainingContext = lap.NN.CreateTrainingContext(errorMetric, TRAINING_RATE, BATCH_SIZE);
                    trainingContext.RecurrentEpochComplete += (tc, rtc) => {
                        var testError = trainer.Execute(testDataProvider, memory, rtc).SelectMany(s => s.Select(d => errorMetric.Compute(d.Output, d.ExpectedOutput))).Average();
                        Console.WriteLine($"Epoch {tc.CurrentEpoch} - score: {testError:P}");
                    };
                    trainer.Train(trainingDataProvider, memory, NUM_EPOCHS, lap.NN.CreateRecurrentTrainingContext(trainingContext));
                    networkData = trainer.NetworkInfo;
                    networkData.Memory = new FloatArray {
                        Data = memory
                    };
                }
            }
        }

        public static void SequenceClassification3()
        {
            const int SEQUENCE_LENGTH = 6, HIDDEN_SIZE = 256, OUTPUT_SIZE = 32, BATCH_SIZE = 32, NUM_EPOCHS = 100;
            const float TRAINING_RATE = 0.01f;

            var errorMetric = ErrorMetricType.OneHot.Create();

            var layerTemplate = new LayerDescriptor(0.1f) {
                Activation = ActivationType.Relu,
                WeightInitialisation = WeightInitialisationType.Gaussian,
                WeightUpdate = WeightUpdateType.RMSprop,
            };
            var recurrentTemplate = layerTemplate.Clone();
            recurrentTemplate.WeightInitialisation = WeightInitialisationType.Gaussian;

            var grammar = new ReberGrammar(false);
            var sequences = grammar.Get(SEQUENCE_LENGTH).Take(100).ToList();
            var sequenceTable = sequences.GroupBy(s => s).Select((g, i) => Tuple.Create(g.Key, i)).ToDictionary(d => d.Item1, d => d.Item2);

            var input = sequences.Select(s => Tuple.Create(s, Encode(s, SEQUENCE_LENGTH, true))).Select(d => new SequenceClassificationTrainingExample {
                Input = d.Item2,
                Output = _GetOneHotOutput(d.Item1, sequenceTable)
            }).ToList();

            using (var lap = Provider.CreateLinearAlgebra(false)) {
                var layers = new INeuralNetworkRecurrentLayer[] {
                    lap.NN.CreateSimpleRecurrentLayer(10, HIDDEN_SIZE, recurrentTemplate),
                    lap.NN.CreateFeedForwardRecurrentLayer(HIDDEN_SIZE, OUTPUT_SIZE, layerTemplate)
                };
                using (var sequenceTrainer = lap.NN.CreateRecurrentBatchTrainer(layers)) {
                    var memory = Enumerable.Range(0, HIDDEN_SIZE).Select(i => 0f).ToArray();
                    var trainingContext = lap.NN.CreateTrainingContext(errorMetric, TRAINING_RATE, BATCH_SIZE);
                    var recurrentContext = lap.NN.CreateRecurrentTrainingContext(trainingContext);
                    var backprop = new List<Stack<SequenceBackpropagationData>>();

                    var trainingDataProvider = new SequenceClassificationTrainingDataProvider(
                        lap, input, OUTPUT_SIZE,
                        mb => {
                            var ret = sequenceTrainer.FeedForward(mb, memory, recurrentContext);
                            backprop.Add(ret);
                            return ret.Last().Output;
                        },
                        err => {
                            for (int i = 0, len = backprop.Count; i < len; i++) {
                                var bp = backprop[i];
                                var em = err[i];
                                var empty = lap.Create(em.RowCount, em.ColumnCount, 0f);
                                var last = bp.Peek();
                                using (var expectedOutput = bp.Peek().Output.Add(em)) {
                                    foreach (var item in bp) {
                                        if (item == last)
                                            item.ExpectedOutput = expectedOutput;
                                        else
                                            item.ExpectedOutput = empty;
                                    }
                                    sequenceTrainer.Backpropagate(recurrentContext, memory, bp, null, null);
                                }
                            }
                            backprop.Clear();
                        }
                    );
                    var testDataProvider = new SequenceClassificationTrainingDataProvider(
                        lap, input, OUTPUT_SIZE,
                        mb => {
                            return sequenceTrainer.FeedForward(mb, memory, recurrentContext).Last().Output;
                        },
                        null
                    );

                    using (var trainer = lap.NN.CreateBatchTrainer(layerTemplate, OUTPUT_SIZE, sequenceTable.Count)) {
                        float bestScore = float.MaxValue;
                        trainingContext.EpochComplete += c => {
                            var testError = trainer.Execute(trainingDataProvider).Select(d => trainingContext.ErrorMetric.Compute(d.Output, d.ExpectedOutput)).Average();
                            var flag = false;
                            if (testError > bestScore) {
                                bestScore = testError;
                                flag = true;
                            }
                            trainingContext.WriteScore(testError, trainingContext.ErrorMetric.DisplayAsPercentage, flag);
                            backprop.Clear();
                        };
                        trainer.Train(trainingDataProvider, NUM_EPOCHS, trainingContext);
                    }
                }
            }
        }

        static IReadOnlyList<SequenceClassificationTrainingExample> _BuildSequenceClassification(IReadOnlyList<Tuple<uint[], string>> data, uint size)
        {
            var positiveClassification = new[] {
                1f,
            };
            var negativeClassification = new[] {
                0f
            };
            var ret = data.Select(s => new
            {
                Input = s.Item1.Select(ind => ind).ToList(),
                Output = s.Item2 == "positive" ? positiveClassification : negativeClassification
            }).ToList();
            return ret.Select(d => new SequenceClassificationTrainingExample {
                Input = new VectorSequence {
                    Sequence = d.Input.Select(ind => {
                        var vector = new float[size];
                        vector[ind] = 1f;
                        return new VectorSequence.Vector {
                            Data = vector
                        };
                    }).ToArray()
                },
                Output = d.Output
            }).ToList();
        }

        [ProtoContract]
        class SequenceModel
        {
            [ProtoMember(1)]
            public RecurrentNetwork Sequence { get; set; }

            [ProtoMember(2)]
            public FeedForwardNetwork FeedForward { get; set; }

            [ProtoMember(3)]
            public string[] StringTable { get; set; }
        }

        public static void SequenceToClassification(string dataFilesPath, string outputDataFilePath)
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
                .Select(s => Tuple.Create(_Tokenise(s[0]).Select(str => stringTable.GetIndex(str)).ToArray(), s[1][0] == '1' ? "positive" : "negative"))
                .Where(d => d.Item1.Any())
            ).Shuffle(0).ToList();
            var splitSentimentData = sentimentData.Split();

            var trainingSequences = _BuildSequenceClassification(splitSentimentData.Training, stringTable.Size);
            var testSequences = _BuildSequenceClassification(splitSentimentData.Test, stringTable.Size);

            var layerTemplate = new LayerDescriptor(0.1f) {
                Activation = ActivationType.Tanh,
                WeightInitialisation = WeightInitialisationType.Gaussian,
                WeightUpdate = WeightUpdateType.RMSprop,
            };

            var recurrentTemplate = layerTemplate.Clone();
            recurrentTemplate.WeightInitialisation = WeightInitialisationType.Gaussian;

            const int HIDDEN_SIZE = 128, BATCH_SIZE = 64, OUTPUT_SIZE = 1, NUM_EPOCHS = 10;
            const float TRAINING_RATE = 0.1f;

            using (var lap = Provider.CreateLinearAlgebra()) {
                var layers = new INeuralNetworkRecurrentLayer[] {
                    lap.NN.CreateSimpleRecurrentLayer((int)stringTable.Size, HIDDEN_SIZE, recurrentTemplate)
                };
                using (var sequenceTrainer = lap.NN.CreateRecurrentBatchTrainer(layers)) {
                    var memory = Enumerable.Range(0, HIDDEN_SIZE).Select(i => 0f).ToArray();
                    var trainingContext = lap.NN.CreateTrainingContext(ErrorMetricType.CrossEntropy, TRAINING_RATE, BATCH_SIZE);
                    var recurrentContext = lap.NN.CreateRecurrentTrainingContext(trainingContext);
                    var backprop = new List<Stack<SequenceBackpropagationData>>();

                    var trainingDataProvider = new SequenceClassificationTrainingDataProvider(
                        lap, trainingSequences, HIDDEN_SIZE,
                        mb => {
                            var ret = sequenceTrainer.FeedForward(mb, memory, recurrentContext);
                            backprop.Add(ret);
                            return ret.Last().Output;
                        },
                        err => {
                            for (int i = 0, len = backprop.Count; i < len; i++) {
                                var bp = backprop[i];
                                var em = err[i];
                                using (var expectedOutput = bp.Peek().Output.Add(em)) {
                                    bp.Peek().ExpectedOutput = expectedOutput;
                                    sequenceTrainer.Backpropagate(recurrentContext, memory, bp, null, null);
                                }
                            }
                            backprop.Clear();
                        }
                    );
                    var testDataProvider = new SequenceClassificationTrainingDataProvider(
                        lap, testSequences, HIDDEN_SIZE,
                        mb => {
                            return sequenceTrainer.FeedForward(mb, memory, recurrentContext).Last().Output;
                        },
                        err => {
                            
                        }
                    );

                    using (var trainer = lap.NN.CreateBatchTrainer(layerTemplate, HIDDEN_SIZE, OUTPUT_SIZE)) {
                        if(File.Exists(outputDataFilePath)) {
                            SequenceModel model;
                            using (var file = new FileStream(outputDataFilePath, FileMode.Open, FileAccess.Read))
                                model = Serializer.Deserialize<SequenceModel>(file);
                            sequenceTrainer.NetworkInfo = model.Sequence;
                            trainer.NetworkInfo = model.FeedForward;
                            memory = model.Sequence.Memory.Data;
                        }

                        float bestScore = float.MaxValue;
                        trainingContext.EpochComplete += c => {
                            var testError = trainer.Execute(trainingDataProvider).Select(d => trainingContext.ErrorMetric.Compute(d.Output, d.ExpectedOutput)).Average();
                            var flag = false;
                            if (testError < bestScore) {
                                bestScore = testError;
                                var bestModel = new SequenceModel {
                                    Sequence = sequenceTrainer.NetworkInfo,
                                    FeedForward = trainer.NetworkInfo,
                                    StringTable = stringTable.StringTable.Data
                                };
                                bestModel.Sequence.Memory = new FloatArray {
                                    Data = memory
                                };
                                using (var file = new FileStream(outputDataFilePath, FileMode.Create, FileAccess.Write))
                                    Serializer.Serialize(file, bestModel);
                                flag = true;
                            }
                            trainingContext.WriteScore(testError, trainingContext.ErrorMetric.DisplayAsPercentage, flag);
                            backprop.Clear();
                        };
                        trainer.Train(trainingDataProvider, NUM_EPOCHS, trainingContext);
                    }
                }
            }
        }

        public static void TestSequenceClassification(string outputDataFilePath)
        {
            SequenceModel model;
            using (var file = new FileStream(outputDataFilePath, FileMode.Open, FileAccess.Read))
                model = Serializer.Deserialize<SequenceModel>(file);
            var stringTable = model.StringTable.Select((s, i) => Tuple.Create(s, i)).ToDictionary(d => d.Item1, d => d.Item2);

            using (var lap = Provider.CreateLinearAlgebra()) {
                var sequence = lap.NN.CreateRecurrent(model.Sequence);
                var classifier = lap.NN.CreateFeedForward(model.FeedForward);

                while (true) {
                    Console.Write(">");
                    var str = Console.ReadLine();
                    if (String.IsNullOrWhiteSpace(str))
                        break;

                    var words = _Tokenise(str).Select(t => {
                        int index;
                        if (stringTable.TryGetValue(t, out index))
                            return index;
                        return -1;
                    }).Where(i => i > 0).ToArray();
                    if (words.Any()) {
                        var sentence = new List<float[]>();
                        foreach (var word in words) {
                            var vector = new float[stringTable.Count];
                            vector[word] = 1f;
                            sentence.Add(vector);
                        }
                        var output = sequence.Execute(sentence).Last().Output;
                        var classification = classifier.Execute(output.ToArray()).AsIndexable()[0];
                        Console.WriteLine(classification);
                    }
                }
            }
        }

        static int NUM_SLOTS = ReberGrammar.Size + 3;
        static int EMPTY_SLOT = ReberGrammar.Size;
        static int START_SLOT = ReberGrammar.Size + 1;
        static int END_SLOT = ReberGrammar.Size + 2;
        static VectorSequence.Vector EMPTY, START, END;

        public static VectorSequence Encode(string sequence, int maxLength, bool isInput)
        {
            var ret = new List<VectorSequence.Vector>();
            if(isInput) {
                // forward pad the input sequences
                for (var i = 0; i < maxLength - sequence.Length; i++)
                    ret.Add(EMPTY);

                // reverse the sequence order
                foreach (var ch in sequence) {
                    var vector = new float[NUM_SLOTS];
                    vector[ReberGrammar.GetIndex(ch)] = 1f;
                    ret.Add(new VectorSequence.Vector {
                        Data = vector
                    });
                }
            }else {
                // backward pad the output sequence and insert the start and end markers
                ret.Add(START);
                foreach(var ch in sequence) {
                    var vector = new float[NUM_SLOTS];
                    vector[ReberGrammar.GetIndex(ch)] = 1f;
                    ret.Add(new VectorSequence.Vector {
                        Data = vector
                    });
                }
                ret.Add(END);
                for (var i = 0; i < maxLength - sequence.Length-2; i++)
                    ret.Add(EMPTY);
            }
            Debug.Assert(ret.Count == maxLength);

            return new VectorSequence {
                Sequence = ret.ToArray()
            };
        }

        public static void SequenceToSequence()
        {
            // create some placeholder vectors for padding, start and end of sequence conditions
            EMPTY = new VectorSequence.Vector {
                Data = new float[NUM_SLOTS]
            };
            START = new VectorSequence.Vector {
                Data = new float[NUM_SLOTS]
            };
            END = new VectorSequence.Vector {
                Data = new float[NUM_SLOTS]
            };
            EMPTY.Data[EMPTY_SLOT] = 1f;
            START.Data[START_SLOT] = 1f;
            END.Data[END_SLOT] = 1f;

            var grammar = new ReberGrammar(false);
            var sequences = grammar.Get(6).Take(1000).ToList();
            var targetSequences = sequences.ToList();
            var inputMaxLength = sequences.Max(s => s.Length);
            var outputMaxLength = targetSequences.Max(s => s.Length) + 2;

            var data = sequences.Zip(targetSequences, (s, t) => new SeqeuenceToSequenceTrainingExample {
                Input = Encode(s, inputMaxLength, true),
                ExpectedOutput = Encode(t, outputMaxLength, false)
            }).Shuffle().ToList().Split();

            // neural network hyper parameters
            const int HIDDEN_SIZE = 256, NUM_EPOCHS = 100, BATCH_SIZE = 32;
            const float TRAINING_RATE = 0.01f;
            var errorMetric = ErrorMetricType.OneHot.Create();
            var layerTemplate = new LayerDescriptor(0.1f) {
                Activation = ActivationType.Relu,
                WeightInitialisation = WeightInitialisationType.Xavier,
                WeightUpdate = WeightUpdateType.RMSprop
            };
            var recurrentTemplate = layerTemplate.Clone();
            recurrentTemplate.WeightInitialisation = WeightInitialisationType.Gaussian;

            using (var lap = Provider.CreateLinearAlgebra()) {
                var encoderLayers = new INeuralNetworkRecurrentLayer[] {
                    lap.NN.CreateSimpleRecurrentLayer(NUM_SLOTS, HIDDEN_SIZE, recurrentTemplate),
                    //lap.NN.CreateFeedForwardRecurrentLayer(HIDDEN_SIZE, NUM_SLOTS, layerTemplate)
                };

                var decoderLayers = new INeuralNetworkRecurrentLayer[] {
                    lap.NN.CreateSimpleRecurrentLayer(HIDDEN_SIZE, HIDDEN_SIZE, recurrentTemplate),
                    //lap.NN.CreateFeedForwardRecurrentLayer(HIDDEN_SIZE, NUM_SLOTS, layerTemplate)
                };

                var classifier = lap.NN.CreateTrainer(lap.NN.CreateLayer(HIDDEN_SIZE, NUM_SLOTS, layerTemplate), layerTemplate);

                // disable decode updates
                //foreach(var layer in decoderLayers) {
                //    foreach (var sub in layer.SubLayer)
                //        sub.IgnoreUpdates = false;
                //}

                var trainer = new SequenceToSequenceTrainer(lap, data.Training, encoderLayers, decoderLayers, classifier, false);
                var encoderMemory = new float[HIDDEN_SIZE];
                var decoderMemory = new float[HIDDEN_SIZE];
                var trainingContext = lap.NN.CreateTrainingContext(errorMetric, TRAINING_RATE, BATCH_SIZE);
                trainingContext.EpochComplete += tc => {
                    var output = Execute(data.Test, errorMetric, encoderMemory, decoderMemory, encoderLayers, decoderLayers, lap);
                    trainingContext.WriteScore(output.Item1, true, false);
                };
                trainer.Train(encoderMemory, decoderMemory, NUM_EPOCHS, lap.NN.CreateRecurrentTrainingContext(trainingContext));

                var finalOutput = Execute(data.Test.Shuffle().Take(10).ToList(), errorMetric, encoderMemory, decoderMemory, encoderLayers, decoderLayers, lap);
                foreach(var item in finalOutput.Item2) {
                    var input = _Decode(item.Item1.Input);
                    var expectedOutput = _Decode(item.Item1.ExpectedOutput);
                    var actualOutput = item.Item2;
                    Console.WriteLine("Input: " + _Write(input));
                    Console.WriteLine("Expected output: " + _Write(expectedOutput));
                    Console.WriteLine("Actual output: " + _Write(actualOutput));
                    Console.WriteLine("-------------------------------------");
                }
            }
        }

        static string _Write(IReadOnlyList<int> output)
        {
            var sb = new StringBuilder();
            foreach(var item in output) {
                if (item == EMPTY_SLOT)
                    sb.Append("<empty>");
                else if (item == START_SLOT)
                    sb.Append("<start>");
                else if (item == END_SLOT)
                    sb.Append("<end>");
                else
                    sb.Append(ReberGrammar.GetChar(item));
            }
            return sb.ToString();
        }

        static int _MaxIndex(float[] vector)
        {
            float val = float.MinValue;
            int ret = 0;
            for(int i = 0, len = vector.Length; i < len; i++) {
                if(vector[i] > val) {
                    val = vector[i];
                    ret = i;
                }
            }
            return ret;
        }

        static IReadOnlyList<int> _Decode(VectorSequence sequence)
        {
            return sequence.Sequence.Select(s => _MaxIndex(s.Data)).ToList();
        }

        static Tuple<float, IReadOnlyList<Tuple<SeqeuenceToSequenceTrainingExample, IReadOnlyList<int>>>> Execute(
            IReadOnlyList<SeqeuenceToSequenceTrainingExample> data, 
            IErrorMetric errorMetric,
            float[] encoderMemory, 
            float[] decoderMemory, 
            INeuralNetworkRecurrentLayer[] encoderLayers, 
            INeuralNetworkRecurrentLayer[] decoderLayers,
            ILinearAlgebraProvider lap)
        {
            var encoderNetwork = new RecurrentNetwork {
                Layer = encoderLayers.Select(l => l.LayerInfo).ToArray(),
                Memory = new FloatArray {
                    Data = encoderMemory
                }
            };
            var decoderNetwork = new RecurrentNetwork {
                Layer = decoderLayers.Select(l => l.LayerInfo).ToArray(),
                Memory = new FloatArray {
                    Data = decoderMemory
                }
            };
            var encoder = lap.NN.CreateRecurrent(encoderNetwork);
            var decoder = lap.NN.CreateRecurrent(decoderNetwork);

            var scoreList = new List<float>();
            var ret = new List<Tuple<SeqeuenceToSequenceTrainingExample, IReadOnlyList<int>>>();
            foreach (var testExample in data) {
                var input = encoder.Execute(testExample.Input.Sequence.Select(s => s.Data).ToList()).Last().Output.Values.ToArray();
                var memory = decoderMemory;

                var outputList = new List<int>();
                foreach (var item in testExample.ExpectedOutput.Sequence) {
                    var output = decoder.ExecuteSingle(input, memory);
                    var score = errorMetric.Compute(output.Output, lap.CreateIndexable(item.Data.Length, i => item.Data[i]));
                    scoreList.Add(score);
                    outputList.Add(output.Output.MaximumIndex());

                    memory = output.Memory.Values.ToArray();
                    input = output.Output.Values.ToArray();
                }
                ret.Add(Tuple.Create<SeqeuenceToSequenceTrainingExample, IReadOnlyList<int>>(testExample, outputList));
            }
            var averageScore = scoreList.Average();
            return Tuple.Create<float, IReadOnlyList<Tuple<SeqeuenceToSequenceTrainingExample, IReadOnlyList<int>>>>(averageScore, ret);
        }
    }
}
