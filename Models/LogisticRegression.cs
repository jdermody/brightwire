using BrightWire.Linear;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Models
{
    [ProtoContract]
    public class LogisticRegression
    {
        [ProtoMember(1)]
        public FloatArray Theta { get; set; }

        public ILogisticRegressionClassifier CreatePredictor(ILinearAlgebraProvider lap)
        {
            return new LogisticRegressionPredictor(lap, lap.Create(Theta.Data));
        }
    }
}
