using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using BrightWire.Helper;
using MathNet.Numerics.Distributions;

namespace BrightWire.Ensemble
{
    internal class AdaBoostBuilder
    {
        readonly float[] _rowWeight;
        readonly List<float> _classifierWeight = new List<float>();

        public AdaBoostBuilder(int rowCount)
        {
            var weight = 1f / rowCount;
            _rowWeight = new float[rowCount];
            for (var i = 0; i < rowCount; i++)
                _rowWeight[i] = weight;
        }

        public float AddClassifierResults(IReadOnlyList<bool> rowClassificationWasCorrect)
        {
            int mistakes = 0, total = 0;
            foreach(var wasCorrect in rowClassificationWasCorrect) {
                if (!wasCorrect)
                    ++mistakes;
                ++total;
            }
            if (total != _rowWeight.Length)
                throw new ArgumentException();

            var weightedError = (float)mistakes / total;
            var classifierWeight = BoundMath.Log((1 - weightedError) / weightedError) * 0.5f;

            float weightTotal = 0;
            for(var i = 0; i < _rowWeight.Length; i++) {
                var wasCorrect = rowClassificationWasCorrect[i];
                float delta;
                if (wasCorrect)
                    delta = BoundMath.Exp(-classifierWeight);
                else
                    delta = BoundMath.Exp(classifierWeight);

                weightTotal += (_rowWeight[i] = _rowWeight[i] * delta);
            }
            //for (var i = 0; i < _rowWeight.Length; i++)
            //    _rowWeight[i] /= weightTotal;

            _classifierWeight.Add(classifierWeight);
            return classifierWeight;
        }

        public IReadOnlyList<float> RowWeight { get { return _rowWeight; } }
        public IReadOnlyList<float> ClassifierWeight { get { return _classifierWeight; } }

        public IReadOnlyList<int> GetNextSamples()
        {
            var distribution = new Categorical(_rowWeight.Select(d => Convert.ToDouble(d)).ToArray());
            var ret = new List<int>();
            for (int i = 0, len = _rowWeight.Length; i < len; i++)
                ret.Add(distribution.Sample());
            return ret.OrderBy(v => v).ToList();
        }
    }
}
