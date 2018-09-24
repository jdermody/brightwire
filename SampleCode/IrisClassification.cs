using BrightWire.ExecutionGraph;
using System;
using System.IO;
using System.Linq;
using System.Net;

namespace BrightWire.SampleCode
{
    partial class Program
    {
        /// <summary>
        /// Trains various classifiers on the Iris data set
        /// 
        /// Tutorial available at http://www.jackdermody.net/brightwire/article/Introduction_to_Bright_Wire
        /// </summary>
        public static void IrisClassification()
        {
            // download the iris data set
            byte[] data;
            using (var client = new WebClient()) {
                data = client.DownloadData("https://archive.ics.uci.edu/ml/machine-learning-databases/iris/iris.data");
            }

            // parse the iris CSV into a data table
            var dataTable = new StreamReader(new MemoryStream(data)).ParseCSV(',');

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
            var randomForest = split.Training.TrainRandomForest(500);
            Console.WriteLine("Random forest accuracy: {0:P}", split.Test
                .Classify(randomForest.CreateClassifier())
                .Average(d => d.Row.GetField<string>(targetColumnIndex) == d.Classification ? 1.0 : 0.0)
            );

            // fire up some linear algebra on the CPU
            using (var lap = BrightWireProvider.CreateLinearAlgebra(false)) {
                // train and evaluate k nearest neighbours
                var knn = split.Training.TrainKNearestNeighbours();
                Console.WriteLine("K nearest neighbours accuracy: {0:P}", split.Test
                    .Classify(knn.CreateClassifier(lap, 10))
                    .Average(d => d.Row.GetField<string>(targetColumnIndex) == d.Classification ? 1.0 : 0.0)
                );

                // train and evaluate a mulitinomial logistic regression classifier
                var logisticRegression = split.Training.TrainMultinomialLogisticRegression(lap, 500, 0.1f);
                Console.WriteLine("Multinomial logistic regression accuracy: {0:P}", split.Test
                    .Classify(logisticRegression.CreateClassifier(lap))
                    .Average(d => d.Row.GetField<string>(targetColumnIndex) == d.Classification ? 1.0 : 0.0)
                );

                // create a neural network graph factory
                var graph = new GraphFactory(lap);

                // the default data table -> vector conversion uses one hot encoding of the classification labels, so create a corresponding cost function
                var errorMetric = graph.ErrorMetric.OneHotEncoding;

                // create the property set (use rmsprop gradient descent optimisation)
                graph.CurrentPropertySet
                    .Use(graph.RmsProp())
                ;

                // create the training and test data sources
                var trainingData = graph.CreateDataSource(split.Training);
                var testData = trainingData.CloneWith(split.Test);

                // create a 4x3x3 neural network with sigmoid activations after each neural network
                const int HIDDEN_LAYER_SIZE = 8, BATCH_SIZE = 8;
                const float LEARNING_RATE = 0.01f;
                var engine = graph.CreateTrainingEngine(trainingData, LEARNING_RATE, BATCH_SIZE);
                graph.Connect(engine)
                    .AddFeedForward(HIDDEN_LAYER_SIZE)
                    .Add(graph.SigmoidActivation())
                    .AddDropOut(dropOutPercentage: 0.5f)
                    .AddFeedForward(engine.DataSource.OutputSize)
                    .Add(graph.SigmoidActivation())
                    .AddBackpropagation(errorMetric)
                ;

                // train the network
                Console.WriteLine("Training a 4x8x3 neural network...");
                engine.Train(500, testData, errorMetric, null, 50);
            }
        }
    }
}
