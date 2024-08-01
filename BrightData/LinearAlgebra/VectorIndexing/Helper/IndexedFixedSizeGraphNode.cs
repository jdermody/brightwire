using System;
using System.Collections.Generic;
using System.Numerics;

namespace BrightData.LinearAlgebra.VectorIndexing.Helper
{
    /// <summary>
    /// Fixed size indexed graph node that maintains weighted list of neighbours as a max heap
    /// </summary>
    /// <typeparam name="T">Weight type</typeparam>
    /// <typeparam name="AT">Array type</typeparam>
    /// <param name="Index"></param>
    public record struct IndexedFixedSizeGraphNode<T, AT>(uint Index)
        where T : unmanaged, INumber<T>, IMinMaxValue<T>
        where AT: struct, IFixedSizeSortedArray<uint, T>
    {
        AT _neighbours = new();

        /// <summary>
        /// Max number of neighbours
        /// </summary>
        public int MaxNeighbours => _neighbours.MaxSize;

        /// <summary>
        /// Current number of neighbours
        /// </summary>
        public byte NeighbourCount => _neighbours.Size;

        /// <summary>
        /// The smallest neighbour weight
        /// </summary>
        public T MinWeight => _neighbours.MinWeight;

        /// <summary>
        /// The largest neighbour weight
        /// </summary>
        public T MaxWeight => _neighbours.MaxWeight;

        /// <summary>
        /// The index of the neighbour with the smallest weight
        /// </summary>
        public uint MinNeighbourIndex => _neighbours.MinValue;

        /// <summary>
        /// The index of the neighbour with the largest weight
        /// </summary>
        public uint MaxNeighbourIndex => _neighbours.MaxValue;

        /// <summary>
        /// Tries to add a new neighbour - will succeed if there aren't already max neighbours with a smaller weight
        /// </summary>
        /// <param name="neighbourIndex"></param>
        /// <param name="neighbourWeight"></param>
        /// <param name="enforceUnique"></param>
        /// <returns></returns>
        public bool TryAddNeighbour(uint neighbourIndex, T neighbourWeight, bool enforceUnique = true) => _neighbours.TryAdd(neighbourIndex, neighbourWeight, enforceUnique);

        /// <summary>
        /// Sorted list of neighbour indices
        /// </summary>
        public ReadOnlySpan<uint> NeighbourIndices => _neighbours.Values;

        /// <summary>
        /// Sorted list of neighbour weights
        /// </summary>
        public ReadOnlySpan<T> NeighbourWeights => _neighbours.Weights;

        /// <summary>
        /// Returns a neighbour weight
        /// </summary>
        /// <param name="index"></param>
        public (uint NeighbourIndex, T NeighbourWeight) this[byte index] => _neighbours[index];

        /// <summary>
        /// Enumerates the weighted neighbours
        /// </summary>
        public IEnumerable<(uint NeighbourIndex, T NeighbourWeight)> WeightedNeighbours => _neighbours.Elements;
    }
}
