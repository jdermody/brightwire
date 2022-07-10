using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using BrightData;
using BrightData.Cuda;
using BrightData.MKL;
using BrightWire;
using ExampleCode.DataSet;
// ReSharper disable once RedundantUsingDirective
using MathNet.Numerics;

namespace ExampleCode
{
    internal class Program
    {
        const int RandomSeed = 0;

        static void Main()
        {
            using var context = new BrightDataContext(null, RandomSeed);
            var useCuda = false;

            //context.UseMKL();
            // IMPORTANT: uncomment below if you have installed native Intel Math Kernel Library binaries as described in https://numerics.mathdotnet.com/MKL.html
            //context.UseMKL();

            // IMPORTANT: uncomment below to use CUDA (if you have installed the CUDA toolkit from https://developer.nvidia.com/cuda-toolkit and have a supported Nvidia GPU)
            //useCuda = true;

            // IMPORTANT: set where to save training data files
            context.Set("DataFileDirectory", new DirectoryInfo(@"c:\data"));

            PerformanceTest.Run(context.LinearAlgebraProvider2, new MklLinearAlgebraProvider(context), new CudaLinearAlgebraProvider(context));
            //Xor(context);
            //IrisClassification(context);
            //IrisClustering(context);
            //MarkovChains(context);
            //TextClustering(context);
            //IntegerAddition(context);
            //ReberPrediction(context);
            //OneToMany(context, useCuda);
            //ManyToOne(context, useCuda);
            //SequenceToSequence(context, useCuda);
            //StockData(context, useCuda);
            //SimpleLinearTest(context);
            //PredictBicyclesWithLinearModel(context);
            //PredictBicyclesWithNeuralNetwork(context);
            //MultiLabelSingleClassifier(context);
            //MultiLabelMultiClassifiers(context);
            //MnistFeedForward(context);
            //MnistConvolutional(context, useCuda);
            //TrainIncomePrediction(context);
            //SentimentClassification(context, useCuda);
        }

        static void Start(BrightDataContext context, bool useCuda = false, [CallerMemberName]string title = "")
        {
            context.ResetRandom(RandomSeed);

            Console.WriteLine("*********************************************");
            Console.WriteLine("*");
            Console.WriteLine($"* {title}");
            if (useCuda) {
                context.CreateCudaProvider();
                Console.WriteLine("* (CUDA)");
            }
            else {
                Console.WriteLine("* (Standard)");
            }
            Console.WriteLine("*");
            Console.WriteLine("*********************************************");
        }

        static void WriteSeparator()
        {
            Console.WriteLine("---------------------------------------------");
        }

        static void Xor(BrightDataContext context)
        {
            Start(context);
            context.Xor().TrainSigmoidNeuralNetwork(4, 100, 0.5f, 4, 10);
        }

        static void IrisClassification(BrightDataContext context)
        {
            Start(context);
            var iris = context.Iris();
            //iris.TrainNaiveBayes();
            //iris.TrainDecisionTree();
            //iris.TrainRandomForest(500, 7);
            //iris.TrainKNearestNeighbours(10);
            //iris.TrainMultinomialLogisticRegression(500, 0.3f, 0.1f);
            iris.TrainSigmoidNeuralNetwork(32, 200, 0.1f, 64, 50);
        }

        static void IrisClustering(BrightDataContext context)
        {
            Start(context);
            var iris = context.Iris();

            // select only the first three columns (ignore the training label)
            var irisTable = iris.Table.Value.CopyColumnsToNewTable(null, 3.AsRange().ToArray());

            void Write(IEnumerable<(uint RowIndex, string? Label)[]> items)
            {
                var clusters = items.Select(c => c.Select(r => iris.Labels[r.RowIndex]).GroupAndCount().Format());
                foreach (var cluster in clusters)
                    Console.WriteLine(cluster);
            }

            Console.WriteLine("K Means...");
            WriteSeparator();
            Write(irisTable.KMeans(3));
            WriteSeparator();

            Console.WriteLine();
            Console.WriteLine("Hierachical...");
            WriteSeparator();
            Write(irisTable.HierarchicalCluster(3));
            WriteSeparator();

            Console.WriteLine();
            Console.WriteLine("NNMF...");
            WriteSeparator();
            Write(irisTable.NonNegativeMatrixFactorisation(3));
            WriteSeparator();
        }

        static void MarkovChains(BrightDataContext context)
        {
            Start(context);
            context.BeautifulandDamned().TrainMarkovModel();
        }

        static void MnistFeedForward(BrightDataContext context)
        {
            Start(context);
            context.Mnist().TrainFeedForwardNeuralNetwork();
        }

        static void MnistConvolutional(BrightDataContext context, bool useCuda)
        {
            Start(context, useCuda);
            context.Mnist().TrainConvolutionalNeuralNetwork();
        }

        static void SentimentClassification(BrightDataContext context, bool useCuda)
        {
            Start(context, useCuda);
            var sentiment = context.SentimentData();

            // train a bernoulli naive bayes classifier
            var bernoulli = sentiment.TrainBernoulli().CreateClassifier();

            // train a multinomial naive bayes classifier
            var multinomial = sentiment.TrainMultinomialNaiveBayes().CreateClassifier();

            var recurrent = sentiment.TrainBiLstm(bernoulli, multinomial);

            sentiment.TestClassifiers(bernoulli, multinomial, recurrent);
        }

        static void TextClustering(BrightDataContext context)
        {
            Start(context);
            var textClustering = context.TextClustering();
            textClustering.KMeans();
            textClustering.Nnmf();
            textClustering.RandomProjection();
            //textClustering.LatentSemanticAnalysis();
        }

        static void IntegerAddition(BrightDataContext context)
        {
            Start(context);
            context.IntegerAddition().TrainRecurrentNeuralNetwork();
        }

        static void ReberPrediction(BrightDataContext context)
        {
            Start(context);
            var reber = context.ReberSequencePrediction();
            var engine = reber.TrainLstm();
            reber.GenerateSequences(engine);
        }

        static void OneToMany(BrightDataContext context, bool useCuda)
        {
            Start(context, useCuda);
            var sequences = context.OneToMany();
            sequences.TrainOneToMany();
        }

        static void ManyToOne(BrightDataContext context, bool useCuda)
        {
            Start(context, useCuda);
            var sequences = context.ManyToOne();
            sequences.TrainManyToOne();
        }

        static void SequenceToSequence(BrightDataContext context, bool useCuda)
        {
            Start(context, useCuda);
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

        static void PredictBicyclesWithNeuralNetwork(BrightDataContext context)
        {
            Start(context);
            context.Bicycles().TrainNeuralNetwork();
        }

        static void MultiLabelSingleClassifier(BrightDataContext context)
        {
            Start(context);
            context.Emotions().TrainNeuralNetwork();
        }

        static void MultiLabelMultiClassifiers(BrightDataContext context)
        {
            Start(context);
            context.Emotions().TrainMultiClassifiers();
        }

        static void StockData(BrightDataContext context, bool useCuda)
        {
            Start(context, useCuda);
            var stockData = context.StockData().GetSequentialWindow();
            stockData.TrainLstm(256);
        }

        static void TrainIncomePrediction(BrightDataContext context)
        {
            Start(context);
            var adult = context.Adult();
            adult.TrainNeuralNetwork();
            //adult.TrainNaiveBayes();
        }
    }
}
