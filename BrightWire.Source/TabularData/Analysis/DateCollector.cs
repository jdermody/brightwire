using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.TabularData.Analysis
{
	class DateCollector : IRowProcessor, IDateColumnInfo
	{
		readonly HashSet<long> _distinct = new HashSet<long>();

		public DateCollector(int columnIndex)
		{
			ColumnIndex = columnIndex;
		}

		public DateTime? MinDate { get; set; }

		public DateTime? MaxDate { get; set; }

		public int ColumnIndex { get; }

		public IEnumerable<object> DistinctValues => throw new NotImplementedException();

		public int? NumDistinct => _distinct.Count;

		public bool Process(IRow row)
		{
			var date = row.GetField<DateTime>(ColumnIndex);
			if (MinDate == null || date < MinDate)
				MinDate = date;
			if (MaxDate == null || date > MaxDate)
				MaxDate = date;
			return true;
		}
	}
}
