using System;
using System.IO;
using BrightData;
using BrightWire.Helper;
using BrightWire.Linear;

namespace BrightWire.Models.Linear
{
    /// <summary>
    /// Multinomial logistic regression model
    /// </summary>
    public class MultinomialLogisticRegression : ISerializable
    {
        /// <summary>
        /// The list of logistic regression models
        /// </summary>
        public LogisticRegression[] Model { get; set; } = Array.Empty<LogisticRegression>();

        /// <summary>
        /// The associated classification labels
        /// </summary>
        public string[] Classification { get; set; } = Array.Empty<string>();

        /// <summary>
        /// The columns used to build the dense input vectors
        /// </summary>
        public uint[] FeatureColumn { get; set; } = Array.Empty<uint>();

        /// <summary>
        /// Creates a classifier from this model
        /// </summary>
        /// <param name="lap">Linear algebra provider</param>
        /// <returns></returns>
        public ITableClassifier CreateClassifier(ILinearAlgebraProvider lap)
        {
            return new MultinomialLogisticRegressionClassifier(lap, this);
        }

        /// <inheritdoc />
        public void WriteTo(BinaryWriter writer) => ModelSerialisation.WriteTo(this, writer);

        /// <inheritdoc />
        public void Initialize(IBrightDataContext context, BinaryReader reader) => ModelSerialisation.ReadFrom(context, reader, this);
    }
}
