using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.SampleCode
{
    public partial class Program
    {
        public static IDataTable KNearestNeighbours(IDataTable dataTable = null)
        {
            const int K = 10;

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
            var featureColumns = Enumerable.Range(0, targetColumnIndex - 1).ToList();

            // split the data table into training and test tables
            var split = dataTable.Split(0);

            using (var lap = Provider.CreateCPULinearAlgebra()) {
                // get the training data feaure columns and corresponding classification labels
                var trainingData = split.Training.GetNumericRows(lap, featureColumns);
                var trainingDataLabels = split.Training.GetColumn<string>(targetColumnIndex);

                // classify each row using the weighted sum of its K closest neighbours
                var predictions = new List<string>();
                foreach(var item in split.Test.GetNumericRows(lap, featureColumns)) {
                    var distances = item.FindDistances(trainingData, DistanceMetric.Euclidean).AsIndexable();
                    var classification = distances.Values
                        .Zip(trainingDataLabels, (s, l) => Tuple.Create(l, s))
                        .OrderBy(d => d.Item2)
                        .Take(K)
                        .GroupBy(d => d.Item1)
                        .Select(g => Tuple.Create(g.Key, g.Sum(d => 1f / d.Item2)))
                        .OrderByDescending(d => d.Item2)
                        .First()
                    ;
                    predictions.Add(classification.Item1);
                }

                // compare the predictions to the actual test labels
                var testDataLabels = split.Training.GetColumn<string>(targetColumnIndex);
                var score = predictions.Zip(testDataLabels, (p, a) => p == a ? 1 : 0).Average();
                Console.WriteLine("KNN classification accuracy: {0:P}", score);
            }
            return dataTable;
        }
    }
}
