using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightData
{
    public partial class ExtensionMethods
    {
        public static uint GetMinimumIndex(this IHaveTensorSegment tensorSegment) => tensorSegment.Segment.GetMinAndMaxValues().MinIndex;
        public static uint GetMaximumIndex(this IHaveTensorSegment tensorSegment) => tensorSegment.Segment.GetMinAndMaxValues().MaxIndex;
    }
}
