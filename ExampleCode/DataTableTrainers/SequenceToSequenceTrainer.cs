using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BrightData;
using BrightData.LinearAlgebra.ReadOnly;
using BrightWire;
using BrightWire.Models;
using BrightWire.TrainingData.Artificial;

namespace ExampleCode.DataTableTrainers
{
    internal class SequenceToSequenceTrainer(SequenceGenerator sequenceGenerator, IDataTable dataTable) : DataTableTrainer(dataTable)
    {
        public async Task TrainOneToMany()
        {
            var graph = _context.CreateGraphFactory();
            var errorMetric = graph.ErrorMetric.BinaryClassification;

            // create the property set
            graph.CurrentPropertySet
                .Use(graph.GradientDescent.RmsProp)
                .Use(graph.GaussianWeightInitialisation(false, 0.01f, GaussianVarianceCalibration.SquareRoot2N, GaussianVarianceCount.FanInFanOut))
            ;

            // create the engine
            const float trainingRate = 0.003f;
            var trainingData = graph.CreateDataSource(Training);
            var testData = trainingData.CloneWith(Test);
            var engine = graph.CreateTrainingEngine(trainingData, errorMetric, trainingRate, 8);
            engine.LearningContext.ScheduleLearningRate(30, trainingRate / 3);

            // build the network
            const int hiddenLayerSize = 128;
            graph.Connect(engine)
                .AddGru(hiddenLayerSize)
                .AddFeedForward(engine.DataSource.GetOutputSizeOrThrow())
                .Add(graph.TanhActivation())
                .AddBackpropagation()
            ;

            ExecutionGraphModel? bestModel = null;
            await engine.Train(40, testData, g => bestModel = g.Graph);
            var executionEngine = engine.CreateExecutionEngine(bestModel);

            var output = await executionEngine.Execute(testData).ToListAsync();
            var orderedOutput = output.OrderSequentialOutput();

            // convert each vector to a string index (vector index with the highest value becomes the string index)
            var inputOutput = orderedOutput.Length.AsRange()
                .Select(i => (
                    Input: GetStringIndices(Test.Get<ReadOnlyVector>(0, i).Result),
                    Output: orderedOutput[i].Select(v => v.GetMaximumIndex()).ToArray()
                ))
            ;

            // write sample of results
            foreach (var (input, result) in inputOutput.Shuffle(_context.Random).Take(20)) {
                Console.WriteLine($"{sequenceGenerator.Decode(input)} => {sequenceGenerator.Decode(result)}");
            }
        }

        static uint[] GetStringIndices(IReadOnlyVector vector)
        {
            return GetStringIndices(vector.ReadOnlySegment.ToNewArray());
        }

        static uint[] GetStringIndices(float[] vector) => vector
            .Select((v, i2) => (Value: v, Index: (uint)i2))
            .Where(d => d.Value >= 0.5)
            .Select(d => d.Index)
            .ToArray()
        ;

        public async Task TrainManyToOne()
        {
            var graph = _context.CreateGraphFactory();
            var errorMetric = graph.ErrorMetric.BinaryClassification;

            // create the property set
            graph.CurrentPropertySet
                .Use(graph.GradientDescent.RmsProp)
                .Use(graph.GaussianWeightInitialisation(false, 0.01f, GaussianVarianceCalibration.SquareRoot2N, GaussianVarianceCount.FanInFanOut))
            ;

            // create the engine
            var trainingData = graph.CreateDataSource(Training);
            var testData = trainingData.CloneWith(Test);
            var engine = graph.CreateTrainingEngine(trainingData, errorMetric, 0.00375f, 8);

            // build the network
            const int hiddenLayerSize = 128;
            graph.Connect(engine)
                .AddGru(hiddenLayerSize, "encoder1")
                .AddRecurrentBridge("encoder1", "encoder2")
                .AddGru(hiddenLayerSize, "encoder2")
                .AddFeedForward(engine.DataSource.GetOutputSizeOrThrow())
                .Add(graph.TanhActivation())
                .AddBackpropagationThroughTime()
            ;

            ExecutionGraphModel? bestModel = null;
            await engine.Train(10, testData, g => bestModel = g.Graph);

            var executionEngine = engine.CreateExecutionEngine(bestModel);
            var output = await executionEngine.Execute(testData).ToListAsync();
            var orderedOutput = output.OrderSequentialOutput();

            // convert each vector to a string index (vector index with the highest value becomes the string index)
            var inputOutput = orderedOutput.Length.AsRange()
                .Select(i => {
                    var matrix = Test.Get<ReadOnlyMatrix>(0, i).Result;
                    return (
                        Input: matrix.RowCount.AsRange().Select(x => matrix.GetReadOnlyRow(x).GetMaximumIndex()).ToArray(),
                        Output: GetStringIndices(orderedOutput[i].Last())
                    );
                })
                .ToList()
            ;

            // write sample of results
            foreach (var (input, result) in inputOutput.Shuffle(_context.Random).Take(20)) {
                Console.WriteLine($"{sequenceGenerator.Decode(input.OrderBy(d => d).ToArray())} => {sequenceGenerator.Decode(result)}");
            }
        }

        public async Task TrainSequenceToSequence()
        {
            var graph = _context.CreateGraphFactory();
            var errorMetric = graph.ErrorMetric.BinaryClassification;

            // create the property set
            graph.CurrentPropertySet
                .Use(graph.GradientDescent.Adam)
                .Use(graph.GaussianWeightInitialisation(false, 0.005f, GaussianVarianceCalibration.SquareRoot2N, GaussianVarianceCount.FanInFanOut))
            ;

            const uint batchSize = 8;
            const uint hiddenLayerSize = 32;
            const float trainingRate = 0.01f;

            // indicate that this is Sequence to Sequence as the sequence lengths are the same
            Training.MetaData.Set("Seq2Seq", true);

            var trainingData = graph.CreateDataSource(Training);
            var testData = trainingData.CloneWith(Test);
            var engine = graph.CreateTrainingEngine(trainingData, errorMetric, trainingRate, batchSize);

            graph.Connect(engine)
                .AddLstm(hiddenLayerSize, "encoder1")
                .AddRecurrentBridge("encoder1", "encoder2")
                .AddLstm(hiddenLayerSize, "encoder2")
                .Add(graph.ReluActivation())
                .AddSequenceToSequencePivot("encoder2", "decoder")
                //.AddSelfAttention("encoder2", "decoder", HIDDEN_LAYER_SIZE, HIDDEN_LAYER_SIZE, "self-attention")
                .AddGru(hiddenLayerSize, "decoder")
                .AddRecurrentBridge("decoder", "decoder2")
                .AddGru(hiddenLayerSize, "decoder2")
                .AddFeedForward(engine.DataSource.GetOutputSizeOrThrow())
                .Add(graph.TanhActivation())
                .AddBackpropagationThroughTime()
            ;

            // train
            ExecutionGraphModel? bestModel = null;
            engine.LearningContext.ScheduleLearningRate(30, trainingRate / 3);
            await engine.Train(60, testData, model => bestModel = model.Graph);

            // execute
            var executionEngine = engine.CreateExecutionEngine(bestModel);
            var testResults = await executionEngine.Execute(testData).ToListAsync();
            var orderedOutput = testResults.OrderSequentialOutput();

            var attention = testResults
                .SelectMany(ec => ec.MiniBatchSequence.MiniBatch.GetGraphContexts().SelectMany(gc => gc.GetData("self-attention")))
                .Select(d => (d.Name, Value: d.Data[0]))
                .GroupBy(d => d.Name)
                .ToDictionary(g => g.Key, d => d.Average(d2 => d2.Value));

            foreach (var item in attention)
                Console.WriteLine($"{item.Key}: {item.Value}");

            // convert each vector to a string index (vector index with the highest value becomes the string index)
            var inputOutput = orderedOutput.Length.AsRange()
                .Select(i => {
                    var matrix = Test.Get<ReadOnlyMatrix>(0, i).Result;
                    return (
                        Input: matrix.RowCount.AsRange().Select(x => matrix.GetReadOnlyRow(x).GetMaximumIndex()).ToArray(),
                        Output: orderedOutput[i].Select(v => v.GetMaximumIndex()).ToArray()
                    );
                })
                .ToList()
            ;

            // group by index and calculate error
            var errorByIndex = new Dictionary<int, int>();
            foreach (var (input, output) in inputOutput) {
                var outputReversed = output.Reverse().ToList();
                var len = Math.Min(input.Length, outputReversed.Count);
                for (var i = 0; i < len; i++) {
                    if (input[i] != outputReversed[i]) {
                        if (errorByIndex.TryGetValue(i, out var error))
                            errorByIndex[i] = error + 1;
                        else
                            errorByIndex[i] = 1;
                    }
                }
            }
            foreach (var item in errorByIndex.OrderBy(d => d.Key))
                Console.WriteLine($"Index {item.Key}: {item.Value:N0} errors");

            // write sample of results
            foreach (var (input, result) in inputOutput.Shuffle(_context.Random).Take(20)) {
                Console.WriteLine($"{sequenceGenerator.Decode(input)} => {sequenceGenerator.Decode(result.Reverse())}");
            }
        }
    }
}
