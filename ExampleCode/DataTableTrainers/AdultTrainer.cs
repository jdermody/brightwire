using System;
using System.Threading.Tasks;
using BrightData;
using BrightWire;

namespace ExampleCode.DataTableTrainers
{
    class AdultTrainer(IDataTable? table, IDataTable training, IDataTable test) : DataTableTrainer(table, training, test)
    {
        public virtual Task TrainNeuralNetwork()
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
            var engine = graph.CreateTrainingEngine(trainingData, errorMetric, learningRate:0.03f, batchSize:128);
            graph.Connect(engine)
                .AddFeedForward(128)
                .Add(graph.TanhActivation())
                //.AddDropOut(0.75f)
                .AddFeedForward(engine.DataSource.GetOutputSizeOrThrow())
                .Add(graph.TanhActivation())
                .AddBackpropagation();

            // train the network
            Console.WriteLine("Training neural network...");
            return engine.Train(20, testData);
        }
    }
}
