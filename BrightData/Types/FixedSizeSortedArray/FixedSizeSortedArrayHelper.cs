using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BrightData.Types.FixedSizeSortedArray
{
    internal class FixedSizeSortedArrayHelper
    {
        public static bool InsertIntoAscending<V, W>(bool enforceUnique, ref byte size, byte maxSize, V value, W weight, Span<V> values, Span<W> weights)
            where V : IComparable<V>
            where W : unmanaged, INumber<W>, IMinMaxValue<W>
        {
            var isFull = size == maxSize;

            // check to see if it should be inserted
            if (isFull && weights[size - 1] <= weight)
                return false;

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
                for (var i = size - (isFull ? 2 : 1); i >= insertPosition; i--) {
                    values[i + 1] = values[i];
                    weights[i + 1] = weights[i];
                }
            }

            // insert the item
            values[insertPosition] = value;
            weights[insertPosition] = weight;
            if (!isFull)
                ++size;
            return true;
        }

        public static bool InsertIntoDescending<V, W>(bool enforceUnique, ref byte size, byte maxSize, V value, W weight, Span<V> values, Span<W> weights)
            where V : IComparable<V>
            where W : unmanaged, INumber<W>, IMinMaxValue<W>
        {
            var isFull = size == maxSize;

            // check to see if it should be inserted
            if (isFull && weights[size - 1] >= weight)
                return false;

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
                for (var i = size - (isFull ? 2 : 1); i >= insertPosition; i--) {
                    values[i + 1] = values[i];
                    weights[i + 1] = weights[i];
                }
            }

            // insert the item
            values[insertPosition] = value;
            weights[insertPosition] = weight;
            if (!isFull)
                ++size;
            return true;
        }

        public static void RemoveAt<V, W>(byte index, Span<V> values, Span<W> weights)
        {
            values[(index+1)..].CopyTo(values[index..]);
            weights[(index+1)..].CopyTo(weights[index..]);
        }
    }
}
