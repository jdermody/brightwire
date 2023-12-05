using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using BrightData;
using BrightWire.TrainingData.Artificial;
using BrightWire.TrainingData.Helper;
using ExampleCode.DataTableTrainers;
using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using SharpCompress.Common;

namespace ExampleCode.DataSet
{
    internal static class SimpleDataSets
    {
        public static async Task<DataTableTrainer> Iris(this BrightDataContext context)
        {
            var reader = GetStreamReader(context, "iris.csv", "https://archive.ics.uci.edu/ml/machine-learning-databases/iris/iris.data");
            try
            {
                using var table = await context.ParseCsv(reader, false);
                table.SetTargetColumn(4);
                using var numericTable = await table.Convert(null, 
                    ColumnConversion.ToNumeric,
                    ColumnConversion.ToNumeric, 
                    ColumnConversion.ToNumeric,
                    ColumnConversion.ToNumeric
                );
                using var normalized = await numericTable.Normalize(NormalizationType.FeatureScale);
                return new DataTableTrainer(normalized);
            }
            finally
            {
                reader.Dispose();
            }
        }

        public static async Task<StockDataTrainer> StockData(this BrightDataContext context)
        {
            var reader = GetStreamReader(context, "stockdata.csv", "https://raw.githubusercontent.com/plotly/datasets/master/stockdata.csv");
            try {
                // load and normalise the data
                using var table = await context.ParseCsv(reader, true);
                table.SetTargetColumn(5);
                using var numericTable = await table.Convert(null, 
                    ColumnConversion.ToNumeric,
                    ColumnConversion.ToNumeric,
                    ColumnConversion.ToNumeric,
                    ColumnConversion.ToNumeric,
                    ColumnConversion.ToNumeric,
                    ColumnConversion.ToDate);
                using var normalised = await numericTable.Normalize(NormalizationType.FeatureScale);
                return new StockDataTrainer(normalised);
            }
            finally {
                reader.Dispose();
            }
        }

        public static async Task<XorTrainer> Xor(this BrightDataContext context)
        {
            // Some training data that the network will learn.  The XOR pattern looks like:
            // 0 0 => 0
            // 1 0 => 1
            // 0 1 => 1
            // 1 1 => 0
            var table = await BrightWire.TrainingData.Artificial.Xor.Get(context);
            return new XorTrainer(table);
        }

        public static async Task<IntegerAdditionTrainer> IntegerAddition(this BrightDataContext context)
        {
            var data = await BinaryIntegers.Addition(context, 1000);
            var (training, test) = await data.Split();
            return new IntegerAdditionTrainer(data, training, test);
        }

        public static async Task<SentenceTable> BeautifulandDamned(this BrightDataContext context)
        {
            using var reader = GetStreamReader(context, "beautiful_and_damned.txt", "http://www.gutenberg.org/cache/epub/9830/pg9830.txt");
            var data = await reader.ReadToEndAsync();
            var pos = data.IndexOf("CHAPTER I", StringComparison.Ordinal);
            var mainText = data[(pos + 9)..].Trim();

            return new SentenceTable(context, SimpleTokeniser.FindSentences(SimpleTokeniser.Tokenise(mainText)));
        }

        public static Mnist Mnist(this BrightDataContext context, uint numToLoad = uint.MaxValue)
        {
            var testImages = DataSet.Mnist.Load(
                GetStream(context, "t10k-labels.idx1-ubyte", "http://yann.lecun.com/exdb/mnist/t10k-labels-idx1-ubyte.gz"),
                GetStream(context, "t10k-images.idx3-ubyte", "http://yann.lecun.com/exdb/mnist/t10k-images-idx3-ubyte.gz"),
                numToLoad
            );
            var trainingImages = DataSet.Mnist.Load(
                GetStream(context, "train-labels.idx1-ubyte", "http://yann.lecun.com/exdb/mnist/train-labels-idx1-ubyte.gz"),
                GetStream(context, "train-images.idx3-ubyte", "http://yann.lecun.com/exdb/mnist/train-images-idx3-ubyte.gz"),
                numToLoad
            );

            return new Mnist(context, trainingImages, testImages);
        }

        public static SentimentDataTrainer SentimentData(this BrightDataContext context)
        {
            // http://dkotzias.com/papers/GICF.pdf
            var directory = ExtractToDirectory(context, "sentiment", "sentiment.zip", "https://archive.ics.uci.edu/ml/machine-learning-databases/00331/sentiment%20labelled%20sentences.zip");
            return new SentimentDataTrainer(context, directory);
        }

        public static async Task<TestClusteringTrainer> TextClustering(this BrightDataContext context)
        {
            using var reader = GetStreamReader(context, "aaai-accepted-papers.csv",
                "https://archive.ics.uci.edu/ml/machine-learning-databases/00307/%5bUCI%5d%20AAAI-14%20Accepted%20Papers%20-%20Papers.csv");
            using var table = await context.ParseCsv(reader, true);

            var keywordSplit = " \n".ToCharArray();
            var topicSplit = "\n".ToCharArray();

            var docList = new TestClusteringTrainer.AaaiDocument[table.RowCount];
            var index = 0;
            await foreach (var row in table.EnumerateRows()) {
                docList[index++] = new TestClusteringTrainer.AaaiDocument(
                    (string)row[0],
                    ((string)row[3]).Split(keywordSplit, StringSplitOptions.RemoveEmptyEntries).Select(str => str.ToLower()).ToArray(),
                    ((string)row[4]).Split(topicSplit, StringSplitOptions.RemoveEmptyEntries),
                    (string)row[5],
                    ((string)row[2]).Split(topicSplit, StringSplitOptions.RemoveEmptyEntries)
                );
            }

            return new TestClusteringTrainer(context, docList);
        }

        public static async Task<ReberSequenceTrainer> ReberSequencePrediction(this BrightDataContext context, bool extended = true, int? minLength = 8, int? maxLength = 12)
        {
            var grammar = new ReberGrammar(context.Random);
            var sequences = extended
                ? grammar.GetExtended(minLength, maxLength)
                : grammar.Get(minLength, maxLength);
            var trainingData = sequences.Take(2000).ToArray();
            return new ReberSequenceTrainer(await ReberGrammar.GetOneHot(context, trainingData));
        }

        public static async Task<SequenceToSequenceTrainer> OneToMany(this BrightDataContext context)
        {
            var grammar = new SequenceGenerator(context, dictionarySize: 10, minSize: 5, maxSize: 5, noRepeat: true);
            var sequences = grammar.GenerateSequences().Take(1000).ToList();
            var builder = context.CreateTableBuilder();
            var addTableColumns = true;

            foreach (var sequence in sequences)
            {
                var sequenceData = sequence
                    .GroupBy(ch => ch)
                    .Select(g => (g.Key, g.Count()))
                    .ToDictionary(d => d.Key, d => (float)d.Item2)
                ;
                var summary = grammar.Encode(sequenceData.Select(kv => (kv.Key, kv.Value)));
                var rows = new IReadOnlyVector[sequenceData.Count];
                var index = 0;
                foreach (var item in sequenceData.OrderBy(kv => kv.Key))
                {
                    var row = grammar.Encode(item.Key, item.Value);
                    rows[index++] = row;
                }

                var output = context.CreateReadOnlyMatrixFromRows(rows);
                if (addTableColumns) {
                    addTableColumns = false;
                    builder.CreateFixedSizeVectorColumn(summary.Size, "Summary");
                    builder.CreateFixedSizeMatrixColumn(output.RowCount, output.ColumnCount, "Sequence").MetaData.SetTarget(true);
                }
                builder.AddRow(summary, output);
            }

            return new SequenceToSequenceTrainer(grammar, await builder.BuildInMemory());
        }

        public static async Task<SequenceToSequenceTrainer> ManyToOne(this BrightDataContext context)
        {
            const int size = 5, dictionarySize = 16;
            var grammar = new SequenceGenerator(context, dictionarySize: dictionarySize, minSize: size-1, maxSize: size+1);
            var sequences = grammar.GenerateSequences().Take(1000).ToList();
            var builder = context.CreateTableBuilder();
            builder.CreateColumn(BrightDataType.Matrix, "Sequence");
            builder.CreateColumn(BrightDataType.Vector, "Summary").MetaData.SetTarget(true);

            foreach (var sequence in sequences) {
                var index = 0;
                var rows = new IReadOnlyVector[sequence.Length];
                var charSet = new HashSet<char>();
                foreach (var ch in sequence)
                {
                    charSet.Add(ch);
                    rows[index++] = grammar.Encode(ch);
                }

                var target = grammar.Encode(charSet.Select(ch2 => (ch2, 1f)));
                builder.AddRow(context.CreateReadOnlyMatrixFromRows(rows), target);
            }
            return new SequenceToSequenceTrainer(grammar, await builder.BuildInMemory());
        }

        static string Reverse(string str) => new(str.Reverse().ToArray());

        public static async Task<SequenceToSequenceTrainer> SequenceToSequence(this BrightDataContext context)
        {
            const int sequenceLength = 5;
            var grammar = new SequenceGenerator(context, 3, sequenceLength-1, sequenceLength+1, false);
            var sequences = grammar.GenerateSequences().Take(1000).ToList();
            var builder = context.CreateTableBuilder();
            builder.CreateColumn(BrightDataType.Matrix, "Input");
            builder.CreateColumn(BrightDataType.Matrix, "Output").MetaData.SetTarget(true);

            foreach (var sequence in sequences)
            {
                var encodedSequence = grammar.Encode(sequence);
                var encodedSequence2 = grammar.Encode(Reverse(sequence));
                builder.AddRow(encodedSequence, encodedSequence2);
            }

            return new SequenceToSequenceTrainer(grammar, await builder.BuildInMemory());
        }

        //public static LinearTrainer SimpleLinear(this BrightDataContext context)
        //{
        //    var dataTableBuilder = context.BuildTable();
        //    dataTableBuilder.AddColumn(BrightDataType.Float, "capital costs");
        //    dataTableBuilder.AddColumn(BrightDataType.Float, "labour costs");
        //    dataTableBuilder.AddColumn(BrightDataType.Float, "energy costs");
        //    dataTableBuilder.AddColumn(BrightDataType.Float, "output").SetTarget(true);

        //    dataTableBuilder.AddRow(98.288f, 0.386f, 13.219f, 1.270f);
        //    dataTableBuilder.AddRow(255.068f, 1.179f, 49.145f, 4.597f);
        //    dataTableBuilder.AddRow(208.904f, 0.532f, 18.005f, 1.985f);
        //    dataTableBuilder.AddRow(528.864f, 1.836f, 75.639f, 9.897f);
        //    dataTableBuilder.AddRow(307.419f, 1.136f, 52.234f, 5.907f);
        //    dataTableBuilder.AddRow(138.283f, 1.085f, 9.027f, 1.832f);
        //    dataTableBuilder.AddRow(418.883f, 2.390f, 1.676f, 4.865f);
        //    dataTableBuilder.AddRow(247.439f, 1.356f, 31.244f, 2.728f);
        //    dataTableBuilder.AddRow(19.478f, 0.115f, 1.739f, 0.125f);
        //    dataTableBuilder.AddRow(537.540f, 2.591f, 104.584f, 9.685f);
        //    dataTableBuilder.AddRow(605.507f, 2.789f, 82.296f, 8.727f);
        //    dataTableBuilder.AddRow(174.765f, 0.933f, 21.990f, 2.239f);
        //    dataTableBuilder.AddRow(946.766f, 4.004f, 125.351f, 10.077f);
        //    dataTableBuilder.AddRow(296.490f, 1.513f, 43.232f, 4.477f);
        //    dataTableBuilder.AddRow(645.690f, 2.540f, 75.581f, 7.037f);
        //    dataTableBuilder.AddRow(288.975f, 1.416f, 42.037f, 3.507f);

        //    var dataTable = dataTableBuilder.BuildRowOriented();
        //    var normalized = dataTable.AsColumnOriented().Normalize(NormalizationType.Standard).AsRowOriented();
        //    return new LinearTrainer(normalized);
        //}

        public static async Task<BicyclesTrainer> Bicycles(this BrightDataContext context)
        {
            var directory = ExtractToDirectory(context, "bicycles", "bicycles.zip", "https://archive.ics.uci.edu/ml/machine-learning-databases/00275/Bike-Sharing-Dataset.zip");
            var hoursData = new StreamReader(Path.Combine(directory.FullName, "hour.csv"));
            using var completeTable = await context.ParseCsv(hoursData, true);

            // drop the first six columns (index and date features)
            using var filteredTable = await completeTable.CopyColumnsToNewTable(null, completeTable.ColumnCount.AsRange().Skip(5).ToArray());
            var dataColumns = (completeTable.ColumnCount - 3).AsRange().ToArray();
            using var converted = await filteredTable.Convert(null, dataColumns.Select(i => ColumnConversion.ToNumeric.ConvertColumn(i)).ToArray());

            // normalise the data columns
            using var ret = await converted.Normalize(NormalizationType.Standard);

            ret.SetTargetColumn(ret.ColumnCount-1);
            return new BicyclesTrainer(ret);
        }

        public static async Task<EmotionsTrainer> Emotions(this BrightDataContext context)
        {
            var directory = ExtractToDirectory(context, "emotions", "emotions.rar", "https://downloads.sourceforge.net/project/mulan/datasets/emotions.rar");
            var table = await EmotionsTrainer.Parse(context, Path.Combine(directory.FullName, "emotions.arff"));
            var test = await EmotionsTrainer.Parse(context, Path.Combine(directory.FullName, "emotions-test.arff"));
            var training = await EmotionsTrainer.Parse(context, Path.Combine(directory.FullName, "emotions-train.arff"));
            return new EmotionsTrainer(table, training, test);
        }

        public static async Task<AdultTrainer> Adult(this BrightDataContext context)
        {
            static string AdjustString(string str)
            {
                str = str.Trim();
                if (str.EndsWith('.'))
                    return str[..^1];
                return str;
            }

            static async Task<IDataTable> ConvertTable(IDataTable table, string path)
            {
                using var converted = await table.Convert(null,
                    // convert numeric columns
                    ColumnConversion.ToNumeric.ConvertColumn(0), 
                    ColumnConversion.ToNumeric.ConvertColumn(2), 
                    ColumnConversion.ToNumeric.ConvertColumn(4), 
                    ColumnConversion.ToNumeric.ConvertColumn(10),
                    ColumnConversion.ToNumeric.ConvertColumn(11),
                    ColumnConversion.ToNumeric.ConvertColumn(12),

                    // convert to categorical index
                    ColumnConversion.ToCategoricalIndex.ConvertColumn(1),
                    ColumnConversion.ToCategoricalIndex.ConvertColumn(3),
                    ColumnConversion.ToCategoricalIndex.ConvertColumn(5),
                    ColumnConversion.ToCategoricalIndex.ConvertColumn(6),
                    ColumnConversion.ToCategoricalIndex.ConvertColumn(7),
                    ColumnConversion.ToCategoricalIndex.ConvertColumn(8),
                    ColumnConversion.ToCategoricalIndex.ConvertColumn(9),
                    ColumnConversion.ToCategoricalIndex.ConvertColumn(13),

                    ColumnConversion.Custom.ConvertColumn<string, string>(14, AdjustString)
                );
                converted.SetTargetColumn(14);
                var normalized = await converted.Normalize(NormalizationType.Standard);
                await normalized.WriteTo(path);
                return normalized;
            }

            var adultTraining = await GetDataTable(context, "adult_data.table", async path => {
                using var reader = GetStreamReader(context, "adult.data.csv",
                    "https://archive.ics.uci.edu/ml/machine-learning-databases/adult/adult.data");
                using var data = await context.ParseCsv(reader, false);
                return await ConvertTable(data, path);
            });

            var adultTest = await GetDataTable(context, "adult_test.table", async path => {
                using var reader2 = GetStreamReader(context, "adult.test.csv",
                    "https://archive.ics.uci.edu/ml/machine-learning-databases/adult/adult.test");
                await reader2.ReadLineAsync();
                using var test = await context.ParseCsv(reader2, false);
                return await ConvertTable(test, path);
            });

            return new AdultTrainer(null, adultTraining, adultTest);
        }

        static string GetDataFilePath(this BrightDataContext context, string name)
        {
            var dataDirectory = context.Get<DirectoryInfo>("DataFileDirectory") ?? throw new Exception("DataFileDirectory not set");

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
                    throw;
                }
            }

            return Path.Combine(dataDirectory.FullName, name);
        }

        static async Task<IDataTable> GetDataTable(this BrightDataContext context, string fileName, Func<string, Task<IDataTable>> createTable)
        {
            var path = GetDataFilePath(context, fileName);
            if (!File.Exists(path))
                return await createTable(path);

            return context.LoadTable(path);
        }

        static StreamReader GetStreamReader(this BrightDataContext context, string fileName, string? remoteUrl = null)
        {
            return new(GetStream(context, fileName, remoteUrl));
        }

        static Stream GetStream(this BrightDataContext context, string fileName, string? remoteUrl = null, Action<string>? downloadedToFile = null)
        {
            var wasDownloaded = false;
            var filePath = GetDataFilePath(context, fileName);
            if (!File.Exists(filePath) && !String.IsNullOrWhiteSpace(remoteUrl))
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
                var mediaType = response.Content.Headers.ContentType?.MediaType;
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

        static DirectoryInfo ExtractToDirectory(BrightDataContext context, string directoryName, string localName, string remoteUrl)
        {
            var directoryPath = GetDataFilePath(context, directoryName);
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            var extension = Path.GetExtension(localName).ToLower();
            var isZip = (extension == ".zip");
            var isRar = (extension == ".rar");
            if (!isZip && !isRar)
                throw new Exception($"Unknown file extension to extract: {extension}");
            Action<string>? extractor = null;
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

            if (extractor == null)
                throw new Exception("Could not create an extractor");

            using var output = GetStream(context, localName, remoteUrl, filePath => extractor(filePath));
            return new DirectoryInfo(directoryPath);
        }
    }
}
