using System;
using System.Linq;
using BrightData;
using BrightWire;
using BrightWire.Models;

namespace ExampleCode.DataTableTrainers
{
    internal class MnistTensorTrainer : DataTableTrainer
    {
        public MnistTensorTrainer(IDataTable training, IDataTable test) : base(null, training, test)
        {
        }

        public ExecutionGraphModel? TrainConvolutionalNeuralNetwork(
            uint hiddenLayerSize = 1024,
            uint numIterations = 20,
            float trainingRate = 0.1f,
            uint batchSize = 128
        ) {
            var context = Training.Context;
            var graph = context.CreateGraphFactory();
            var trainingData = graph.CreateDataSource(Training);

            // one hot encoding uses the index of the output vector's maximum value as the classification label
            var errorMetric = graph.ErrorMetric.OneHotEncoding;

            // configure the network properties
            graph.CurrentPropertySet
                .Use(graph.GradientDescent.Adam)
                .Use(graph.GaussianWeightInitialisation(false, 0.1f, GaussianVarianceCalibration.SquareRoot2N))
            ;

            // create the network
            var engine = graph.CreateTrainingEngine(trainingData, errorMetric, trainingRate, batchSize);
            //if (!String.IsNullOrWhiteSpace(outputModelPath) && File.Exists(outputModelPath)) {
            //    Console.WriteLine("Loading existing model from: " + outputModelPath);
                //using (var file = new FileStream(outputModelPath, FileMode.Open, FileAccess.Read)) {
                //    var model = Serializer.Deserialize<GraphModel>(file);
                //    engine = graph.CreateTrainingEngine(trainingData, model.Graph, LEARNING_RATE, BATCH_SIZE);
                //}
            //} else {
                graph.Connect(engine)
                 .AddConvolutional(filterCount: 16, padding: 2, filterWidth: 5, filterHeight: 5, xStride: 1, yStride: 1, shouldBackpropagate: false)
                 .Add(graph.LeakyReluActivation())
                 .AddMaxPooling(filterWidth: 2, filterHeight: 2, xStride: 2, yStride: 2)
                 .AddConvolutional(filterCount: 32, padding: 2, filterWidth: 5, filterHeight: 5, xStride: 1, yStride: 1)
                 .Add(graph.LeakyReluActivation())
                 .AddMaxPooling(filterWidth: 2, filterHeight: 2, xStride: 2, yStride: 2)
                 .TransposeFrom4DTensorToMatrix()
                 .AddFeedForward(hiddenLayerSize)
                 .Add(graph.LeakyReluActivation())
                 //.AddDropOut(dropOutPercentage: 0.5f)
                 .AddFeedForward(trainingData.GetOutputSizeOrThrow())
                 .Add(graph.SoftMaxActivation())
                 .AddBackpropagation()
                ;
            //}

            // lower the learning rate over time
            engine.LearningContext.ScheduleLearningRate(Convert.ToUInt32(numIterations * 0.75), trainingRate / 2);

            // train the network for twenty iterations, saving the model on each improvement
            ExecutionGraphModel? bestGraph = null;
            var testData = trainingData.CloneWith(Test);
            engine.Train(numIterations, testData, model => {
                bestGraph = model.Graph;
                //if (!String.IsNullOrWhiteSpace(outputModelPath)) {
                //    using (var file = new FileStream(outputModelPath, FileMode.Create, FileAccess.Write)) {
                //        Serializer.Serialize(file, model);
                //    }
                //}
            });

            // export the final model and execute it on the training set
            var executionEngine = graph.CreateExecutionEngine(bestGraph ?? engine.Graph);
            var output = executionEngine.Execute(testData);
            Console.WriteLine($"Final accuracy: {output.Average(o => o.CalculateError(errorMetric)):P2}");

            // execute the model with a single image
            var firstRow = Test[0];
            var tensor = (IReadOnlyTensor3D) firstRow[0];
            var singleData = graph.CreateDataSource(new[] { tensor.Create(context.LinearAlgebraProvider) });
            var result = executionEngine.Execute(singleData);
            var prediction = result.Single().Output[0].GetMaximumIndex();
            var expectedPrediction = ((IReadOnlyVector)firstRow[1]).GetMaximumIndex();
            Console.WriteLine($"Final model predicted: {prediction}, expected {expectedPrediction}");
            return bestGraph;
        }
    }
}
