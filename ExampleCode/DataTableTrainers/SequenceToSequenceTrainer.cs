﻿using System;
using System.Linq;
using BrightData;
using BrightData.LinearAlgebra;
using BrightWire;
using BrightWire.Models;
using BrightWire.TrainingData.Artificial;
using MathNet.Numerics.LinearAlgebra.Complex;

namespace ExampleCode.DataTableTrainers
{
    internal class SequenceToSequenceTrainer : DataTableTrainer
    {
        readonly SequenceGenerator _sequenceGenerator;
        readonly IBrightDataContext _context;

        public SequenceToSequenceTrainer(SequenceGenerator sequenceGenerator, IBrightDataContext context, IRowOrientedDataTable dataTable) : base(dataTable)
        {
            _sequenceGenerator = sequenceGenerator;
            _context = context;
        }

        public void TrainOneToMany()
        {
            var graph = _context.CreateGraphFactory();
            var errorMetric = graph.ErrorMetric.BinaryClassification;

            // create the property set
            graph.CurrentPropertySet
                .Use(graph.GradientDescent.RmsProp)
                .Use(graph.WeightInitialisation.Xavier)
            ;

            // create the engine
            const float TRAINING_RATE = 0.03f;
            var trainingData = graph.CreateDataSource(Training);
            var testData = trainingData.CloneWith(Test);
            var engine = graph.CreateTrainingEngine(trainingData, errorMetric, TRAINING_RATE, 16);
            engine.LearningContext.ScheduleLearningRate(10, TRAINING_RATE / 3);

            // build the network
            const int HIDDEN_LAYER_SIZE = 128;
            graph.Connect(engine)
                .AddGru(HIDDEN_LAYER_SIZE)
                .AddFeedForward(engine.DataSource.GetOutputSizeOrThrow())
                .Add(graph.SigmoidActivation())
                .AddBackpropagation()
            ;

            ExecutionGraphModel? bestModel = null;
            engine.Train(15, testData, g => bestModel = g.Graph);
            var executionEngine = engine.CreateExecutionEngine(bestModel);

            var output = executionEngine.Execute(testData);
            var orderedOutput = output.OrderSequentialOutput();
            
            // convert each vector to a string index (vector index with highest value becomes the string index)
            var inputOutput = orderedOutput.Length.AsRange()
                .Select(i => (
                    Input: GetStringIndices(Test.Row(i).Get<Vector<float>>(0)),
                    Output: orderedOutput[i].Select(v => v.MaximumIndex()).ToArray()
                ))
            ;

            // write sample of results
            foreach (var item in inputOutput.Shuffle(_context.Random).Take(20)) {
                Console.WriteLine($"{_sequenceGenerator.Decode(item.Input)} => {_sequenceGenerator.Decode(item.Output)}");
            }
        }

        uint[] GetStringIndices(Vector<float> vector) => vector.Values
            .Select((v, i2) => (Value: v, Index: (uint) i2))
            .Where(d => d.Value >= 0.5)
            .Select(d => d.Index)
            .ToArray()
        ;

        public void TrainManyToOne()
        {
            var graph = _context.CreateGraphFactory();
            var errorMetric = graph.ErrorMetric.BinaryClassification;

            // create the property set
            graph.CurrentPropertySet
                .Use(graph.GradientDescent.RmsProp)
                .Use(graph.WeightInitialisation.Xavier)
            ;

            // create the engine
            var trainingData = graph.CreateDataSource(Training);
            var testData = trainingData.CloneWith(Test);
            var engine = graph.CreateTrainingEngine(trainingData, errorMetric, 0.03f, 8);

            // build the network
            const int HIDDEN_LAYER_SIZE = 128;
            graph.Connect(engine)
                .AddGru(HIDDEN_LAYER_SIZE, "encoder1")
                .AddRecurrentBridge("encoder1", "encoder2")
                .AddGru(HIDDEN_LAYER_SIZE, "encoder2")
                .AddFeedForward(engine.DataSource.GetOutputSizeOrThrow())
                .Add(graph.TanhActivation())
                .AddBackpropagationThroughTime()
            ;

            ExecutionGraphModel? bestModel = null;
            engine.Train(5, testData, g => bestModel = g.Graph);

            var executionEngine = engine.CreateExecutionEngine(bestModel);
            var output = executionEngine.Execute(testData);
            var orderedOutput = output.OrderSequentialOutput();

            // convert each vector to a string index (vector index with highest value becomes the string index)
            var inputOutput = orderedOutput.Length.AsRange()
                .Select(i => (
                    Input: Test.Row(i).Get<Matrix<float>>(0).Rows.Select(v => v.MaximumIndex()).ToArray(),
                    Output: GetStringIndices(orderedOutput[i].Last())
                ))
                .ToList()
            ;

            // write sample of results
            foreach (var item in inputOutput.Shuffle(_context.Random).Take(20)) {
                Console.WriteLine($"{_sequenceGenerator.Decode(item.Input.OrderBy(d => d).ToArray())} => {_sequenceGenerator.Decode(item.Output)}");
            }
        }

        public void TrainSequenceToSequence()
        {
            var graph = _context.CreateGraphFactory();
            var errorMetric = graph.ErrorMetric.BinaryClassification;

            // create the property set
            graph.CurrentPropertySet
                .Use(graph.GradientDescent.Adam)
                .Use(graph.GaussianWeightInitialisation(true, 0.005f))
            ;

            const uint BATCH_SIZE = 16;
            const uint HIDDEN_LAYER_SIZE = 128;
            const float TRAINING_RATE = 0.1f;

            Training.MetaData.Set("Seq2Seq", true);
            var trainingData = graph.CreateDataSource(Training);
            var testData = trainingData.CloneWith(Test);
            var engine = graph.CreateTrainingEngine(trainingData, errorMetric, TRAINING_RATE, BATCH_SIZE);

            graph.Connect(engine)
                .AddGru(HIDDEN_LAYER_SIZE, "encoder1")
                .AddRecurrentBridge("encoder1", "encoder2")
                .AddGru(HIDDEN_LAYER_SIZE, "encoder2")
                .Add(graph.ReluActivation())
                .AddSequenceToSequencePivot("encoder2", "decoder")
                .AddGru(HIDDEN_LAYER_SIZE, "decoder")
                .AddFeedForward(engine.DataSource.GetOutputSizeOrThrow())
                .Add(graph.TanhActivation())
                .AddBackpropagationThroughTime()
            ;

            // train
            ExecutionGraphModel? bestModel = null;
            engine.Train(20, testData, model => bestModel = model.Graph);

            // execute
            var executionEngine = engine.CreateExecutionEngine(bestModel);
            var testResults = executionEngine.Execute(testData).ToList();
            var orderedOutput = testResults.OrderSequentialOutput();

            // convert each vector to a string index (vector index with highest value becomes the string index)
            var inputOutput = orderedOutput.Length.AsRange()
                .Select(i => (
                    Input: Test.Row(i).Get<Matrix<float>>(0).Rows.Select(v => v.MaximumIndex()).ToArray(), 
                    Output: orderedOutput[i].Select(v => v.MaximumIndex()).ToArray()
                ))
            ;

            // write sample of results
            foreach (var item in inputOutput.Shuffle(_context.Random).Take(20)) {
                Console.WriteLine($"{_sequenceGenerator.Decode(item.Input)} => {_sequenceGenerator.Decode(item.Output.Reverse())}");
            }
        }
    }
}
