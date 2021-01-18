using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using BrightData;
using BrightTable;
using BrightTable.Transformations;
using BrightWire.TrainingData.Artificial;
using BrightWire.TrainingData.Helper;
using ExampleCode.DataTableTrainers;
using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using SharpCompress.Common;

namespace ExampleCode.Datasets
{
    internal static class SimpleDatasets
    {
        public static IrisTrainer Iris(this IBrightDataContext context)
        {
            var reader = GetStreamReader(context, "iris.csv", "https://archive.ics.uci.edu/ml/machine-learning-databases/iris/iris.data");
            try
            {
                using var table = context.ParseCsv(reader, false);
                table.SetTargetColumn(4);
                using var numericTable = table.Convert(
                    ColumnConversionType.ToNumeric,
                    ColumnConversionType.ToNumeric,
                    ColumnConversionType.ToNumeric,
                    ColumnConversionType.ToNumeric);
                using var normalized = numericTable.Normalize(NormalizationType.FeatureScale);
                return new IrisTrainer(normalized.AsRowOriented(), table.Column(4).ToArray<string>());
            }
            finally
            {
                reader?.Dispose();
            }
        }

        public static StockDataTrainer StockData(this IBrightDataContext context)
        {
            var reader = GetStreamReader(context, "stockdata.csv", "https://raw.githubusercontent.com/plotly/datasets/master/stockdata.csv");
            try
            {
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
            finally
            {
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
            using var reader = GetStreamReader(context, "beautiful_and_damned.txt", "http://www.gutenberg.org/cache/epub/9830/pg9830.txt");
            var data = reader.ReadToEnd();
            var pos = data.IndexOf("CHAPTER I");
            var mainText = data.Substring(pos + 9).Trim();

            return new SentenceTable(context, SimpleTokeniser.FindSentences(SimpleTokeniser.Tokenise(mainText)));
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

        public static SentimentDataTrainer SentimentData(this IBrightDataContext context)
        {
            var directory = ExtractToDirectory(context, "sentiment", "sentiment.zip", "https://archive.ics.uci.edu/ml/machine-learning-databases/00331/sentiment%20labelled%20sentences.zip");
            return new SentimentDataTrainer(context, directory);
        }

        public static TestClusteringTrainer TextClustering(this IBrightDataContext context)
        {
            using var reader = GetStreamReader(context, "aaai-accepted-papers.csv",
                "https://archive.ics.uci.edu/ml/machine-learning-databases/00307/%5bUCI%5d%20AAAI-14%20Accepted%20Papers%20-%20Papers.csv");
            using var table = context.ParseCsv(reader, true);

            var KEYWORD_SPLIT = " \n".ToCharArray();
            var TOPIC_SPLIT = "\n".ToCharArray();

            var docList = new List<TestClusteringTrainer.AAAIDocument>();
            table.ForEachRow(row => docList.Add(new TestClusteringTrainer.AAAIDocument
            {
                Abstract = (string)row[5],
                Keyword = ((string)row[3]).Split(KEYWORD_SPLIT, StringSplitOptions.RemoveEmptyEntries).Select(str => str.ToLower()).ToArray(),
                Topic = ((string)row[4]).Split(TOPIC_SPLIT, StringSplitOptions.RemoveEmptyEntries),
                Group = ((string)row[2]).Split(TOPIC_SPLIT, StringSplitOptions.RemoveEmptyEntries),
                Title = (string)row[0]
            }));

            return new TestClusteringTrainer(context, docList);
        }

        public static ReberSequenceTrainer ReberSequencePrediction(this IBrightDataContext context, bool extended = true, int? minLength = 10, int? maxLength = 10)
        {
            var grammar = new ReberGrammar(context.Random);
            var sequences = extended
                ? grammar.GetExtended(minLength, maxLength)
                : grammar.Get(minLength, maxLength);

            return new ReberSequenceTrainer(context, ReberGrammar.GetOneHot(context, sequences.Take(500)));
        }

        public static SequenceToSequenceTrainer OneToMany(this IBrightDataContext context)
        {
            var grammar = new SequenceGenerator(context, dictionarySize: 10, minSize: 5, maxSize: 5, noRepeat: true);
            var sequences = grammar.GenerateSequences().Take(1000).ToList();
            var builder = context.BuildTable();
            builder.AddColumn(ColumnType.Vector, "Summary");
            builder.AddColumn(ColumnType.Matrix, "Sequence").SetTargetColumn(true);

            foreach (var sequence in sequences)
            {
                var sequenceData = sequence
                        .GroupBy(ch => ch)
                        .Select(g => (g.Key, g.Count()))
                        .ToDictionary(d => d.Item1, d => (float)d.Item2)
                    ;
                var summary = grammar.Encode(sequenceData.Select(kv => (kv.Key, kv.Value)));
                var list = new List<Vector<float>>();
                foreach (var item in sequenceData.OrderBy(kv => kv.Key))
                {
                    var row = grammar.Encode(item.Key, item.Value);
                    list.Add(row);
                }
                builder.AddRow(summary, context.CreateMatrixFromRows(list.ToArray()));
            }

            return new SequenceToSequenceTrainer(context, grammar.DictionarySize, builder.BuildRowOriented());
        }

        public static SequenceToSequenceTrainer ManyToOne(this IBrightDataContext context)
        {
            var grammar = new SequenceGenerator(context, dictionarySize: 16, minSize: 3, maxSize: 6, noRepeat: true);
            var sequences = grammar.GenerateSequences().Take(1000).ToList();
            var builder = context.BuildTable();
            builder.AddColumn(ColumnType.Matrix, "Sequence");
            builder.AddColumn(ColumnType.Vector, "Summary").SetTargetColumn(true);

            foreach (var sequence in sequences)
            {
                var list = new List<Vector<float>>();
                var charSet = new HashSet<char>();
                foreach (var ch in sequence)
                {
                    charSet.Add(ch);
                    list.Add(grammar.Encode(ch));
                }

                var target = grammar.Encode(charSet.Select(ch2 => (ch2, 1f)));
                builder.AddRow(context.CreateMatrixFromRows(list.ToArray()), target);
            }
            return new SequenceToSequenceTrainer(context, grammar.DictionarySize, builder.BuildRowOriented());
        }

        public static SequenceToSequenceTrainer SequenceToSequence(this IBrightDataContext context)
        {
            const int SEQUENCE_LENGTH = 5;
            var grammar = new SequenceGenerator(context, 8, SEQUENCE_LENGTH, SEQUENCE_LENGTH, true);
            var sequences = grammar.GenerateSequences().Take(2000).ToList();
            var builder = context.BuildTable();
            builder.AddColumn(ColumnType.Matrix, "Input");
            builder.AddColumn(ColumnType.Matrix, "Output").SetTargetColumn(true);

            foreach (var sequence in sequences)
            {
                var encodedSequence = grammar.Encode(sequence);
                var reversedSequence = context.CreateMatrixFromRows(encodedSequence.Rows.Reverse().Take(SEQUENCE_LENGTH - 1).ToArray());
                builder.AddRow(encodedSequence, reversedSequence);
            }

            return new SequenceToSequenceTrainer(context, grammar.DictionarySize, builder.BuildRowOriented());
        }

        public static LinearTrainer SimpleLinear(this IBrightDataContext context)
        {
            var dataTableBuilder = context.BuildTable();
            dataTableBuilder.AddColumn(ColumnType.Float, "capital costs");
            dataTableBuilder.AddColumn(ColumnType.Float, "labour costs");
            dataTableBuilder.AddColumn(ColumnType.Float, "energy costs");
            dataTableBuilder.AddColumn(ColumnType.Float, "output").SetTargetColumn(true);

            dataTableBuilder.AddRow(98.288f, 0.386f, 13.219f, 1.270f);
            dataTableBuilder.AddRow(255.068f, 1.179f, 49.145f, 4.597f);
            dataTableBuilder.AddRow(208.904f, 0.532f, 18.005f, 1.985f);
            dataTableBuilder.AddRow(528.864f, 1.836f, 75.639f, 9.897f);
            dataTableBuilder.AddRow(307.419f, 1.136f, 52.234f, 5.907f);
            dataTableBuilder.AddRow(138.283f, 1.085f, 9.027f, 1.832f);
            dataTableBuilder.AddRow(418.883f, 2.390f, 1.676f, 4.865f);
            dataTableBuilder.AddRow(247.439f, 1.356f, 31.244f, 2.728f);
            dataTableBuilder.AddRow(19.478f, 0.115f, 1.739f, 0.125f);
            dataTableBuilder.AddRow(537.540f, 2.591f, 104.584f, 9.685f);
            dataTableBuilder.AddRow(605.507f, 2.789f, 82.296f, 8.727f);
            dataTableBuilder.AddRow(174.765f, 0.933f, 21.990f, 2.239f);
            dataTableBuilder.AddRow(946.766f, 4.004f, 125.351f, 10.077f);
            dataTableBuilder.AddRow(296.490f, 1.513f, 43.232f, 4.477f);
            dataTableBuilder.AddRow(645.690f, 2.540f, 75.581f, 7.037f);
            dataTableBuilder.AddRow(288.975f, 1.416f, 42.037f, 3.507f);

            var dataTable = dataTableBuilder.BuildRowOriented();
            var normalized = dataTable.AsColumnOriented().Normalize(NormalizationType.Standard).AsRowOriented();
            return new LinearTrainer(normalized);
        }

        public static BicyclesTrainer Bicycles(this IBrightDataContext context)
        {
            var directory = ExtractToDirectory(context, "bicycles", "bicycles.zip", "https://archive.ics.uci.edu/ml/machine-learning-databases/00275/Bike-Sharing-Dataset.zip");
            var hoursData = new StreamReader(Path.Combine(directory.FullName, "hour.csv"));
            using var completeTable = context.ParseCsv(hoursData, true);

            // drop the first six columns (index and date features)
            using var filteredTable = completeTable.CopyColumns(completeTable.ColumnCount.AsRange().Skip(5).ToArray());
            var dataColumns = (completeTable.ColumnCount - 3).AsRange().ToArray();
            using var converted = filteredTable.Convert(dataColumns.Select(i => new ColumnConversion(i, ColumnConversionType.ToNumeric)).ToArray());

            // normalise the data columns
            using var ret = converted.Normalize(dataColumns.Select(i => new ColumnNormalization(i, NormalizationType.Standard)).ToArray());

            ret.SetTargetColumn(ret.ColumnCount-1);
            return new BicyclesTrainer(context, ret.AsRowOriented());
        }

        public static EmotionsTrainer Emotions(this IBrightDataContext context)
        {
            var directory = ExtractToDirectory(context, "emotions", "emotions.rar", "https://downloads.sourceforge.net/project/mulan/datasets/emotions.rar");
            var table = EmotionsTrainer.Parse(context, Path.Combine(directory.FullName, "emotions.arff"));
            var test = EmotionsTrainer.Parse(context, Path.Combine(directory.FullName, "emotions-test.arff"));
            var training = EmotionsTrainer.Parse(context, Path.Combine(directory.FullName, "emotions-train.arff"));
            return new EmotionsTrainer(context, table, training, test);
        }

        static string GetDataFilePath(this IBrightDataContext context, string name)
        {
            var dataDirectory = context.Get<DirectoryInfo>("DataFileDirectory");

            // no directory specified
            if (dataDirectory == null)
                return null;

            // try to create the directory if it doesn't exist
            if (!dataDirectory.Exists)
            {
                try
                {
                    dataDirectory.Create();
                }
                catch (Exception ex)
                {
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

        static Stream GetStream(this IBrightDataContext context, string fileName, string remoteUrl = null, Action<string> downloadedToFile = null)
        {
            var wasDownloaded = false;
            var filePath = GetDataFilePath(context, fileName);
            if (filePath == null || !File.Exists(filePath) && !String.IsNullOrWhiteSpace(remoteUrl))
            {
                Console.Write($"Downloading data set from {remoteUrl}...");

                using var handler = new HttpClientHandler
                {
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

                try
                {
                    using var file = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                    responseStream.CopyTo(file);
                    wasDownloaded = true;
                }
                catch
                {
                    Console.WriteLine($"Tried to write data to {filePath} but got exception");
                    throw;
                }
            }
            if (wasDownloaded)
                downloadedToFile?.Invoke(filePath);
            return new FileStream(filePath, FileMode.Open, FileAccess.Read);
        }

        static DirectoryInfo ExtractToDirectory(IBrightDataContext context, string directoryName, string localName, string remoteUrl)
        {
            var directoryPath = GetDataFilePath(context, directoryName);
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            var extension = Path.GetExtension(localName).ToLower();
            var isZip = (extension == ".zip");
            var isRar = (extension == ".rar");
            if (!isZip && !isRar)
                throw new Exception($"Unknown file extension to extract: {extension}");
            Action<string> extractor = null;
            if (isZip)
                extractor = filePath => ZipFile.ExtractToDirectory(filePath, directoryPath);
            else if (isRar)
            {
                extractor = filePath =>
                {
                    using var archive = RarArchive.Open(filePath);
                    foreach (var entry in archive.Entries.Where(entry => !entry.IsDirectory)) {
                        entry.WriteToDirectory(directoryPath, new ExtractionOptions()
                        {
                            ExtractFullPath = true,
                            Overwrite = true
                        });
                    }
                };
            }

            using var output = GetStream(context, localName, remoteUrl, filePath => extractor(filePath));
            return new DirectoryInfo(directoryPath);
        }
    }
}
