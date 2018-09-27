using BrightWire.Models.DataTable;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BrightWire.Models;
using BrightWire.TabularData.Analysis;

namespace BrightWire.TabularData.Helper
{
	/// <summary>
	/// Builds a normalisation model that can be used to normalise a data table
	/// </summary>
	class DataTableNormaliser : IRowProcessor
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

			var vectorColumns = new List<(int ColumnIndex, int Size)>();
			foreach (var columnInfo in columns) {
				var column = dataTable.Columns[columnInfo.ColumnIndex];
				if (column.IsContinuous && columnInfo is INumericColumnInfo numericInfo) {
					var columnNorm = _GetColumn(type, numericInfo, columnInfo.ColumnIndex, column.Type);
					if(columnNorm != null)
						columnNormList.Add(columnNorm);
				}else if (column.Type == ColumnType.Vector && columnInfo is IDimensionsColumnInfo vector && vector.XDimension.HasValue && vector.XDimension.Value > 0) {
					vectorColumns.Add((column.Index, vector.XDimension.Value));
				}
			}

			DataTableNormalisation.VectorColumn[] vectorColumnNormList = null;
			if (vectorColumns.Any()) {
				var collectors = vectorColumns.Select(vc => Enumerable.Range(0, vc.Size).Select(i => new NumberCollector(i)).ToList()).ToList();
				dataTable.ForEach(row => {
					foreach (var column in vectorColumns.Zip(collectors, (vc, c) => (vc, c))) {
						var vectorAsRow = row.GetField<FloatVector>(column.Item1.ColumnIndex).AsRow();
						foreach (var collector in column.Item2)
							collector.Process(vectorAsRow);
					}
				});
				vectorColumnNormList = collectors.Select((c, i) => new DataTableNormalisation.VectorColumn {
					ColumnIndex = vectorColumns[i].ColumnIndex,
					VectorColumns = c.Select((nc, j) => _GetColumn(type, nc, j, ColumnType.Float)).ToArray()
				}).ToArray();
			}

			_normalisationModel = new DataTableNormalisation {
				Type = type,
				ColumnNormalisation = columnNormList.ToArray(),
				VectorColumnNormalisation = vectorColumnNormList
			};
		}

		DataTableNormalisation.Column _GetColumn(NormalisationType type, INumericColumnInfo numericInfo, int columnIndex, ColumnType columnType)
		{
			if (type == NormalisationType.Standard && !numericInfo.StdDev.HasValue)
				return null;

			if (type == NormalisationType.Standard)
				return new DataTableNormalisation.Column(columnIndex, columnType, numericInfo.StdDev ?? 1, numericInfo.Mean);
			else if (type == NormalisationType.Euclidean)
				return new DataTableNormalisation.Column(columnIndex, columnType, numericInfo.L2Norm);
			else if (type == NormalisationType.Manhattan)
				return new DataTableNormalisation.Column(columnIndex, columnType, numericInfo.L1Norm);
			else if (type == NormalisationType.FeatureScale)
				return new DataTableNormalisation.Column(columnIndex, columnType, numericInfo.Max - numericInfo.Min, numericInfo.Min);
			else
				throw new NotImplementedException();
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
