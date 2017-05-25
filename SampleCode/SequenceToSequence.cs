using BrightWire.TrainingData.Artificial;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightWire.Models;
using BrightWire.Helper;
using System.IO;
using ProtoBuf;
using BrightWire.ExecutionGraph;
using BrightWire.ExecutionGraph.Action;

namespace BrightWire.SampleCode
{
    public partial class Program
    {
        static void OneToMany()
        {
            var grammar = new SequenceClassification(10, 5, 5, true, false);
            var sequences = grammar.GenerateSequences().Take(100).ToList();
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
                builder.Add(summary, new FloatMatrix { Row = list.ToArray() });
            }
            var data = builder.Build().Split(0);
            using (var lap = BrightWireProvider.CreateLinearAlgebra(false)) {
                var graph = new GraphFactory(lap);
                var errorMetric = graph.ErrorMetric.OneHotEncoding;

                // create the property set
                var propertySet = graph.CurrentPropertySet
                    .Use(graph.RmsProp())
                    .Use(graph.WeightInitialisation.Xavier)
                ;

                // create the engine
                var trainingData = graph.GetDataSource(data.Training);
                var testData = trainingData.CloneWith(data.Test);
                var engine = graph.CreateTrainingEngine(trainingData, 0.003f, 8);

                // build the network
                const int HIDDEN_LAYER_SIZE = 128;
                var memory = new float[HIDDEN_LAYER_SIZE];
                var network = graph.Connect(engine)
                    //.AddSimpleRecurrent(graph.ReluActivation(), memory)
                    .AddGru(memory)
                    .AddFeedForward(engine.DataSource.OutputSize)
                    .AddForwardAction(new Backpropagate(errorMetric))
                ;

                engine.Train(30, testData, errorMetric);

                var networkGraph = engine.Graph;
                var executionEngine = graph.CreateEngine(networkGraph);

                var output = executionEngine.Execute(testData);
                Console.WriteLine(output.Average(o => o.CalculateError(errorMetric)));
            }
        }

        static void ManyToOne()
        {
            var grammar = new SequenceClassification(10, 5, 5, true, false);
            var sequences = grammar.GenerateSequences().Take(100).ToList();
            var builder = BrightWireProvider.CreateDataTableBuilder();
            builder.AddColumn(ColumnType.Matrix, "Sequence");
            builder.AddColumn(ColumnType.Vector, "Summary");

            foreach (var sequence in sequences) {
                var list = new List<FloatVector>();
                var charSet = new HashSet<char>();
                foreach (var ch in sequence) {
                    charSet.Add(ch);
                    var row = grammar.Encode(charSet.Select(ch2 => (ch2, 1f)));
                    list.Add(row);
                }
                builder.Add(new FloatMatrix { Row = list.ToArray() }, list.Last());
            }
            var data = builder.Build().Split(0);

            using (var lap = BrightWireProvider.CreateLinearAlgebra(false)) {
                var graph = new GraphFactory(lap);
                var errorMetric = graph.ErrorMetric.Rmse;

                // create the property set
                var propertySet = graph.CurrentPropertySet
                    .Use(graph.RmsProp())
                    .Use(graph.WeightInitialisation.Xavier)
                ;

                // create the engine
                var trainingData = graph.GetDataSource(data.Training);
                var testData = trainingData.CloneWith(data.Test);
                var engine = graph.CreateTrainingEngine(trainingData, 0.003f, 8);

                // build the network
                const int HIDDEN_LAYER_SIZE = 128;
                var memory = new float[HIDDEN_LAYER_SIZE];
                var memory2 = new float[HIDDEN_LAYER_SIZE];
                var network = graph.Connect(engine)
                    .AddGru(memory)
                    //.AddSimpleRecurrent(graph.ReluActivation(), memory)
                    .AddFeedForward(engine.DataSource.OutputSize)
                    //.Add(graph.SigmoidActivation())
                    .AddForwardAction(new BackpropagateThroughTime(errorMetric))
                ;

                engine.Train(30, testData, errorMetric);

                var networkGraph = engine.Graph;
                var executionEngine = graph.CreateEngine(networkGraph);

                var output = executionEngine.Execute(testData);
                Console.WriteLine(output.Where(o => o.Target != null).Average(o => o.CalculateError(errorMetric)));
            }
        }

        static void SequenceToSequence()
        {
            const int SEQUENCE_LENGTH = 4;
            var grammar = new SequenceClassification(8, SEQUENCE_LENGTH, SEQUENCE_LENGTH, true, false);
            var sequences = grammar.GenerateSequences().Take(1000).ToList();
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

            using (var lap = BrightWireProvider.CreateLinearAlgebra(false)) {
                var graph = new GraphFactory(lap);
                var errorMetric = graph.ErrorMetric.OneHotEncoding;

                // create the property set
                var propertySet = graph.CurrentPropertySet
                    .Use(graph.RmsProp())
                    .Use(graph.WeightInitialisation.Xavier)
                ;

                const int BATCH_SIZE = 128;
                int HIDDEN_LAYER_SIZE = 64;
                const float TRAINING_RATE = 0.003f;
                var encoderLearningContext = graph.CreateLearningContext(TRAINING_RATE, BATCH_SIZE, true, true);
                var encoderMemory = new float[HIDDEN_LAYER_SIZE];
                var decoderMemory = new float[HIDDEN_LAYER_SIZE];

                var trainingData = graph.GetDataSource(data.Training, encoderLearningContext, wb => wb
                    .AddLstm(encoderMemory, "encoder")
                    .AddForwardAction(new WriteNodeMemoryToSlot("shared-memory", wb.Find("encoder") as IHaveMemoryNode))
                    .AddFeedForward(grammar.DictionarySize)
                    .Add(graph.SigmoidActivation())
                    .AddForwardAction(new BackpropagateThroughTime(errorMetric))
                );
                var testData = trainingData.CloneWith(data.Test);

                // create the engine
                var engine = graph.CreateTrainingEngine(trainingData, TRAINING_RATE, BATCH_SIZE);
                var wb2 = graph.Connect(engine);
                wb2
                    .AddForwardAction(new JoinInputWithMemory("shared-memory"))
                    .IncrementSizeBy(HIDDEN_LAYER_SIZE)
                    .AddLstm(decoderMemory, "decoder")
                    .AddForwardAction(new WriteNodeMemoryToSlot("shared-memory", wb2.Find("decoder") as IHaveMemoryNode))
                    .AddFeedForward(trainingData.OutputSize)
                    .Add(graph.SoftMaxActivation())
                    .AddForwardAction(new BackpropagateThroughTime(errorMetric))
                ;

                engine.Train(100, testData, errorMetric);

                var dataSourceModel = (trainingData as IAdaptiveDataSource).GetModel();
                var testData2 = graph.GetDataSource(data.Test, dataSourceModel);
                var networkGraph = engine.Graph;
                var executionEngine = graph.CreateEngine(networkGraph);

                var output = executionEngine.Execute(testData2);
                Console.WriteLine(output.Average(o => o.CalculateError(errorMetric)));
            }
        }
    }
}
