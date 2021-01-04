using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using Microsoft.VisualBasic;

namespace BrightData
{
    /// <summary>
    /// Contains a list of indices
    /// </summary>
    public class IndexList : IHaveIndices, ISerializable, I
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">Bright data context</param>
        public IndexList(IBrightDataContext context)
        {
            Context = context;
        }

        /// <summary>
        /// Bright data context
        /// </summary>
        public IBrightDataContext Context { get; private set; }

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
            if (Count < 32) {
                var indices = String.Join('|', Indices);
                return $"IndexList - {indices}";
            } 
            return $"IndexList ({Count} indices)";
        }

        public static IndexList Merge(IEnumerable<IndexList> lists)
        {
            IBrightDataContext context = null;
            var items = new HashSet<uint>();
            foreach (var list in lists) {
                context = list.Context;
                foreach (var index in list.Indices)
                    items.Add(index);
            }

            return new IndexList(context) {
                Indices = items.OrderBy(d => d).ToArray()
            };
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

        public void Initialize(IBrightDataContext context, BinaryReader reader)
        {
            var len = reader.ReadInt32();
            Indices = new uint[len];
            reader.BaseStream.Read(MemoryMarshal.Cast<uint, byte>(Indices));
            Context = context;
        }

        /// <summary>
        /// Creates an index list from a binary reader
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reader">The binary reader</param>
        public static IndexList ReadFrom(IBrightDataContext context, BinaryReader reader)
        {
            var ret = new IndexList(context);;
            ret.Initialize(context, reader);
            return ret;
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

        public Vector<float> ToDense(uint? maxIndex)
        {
            var indices = new HashSet<uint>();
            uint max = maxIndex ?? uint.MinValue;

            foreach(var item in Indices) {
                if(!maxIndex.HasValue && item > max)
                    max = item;
                indices.Add(item);
            }
            if(indices.Any())
                return Context.CreateVector(max+1, i => indices.Contains(i) ? 1f : 0f);
            return Context.CreateVector(maxIndex ?? 0, 0f);
        }

        public bool HasIndex(uint index) => Indices.Contains(index);

        // TODO: pearson similarity, overlap similarity
        // use overlap to build a graph: https://jbarrasa.com/2017/03/31/quickgraph5-learning-a-taxonomy-from-your-tagged-data/
    }
}
