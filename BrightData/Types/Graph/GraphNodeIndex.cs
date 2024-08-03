using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightData.Types.Graph
{
    /// <summary>
    /// Index only graph node value
    /// </summary>
    /// <param name="Index"></param>
    public readonly record struct GraphNodeIndex(uint Index) : IHaveSingleIndex, IComparable<GraphNodeIndex>
    {
        /// <inheritdoc />
        public int CompareTo(GraphNodeIndex other) => Index.CompareTo(other.Index);

        /// <inheritdoc />
        public override string ToString() => Index.ToString();
    }
}
