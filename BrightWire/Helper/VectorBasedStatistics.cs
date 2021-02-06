using BrightData;

namespace BrightWire.Helper
{
	/// <summary>
	/// Calculate vector based statistics
	/// </summary>
	public class VectorBasedStatistics
	{
        public VectorBasedStatistics(ILinearAlgebraProvider lap, uint size, float[]? mean, float[]? m2, uint count)
		{
			Size = size;
			Count = count;
			Mean = mean != null ? lap.CreateVector(mean) : lap.CreateVector(size, 0f);
			M2 = m2 != null ? lap.CreateVector(m2) : lap.CreateVector(size, 0f);
		}

		public uint Size { get; }
        public uint Count { get; private set; }
        public IFloatVector Mean { get; }
        public IFloatVector M2 { get; }

		public void Update(IFloatVector data)
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

        public IFloatVector GetVariance()
		{
			var ret = M2.Clone();
			ret.Multiply(1f / Count);
			return ret;
		}

		public IFloatVector GetSampleVariance()
		{
			var ret = M2.Clone();
			ret.Multiply(1f / (Count-1));
			return ret;
		}
	}
}
