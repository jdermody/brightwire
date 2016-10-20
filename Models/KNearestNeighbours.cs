using BrightWire.InstanceBased;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Models
{
    [ProtoContract]
    public class KNearestNeighbours
    {
        [ProtoMember(1)]
        public FloatArray[] Instance { get; set; }

        [ProtoMember(2)]
        public string[] Classification { get; set; }

        [ProtoMember(3)]
        public int[] FeatureColumn { get; set; }

        public IRowClassifier CreateClassifier(ILinearAlgebraProvider lap, int k, DistanceMetric distanceMetric = DistanceMetric.Euclidean)
        {
            return new KNNClassifier(lap, this, k, distanceMetric);
        }
    }
}
