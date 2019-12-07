using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;

namespace BrightData
{
    public class IndexList : IHaveIndices, ICanWriteToBinaryWriter
    {
        public IndexList(IBrightDataContext context)
        {
            Context = context;
        }

        public IBrightDataContext Context { get; }

        /// <summary>
        /// The list of indices
        /// </summary>
        public uint[] Indices { get; private set; }

        /// <summary>
        /// Create a new index list with the specified indices
        /// </summary>
        /// <param name="context"></param>
        /// <param name="indices">Sparse list of indices</param>
        public static IndexList Create(IBrightDataContext context, params uint[] indices) => new IndexList(context) { Indices = indices };

        public static IndexList Create(IBrightDataContext context, IEnumerable<uint> indices) => new IndexList(context) { Indices = indices.ToArray() };

        /// <summary>
        /// The number of items in the list
        /// </summary>
        public int Count => Indices?.Length ?? 0;

        /// <summary>
        /// ToString override
        /// </summary>
        public override string ToString()
        {
            return $"{Count} indices";
        }

        /// <summary>
        /// Writes the data to an XML writer
        /// </summary>
        /// <param name="name">The name to give the data</param>
        /// <param name="writer">The writer to write to</param>
        public void WriteTo(string name, XmlWriter writer)
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

        /// <summary>
        /// Creates an index list from a binary reader
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reader">The binary reader</param>
        public static IndexList ReadFrom(IBrightDataContext context, BinaryReader reader)
        {
            var len = reader.ReadInt32();
            var ret = new uint[len];
            var span = MemoryMarshal.Cast<uint, byte>(ret);
            reader.BaseStream.Read(span);

            return Create(context, ret);
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
            using (var writer = XmlWriter.Create(sb, settings))
                WriteTo(null, writer);
            return sb.ToString();
        }

        IEnumerable<uint> IHaveIndices.Indices => this.Indices;

        public float JaccardSimilarity(IndexList other)
        {
            var set1 = new HashSet<uint>(Indices);
            var set2 = new HashSet<uint>(other.Indices);
            uint intersection = 0, union = (uint)set1.Count;

            foreach(var item in set2) {
                if(set1.Contains(item))
                    intersection++;
                else
                    union++;
            }
            if(union > 0)
                return (float)intersection/union;
            return 0f;
        }

        public Vector<float> ToDense()
        {
            var indices = new HashSet<uint>();
            uint max = uint.MinValue;

            foreach(var item in Indices) {
                if(item > max)
                    max = item;
                indices.Add(item);
            }
            if(indices.Any())
                return Context.CreateVector(max+1, i => indices.Contains(i) ? 1f : 0f);
            return Context.CreateVector(0, 0f);
        }

        // TODO: pearson similarity, overlap similarity
        // use overlap to build a graph: https://jbarrasa.com/2017/03/31/quickgraph5-learning-a-taxonomy-from-your-tagged-data/
    }
}
