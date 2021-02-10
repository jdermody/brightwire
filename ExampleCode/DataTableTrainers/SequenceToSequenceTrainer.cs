using System;
using System.Linq;
using BrightData;
using BrightWire;

namespace ExampleCode.DataTableTrainers
{
    internal class SequenceToSequenceTrainer : DataTableTrainer
    {
        readonly IBrightDataContext _context;
        readonly uint _dictionarySize;

        public SequenceToSequenceTrainer(IBrightDataContext context, uint dictionarySize, IRowOrientedDataTable dataTable) : base(dataTable)
        {
            _context = context;
            _dictionarySize = dictionarySize;
        }

        public void TrainLstm()
        {
            var graph = _context.CreateGraphFactory();
            var errorMetric = graph.ErrorMetric.BinaryClassification;

            // create the property set
            var propertySet = graph.CurrentPropertySet
                .Use(graph.GradientDescent.RmsProp)
                .Use(graph.WeightInitialisation.Xavier)
            ;

            // create the engine
            const float TRAINING_RATE = 0.1f;
            var trainingData = graph.CreateDataSource(Training);
            var testData = trainingData.CloneWith(Test);
            var engine = graph.CreateTrainingEngine(trainingData, errorMetric, TRAINING_RATE, 8);
            engine.LearningContext.ScheduleLearningRate(30, TRAINING_RATE / 3);

            // build the network
            const int HIDDEN_LAYER_SIZE = 128;
            graph.Connect(engine)
                .AddLstm(HIDDEN_LAYER_SIZE)
                .AddFeedForward(engine.DataSource.GetOutputSizeOrThrow())
                .Add(graph.SigmoidActivation())
                .AddBackpropagation(errorMetric)
            ;

            engine.Train(40, testData, errorMetric);

            var networkGraph = engine.Graph;
            var executionEngine = graph.CreateExecutionEngine(networkGraph);

            var output = executionEngine.Execute(testData);
            Console.WriteLine(output.Average(o => o.CalculateError(errorMetric)));
        }

        public void TrainGru()
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
            var engine = graph.CreateTrainingEngine(trainingData, errorMetric, 0.01f, 8);

            // build the network
            const int HIDDEN_LAYER_SIZE = 128;
            graph.Connect(engine)
                .AddGru(HIDDEN_LAYER_SIZE)
                .AddFeedForward(engine.DataSource.GetOutputSizeOrThrow())
                .Add(graph.TanhActivation())
                .AddBackpropagationThroughTime(errorMetric)
            ;

            engine.Train(20, testData, errorMetric);

            var networkGraph = engine.Graph;
            var executionEngine = graph.CreateExecutionEngine(networkGraph);

            var output = executionEngine.Execute(testData);
            Console.WriteLine(output.Where(o => o.Target != null).Average(o => o.CalculateError(errorMetric)));
        }

        public void TrainSequenceToSequence()
        {
            var graph = _context.CreateGraphFactory();
            var errorMetric = graph.ErrorMetric.BinaryClassification;

            // create the property set
            graph.CurrentPropertySet
                .Use(graph.GradientDescent.RmsProp)
                .Use(graph.WeightInitialisation.Xavier)
            ;

            const uint BATCH_SIZE = 16;
            const uint HIDDEN_LAYER_SIZE = 64;
            const float TRAINING_RATE = 0.01f;

            var trainingData = graph.CreateDataSource(Training);
            var testData = trainingData.CloneWith(Test);
            var engine = graph.CreateTrainingEngine(trainingData, errorMetric, TRAINING_RATE, BATCH_SIZE);

            graph.Connect(engine)
                .AddLstm(HIDDEN_LAYER_SIZE, "encoder")
                //.WriteNodeMemoryToSlot("shared-memory", "encoder")
                //.AddFeedForward(_dictionarySize)
                //.Add(graph.SigmoidActivation())
                .AddSequenceToSequencePivot()
                //.JoinInputWithMemory("shared-memory", HIDDEN_LAYER_SIZE)
                .AddLstm(HIDDEN_LAYER_SIZE, "decoder")
                .AddFeedForward(engine.DataSource.GetOutputSizeOrThrow())
                .Add(graph.SigmoidActivation())
                .AddBackpropagationThroughTime(errorMetric)
            ;

            // create the engine
            
            engine.LearningContext.ScheduleLearningRate(30, TRAINING_RATE / 3);
            engine.LearningContext.ScheduleLearningRate(40, TRAINING_RATE / 9);

            engine.Train(50, testData, errorMetric);
        }
    }
}
