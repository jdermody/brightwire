using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace BrightWire.Models.Input
{
    /// <summary>
    /// A sparse vector with a classification label (missing indexes are assumed to be zero)
    /// </summary>
    [ProtoContract]
    public class SparseVectorClassification
    {
        /// <summary>
        /// The classification label
        /// </summary>
        [ProtoMember(1)]
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [ProtoMember(2)]
        public WeightedIndex[] Data { get; set; }

        /// <summary>
        /// Converts the sparse vector to a dense vector
        /// </summary>
        /// <param name="maxIndex">The size of the dense vector (the greatest possible sparse vector index)</param>
        public float[] Vectorise(uint maxIndex)
        {
            var vector = new float[maxIndex];
            foreach (var token in Data)
                vector[token.Index] = token.Weight;
            return vector;
        }

        /// <summary>
        /// Returns the complete set of indexes expanded by weight (so an index of weight 2 will be returned twice)
        /// </summary>
        public IReadOnlyList<uint> GetIndexList()
        {
            return Data
                .SelectMany(v => Enumerable.Range(0, Math.Max(1, Convert.ToInt32(Math.Round(v.Weight)))).Select(i => v.Index))
                .ToList()
            ;
        }
    }
}
