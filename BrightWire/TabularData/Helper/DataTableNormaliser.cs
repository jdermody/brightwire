using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BrightWire.TabularData.Helper
{
    internal class DataTableNormaliser : IRowProcessor
    {
        readonly DataTableWriter _writer;
        readonly DataTableNormalisation _normalisationModel;
        readonly IDataTable _table;

        public DataTableNormaliser(IDataTable dataTable, NormalisationType type, Stream output = null, DataTableNormalisation model = null)
        {
            _table = dataTable;
            _writer = new DataTableWriter(dataTable.Template, dataTable.Columns, output);

            if (model != null)
                _normalisationModel = model;
            else {
                var analysis = dataTable.GetAnalysis();
                var columnNormList = new List<DataTableNormalisation.Column>();
                foreach (var columnInfo in analysis.ColumnInfo) {
                    var column = dataTable.Columns[columnInfo.ColumnIndex];
                    if (column.IsContinuous) {
                        var numericInfo = columnInfo as INumericColumnInfo;
                        if (numericInfo != null) {
                            if (type == NormalisationType.Standard && !numericInfo.StdDev.HasValue)
                                continue;

                            DataTableNormalisation.Column columnNorm;
                            if (type == NormalisationType.Standard)
                                columnNorm = new DataTableNormalisation.Column(columnInfo.ColumnIndex, column.Type, numericInfo.StdDev.Value, numericInfo.Mean);
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
        }

        public bool Process(IRow row)
        {
            _writer.AddRow(_normalisationModel.Normalise(row.GetData()));
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
