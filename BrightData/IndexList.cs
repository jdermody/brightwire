using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using BrightData.LinearAlgebra;

namespace BrightData
{
    /// <summary>
    /// Contains a list of indices
    /// </summary>
    public struct IndexList : IHaveIndices, IAmSerializable, IEquatable<IndexList>
    {
        readonly uint[] _indices;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="indices">Initial indices</param>
        public IndexList(params uint[] indices)
        {
            _indices = indices;
        }

        /// <summary>
        /// Current indices in list
        /// </summary>
        public IReadOnlyList<uint> Indices => _indices;

        /// <summary>
        /// Indices as a span
        /// </summary>
        /// <returns></returns>
        public ReadOnlySpan<uint> AsSpan() => new(_indices);

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
        public int Count => _indices.Length;

        /// <summary>
        /// ToString override
        /// </summary>
        public override string ToString()
        {
            if (Count < 32) {
                var indices = String.Join('|', Indices);
                return $"IndexList - {indices}";
            } 
            return $"IndexList ({Count} indices)";
        }

        /// <inheritdoc />
        public bool Equals(IndexList other) => StructuralComparisons.StructuralEqualityComparer.Equals(_indices, other._indices);

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is IndexList other && Equals(other);
        }

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
            return Indices.GetHashCode();
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
            writer.Write(Count);
            fixed (uint* ptr = _indices) {
                writer.Write(new ReadOnlySpan<byte>(ptr, Indices.Count * sizeof(uint)));
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
        /// <param name="lap">Linear algebra provider</param>
        /// <param name="maxIndex">Maximum index to include</param>
        /// <returns></returns>
        public IVector AsDense(LinearAlgebraProvider lap, uint? maxIndex = null)
        {
            var indices = new HashSet<uint>();
            var max = maxIndex ?? uint.MinValue;

            foreach (var item in Indices) {
                if (!maxIndex.HasValue && item > max)
                    max = item;
                indices.Add(item);
            }

            if(indices.Any())
                return lap.CreateVector(max+1, i => indices.Contains(i) ? 1f : 0f);
            return lap.CreateVector(maxIndex ?? 0, 0f);
        }

        /// <summary>
        /// Checks if the specified index has been set
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool HasIndex(uint index) => Indices.Contains(index);

        // TODO: pearson similarity, overlap similarity
        // use overlap to build a graph: https://jbarrasa.com/2017/03/31/quickgraph5-learning-a-taxonomy-from-your-tagged-data/
    }
}
