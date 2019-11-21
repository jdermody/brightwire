using System;
using System.Collections.Generic;
using System.Text;
using BrightData;
using BrightTable.Builders;
using BrightTable.Transformations.Conversions;

namespace BrightTable.Transformations
{
    public class ColumnConversionTransformation : TableTransformationBase
	{
        readonly IColumnOrientedDataTable _dataTable;
		readonly Dictionary<uint, IConvertColumn> _transform = new Dictionary<uint, IConvertColumn>();

        public ColumnConversionTransformation(IColumnOrientedDataTable dataTable, params ColumnConversion[] conversion)
        {
            _dataTable = dataTable;
            uint index = 0;
            foreach (var item in conversion) {
                if (item == ColumnConversion.ToBoolean)
                    Add(index, new ConvertColumnToBoolean("Y", "YES", "TRUE", "T"));
                else if (item == ColumnConversion.ToDate)
                    Add(index, new ConvertColumnToDate());
                else if (item == ColumnConversion.ToNumeric)
                    Add(index, new ConvertColumnToNumeric());
                else if (item == ColumnConversion.ToString)
                    Add(index, new ConvertColumnToString());
                else if (item == ColumnConversion.ToIndexList)
                    Add(index, new ConvertColumnToIndexList());
                else if (item == ColumnConversion.ToWeightedIndexList)
                    Add(index, new ConvertColumnToWeightedIndexList());
                else if (item == ColumnConversion.ToVector)
                    Add(index, new ConvertColumnToVector());
                ++index;
            }
        }

        public void Add(uint columnIndex, IConvertColumn columnConversion)
		{
			_transform.Add(columnIndex, columnConversion);
		}

        protected override ISingleTypeTableSegment Transform(IColumnOrientedDataTable dataTable, uint index, ISingleTypeTableSegment column)
        {
            if(_transform.TryGetValue(index, out var conversion)) {
                var converted = conversion.Convert(_dataTable.Context, column);
                column.MetaData.CopyTo(converted.MetaData, Consts.Name, Consts.IsTarget, Consts.Index);
                return converted;
            }
            return column;
        }

        internal override void Transform(IRowOrientedDataTable dataTable, RowOrientedTableBuilder builder)
        {
            throw new NotImplementedException();
        }

        internal override IReadOnlyList<(long Position, long EndOfColumnOffset)> Transform(ColumnOrientedTableBuilder builder)
        {
            throw new NotImplementedException();
        }
    }
}
