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
using BrightData.LinearAlgebra.ReadOnly;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData
{
    /// <summary>
    /// Contains a list of indices
    /// </summary>
    public readonly struct IndexList : 
        IHaveIndices, 
        IAmSerializable, 
        IEquatable<IndexList>, 
        IHaveDataAsReadOnlyByteSpan, 
        IHaveReadOnlyContiguousSpan<uint>, 
        IHaveSpanOf<uint>, 
        IHaveSize,
        IHaveMemory<uint>
    {
        readonly ReadOnlyMemory<uint> _indices;

        /// <summary>
        /// Index iterator
        /// </summary>
        public ref struct ItemIterator
        {
            readonly ReadOnlySpan<uint> _items;
            int _pos = -1;

            internal ItemIterator(ReadOnlySpan<uint> items) => _items = items;

            /// <summary>
            /// Current value
            /// </summary>
            public readonly uint Current
            {
                get
                {
                    if (_pos < _items.Length)
                        return _items[_pos];
                    return uint.MaxValue;
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
        /// Enumerates the indices in the list
        /// </summary>
        /// <returns></returns>
        public ItemIterator GetEnumerator() => new(_indices.Span);

        /// <summary>
        /// Creates an index list from an array of indices
        /// </summary>
        /// <param name="indices">Initial indices</param>
        public IndexList(params uint[] indices)
        {
            _indices = indices;
        }

        public IndexList(ReadOnlyMemory<uint> indices) => _indices = indices;

        public IndexList(ReadOnlySpan<float> data)
        {
            var list = new List<uint>();
            uint index = 0;
            foreach (var item in data) {
                if (FloatMath.IsNotZero(item))
                    list.Add(index);
                ++index;
            }
            _indices = list.ToArray();
        }

        /// <summary>
        /// Creates an index list from a byte span
        /// </summary>
        /// <param name="data"></param>
        public IndexList(ReadOnlySpan<byte> data)
        {
            _indices = data.Cast<byte, uint>().ToArray();
        }

        /// <summary>
        /// Returns the indices
        /// </summary>
        public ReadOnlyMemory<uint> ReadOnlyMemory => _indices;

        /// <summary>
        /// Current indices in list
        /// </summary>
        public IEnumerable<uint> Indices
        {
            get
            {
                for (var i = 0; i < _indices.Length; ++i)
                    yield return _indices.Span[i];
            }
        }

        /// <summary>
        /// Indices as a span
        /// </summary>
        /// <returns></returns>
        public ReadOnlySpan<uint> AsSpan() => _indices.Span;
        /// <summary>
        /// Creates an index list
        /// </summary>
        /// <param name="indices"></param>
        /// <returns></returns>
        public static IndexList Create(params uint[] indices) => new(indices);

        /// <summary>
        /// Creates an index list
        /// </summary>
        /// <param name="indices"></param>
        /// <returns></returns>
        public static IndexList Create(ReadOnlySpan<uint> indices) => new(indices.ToArray());

        /// <summary>
        /// Creates an index list
        /// </summary>
        /// <param name="indices"></param>
        /// <returns></returns>
        public static IndexList Create(IEnumerable<uint> indices) => new(indices.ToArray());

        /// <summary>
        /// The number of items in the list
        /// </summary>
        public uint Size => (uint)_indices.Length;

        /// <summary>
        /// ToString override
        /// </summary>
        public override string ToString()
        {
            if (Size < 32) {
                var indices = String.Join('|', Indices);
                return $"IndexList - {indices}";
            } 
            return $"IndexList ({Size} indices)";
        }

        /// <inheritdoc />
        public bool Equals(IndexList other) => StructuralComparisons.StructuralEqualityComparer.Equals(_indices, other._indices);

        /// <inheritdoc />
        public ReadOnlySpan<uint> GetSpan(ref SpanOwner<uint> temp, out bool wasTempUsed)
        {
            wasTempUsed = false;
            return _indices.Span;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is IndexList other && Equals(other);
        }

        /// <summary>
        /// Checks for index list equality
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static bool operator ==(IndexList lhs, IndexList rhs) => lhs.Equals(rhs);

        /// <summary>
        /// Checks for index list inequality
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static bool operator !=(IndexList lhs, IndexList rhs) => !(lhs.Equals(rhs));

        /// <summary>
        /// Merges a sequence of index lists into a single index list
        /// </summary>
        /// <param name="lists">Lists to merge</param>
        public static IndexList Merge(IEnumerable<IndexList> lists)
        {
            var items = new HashSet<uint>();
            foreach (var list in lists) {
                foreach (var index in list.Indices)
                    items.Add(index);
            }

            return new IndexList(
                items.OrderBy(d => d).ToArray()
            );
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
        /// Writes the data to an XML writer
        /// </summary>
        /// <param name="name">The name to give the data</param>
        /// <param name="writer">The writer to write to</param>
        public void WriteTo(string? name, XmlWriter writer)
        {
            writer.WriteStartElement(name ?? "index-list");
            writer.WriteValue(String.Join("|", Indices.OrderBy(d => d).Select(c => c.ToString())));
            writer.WriteEndElement();
        }

        /// <summary>
        /// Writes the data to a binary writer
        /// </summary>
        /// <param name="writer"></param>
        public unsafe void WriteTo(BinaryWriter writer)
        {
            writer.Write(Size);
            fixed (uint* ptr = _indices.Span) {
                writer.Write(new ReadOnlySpan<byte>(ptr, (int)Size * sizeof(uint)));
            }
        }

        /// <inheritdoc />
        public void Initialize(BrightDataContext context, BinaryReader reader)
        {
            var len = reader.ReadInt32();
            ref var array = ref Unsafe.AsRef(_indices);
            array = reader.BaseStream.ReadArray<uint>(len);
        }

        /// <summary>
        /// Converts the index list to XML
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

        IEnumerable<uint> IHaveIndices.Indices => Indices;

        /// <summary>
        /// Calculates the jaccard similarity between this and another index list
        /// </summary>
        /// <param name="other">Index list to compare to</param>
        /// <returns></returns>
        public float JaccardSimilarity(IndexList other)
        {
            var set1 = new HashSet<uint>(Indices);
            var set2 = new HashSet<uint>(other.Indices);
            uint intersection = 0, union = (uint)set1.Count;

            foreach (var item in set2)
            {
                if (set1.Contains(item))
                    intersection++;
                else
                    union++;
            }

            if (union > 0)
                return (float)intersection / union;
            return 0f;
        }

        /// <summary>
        /// Converts to a dense vector in which each set index is 1
        /// </summary>
        /// <param name="maxIndex">Maximum index to include</param>
        /// <returns></returns>
        public ReadOnlyVector AsDense(uint? maxIndex = null)
        {
            var indices = new HashSet<uint>();
            var max = maxIndex ?? uint.MinValue;

            foreach (var item in Indices) {
                if (!maxIndex.HasValue && item > max)
                    max = item;
                indices.Add(item);
            }

            return indices.Any() 
                ? new ReadOnlyVector(max + 1, i => indices.Contains(i) ? 1f : 0f) 
                : new ReadOnlyVector(maxIndex ?? 0);
        }

        /// <summary>
        /// Checks if the specified index has been set
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool HasIndex(uint index) => Indices.Contains(index);

        /// <summary>
        /// Calculates the overlap similarity between this and another index list
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public float OverlapSimilarity(IndexList other)
        {
            var set = new HashSet<uint>(Indices);
            var overlap = other.Indices.Where(set.Contains).Count();
            return (float)overlap / Math.Min(Size, other.Size);
        }

        // TODO: use overlap to build a graph: https://jbarrasa.com/2017/03/31/quickgraph5-learning-a-taxonomy-from-your-tagged-data/

        /// <inheritdoc />
        public ReadOnlySpan<byte> DataAsBytes => _indices.Span.AsBytes();

        /// <inheritdoc />
        public ReadOnlySpan<uint> ReadOnlySpan => _indices.Span;

        /// <summary>
        /// Converts to a weighted index list
        /// </summary>
        /// <returns></returns>
        public WeightedIndexList AsWeightedIndexList() => WeightedIndexList.Create(Indices.Select(ind => (ind, 1f)));
    }
}
