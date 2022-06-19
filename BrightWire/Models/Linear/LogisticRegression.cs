using System.IO;
using BrightData;
using BrightData.LinearAlegbra2;
using BrightData.LinearAlgebra;
using BrightWire.Linear;

namespace BrightWire.Models.Linear
{
    /// <summary>
    /// A logistic regression model
    /// </summary>
    public class LogisticRegression : ISerializable
    {
        /// <summary>
        /// The model parameters
        /// </summary>
        public Vector<float>? Theta { get; set; }

        /// <summary>
        /// Creates a classifier from this model
        /// </summary>
        /// <param name="lap">Linear algebra provider</param>
        public ILogisticRegressionClassifier CreateClassifier(LinearAlgebraProvider lap)
        {
            return new LogisticRegressionPredictor(lap, lap.CreateVector(Theta!));
        }

        /// <inheritdoc />
        public void WriteTo(BinaryWriter writer)
        {
            Theta!.WriteTo(writer);
        }

        /// <inheritdoc />
        public void Initialize(IBrightDataContext context, BinaryReader reader)
        {
            Theta = context.CreateVector<float>(reader);
        }
    }
}
