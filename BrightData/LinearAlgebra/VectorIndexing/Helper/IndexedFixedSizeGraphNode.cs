using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BrightData.LinearAlgebra.VectorIndexing.Helper
{
    /// <summary>
    /// Fixed size indexed graph node that maintains weighted list of neighbours as a max heap
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="Index"></param>
    public record struct IndexedFixedSizeGraphNode<T>(uint Index)
        where T : unmanaged, INumber<T>, IMinMaxValue<T>
    {
        /// <summary>
        /// Max number of neighbours
        /// </summary>
        public const int MaxNeighbours = 8;
        [InlineArray(MaxNeighbours)]
        internal struct IndexFixedSize
        {
            public uint _element0;
        }
        [InlineArray(MaxNeighbours)]
        internal struct DistanceFixedSize
        {
            public T _element0;
        }
        readonly IndexFixedSize _neighbourIndices = new();
        readonly DistanceFixedSize _neighbourWeights = new();

        /// <summary>
        /// Current number of neighbours
        /// </summary>
        public byte NeighbourCount { get; private set; } = 0;

        /// <summary>
        /// The smallest neighbour weight
        /// </summary>
        public readonly T MinDistance => NeighbourCount > 0 ? NeighbourWeights[0] : T.MaxValue;

        /// <summary>
        /// The largest neighbour weight
        /// </summary>
        public readonly T MaxDistance => NeighbourCount > 0 ? NeighbourWeights[NeighbourCount - 1] : T.MinValue;

        /// <summary>
        /// The index of the neighbour with the smallest weight
        /// </summary>
        public readonly uint MinNeighbourIndex => NeighbourCount > 0 ? NeighbourIndices[0] : uint.MaxValue;

        /// <summary>
        /// The index of the neighbour with the largest weight
        /// </summary>
        public readonly uint MaxNeighbourIndex => NeighbourCount > 0 ? NeighbourIndices[NeighbourCount - 1] : uint.MaxValue;

        /// <summary>
        /// Tries to add a new neighbour - will succeed if there aren't already max neighbours with a smaller weight
        /// </summary>
        /// <param name="neighbourIndex"></param>
        /// <param name="neighbourWeight"></param>
        /// <returns></returns>
        public unsafe bool TryAddNeighbour2(uint neighbourIndex, T neighbourWeight)
        {
            var isFull = NeighbourCount == MaxNeighbours;
            fixed (uint* indices = &_neighbourIndices._element0)
            fixed (T* weights = &_neighbourWeights._element0) {
                // check to see if it should be inserted
                if (isFull && weights[NeighbourCount - 1] <= neighbourWeight)
                    return false;

                byte insertPosition = 0;
                var foundInsertPosition = false;
                for (byte i = 0; i < NeighbourCount; i++) {
                    // check that the neighbour has not already been added
                    if (indices[i] == neighbourIndex)
                        return false;

                    // see if we should insert here
                    if (weights[i] > neighbourWeight) {
                        insertPosition = i;
                        foundInsertPosition = true;
                        break;
                    }
                }

                if (!foundInsertPosition) {
                    // there is no room left
                    if (isFull)
                        return false;

                    // insert at end
                    insertPosition = NeighbourCount;
                }
                else {
                    // shuffle to make room
                    for (var i = NeighbourCount - (isFull ? 2 : 1); i >= insertPosition; i--) {
                        indices[i + 1] = indices[i];
                        weights[i + 1] = weights[i];
                    }
                }

                // insert the item
                indices[insertPosition] = neighbourIndex;
                weights[insertPosition] = neighbourWeight;
                if (!isFull)
                    ++NeighbourCount;
            }
            return true;
        }

        public bool TryAddNeighbour(uint neighbourIndex, T neighbourWeight)
        {
            var isFull = NeighbourCount == MaxNeighbours;
            var indices = MemoryMarshal.CreateSpan(ref Unsafe.As<IndexFixedSize, uint>(ref Unsafe.AsRef(in _neighbourIndices)), MaxNeighbours);
            var weights = MemoryMarshal.CreateSpan(ref Unsafe.As<DistanceFixedSize, T>(ref Unsafe.AsRef(in _neighbourWeights)), MaxNeighbours);

            // check to see if it should be inserted
            if (isFull && weights[NeighbourCount - 1] <= neighbourWeight)
                return false;

            // Use binary search to find the insertion position
            int left = 0, 
                right = NeighbourCount - 1, 
                insertPosition = NeighbourCount
            ;
            while (left <= right) {
                var mid = left + (right - left) / 2;
                if (weights[mid] > neighbourWeight) {
                    insertPosition = mid;
                    right = mid - 1;
                }
                else if (weights[mid] < neighbourWeight) {
                    left = mid + 1;
                }
                else {
                    // Check if the neighbour already exists
                    if (indices[mid] == neighbourIndex)
                        return false;

                    left = mid + 1;
                }
            }

            // Check if the neighbour already exists in the left partition
            for (var i = insertPosition-1; i >= 0; i--) {
                if (weights[i] < neighbourWeight)
                    break;
                if (indices[i] == neighbourIndex)
                    return false;
            }

            if (insertPosition == NeighbourCount) {
                // there is no room left
                if (isFull)
                    return false;
            }
            else {
                // shuffle to make room
                for (var i = NeighbourCount - (isFull ? 2 : 1); i >= insertPosition; i--) {
                    indices[i + 1] = indices[i];
                    weights[i + 1] = weights[i];
                }
            }

            // insert the item
            indices[insertPosition] = neighbourIndex;
            weights[insertPosition] = neighbourWeight;
            if (!isFull)
                ++NeighbourCount;
            return true;
        }

        /// <summary>
        /// Sorted list of neighbour indices
        /// </summary>
        public readonly ReadOnlySpan<uint> NeighbourIndices => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<IndexFixedSize, uint>(ref Unsafe.AsRef(in _neighbourIndices)), NeighbourCount);

        /// <summary>
        /// Sorted list of neighbour weights
        /// </summary>
        public readonly ReadOnlySpan<T> NeighbourWeights => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<DistanceFixedSize, T>(ref Unsafe.AsRef(in _neighbourWeights)), NeighbourCount);

        /// <summary>
        /// Returns a neighbour weight
        /// </summary>
        /// <param name="index"></param>
        public readonly (uint NeighbourIndex, T NeighbourWeight) this[byte index]
        {
            get
            {
                if (index < NeighbourCount)
                    return (NeighbourIndices[index], NeighbourWeights[index]);
                return (uint.MaxValue, T.MinValue);
            }
        }

        /// <summary>
        /// Enumerates the weighted neighbours
        /// </summary>
        public readonly IEnumerable<(uint NeighbourIndex, T NeighbourWeight)> WeightedNeighbours
        {
            get
            {
                for (byte i = 0; i < NeighbourCount; i++)
                    yield return this[i];
            }
        }
    }
}
