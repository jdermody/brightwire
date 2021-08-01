using System;
using System.Linq;
using System.Text;
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

        public IGraphExecutionEngine TrainSimpleRecurrent()
        {
            var graph = _context.CreateGraphFactory();
            var errorMetric = graph.ErrorMetric.CrossEntropy;

            // configure the network properties
            graph.CurrentPropertySet
                .Use(graph.GradientDescent.RmsProp)
                .Use(graph.WeightInitialisation.Xavier)
            ;

            // create the engine
            var trainingData = graph.CreateDataSource(Training);
            var testData = trainingData.CloneWith(Test);
            var engine = graph.CreateTrainingEngine(trainingData, errorMetric, learningRate: 0.3f, batchSize: 32);

            // build the network
            const int HIDDEN_LAYER_SIZE = 50, TRAINING_ITERATIONS = 40;
            graph.Connect(engine)
                .AddSimpleRecurrent(graph.SigmoidActivation(), HIDDEN_LAYER_SIZE, "layer1")
                .AddRecurrentBridge("layer1", "layer2")
                .AddSimpleRecurrent(graph.SigmoidActivation(), HIDDEN_LAYER_SIZE, "layer2")
                .AddFeedForward(engine.DataSource.GetOutputSizeOrThrow())
                .Add(graph.SoftMaxActivation())
                .AddBackpropagationThroughTime()
            ;

            engine.LearningContext.ScheduleLearningRate(10, 0.1f);
            engine.LearningContext.ScheduleLearningRate(20, 0.03f);
            var model = engine.Train(TRAINING_ITERATIONS, testData);
            return engine.CreateExecutionEngine(model?.Graph);
        }

        public IGraphExecutionEngine TrainGru()
        {
            var graph = _context.CreateGraphFactory();

            var errorMetric = graph.ErrorMetric.BinaryClassification;

            // configure the network properties
            graph.CurrentPropertySet
                .Use(graph.GradientDescent.RmsProp)
                .Use(graph.WeightInitialisation.Xavier)
            ;

            // create the engine
            var trainingData = graph.CreateDataSource(Training);
            var testData = trainingData.CloneWith(Test);
            var engine = graph.CreateTrainingEngine(trainingData, errorMetric, learningRate: 0.3f, batchSize: 32);

            // build the network
            const int HIDDEN_LAYER_SIZE = 40, TRAINING_ITERATIONS = 50;
            graph.Connect(engine)
                .AddGru(HIDDEN_LAYER_SIZE, "layer1")
                .AddFeedForward(engine.DataSource.GetOutputSizeOrThrow())
                .Add(graph.SoftMaxActivation())
                .AddBackpropagationThroughTime()
            ;

            engine.LearningContext.ScheduleLearningRate(15, 0.1f);
            engine.LearningContext.ScheduleLearningRate(30, 0.03f);
            var model = engine.Train(TRAINING_ITERATIONS, testData);
            return engine.CreateExecutionEngine(model?.Graph);
        }

        public IGraphExecutionEngine TrainLstm()
        {
            var graph = _context.CreateGraphFactory();

            var errorMetric = graph.ErrorMetric.BinaryClassification;

            // configure the network properties
            graph.CurrentPropertySet
                .Use(graph.GradientDescent.RmsProp)
                .Use(graph.WeightInitialisation.Xavier)
            ;

            // create the engine
            var trainingData = graph.CreateDataSource(Training);
            var testData = trainingData.CloneWith(Test);
            var engine = graph.CreateTrainingEngine(trainingData, errorMetric, learningRate: 0.3f, batchSize: 32);

            // build the network
            const int HIDDEN_LAYER_SIZE = 40, TRAINING_ITERATIONS = 50;
            graph.Connect(engine)
                .AddLstm(HIDDEN_LAYER_SIZE)
                .AddFeedForward(engine.DataSource.GetOutputSizeOrThrow())
                .Add(graph.SoftMaxActivation())
                .AddBackpropagationThroughTime()
            ;

            engine.LearningContext.ScheduleLearningRate(15, 0.1f);
            engine.LearningContext.ScheduleLearningRate(30, 0.03f);
            var model = engine.Train(TRAINING_ITERATIONS, testData);
            return engine.CreateExecutionEngine(model?.Graph);
        }

        public void GenerateSequences(IGraphExecutionEngine engine)
        {
            Console.WriteLine("Generating new reber sequences from the observed state probabilities...");

            for (var z = 0; z < 10; z++)
            {
                // prepare the first input
                var input = new float[ReberGrammar.Size];
                input[ReberGrammar.GetIndex('B')] = 1f;
                Console.Write("B");

                uint index = 0, eCount = 0;
                var executionContext = engine.CreateExecutionContext();
                var result = engine.ExecuteSingleSequentialStep(executionContext, index++, input, MiniBatchSequenceType.SequenceStart);
                if (result != null) {
                    var sb = new StringBuilder();
                    for (var i = 0; i < 32; i++) {
                        var next = result!.Output[0]
                            .Select((v, j) => ((double) v, j))
                            .Where(d => d.Item1 >= 0.1f)
                            .ToList();

                        var distribution = new Categorical(next.Select(d => d.Item1).ToArray());
                        var nextIndex = next[distribution.Sample()].j;
                        sb.Append(ReberGrammar.GetChar(nextIndex));
                        if (nextIndex == ReberGrammar.GetIndex('E') && ++eCount == 2)
                            break;

                        Array.Clear(input, 0, ReberGrammar.Size);
                        input[nextIndex] = 1f;
                        result = engine.ExecuteSingleSequentialStep(executionContext, index++, input, MiniBatchSequenceType.Standard);
                    }

                    var str = sb.ToString();
                    Console.Write(str);
                    if(str[0] == str[^2])
                        Console.WriteLine(" - end sequence matched start sequence");
                    else
                        Console.WriteLine();
                }
            }
        }
    }
}
