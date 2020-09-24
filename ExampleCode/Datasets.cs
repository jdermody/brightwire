using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using BrightData;
using BrightData.Distributions;
using BrightTable;
using BrightWire;
using BrightWire.TrainingData.Artificial;
using BrightWire.TrainingData.Helper;

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
                using var normalized = numericTable.Normalize(NormalizationType.FeatureScale);
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
            var (training, test) = data.Split();
            return new DataTableTrainer(data, training, test);
        }

        public static void BeautifulandDamned(this IBrightDataContext context)
        {
            var reader = GetReader(context, "beautiful_and_damned.txt", "http://www.gutenberg.org/cache/epub/9830/pg9830.txt");
            var data = reader.ReadToEnd();
            var pos = data.IndexOf("CHAPTER I");

            // create a markov trainer that uses a window of size 3
            var trainer = context.CreateMarkovTrainer3<string>();
            foreach (var sentence in SimpleTokeniser.FindSentences(SimpleTokeniser.Tokenise(data.Substring(pos))))
                trainer.Add(sentence);

            // generate some text
            var model = trainer.Build().AsDictionary;
            for (var i = 0; i < 50; i++) {
                var sb = new StringBuilder();
                string prevPrev = default, prev = default, curr = default;
                for (var j = 0; j < 1024; j++) {
                    var transitions = model.GetTransitions(prevPrev, prev, curr);
                    var distribution = new CategoricalDistribution(context, transitions.Select(d => d.Probability));
                    var next = transitions[distribution.Sample()].NextState;
                    if (Char.IsLetterOrDigit(next[0]) && sb.Length > 0) {
                        var lastChar = sb[^1];
                        if (lastChar != '\'' && lastChar != '-')
                            sb.Append(' ');
                    }
                    sb.Append(next);

                    if (SimpleTokeniser.IsEndOfSentence(next))
                        break;
                    prevPrev = prev;
                    prev = curr;
                    curr = next;
                }
                Console.WriteLine(sb.ToString());
            }
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

                using var handler = new HttpClientHandler {
                    AutomaticDecompression = DecompressionMethods.All, 
                    AllowAutoRedirect = true,
                };
                using var client = new HttpClient(handler);
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
            return new StreamReader(filePath, Encoding.UTF8);
        }


    }
}
