using System.IO;
using BrightData;
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
        public Vector<float> Theta { get; set; }

        /// <summary>
        /// Creates a classifier from this model
        /// </summary>
        /// <param name="lap">Linear algebra provider</param>
        public ILogisticRegressionClassifier CreateClassifier(ILinearAlgebraProvider lap)
        {
            return new LogisticRegressionPredictor(lap, lap.CreateVector(Theta.Segment));
        }

        public void WriteTo(BinaryWriter writer)
        {
            Theta.WriteTo(writer);
        }

        public void Initialize(IBrightDataContext context, BinaryReader reader)
        {
            Theta.Initialize(context, reader);
        }
    }
}
