using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using BrightData;
using BrightTable;
using BrightWire;

namespace ExampleCode.DataTableTrainers
{
    class BicyclesTrainer : DataTableTrainer
    {
        private readonly IBrightDataContext _context;

        public BicyclesTrainer(IBrightDataContext context, IRowOrientedDataTable table) : base(table)
        {
            _context = context;
        }

        public void TrainLinearModel()
        {
            var lap = _context.LinearAlgebraProvider;
            var trainer = Training.CreateLinearRegressionTrainer(lap);
            int iteration = 0;
            var theta = trainer.GradientDescent(500, 0.000025f, 0f, cost =>
            {
                if (iteration++ % 20 == 0)
                    Console.WriteLine(cost);
                return true;
            });
            Console.WriteLine(theta.Theta);

            var numFeatures = Test.ColumnCount - 1;
            var numericColumns = numFeatures.AsRange().ToArray();
            var testData = Test.AsConvertible().Rows().Select(r => (Features: numericColumns.Select(r.GetTyped<float>).ToArray(), Label: (float)r.GetTyped<float>(numFeatures)));
            var predictor = theta.CreatePredictor(lap);
            int index = 0;
            foreach (var row in testData)
            {
                var prediction = predictor.Predict(row.Features);
                var actual = row.Label;
            }
        }

        public void TrainNeuralNetwork()
        {
            var graph = _context.CreateGraphFactory();
            var errorMetric = graph.ErrorMetric.Quadratic;
            var trainingData = graph.CreateDataSource(Training);
            var testData = trainingData.CloneWith(Test);
            graph.CurrentPropertySet
                .Use(graph.RmsProp())
            ;

            var engine = graph.CreateTrainingEngine(trainingData, 0.1f, 32);
            graph.Connect(engine)
                .AddFeedForward(16)
                .Add(graph.SigmoidActivation())
                //.AddDropOut(dropOutPercentage: 0.5f)
                .AddFeedForward(engine.DataSource.GetOutputSizeOrThrow())
                .AddBackpropagation(errorMetric)
            ;

            engine.Train(100, testData, errorMetric);
        }
    }
}
