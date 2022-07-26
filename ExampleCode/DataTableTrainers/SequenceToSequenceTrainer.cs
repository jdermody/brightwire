using System;
using System.Collections.Generic;
using System.Linq;
using BrightData;
using BrightData.LinearAlgebra;
using BrightWire;
using BrightWire.Models;
using BrightWire.TrainingData.Artificial;
using BrightDataTable = BrightData.DataTable.BrightDataTable;

namespace ExampleCode.DataTableTrainers
{
    internal class SequenceToSequenceTrainer : DataTableTrainer
    {
        readonly SequenceGenerator _sequenceGenerator;

        public SequenceToSequenceTrainer(SequenceGenerator sequenceGenerator, BrightDataTable dataTable) : base(dataTable)
        {
            _sequenceGenerator = sequenceGenerator;
        }

        public void TrainOneToMany()
        {
            var graph = _context.CreateGraphFactory();
            var errorMetric = graph.ErrorMetric.BinaryClassification;

            // create the property set
            graph.CurrentPropertySet
                .Use(graph.GradientDescent.RmsProp)
                .Use(graph.GaussianWeightInitialisation(false, 0.01f, GaussianVarianceCalibration.SquareRoot2N, GaussianVarianceCount.FanInFanOut))
            ;

            // create the engine
            const float TRAINING_RATE = 0.003f;
            var trainingData = graph.CreateDataSource(Training);
            var testData = trainingData.CloneWith(Test);
            var engine = graph.CreateTrainingEngine(trainingData, errorMetric, TRAINING_RATE, 8);
            engine.LearningContext.ScheduleLearningRate(30, TRAINING_RATE / 3);

            // build the network
            const int HIDDEN_LAYER_SIZE = 128;
            graph.Connect(engine)
                .AddGru(HIDDEN_LAYER_SIZE)
                .AddFeedForward(engine.DataSource.GetOutputSizeOrThrow())
                .Add(graph.TanhActivation())
                .AddBackpropagation()
            ;

            ExecutionGraphModel? bestModel = null;
            engine.Train(40, testData, g => bestModel = g.Graph);
            var executionEngine = engine.CreateExecutionEngine(bestModel);

            var output = executionEngine.Execute(testData);
            var orderedOutput = output.OrderSequentialOutput();

            // convert each vector to a string index (vector index with highest value becomes the string index)
            var inputOutput = orderedOutput.Length.AsRange()
                .Select(i => (
                    Input: GetStringIndices(Test.Get<IReadOnlyVector>(i, 0)),
                    Output: orderedOutput[i].Select(v => v.MaximumIndex()).ToArray()
                ))
            ;

            // write sample of results
            foreach (var (input, result) in inputOutput.Shuffle(_context.Random).Take(20)) {
                Console.WriteLine($"{_sequenceGenerator.Decode(input)} => {_sequenceGenerator.Decode(result)}");
            }
        }

        static uint[] GetStringIndices(IReadOnlyVector vector)
        {
            return GetStringIndices(vector.Segment.GetLocalOrNewArray());
        }

        static uint[] GetStringIndices(float[] vector) => vector
            .Select((v, i2) => (Value: v, Index: (uint)i2))
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
                .Use(graph.GaussianWeightInitialisation(false, 0.01f, GaussianVarianceCalibration.SquareRoot2N, GaussianVarianceCount.FanInFanOut))
            ;

            // create the engine
            var trainingData = graph.CreateDataSource(Training);
            var testData = trainingData.CloneWith(Test);
            var engine = graph.CreateTrainingEngine(trainingData, errorMetric, 0.00375f, 8);

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
            engine.Train(10, testData, g => bestModel = g.Graph);

            var executionEngine = engine.CreateExecutionEngine(bestModel);
            var output = executionEngine.Execute(testData);
            var orderedOutput = output.OrderSequentialOutput();

            // convert each vector to a string index (vector index with highest value becomes the string index)
            var inputOutput = orderedOutput.Length.AsRange()
                .Select(i => {
                    var matrix = Test.Get<IReadOnlyMatrix>(i, 0);
                    return (
                        Input: matrix.AllRows(false).Select(r => r.Segment.GetMinAndMaxValues().MaxIndex).ToArray(),
                        Output: GetStringIndices(orderedOutput[i].Last())
                    );
                })
                .ToList()
            ;

            // write sample of results
            foreach (var (input, result) in inputOutput.Shuffle(_context.Random).Take(20)) {
                Console.WriteLine($"{_sequenceGenerator.Decode(input.OrderBy(d => d).ToArray())} => {_sequenceGenerator.Decode(result)}");
            }
        }

        public void TrainSequenceToSequence()
        {
            var graph = _context.CreateGraphFactory();
            var errorMetric = graph.ErrorMetric.BinaryClassification;

            // create the property set
            graph.CurrentPropertySet
                .Use(graph.GradientDescent.Adam)
                .Use(graph.GaussianWeightInitialisation(false, 0.005f, GaussianVarianceCalibration.SquareRoot2N, GaussianVarianceCount.FanInFanOut))
            ;

            const uint BATCH_SIZE = 16;
            const uint HIDDEN_LAYER_SIZE = 128;
            const float TRAINING_RATE = 0.03f;

            // indicate that this is Sequence to Sequence as the sequence lengths are the same
            Training.TableMetaData.Set("Seq2Seq", true);
            var trainingData = graph.CreateDataSource(Training);
            var testData = trainingData.CloneWith(Test);
            var engine = graph.CreateTrainingEngine(trainingData, errorMetric, TRAINING_RATE, BATCH_SIZE);

            graph.Connect(engine)
                .AddLstm(HIDDEN_LAYER_SIZE, "encoder1")
                .AddRecurrentBridge("encoder1", "encoder2")
                .AddLstm(HIDDEN_LAYER_SIZE, "encoder2")
                .Add(graph.ReluActivation())
                .AddSequenceToSequencePivot("encoder2", "decoder")
                .AddSelfAttention("encoder2", "decoder", 3, HIDDEN_LAYER_SIZE, HIDDEN_LAYER_SIZE, "self-attention")
                .AddGru(HIDDEN_LAYER_SIZE, "decoder")
                .AddFeedForward(engine.DataSource.GetOutputSizeOrThrow())
                .Add(graph.TanhActivation())
                .AddBackpropagationThroughTime()
            ;

            // train
            ExecutionGraphModel? bestModel = null;
            engine.LearningContext.ScheduleLearningRate(20, TRAINING_RATE / 3);
            engine.Train(50, testData, model => bestModel = model.Graph);

            // execute
            var executionEngine = engine.CreateExecutionEngine(bestModel);
            var testResults = executionEngine.Execute(testData).ToList();
            var orderedOutput = testResults.OrderSequentialOutput();

            var attention = testResults
                .SelectMany(ec => ec.MiniBatchSequence.MiniBatch.GetGraphContexts().SelectMany(gc => gc.GetData("self-attention")))
                .Select(d => (d.Name, Value: d.Data[0]))
                .GroupBy(d => d.Name)
                .ToDictionary(g => g.Key, d => d.Average(d2 => d2.Value));

            foreach (var item in attention)
                Console.WriteLine($"{item.Key}: {item.Value}");

            // convert each vector to a string index (vector index with highest value becomes the string index)
            var inputOutput = orderedOutput.Length.AsRange()
                .Select(i => {
                    var matrix = Test.Get<IReadOnlyMatrix>(i, 0);
                    return (
                        Input: matrix.AllRows(false).Select(r => r.Segment.GetMinAndMaxValues().MaxIndex).ToArray(),
                        Output: orderedOutput[i].Select(v => v.MaximumIndex()).ToArray()
                    );
                })
                .ToList()
            ;

            // group by index and calculate error
            var errorByIndex = new Dictionary<int, int>();
            foreach (var (input, output) in inputOutput) {
                var len = input.Length;
                var outputReversed = output.Reverse().ToList();
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
                Console.WriteLine($"{_sequenceGenerator.Decode(input)} => {_sequenceGenerator.Decode(result.Reverse())}");
            }
        }
    }
}
