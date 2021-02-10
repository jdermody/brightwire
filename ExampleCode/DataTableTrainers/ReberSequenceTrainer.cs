using System;
using System.Linq;
using BrightData;
using BrightWire;
using BrightWire.TrainingData.Artificial;
using MathNet.Numerics.Distributions;

namespace ExampleCode.DataTableTrainers
{
    internal class ReberSequenceTrainer : DataTableTrainer
    {
        readonly IBrightDataContext _context;

        public ReberSequenceTrainer(IBrightDataContext context, IRowOrientedDataTable dataTable) : base(dataTable)
        {
            _context = context;
        }

        public IGraphExecutionEngine TrainLstm()
        {
            var graph = _context.CreateGraphFactory();

            // binary classification rounds each output to either 0 or 1
            var errorMetric = graph.ErrorMetric.BinaryClassification;

            // configure the network properties
            graph.CurrentPropertySet
                .Use(graph.GradientDescent.RmsProp)
                .Use(graph.WeightInitialisation.Xavier)
            ;

            // create the engine
            var trainingData = graph.CreateDataSource(Training);
            var testData = trainingData.CloneWith(Test);
            var engine = graph.CreateTrainingEngine(trainingData, errorMetric, learningRate: 0.1f, batchSize: 32);

            // build the network
            const int HIDDEN_LAYER_SIZE = 38, TRAINING_ITERATIONS = 2;
            graph.Connect(engine)
                .AddLstm(HIDDEN_LAYER_SIZE)
                .AddFeedForward(engine.DataSource.GetOutputSizeOrThrow())
                .Add(graph.SoftMaxActivation())
                .AddBackpropagationThroughTime(errorMetric)
            ;

            var model = engine.Train(TRAINING_ITERATIONS, testData, errorMetric);
            return engine.CreateExecutionEngine(model?.Graph);
        }

        public void GenerateSequences(IGraphExecutionEngine engine)
        {
            Console.WriteLine("Generating new reber sequences from the observed state probabilities...");
            var graph = _context.CreateGraphFactory();

            for (var z = 0; z < 5; z++)
            {
                // prepare the first input
                var input = new float[ReberGrammar.Size];
                input[ReberGrammar.GetIndex('B')] = 1f;
                Console.Write("B");

                uint index = 0, eCount = 0;
                var result = engine.ExecuteSingleSequentialStep(index++, input, MiniBatchSequenceType.SequenceStart);
                if (result != null) {
                    for (var i = 0; i < 32; i++) {
                        var next = result!.Output[0].Values
                            .Select((v, j) => ((double) v, j))
                            .Where(d => d.Item1 >= 0.1f)
                            .ToList();

                        var distribution = new Categorical(next.Select(d => d.Item1).ToArray());
                        var nextIndex = next[distribution.Sample()].j;
                        Console.Write(ReberGrammar.GetChar(nextIndex));
                        if (nextIndex == ReberGrammar.GetIndex('E') && ++eCount == 2)
                            break;

                        Array.Clear(input, 0, ReberGrammar.Size);
                        input[nextIndex] = 1f;
                        result = engine.ExecuteSingleSequentialStep(index++, input, MiniBatchSequenceType.Standard);
                    }
                    Console.WriteLine();
                }
            }
        }
    }
}
