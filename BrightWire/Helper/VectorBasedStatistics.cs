using BrightData;
using BrightData.LinearAlegbra2;

namespace BrightWire.Helper
{
	/// <summary>
	/// Calculate vector based statistics
	/// </summary>
	internal class VectorBasedStatistics
	{
        public VectorBasedStatistics(LinearAlgebraProvider lap, uint size, float[]? mean, float[]? m2, uint count)
		{
			Size = size;
			Count = count;
			Mean = mean != null ? lap.CreateVector(mean) : lap.CreateVector(size, 0f);
			M2 = m2 != null ? lap.CreateVector(m2) : lap.CreateVector(size, 0f);
		}

		public uint Size { get; }
        public uint Count { get; private set; }
        public IVector Mean { get; }
        public IVector M2 { get; }

		public void Update(IVector data)
		{
			++Count;
            using var delta = data.Subtract(Mean);
            using var diff = delta.Clone();
            diff.Multiply(1f / Count);
            Mean.AddInPlace(diff);

            using var delta2 = data.Subtract(Mean);
            using var diff2 = delta.PointwiseMultiply(delta2);
            M2.AddInPlace(diff2);
        }

        public IVector GetVariance()
		{
			var ret = M2.Clone();
			ret.Multiply(1f / Count);
			return ret;
		}

		public IVector GetSampleVariance()
		{
			var ret = M2.Clone();
			ret.Multiply(1f / (Count-1));
			return ret;
		}
	}
}
