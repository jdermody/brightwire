using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using BrightData;
using BrightData.Distributions;
using BrightTable;
using BrightWire;
using BrightWire.TrainingData.Artificial;
using BrightWire.TrainingData.Helper;
using ExampleCode.DataTableTrainers;

namespace ExampleCode.Datasets
{
    static class Dataset
    {
        public static IrisTrainer Iris(this IBrightDataContext context)
        {
            var reader = GetStreamReader(context, "iris.csv", "https://archive.ics.uci.edu/ml/machine-learning-databases/iris/iris.data");
            try {
                using var table = context.ParseCsv(reader, false);
                table.SetTargetColumn(4);
                using var numericTable = table.Convert(
                    ColumnConversionType.ToNumeric,
                    ColumnConversionType.ToNumeric,
                    ColumnConversionType.ToNumeric,
                    ColumnConversionType.ToNumeric);
                using var normalized = numericTable.Normalize(NormalizationType.FeatureScale);
                return new IrisTrainer(normalized.AsRowOriented());
            }
            finally {
                reader?.Dispose();
            }
        }

        public static StockDataTrainer StockData(this IBrightDataContext context)
        {
            var reader = GetStreamReader(context, "stockdata.csv", "https://raw.githubusercontent.com/plotly/datasets/master/stockdata.csv");
            try {
                // load and normalise the data
                using var table = context.ParseCsv(reader, true);
                table.SetTargetColumn(5);
                using var numericTable = table.Convert(
                    ColumnConversionType.ToNumeric,
                    ColumnConversionType.ToNumeric,
                    ColumnConversionType.ToNumeric,
                    ColumnConversionType.ToNumeric,
                    ColumnConversionType.ToNumeric,
                    ColumnConversionType.ToDate);
                using var normalised = numericTable.Normalize(NormalizationType.FeatureScale);
                return new StockDataTrainer(normalised.AsRowOriented());
            }
            finally {
                reader.Dispose();
            }
        }

        public static XorTrainer Xor(this IBrightDataContext context)
        {
            // Some training data that the network will learn.  The XOR pattern looks like:
            // 0 0 => 0
            // 1 0 => 1
            // 0 1 => 1
            // 1 1 => 0
            var table = BrightWire.TrainingData.Artificial.Xor.Get(context);
            return new XorTrainer(table);
        }

        public static IntegerAdditionTrainer IntegerAddition(this IBrightDataContext context)
        {
            var data = BinaryIntegers.Addition(context, 1000);
            var (training, test) = data.Split();
            return new IntegerAdditionTrainer(data, training, test);
        }

        public static SentenceTable BeautifulandDamned(this IBrightDataContext context)
        {
            var reader = GetStreamReader(context, "beautiful_and_damned.txt", "http://www.gutenberg.org/cache/epub/9830/pg9830.txt");
            var data = reader.ReadToEnd();
            var pos = data.IndexOf("CHAPTER I");

            return new SentenceTable(context, SimpleTokeniser.FindSentences(SimpleTokeniser.Tokenise(data.Substring(pos))));
        }

        public static MNIST MNIST(this IBrightDataContext context, uint numToLoad = uint.MaxValue)
        {
            var testImages = Datasets.MNIST.Load(
                GetStream(context, "t10k-labels.idx1-ubyte", "http://yann.lecun.com/exdb/mnist/t10k-labels-idx1-ubyte.gz"), 
                GetStream(context, "t10k-images.idx3-ubyte", "http://yann.lecun.com/exdb/mnist/t10k-images-idx3-ubyte.gz")
            );
            var trainingImages = Datasets.MNIST.Load(
                GetStream(context, "train-labels.idx1-ubyte", "http://yann.lecun.com/exdb/mnist/train-labels-idx1-ubyte.gz"),
                GetStream(context, "train-images.idx3-ubyte", "http://yann.lecun.com/exdb/mnist/train-images-idx3-ubyte.gz")
            );

            return new MNIST(context, trainingImages, testImages);
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

        static StreamReader GetStreamReader(this IBrightDataContext context, string fileName, string remoteUrl = null)
        {
            return new StreamReader(GetStream(context, fileName, remoteUrl));
        }

        static Stream GetStream(this IBrightDataContext context, string fileName, string remoteUrl = null)
        {
            var filePath = GetDataFilePath(context, fileName);
            if (filePath == null || !File.Exists(filePath) && !String.IsNullOrWhiteSpace(remoteUrl)) {
                Console.Write($"Downloading data set from {remoteUrl}...");

                using var handler = new HttpClientHandler {
                    AutomaticDecompression = DecompressionMethods.All, 
                    AllowAutoRedirect = true,
                };
                using var client = new HttpClient(handler);
                var response = client.GetAsync(remoteUrl).Result;
                response.EnsureSuccessStatusCode();

                // get the stream
                var responseStream = response.Content.ReadAsStreamAsync().Result;
                var mediaType = response.Content.Headers.ContentType.MediaType;
                if (mediaType == "application/gzip" || mediaType == "application/x-gzip")
                    responseStream = new GZipStream(responseStream, CompressionMode.Decompress);

                Console.WriteLine("done");

                if (String.IsNullOrWhiteSpace(filePath))
                    return responseStream;

                try {
                    using var file = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                    responseStream.CopyTo(file);
                }
                catch {
                    Console.WriteLine($"Tried to write data to {filePath} but got exception");
                    throw;
                }
            }
            return new FileStream(filePath, FileMode.Open, FileAccess.Read);
        }


    }
}
