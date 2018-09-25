using BrightWire.TrainingData.Artificial;
using System;
using System.Collections.Generic;
using System.Linq;
using BrightWire.Models;
using BrightWire.ExecutionGraph;

namespace BrightWire.SampleCode
{
    public partial class Program
    {
        static void OneToMany()
        {
            var grammar = new SequenceGenerator(dictionarySize: 10, minSize: 5, maxSize: 5, noRepeat: true, isStochastic: false);
            var sequences = grammar.GenerateSequences().Take(1000).ToList();
            var builder = BrightWireProvider.CreateDataTableBuilder();
            builder.AddColumn(ColumnType.Vector, "Summary");
            builder.AddColumn(ColumnType.Matrix, "Sequence");

            foreach (var sequence in sequences) {
                var sequenceData = sequence
                    .GroupBy(ch => ch)
                    .Select(g => (g.Key, g.Count()))
                    .ToDictionary(d => d.Item1, d => (float)d.Item2)
                ;
                var summary = grammar.Encode(sequenceData.Select(kv => (kv.Key, kv.Value)));
                var list = new List<FloatVector>();
                foreach (var item in sequenceData.OrderBy(kv => kv.Key)) {
                    var row = grammar.Encode(item.Key, item.Value);
                    list.Add(row);
                }
                builder.Add(summary, FloatMatrix.Create(list.ToArray()));
            }
            var data = builder.Build().Split(0);
            using (var lap = BrightWireProvider.CreateLinearAlgebra(false)) {
                var graph = new GraphFactory(lap);
                var errorMetric = graph.ErrorMetric.BinaryClassification;

                // create the property set
                var propertySet = graph.CurrentPropertySet
                    .Use(graph.GradientDescent.RmsProp)
                    .Use(graph.WeightInitialisation.Xavier)
                ;

                // create the engine
                const float TRAINING_RATE = 0.1f;
                var trainingData = graph.CreateDataSource(data.Training);
                var testData = trainingData.CloneWith(data.Test);
                var engine = graph.CreateTrainingEngine(trainingData, TRAINING_RATE, 8);
                engine.LearningContext.ScheduleLearningRate(30, TRAINING_RATE / 3);

                // build the network
                const int HIDDEN_LAYER_SIZE = 128;
                graph.Connect(engine)
                    .AddLstm(HIDDEN_LAYER_SIZE)
                    .AddFeedForward(engine.DataSource.OutputSize)
                    .Add(graph.SigmoidActivation())
                    .AddBackpropagation(errorMetric)
                ;

                engine.Train(40, testData, errorMetric);

                var networkGraph = engine.Graph;
                var executionEngine = graph.CreateEngine(networkGraph);

                var output = executionEngine.Execute(testData);
                Console.WriteLine(output.Average(o => o.CalculateError(errorMetric)));
            }
        }

        static void ManyToOne()
        {
            var grammar = new SequenceGenerator(dictionarySize: 16, minSize: 3, maxSize: 6, noRepeat: true, isStochastic: false);
            var sequences = grammar.GenerateSequences().Take(1000).ToList();
            var builder = BrightWireProvider.CreateDataTableBuilder();
            builder.AddColumn(ColumnType.Matrix, "Sequence");
            builder.AddColumn(ColumnType.Vector, "Summary");

            foreach (var sequence in sequences) {
                var list = new List<FloatVector>();
                var charSet = new HashSet<char>();
                foreach (var ch in sequence) {
                    charSet.Add(ch);
                    list.Add(grammar.Encode(ch));
                }

	            var target = grammar.Encode(charSet.Select(ch2 => (ch2, 1f)));
                builder.Add(FloatMatrix.Create(list.ToArray()), target);
            }
            var data = builder.Build().Split(0);

            using (var lap = BrightWireProvider.CreateLinearAlgebra(false)) {
                var graph = new GraphFactory(lap);
                var errorMetric = graph.ErrorMetric.BinaryClassification;

                // create the property set
                var propertySet = graph.CurrentPropertySet
                    .Use(graph.GradientDescent.RmsProp)
                    .Use(graph.WeightInitialisation.Xavier)
                ;

                // create the engine
                var trainingData = graph.CreateDataSource(data.Training);
                var testData = trainingData.CloneWith(data.Test);
                var engine = graph.CreateTrainingEngine(trainingData, 0.03f, 8);

                // build the network
                const int HIDDEN_LAYER_SIZE = 128;
                graph.Connect(engine)
                    .AddGru(HIDDEN_LAYER_SIZE)
                    //.AddSimpleRecurrent(graph.ReluActivation(), memory)
                    .AddFeedForward(engine.DataSource.OutputSize)
                    .Add(graph.SigmoidActivation())
                    .AddBackpropagationThroughTime(errorMetric)
                ;

                engine.Train(20, testData, errorMetric);

                var networkGraph = engine.Graph;
                var executionEngine = graph.CreateEngine(networkGraph);

                var output = executionEngine.Execute(testData);
                Console.WriteLine(output.Where(o => o.Target != null).Average(o => o.CalculateError(errorMetric)));
            }
        }

        static void SequenceToSequence()
        {
            const int SEQUENCE_LENGTH = 5;
            var grammar = new SequenceGenerator(8, SEQUENCE_LENGTH, SEQUENCE_LENGTH, true, false);
            var sequences = grammar.GenerateSequences().Take(2000).ToList();
            var builder = BrightWireProvider.CreateDataTableBuilder();
            builder.AddColumn(ColumnType.Matrix, "Input");
            builder.AddColumn(ColumnType.Matrix, "Output");

            foreach (var sequence in sequences) {
                var encodedSequence = grammar.Encode(sequence);
                var reversedSequence = new FloatMatrix {
                    Row = encodedSequence.Row.Reverse().Take(SEQUENCE_LENGTH - 1).ToArray()
                };
                builder.Add(encodedSequence, reversedSequence);
            }
            var data = builder.Build().Split(0);

            using (var lap = BrightWireProvider.CreateLinearAlgebra()) {
                var graph = new GraphFactory(lap);
                var errorMetric = graph.ErrorMetric.BinaryClassification;

                // create the property set
                var propertySet = graph.CurrentPropertySet
                    .Use(graph.GradientDescent.RmsProp)
                    .Use(graph.WeightInitialisation.Xavier)
                ;

                const int BATCH_SIZE = 16;
                int HIDDEN_LAYER_SIZE = 64;
                const float TRAINING_RATE = 0.1f;

                // create the encoder
                var encoderLearningContext = graph.CreateLearningContext(TRAINING_RATE, BATCH_SIZE, TrainingErrorCalculation.Fast, true);
                var trainingData = graph.CreateDataSource(data.Training, encoderLearningContext, wb => wb
                    .AddLstm(HIDDEN_LAYER_SIZE, "encoder")
                    .WriteNodeMemoryToSlot("shared-memory", wb.Find("encoder") as IHaveMemoryNode)
                    .AddFeedForward(grammar.DictionarySize)
                    .Add(graph.SigmoidActivation())
                    .AddBackpropagationThroughTime(errorMetric)
                );
                var testData = trainingData.CloneWith(data.Test);

                // create the engine
                var engine = graph.CreateTrainingEngine(trainingData, TRAINING_RATE, BATCH_SIZE);
                engine.LearningContext.ScheduleLearningRate(30, TRAINING_RATE / 3);
                engine.LearningContext.ScheduleLearningRate(40, TRAINING_RATE / 9);

                // create the decoder
                var wb2 = graph.Connect(engine);
                wb2
                    .JoinInputWithMemory("shared-memory")
						.IncrementSizeBy(HIDDEN_LAYER_SIZE)
                    .AddLstm(HIDDEN_LAYER_SIZE, "decoder")
                    .AddFeedForward(trainingData.OutputSize)
                    .Add(graph.SigmoidActivation())
                    .AddBackpropagationThroughTime(errorMetric)
                ;

                engine.Train(50, testData, errorMetric);

                //var dataSourceModel = (trainingData as IAdaptiveDataSource).GetModel();
                //var testData2 = graph.CreateDataSource(data.Test, dataSourceModel);
                //var networkGraph = engine.Graph;
                //var executionEngine = graph.CreateEngine(networkGraph);

                //var output = executionEngine.Execute(testData2);
                //Console.WriteLine(output.Average(o => o.CalculateError(errorMetric)));
            }
        }
    }
}
