using BrightWire.Linear;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Models
{
    /// <summary>
    /// A linear regression model
    /// </summary>
    [ProtoContract]
    public class LinearRegression
    {
        /// <summary>
        /// The model parameters
        /// </summary>
        [ProtoMember(1)]
        public FloatArray Theta { get; set; }

        /// <summary>
        /// Creates a predictor from this model
        /// </summary>
        /// <param name="lap">The linear algebra provider</param>
        public ILinearRegressionPredictor CreatePredictor(ILinearAlgebraProvider lap)
        {
            return new RegressionPredictor(lap, lap.Create(Theta.Data));
        }
    }
}
