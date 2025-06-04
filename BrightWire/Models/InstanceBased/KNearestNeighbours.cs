using System.IO;
using BrightData;
using BrightData.LinearAlgebra;
using BrightWire.Helper;
using BrightWire.InstanceBased;

namespace BrightWire.Models.InstanceBased
{
    /// <summary>
    /// K Nearest Neighbour Model
    /// </summary>
    public class KNearestNeighbours : IAmSerializable
    {
        /// <summary>
        /// The list of vectors to match against
        /// </summary>
        public float[][] Instance { get; set; } = [];

        /// <summary>
        /// The corresponding list of classifications
        /// </summary>
        public string[] Classification { get; set; } = [];

        /// <summary>
        /// The vector indexes to use to encode a data table row as a vector
        /// </summary>
        public uint[] DataColumns { get; set; } = [];

        /// <summary>
        /// The vector indexes to use to encode the other column(s) as a classification target
        /// </summary>
        public uint TargetColumn { get; set; }

        /// <summary>
        /// Creates a classifier from this model
        /// </summary>
        /// <param name="lap">The linear algebra provider</param>
        /// <param name="k">The number of instances to consider</param>
        /// <param name="distanceMetric">The distance metric to compare each row with each instance</param>
        public IRowClassifier CreateClassifier(LinearAlgebraProvider<float> lap, uint k, DistanceMetric distanceMetric = DistanceMetric.Euclidean)
        {
            return new KnnClassifier(lap, this, k, distanceMetric);
        }

        /// <inheritdoc />
        public void WriteTo(BinaryWriter writer) => ModelSerialisation.WriteTo(this, writer);

        /// <inheritdoc />
        public void Initialize(BrightDataContext context, BinaryReader reader) => ModelSerialisation.ReadFrom(context, reader, this);
    }
}
