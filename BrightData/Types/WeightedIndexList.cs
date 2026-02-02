using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using BrightData.Analysis;
using BrightData.Helper;
using BrightData.LinearAlgebra.ReadOnly;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData.Types
{
    /// <summary>
    /// A list of weighted indices is a sparse vector
    /// </summary>
    public readonly struct WeightedIndexList :
        IHaveIndices,
        IAmSerializable,
        IEquatable<WeightedIndexList>,
        IHaveDataAsReadOnlyByteSpan,
        IHaveSize,
        IHaveSpanOf<WeightedIndexList.Item>,
        IHaveReadOnlyContiguousMemory<WeightedIndexList.Item>,
        IHaveMemory<WeightedIndexList.Item>
    {
        readonly ReadOnlyMemory<Item> _indices;

        /// <summary>
        /// Creates a weighted index list from an array of indices
        /// </summary>
        /// <param name="indices">Weighted indices</param>
        public WeightedIndexList(params Item[] indices) => _indices = indices;

        /// <summary>
        /// Creates a weighted index list from a buffer of items
        /// </summary>
        /// <param name="indices"></param>
        public WeightedIndexList(ReadOnlyMemory<Item> indices) => _indices = indices;

        /// <summary>
        /// Creates a weighted index list from a byte span
        /// </summary>
        /// <param name="data"></param>
        public WeightedIndexList(ReadOnlySpan<byte> data)
        {
            _indices = data.Cast<byte, Item>().ToArray();
        }

        /// <summary>
        /// Creates a weighted index list from a span of floats
        /// </summary>
        /// <param name="data"></param>
        public WeightedIndexList(ReadOnlySpan<float> data)
        {
            var list = new List<Item>();
            uint index = 0;
            foreach (var item in data)
            {
                if (Math<float>.IsNotZero(item))
                    list.Add(new(index, item));
                ++index;
            }
            _indices = list.ToArray();
        }

        /// <summary>
        /// Weighted indices
        /// </summary>
        public IEnumerable<Item> Indices
        {
            get
            {
                for (var i = 0; i < _indices.Length; i++)
                    yield return _indices.Span[i];
            }
        }

        /// <summary>
        /// Returns a span of the weighted indices
        /// </summary>
        /// <returns></returns>
        public ReadOnlySpan<Item> AsSpan() => _indices.Span;

        /// <summary>
        /// An item within a weighted index list
        /// </summary>
        //[StructLayout(LayoutKind.Sequential, Pack=0)]
        public readonly record struct Item(uint Index, float Weight)
        {
            /// <inheritdoc />
            public override string ToString() => $"{Index}:{Weight}";

            /// <inheritdoc />
            public override int GetHashCode() => HashCode.Combine(Index, Weight);

            /// <inheritdoc />
            public bool Equals(Item other) => Index == other.Index && Math.Abs(Weight - other.Weight) < Math<float>.AlmostZero;
        }

        /// <summary>
        /// Index iterator
        /// </summary>
        public ref struct ItemIterator
        {
            readonly ReadOnlySpan<Item> _items;
            int _pos = -1;

            internal ItemIterator(ReadOnlySpan<Item> items) => _items = items;

            /// <summary>
            /// Current item
            /// </summary>
            public readonly ref readonly Item Current
            {
                get
                {
                    if (_pos < _items.Length)
                        return ref _items[_pos];
                    return ref Unsafe.NullRef<Item>();
                }
            }

            /// <summary>
            /// Advances the iterator
            /// </summary>
            /// <returns></returns>
            public bool MoveNext() => ++_pos < _items.Length;

            /// <summary>
            /// Get enumerator
            /// </summary>
            /// <returns></returns>
            public readonly ItemIterator GetEnumerator() => this;
        }

        /// <summary>
        /// Enumerates the weighted indices in the list
        /// </summary>
        /// <returns></returns>
        public ItemIterator GetEnumerator() => new(_indices.Span);

        /// <summary>
        /// Creates a new weighted index list
        /// </summary>
        /// <param name="indexList">Weighted indices</param>
        /// <returns></returns>
        public static WeightedIndexList Create(params Item[] indexList) => Merge(indexList.AsSpan());

        /// <summary>
        /// Creates a new weighted index list
        /// </summary>
        /// <param name="indexList">Weighted indices</param>
        /// <returns></returns>
        public static WeightedIndexList Create(ReadOnlySpan<Item> indexList) => Merge(indexList);

        /// <summary>
        /// Creates a new weighted index list
        /// </summary>
        /// <param name="indexList">Weighted indices</param>
        /// <returns></returns>
        public static WeightedIndexList Create(IEnumerable<Item> indexList) => Merge(indexList);

        /// <summary>
        /// Creates a new weighted index list
        /// </summary>
        /// <param name="indexList">Weighted indices</param>
        /// <returns></returns>
        public static WeightedIndexList Create(params (uint Index, float Weight)[] indexList) =>
            Merge(indexList.Select(d => new Item(d.Index, d.Weight)));

        /// <summary>
        /// Creates a new weighted index list
        /// </summary>
        /// <param name="indexList">Weighted indices</param>
        /// <returns></returns>
        public static WeightedIndexList Create(IEnumerable<(uint Index, float Weight)> indexList) =>
            Merge(indexList.Select(d => new Item(d.Index, d.Weight)));

        /// <summary>
        /// The number of items in the list
        /// </summary>
        public uint Size => (uint)_indices.Length;

        /// <summary>
        /// Returns the indices
        /// </summary>
        public ReadOnlyMemory<Item> ReadOnlyMemory => _indices;

        /// <summary>
        /// ToString override
        /// </summary>
        public override string ToString()
        {
            if (Size < 32)
            {
                var indices = string.Join('|', Indices);
                return $"Weighted Index List - {indices}";
            }
            return $"Weighted Index List ({Size} indices)";
        }

        /// <inheritdoc />
        public void Initialize(BrightDataContext context, BinaryReader reader)
        {
            var len = reader.ReadInt32();
            ref var array = ref Unsafe.AsRef(in _indices);
            array = reader.BaseStream.ReadArray<Item>(len);
        }

        /// <summary>
        /// Merges a sequence of weighted index lists into one list
        /// </summary>
        /// <param name="lists">Lists to merge</param>
        /// <param name="mergeOperation">How to merge item weights</param>
        /// <returns></returns>
        public static WeightedIndexList Merge(IEnumerable<WeightedIndexList> lists, AggregationType mergeOperation = AggregationType.Sum) => Merge(lists.SelectMany(x => x.Indices), mergeOperation);

        /// <summary>
        /// Merges a sequence of weighted index items into one list
        /// </summary>
        /// <param name="items">Lists to merge</param>
        /// <param name="mergeOperation">How to merge item weights</param>
        /// <returns></returns>
        [OverloadResolutionPriority(2)]
        public static WeightedIndexList Merge(IEnumerable<Item> items, AggregationType mergeOperation = AggregationType.Sum)
        {
            var itemWeights = new Dictionary<uint, List<float>>();
            foreach (var (index, weight) in items) {
                if (!itemWeights.TryGetValue(index, out var weights))
                    itemWeights.Add(index, weights = []);
                weights.Add(weight);
            }

            return new WeightedIndexList(
                itemWeights.Select(d => new Item(d.Key, mergeOperation.Aggregate(d.Value))).ToArray()
            );
        }

        /// <summary>
        /// Merges a sequence of weighted index items into one list
        /// </summary>
        /// <param name="items">Lists to merge</param>
        /// <param name="mergeOperation">How to merge item weights</param>
        /// <returns></returns>
        [OverloadResolutionPriority(1)]
        public static WeightedIndexList Merge(ReadOnlySpan<Item> items, AggregationType mergeOperation = AggregationType.Sum)
        {
            var itemWeights = new Dictionary<uint, List<float>>();
            foreach (var (index, weight) in items) {
                if (!itemWeights.TryGetValue(index, out var weights))
                    itemWeights.Add(index, weights = []);
                weights.Add(weight);
            }

            return new WeightedIndexList(
                itemWeights.Select(d => new Item(d.Key, mergeOperation.Aggregate(d.Value))).ToArray()
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

            writer.WriteValue(string.Join("|", Indices
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
            writer.Write(Size);
            fixed (Item* ptr = _indices.Span)
            {
                writer.Write(new ReadOnlySpan<byte>(ptr, _indices.Length * sizeof(Item)));
            }
        }

        /// <summary>
        /// Converts the weighted index list to XML
        /// </summary>
        public string ToXml()
        {
            var sb = new StringBuilder();
            var settings = new XmlWriterSettings
            {
                OmitXmlDeclaration = true,
                Encoding = Encoding.UTF8
            };
            using var writer = XmlWriter.Create(sb, settings);
            WriteTo(null, writer);
            return sb.ToString();
        }

        /// <summary>
        /// Converts the weighted index-list to an unweighted index-list (only those indices whose weight is not zero)
        /// </summary>
        /// <returns></returns>
        public IndexList AsIndexList() => IndexList.Create(Indices.Where(ind => Math<float>.IsNotZero(ind.Weight)).Select(ind => ind.Index).ToArray());

        IEnumerable<uint> IHaveIndices.Indices => Indices.Select(ind => ind.Index);

        /// <summary>
        /// Dot product of this combined with the other weighted index list
        /// </summary>
        /// <param name="other">Other weighted index list</param>
        public float Dot(WeightedIndexList other)
        {
            var ret = 0f;
            var otherTable = other.Indices.ToDictionary(d => d.Index, d => d.Weight);
            foreach (ref readonly var item in ReadOnlySpan)
            {
                if (otherTable.TryGetValue(item.Index, out var otherWeight))
                    ret += otherWeight * item.Weight;
            }
            return ret;
        }

        /// <summary>
        /// Magnitude of weights
        /// </summary>
        public float Magnitude => Size > 0
            ? Math<float>.Sqrt(Indices.Sum(d => d.Weight * d.Weight))
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
            foreach (ref readonly var item in _indices.Span)
                data[item.Index] = (item.Weight, 0f);

            foreach (ref readonly var item in other._indices.Span)
            {
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
        public float GetMaxWeight() => Size > 0
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
                    intersection += weight + value;
                union += value;
            }

            if (Math<float>.IsNotZero(union))
                return intersection / union;

            return 0f;
        }

        /// <summary>
        /// Converts to a vector
        /// </summary>
        /// <param name="maxIndex">Inclusive highest index to copy (optional)</param>
        /// <returns></returns>
        public ReadOnlyVector<float> AsDense(uint? maxIndex = null)
        {
            var indices = new Dictionary<uint, float>();
            var max = uint.MinValue;

            foreach (ref readonly var item in _indices.Span)
            {
                if (maxIndex.HasValue && item.Index > maxIndex.Value)
                    continue;

                if (item.Index > max)
                    max = item.Index;
                indices.Add(item.Index, item.Weight);
            }
            return indices.Count > 0
                ? new ReadOnlyVector<float>(maxIndex ?? max + 1, i => indices.GetValueOrDefault(i, 0f))
                : new ReadOnlyVector<float>(maxIndex ?? 0, _ => 0f);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            foreach (var item in _indices.Span)
                hashCode.Add(item);
            return hashCode.ToHashCode();
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
        /// Creates a new weighted index list with each item normalised with the normalisation model
        /// </summary>
        /// <param name="model">Normalisation model</param>
        /// <returns></returns>
        public WeightedIndexList Normalize(NormalisationModel model)
        {
            return Create(Indices.Select(x => x with { Weight = (float)model.Normalize(x.Weight) }));
        }

        /// <summary>
        /// All weights
        /// </summary>
        public IEnumerable<float> Weights => Indices.Select(d => d.Weight);

        /// <inheritdoc />
        public bool Equals(WeightedIndexList other) => DataAsBytes.SequenceEqual(other.DataAsBytes);

        /// <inheritdoc />
        public ReadOnlySpan<Item> GetSpan(ref SpanOwner<Item> temp, out bool wasTempUsed)
        {
            wasTempUsed = false;
            return _indices.Span;
        }

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

        /// <inheritdoc />
        public ReadOnlySpan<byte> DataAsBytes => _indices.Span.AsBytes();

        /// <inheritdoc />
        public ReadOnlySpan<Item> ReadOnlySpan => _indices.Span;

        /// <inheritdoc />
        public ReadOnlyMemory<Item> ContiguousMemory => _indices;
    }
}
