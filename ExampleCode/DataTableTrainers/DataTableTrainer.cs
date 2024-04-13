using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BrightData;
using BrightWire;
using BrightWire.Models.Bayesian;
using BrightWire.Models.InstanceBased;
using BrightWire.Models.TreeBased;

namespace ExampleCode.DataTableTrainers
{
    internal class DataTableTrainer : IDisposable
    {
        protected readonly BrightDataContext _context;

        public DataTableTrainer(IDataTable table)
        {
            OriginalTable = table;
            var shuffled = table.Shuffle(null).Result;
            _context = table.Context;
            TargetColumn = table.GetTargetColumnOrThrow();
            Table = new(shuffled);
            var (training, test) = shuffled.Split().Result;
            Training = training;
            Test = test;
        }

        public DataTableTrainer(IDataTable? table, IDataTable training, IDataTable test)
        {
            _context = training.Context;
            TargetColumn = training.GetTargetColumnOrThrow();
            Training = training;
            Test = test;
            if (table is null)
                Table = new(() => training.ConcatenateRows(null, test).Result);
            else
                Table = new(table);
        }

        public void Dispose()
        {
            OriginalTable?.Dispose();
            if(Table.IsValueCreated)
                Table.Value.Dispose();
            Training.Dispose();
            Test.Dispose();
        }

        public uint TargetColumn { get; }
        public IDataTable? OriginalTable { get; }
        public Lazy<IDataTable> Table { get; }
        public IDataTable Training { get; }
        public IDataTable Test { get; }

        public IEnumerable<string> KMeans(uint k) => AggregateLabels(Table.Value.KMeans(k));
        public IEnumerable<string> HierarchicalCluster(uint k) => AggregateLabels(Table.Value.HierarchicalCluster(k));
        public IEnumerable<string> NonNegativeMatrixFactorisation(uint k) => AggregateLabels(Table.Value.NonNegativeMatrixFactorisation(k));

        static IEnumerable<string> AggregateLabels(IEnumerable<(uint RowIndex, string? Label)[]> clusters) => clusters
            .Select(c => String.Join(';', c
                .Select(r => r.Label)
                .GroupBy(d => d)
                .Select(g => (Label: g.Key, Count: g.Count()))
                .OrderByDescending(g => g.Count)
                .ThenBy(g => g.Label)
                .Select(g => $"{g.Label ?? "<<NULL>>"} ({g.Count})")
            ));

        public async Task<NaiveBayes> TrainNaiveBayes(bool writeResults = true)
        {
            var ret = await Training.TrainNaiveBayes();
            if (writeResults)
                WriteResults("Naive bayes", ret.CreateClassifier());
            return ret;
        }

        public DecisionTree TrainDecisionTree(bool writeResults = true)
        {
            var ret = Training.TrainDecisionTree();
            if (writeResults)
                WriteResults("Decision tree", ret.CreateClassifier());
            return ret;
        }

        public async Task<RandomForest> TrainRandomForest(uint numTrees, uint? baggedRowCount = null, bool writeResults = true)
        {
            var ret = await Training.TrainRandomForest(numTrees, baggedRowCount);
            if(writeResults)
                WriteResults("Random forest", ret.CreateClassifier());
            return ret;
        }

        public async Task<KNearestNeighbours> TrainKNearestNeighbours(uint k, bool writeResults = true)
        {
            var ret = await Training.TrainKNearestNeighbours();
            if (writeResults)
                WriteResults("K nearest neighbours", ret.CreateClassifier(_context.LinearAlgebraProvider, k));
            return ret;
        }

        //public void TrainMultinomialLogisticRegression(uint iterations, float trainingRate, float lambda, bool writeResults = true)
        //{
        //    var ret = Training.TrainMultinomialLogisticRegression(iterations, trainingRate, lambda);
        //    if (writeResults)
        //        WriteResults("Multinomial logistic regression", ret.CreateClassifier(Table.Context.LinearAlgebraProvider2));
        //}

        async Task WriteResults(string type, IRowClassifier classifier)
        {
            var results = await Test.Classify(classifier).ToArrayAsync(Test.RowCount);
            var score = results
                .Average(d => d.Row.Get<string>(TargetColumn) == d.Classification.MaxBy(c => c.Weight).Label ? 1.0 : 0.0);
            Console.WriteLine($"{type} accuracy: {score:P}");
        }

        void WriteResults(string type, ITableClassifier classifier)
        {
            var results = classifier.Classify(Test).ToList();
            var column = Test.GetColumn(TargetColumn);
            var expectedLabels = column.GetValues().Select(o => o.ToString()).ToArray();
            var score = results
                .Average(d => expectedLabels[d.RowIndex] == d.Predictions.MaxBy(c => c.Weight).Classification ? 1.0 : 0.0);

            Console.WriteLine($"{type} accuracy: {score:P}");
        }

        public virtual async Task TrainSigmoidNeuralNetwork(uint hiddenLayerSize, uint numIterations, float trainingRate, uint batchSize, int testCadence = 1)
        {
            // create a neural network graph factory
            var graph = _context.CreateGraphFactory();

            // the default data table -> vector conversion uses one hot encoding of the classification labels, so create a corresponding cost function
            var errorMetric = graph.ErrorMetric.OneHotEncoding;

            // create the property set (use rms prop gradient descent optimisation)
            graph.CurrentPropertySet
                .Use(graph.RmsProp())
                .Use(graph.GaussianWeightInitialisation(true, 0.1f, GaussianVarianceCalibration.SquareRoot2N));

            // create the training and test data sources
            var trainingData = await graph.CreateDataSource(Training);
            var testData = trainingData.CloneWith(Test);

            // create a neural network with sigmoid activations after each neural network
            var engine = graph.CreateTrainingEngine(trainingData, errorMetric, trainingRate, batchSize);
            graph.Connect(engine)
                .AddFeedForward(hiddenLayerSize)
                .Add(graph.SigmoidActivation())
                .AddDropOut(dropOutPercentage: 0.5f)
                .AddFeedForward(engine.DataSource.GetOutputSizeOrThrow())
                .Add(graph.SigmoidActivation())
                .AddBackpropagation();

            // train the network
            Console.WriteLine("Training neural network...");
            await engine.Train(numIterations, testData, null, testCadence);
        }
    }
}
