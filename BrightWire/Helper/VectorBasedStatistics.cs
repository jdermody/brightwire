using System;
using System.Collections.Generic;
using System.Text;
using BrightData;

namespace BrightWire.Helper
{
	public class VectorBasedStatistics
	{
		readonly IFloatVector _mean, _m2;
		readonly uint _size;
		uint _count;

		public VectorBasedStatistics(ILinearAlgebraProvider lap, uint size, float[] mean, float[] m2, uint count)
		{
			_size = size;
			_count = count;
			_mean = mean != null ? lap.CreateVector(mean) : lap.CreateVector(size, 0f);
			_m2 = m2 != null ? lap.CreateVector(m2) : lap.CreateVector(size, 0f);
		}

		public uint Size => _size;
		public uint Count => _count;

		public void Update(IFloatVector data)
		{
			++_count;
			using (var delta = data.Subtract(_mean))
			using (var diff = delta.Clone()) {
				diff.Multiply(1f / _count);
				_mean.AddInPlace(diff);

				using (var delta2 = data.Subtract(_mean))
				using(var diff2 = delta.PointwiseMultiply(delta2)) {
					_m2.AddInPlace(diff2);
				}
			}
		}

		public IFloatVector Mean => _mean;
		public IFloatVector M2 => _m2;

		public IFloatVector GetVariance()
		{
			var ret = _m2.Clone();
			ret.Multiply(1f / _count);
			return ret;
		}

		public IFloatVector GetSampleVariance()
		{
			var ret = _m2.Clone();
			ret.Multiply(1f / (_count-1));
			return ret;
		}
	}
}
