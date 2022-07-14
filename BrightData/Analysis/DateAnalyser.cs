using System;
using System.Collections.Generic;
using System.Globalization;

namespace BrightData.Analysis
{
    internal class DateAnalyser : FrequencyAnalyser<DateTime>
    {
        readonly HashSet<long> _distinct = new();
        readonly uint _maxCount;

        public DateAnalyser(uint maxCount = Consts.MaxDistinct)
        {
            _maxCount = maxCount;
        }

        public override void Add(DateTime date)
        {
            if (MinDate == null || date < MinDate)
                MinDate = date;
            if (MaxDate == null || date > MaxDate)
                MaxDate = date;

            AddString(date.ToString(CultureInfo.InvariantCulture));
            var ticks = date.Ticks;
            if (_distinct.Count < _maxCount)
                _distinct.Add(ticks);
        }

        public DateTime? MinDate { get; private set; }
        public DateTime? MaxDate { get; private set; }

        public override void WriteTo(MetaData metadata)
        {
            base.WriteTo(metadata);
            metadata.SetIfNotNull(Consts.MinDate, MinDate);
            metadata.SetIfNotNull(Consts.MaxDate, MaxDate);
            if(_distinct.Count < _maxCount)
                metadata.Set(Consts.NumDistinct, (uint)_distinct.Count);
        }
    }
}
