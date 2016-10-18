using BrightWire.Connectionist;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.SampleCode
{
    partial class Program
    {
        public static IDataTable IrisClassification(IDataTable dataTable = null)
        {
            if (dataTable == null) {
                // download the iris data set
                byte[] data;
                using (var client = new WebClient()) {
                    data = client.DownloadData("https://archive.ics.uci.edu/ml/machine-learning-databases/iris/iris.data");
                }

                // parse the iris CSV into a data table
                dataTable = new StreamReader(new MemoryStream(data)).ParseCSV(',');
            }

            // the last column is the classification target ("Iris-setosa", "Iris-versicolor", or "Iris-virginica")
            var targetColumnIndex = dataTable.TargetColumnIndex = dataTable.ColumnCount - 1;

            // split the data table into training and test tables
            var split = dataTable.Split(0);

            // train and evaluate a naive bayes classifier
            var naiveBayes = split.Training.TrainNaiveBayes();
            Console.WriteLine("Naive bayes accuracy: {0:P}", split.Test
                .Classify(naiveBayes.CreateClassifier())
                .Average(d => d.Row.GetField<string>(targetColumnIndex) == d.Classification ? 1.0 : 0.0)
            );

            // train and evaluate a decision tree classifier
            var decisionTree = split.Training.TrainDecisionTree();
            Console.WriteLine("Decision tree accuracy: {0:P}", split.Test
                .Classify(decisionTree.CreateClassifier())
                .Average(d => d.Row.GetField<string>(targetColumnIndex) == d.Classification ? 1.0 : 0.0)
            );

            // train and evaluate a random forest classifier
            var randomForest = dataTable.TrainRandomForest(500);
            Console.WriteLine("Random forest accuracy: {0:P}", split.Test
                .Classify(randomForest.CreateClassifier())
                .Average(d => d.Row.GetField<string>(targetColumnIndex) == d.Classification ? 1.0 : 0.0)
            );

            // fire up some linear algebra on the CPU
            using (var lap = Provider.CreateCPULinearAlgebra(false)) {
                // convert the data tables into linear algebra friendly training data providers
                var trainingData = lap.NN.CreateTrainingDataProvider(split.Training);
                var testData = lap.NN.CreateTrainingDataProvider(split.Test);

                // create a feed forward network with 8 hidden neurons
                const int BATCH_SIZE = 8, NUM_EPOCHS = 300;
                const float LEARNING_RATE = 0.03f;
                var layerTemplate = new LayerDescriptor(0.1f) { // add some L2 regularisation
                    Activation = ActivationType.Sigmoid, // sigmoid activation function
                    WeightUpdate = WeightUpdateType.RMSprop, // use rmsprop gradient descent optimisation
                    WeightInitialisation = WeightInitialisationType.Xavier, // xavier weight initialisation
                    LayerTrainer = LayerTrainerType.DropConnect // throw in some drop connect regularisation for fun
                };

                // the default data table -> vector conversion uses one hot encoding of the classification labels, so create a corresponding cost function
                var errorMetric = ErrorMetricType.OneHot.Create();

                // create a network trainer and evaluate against the test set after every 50 epochs
                Console.WriteLine("Training a 4x8x3 neural network...");
                using (var trainer = lap.NN.CreateBatchTrainer(layerTemplate, trainingData.InputSize, 8, trainingData.OutputSize)) {
                    var trainingContext = lap.NN.CreateTrainingContext(LEARNING_RATE, BATCH_SIZE, errorMetric);
                    trainingContext.EpochComplete += c => {
                        if (c.CurrentEpoch % 50 == 0) {
                            var testError = trainer.Execute(testData).Select(d => errorMetric.Compute(d.Output, d.ExpectedOutput)).Average();
                            trainingContext.WriteScore(testError, errorMetric.DisplayAsPercentage);
                        }
                    };
                    trainer.Train(trainingData, NUM_EPOCHS, trainingContext);
                }
                Console.WriteLine();

                // let's unload some deep learning on these flowers...
                Console.WriteLine("Training a 4x8x16x32x16x8x3 neural network...");
                using (var deepTrainer = lap.NN.CreateBatchTrainer(layerTemplate, trainingData.InputSize, 8, 16, 32, 16, 8, trainingData.OutputSize)) {
                    var trainingContext = lap.NN.CreateTrainingContext(LEARNING_RATE, BATCH_SIZE, errorMetric);
                    trainingContext.EpochComplete += c => {
                        if (c.CurrentEpoch % 50 == 0) {
                            var testError = deepTrainer.Execute(testData).Select(d => errorMetric.Compute(d.Output, d.ExpectedOutput)).Average();
                            trainingContext.WriteScore(testError, errorMetric.DisplayAsPercentage);
                        }
                    };
                    deepTrainer.Train(trainingData, NUM_EPOCHS, trainingContext);
                }
                Console.WriteLine();
            }
            return dataTable;
        }
    }
}
