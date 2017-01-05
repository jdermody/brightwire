using BrightWire.Connectionist;
using BrightWire.Connectionist.Helper;
using BrightWire.Connectionist.Training.Layer.Convolutional;
using BrightWire.DimensionalityReduction;
using BrightWire.Ensemble;
using BrightWire.Helper;
using BrightWire.Models.Convolutional;
using BrightWire.Models.Input;
using BrightWire.Models.Output;
using BrightWire.TrainingData;
using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.SampleCode
{
    partial class Program
    {
        static void Main(string[] args)
        {
            //Control.UseNativeMKL();
            //ReducedMNIST(@"D:\data\mnist\");
            //MNISTConvolutional(@"D:\data\mnist\");

            //IrisClassification();
            //IrisClustering();
            //MarkovChains();
            //MNIST(@"D:\data\mnist\", @"D:\data\mnist\model_test.dat");
            //SentimentClassification(@"D:\data\sentiment labelled sentences\");
            //TextClustering(@"D:\data\[UCI] AAAI-14 Accepted Papers - Papers.csv", @"d:\temp\");
            //IntegerAddition();
            //IncomePrediction(@"d:\data\adult.data", @"d:\data\adult.test");
        }
    }
}
