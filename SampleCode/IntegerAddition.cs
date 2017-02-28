using BrightWire.Connectionist;
using BrightWire.Models;
using BrightWire.TrainingData.Artificial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.SampleCode
{
    public partial class Program
    {
        static void _WriteBinary(float value)
        {
            if (value >= 0.5)
                Console.Write("1");
            else
                Console.Write("0");
        }

        public static void IntegerAddition()
        {
            // generate 1000 random integer additions
            var dataSet = BinaryIntegers.Addition(1000, false);

            // split the numbers into training and test sets
            int split = Convert.ToInt32(dataSet.Count * 0.8);
            var trainingData = dataSet.Take(split).ToList();
            var testData = dataSet.Skip(split).ToList();

            // neural network hyper parameters
            const int HIDDEN_SIZE = 32, NUM_EPOCHS = 100, BATCH_SIZE = 8;
            const float TRAINING_RATE = 0.001f;
            var errorMetric = ErrorMetricType.BinaryClassification.Create();
            var layerTemplate = new LayerDescriptor(0.3f) {
                Activation = ActivationType.Relu,
                WeightInitialisation = WeightInitialisationType.Xavier,
                WeightUpdate = WeightUpdateType.RMSprop
            };
            var recurrentTemplate = layerTemplate.Clone();
            recurrentTemplate.WeightInitialisation = WeightInitialisationType.Xavier;

            using (var lap = Provider.CreateLinearAlgebra()) {
                // create training data providers
                var trainingDataProvider = lap.NN.CreateSequentialTrainingDataProvider(trainingData);
                var testDataProvider = lap.NN.CreateSequentialTrainingDataProvider(testData);
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

                // evaluate the network on some freshly generated data
                var network = lap.NN.CreateRecurrent(networkData);
                foreach (var item in BinaryIntegers.Addition(8, true)) {
                    var result = network.Execute(item.Sequence.Select(d => d.Input).ToList());
                    Console.Write("First:     ");
                    foreach (var item2 in item.Sequence)
                        _WriteBinary(item2.Input[0]);
                    Console.WriteLine();

                    Console.Write("Second:    ");
                    foreach (var item2 in item.Sequence)
                        _WriteBinary(item2.Input[1]);
                    Console.WriteLine();
                    Console.WriteLine("           --------------------------------");

                    Console.Write("Expected:  ");
                    foreach (var item2 in item.Sequence)
                        _WriteBinary(item2.Output[0]);
                    Console.WriteLine();

                    Console.Write("Predicted: ");
                    foreach (var item2 in result)
                        _WriteBinary(item2.Output[0]);
                    Console.WriteLine();
                    Console.WriteLine();
                }
            }
        }
    }
}
