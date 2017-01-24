using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Models.Output
{
    /// <summary>
    /// A matrix that has been split into two sub matrices
    /// </summary>
    public class RowSplit
    {
        /// <summary>
        /// The left part
        /// </summary>
        public IMatrix Left { get; private set; }

        /// <summary>
        /// The right part
        /// </summary>
        public IMatrix Right { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        public RowSplit(IMatrix left, IMatrix right)
        {
            Left = left;
            Right = right;
        }
    }
}
