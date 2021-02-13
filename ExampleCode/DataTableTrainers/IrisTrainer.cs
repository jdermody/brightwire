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
            // create a neural network graph factory
            var graph = Table.Context.CreateGraphFactory();

            // the default data table -> vector conversion uses one hot encoding of the classification labels, so create a corresponding cost function
            var errorMetric = graph.ErrorMetric.OneHotEncoding;

            // create the property set (use rmsprop gradient descent optimisation)
            graph.CurrentPropertySet
                .Use(graph.RmsProp())
                .Use(graph.GaussianWeightInitialisation(true, 0.1f, GaussianVarianceCalibration.SquareRoot2N));

            // create the training and test data sources
            var trainingData = graph.CreateDataSource(Training);
            var testData = trainingData.CloneWith(Test);

            // create a 4x8x3 neural network with sigmoid activations after each neural network
            var engine = graph.CreateTrainingEngine(trainingData, errorMetric, trainingRate, batchSize);
            graph.Connect(engine)
                .AddFeedForward(hiddenLayerSize)
                .Add(graph.SigmoidActivation())
                .AddDropOut(dropOutPercentage: 0.5f)
                .AddFeedForward(engine.DataSource.GetOutputSizeOrThrow())
                .Add(graph.SigmoidActivation())
                .AddBackpropagation();

            // train the network
            Console.WriteLine("Training a 4x8x3 neural network...");
            engine.Train(numIterations, testData, null, 50);
        }

        public void TrainWithSelu(uint numIterations = 1000, uint layerSize = 8, float trainingRate = 0.1f, uint batchSize = 64)
        {
            var graph = Table.Context.CreateGraphFactory();
            var trainingData = graph.CreateDataSource(Training);
            var testData = trainingData.CloneWith(Test);

            // one hot encoding uses the index of the output vector's maximum value as the classification label
            var errorMetric = graph.ErrorMetric.Quadratic;

            // configure the network properties
            graph.CurrentPropertySet
                .Use(graph.GradientDescent.RmsProp)
                .Use(graph.GaussianWeightInitialisation(true, 0.1f, GaussianVarianceCalibration.SquareRoot2N, GaussianVarianceCount.FanInFanOut));

            // create the training engine and schedule a training rate change
            var engine = graph.CreateTrainingEngine(trainingData, errorMetric, trainingRate, batchSize);

            static NodeBase Activation() => new SeluActivation();
            //Func<NodeBase> Activation = () => graph.ReluActivation();

            // create the network with the custom activation function
            graph.Connect(engine)
                .AddFeedForward(layerSize)
                //.AddBatchNormalisation()
                .Add(Activation())
                .AddFeedForward(layerSize)
                //.AddBatchNormalisation()
                .Add(Activation())
                .AddFeedForward(layerSize)
                //.AddBatchNormalisation()
                .Add(Activation())
                .AddFeedForward(layerSize)
                //.AddBatchNormalisation()
                .Add(Activation())
                .AddFeedForward(layerSize)
                //.AddBatchNormalisation()
                .Add(Activation())
                .AddFeedForward(layerSize)
                //.AddBatchNormalisation()
                .Add(Activation())
                .AddFeedForward(trainingData.GetOutputSizeOrThrow())
                .Add(graph.SoftMaxActivation())
                .AddBackpropagation()
            ;

            engine.Train(numIterations, testData, null, 50);
        }
    }
}
