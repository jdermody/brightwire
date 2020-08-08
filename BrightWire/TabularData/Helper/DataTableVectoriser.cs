using BrightWire.Models;
using BrightWire.Models.DataTable;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BrightWire.TabularData.Helper
{
	/// <summary>
	/// Converts data tables that might contain categorical or sparse vector based data into a dense vector format
	/// </summary>
	class DataTableVectoriser : IDataTableVectoriser
	{
		readonly DataTableVectorisation _vectorisationModel;

		public DataTableVectoriser(DataTableVectorisation model)
		{
			_vectorisationModel = model;
		}

		public DataTableVectoriser(IDataTable table, bool useTargetColumnIndex, int maxNumericCategoricalExpansion = 128)
		{
			var classColumnIndex = useTargetColumnIndex ? table.TargetColumnIndex : -1;

			var analysis = table.GetAnalysis();
			_vectorisationModel = new DataTableVectorisation {
				ClassColumnIndex = classColumnIndex
			};

			var columnList = new List<DataTableVectorisation.Column>();
			foreach (var columnInfo in analysis.ColumnInfo) {
				var column = table.Columns[columnInfo.ColumnIndex];
				var columnModel = new DataTableVectorisation.Column {
					ColumnIndex = columnInfo.ColumnIndex,
					IsContinuous = false,
					IsTargetColumn = columnInfo.ColumnIndex == classColumnIndex,
					Size = 0,
					Name = column.Name
				};
				columnList.Add(columnModel);

				if (column.Type == ColumnType.Boolean) {
					columnModel.Size = 1;
					columnModel.IsBinary = true;
				}else if (columnInfo is IIndexColumnInfo indexColumn) {
					columnModel.Size = Convert.ToInt32(indexColumn.MaxIndex + 1);
				} else if (columnInfo is IDimensionsColumnInfo vectorColumn) {
					var size = vectorColumn.XDimension ?? 0;
					if (vectorColumn.YDimension.HasValue)
						size *= vectorColumn.YDimension.Value;
					if (vectorColumn.ZDimension.HasValue)
						size *= vectorColumn.ZDimension.Value;
					columnModel.Size = size;
				} else if (columnInfo is INumericColumnInfo) {
					columnModel.IsContinuous = column.IsContinuous || !columnInfo.NumDistinct.HasValue || columnInfo.NumDistinct.Value > maxNumericCategoricalExpansion;
					if (columnModel.IsContinuous)
						columnModel.Size = 1;
					else {
						columnModel.Size = columnInfo.NumDistinct ?? 0;
						columnModel.Values = _GetCategoricalValues(columnInfo.DistinctValues);
					}
				} else if (columnInfo.NumDistinct.HasValue) {
					columnModel.Size = columnInfo.NumDistinct ?? 0;
					columnModel.Values = _GetCategoricalValues(columnInfo.DistinctValues);
				}

				if (columnModel.IsTargetColumn) {
					_vectorisationModel.OutputSize = columnModel.Size;
					_vectorisationModel.IsTargetContinuous = columnModel.IsContinuous;
					_vectorisationModel.HasTarget = true;
					_vectorisationModel.IsTargetBinary = columnModel.IsBinary;
				} else
					_vectorisationModel.InputSize += columnModel.Size;
			}
			_vectorisationModel.Columns = columnList.ToArray();
		}

		DataTableVectorisation.CategoricalIndex[] _GetCategoricalValues(IEnumerable<object> values)
		{
			return values
				.Select(s => s.ToString())
				.OrderBy(s => s)
				.Select((s, i) => new DataTableVectorisation.CategoricalIndex {
					Category = s,
					Index = i
				})
				.ToArray()
			;
		}

		public int InputSize => _vectorisationModel.InputSize;
		public int OutputSize => _vectorisationModel.OutputSize;

		public string GetOutputLabel(int columnIndex, int vectorIndex)
		{
			return _vectorisationModel.GetOutputLabel(columnIndex, vectorIndex);
		}

		public FloatVector GetInput(IRow row)
		{
			return _vectorisationModel.GetInput(row);
		}

		public FloatVector GetOutput(IRow row)
		{
			return _vectorisationModel.GetOutput(row);
		}

		public DataTableVectorisation GetVectorisationModel() => _vectorisationModel;
	}
}
