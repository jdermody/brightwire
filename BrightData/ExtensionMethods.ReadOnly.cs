using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BrightData
{
    public partial class ExtensionMethods
    {
        /// <summary>
        /// Converts the vector to a sparse format (only non zero entries are preserved)
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static WeightedIndexList ToSparse(this IReadOnlyVector vector) => vector.ReadOnlySegment.ToSparse();
    }
}
