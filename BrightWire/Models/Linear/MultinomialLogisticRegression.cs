using BrightData;
using BrightWire.Linear;

namespace BrightWire.Models
{
    /// <summary>
    /// Multinomial logistic regression model
    /// </summary>
    public class MultinomialLogisticRegression
    {
        /// <summary>
        /// The list of logistic regression models
        /// </summary>
        public LogisticRegression[] Model { get; set; }

        /// <summary>
        /// The associated classification labels
        /// </summary>
        public string[] Classification { get; set; }

        /// <summary>
        /// The columns used to build the dense input vectors
        /// </summary>
        public uint[] FeatureColumn { get; set; }

        /// <summary>
        /// Creates a classifier from this model
        /// </summary>
        /// <param name="lap">Linear algebra provider</param>
        /// <returns></returns>
        public ITableClassifier CreateClassifier(ILinearAlgebraProvider lap)
        {
            return new MultinomialLogisticRegressionClassifier(lap, this);
        }
    }
}
