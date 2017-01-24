using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Models.Output
{
    /// <summary>
    /// A matrix that has been split into two sub matrices
    /// </summary>
    public class ColumnSplit
    {
        /// <summary>
        /// The top most sub matrix
        /// </summary>
        public IMatrix Top { get; private set; }

        /// <summary>
        /// The bottom most sub matrix
        /// </summary>
        public IMatrix Bottom { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="top"></param>
        /// <param name="bottom"></param>
        public ColumnSplit(IMatrix top, IMatrix bottom)
        {
            Top = top;
            Bottom = bottom;
        }
    }
}
