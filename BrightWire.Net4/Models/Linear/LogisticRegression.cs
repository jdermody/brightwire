using BrightWire.Linear;
using ProtoBuf;

namespace BrightWire.Models
{
    /// <summary>
    /// A logistic regression model
    /// </summary>
    [ProtoContract]
    public class LogisticRegression
    {
        /// <summary>
        /// The model parameters
        /// </summary>
        [ProtoMember(1)]
        public FloatVector Theta { get; set; }

        /// <summary>
        /// Creates a classifier from this model
        /// </summary>
        /// <param name="lap">Linear algebra provider</param>
        public ILogisticRegressionClassifier CreatePredictor(ILinearAlgebraProvider lap)
        {
            return new LogisticRegressionPredictor(lap, lap.CreateVector(Theta.Data));
        }
    }
}
