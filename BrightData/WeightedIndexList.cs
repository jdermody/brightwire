using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using BrightData.Helper;

namespace BrightData
{
    /// <summary>
    /// Contains a list of weighted indices
    /// </summary>
    public class WeightedIndexList : IHaveIndices, ISerializable, IHaveDataContext
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
        }

        internal WeightedIndexList(IBrightDataContext context, Item[] indices)
        {
            Context = context;
            Indices = indices;
        }

        /// <inheritdoc />
        public IBrightDataContext Context { get; private set; }

        /// <summary>
        /// The list of indices
        /// </summary>
        public Item[] Indices { get; private set; }

        internal static WeightedIndexList Create(IBrightDataContext context, params Item[] indexList) => new WeightedIndexList(context, indexList);
        internal static WeightedIndexList Create(IBrightDataContext context, IEnumerable<Item> indexList) => new WeightedIndexList(context, indexList.ToArray());

        internal static WeightedIndexList Create(IBrightDataContext context, params (uint Index, float Weight)[] indexList) =>
            new WeightedIndexList(context, indexList.Select(d => new Item(d.Index, d.Weight)).ToArray());

        internal static WeightedIndexList Create(IBrightDataContext context, IEnumerable<(uint Index, float Weight)> indexList) =>
            new WeightedIndexList(context, indexList.Select(d => new Item(d.Index, d.Weight)).ToArray());

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
        public void Initialize(IBrightDataContext context, BinaryReader reader)
        {
            Context = context;
            var len = reader.ReadInt32();
            Indices = new Item[len];
            var span = MemoryMarshal.Cast<Item, byte>(Indices);
            reader.BaseStream.Read(span);
        }

        /// <summary>
        /// Merges a sequence of weighted index lists into one list
        /// </summary>
        /// <param name="lists">Lists to merge</param>
        /// <param name="mergeOperation">How to merge item weights</param>
        /// <returns></returns>
        public static WeightedIndexList Merge(IEnumerable<WeightedIndexList> lists, AggregationType mergeOperation = AggregationType.Average)
        {
            IBrightDataContext? context = null;
            var items = new Dictionary<uint, List<float>>();
            foreach (var list in lists) {
                context = list.Context;
                if (list.Indices != null) {
                    foreach (var index in list.Indices) {
                        if (!items.TryGetValue(index.Index, out var weights))
                            items.Add(index.Index, weights = new List<float>());
                        weights.Add(index.Weight);
                    }
                }
            }

            return new WeightedIndexList(
                context ?? throw new ArgumentException("No valid lists were supplied"),
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
        public IndexList AsIndexList() => IndexList.Create(Context, Indices.Where(ind => FloatMath.IsNotZero(ind.Weight)).Select(ind => ind.Index).ToArray());

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
            ? Convert.ToSingle(Math.Sqrt(Indices.Sum(d => d.Weight * d.Weight)))
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
            float intersection = 0f, union = 0f;
            foreach (var item in set2)
            {
                if (set1.TryGetValue(item.Key, out var weight))
                {
                    intersection += (weight + item.Value);
                    union += (weight + item.Value) / 2;
                }
                else
                    union += item.Value;
            }

            foreach (var item in set1)
            {
                if (!set2.ContainsKey(item.Key))
                    union += item.Value;
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
        public Vector<float> AsDense(uint? maxIndex)
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
                return Context.CreateVector(maxIndex ?? (max + 1), i => indices.TryGetValue(i, out var val) ? val : 0f);
            return Context.CreateVector(maxIndex ?? 0, i => 0f);
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
    }
}
