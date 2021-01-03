using System;
using System.Collections.Generic;

namespace BrightData.Analysis
{
    class DateAnalyser : IDataAnalyser<DateTime>
    {
        readonly HashSet<long> _distinct = new HashSet<long>();
        private readonly uint _maxCount;

        public DateAnalyser(uint maxCount = Consts.MaxDistinct)
        {
            _maxCount = maxCount;
        }

        public void Add(DateTime date)
        {
            if (MinDate == null || date < MinDate)
                MinDate = date;
            if (MaxDate == null || date > MaxDate)
                MaxDate = date;

            var ticks = date.Ticks;
            if (_distinct.Count < _maxCount)
                _distinct.Add(ticks);
        }

        public DateTime? MinDate { get; private set; } = null;
        public DateTime? MaxDate { get; private set; } = null;

        public void WriteTo(IMetaData metadata)
        {
            metadata.Set(Consts.HasBeenAnalysed, true);
            metadata.SetIfNotNull(Consts.MinDate, MinDate);
            metadata.SetIfNotNull(Consts.MaxDate, MaxDate);
            if(_distinct.Count < _maxCount)
                metadata.Set(Consts.NumDistinct, (uint)_distinct.Count);
        }

        void IDataAnalyser.AddObject(object obj) => Add((DateTime) obj);
    }
}
