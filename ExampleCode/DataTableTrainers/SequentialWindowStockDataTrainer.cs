﻿using System;
using System.Linq;
using BrightData;
using BrightData.LinearAlgebra;
using BrightWire;
using BrightWire.Models;

namespace ExampleCode.DataTableTrainers
{
    internal class SequentialWindowStockDataTrainer : DataTableTrainer
    {
        public SequentialWindowStockDataTrainer(IRowOrientedDataTable table) : base(table)
        {
        }

        public void TrainLstm(uint hiddenLayerSize)
        {
            var graph = Table.Context.CreateGraphFactory();
            var errorMetric = graph.ErrorMetric.Quadratic;

            // create the property set
            graph.CurrentPropertySet
                .Use(graph.GradientDescent.Adam)
                .Use(graph.WeightInitialisation.Xavier);

            // create the engine
            var trainingData = graph.CreateDataSource(Training);
            var testData = trainingData.CloneWith(Test);
            var engine = graph.CreateTrainingEngine(trainingData, errorMetric, learningRate: 0.03f, batchSize: 128);

            // build the network
            graph.Connect(engine)
                .AddLstm(hiddenLayerSize)
                .AddFeedForward(engine.DataSource.GetOutputSizeOrThrow())
                .Add(graph.TanhActivation())
                .AddBackpropagationThroughTime();

            // train the network and restore the best result
            GraphModel? bestNetwork = null;
            engine.Train(20, testData, model => bestNetwork = model);
            if (bestNetwork != null) {
                // execute each row of the test data on an execution engine
                var executionEngine = graph.CreateExecutionEngine(bestNetwork.Graph);
                var results = executionEngine.Execute(testData).OrderSequentialOutput();
                var expectedOutput = Test.Column<Vector<float>>(1).ToArray();

                var score = results.Select((r, i) => errorMetric.Compute(r.Last(), expectedOutput[i])).Average();
                Console.WriteLine($"Final quadratic prediction error: {score}");
            }
        }
    }
}
