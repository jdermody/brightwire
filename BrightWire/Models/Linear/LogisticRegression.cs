using BrightData;
using BrightWire.Linear;

namespace BrightWire.Models
{
    /// <summary>
    /// A logistic regression model
    /// </summary>
    public class LogisticRegression
    {
        /// <summary>
        /// The model parameters
        /// </summary>
        public Vector<float> Theta { get; set; }

        /// <summary>
        /// Creates a classifier from this model
        /// </summary>
        /// <param name="lap">Linear algebra provider</param>
        public ILogisticRegressionClassifier CreatePredictor(ILinearAlgebraProvider lap)
        {
            return new LogisticRegressionPredictor(lap, lap.CreateVector(Theta.Segment));
        }
    }
}
