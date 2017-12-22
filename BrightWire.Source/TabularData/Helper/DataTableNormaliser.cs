using BrightWire.Models.DataTable;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BrightWire.TabularData.Helper
{
	/// <summary>
	/// Builds a normalisation model that can be used to normalise a data table
	/// </summary>
	internal class DataTableNormaliser : IRowProcessor
	{
		readonly DataTableWriter _writer;
		readonly DataTableNormalisation _normalisationModel;

		public DataTableNormaliser(IDataTable dataTable, Stream output, DataTableNormalisation model)
		{
			_writer = new DataTableWriter(dataTable.Columns, output);
			_normalisationModel = model;
		}

		public DataTableNormaliser(IDataTable dataTable, NormalisationType type, Stream output, IEnumerable<int> columnIndices)
		{
			_writer = new DataTableWriter(dataTable.Columns, output);

			var analysis = dataTable.GetAnalysis();
			var columnNormList = new List<DataTableNormalisation.Column>();
			var columns = analysis.ColumnInfo.AsQueryable();
			if (columnIndices != null) {
				var columnSet = new HashSet<int>(columnIndices);
				columns = columns.Where(ci => columnSet.Contains(ci.ColumnIndex));
			}

			foreach (var columnInfo in columns) {
				var column = dataTable.Columns[columnInfo.ColumnIndex];
				if (column.IsContinuous) {
					if (columnInfo is INumericColumnInfo numericInfo) {
						if (type == NormalisationType.Standard && !numericInfo.StdDev.HasValue)
							continue;

						DataTableNormalisation.Column columnNorm;
						if (type == NormalisationType.Standard)
							columnNorm = new DataTableNormalisation.Column(columnInfo.ColumnIndex, column.Type, numericInfo.StdDev ?? 1, numericInfo.Mean);
						else if (type == NormalisationType.Euclidean)
							columnNorm = new DataTableNormalisation.Column(columnInfo.ColumnIndex, column.Type, numericInfo.L2Norm);
						else if (type == NormalisationType.Manhattan)
							columnNorm = new DataTableNormalisation.Column(columnInfo.ColumnIndex, column.Type, numericInfo.L1Norm);
						else if (type == NormalisationType.FeatureScale)
							columnNorm = new DataTableNormalisation.Column(columnInfo.ColumnIndex, column.Type, numericInfo.Max - numericInfo.Min, numericInfo.Min);
						else
							throw new NotImplementedException();
						columnNormList.Add(columnNorm);
					}
				}
			}
			_normalisationModel = new DataTableNormalisation {
				Type = type,
				ColumnNormalisation = columnNormList.ToArray()
			};
		}

		public bool Process(IRow row)
		{
			_writer.AddRow(_normalisationModel.Normalise(row.Data));
			return true;
		}

		public IDataTable GetDataTable()
		{
			return _writer.GetDataTable();
		}

		public DataTableNormalisation GetNormalisationModel()
		{
			return _normalisationModel;
		}
	}
}
