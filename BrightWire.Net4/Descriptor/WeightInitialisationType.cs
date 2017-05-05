using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Descriptor
{
    /// <summary>
    /// Layer weight initialisation
    /// </summary>
    public enum WeightInitialisationType
    {
        /// <summary>
        /// Gaussian distribution
        /// </summary>
        Gaussian,

        /// <summary>
        /// Xavier initialisation: 
        /// </summary>
        Xavier,

        /// <summary>
        /// 
        /// </summary>
        Identity,

        /// <summary>
        /// Identity matrix of 0.1
        /// </summary>
        Identity1,

        /// <summary>
        /// Identity matrix of 0.01
        /// </summary>
        Identity01,

        /// <summary>
        /// Identity matrix of 0.001
        /// </summary>
        Identity001
    }
}
