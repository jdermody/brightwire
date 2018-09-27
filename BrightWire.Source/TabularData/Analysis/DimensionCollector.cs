using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.TabularData.Analysis
{
	class DimensionCollector : IRowProcessor, IDimensionsColumnInfo
	{
		public DimensionCollector(int index)
		{
			ColumnIndex = index;
		}

		public int? XDimension { get; set; }

		public int? YDimension { get; set; }

		public int? ZDimension { get; set; }

		public int ColumnIndex { get; }

		public IEnumerable<object> DistinctValues => null;

		public int? NumDistinct => null;
		public ColumnInfoType Type => ColumnInfoType.Dimensions;

		public bool Process(IRow row)
		{
			var obj = row.Data[ColumnIndex];
			if (obj is FloatVector vector) {
				if (XDimension == null || vector.Size > XDimension)
					XDimension = vector.Size;
			} else if (obj is FloatMatrix matrix) {
				if (XDimension == null || matrix.ColumnCount > XDimension)
					XDimension = matrix.ColumnCount;
				if (YDimension == null || matrix.RowCount > YDimension)
					XDimension = matrix.RowCount;
			} else if (obj is FloatTensor tensor) {
				if (XDimension == null || tensor.ColumnCount > XDimension)
					XDimension = tensor.ColumnCount;
				if (YDimension == null || tensor.RowCount > YDimension)
					XDimension = tensor.RowCount;
				if (ZDimension == null || tensor.Depth > ZDimension)
					ZDimension = tensor.Depth;
			} else
				throw new NotImplementedException();

			return true;
		}
	}
}
