using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using BrightData;
using BrightData.Cuda;
using BrightData.LinearAlgebra;
using BrightData.MKL;
using BrightWire;
using CommunityToolkit.HighPerformance.Buffers;
using ExampleCode.DataSet;
using ExampleCode.DataTableTrainers;

namespace ExampleCode
{
    internal class Program
    {
        const int RandomSeed = 0;

        class TestClass : IHaveDataAsReadOnlyByteSpan
        {
            readonly byte[] _data;

            public TestClass(ReadOnlySpan<byte> data)
            {
                _data = data.ToArray();
            }

            public ReadOnlySpan<byte> DataAsBytes => _data;
            public override string ToString()
            {
                return String.Join(", ", _data);
            }
        }

        struct TestClass2
        {
            public int Property { get; set; }
        }

        static void Main()
        {
            using var context = new BrightDataContext(null, RandomSeed);

            //static async Task Test<T>(BrightData.Table.ICompositeBuffer<T> buffer) where T : notnull
            //{
            //    await foreach (var item in buffer) {
            //        Console.WriteLine(item.ToString());
            //    }
            //}
            //var vectorBuffer = ExtensionMethods.CreateCompositeBuffer<TestClass>(x => new(x), null, 2, 0);
            //vectorBuffer.Add(new TestClass(new byte[] { 1, 2, 3 }));
            //vectorBuffer.Add(new TestClass(new byte[] { 4, 5, 6 }));
            //vectorBuffer.Add(new TestClass(new byte[] { 7, 8, 9 }));
            //vectorBuffer.ForEachBlock(x => {
            //    foreach (var item in x)
            //        Console.WriteLine(item.ToString());
            //});

            //var parser = new BrightData.Table.Helper.CsvParser(true, ',');
            //var buffers = parser.Parse(@"
            //test,test2,test3
            //123,234,567
            //123,234
            //");

            //var stringBuffer = ExtensionMethods.CreateCompositeBuffer(null, 2, 0, 128);
            //stringBuffer.Add("this is a test");
            //stringBuffer.Add("this is another test");
            //stringBuffer.Add("this is a final test");
            //for (uint i = 0; i < stringBuffer.BlockCount; i++) {
            //    var block = stringBuffer.GetBlock(i).Result;
            //}
            //foreach (var item in ExtensionMethods.GetEnumerator(stringBuffer))
            //    Console.WriteLine(item);
            //Test(stringBuffer).Wait();

            ////var (table, encoded) = ExtensionMethods.Encode(stringBuffer);
            //var test2 = ExtensionMethods.CreateCompositeBuffer<int>(null, 2, 0);
            //test2.Add(1);
            //test2.Add(new ReadOnlySpan<int>(new[] { 2, 3 }));
            //test2.ForEachBlock(block => {
            //    foreach (var num in block)
            //        Console.WriteLine(num);
            //});
            //var test = ExtensionMethods.ToNumeric(test2, null, 256).Result;

            //foreach (var item in ExtensionMethods.GetEnumerator(test2))
            //    Console.WriteLine(item);
            //Test(test2).Wait();
            //for (uint i = 0; i < test2.BlockCount; i++) {
            //    var block = test2.GetBlock(i).Result;
            //}
            //var table = ExtensionMethods.CreateTableInMemory(context, stringBuffer, test2).Result;
            //var metaData = table.GetColumnAnalysis().Result;
            //var strs = ExtensionMethods.AsReadOnlySequence(table.GetColumn<string>(0)).Result;
            //var nums = ExtensionMethods.AsReadOnlySequence(table.GetColumn<int>(1)).Result;
            //foreach (var item in strs) {
            //    foreach (var str in item.Span) {
            //        Console.WriteLine(str);
            //    }
            //}
            //return;

            //var nums = ExtensionMethods.CreateCompositeBuffer<int>(null, 32);
            //nums.Add(1);
            //nums.Add(2);

            //var buffer = new TestClass2[2];
            //ExtensionMethods.SetProperty(nums, x => x.Property, (start, count) => buffer.AsSpan(start, count));


            bool useCuda = true, useMkl = true;

            // IMPORTANT: uncomment the following line to disable MKL (for example if you do not have an Intel CPU)
            // ALSO: check the mkl.net nuget package version is correct for your OS (default for ExampleCode is Windows x64)
            //useMkl = false;

            // IMPORTANT: uncomment the following line to disable CUDA (for example if you have a do not have an NVIDIA GPU)
            // ALSO: make sure you have installed the CUDA toolkit from https://developer.nvidia.com/cuda-toolkit
            //useCuda = false;

            // IMPORTANT: set where to save training data files
            context.Set("DataFileDirectory", new DirectoryInfo(@"c:\data"));

            if (useMkl && useCuda)
                PerformanceTest.Run(new LinearAlgebraProvider(context), new MklLinearAlgebraProvider(context), new CudaLinearAlgebraProvider(context));
            else if (useMkl)
                PerformanceTest.Run(new LinearAlgebraProvider(context), new CudaLinearAlgebraProvider(context));
            else
                PerformanceTest.Run(new LinearAlgebraProvider(context));

            //Xor(context, useMkl);
            //IrisClassification(context, useMkl);
            //IrisClustering(context, useMkl);
            //MarkovChains(context, useMkl);
            //TextClustering(context, useMkl);
            //IntegerAddition(context, useMkl);
            //ReberPrediction(context, useMkl);
            //OneToMany(context, useMkl);
            //ManyToOne(context, useMkl);
            //SequenceToSequence(context, useMkl);
            //StockData(context, useMkl, useCuda);
            //PredictBicyclesWithNeuralNetwork(context, useMkl);
            //MultiLabelSingleClassifier(context, useMkl);
            //MultiLabelMultiClassifiers(context, useMkl);
            //MnistFeedForward(context, useMkl);
            //MnistConvolutional(context, useMkl, useCuda);
            //TrainIncomePrediction(context, useMkl);
            //SentimentClassification(context, useMkl);
        }

        static void Start(BrightDataContext context, bool useMkl, bool useCuda = false, [CallerMemberName]string title = "")
        {
            context.ResetRandom(RandomSeed);

            Console.WriteLine("*********************************************");
            Console.WriteLine("*");
            Console.WriteLine($"* {title}");
            if (useCuda) {
                context.UseCuda();
                Console.WriteLine("* (GPU - CUDA)");
            }else if (useMkl) {
                context.UseMkl();
                Console.WriteLine("* (CPU - MKL)");
            }
            else {
                Console.WriteLine("* (CPU)");
            }
            Console.WriteLine("*");
            Console.WriteLine("*********************************************");
        }

        static void WriteSeparator()
        {
            Console.WriteLine("---------------------------------------------");
        }

        static async Task Xor(BrightDataContext context, bool useMkl)
        {
            Start(context, useMkl);
            using var trainer = await context.Xor();
            trainer.Train(4, 100, 0.5f, 4);
        }

        static async Task IrisClassification(BrightDataContext context, bool useMkl)
        {
            Start(context, useMkl);
            using var iris = await context.Iris();
            iris.TrainNaiveBayes();
            iris.TrainDecisionTree();
            iris.TrainRandomForest(500, 7);
            iris.TrainKNearestNeighbours(10);
            //iris.TrainMultinomialLogisticRegression(500, 0.3f, 0.1f);
            iris.TrainSigmoidNeuralNetwork(32, 200, 0.1f, 64, 50);
        }

        static async Task IrisClustering(BrightDataContext context, bool useMkl)
        {
            Start(context, useMkl);
            using var iris = await context.Iris();
            var irisTable = iris.Table.Value;

            Console.WriteLine("K Means...");
            WriteSeparator();
            Write(irisTable.KMeans(3));
            WriteSeparator();

            Console.WriteLine();
            Console.WriteLine("Hierarchical...");
            WriteSeparator();
            Write(irisTable.HierarchicalCluster(3));
            WriteSeparator();

            Console.WriteLine();
            Console.WriteLine("NNMF...");
            WriteSeparator();
            Write(irisTable.NonNegativeMatrixFactorisation(3));
            WriteSeparator();
            return;

            static void Write(IEnumerable<(uint RowIndex, string? Label)[]> items)
            {
                var clusters = items.Select(c => c.Select(r => r.Label).GroupAndCount().Format());
                foreach (var cluster in clusters)
                    Console.WriteLine(cluster);
            }
        }

        static async Task MarkovChains(BrightDataContext context, bool useMkl)
        {
            Start(context, useMkl);
            var trainer = await context.BeautifulandDamned();
            trainer.TrainMarkovModel();
        }

        static async Task MnistFeedForward(BrightDataContext context, bool useMkl)
        {
            Start(context, useMkl);
            using var mnist = context.Mnist();
            mnist.TrainFeedForwardNeuralNetwork();
        }

        static void MnistConvolutional(BrightDataContext context, bool useMkl, bool useCuda)
        {
            Start(context, useMkl, useCuda);
            using var mnist = context.Mnist();
            mnist.TrainConvolutionalNeuralNetwork();
        }

        static async Task SentimentClassification(BrightDataContext context, bool useMkl)
        {
            Start(context, useMkl);
            var sentiment = context.SentimentData();

            // train a bernoulli naive bayes classifier
            var bernoulli = sentiment.TrainBernoulli().CreateClassifier();

            // train a multinomial naive bayes classifier
            var multinomial = sentiment.TrainMultinomialNaiveBayes().CreateClassifier();

            var recurrent = await sentiment.TrainBiLstm(bernoulli, multinomial);

            sentiment.TestClassifiers(bernoulli, multinomial, recurrent);
        }

        static async Task TextClustering(BrightDataContext context, bool useMkl)
        {
            Start(context, useMkl);
            var textClustering = await context.TextClustering();
            textClustering.KMeans();
            textClustering.Nnmf();
            textClustering.RandomProjection();
            if(context.LinearAlgebraProvider.ProviderName != Consts.DefaultLinearAlgebraProviderName)
                textClustering.LatentSemanticAnalysis();
        }

        static async Task IntegerAddition(BrightDataContext context, bool useMkl)
        {
            Start(context, useMkl);
            using var trainer = await context.IntegerAddition();
            trainer.TrainRecurrentNeuralNetwork();
        }

        static async Task ReberPrediction(BrightDataContext context, bool useMkl)
        {
            Start(context, useMkl);
            using var reber = await context.ReberSequencePrediction(extended: true, minLength:10, maxLength:10);
            var engine = reber.TrainLstm();
            ReberSequenceTrainer.GenerateSequences(engine);
        }

        static async Task OneToMany(BrightDataContext context, bool useMkl)
        {
            Start(context, useMkl);
            using var sequences = await context.OneToMany();
            sequences.TrainOneToMany();
        }

        static async Task ManyToOne(BrightDataContext context, bool useMkl)
        {
            Start(context, useMkl);
            using var sequences = await context.ManyToOne();
            sequences.TrainManyToOne();
        }

        static async Task SequenceToSequence(BrightDataContext context, bool useMkl)
        {
            Start(context, useMkl);
            using var sequences = await context.SequenceToSequence();
            sequences.TrainSequenceToSequence();
        }

        //static void SimpleLinearTest(BrightDataContext context)
        //{
        //    Start(context);
        //    context.SimpleLinear().TrainLinearRegression();
        //}

        //static void PredictBicyclesWithLinearModel(BrightDataContext context)
        //{
        //    Start(context);
        //    context.Bicycles().TrainLinearModel();
        //}

        static async Task PredictBicyclesWithNeuralNetwork(BrightDataContext context, bool useMkl)
        {
            Start(context, useMkl);
            using var trainer = await context.Bicycles();
            trainer.TrainNeuralNetwork();
        }

        static async Task MultiLabelSingleClassifier(BrightDataContext context, bool useMkl)
        {
            Start(context, useMkl);
            using var trainer = await context.Emotions();
            trainer.TrainNeuralNetwork();
        }

        static async Task MultiLabelMultiClassifiers(BrightDataContext context, bool useMkl)
        {
            Start(context, useMkl);
            using var trainer = await context.Emotions();
            trainer.TrainMultiClassifiers();
        }

        static async Task StockData(BrightDataContext context, bool useMkl, bool useCuda)
        {
            Start(context, useMkl, useCuda);
            using var trainer = await context.StockData();
            var stockData = trainer.GetSequentialWindow();
            stockData.TrainLstm(256);
        }

        static async Task TrainIncomePrediction(BrightDataContext context, bool useMkl)
        {
            Start(context, useMkl);
            using var adult = await context.Adult();
            adult.TrainNeuralNetwork();
            //adult.TrainNaiveBayes();
        }
    }
}
