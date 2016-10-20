using BrightWire.Linear;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Models
{
    [ProtoContract]
    public class MultinomialLogisticRegression
    {
        [ProtoMember(1)]
        public LogisticRegression[] Model { get; set; }

        [ProtoMember(2)]
        public string[] Classification { get; set; }

        [ProtoMember(3)]
        public int[] FeatureColumn { get; set; }

        public IRowClassifier CreateClassifier(ILinearAlgebraProvider lap)
        {
            return new MultinomialLogisticRegressionClassifier(lap, this);
        }
    }
}
