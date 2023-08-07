using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using BrightData;
using BrightData.Cuda;
using BrightData.LinearAlgebra;
using BrightData.MKL;
using BrightWire;
using ExampleCode.DataSet;
using ExampleCode.DataTableTrainers;

namespace ExampleCode
{
    internal class Program
    {
        const int RandomSeed = 0;

        static void Main()
        {
            using var context = new BrightDataContext(null, RandomSeed);
            bool useCuda = true, useMkl = true;

            // IMPORTANT: uncomment the following line to disable MKL (for example if you do not have an Intel CPU)
            // ALSO: check the mkl.net nuget package version is correct for your OS (default for ExampleCode is Windows x64)
            //useMkl = false;

            // IMPORTANT: uncomment the following line to disable CUDA (for example if you have a do not have an NVIDA GPU)
            // ALSO: make sure you have installed CUDA toolkit from https://developer.nvidia.com/cuda-toolkit
            //useCuda = false;

            // IMPORTANT: set where to save training data files
            context.Set("DataFileDirectory", new DirectoryInfo(@"c:\data"));

            //PerformanceTest.Run(new LinearAlgebraProvider(context), new MklLinearAlgebraProvider(context), new CudaLinearAlgebraProvider(context));

            Xor(context, useMkl);
            IrisClassification(context, useMkl);
            IrisClustering(context, useMkl);
            MarkovChains(context, useMkl);
            TextClustering(context, useMkl);
            IntegerAddition(context, useMkl);
            ReberPrediction(context, useMkl);
            OneToMany(context, useMkl);
            ManyToOne(context, useMkl);
            SequenceToSequence(context, useMkl);
            StockData(context, useMkl, useCuda);
            PredictBicyclesWithNeuralNetwork(context, useMkl);
            MultiLabelSingleClassifier(context, useMkl);
            MultiLabelMultiClassifiers(context, useMkl);
            MnistFeedForward(context, useMkl);
            MnistConvolutional(context, useMkl, useCuda);
            TrainIncomePrediction(context, useMkl);
            SentimentClassification(context, useMkl, useCuda);
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

        static void Xor(BrightDataContext context, bool useMkl)
        {
            Start(context, useMkl);
            context.Xor().Train(4, 100, 0.5f, 4);
        }

        static void IrisClassification(BrightDataContext context, bool useMkl)
        {
            Start(context, useMkl);
            var iris = context.Iris();
            iris.TrainNaiveBayes();
            iris.TrainDecisionTree();
            iris.TrainRandomForest(500, 7);
            iris.TrainKNearestNeighbours(10);
            //iris.TrainMultinomialLogisticRegression(500, 0.3f, 0.1f);
            iris.TrainSigmoidNeuralNetwork(32, 200, 0.1f, 64, 50);
        }

        static void IrisClustering(BrightDataContext context, bool useMkl)
        {
            Start(context, useMkl);
            var iris = context.Iris();

            var irisTable = iris.Table.Value;

            static void Write(IEnumerable<(uint RowIndex, string? Label)[]> items)
            {
                var clusters = items.Select(c => c.Select(r => r.Label).GroupAndCount().Format());
                foreach (var cluster in clusters)
                    Console.WriteLine(cluster);
            }

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
        }

        static void MarkovChains(BrightDataContext context, bool useMkl)
        {
            Start(context, useMkl);
            context.BeautifulandDamned().TrainMarkovModel();
        }

        static void MnistFeedForward(BrightDataContext context, bool useMkl)
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

        static void SentimentClassification(BrightDataContext context, bool useMkl, bool useCuda)
        {
            Start(context, useMkl, useCuda);
            var sentiment = context.SentimentData();

            // train a bernoulli naive bayes classifier
            var bernoulli = sentiment.TrainBernoulli().CreateClassifier();

            // train a multinomial naive bayes classifier
            var multinomial = sentiment.TrainMultinomialNaiveBayes().CreateClassifier();

            var recurrent = sentiment.TrainBiLstm(bernoulli, multinomial);

            sentiment.TestClassifiers(bernoulli, multinomial, recurrent);
        }

        static void TextClustering(BrightDataContext context, bool useMkl)
        {
            Start(context, useMkl);
            var textClustering = context.TextClustering();
            textClustering.KMeans();
            textClustering.Nnmf();
            textClustering.RandomProjection();
            textClustering.LatentSemanticAnalysis();
        }

        static void IntegerAddition(BrightDataContext context, bool useMkl)
        {
            Start(context, useMkl);
            context.IntegerAddition().TrainRecurrentNeuralNetwork();
        }

        static void ReberPrediction(BrightDataContext context, bool useMkl)
        {
            Start(context, useMkl);
            var reber = context.ReberSequencePrediction(extended: true, minLength:10, maxLength:10);
            var engine = reber.TrainLstm();
            ReberSequenceTrainer.GenerateSequences(engine);
        }

        static void OneToMany(BrightDataContext context, bool useMkl)
        {
            Start(context, useMkl);
            var sequences = context.OneToMany();
            sequences.TrainOneToMany();
        }

        static void ManyToOne(BrightDataContext context, bool useMkl)
        {
            Start(context, useMkl);
            var sequences = context.ManyToOne();
            sequences.TrainManyToOne();
        }

        static void SequenceToSequence(BrightDataContext context, bool useMkl)
        {
            Start(context, useMkl);
            var sequences = context.SequenceToSequence();
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

        static void PredictBicyclesWithNeuralNetwork(BrightDataContext context, bool useMkl)
        {
            Start(context, useMkl);
            context.Bicycles().TrainNeuralNetwork();
        }

        static void MultiLabelSingleClassifier(BrightDataContext context, bool useMkl)
        {
            Start(context, useMkl);
            context.Emotions().TrainNeuralNetwork();
        }

        static void MultiLabelMultiClassifiers(BrightDataContext context, bool useMkl)
        {
            Start(context, useMkl);
            context.Emotions().TrainMultiClassifiers();
        }

        static void StockData(BrightDataContext context, bool useMkl, bool useCuda)
        {
            Start(context, useMkl, useCuda);
            var stockData = context.StockData().GetSequentialWindow();
            stockData.TrainLstm(256);
        }

        static void TrainIncomePrediction(BrightDataContext context, bool useMkl)
        {
            Start(context, useMkl);
            var adult = context.Adult();
            adult.TrainNeuralNetwork();
            //adult.TrainNaiveBayes();
        }
    }
}
