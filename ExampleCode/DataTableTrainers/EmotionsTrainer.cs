using System;
using System.IO;
using System.Linq;
using System.Text;
using BrightData;
using BrightWire;
using BrightWire.Models;
using BrightDataTable = BrightData.DataTable.BrightDataTable;

namespace ExampleCode.DataTableTrainers
{
    internal class EmotionsTrainer : DataTableTrainer
    {
        public EmotionsTrainer(BrightDataTable table, BrightDataTable training, BrightDataTable test) : base(table, training, test)
        {
        }

        public static BrightDataTable Parse(BrightDataContext context, string filePath)
        {
            const int targetColumnCount = 6;

            // read the data as CSV, skipping the header
            using var reader = new StreamReader(filePath);
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (line == "@data")
                    break;
            }
            using var table = context.ParseCsv(reader, false);

            // convert the feature columns to numeric and the target columns to boolean
            var featureColumns = (table.ColumnCount - targetColumnCount).AsRange().ToArray();
            var targetColumns = targetColumnCount.AsRange((int)table.ColumnCount - targetColumnCount).ToArray();
            var columnConversions = featureColumns
                .Select(i => ColumnConversionOperation.ToNumeric.ConvertColumn(i))
                .Concat(targetColumns.Select(i => ColumnConversionOperation.ToBoolean.ConvertColumn(i)))
                .ToArray();
            using var converted = table.Convert(columnConversions);

            // convert the many feature columns to an index list and set that as the feature column
            using var tempStreams = context.CreateTempStreamProvider();
            var reinterpreted = converted.ReinterpretColumns(tempStreams, filePath, targetColumns.ReinterpretColumns(BrightDataType.IndexList, "Targets"));
            reinterpreted.SetTargetColumn(reinterpreted.ColumnCount-1);

            return reinterpreted.Normalize(featureColumns.Select(i => NormalizationType.FeatureScale.ConvertColumn(i)).ToArray());

        }

        static BrightDataTable ConvertToBinary(BrightDataTable table, uint indexOffset)
        {
            // converts the index list to a boolean based on if the flag is set
            var ret = table.Project(null, r => {
                var ret2 = (object[]) r.Clone();
                var indexList = (IndexList) ret2[r.Length - 1];
                ret2[r.Length - 1] = indexList.HasIndex(indexOffset);
                return ret2;
            });
            ret.SetTargetColumn(ret.ColumnCount-1);
            return ret;
        }

        public void TrainNeuralNetwork()
        {
            var graph = _context.CreateGraphFactory();

            // binary classification rounds each output to 0 or 1 and compares each output against the binary classification targets
            var errorMetric = graph.ErrorMetric.BinaryClassification;

            // configure the network properties
            graph.CurrentPropertySet
                .Use(graph.GradientDescent.Adam)
                .Use(graph.WeightInitialisation.Xavier)
            ;

            // create a training engine
            const float trainingRate = 0.3f;
            var trainingData = graph.CreateDataSource(Training);
            var testData = trainingData.CloneWith(Test);
            var engine = graph.CreateTrainingEngine(trainingData, errorMetric, trainingRate);

            // build the network
            const int hiddenLayerSize = 64, trainingIterations = 2000;
            graph.Connect(engine)
                .AddFeedForward(hiddenLayerSize)
                .Add(graph.SigmoidActivation())
                .AddDropOut(dropOutPercentage: 0.5f)
                .AddFeedForward(engine.DataSource.GetOutputSizeOrThrow())
                .Add(graph.SigmoidActivation())
                .AddBackpropagation();

            // train the network
            ExecutionGraphModel? bestGraph = null;
            engine.Train(trainingIterations, testData, model => bestGraph = model.Graph, 50);

            // export the final model and execute it on the training set
            var executionEngine = graph.CreateExecutionEngine(bestGraph ?? engine.Graph);
            var output = executionEngine.Execute(testData);

            // output the results
            var rowIndex = 0;
            foreach (var item in output) {
                var sb = new StringBuilder();
                foreach (var (vector, target) in item.Output.Zip(item.Target!, (o, t) => (Output: o, Target: t))) {
                    var columnIndex = 0;
                    sb.AppendLine($"{rowIndex++}) ");
                    for(var i = 0; i < vector.Size; i++) {
                        var prediction = vector[i] >= 0.5f ? "true" : "false";
                        var actual = target[i] >= 0.5f ? "true" : "false";
                        sb.AppendLine($"\t{columnIndex++}) predicted {prediction} (expected {actual})");
                    }
                }

                Console.WriteLine(sb.ToString());
            }
        }

        public void TrainMultiClassifiers()
        {
            var classificationLabel = new[] {
                "amazed-suprised",
                "happy-pleased",
                "relaxing-calm",
                "quiet-still",
                "sad-lonely",
                "angry-aggresive"
            };
            var trainingTables = classificationLabel
                .Select((c, i) => (Table: ConvertToBinary(Training, (uint) i), Label: c))
                .ToArray();
            var testTables = classificationLabel
                .Select((c, i) => (Table: ConvertToBinary(Test, (uint)i), Label: c))
                .ToArray();

            var graph = _context.CreateGraphFactory();

            // binary classification rounds each output to 0 or 1 and compares each output against the binary classification targets
            var errorMetric = graph.ErrorMetric.BinaryClassification;

            // configure the network properties
            graph.CurrentPropertySet
                .Use(graph.GradientDescent.Adam)
                .Use(graph.WeightInitialisation.Xavier)
            ;

            var targetColumn = Table.Value.GetTargetColumnOrThrow();

            foreach (var item in trainingTables.Zip(testTables, (train, test) => (Training: train, Test: test))) {
                Console.WriteLine("Training on {0}", item.Training.Label);

                // train and evaluate a naive bayes classifier
                var naiveBayes = item.Training.Table.TrainNaiveBayes().CreateClassifier();
                Console.WriteLine("\tNaive bayes accuracy: {0:P}", item.Test.Table
                    .Classify(naiveBayes)
                    .Average(d => d.Row.Get<string>(targetColumn) == d.Classification.First().Label ? 1.0 : 0.0)
                );

                // train a logistic regression classifier
                //var logisticRegression = item.Training.Table
                //    .TrainMultinomialLogisticRegression(2500, 0.25f, 0.01f)
                //    .CreateClassifier(_context.LinearAlgebraProvider2)
                //;

                //var convertible = item.Test.Table.AsConvertible();
                //Console.WriteLine("\tLogistic regression accuracy: {0:P}", logisticRegression.Classify(item.Test.Table)
                //    .Average(d => convertible.Row(d.RowIndex).GetTyped<string>(targetColumn) == d.Predictions.First().Classification ? 1.0 : 0.0)
                //);

                // train and evaluate k nearest neighbours
                var knn = item.Training.Table.TrainKNearestNeighbours().CreateClassifier(_context.LinearAlgebraProvider, 10);
                Console.WriteLine("\tK nearest neighbours accuracy: {0:P}", item.Test.Table
                    .Classify(knn)
                    .Average(d => d.Row.Get<string>(targetColumn) == d.Classification.First().Label ? 1.0 : 0.0)
                );

                // create a training engine
                const float trainingRate = 0.1f;
                var trainingData = graph.CreateDataSource(item.Training.Table);
                var testData = trainingData.CloneWith(item.Test.Table);
                var engine = graph.CreateTrainingEngine(trainingData, errorMetric, trainingRate, 64);

                // build the network
                const int hiddenLayerSize = 64, trainingIterations = 1000;
                graph.Connect(engine)
                    .AddFeedForward(hiddenLayerSize)
                    .Add(graph.SigmoidActivation())
                    .AddDropOut(dropOutPercentage: 0.5f)
                    .AddFeedForward(engine.DataSource.GetOutputSizeOrThrow())
                    .Add(graph.SigmoidActivation())
                    .AddBackpropagation()
                ;

                // train the network
                engine.Train(trainingIterations, testData, null, 200);
            }
        }
    }
}
