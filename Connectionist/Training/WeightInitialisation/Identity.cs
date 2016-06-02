using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icbld.BrightWire.Connectionist.Training.WeightInitialisation
{
    public class Identity : IWeightInitialisation
    {
        readonly float _scale;

        public Identity(float scale = 1f)
        {
            _scale = scale;
        }

        public float GetBias()
        {
            return 0f;
        }

        public float GetWeight(int inputSize, int outputSize, int i, int j)
        {
            return (i == j) ? _scale : 0f;
        }
    }
}
