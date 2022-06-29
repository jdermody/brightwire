using System;
using System.IO;
using BrightData;
using BrightWire.Bayesian;
using BrightWire.Helper;

namespace BrightWire.Models.Bayesian
{
    /// <summary>
    /// A bernoulli naive bayes model
    /// </summary>
    public class BernoulliNaiveBayes : ISerializable
    {
        /// <summary>
        /// The probabilities associated with a string index
        /// </summary>
        public class StringIndexProbability : ISerializable
        {
            /// <summary>
            /// The string index
            /// </summary>
            public uint StringIndex { get; set; }

            /// <summary>
            /// The log of the conditional probability
            /// </summary>
            public double ConditionalProbability { get; set; }

            /// <summary>
            /// The log of the inverse conditional probability
            /// </summary>
            public double InverseProbability { get; set; }

            /// <inheritdoc />
            public void WriteTo(BinaryWriter writer) => ModelSerialisation.WriteTo(this, writer);

            /// <inheritdoc />
            public void Initialize(BrightDataContext context, BinaryReader reader) => ModelSerialisation.ReadFrom(context, reader, this);
        }

        /// <summary>
        /// A classification
        /// </summary>
        public class Class : ISerializable
        {
            /// <summary>
            /// The classification label
            /// </summary>
            public string Label { get; set; } = "";

            /// <summary>
            /// The log of the prior probablilty for this classification
            /// </summary>
            public double Prior { get; set; }

            /// <summary>
            /// The log of the missing probability
            /// </summary>
            public double MissingProbability { get; set; }

            /// <summary>
            /// The list of probabilities for each string index
            /// </summary>
            public StringIndexProbability[] Index { get; set; } = Array.Empty<StringIndexProbability>();

            /// <summary>
            /// The log of the inverse missing probability
            /// </summary>
            public double InverseMissingProbability { get; set; }

            /// <inheritdoc />
            public void WriteTo(BinaryWriter writer) => ModelSerialisation.WriteTo(this, writer);

            /// <inheritdoc />
            public void Initialize(BrightDataContext context, BinaryReader reader) => ModelSerialisation.ReadFrom(context, reader, this);
        }

        /// <summary>
        /// Classification data
        /// </summary>
        public Class[] ClassData { get; set; } = Array.Empty<Class>();

        /// <summary>
        /// The list of string indexes that were in the training set
        /// </summary>
        public uint[] Vocabulary { get; set; } = Array.Empty<uint>();

        /// <summary>
        /// Creates a classifier from this model
        /// </summary>
        /// <returns></returns>
        public IIndexListClassifier CreateClassifier()
        {
            return new BernoulliNaiveBayesClassifier(this);
        }

        /// <inheritdoc />
        public void WriteTo(BinaryWriter writer) => ModelSerialisation.WriteTo(this, writer);

        /// <inheritdoc />
        public void Initialize(BrightDataContext context, BinaryReader reader) => ModelSerialisation.ReadFrom(context, reader, this);
    }
}
