using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace BrightWire.Models.Input
{
    [ProtoContract]
    public class SparseVectorClassification
    {
        [ProtoMember(1)]
        public string Name { get; set; }

        [ProtoMember(2)]
        public WeightedIndex[] Data { get; set; }

        public float[] Vectorise(uint maxIndex)
        {
            var vector = new float[maxIndex];
            foreach (var token in Data)
                vector[token.Index] = token.Weight;
            return vector;
        }

        public IReadOnlyList<uint> GetIndexList()
        {
            return Data
                .SelectMany(v => Enumerable.Range(0, Math.Max(1, Convert.ToInt32(Math.Round(v.Weight)))).Select(i => v.Index))
                .ToList()
            ;
        }
    }
}
