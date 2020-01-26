using System;
using System.Collections.Generic;
using System.Text;
using BrightData;

namespace BrightWire.Models
{
    public class LogisticRegression : IModel
    {
        public Vector<float> Theta { get; }

        public LogisticRegression(Vector<float> theta)
        {
            Theta = theta;
        }

        public void Dispose()
        {
            Theta.Dispose();
        }
    }
}
