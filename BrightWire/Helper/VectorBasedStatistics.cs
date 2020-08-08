using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Helper
{
	public class VectorBasedStatistics
	{
		readonly IVector _mean, _m2;
		readonly int _size;
		int _count;

		public VectorBasedStatistics(ILinearAlgebraProvider lap, int size, float[] mean, float[] m2, int count)
		{
			_size = size;
			_count = count;
			_mean = mean != null ? lap.CreateVector(mean) : lap.CreateVector(size, 0f);
			_m2 = m2 != null ? lap.CreateVector(m2) : lap.CreateVector(size, 0f);
		}

		public int Size => _size;
		public int Count => _count;

		public void Update(IVector data)
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

		public IVector Mean => _mean;
		public IVector M2 => _m2;

		public IVector GetVariance()
		{
			var ret = _m2.Clone();
			ret.Multiply(1f / _count);
			return ret;
		}

		public IVector GetSampleVariance()
		{
			var ret = _m2.Clone();
			ret.Multiply(1f / (_count-1));
			return ret;
		}
	}
}
