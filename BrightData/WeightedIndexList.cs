using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using BrightData.Helper;
using BrightData.LinearAlgebra;

namespace BrightData
{
    /// <summary>
    /// A list of weighted indices is a sparse vector
    /// </summary>
    public readonly struct WeightedIndexList : IHaveIndices, IAmSerializable, IEquatable<WeightedIndexList>
    {
        readonly Item[] _indices;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="indices">Weighted indices</param>
        public WeightedIndexList(Item[] indices)
        {
            _indices = indices;
        }

        /// <summary>
        /// Weighted indices
        /// </summary>
        public IReadOnlyList<Item> Indices => _indices;

        /// <summary>
        /// Returns a span of the weighted indices
        /// </summary>
        /// <returns></returns>
        public ReadOnlySpan<Item> AsSpan() => new(_indices);

        /// <summary>
        /// An item within a weighted index list
        /// </summary>
        //[StructLayout(LayoutKind.Sequential, Pack=0)]
        public readonly record struct Item : IEquatable<Item>
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
            public override int GetHashCode() => HashCode.Combine(Index, Weight);

            /// <inheritdoc />
            public bool Equals(Item other) => Index == other.Index && Math.Abs(Weight - other.Weight) < FloatMath.AlmostZero;
        }

        /// <summary>
        /// Creates a new weighted index list
        /// </summary>
        /// <param name="indexList">Weighted indices</param>
        /// <returns></returns>
        public static WeightedIndexList Create(params Item[] indexList) => new(indexList);
        
        /// <summary>
        /// Creates a new weighted index list
        /// </summary>
        /// <param name="indexList">Weighted indices</param>
        /// <returns></returns>
        public static WeightedIndexList Create(ReadOnlySpan<Item> indexList) => new(indexList.ToArray());

        /// <summary>
        /// Creates a new weighted index list
        /// </summary>
        /// <param name="indexList">Weighted indices</param>
        /// <returns></returns>
        public static WeightedIndexList Create(IEnumerable<Item> indexList) => new(indexList.ToArray());

        /// <summary>
        /// Creates a new weighted index list
        /// </summary>
        /// <param name="indexList">Weighted indices</param>
        /// <returns></returns>
        public static WeightedIndexList Create(params (uint Index, float Weight)[] indexList) =>
            new(indexList.Select(d => new Item(d.Index, d.Weight)).ToArray());

        /// <summary>
        /// Creates a new weighted index list
        /// </summary>
        /// <param name="indexList">Weighted indices</param>
        /// <returns></returns>
        public static WeightedIndexList Create(IEnumerable<(uint Index, float Weight)> indexList) =>
            new(indexList.Select(d => new Item(d.Index, d.Weight)).ToArray());

        /// <summary>
        /// The number of items in the list
        /// </summary>
        public int Count => _indices.Length;

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
            ref var array = ref Unsafe.AsRef(_indices);
            array = reader.BaseStream.ReadArray<Item>(len);
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
            fixed (Item* ptr = _indices) {
                writer.Write(new ReadOnlySpan<byte>(ptr, _indices.Length * sizeof(Item)));
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
        /// Calculates the euclidean distance between this and another weighted index list
        /// </summary>
        /// <param name="other">Other list to compare</param>
        /// <returns></returns>
        public float EuclideanDistance(WeightedIndexList other)
        {
            var data = new Dictionary<uint /* index */, (float Weight1, float Weight2)>();
            foreach (var item in _indices)
                data[item.Index] = (item.Weight, 0f);

            foreach (var item in other._indices) {
                var index = item.Index;
                if (data.TryGetValue(index, out var pair))
                    data[index] = (pair.Weight1, item.Weight);
                else
                    data[index] = (0f, item.Weight);
            }

            return MathF.Sqrt(data.Values
                .Select(pair => pair.Weight1 - pair.Weight2)
                .Select(diff => diff * diff)
                .Sum()
            );
        }

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
        /// <param name="lap">Linear algebra provider</param>
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
        public override int GetHashCode() => ((IStructuralEquatable)Indices).GetHashCode(EqualityComparer<Item>.Default);

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

        /// <inheritdoc />
        public bool Equals(WeightedIndexList other) => StructuralComparisons.StructuralEqualityComparer.Equals(_indices, other._indices);

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is WeightedIndexList other && Equals(other);
        }

        /// <summary>
        /// Structural equality operator
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static bool operator ==(WeightedIndexList lhs, WeightedIndexList rhs) => lhs.Equals(rhs);

        /// <summary>
        /// Structural inequality operator
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static bool operator !=(WeightedIndexList lhs, WeightedIndexList rhs) => !lhs.Equals(rhs);
    }
}
