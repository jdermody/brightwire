using BrightWire.Bayesian;
using ProtoBuf;

namespace BrightWire.Models.Bayesian
{
    /// <summary>
    /// A bernoulli naive bayes model
    /// </summary>
    [ProtoContract]
    public class BernoulliNaiveBayes
    {
        /// <summary>
        /// The probabilities associated with a string index
        /// </summary>
        [ProtoContract]
        public class StringIndexProbability
        {
            /// <summary>
            /// The string index
            /// </summary>
            [ProtoMember(1)]
            public uint StringIndex { get; set; }

            /// <summary>
            /// The log of the conditional probability
            /// </summary>
            [ProtoMember(2)]
            public double ConditionalProbability { get; set; }

            /// <summary>
            /// The log of the inverse conditional probability
            /// </summary>
            [ProtoMember(3)]
            public double InverseProbability { get; set; }
        }

        /// <summary>
        /// A classification
        /// </summary>
        [ProtoContract]
        public class Class
        {
            /// <summary>
            /// The classification label
            /// </summary>
            [ProtoMember(1)]
            public string Label { get; set; }

            /// <summary>
            /// The log of the prior probablilty for this classification
            /// </summary>
            [ProtoMember(2)]
            public double Prior { get; set; }

            /// <summary>
            /// The log of the missing probability
            /// </summary>
            [ProtoMember(3)]
            public double MissingProbability { get; set; }

            /// <summary>
            /// The list of probabilities for each string index
            /// </summary>
            [ProtoMember(4)]
            public StringIndexProbability[] Index { get; set; }

            /// <summary>
            /// The log of the inverse missing probability
            /// </summary>
            [ProtoMember(5)]
            public double InverseMissingProbability { get; set; }
        }

        /// <summary>
        /// Classification data
        /// </summary>
        [ProtoMember(1)]
        public Class[] ClassData { get; set; }

        /// <summary>
        /// The list of string indexes that were in the training set
        /// </summary>
        [ProtoMember(2)]
        public uint[] Vocabulary { get; set; }

        /// <summary>
        /// Creates a classifier from this model
        /// </summary>
        /// <returns></returns>
        public IIndexListClassifier CreateClassifier()
        {
            return new BernoulliNaiveBayesClassifier(this);
        }
    }
}
