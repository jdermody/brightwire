using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightData
{
    public partial class ExtensionMethods
    {
        /// <summary>
        /// Returns the index with the minimum value from this tensor segment
        /// </summary>
        /// <param name="tensorSegment"></param>
        /// <returns></returns>
        public static uint GetMinimumIndex(this IHaveTensorSegment tensorSegment) => tensorSegment.Segment.GetMinAndMaxValues().MinIndex;

        /// <summary>
        /// Returns the index with the maximum value from this tensor segment
        /// </summary>
        /// <param name="tensorSegment"></param>
        /// <returns></returns>
        public static uint GetMaximumIndex(this IHaveTensorSegment tensorSegment) => tensorSegment.Segment.GetMinAndMaxValues().MaxIndex;
    }
}
