using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.ExecutionGraph.Descriptor
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
        /// Xavier initialisation: http://andyljones.tumblr.com/post/110998971763/an-explanation-of-xavier-initialization
        /// </summary>
        Xavier,

        /// <summary>
        /// Identity matrix: https://arxiv.org/abs/1504.00941
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
