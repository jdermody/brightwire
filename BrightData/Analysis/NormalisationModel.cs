using System;
using BrightData.Helper;
using BrightData.Types;

namespace BrightData.Analysis
{
    /// <summary>
    /// Normalization parameters
    /// </summary>
    public class NormalisationModel : INormalize
    {
        readonly bool _divideByZero;

        /// <summary>
        /// Creates a new set of parameters based on supplied the numeric analysis
        /// </summary>
        /// <param name="type">Type of normalization</param>
        /// <param name="analysedMetaData">Numeric analysis</param>
        public NormalisationModel(NormalizationType type, MetaData analysedMetaData)
        {
            NormalizationType = type;

            double divide, subtract = 0;
            if (analysedMetaData.Get(Consts.HasBeenAnalysed, false) && analysedMetaData.Get(Consts.IsNumeric, false))
            {
                if (type == NormalizationType.Euclidean)
                    divide = Convert.ToDouble(analysedMetaData.Get(Consts.L2Norm));
                else if (type == NormalizationType.Manhattan)
                    divide = Convert.ToDouble(analysedMetaData.Get(Consts.L1Norm));
                else if (type == NormalizationType.Standard)
                {
                    divide = Convert.ToDouble(analysedMetaData.Get(Consts.SampleStdDev) ?? 1);
                    subtract = Convert.ToDouble(analysedMetaData.Get(Consts.Mean));
                }
                else if (type == NormalizationType.FeatureScale)
                {
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
            _divideByZero = Math.Abs(Divide) <= Math<double>.AlmostZero;
        }

        internal NormalisationModel(MetaData metaData)
        {
            NormalizationType = (NormalizationType)metaData.Get<byte>(Consts.NormalizationType, 0);
            Subtract = metaData.Get(Consts.NormalizationP1, 0D);
            Divide = metaData.Get(Consts.NormalizationP2, 0D);
        }

        /// <summary>
        /// Writes the normalization parameters to a meta data store
        /// </summary>
        /// <param name="metaData"></param>
        public void WriteTo(MetaData metaData)
        {
            metaData.Set(Consts.NormalizationType, (byte)NormalizationType);
            metaData.Set(Consts.NormalizationP1, Subtract);
            metaData.Set(Consts.NormalizationP2, Divide);
        }

        /// <summary>
        /// Type of normalization
        /// </summary>
        public NormalizationType NormalizationType { get; }

        /// <summary>
        /// Value that will be divided (after subtraction)
        /// </summary>
        public double Divide { get; }

        /// <summary>
        /// Value that will be subtracted
        /// </summary>
        public double Subtract { get; }

        /// <summary>
        /// Normalizes a value with the parameters
        /// </summary>
        /// <param name="val">Value to normalize</param>
        /// <returns>Normalized result</returns>
        public double Normalize(double val) => (val - Subtract) / (_divideByZero ? 1 : Divide);

        /// <summary>
        /// Reverse a normalized value
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public double ReverseNormalize(double val) => (_divideByZero ? val : val * Divide) + Subtract;
    }
}
