using System;
using System.IO;
using System.Net.Http;
using BrightData;
using BrightTable;
using BrightWire.TrainingData.Artificial;

namespace ExampleCode.Datasets
{
    static class Datasets
    {
        public static DataTableTrainer Iris(this IBrightDataContext context)
        {
            var reader = GetReader(context, "iris.csv", "https://archive.ics.uci.edu/ml/machine-learning-databases/iris/iris.data");
            try {
                using var table = context.ParseCsv(reader, false);
                table.SetTargetColumn(4);
                using var numericTable = table.Convert(
                    ColumnConversionType.ToNumeric,
                    ColumnConversionType.ToNumeric,
                    ColumnConversionType.ToNumeric,
                    ColumnConversionType.ToNumeric);
                using var normalized = numericTable.Normalize(NormalizationType.Euclidean);
                return new DataTableTrainer(normalized.AsRowOriented());
            }
            finally {
                reader?.Dispose();
            }
        }

        public static DataTableTrainer Xor(this IBrightDataContext context)
        {
            // Some training data that the network will learn.  The XOR pattern looks like:
            // 0 0 => 0
            // 1 0 => 1
            // 0 1 => 1
            // 1 1 => 0
            var table = BrightWire.TrainingData.Artificial.Xor.Get(context);
            return new DataTableTrainer(table, table, table);
        }

        public static DataTableTrainer IntegerAddition(this IBrightDataContext context)
        {
            var data = BinaryIntegers.Addition(context, 1000);
            var split = data.Split();
            return new DataTableTrainer(data, split.Training, split.Test);
        }

        static string GetDataFilePath(this IBrightDataContext context, string name)
        {
            var dataDirectory = context.Get<DirectoryInfo>("DataFileDirectory");

            // no directory specified
            if (dataDirectory == null)
                return null;

            // try to create the directory if it doesn't exist
            if (!dataDirectory.Exists) {
                try {
                    dataDirectory.Create();
                }
                catch (Exception ex) {
                    Console.WriteLine($"Tried to create directory at {dataDirectory.FullName} but got exception: {ex}");
                    return null;
                }
            }

            return Path.Combine(dataDirectory.FullName, name);
        }

        static StreamReader GetReader(this IBrightDataContext context, string fileName, string remoteUrl = null)
        {
            var filePath = GetDataFilePath(context, fileName);
            if (filePath == null || !File.Exists(filePath) && !String.IsNullOrWhiteSpace(remoteUrl)) {
                Console.Write($"Downloading data set from {remoteUrl}...");
                using var client = new HttpClient();
                var stream = client.GetStreamAsync(remoteUrl).Result;
                Console.WriteLine("done");
                if (String.IsNullOrWhiteSpace(filePath))
                    return new StreamReader(stream);

                try {
                    using var file = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                    stream.CopyTo(file);
                }
                catch {
                    Console.WriteLine($"Tried to write data to {filePath} but got exception");
                    throw;
                }
            }
            return new StreamReader(filePath);
        }


    }
}
