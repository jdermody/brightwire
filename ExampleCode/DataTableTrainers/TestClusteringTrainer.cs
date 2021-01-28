using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BrightData;
using BrightWire;
using BrightWire.TrainingData.Helper;

namespace ExampleCode.DataTableTrainers
{
    internal class TestClusteringTrainer
    {
        readonly IBrightDataContext _context;

        public class AAAIDocument
        {
            /// <summary>
            /// Free text description of the document
            /// </summary>
            public string Title { get; set; }

            /// <summary>
            /// Free text; author-generated keywords
            /// </summary>
            public string[] Keyword { get; set; }

            /// <summary>
            /// Free text; author-selected, low-level keywords
            /// </summary>
            public string[] Topic { get; set; }

            /// <summary>
            /// Free text; paper abstracts
            /// </summary>
            public string Abstract { get; set; }

            /// <summary>
            /// Categorical; author-selected, high-level keyword(s)
            /// </summary>
            public string[] Group { get; set; }

            public (string Classification, WeightedIndexList Data) AsClassification(IBrightDataContext context, StringTableBuilder stringTable)
            {
                var weightedIndex = new List<WeightedIndexList.Item>();
                foreach (var item in Keyword)
                {
                    weightedIndex.Add(new WeightedIndexList.Item(stringTable.GetIndex(item), 1f));
                }
                foreach (var item in Topic)
                {
                    weightedIndex.Add(new WeightedIndexList.Item(stringTable.GetIndex(item), 1f));
                }
                return (Title, context.CreateWeightedIndexList(weightedIndex
                    .GroupBy(d => d.Index)
                    .Select(g => (g.Key, g.Sum(d => d.Weight)))
                ));
            }
        }
        readonly StringTableBuilder _stringTable = new StringTableBuilder();
        readonly (string Classification, WeightedIndexList Data)[] _data;
        readonly IFloatVector[] _vectors;
        readonly Dictionary<IFloatVector, AAAIDocument> _documentTable = new Dictionary<IFloatVector, AAAIDocument>();
        readonly uint _groupCount;

        public TestClusteringTrainer(IBrightDataContext context, IReadOnlyCollection<AAAIDocument> documents)
        {
            _context = context;
            var lap = context.LinearAlgebraProvider;
            _data = documents.Select(d => d.AsClassification(context, _stringTable)).ToArray();
            _vectors = _data.Select(d => lap.CreateVector(d.Data.AsDense(_stringTable.Size + 1))).ToArray();
            foreach(var combo in _vectors.Zip(documents))
                _documentTable.Add(combo.First, combo.Second);
            var allGroups = new HashSet<string>(documents.SelectMany(d => d.Group));
            _groupCount = (uint) allGroups.Count;
        }

        public void KMeans()
        {
            Console.Write("Kmeans clustering...");
            var outputPath = GetOutputPath("kmeans");
            _WriteClusters(outputPath, _vectors.KMeans(_context, _groupCount), _documentTable);
            Console.WriteLine($"written to {outputPath}");
        }

        public void NNMF()
        {
            Console.Write("NNMF  clustering...");
            var outputPath = GetOutputPath("nnmf");
            _WriteClusters(outputPath, _vectors.NNMF(_context.LinearAlgebraProvider, _groupCount), _documentTable);
            Console.WriteLine($"written to {outputPath}");
        }

        public void RandomProjection()
        {
            var lap = _context.LinearAlgebraProvider;
            // create a term/document matrix with terms as columns and documents as rows
            var matrix = lap.CreateMatrixFromRows(_vectors);

            Console.Write("Creating random projection...");
            var outputPath = GetOutputPath("projected-kmeans");
            using var randomProjection = lap.CreateRandomProjection(_stringTable.Size + 1, 512);
            using var projectedMatrix = randomProjection.Compute(matrix);
            var vectorList2 = projectedMatrix.RowCount.AsRange().Select(i => projectedMatrix.Row(i)).ToList();
            var lookupTable2 = vectorList2.Select((v, i) => Tuple.Create(v, _vectors[i])).ToDictionary(d => d.Item1, d => _documentTable[d.Item2]);
            Console.Write("done...");

            Console.Write("Kmeans clustering of random projection...");
            _WriteClusters(outputPath, vectorList2.KMeans(_context, _groupCount), lookupTable2);
            vectorList2.ForEach(v => v.Dispose());
            Console.WriteLine($"written to {outputPath}");
        }

        public void LatentSemanticAnalysis(uint k = 256)
        {
            Console.Write("Building latent term/document space...");
            var outputPath = GetOutputPath("latent-kmeans");

            var lap = _context.LinearAlgebraProvider;
            // create a term/document matrix with terms as columns and documents as rows
            var matrix = lap.CreateMatrixFromRows(_vectors);
            var kIndices = k.AsRange().ToList();
            var matrixT = matrix.Transpose();
            matrix.Dispose();
            var svd = matrixT.Svd();
            matrixT.Dispose();

            var s = lap.CreateDiagonalMatrix(svd.S.AsIndexable().Values.Take((int)k).ToArray());
            var v2 = svd.VT.GetNewMatrixFromRows(kIndices);
            using (var sv2 = s.Multiply(v2))
            {
                v2.Dispose();
                s.Dispose();

                var vectorList3 = sv2.AsIndexable().Columns.ToList();
                var lookupTable3 = vectorList3.Select((v, i) => Tuple.Create(v, _vectors[i])).ToDictionary(d => (IFloatVector)d.Item1, d => _documentTable[d.Item2]);

                Console.WriteLine("Kmeans clustering in latent document space...");
                _WriteClusters(outputPath, vectorList3.KMeans(_context, _groupCount), lookupTable3);
            }

            Console.WriteLine($"written to {outputPath}");
        }

        string GetOutputPath(string name) => Path.Combine(DataFileDirectory, "output", $"{name}.txt");

        string DataFileDirectory => _context.Get<DirectoryInfo>("DataFileDirectory", null)?.FullName ?? throw new Exception("Data File Directory not set");

        void _WriteClusters(string filePath, IFloatVector[][] clusters, Dictionary<IFloatVector, AAAIDocument> lookupTable)
        {
            new FileInfo(filePath).Directory?.Create();
            using var writer = new StreamWriter(filePath);
            foreach (var cluster in clusters)
            {
                foreach (var item in cluster)
                {
                    var document = lookupTable[item];
                    writer.WriteLine(document.Title);
                    writer.WriteLine(String.Join(", ", document.Keyword));
                    writer.WriteLine(String.Join(", ", document.Topic));
                    writer.WriteLine(String.Join(", ", document.Group));
                    writer.WriteLine();
                }
                writer.WriteLine("------------------------------------------------------------------");
            }
        }
    }
}
