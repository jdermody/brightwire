using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightData;
using BrightWire;
using BrightWire.TrainingData.Artificial;

namespace ExampleCode.DataTableTrainers
{
    internal class ReberSequenceTrainer(IDataTable dataTable) : DataTableTrainer(dataTable)
    {
        public async Task<IGraphExecutionEngine> TrainSimpleRecurrent()
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
            var engine = graph.CreateTrainingEngine(trainingData, errorMetric, learningRate: 0.03f, batchSize: 32);

            // build the network
            const int hiddenLayerSize = 50, trainingIterations = 40;
            graph.Connect(engine)
                .AddSimpleRecurrent(graph.SigmoidActivation(), hiddenLayerSize, "layer1")
                .AddRecurrentBridge("layer1", "layer2")
                .AddSimpleRecurrent(graph.SigmoidActivation(), hiddenLayerSize, "layer2")
                .AddFeedForward(engine.DataSource.GetOutputSizeOrThrow())
                .Add(graph.TanhActivation())
                .AddBackpropagationThroughTime()
            ;

            engine.LearningContext.ScheduleLearningRate(10, 0.01f);
            engine.LearningContext.ScheduleLearningRate(20, 0.003f);
            var model = await engine.Train(trainingIterations, testData);
            return engine.CreateExecutionEngine(model?.Graph);
        }

        public async Task<IGraphExecutionEngine> TrainGru()
        {
            var graph = _context.CreateGraphFactory();

            var errorMetric = graph.ErrorMetric.BinaryClassification;

            // configure the network properties
            graph.CurrentPropertySet
                .Use(graph.GradientDescent.RmsProp)
                .Use(graph.WeightInitialisation.Xavier)
            ;

            // create the engine
            var learningRate = 0.01f;
            var trainingData = graph.CreateDataSource(Training);
            var testData = trainingData.CloneWith(Test);
            var engine = graph.CreateTrainingEngine(trainingData, errorMetric, learningRate, batchSize: 32);

            // build the network
            const int hiddenLayerSize = 30, trainingIterations = 50;
            graph.Connect(engine)
                .AddGru(hiddenLayerSize, "layer1")
                //.AddRecurrentBridge("layer1", "layer2")
                //.AddGru(hiddenLayerSize, "layer2")
                .AddFeedForward(engine.DataSource.GetOutputSizeOrThrow())
                .Add(graph.SoftMaxActivation())
                .AddBackpropagationThroughTime()
            ;

            engine.LearningContext.ScheduleLearningRate(15, learningRate / 3);
            engine.LearningContext.ScheduleLearningRate(30, learningRate / 9);
            var model = await engine.Train(trainingIterations, testData);
            return engine.CreateExecutionEngine(model?.Graph);
        }

        public async Task<IGraphExecutionEngine> TrainLstm()
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
            var learningRate = 0.01f;
            var engine = graph.CreateTrainingEngine(trainingData, errorMetric, learningRate, batchSize: 32);

            // build the network
            const int hiddenLayerSize = 30, trainingIterations = 50;
            graph.Connect(engine)
                .AddLstm(hiddenLayerSize, "encoder1")
                //.AddRecurrentBridge("encoder1", "encoder2")
                //.AddLstm(hiddenLayerSize, "encoder2")
                .AddFeedForward(engine.DataSource.GetOutputSizeOrThrow())
                .Add(graph.SoftMaxActivation())
                .AddBackpropagationThroughTime()
            ;

            engine.LearningContext.ScheduleLearningRate(15, learningRate / 3);
            engine.LearningContext.ScheduleLearningRate(30, learningRate / 9);
            var model = await engine.Train(trainingIterations, testData);
            return engine.CreateExecutionEngine(model?.Graph);
        }

        public static async Task GenerateSequences(IGraphExecutionEngine engine)
        {
            Console.WriteLine("Generating new reber sequences from the observed state probabilities...");

            for (var z = 0; z < 10; z++)
            {
                // prepare the first input
                var input = new float[ReberGrammar.Size];
                input[ReberGrammar.GetIndex('B')] = 1f;
                Console.Write("B");

                uint index = 0, eCount = 0;
                var context = engine.LinearAlgebraProvider.Context;
                var executionContext = engine.CreateExecutionContext();
                var result = await engine.ExecuteSingleSequentialStep(executionContext, index++, input, MiniBatchSequenceType.SequenceStart).FirstOrDefault();
                if (result != null) {
                    var sb = new StringBuilder();
                    for (var i = 0; i < 32; i++) {
                        var next = result!.Output[0].ToArray()
                            .Select((v, j) => ((double) v, j))
                            .Where(d => d.Item1 >= 0.1f)
                            .ToList();
                        if (next.Count == 0)
                            break;

                        var distribution = context.CreateCategoricalDistribution(next.Select(d => (float)d.Item1));
                        var nextIndex = next[(int)distribution.Sample()].j;
                        sb.Append(ReberGrammar.GetChar(nextIndex));
                        if (nextIndex == ReberGrammar.GetIndex('E') && ++eCount == 2)
                            break;

                        Array.Clear(input, 0, ReberGrammar.Size);
                        input[nextIndex] = 1f;
                        result = await engine.ExecuteSingleSequentialStep(executionContext, index++, input, MiniBatchSequenceType.Standard).FirstOrDefault();
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
