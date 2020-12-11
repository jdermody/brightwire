using System;
using System.Collections.Generic;
using System.Text;
using BrightTable;
using BrightWire;
using BrightWire.ExecutionGraph;

namespace ExampleCode.DataTableTrainers
{
    class IrisTrainer : DataTableTrainer
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
            var engine = graph.CreateTrainingEngine(trainingData, trainingRate, batchSize, TrainingErrorCalculation.TrainingData);
            graph.Connect(engine)
                .AddFeedForward(hiddenLayerSize)
                .Add(graph.SigmoidActivation())
                .AddDropOut(dropOutPercentage: 0.5f)
                .AddFeedForward(engine.DataSource.OutputSize ?? 0)
                .Add(graph.SigmoidActivation())
                .AddBackpropagation(errorMetric);

            // train the network
            Console.WriteLine("Training a 4x8x3 neural network...");
            engine.Train(numIterations, testData, errorMetric, null, 50);
        }
    }
}
