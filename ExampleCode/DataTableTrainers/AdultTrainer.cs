using System;
using BrightData;
using BrightWire;
using BrightDataTable = BrightData.DataTable.BrightDataTable;

namespace ExampleCode.DataTableTrainers
{
    class AdultTrainer : DataTableTrainer
    {
        public AdultTrainer(BrightDataTable? table, BrightDataTable training, BrightDataTable test) : base(table, training, test)
        {
        }

        public virtual void TrainNeuralNetwork()
        {
            // create a neural network graph factory
            var graph = _context.CreateGraphFactory();

            // the default data table -> vector conversion uses one hot encoding of the classification labels, so create a corresponding cost function
            var errorMetric = graph.ErrorMetric.OneHotEncoding;

            // create the property set (use rmsprop gradient descent optimisation)
            graph.CurrentPropertySet
                .Use(graph.RmsProp())
                .Use(graph.GaussianWeightInitialisation(true, 0.1f, GaussianVarianceCalibration.SquareRoot2N));

            // create the training and test data sources
            var trainingData = graph.CreateDataSource(Training);
            var testData = trainingData.CloneWith(Test);

            // create a neural network with sigmoid activations after each neural network
            var engine = graph.CreateTrainingEngine(trainingData, errorMetric, 0.1f, 128);
            graph.Connect(engine)
                .AddFeedForward(128)
                .Add(graph.LeakyReluActivation())
                .AddDropOut(0.75f)
                .AddFeedForward(engine.DataSource.GetOutputSizeOrThrow())
                .Add(graph.TanhActivation())
                .AddBackpropagation();

            // train the network
            Console.WriteLine("Training neural network...");
            engine.Train(5, testData);
        }
    }
}
