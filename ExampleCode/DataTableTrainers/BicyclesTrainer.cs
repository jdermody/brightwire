﻿using System.Threading.Tasks;
using BrightData;
using BrightWire;

namespace ExampleCode.DataTableTrainers
{
    internal class BicyclesTrainer(IDataTable table) : DataTableTrainer(table)
    {
        //public void TrainLinearModel()
        //{
        //    var lap = _context.LinearAlgebraProvider2;
        //    var trainer = Training.CreateLinearRegressionTrainer(lap);
        //    int iteration = 0;
        //    var theta = trainer.GradientDescent(500, 0.000025f, 0f, cost =>
        //    {
        //        if (iteration++ % 20 == 0)
        //            Console.WriteLine(cost);
        //        return true;
        //    });
        //    Console.WriteLine(theta.Theta);

        //    var numFeatures = Test.ColumnCount - 1;
        //    var numericColumns = numFeatures.AsRange().ToArray();
        //    var testData = Test.AsConvertible().Rows().Select(r => (Features: numericColumns.Select(r.GetTyped<float>).ToArray(), Label: r.GetTyped<float>(numFeatures)));
        //    var predictor = theta.CreatePredictor(lap);
            
        //    foreach (var (features, actual) in testData)
        //    {
        //        var prediction = predictor.Predict(features);
        //    }
        //}

        public async Task TrainNeuralNetwork()
        {
            var graph = _context.CreateGraphFactory();
            var errorMetric = graph.ErrorMetric.Quadratic;
            var trainingData = await graph.CreateDataSource(Training);
            var testData = trainingData.CloneWith(Test);
            graph.CurrentPropertySet
                .Use(graph.RmsProp())
            ;

            var engine = graph.CreateTrainingEngine(trainingData, errorMetric, 0.03f, 32);
            graph.Connect(engine)
                .AddFeedForward(16)
                .Add(graph.SigmoidActivation())
                //.AddDropOut(dropOutPercentage: 0.5f)
                .AddFeedForward(engine.DataSource.GetOutputSizeOrThrow())
                .AddBackpropagation()
            ;

            await engine.Train(50, testData);
        }
    }
}
