using System;
using System.Collections.Generic;
using BrightData.LinearAlgebra;

namespace BrightData
{
    /// <summary>
    /// Distance metrics
    /// </summary>
    public enum DistanceMetric
    {
        /// <summary>
        /// Euclidean Distance
        /// </summary>
        Euclidean,

        /// <summary>
        /// Cosine Distance Metric
        /// </summary>
        Cosine,

        /// <summary>
        /// Manhattan Distance
        /// </summary>
        Manhattan,

        /// <summary>
        /// Means Square Error
        /// </summary>
        MeanSquared,

        /// <summary>
        /// Square Euclidean
        /// </summary>
        SquaredEuclidean
    }
}
