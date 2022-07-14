using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using BrightData.Helper;
using BrightData.LinearAlgebra;

namespace BrightData
{
    /// <summary>
    /// A list of weighted indices is a sparse vector
    /// </summary>
    public struct WeightedIndexList : IHaveIndices, IAmSerializable
    {
        /// <summary>
        /// An item within a weighted index list
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack=0)]
        public readonly struct Item : IEquatable<Item>
        {
            /// <summary>
            /// Index of item
            /// </summary>
            public uint Index { get; }

            /// <summary>
            /// Weight of item
            /// </summary>
            public float Weight { get; }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="index">Index of item</param>
            /// <param name="weight">Weight of item</param>
            public Item(uint index, float weight)
            {
                Index = index;
                Weight = weight;
            }

            /// <inheritdoc />
            public override string ToString() => $"{Index}:{Weight}";

            /// <inheritdoc />
            public bool Equals(Item other) => other.Index == Index && FloatMath.Equals(other.Weight, Weight);
            /// <inheritdoc />
            public override bool Equals(object? obj) => obj is Item item && Equals(item);
            /// <inheritdoc />
            public override int GetHashCode() => HashCode.Combine(Index, Weight);

            /// <summary>
            /// 
            /// </summary>
            /// <param name="left"></param>
            /// <param name="right"></param>
            /// <returns></returns>
            public static bool operator ==(Item left, Item right)
            {
                return left.Equals(right);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="left"></param>
            /// <param name="right"></param>
            /// <returns></returns>
            public static bool operator !=(Item left, Item right)
            {
                return !(left == right);
            }
        }

        public WeightedIndexList(Item[] indices)
        {
            Indices = indices;
        }

        /// <summary>
        /// The list of indices
        /// </summary>
        public Item[] Indices { get; private set; }

        public static WeightedIndexList Create(params Item[] indexList) => new(indexList);
        public static WeightedIndexList Create(ReadOnlySpan<Item> indexList) => new(indexList.ToArray());
        public static WeightedIndexList Create(IEnumerable<Item> indexList) => new(indexList.ToArray());

        public static WeightedIndexList Create(params (uint Index, float Weight)[] indexList) =>
            new(indexList.Select(d => new Item(d.Index, d.Weight)).ToArray());

        public static WeightedIndexList Create(IEnumerable<(uint Index, float Weight)> indexList) =>
            new(indexList.Select(d => new Item(d.Index, d.Weight)).ToArray());

        /// <summary>
        /// The number of items in the list
        /// </summary>
        public int Count => Indices.Length;

        /// <summary>
        /// ToString override
        /// </summary>
        public override string ToString()
        {
            if (Count < 32)
            {
                var indices = String.Join('|', Indices);
                return $"Weighted Index List - {indices}";
            }
            return $"Weighted Index List ({Count} indices)";
        }

        /// <inheritdoc />
        public void Initialize(BrightDataContext context, BinaryReader reader)
        {
            var len = reader.ReadInt32();
            Indices = reader.BaseStream.ReadArray<Item>(len);
        }

        /// <summary>
        /// Merges a sequence of weighted index lists into one list
        /// </summary>
        /// <param name="lists">Lists to merge</param>
        /// <param name="mergeOperation">How to merge item weights</param>
        /// <returns></returns>
        public static WeightedIndexList Merge(IEnumerable<WeightedIndexList> lists, AggregationType mergeOperation = AggregationType.Average)
        {
            var items = new Dictionary<uint, List<float>>();
            foreach (var list in lists) {
                foreach (var index in list.Indices) {
                    if (!items.TryGetValue(index.Index, out var weights))
                        items.Add(index.Index, weights = new List<float>());
                    weights.Add(index.Weight);
                }
            }

            return new WeightedIndexList(
                items.Select(d => new Item(d.Key, mergeOperation.Aggregate(d.Value))).ToArray()
            );
        }

        /// <summary>
        /// Writes the data to an XML writer
        /// </summary>
        /// <param name="name">The name to give the data</param>
        /// <param name="writer">The writer to write to</param>
        public void WriteTo(string? name, XmlWriter writer)
        {
            writer.WriteStartElement(name ?? "weighted-index-list");

            writer.WriteValue(String.Join("|", Indices
                .OrderBy(d => d.Index)
                .Select(c => $"{c.Index}:{c.Weight}")
            ));
            writer.WriteEndElement();
        }

        /// <summary>
        /// Writes the data to a binary writer
        /// </summary>
        /// <param name="writer"></param>
        public unsafe void WriteTo(BinaryWriter writer)
        {
            writer.Write(Count);
            fixed (Item* ptr = Indices) {
                writer.Write(new ReadOnlySpan<byte>(ptr, Indices.Length * sizeof(Item)));
            }
        }

        /// <summary>
        /// Converts the weighted index list to XML
        /// </summary>
        public string ToXml()
        {
            var sb = new StringBuilder();
            var settings = new XmlWriterSettings {
                OmitXmlDeclaration = true
            };
            using var writer = XmlWriter.Create(sb, settings);
            WriteTo(null, writer);
            return sb.ToString();
        }

        /// <summary>
        /// Converts the weighted index-list to an unweighted index-list (only those indices whose weight is not zero)
        /// </summary>
        /// <returns></returns>
        public IndexList AsIndexList() => IndexList.Create(Indices.Where(ind => FloatMath.IsNotZero(ind.Weight)).Select(ind => ind.Index).ToArray());

        IEnumerable<uint> IHaveIndices.Indices => Indices.Select(ind => ind.Index);

        /// <summary>
        /// Dot product of this combined with the other weighted index list
        /// </summary>
        /// <param name="other">Other weighted index list</param>
        public float Dot(WeightedIndexList other)
        {
            var ret = 0f;
            var otherTable = other.Indices.ToDictionary(d => d.Index, d => d.Weight);
            foreach (var item in Indices)
            {
                if (otherTable.TryGetValue(item.Index, out var otherWeight))
                    ret += otherWeight * item.Weight;
            }
            return ret;
        }

        /// <summary>
        /// Magnitude of weights
        /// </summary>
        public float Magnitude => Indices.Any()
            ? FloatMath.Sqrt(Indices.Sum(d => d.Weight * d.Weight))
            : 0f
        ;

        /// <summary>
        /// Cosine similarity between this and another weighted index list
        /// </summary>
        /// <param name="other">Other list to compare</param>
        /// <returns></returns>
        public float CosineSimilarity(WeightedIndexList other) => Dot(other) / (Magnitude * other.Magnitude);

        /// <summary>
        /// Returns the index with the highest weight
        /// </summary>
        public float GetMaxWeight() => Indices.Any()
            ? Indices.Max(item => item.Weight)
            : float.NaN
        ;

        /// <summary>
        /// Computes the jaccard similarity between this and another weighted index list
        /// </summary>
        /// <param name="other">Other list to compare</param>
        public float JaccardSimilarity(WeightedIndexList other)
        {
            var set1 = Indices.GroupBy(d => d.Index).ToDictionary(g => g.Key, g => g.Sum(d => d.Weight));
            var set2 = other.Indices.GroupBy(d => d.Index).ToDictionary(g => g.Key, g => g.Sum(d => d.Weight));
            float intersection = 0f, union = set1.Sum(kv => kv.Value);

            foreach (var (key, value) in set2)
            {
                if (set1.TryGetValue(key, out var weight))
                    intersection += (weight + value);
                union += value;
            }

            if (FloatMath.IsNotZero(union))
                return intersection / union;

            return 0f;
        }

        /// <summary>
        /// Converts to a vector
        /// </summary>
        /// <param name="maxIndex">Inclusive highest index to copy (optional)</param>
        /// <returns></returns>
        public IVector AsDense(LinearAlgebraProvider lap, uint? maxIndex = null)
        {
            var indices = new Dictionary<uint, float>();
            var max = uint.MinValue;

            foreach (var item in Indices) {
                if (maxIndex.HasValue && item.Index > maxIndex.Value)
                    continue;

                if (item.Index > max)
                    max = item.Index;
                indices.Add(item.Index, item.Weight);
            }

            if (indices.Any())
                return lap.CreateVector(maxIndex ?? (max + 1), i => indices.TryGetValue(i, out var val) ? val : 0f);
            return lap.CreateVector(maxIndex ?? 0, _ => 0f);
        }

        /// <inheritdoc />
        // ReSharper disable once NonReadonlyMemberInGetHashCode
        public override int GetHashCode() => (Indices as IStructuralEquatable).GetHashCode(EqualityComparer<Item>.Default);

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is WeightedIndexList other)
                return StructuralComparisons.StructuralEqualityComparer.Equals(Indices, other.Indices);
            return false;
        }

        /// <summary>
        /// Returns a new weighted index list with unique indices - duplicate values are treated according to the specified aggregation type
        /// </summary>
        /// <returns>New weighted index list with unique indices</returns>
        public WeightedIndexList Unique(AggregationType type = AggregationType.Sum)
        {
            return new WeightedIndexList(Indices
                .GroupBy(d => d.Index)
                .Select(g => new Item(g.Key, g.Count() == 1 
                    ? g.Single().Weight 
                    : type.Aggregate(g.Select(d => d.Weight)))
                ).ToArray()
            );
        }

        /// <summary>
        /// All weights
        /// </summary>
        public IEnumerable<float> Weights => Indices.Select(d => d.Weight);
    }
}
