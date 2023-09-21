using System;
using System.IO;
using BrightData.Helper;

namespace BrightData
{
    public partial class ExtensionMethods
    {
        /// <summary>
        /// Copies all values from a tensor segment into a float buffer
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="segment"></param>
        public static void CopyFrom(this ICompositeBuffer<float> buffer, INumericSegment<float> segment)
        {
            for(uint i = 0, len = segment.Size; i < len; i++)
                buffer.Add(segment[i]);
        }

        /// <summary>
        /// Copies all values from a span into a float buffer
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="span"></param>
        public static void CopyFrom(this ICompositeBuffer<float> buffer, ReadOnlySpan<float> span)
        {
            for(int i = 0, len = span.Length; i < len; i++)
                buffer.Add(span[i]);
        }
    }
}
