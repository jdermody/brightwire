using System;
using System.Linq;
using BrightData;
using BrightWire;
using BrightWire.Models;

namespace ExampleCode.DataTableTrainers
{
    internal class MnistVectorTrainer : DataTableTrainer
    {
        public MnistVectorTrainer(IRowOrientedDataTable training, IRowOrientedDataTable test) : base(null, training, test)
        {

        }

        public ExecutionGraphModel? TrainingFeedForwardNeuralNetwork(
            uint hiddenLayerSize = 1024, 
            uint numIterations = 20, 
            float trainingRate = 0.1f,
            uint batchSize = 128
        ) {
            var graph = Training.Context.CreateGraphFactory();
            var trainingData = graph.CreateDataSource(Training);

            // one hot encoding uses the index of the output vector's maximum value as the classification label
            var errorMetric = graph.ErrorMetric.OneHotEncoding;

            // configure the network properties
            graph.CurrentPropertySet
                .Use(graph.GradientDescent.Adam)
                .Use(graph.GaussianWeightInitialisation(false, 0.1f, GaussianVarianceCalibration.SquareRoot2N))
            ;

            // create the training engine and schedule a training rate change
            var engine = graph.CreateTrainingEngine(trainingData, errorMetric, trainingRate, batchSize);
            engine.LearningContext.ScheduleLearningRate(Convert.ToUInt32(numIterations * 0.75), trainingRate / 3);

            // create the network
            graph.Connect(engine)
                .AddFeedForward(outputSize: hiddenLayerSize)
                .Add(graph.LeakyReluActivation())
                .AddDropOut(dropOutPercentage: 0.5f)
                .AddFeedForward(outputSize: trainingData.GetOutputSizeOrThrow())
                .Add(graph.SoftMaxActivation())
                .AddBackpropagation(errorMetric)
            ;

            // train the network for twenty iterations, saving the model on each improvement
            ExecutionGraphModel? bestGraph = null;
            var testData = trainingData.CloneWith(Test);
            engine.Train(numIterations, testData, errorMetric, model => bestGraph = model.Graph);

            // export the final model and execute it on the training set
            var executionEngine = graph.CreateExecutionEngine(bestGraph ?? engine.Graph);
            var output = executionEngine.Execute(testData);
            Console.WriteLine($"Final accuracy: {output.Average(o => o.CalculateError(errorMetric)):P2}");

            return bestGraph;
        }
    }
}
