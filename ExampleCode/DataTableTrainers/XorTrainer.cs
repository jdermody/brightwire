﻿using System;
using System.Linq;
using BrightData;
using BrightWire;
using BrightWire.Models;

namespace ExampleCode.DataTableTrainers
{
    internal class XorTrainer : DataTableTrainer
    {
        public XorTrainer(IRowOrientedDataTable table) : base(table, table, table)
        {
        }

        public ExecutionGraphModel? TrainSigmoidNeuralNetwork(uint hiddenLayerSize, uint numIterations, float learningRate, uint batchSize, bool writeResults = true)
        {
            var context = Table.Context;

            // train a model
            var graph = context.CreateGraphFactory();
            var model = graph.TrainSimpleNeuralNetwork(Training, Test, graph.ErrorMetric.BinaryClassification, learningRate, batchSize,
                hiddenLayerSize, numIterations, g => g.SigmoidActivation(), g => g.RmsProp, g => g.Gaussian);

            if (model != null) {
                // create a new network to execute the learned network
                var executionEngine = graph.CreateExecutionEngine(model);
                var testData = graph.CreateDataSource(Test);
                var output = executionEngine.Execute(testData).ToList();
                if (writeResults) {
                    var testAccuracy = output.Average(o => o.CalculateError(graph.ErrorMetric.OneHotEncoding));
                    Console.WriteLine($"Neural network accuracy: {testAccuracy:P}");

                    // print the values that have been learned
                    foreach (var item in output) {
                        foreach (var index in item.MiniBatchSequence.MiniBatch.Rows) {
                            var row = Test.Row(index);
                            var result = item.Output[index];
                            Console.WriteLine($"{row} = {result}");
                        }
                    }
                }
            }

            return model;
        }
    }
}
