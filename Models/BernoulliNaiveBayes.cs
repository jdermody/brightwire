using BrightWire.Bayesian;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.Models
{
    [ProtoContract]
    public class BernoulliNaiveBayes
    {
        [ProtoContract]
        public class StringIndexProbability
        {
            [ProtoMember(1)]
            public uint StringIndex { get; set; }

            [ProtoMember(2)]
            public double ConditionalProbability { get; set; }

            [ProtoMember(3)]
            public double InverseProbability { get; set; }
        }

        [ProtoContract]
        public class Class
        {
            [ProtoMember(1)]
            public string Label { get; set; }

            [ProtoMember(2)]
            public double Prior { get; set; }

            [ProtoMember(3)]
            public double MissingProbability { get; set; }

            [ProtoMember(4)]
            public StringIndexProbability[] Index { get; set; }

            [ProtoMember(5)]
            public double InverseMissingProbability { get; set; }
        }

        [ProtoMember(1)]
        public Class[] ClassData { get; set; }

        [ProtoMember(2)]
        public uint[] Vocabulary { get; set; }

        public IIndexBasedClassifier CreateClassifier()
        {
            return new BernoulliNaiveBayesClassifier(this);
        }
    }
}
