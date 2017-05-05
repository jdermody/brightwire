using BrightWire.Bayesian;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Models.Bayesian
{
    /// <summary>
    /// Multinomial naive bayes model
    /// </summary>
    [ProtoContract]
    public class MultinomialNaiveBayes
    {
        /// <summary>
        /// The conditional probability associated with a string index
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
            /// The conditional probability
            /// </summary>
            [ProtoMember(2)]
            public double ConditionalProbability { get; set; }
        }

        /// <summary>
        /// Classification data
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
            /// The classification's prior log probability
            /// </summary>
            [ProtoMember(2)]
            public double Prior { get; set; }

            /// <summary>
            /// The classifications missing log probability
            /// </summary>
            [ProtoMember(3)]
            public double MissingProbability { get; set; }

            /// <summary>
            /// The list of string indexes and their probability
            /// </summary>
            [ProtoMember(4)]
            public StringIndexProbability[] Index { get; set; }
        }

        /// <summary>
        /// The list of possible classifications
        /// </summary>
        [ProtoMember(1)]
        public Class[] ClassData { get; set; }

        /// <summary>
        /// Creates a classifier from the model
        /// </summary>
        /// <returns></returns>
        public IRowClassifier CreateClassifier()
        {
            return new MultinomialNaiveBayesClassifier(this);
        }
    }
}
