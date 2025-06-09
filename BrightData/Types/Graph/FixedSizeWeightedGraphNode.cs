using System;
using System.Collections.Generic;
using System.Numerics;

namespace BrightData.Types.Graph
{
    /// <summary>
    /// A graph node with a (fixed size) maximum number of neighbours
    /// NOTE: this should not be readonly, as it is mutable
    /// </summary>
    public record struct FixedSizeWeightedGraphNode<T, W, AT>(T Value) : IWeightedGraphNode<T, W>, IComparable<FixedSizeWeightedGraphNode<T, W, AT>>
        where T : unmanaged, IHaveSingleIndex
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
        where AT : unmanaged, IFixedSizeSortedArray<uint, W>
    {
        AT _neighbours = new();

        /// <inheritdoc />
        public uint Index => Value.Index;

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
        public W MinWeight => _neighbours.MinWeight;

        /// <summary>
        /// The largest neighbour weight
        /// </summary>
        public W MaxWeight => _neighbours.MaxWeight;

        /// <summary>
        /// The index of the neighbour with the smallest weight
        /// </summary>
        public uint MinNeighbourIndex => _neighbours.MinValue;

        /// <summary>
        /// The index of the neighbour with the largest weight
        /// </summary>
        public uint MaxNeighbourIndex => _neighbours.MaxValue;

        /// <summary>
        /// Sorted list of neighbour indices
        /// </summary>
        public ReadOnlySpan<uint> NeighbourIndices => _neighbours.Values;

        /// <summary>
        /// Sorted list of neighbour weights
        /// </summary>
        public ReadOnlySpan<W> NeighbourWeights => _neighbours.Weights;

        /// <summary>
        /// Tries to add a new neighbour
        /// </summary>
        /// <param name="index">Index of neighbour</param>
        /// <param name="weight">Neighbour weight</param>
        public bool TryAddNeighbour(uint index, W weight)
        {
            if(index != Value.Index)
                return _neighbours.TryAdd(index, weight);
            return false;
        }

        /// <summary>
        /// Returns a neighbour weight
        /// </summary>
        /// <param name="position">Position of neighbour to return</param>
        public (uint NeighbourIndex, W NeighbourWeight) this[uint position] => _neighbours[position];

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
