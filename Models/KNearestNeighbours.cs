using BrightWire.InstanceBased;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Models
{
    /// <summary>
    /// K Nearest Neighbour Model
    /// </summary>
    [ProtoContract]
    public class KNearestNeighbours
    {
        /// <summary>
        /// The list of vectors to match against
        /// </summary>
        [ProtoMember(1)]
        public FloatArray[] Instance { get; set; }

        /// <summary>
        /// The corresponding list of classifications
        /// </summary>
        [ProtoMember(2)]
        public string[] Classification { get; set; }

        /// <summary>
        /// The vector indexes to use to encode a data table row as a vector
        /// </summary>
        [ProtoMember(3)]
        public int[] FeatureColumn { get; set; }

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
