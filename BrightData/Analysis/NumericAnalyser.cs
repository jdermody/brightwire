using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using BrightData.Types;

namespace BrightData.Analysis
{
    /// <summary>
    /// Numeric analysis
    /// </summary>
    internal class NumericAnalyser<T>(uint writeCount = Consts.MaxWriteCount) : OnlineStandardDeviationAnalysis<T>, IDataAnalyser<T>, INumericAnalysis<T>
        where T: unmanaged, INumber<T>, IMinMaxValue<T>, IBinaryFloatingPointIeee754<T>, IConvertible
    {
        readonly SortedDictionary<T, ulong> _distinct = [];
		T _mode, _l2;
		ulong _highestCount;

        public override void Add(T value)
		{
			base.Add(value);

			// find the min and the max
			if (value < Min)
				Min = value;
			if (value > Max)
				Max = value;

			// add to distinct values
            if (_distinct.TryGetValue(value, out var count))
                _distinct[value] = ++count;
            else
                _distinct.Add(value, count = 1);

            if (count > _highestCount) {
                _highestCount = count;
                _mode = value;
            }

			// calculate norms
			L1Norm += T.Abs(value);
			_l2 += value * value;
		}

        public void Append(ReadOnlySpan<T> span)
        {
            foreach(var item in span)
                Add(item);
        }

		public T L1Norm { get; private set; }
        public T L2Norm => T.Sqrt(_l2);
	    public T Min { get; private set; } = T.MaxValue;
        public T Max { get; private set; } = T.MinValue;
        public uint NumDistinct => (uint)_distinct.Count;

        public T? Median
        {
            get
            {
                T? ret = null;
                if (_distinct.Count > 0) {
                    if (Count % 2 == 1)
                        return SortedValues.Skip((int) (Count / 2)).First();
                    return CalculateAverage(SortedValues.Skip((int)(Count / 2 - 1)).Take(2));
                }
                return ret;
            }
        }

        static T CalculateAverage(IEnumerable<T> values)
        {
            var ret = T.Zero;
            var count = 0;
            foreach (var value in values) {
                ret += value;
                ++count;
            }
            return ret / T.CreateTruncating(count);
        }

        IEnumerable<T> SortedValues
        {
            get
            {
                foreach (var item in _distinct) {
                    for (ulong i = 0; i < item.Value; i++)
                        yield return item.Key;
                }
            }
        }

        public T? Mode
        {
            get
            {
                if (_distinct.Count > 0)
                    return _mode;
                return null;
            }
        }

        public void AddObject(object obj)
        {
            var str = obj.ToString();
            if (str != null) {
                var val = T.Parse(str, null);
                Add(val);
            }
        }

        static double? CreateNullable(T? value)
        {
            if (value.HasValue)
                return double.CreateChecked(value.Value);
            return null;
        }

        public void WriteTo(MetaData metadata)
        {
            metadata.Set(Consts.HasBeenAnalysed, true);
            metadata.Set(Consts.IsNumeric, true);
            metadata.Set(Consts.L1Norm, double.CreateChecked(L1Norm));
			metadata.Set(Consts.L2Norm, double.CreateChecked(L2Norm));
			metadata.Set(Consts.Min, double.CreateChecked(Min));
			metadata.Set(Consts.Max, double.CreateChecked(Max));
			metadata.Set(Consts.Mean, double.CreateChecked(Mean));
            metadata.Set(Consts.Total, Count);
			metadata.SetIfNotNull(Consts.SampleVariance, CreateNullable(SampleVariance));
			metadata.SetIfNotNull(Consts.SampleStdDev, CreateNullable(SampleStdDev));
            metadata.SetIfNotNull(Consts.PopulationVariance, CreateNullable(PopulationVariance));
            metadata.SetIfNotNull(Consts.PopulationStdDev, CreateNullable(PopulationStdDev));
            metadata.SetIfNotNull(Consts.Median, CreateNullable(Median));
			metadata.SetIfNotNull(Consts.Mode, CreateNullable(Mode));
            metadata.Set(Consts.NumDistinct, NumDistinct);

			var total = T.CreateTruncating(Count);
            var range = Max - Min;
            if (range > T.Zero) {
                var bin = new LinearBinnedFrequencyAnalysis<T>(Min, Max, 10);
                var index = 0U;
                foreach (var item in _distinct.OrderByDescending(kv => kv.Value)) {
                    if (index++ < writeCount)
                        metadata.Set($"{Consts.FrequencyPrefix}{item.Key}", double.CreateChecked(T.CreateTruncating(item.Value) / total));
                    for (ulong i = 0; i < item.Value; i++)
                        bin.Add(item.Key);
                }

                var formatProvider = CultureInfo.InvariantCulture.NumberFormat;
                foreach (var (s, e, c) in bin.ContinuousFrequency) {
                    if (c == 0 && (T.IsNegativeInfinity(s) || T.IsPositiveInfinity(e)))
                        continue;
                    var start = T.IsNegativeInfinity(s) ? "-∞" : s.ToString("G17", formatProvider);
                    var end = T.IsPositiveInfinity(e) ? "∞" : e.ToString("G17", formatProvider);
                    metadata.Set($"{Consts.FrequencyRangePrefix}{start}/{end}", double.CreateChecked(T.CreateTruncating(c) / total));
                }
            }
		}
	}
}
