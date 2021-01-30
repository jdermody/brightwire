using System.IO;
using BrightData;
using BrightData.LinearAlgebra;
using BrightWire.ExecutionGraph;
using BrightWire.Linear;

namespace BrightWire.Models.Linear
{
    /// <summary>
    /// A linear regression model
    /// </summary>
    public class LinearRegression : ISerializable
    {
        /// <summary>
        /// The model parameters
        /// </summary>
        public Vector<float> Theta { get; set; }

        /// <summary>
        /// Creates a predictor from this model
        /// </summary>
        /// <param name="lap">The linear algebra provider</param>
        public ILinearRegressionPredictor CreatePredictor(ILinearAlgebraProvider lap)
        {
            return new RegressionPredictor(lap, lap.CreateVector(Theta.Segment));
        }

        /// <inheritdoc />
        public void WriteTo(BinaryWriter writer)
        {
            Theta.WriteTo(writer);
        }

        /// <inheritdoc />
        public void Initialize(IBrightDataContext context, BinaryReader reader)
        {
            Theta.Initialize(context, reader);
        }
    }
}
