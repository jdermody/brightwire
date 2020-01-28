using System;
using System.Collections.Generic;
using System.Linq;
using BrightData;
using BrightTable;

namespace BrightML
{
    class Graph
    {
        readonly List<BrightWire.INode> _inputNodes = new List<BrightWire.INode>();

        public Graph(IRowOrientedDataTable inputTable)
        {
            var columns = inputTable.AllColumns();

            // find the classification target
            var classificationTarget = columns.SingleOrDefault(c => c.IsTarget());
            if (classificationTarget == null)
                throw new ArgumentException("Table does not contain a target classification column");

            if (classificationTarget.SingleType.IsNumeric()) {
                
            }
        }
    }
}
