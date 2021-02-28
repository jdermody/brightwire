using System;
using BrightData;
using BrightWire;
using BrightWire.ExecutionGraph.Node;
using ExampleCode.Extensions;

namespace ExampleCode.DataTableTrainers
{
    internal class IrisTrainer : DataTableTrainer
    {
        public string[] Labels { get; }

        public IrisTrainer(IRowOrientedDataTable table, string[] labels) : base(table)
        {
            Labels = labels;
        }

        public void TrainSigmoidNeuralNetwork(uint hiddenLayerSize, uint numIterations, float trainingRate, uint batchSize)
        {
            base.TrainSigmoidNeuralNetwork(hiddenLayerSize, numIterations, trainingRate, batchSize, 50);
        }

        public void TrainWithSelu(uint numIterations = 1000, uint layerSize = 8, float trainingRate = 0.01f, uint batchSize = 32)
        {
            var graph = Table.Context.CreateGraphFactory();
            var trainingData = graph.CreateDataSource(Training);
            var testData = trainingData.CloneWith(Test);

            // one hot encoding uses the index of the output vector's maximum value as the classification label
            var errorMetric = graph.ErrorMetric.CrossEntropy;

            // configure the network properties
            graph.CurrentPropertySet
                .Use(graph.GradientDescent.RmsProp)
                .Use(graph.GaussianWeightInitialisation(false, 0.1f, GaussianVarianceCalibration.SquareRoot2N, GaussianVarianceCount.FanInFanOut));

            // create the training engine and schedule a training rate change
            var engine = graph.CreateTrainingEngine(trainingData, errorMetric, trainingRate, batchSize);

            static NodeBase Activation() => new SeluActivation();
            //Func<NodeBase> Activation = () => graph.ReluActivation();

            // create the network with the custom activation function
            graph.Connect(engine)
                .AddFeedForward(layerSize)
                .Add(Activation())
                .AddFeedForward(layerSize)
                .Add(Activation())
                .AddFeedForward(layerSize)
                .Add(Activation())
                .AddFeedForward(layerSize)
                .Add(Activation())
                .AddFeedForward(trainingData.GetOutputSizeOrThrow())
                .Add(graph.SoftMaxActivation())
                .AddBackpropagation()
            ;

            engine.Train(numIterations, testData, null, 50);
        }
    }
}
