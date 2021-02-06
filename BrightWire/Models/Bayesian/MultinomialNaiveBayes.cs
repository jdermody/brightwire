using System.IO;
using BrightData;
using BrightWire.Bayesian;
using BrightWire.Helper;

namespace BrightWire.Models.Bayesian
{
    /// <summary>
    /// Multinomial naive bayes model
    /// </summary>
    public class MultinomialNaiveBayes : ISerializable
    {
        /// <summary>
        /// The conditional probability associated with a string index
        /// </summary>
        public class StringIndexProbability : ISerializable
        {
            /// <summary>
            /// The string index
            /// </summary>
            public uint StringIndex { get; set; }

            /// <summary>
            /// The conditional probability
            /// </summary>
            public double ConditionalProbability { get; set; }

            /// <inheritdoc />
            public void WriteTo(BinaryWriter writer) => ModelSerialisation.WriteTo(this, writer);

            /// <inheritdoc />
            public void Initialize(IBrightDataContext context, BinaryReader reader) => ModelSerialisation.ReadFrom(context, reader, this);
        }

        /// <summary>
        /// Classification data
        /// </summary>
        public class Class : ISerializable
        {
            /// <summary>
            /// The classification label
            /// </summary>
            public string Label { get; set; } = "";

            /// <summary>
            /// The classification's prior log probability
            /// </summary>
            public double Prior { get; set; }

            /// <summary>
            /// The classifications missing log probability
            /// </summary>
            public double MissingProbability { get; set; }

            /// <summary>
            /// The list of string indexes and their probability
            /// </summary>
            public StringIndexProbability[] Index { get; set; } = new StringIndexProbability[0];

            /// <inheritdoc />
            public void WriteTo(BinaryWriter writer) => ModelSerialisation.WriteTo(this, writer);

            /// <inheritdoc />
            public void Initialize(IBrightDataContext context, BinaryReader reader) => ModelSerialisation.ReadFrom(context, reader, this);
        }

        /// <summary>
        /// The list of possible classifications
        /// </summary>
        public Class[] ClassData { get; set; } = new Class[0];

        /// <summary>
        /// Creates a classifier from the model
        /// </summary>
        /// <returns></returns>
        public IIndexListClassifier CreateClassifier()
        {
            return new MultinomialNaiveBayesClassifier(this);
        }

        /// <inheritdoc />
        public void WriteTo(BinaryWriter writer) => ModelSerialisation.WriteTo(this, writer);

        /// <inheritdoc />
        public void Initialize(IBrightDataContext context, BinaryReader reader) => ModelSerialisation.ReadFrom(context, reader, this);
    }
}
