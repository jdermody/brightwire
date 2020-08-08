using BrightWire.InstanceBased;

namespace BrightWire.Models.InstanceBased
{
    /// <summary>
    /// K Nearest Neighbour Model
    /// </summary>
    public class KNearestNeighbours
    {
        /// <summary>
        /// The list of vectors to match against
        /// </summary>
        public FloatVector[] Instance { get; set; }

        /// <summary>
        /// The corresponding list of classifications
        /// </summary>
        public string[] Classification { get; set; }

        /// <summary>
        /// The vector indexes to use to encode a data table row as a vector
        /// </summary>
        public uint[] DataColumns { get; set; }

        /// <summary>
        /// The vector indexes to use to encode the other column(s) as a classification target
        /// </summary>
        public uint[] OtherColumns { get; set; }

        /// <summary>
        /// Creates a classifier from this model
        /// </summary>
        /// <param name="lap">The linear algebra provider</param>
        /// <param name="k">The number of instances to consider</param>
        /// <param name="distanceMetric">The distance metric to compare each row with each instance</param>
        public IRowClassifier CreateClassifier(ILinearAlgebraProvider lap, int k, DistanceMetric distanceMetric = DistanceMetric.Euclidean)
        {
            return new KNNClassifier(lap, this, k, distanceMetric);
        }
    }
}
