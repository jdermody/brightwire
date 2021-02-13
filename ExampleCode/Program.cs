using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using BrightData;
using BrightData.Cuda;
using BrightData.Numerics;
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
            using var context = new BrightDataContext(RandomSeed);
            var useCuda = false;

            // IMPORTANT: uncomment below if you have installed native Intel Math Kernel Library binaries as described in https://numerics.mathdotnet.com/MKL.html
            //Control.UseNativeMKL();

            // IMPORTANT: uncomment below to use CUDA (if you have installed the CUDA toolkit from https://developer.nvidia.com/cuda-toolkit and have a supported Nvidia GPU)
            useCuda = true;

            // set where to save training data files
            context.Set("DataFileDirectory", new DirectoryInfo(@"c:\data"));

            //Xor(context);
            //IrisClassification(context);
            //IrisClustering(context);
            //MarkovChains(context);
            //TextClustering(context);
            IntegerAddition(context);
            //ReberPrediction(context);
            //OneToMany(context, useCuda);
            //ManyToOne(context, useCuda);
            //SequenceToSequence(context, useCuda);
            //TrainWithSelu(context);
            //StockData(context, useCuda);
            //SimpleLinearTest(context);
            //PredictBicyclesWithLinearModel(context);
            //PredictBicyclesWithNeuralNetwork(context);
            //MultiLabelSingleClassifier(context);
            //MultiLabelMultiClassifiers(context);
            //MnistFeedForward(context, useCuda);
            //MnistConvolutional(context, useCuda);
            //SentimentClassification(context, useCuda);
        }

        static void Start(IBrightDataContext context, bool useCuda = false, [CallerMemberName]string title = "")
        {
            context.ResetRandom(RandomSeed);

            Console.WriteLine("*********************************************");
            Console.WriteLine("*");
            Console.WriteLine($"*  {title}");
            if (useCuda) {
                context.UseCudaLinearAlgebra();
                Console.WriteLine("* (CUDA)");
            }
            else {
                context.UseNumericsLinearAlgebra();
                Console.WriteLine("* (Numerics)");
            }
            Console.WriteLine($"*");
            Console.WriteLine("*********************************************");
        }

        static void WriteSeparator()
        {
            Console.WriteLine("---------------------------------------------");
        }

        static void Xor(IBrightDataContext context)
        {
            Start(context);
            context.Xor().TrainSigmoidNeuralNetwork(6, 50, 0.5f, 4);
        }

        static void IrisClassification(IBrightDataContext context)
        {
            Start(context);
            var iris = context.Iris();
            iris.TrainNaiveBayes();
            iris.TrainDecisionTree();
            iris.TrainRandomForest(500, 7);
            iris.TrainKNearestNeighbours(10);
            iris.TrainMultinomialLogisticRegression(500, 0.3f, 0.1f);
            iris.TrainSigmoidNeuralNetwork(8, 500, 0.1f, 16);
        }

        static void IrisClustering(IBrightDataContext context)
        {
            Start(context);
            var iris = context.Iris();

            // select only the first three columns (ignore the training label)
            var irisTable = iris.Table.AsColumnOriented().CopyColumns(3.AsRange().ToArray());

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

        static void MarkovChains(IBrightDataContext context)
        {
            Start(context);
            context.BeautifulandDamned().TrainMarkovModel();
        }

        static void MnistFeedForward(IBrightDataContext context, bool useCuda)
        {
            Start(context, useCuda);
            context.Mnist().TrainFeedForwardNeuralNetwork();
        }

        static void MnistConvolutional(IBrightDataContext context, bool useCuda)
        {
            Start(context, useCuda);
            context.Mnist().TrainConvolutionalNeuralNetwork();
        }

        static void SentimentClassification(IBrightDataContext context, bool useCuda)
        {
            Start(context, useCuda);
            var sentiment = context.SentimentData();

            // train a bernoulli naive bayes classifier
            var bernoulli = sentiment.TrainBernoulli().CreateClassifier();

            // train a multinomial naive bayes classifier
            var multinomial = sentiment.TrainMultinomialNaiveBayes().CreateClassifier();

            // train a neural network
            var (_, _, neuralNetwork) = sentiment.TrainNeuralNetwork(20);

            // train a combined graph with all three classifiers
            //sentiment.StackClassifiers(engine, wire, bernoulli, multinomial);

            sentiment.TestClassifiers(bernoulli, multinomial, neuralNetwork);
        }

        static void TextClustering(IBrightDataContext context)
        {
            Start(context);
            var textClustering = context.TextClustering();
            textClustering.KMeans();
            textClustering.Nnmf();
            textClustering.RandomProjection();
            textClustering.LatentSemanticAnalysis();
        }

        static void IntegerAddition(IBrightDataContext context)
        {
            Start(context);
            context.IntegerAddition().TrainRecurrentNeuralNetwork();
        }

        static void ReberPrediction(IBrightDataContext context)
        {
            Start(context);
            var reber = context.ReberSequencePrediction();
            var engine = reber.TrainGru();
            reber.GenerateSequences(engine);
        }

        static void OneToMany(IBrightDataContext context, bool useCuda)
        {
            Start(context, useCuda);
            var sequences = context.OneToMany();
            sequences.TrainLstm();
        }

        static void ManyToOne(IBrightDataContext context, bool useCuda)
        {
            Start(context, useCuda);
            var sequences = context.ManyToOne();
            sequences.TrainGru();
        }

        static void SequenceToSequence(IBrightDataContext context, bool useCuda)
        {
            Start(context, useCuda);
            var sequences = context.SequenceToSequence();
            sequences.TrainSequenceToSequence();
        }

        static void TrainWithSelu(IBrightDataContext context)
        {
            Start(context);
            context.Iris().TrainWithSelu();
        }

        static void SimpleLinearTest(IBrightDataContext context)
        {
            Start(context);
            context.SimpleLinear().TrainLinearRegression();
        }

        static void PredictBicyclesWithLinearModel(IBrightDataContext context)
        {
            Start(context);
            context.Bicycles().TrainLinearModel();
        }

        static void PredictBicyclesWithNeuralNetwork(IBrightDataContext context)
        {
            Start(context);
            context.Bicycles().TrainNeuralNetwork();
        }

        static void MultiLabelSingleClassifier(IBrightDataContext context)
        {
            Start(context);
            context.Emotions().TrainNeuralNetwork();
        }

        static void MultiLabelMultiClassifiers(IBrightDataContext context)
        {
            Start(context);
            context.Emotions().TrainMultiClassifiers();
        }

        static void StockData(IBrightDataContext context, bool useCuda)
        {
            Start(context, useCuda);
            var stockData = context.StockData().GetSequentialWindow();
            stockData.TrainLstm(256);
        }
    }
}
