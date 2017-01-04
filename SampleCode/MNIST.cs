using BrightWire.Connectionist;
using BrightWire.Connectionist.Helper;
using BrightWire.Connectionist.Training.Layer.Convolutional;
using BrightWire.Helper;
using BrightWire.Models.Convolutional;
using BrightWire.TrainingData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.SampleCode
{
    public partial class Program
    {
        /// <summary>
        /// Trains a neural net on the MNIST database (digit recognition)
        /// The data files can be downloaded from http://yann.lecun.com/exdb/mnist/
        /// </summary>
        /// <param name="dataFilesPath">The path to a directory with the four extracted data files</param>
        public static void MNIST(string dataFilesPath, string outputModelPath)
        {
            // neural network hyper parameters
            const int HIDDEN_SIZE = 1024, BATCH_SIZE = 128, NUM_EPOCHS = 40;
            const float TRAINING_RATE = 0.03f;
            var errorMetric = ErrorMetricType.OneHot.Create();
            var layerTemplate = new LayerDescriptor(0f) {
                WeightUpdate = WeightUpdateType.RMSprop,
                Activation = ActivationType.LeakyRelu
            };

            Console.Write("Loading training data...");
            var trainingData = Mnist.Load(dataFilesPath + "train-labels.idx1-ubyte", dataFilesPath + "train-images.idx3-ubyte");
            var testData = Mnist.Load(dataFilesPath + "t10k-labels.idx1-ubyte", dataFilesPath + "t10k-images.idx3-ubyte");
            Console.WriteLine("done");

            Console.WriteLine("Starting training...");
            using (var lap = GPUProvider.CreateLinearAlgebra()) {
                var trainingSet = lap.NN.CreateTrainingDataProvider(trainingData.Select(d => d.Sample).ToList());
                var testSet = lap.NN.CreateTrainingDataProvider(testData.Select(d => d.Sample).ToList());

                using (var trainer = lap.NN.CreateBatchTrainer(layerTemplate, Mnist.INPUT_SIZE, HIDDEN_SIZE, Mnist.OUTPUT_SIZE)) {
                    var trainingManager = lap.NN.CreateFeedForwardManager(trainer, outputModelPath, testSet);
                    var trainingContext = lap.NN.CreateTrainingContext(errorMetric, TRAINING_RATE, BATCH_SIZE);
                    trainingContext.ScheduleTrainingRateChange(NUM_EPOCHS / 2, TRAINING_RATE / 3);
                    trainingManager.Train(trainingSet, NUM_EPOCHS, trainingContext);
                }
            }
        }
    }
}
