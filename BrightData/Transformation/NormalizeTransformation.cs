using System;
using BrightData.Helper;

namespace BrightData.Transformation
{
    public class NormalizeTransformation : INormalize
    {
        private readonly bool _divideByZero;

        public NormalizeTransformation(NormalizationType type, IMetaData analysedMetaData)
        {
            NormalizationType = type;

            double divide, subtract = 0;
            if (analysedMetaData.Get(Consts.HasBeenAnalysed, false) && analysedMetaData.Get(Consts.IsNumeric, false)) {
                if (type == NormalizationType.Euclidean)
                    divide = Convert.ToDouble(analysedMetaData.Get(Consts.L2Norm));
                else if (type == NormalizationType.Manhattan)
                    divide = Convert.ToDouble(analysedMetaData.Get(Consts.L1Norm));
                else if (type == NormalizationType.Standard) {
                    divide = Convert.ToDouble(analysedMetaData.Get(Consts.StdDev) ?? 1);
                    subtract = Convert.ToDouble(analysedMetaData.Get(Consts.Mean));
                }
                else if (type == NormalizationType.FeatureScale) {
                    var min = Convert.ToDouble(analysedMetaData.Get(Consts.Min));
                    var max = Convert.ToDouble(analysedMetaData.Get(Consts.Max));
                    divide = max - min;
                    subtract = min;
                }
                else
                    throw new NotImplementedException();
            }
            else
                throw new Exception("Metadata did not indicate numeric data that has been analyzed");

            Divide = divide;
            Subtract = subtract;
            _divideByZero = Math.Abs(Divide) <= FloatMath.AlmostZero;
        }

        public NormalizationType NormalizationType { get; }
        public double Divide { get; }
        public double Subtract { get; }

        public double Normalize(double val) => _divideByZero ? val : (val - Subtract) / Divide;
    }
}
