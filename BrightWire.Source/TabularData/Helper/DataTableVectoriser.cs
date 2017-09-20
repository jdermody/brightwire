using BrightWire.Models;
using BrightWire.Source.Models.DataTable;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BrightWire.TabularData.Helper
{
    /// <summary>
    /// Converts data tables that might contain categorical or sparse vector based data into a dense vector format
    /// </summary>
    internal class DataTableVectoriser : IDataTableVectoriser
    {
        readonly DataTableVectorisation _vectorisationModel;

        public DataTableVectoriser(DataTableVectorisation model)
        {
            _vectorisationModel = model;
        }

        public DataTableVectoriser(IDataTable table, bool useTargetColumnIndex)
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

                if (columnInfo is IIndexColumnInfo indexColumn) {
                    columnModel.Size = Convert.ToInt32(indexColumn.MaxIndex + 1);
                } else {
                    columnModel.IsContinuous = column.IsContinuous || !columnInfo.NumDistinct.HasValue;
                    if (columnModel.IsContinuous)
                        columnModel.Size = 1;
                    else {
                        columnModel.Size = columnInfo.NumDistinct.Value;
                        columnModel.Values = columnInfo.DistinctValues
                            .Select(s => s.ToString())
                            .OrderBy(s => s)
                            .Select((s, i) => new DataTableVectorisation.CategoricalIndex {
                                Category = s,
                                Index = i
                            })
                            .ToArray()
                        ;
                    }
                }
                if (columnModel.IsTargetColumn) {
                    _vectorisationModel.OutputSize = columnModel.Size;
                    _vectorisationModel.IsTargetContinuous = columnModel.IsContinuous;
                    _vectorisationModel.HasTarget = true;
                } else
                    _vectorisationModel.InputSize += columnModel.Size;
            }
            _vectorisationModel.Columns = columnList.ToArray();
        }

        public int InputSize
        {
            get
            {
                return _vectorisationModel.InputSize;
            }
        }

        public int OutputSize
        {
            get
            {
                return _vectorisationModel.OutputSize;
            }
        }

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
