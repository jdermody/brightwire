using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using BrightData.Helper;

namespace BrightData
{
    public class WeightedIndexList : IHaveIndices, ICanWriteToBinaryWriter
    {
        public struct Item
        {
            public uint Index { get; }
            public float Weight { get; }

            public Item(uint index, float weight)
            {
                Index = index;
                Weight = weight;
            }

            public override string ToString() => $"{Index}: {Weight}";
            public override bool Equals(object obj) => obj is Item item && Equals(item);
            public bool Equals(Item item) => item.Index == Index && FloatMath.Equals(item.Weight, Weight);
            public override int GetHashCode() => Index.GetHashCode() ^ Weight.GetHashCode();
        }

        public WeightedIndexList(IBrightDataContext context)
        {
            Context = context;
        }

        public IBrightDataContext Context { get; }

        /// <summary>
        /// The list of indices
        /// </summary>
        public Item[] Indices { get; private set; }

        /// <summary>
        /// Create a new weighted index list with the specified weighted indices
        /// </summary>
        /// <param name="context"></param>
        /// <param name="indexList">Sparse list of weighted indices</param>
        public static WeightedIndexList Create(IBrightDataContext context, params Item[] indexList) => new WeightedIndexList(context) { Indices = indexList };
        public static WeightedIndexList Create(IBrightDataContext context, params (uint Index, float Weight)[] indexList) => 
            new WeightedIndexList(context) { 
                Indices = indexList.Select(d => new Item(d.Index, d.Weight)).ToArray() 
            };

        public static WeightedIndexList Create(IBrightDataContext context, IEnumerable<Item> indexList) => new WeightedIndexList(context) { Indices = indexList.ToArray() };

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
            writer.WriteStartElement(name ?? "weighted-index-list");

            if (Indices != null) {
                writer.WriteValue(String.Join("|", Indices
                    .OrderBy(d => d.Index)
                    .Select(c => $"{c.Index}:{c.Weight}")
                ));
            }
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
                fixed (Item* ptr = Indices) {
                    writer.Write(new ReadOnlySpan<byte>(ptr, Indices.Length * sizeof(Item)));
                }
            }
        }

        /// <summary>
        /// Creates a weighted index list from a binary reader
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reader">The binary reader</param>
        public static unsafe WeightedIndexList ReadFrom(IBrightDataContext context, BinaryReader reader)
        {
            var len = reader.ReadInt32();
            var ret = new Item[len];
            var span = MemoryMarshal.Cast<Item, byte>(ret);
            reader.BaseStream.Read(span);

            return Create(context, ret);
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
            using (var writer = XmlWriter.Create(sb, settings))
                WriteTo(null, writer);
            return sb.ToString();
        }

        /// <summary>
        /// Converts the weighted index-list to an unweighted index-list (only those indices whose weight is not zero)
        /// </summary>
        /// <returns></returns>
        public IndexList AsIndexList() => IndexList.Create(Context,
            Indices.Where(ind => FloatMath.IsNotZero(ind.Weight)).Select(ind => ind.Index).ToArray()
        );

        IEnumerable<uint> IHaveIndices.Indices => this.Indices.Select(ind => ind.Index);

        public float Dot(WeightedIndexList other)
        {
            var otherTable = other.Indices.ToDictionary(d => d.Index, d => d.Weight);
            var ret = 0f;
            foreach (var item in Indices) {
                if (otherTable.TryGetValue(item.Index, out var otherWeight))
                    ret += otherWeight * item.Weight;
            }
            return ret;
        }

        public float Magnitude => Convert.ToSingle(Math.Sqrt(Indices.Sum(d => d.Weight * d.Weight)));
        public float CosineSimilarity(WeightedIndexList other) => Dot(other) / (Magnitude * other.Magnitude);
        public float GetMaxWeight() => Indices.Max(item => item.Weight);

        public WeightedIndexList Normalise()
        {
            var maxWeight = GetMaxWeight();
            return Create(Context, Indices.Select(item => new Item(item.Index, item.Weight / maxWeight)));
        }

        public float JaccardSimilarity(WeightedIndexList other)
        {
            var set1 = Indices.GroupBy(d => d.Index).ToDictionary(g => g.Key, g => g.Sum(d => d.Weight));
            var set2 = other.Indices.GroupBy(d => d.Index).ToDictionary(g => g.Key, g => g.Sum(d => d.Weight));
            float intersection = 0f, union = 0f;
            foreach (var item in set2) {
                if (set1.TryGetValue(item.Key, out var weight)) {
                    intersection += (weight + item.Value);
                    union += (weight + item.Value) / 2;
                } else
                    union += item.Value;
            }
            foreach (var item in set1) {
                if (!set2.ContainsKey(item.Key))
                    union += item.Value;
            }
            if (FloatMath.IsNotZero(union))
                return intersection / union;
            return 0f;
        }

        public Vector<float> ToDense()
        {
            var indices = new Dictionary<uint, float>();
            uint max = uint.MinValue;
            foreach (var item in Indices) {
                if (item.Index > max)
                    max = item.Index;
                indices.Add(item.Index, item.Weight);
            }
            if (indices.Any())
                return Context.CreateVector(max + 1, i => indices.TryGetValue(i, out var val) ? val : 0f);
            return Context.CreateVector(0, i => 0f);
        }
    }
}
