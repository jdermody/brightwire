using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using BrightData;
using BrightData.Cuda;
using BrightData.Numerics;
using BrightTable;
using BrightWire;
using ExampleCode.Datasets;
using MathNet.Numerics;

namespace ExampleCode
{
    class Program
    {
        static void Main(string[] args)
        {
            using var context = new BrightDataContext(0);
            var useCuda = false;

            // performance can be improved using the Intel Math Kernel Library...
            // IMPORTANT: uncomment below if you have installed native binaries as described in https://numerics.mathdotnet.com/MKL.html
            //Control.UseNativeMKL();

            // IMPORTANT: uncomment below to use CUDA (if you have installed the CUDA toolkit from https://developer.nvidia.com/cuda-toolkit and have a valid NVidia GPU)
            //useCuda = true;

            // set where to save training data files
            context.Set("DataFileDirectory", new DirectoryInfo(@"c:\data"));

            Xor(context);
            //IrisClassification(context);
            //IrisClustering(context);
            //MarkovChains(context);
            //MNIST(context, useCuda);
            //MNISTConvolutional(context, useCuda);
            //SentimentClassification(context);
            //TextClustering(context);
            //IntegerAddition(context);
            //ReberPrediction(context);
            //OneToMany(context);
            //ManyToOne(context);
            //SequenceToSequence(context);
            //TrainWithSelu(context);
            //SimpleLinearTest(context);
            //PredictBicyclesWithLinearModel(context);
            //PredictBicyclesWithNeuralNetwork(context);
            //MultiLabelSingleClassifier(context);
            //MultiLabelMultiClassifiers(context);
            //StockData(context);
        }

        static void WriteTitle(string title)
        {
            Console.WriteLine("*********************************************");
            Console.WriteLine($"*");
            Console.WriteLine($"*  {title}");
            Console.WriteLine($"*");
            Console.WriteLine("*********************************************");
        }

        static void WriteSeparator()
        {
            Console.WriteLine("---------------------------------------------");
        }

        static void Xor(IBrightDataContext context)
        {
            WriteTitle("XOR");
            context.UseNumericsLinearAlgebra();
            context.Xor().TrainSigmoidNeuralNetwork(6, 300, 0.5f, 4);
        }

        static void IrisClassification(IBrightDataContext context)
        {
            WriteTitle("Iris Classification");
            context.UseNumericsLinearAlgebra();
            var iris = context.Iris();
            iris.TrainNaiveBayes();
            iris.TrainDecisionTree();
            iris.TrainRandomForest(500, 7);
            iris.TrainKNearestNeighbours(10);
            iris.TrainMultinomialLogisticRegression(500, 0.3f);
            iris.TrainLegacyMultinomialLogisticRegression(500, 0.3f, 0.1f);
            iris.TrainSigmoidNeuralNetwork(4, 500, 0.1f, 16);
        }

        static void IrisClustering(IBrightDataContext context)
        {
            WriteTitle("Iris Clustering");
            context.UseNumericsLinearAlgebra();
            var iris = context.Iris();

            // select only the first three columns (ignore the training label)
            var irisTable = iris.Table.AsColumnOriented().SelectColumns(3.AsRange().ToArray());

            void Write(IEnumerable<(uint RowIndex, string Label)[]> items)
            {
                var clusters = items.Select(c => c.Select(r => iris.Labels[r.RowIndex]).GroupAndCount().Format());
                foreach (var cluster in clusters)
                    Console.WriteLine(cluster);
            }

            Console.WriteLine("K Means...");
            Console.WriteLine("---------------------------------------------");
            Write(irisTable.KMeans(3));
            Console.WriteLine("---------------------------------------------");

            Console.WriteLine();
            Console.WriteLine("Hierachical...");
            Console.WriteLine("---------------------------------------------");
            Write(irisTable.HierachicalCluster(3));
            Console.WriteLine("---------------------------------------------");

            Console.WriteLine();
            Console.WriteLine("NNMF...");
            Console.WriteLine("---------------------------------------------");
            Write(irisTable.NonNegativeMatrixFactorisation(3));
            Console.WriteLine("---------------------------------------------");
        }

        static void MarkovChains(IBrightDataContext context)
        {
            WriteTitle("Markov Chains");
            context.UseNumericsLinearAlgebra();
            context.BeautifulandDamned().TrainMarkovModel();
        }

        static void MNIST(IBrightDataContext context, bool useCuda)
        {
            WriteTitle("MNIST (Feed Forward)");
            if(useCuda)
                context.UseCudaLinearAlgebra();
            else
                context.UseNumericsLinearAlgebra();
            context.MNIST().TrainFeedForwardNeuralNetwork();
        }

        static void MNISTConvolutional(IBrightDataContext context, bool useCuda)
        {
            WriteTitle("MNIST (Convolutional)");
            if (useCuda)
                context.UseCudaLinearAlgebra();
            else
                context.UseNumericsLinearAlgebra();
            context.MNIST().TrainConvolutionalNeuralNetwork();
        }

        static void SentimentClassification(IBrightDataContext context)
        {
            WriteTitle("Sentiment Classification");
            context.UseNumericsLinearAlgebra();
            var sentiment = context.SentimentData();

            // train a bernoulli naive bayes classifier
            var bernoulli = sentiment.TrainBernoulli();

            // train a multinomial naive bayes classifier
            var multinomial = sentiment.TrainMultinomialNaiveBayes();
        }

        static void TextClustering(IBrightDataContext context)
        {

        }

        static void IntegerAddition(IBrightDataContext context)
        {
            context.IntegerAddition().TrainRecurrentNeuralNetwork();
        }

        static void ReberPrediction(IBrightDataContext context)
        {
            
        }

        static void OneToMany(IBrightDataContext context)
        {
        }

        static void ManyToOne(IBrightDataContext context)
        {
        }

        static void SequenceToSequence(IBrightDataContext context)
        {
        }

        static void TrainWithSelu(IBrightDataContext context)
        {
        }

        static void SimpleLinearTest(IBrightDataContext context)
        {
        }

        static void PredictBicyclesWithLinearModel(IBrightDataContext context)
        {
        }

        static void PredictBicyclesWithNeuralNetwork(IBrightDataContext context)
        {
        }

        static void MultiLabelSingleClassifier(IBrightDataContext context)
        {
        }

        static void MultiLabelMultiClassifiers(IBrightDataContext context)
        {
        }

        static void StockData(IBrightDataContext context)
        {
            var stockData = context.StockData().GetSequentialWindow();
            stockData.TrainLSTM(256);
        }
    }
}
