using System;
using System.Collections.Generic;

namespace BrightData.Analysis
{
    public class DateAnalyser : IDataAnalyser<DateTime>
    {
        readonly HashSet<long> _distinct = new HashSet<long>();

        public void Add(DateTime date)
        {
            if (MinDate == null || date < MinDate)
                MinDate = date;
            if (MaxDate == null || date > MaxDate)
                MaxDate = date;

            var ticks = date.Ticks;
            if (_distinct.Count < Consts.MaxDistinct)
                _distinct.Add(ticks);
        }

        public DateTime? MinDate { get; private set; } = null;
        public DateTime? MaxDate { get; private set; } = null;

        public void WriteTo(IMetaData metadata)
        {
            metadata.Set(Consts.HasBeenAnalysed, true);
            metadata.WriteIfNotNull(Consts.MinDate, MinDate);
            metadata.WriteIfNotNull(Consts.MaxDate, MaxDate);
            if(_distinct.Count < Consts.MaxDistinct)
                metadata.Set(Consts.NumDistinct, _distinct.Count);
        }

        void IDataAnalyser.AddObject(object obj) => Add((DateTime) obj);
    }
}
