using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BrightData;
using BrightData.Types;
using BrightWire;
using BrightWire.TrainingData.Helper;

namespace ExampleCode.DataTableTrainers
{
    internal class TestClusteringTrainer
    {
        readonly BrightDataContext _context;

        public class AaaiDocument(string title, string[] keyword, string[] topic, string @abstract, string[] group)
        {
            /// <summary>
            /// Free text description of the document
            /// </summary>
            public string Title { get; } = title;

            /// <summary>
            /// Free text; author-generated keywords
            /// </summary>
            public string[] Keyword { get; } = keyword;

            /// <summary>
            /// Free text; author-selected, low-level keywords
            /// </summary>
            public string[] Topic { get; } = topic;

            /// <summary>
            /// Free text; paper abstracts
            /// </summary>
            public string Abstract { get; } = @abstract;

            /// <summary>
            /// Categorical; author-selected, high-level keyword(s)
            /// </summary>
            public string[] Group { get; } = group;

            public WeightedIndexListWithLabel<string> AsClassification(StringTableBuilder stringTable)
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

                var weights = WeightedIndexList.Create(weightedIndex
                    .GroupBy(d => d.Index)
                    .Select(g => (g.Key, g.Sum(d => d.Weight)))
                );
                return new(Title, weights);
            }

            public override string ToString() => $"{Title} - {Abstract}";
        }
        readonly StringTableBuilder _stringTable = new();
        readonly IVector<float>[] _vectors;
        readonly AaaiDocument[] _documents;
        readonly uint _groupCount;

        public TestClusteringTrainer(BrightDataContext context, AaaiDocument[] documents)
        {
            _context = context;
            var lap = context.LinearAlgebraProvider;
            var data = documents.Select(d => d.AsClassification(_stringTable)).ToArray();
            var data2 = data.Bm25Plus();
            _vectors = data2.Select(d => d.Data.AsDense(_stringTable.Size + 1).Create(lap)).ToArray();
            _documents = documents;
            var allGroups = new HashSet<string>(documents.SelectMany(d => d.Group));
            _groupCount = (uint) allGroups.Count;
        }

        public void KMeans()
        {
            Console.Write("Kmeans clustering...");
            var outputPath = GetOutputPath("kmeans");
            WriteClusters(outputPath, _vectors.KMeansCluster(_context, _groupCount), _documents);
            Console.WriteLine($"written to {outputPath}");
        }

        public void Nnmf()
        {
            Console.Write("NNMF  clustering...");
            var outputPath = GetOutputPath("nnmf");
            WriteClusters(outputPath, _vectors.Nnmf(_context.LinearAlgebraProvider, _groupCount), _documents);
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
            var vectorList2 = projectedMatrix.RowCount.AsRange().Select(projectedMatrix.GetRowVector).ToArray();
            Console.Write("done...");

            Console.Write("Kmeans clustering of random projection...");
            WriteClusters(outputPath, vectorList2.KMeansCluster(_context, _groupCount), _documents);
            vectorList2.DisposeAll();
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
            var (_, floatVector, vt) = matrixT.Svd();
            matrixT.Dispose();

            var s = lap.CreateDiagonalMatrix(floatVector.Segment.Values.Take((int)k).ToArray());
            var v2 = vt.GetNewMatrixFromRows(kIndices);
            using (var sv2 = s.Multiply(v2))
            {
                v2.Dispose();
                s.Dispose();

                var vectorList3 = sv2.AllColumnsAsReadOnly(false).Select(c => c.Create(lap)).ToArray();

                Console.WriteLine("Kmeans clustering in latent document space...");
                WriteClusters(outputPath, vectorList3.KMeansCluster(_context, _groupCount), _documents);
                vectorList3.DisposeAll();
            }

            Console.WriteLine($"written to {outputPath}");
        }

        string GetOutputPath(string name) => Path.Combine(DataFileDirectory, "output", $"{name}.txt");

        string DataFileDirectory => _context.Get<DirectoryInfo>("DataFileDirectory")?.FullName ?? throw new Exception("Data File Directory not set");

        static void WriteClusters(string filePath, uint[][] clusters, AaaiDocument[] lookupTable)
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
