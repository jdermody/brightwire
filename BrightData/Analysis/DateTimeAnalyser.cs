using System;
using System.Collections.Generic;
using System.Globalization;

namespace BrightData.Analysis
{
    /// <summary>
    /// Date time analysis
    /// </summary>
    internal class DateTimeAnalyser : FrequencyAnalyser<DateTime>
    {
        readonly HashSet<long> _distinct = new();

        public DateTimeAnalyser()
        {
        }

        public override void Add(DateTime date)
        {
            if (MinDate == null || date < MinDate)
                MinDate = date;
            if (MaxDate == null || date > MaxDate)
                MaxDate = date;

            AddString(date.ToString(CultureInfo.InvariantCulture));
            var ticks = date.Ticks;
            _distinct.Add(ticks);
        }

        public DateTime? MinDate { get; private set; }
        public DateTime? MaxDate { get; private set; }

        public override void WriteTo(MetaData metadata)
        {
            base.WriteTo(metadata);
            metadata.SetIfNotNull(Consts.MinDate, MinDate);
            metadata.SetIfNotNull(Consts.MaxDate, MaxDate);
            metadata.Set(Consts.NumDistinct, (uint)_distinct.Count);
        }
    }
}
