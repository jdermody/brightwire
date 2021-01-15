using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using Microsoft.VisualBasic;

namespace BrightData
{
    /// <summary>
    /// Contains a list of indices
    /// </summary>
    public class IndexList : IHaveIndices, ISerializable, IHaveBrightDataContext
    {
        internal IndexList(IBrightDataContext context)
        {
            Context = context;
        }

        /// <inheritdoc />
        public IBrightDataContext Context { get; private set; }

        /// <summary>
        /// The list of indices
        /// </summary>
        public uint[]? Indices { get; private set; }

        internal static IndexList Create(IBrightDataContext context, params uint[] indices) => new IndexList(context) { Indices = indices };
        internal static IndexList Create(IBrightDataContext context, IEnumerable<uint> indices) => new IndexList(context) { Indices = indices.ToArray() };

        /// <summary>
        /// The number of items in the list
        /// </summary>
        public int Count => Indices?.Length ?? 0;

        /// <summary>
        /// ToString override
        /// </summary>
        public override string ToString()
        {
            if (Count < 32 && Indices != null) {
                var indices = String.Join('|', Indices);
                return $"IndexList - {indices}";
            } 
            return $"IndexList ({Count} indices)";
        }

        /// <summary>
        /// Merges a sequence of index lists into a single index list
        /// </summary>
        /// <param name="lists">Lists to merge</param>
        public static IndexList Merge(IEnumerable<IndexList> lists)
        {
            IBrightDataContext? context = null;
            var items = new HashSet<uint>();
            foreach (var list in lists) {
                context = list.Context;
                if (list.Indices == null)
                    continue;
                foreach (var index in list.Indices)
                    items.Add(index);
            }

            return new IndexList(context ?? throw new ArgumentException("No valid index lists were supplied")) {
                Indices = items.OrderBy(d => d).ToArray()
            };
        }

        /// <inheritdoc />
        public override int GetHashCode() => Indices?.GetHashCode() ?? 0;

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is IndexList other)
                return StructuralComparisons.StructuralEqualityComparer.Equals(Indices, other.Indices);
            return false;
        }

        /// <summary>
        /// Writes the data to an XML writer
        /// </summary>
        /// <param name="name">The name to give the data</param>
        /// <param name="writer">The writer to write to</param>
        public void WriteTo(string? name, XmlWriter writer)
        {
            writer.WriteStartElement(name ?? "index-list");

            if (Indices != null)
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
            if (Indices != null) {
                fixed (uint* ptr = Indices) {
                    writer.Write(new ReadOnlySpan<byte>(ptr, Indices.Length * sizeof(uint)));
                }
            }
        }

        /// <inheritdoc />
        public void Initialize(IBrightDataContext context, BinaryReader reader)
        {
            var len = reader.ReadInt32();
            Indices = new uint[len];
            reader.BaseStream.Read(MemoryMarshal.Cast<uint, byte>(Indices));
            Context = context;
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

        IEnumerable<uint> IHaveIndices.Indices => Indices ?? Enumerable.Empty<uint>();

        /// <summary>
        /// Calculates the jaccard similarity between this and another index list
        /// </summary>
        /// <param name="other">Index list to compare to</param>
        /// <returns></returns>
        public float JaccardSimilarity(IndexList other)
        {
            if (Indices != null && other.Indices != null) {
                var set1 = new HashSet<uint>(Indices);
                var set2 = new HashSet<uint>(other.Indices);
                uint intersection = 0, union = (uint) set1.Count;

                foreach (var item in set2) {
                    if (set1.Contains(item))
                        intersection++;
                    else
                        union++;
                }

                if (union > 0)
                    return (float) intersection / union;
            }

            return 0f;
        }

        /// <summary>
        /// Converts to a vector
        /// </summary>
        /// <param name="maxIndex">Maximum index to include</param>
        /// <returns></returns>
        public Vector<float> ToDense(uint? maxIndex)
        {
            var indices = new HashSet<uint>();
            uint max = maxIndex ?? uint.MinValue;

            if (Indices != null) {
                foreach (var item in Indices) {
                    if (!maxIndex.HasValue && item > max)
                        max = item;
                    indices.Add(item);
                }
            }

            if(indices.Any())
                return Context.CreateVector(max+1, i => indices.Contains(i) ? 1f : 0f);
            return Context.CreateVector(maxIndex ?? 0, 0f);
        }

        /// <summary>
        /// Checks if the specified index has been set
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool HasIndex(uint index) => Indices?.Contains(index) ?? false;

        // TODO: pearson similarity, overlap similarity
        // use overlap to build a graph: https://jbarrasa.com/2017/03/31/quickgraph5-learning-a-taxonomy-from-your-tagged-data/
    }
}
