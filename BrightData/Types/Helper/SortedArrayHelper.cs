﻿using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace BrightData.Types.Helper
{
    /// <summary>
    /// Fixed size array helper methods
    /// </summary>
    internal static class SortedArrayHelper
    {
        internal static void InsertIndexed<V>(uint currSize, V value, Span<V> values)
            where V : unmanaged, IHaveSingleIndex
        {
            var size = (int)currSize;
            var index = value.Index;

            // use binary search to find the insertion position
            int left = 0,
                right = size - 1,
                insertPosition = size
            ;
            while (left <= right) {
                var mid = left + (right - left) / 2;
                if (values[mid].Index > index) {
                    insertPosition = mid;
                    right = mid - 1;
                }
                else {
                    left = mid + 1;
                }
            }

            if (insertPosition != size) {
                // shuffle to make room
                var vf = values[insertPosition..size];
                vf.CopyTo(values.Slice(insertPosition+1, vf.Length));
                //for (var i = size - 1; i >= insertPosition; i--) {
                //    values[i + 1] = values[i];
                //}
            }

            // insert the item
            values[insertPosition] = value;
        }

        internal static bool InsertIntoAscending<V, W>(bool enforceUnique, uint currSize, uint maxSize, V value, W weight, Span<V> values, Span<W> weights)
            where V : IComparable<V>
            where W : unmanaged, INumber<W>, IMinMaxValue<W>
        {
            var size = (int)currSize;
            var isFull = size == maxSize;

            // check to see if it should be inserted
            if (isFull && weights[size - 1] <= weight)
                return false;

            // special case for size 1
            if (maxSize == 1) {
                weights[0] = weight;
                values[0] = value;
                return true;
            }

            // use binary search to find the insertion position
            int left = 0,
                right = size - 1,
                insertPosition = size
            ;
            while (left <= right) {
                var mid = left + (right - left) / 2;
                if (weights[mid] > weight) {
                    insertPosition = mid;
                    right = mid - 1;
                }
                else {
                    // check for an existing weight/value
                    if (enforceUnique && weights[mid] == weight && values[mid].CompareTo(value) == 0)
                        return false;
                    left = mid + 1;
                }
            }

            if (enforceUnique) {
                // check if the same element already exists in the left partition
                for (var i = insertPosition - 1; i >= 0; i--) {
                    if (weights[i] < weight)
                        break;
                    if (values[i].CompareTo(value) == 0)
                        return false;
                }

                // check if the same element already exists in the right partition
                for (var i = insertPosition; i < size; i++) {
                    if (weights[i] > weight)
                        break;
                    if (values[i].CompareTo(value) == 0)
                        return false;
                }
            }

            if (insertPosition == size) {
                // there is no room left
                if (isFull)
                    return false;
            }
            else {
                // shuffle to make room
                ShiftRight(isFull, insertPosition, size, values, weights);
                //for (var i = size - (isFull ? 2 : 1); i >= insertPosition; i--) {
                //    values[i + 1] = values[i];
                //    weights[i + 1] = weights[i];
                //}
            }

            // insert the item
            values[insertPosition] = value;
            weights[insertPosition] = weight;
            return true;
        }

        internal static bool InsertIntoDescending<V, W>(bool enforceUnique, uint currSize, uint maxSize, V value, W weight, Span<V> values, Span<W> weights)
            where V : IComparable<V>
            where W : unmanaged, INumber<W>, IMinMaxValue<W>
        {
            var size = (int)currSize;
            var isFull = size == maxSize;

            // check to see if it should be inserted
            if (isFull && weights[size - 1] >= weight)
                return false;

            // special case for size 1
            if (maxSize == 1) {
                weights[0] = weight;
                values[0] = value;
                return true;
            }

            // use binary search to find the insertion position
            int left = 0,
                right = size - 1,
                insertPosition = size
            ;
            while (left <= right) {
                var mid = left + (right - left) / 2;
                if (weights[mid] < weight) {
                    insertPosition = mid;
                    right = mid - 1;
                }
                else {
                    // check for an existing weight/value
                    if (enforceUnique && weights[mid] == weight && values[mid].CompareTo(value) == 0)
                        return false;
                    left = mid + 1;
                }
            }

            if (enforceUnique) {
                // check if the same element already exists in the left partition
                for (var i = insertPosition - 1; i >= 0; i--) {
                    if (weights[i] > weight)
                        break;
                    if (values[i].CompareTo(value) == 0)
                        return false;
                }

                // check if the same element already exists in the right partition
                for (var i = insertPosition; i < size; i++) {
                    if (weights[i] < weight)
                        break;
                    if (values[i].CompareTo(value) == 0)
                        return false;
                }
            }

            if (insertPosition == size) {
                // there is no room left
                if (isFull)
                    return false;
            }
            else {
                // shuffle to make room
                ShiftRight(isFull, insertPosition, size, values, weights);
                //for (var i = size - (isFull ? 2 : 1); i >= insertPosition; i--) {
                //    values[i + 1] = values[i];
                //    weights[i + 1] = weights[i];
                //}
            }

            // insert the item
            values[insertPosition] = value;
            weights[insertPosition] = weight;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void ShiftRight<V, W>(bool isFull, int fromPosition, int size, Span<V> values, Span<W> weights)
        {
            var len = size - fromPosition;
            var cs = isFull ? len - 1 : len;
            var vf = values.Slice(fromPosition, cs);
            vf.CopyTo(values.Slice(fromPosition+1, vf.Length));
            var wf = weights.Slice(fromPosition, cs);
            wf.CopyTo(weights.Slice(fromPosition+1, wf.Length));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void RemoveAt<V, W>(int index, Span<V> values, Span<W> weights)
        {
            values[(index + 1)..].CopyTo(values[index..]);
            weights[(index + 1)..].CopyTo(weights[index..]);
        }
    }
}
