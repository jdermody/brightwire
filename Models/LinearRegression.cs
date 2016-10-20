using BrightWire.Linear;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Models
{
    [ProtoContract]
    public class LinearRegression
    {
        [ProtoMember(1)]
        public FloatArray Theta { get; set; }

        public ILinearRegressionPredictor CreatePredictor(ILinearAlgebraProvider lap)
        {
            return new RegressionPredictor(lap, lap.Create(Theta.Data));
        }
    }
}
