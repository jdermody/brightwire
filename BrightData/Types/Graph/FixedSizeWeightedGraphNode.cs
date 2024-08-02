using System;
using System.Collections.Generic;
using System.Numerics;

namespace BrightData.Types.Graph
{
    /// <summary>
    /// A graph node with a (fixed size) maximum number of neighbours
    /// </summary>
    public record struct FixedSizeWeightedGraphNode<T, W, AT>(T Value) : IWeightedGraphNode<T, W>, IComparable<FixedSizeWeightedGraphNode<T, W, AT>>, IHaveSingleIndex
        where T : unmanaged, IHaveSingleIndex
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
        where AT : unmanaged, IFixedSizeSortedArray<uint, W>
    {
        AT _neighbours = new();

        /// <inheritdoc />
        public uint Index => Value.Index;

        /// <summary>
        /// Tries to add a new neighbour
        /// </summary>
        /// <param name="index">Index of neighbour</param>
        /// <param name="weight">Neighbour weight</param>
        public bool AddNeighbour(uint index, W weight)
        {
            if(index != Value.Index)
                return _neighbours.TryAdd(index, weight);
            return false;
        }

        /// <inheritdoc />
        public IEnumerable<(uint Index, W Weight)> WeightedNeighbours => _neighbours.Elements;

        /// <summary>
        /// Ordered span of neighbours by weight
        /// </summary>
        public ReadOnlySpan<uint> NeighbourSpan => _neighbours.Values;

        /// <summary>
        /// Enumerates the neighbours by weight
        /// </summary>
        public IEnumerable<uint> Neighbours
        {
            get
            {
                for (byte i = 0, len = _neighbours.Size; i < len; i++) {
                    yield return _neighbours.Values[i];
                }
            }
        }

        /// <inheritdoc />
        //public int CompareTo(FixedSizeWeightedGraphNode<T, W, AT>? other) => Value.Index.CompareTo(other?.Value.Index);
        public int CompareTo(FixedSizeWeightedGraphNode<T, W, AT> other) => Value.Index.CompareTo(other.Value.Index);

        /// <inheritdoc />
        public override string ToString() => $"{Value}: {_neighbours}";

        
    }
}
