using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.TabularData.Analysis
{
	class DimensionCollector : IRowProcessor, IDimensionsColumnInfo
	{
		readonly HashSet<string> _distinct = new HashSet<string>();

		public DimensionCollector(int index)
		{
			ColumnIndex = index;
		}

		public int? XDimension { get; set; }

		public int? YDimension { get; set; }

		public int? ZDimension { get; set; }

		public int ColumnIndex {get;}

		public IEnumerable<object> DistinctValues => throw new NotImplementedException();

		public int? NumDistinct => _distinct.Count;

		public bool Process(IRow row)
		{
			var obj = row.Data[ColumnIndex];
			if (obj is FloatVector vector) {
				_distinct.Add($"{vector.Size}");
				if (XDimension == null || vector.Size > XDimension)
					XDimension = vector.Size;
			}else if (obj is FloatMatrix matrix) {
				_distinct.Add($"{matrix.RowCount},{matrix.ColumnCount}");
				if (XDimension == null || matrix.ColumnCount > XDimension)
					XDimension = matrix.ColumnCount;
				if (YDimension == null || matrix.RowCount > YDimension)
					XDimension = matrix.RowCount;
			}else if (obj is FloatTensor tensor) {
				_distinct.Add($"{tensor.RowCount},{tensor.ColumnCount},{tensor.Depth}");
				if (XDimension == null || tensor.ColumnCount > XDimension)
					XDimension = tensor.ColumnCount;
				if (YDimension == null || tensor.RowCount > YDimension)
					XDimension = tensor.RowCount;
				if (ZDimension == null || tensor.Depth > ZDimension)
					ZDimension = tensor.Depth;
			}
			else
				throw new NotImplementedException();

			return true;
		}
	}
}
