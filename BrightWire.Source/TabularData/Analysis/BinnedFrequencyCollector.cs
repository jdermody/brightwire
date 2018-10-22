using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightWire.LinearAlgebra.Helper;

namespace BrightWire.TabularData.Analysis
{
	class BinnedFrequencyCollector : IRowProcessor, IDataTableColumnFrequency
	{
		readonly double _min, _max, _step;
		readonly ulong[] _counts;

		public BinnedFrequencyCollector(int columnsIndex, double min, double max, int numBins)
		{
			ColumnIndex = columnsIndex;
			_step = (max - min) / numBins + BoundMath.ZERO_LIKE;
			_min = min;
			_max = max;

			// allocate extra slots for anything outside the range
			_counts = new ulong[numBins + 2];
		}

		public int ColumnIndex { get; }
		public IReadOnlyList<(string Category, ulong Count)> CategoricalFrequency => null;

		public bool Process(IRow row)
		{
			var val = row.GetField<double>(ColumnIndex);
			if (val < _min)
				_counts[0]++;
			else if (val > _max)
				_counts[_counts.Length - 1]++;
			else if(Math.Abs(val - _max) < BoundMath.ZERO_LIKE)
				_counts[_counts.Length - 2]++;
			else {
				var val2 = val - _min;
				var bin = Convert.ToInt32(val2 / _step);
				_counts[bin + 1]++;
			}

			return true;
		}

		public IReadOnlyList<(double Start, double End, ulong Count)> ContinuousFrequency
		{
			get { return _counts.Select((c, i) => {
				if (i == 0)
					return (double.NegativeInfinity, _min, c);
				if (i == _counts.Length - 1)
					return (_max, double.PositiveInfinity, c);
				return (_min + (i - 1) * _step, _min + i * _step, c);
			}).ToList(); }
		}
	}
}
