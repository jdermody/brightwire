using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire
{
    public static partial class ExtensionMethods
    {
        /// <summary>
        /// Returns the underlying .net type associated with the column type
        /// </summary>
        /// <param name="type">The column type</param>
        public static Type GetColumnType(this ColumnType type)
        {
            switch (type) {
                case ColumnType.Boolean:
                    return typeof(bool);

                case ColumnType.Byte:
                    return typeof(byte);

                case ColumnType.Date:
                    return typeof(DateTime);

                case ColumnType.Double:
                    return typeof(double);

                case ColumnType.Float:
                    return typeof(float);

                case ColumnType.Int:
                    return typeof(int);

                case ColumnType.Long:
                    return typeof(long);

                case ColumnType.Null:
                    return null;

                case ColumnType.String:
                    return typeof(string);

                case ColumnType.IndexList:
                    return typeof(IndexList);

                case ColumnType.WeightedIndexList:
                    return typeof(WeightedIndexList);

                case ColumnType.Vector:
                    return typeof(FloatVector);

                case ColumnType.Matrix:
                    return typeof(FloatMatrix);

                case ColumnType.Tensor:
                    return typeof(FloatTensor);

                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Converts the indexed classifications to sparse vectors
        /// </summary>
        /// <param name="groupByClassification">True to group by classification (i.e convert the bag to a set)</param>
        public static IReadOnlyList<(string Classification, WeightedIndexList WeightedIndex)> ConvertToSparseVectors(
            this IDataTable dataTable, 
            bool groupByClassification, 
            int columnIndex = 0
        ){
            var indexColumn = dataTable.Columns[columnIndex];
            if (indexColumn.Type != ColumnType.IndexList)
                throw new ArgumentException("Unexpected column type: " + indexColumn.Type.ToString());
            var targetColumnIndex = dataTable.TargetColumnIndex;
            if (targetColumnIndex == columnIndex)
                throw new ArgumentException("Invalid target column");

            var data = new List<(string Name, IndexList WeightedIndex)>();
            dataTable.ForEach(row => {
                data.Add((row.GetField<string>(targetColumnIndex), row.Data[0] as IndexList));
            });

            if (groupByClassification) {
                return data.GroupBy(c => c.Name)
                    .Select(g => (g.Key, new WeightedIndexList {
                        IndexList = g.SelectMany(d => d.WeightedIndex.Index)
                            .GroupBy(d => d)
                            .Select(g2 => new WeightedIndexList.WeightedIndex {
                                Index = g2.Key,
                                Weight = g2.Count()
                            })
                            .ToArray()
                    }))
                    .ToArray()
                ;
            } else {
                return data
                    .Select(d => (d.Name, new WeightedIndexList {
                        IndexList = d.WeightedIndex.Index
                            .GroupBy(wi => wi)
                            .Select(g2 => new WeightedIndexList.WeightedIndex {
                                Index = g2.Key,
                                Weight = g2.Count()
                            })
                            .ToArray()
                    }))
                    .ToArray()
                ;
            }
        }

        public static IDataTable ConvertToTable(this IReadOnlyList<(string Classification, WeightedIndexList WeightedIndex)> data)
        {
            var builder = BrightWireProvider.CreateDataTableBuilder();
            builder.AddColumn(ColumnType.WeightedIndexList, "Weighted Index");
            builder.AddColumn(ColumnType.String, "Classification", true);

            foreach (var item in data)
                builder.Add(item.WeightedIndex, item.Classification);

            return builder.Build();
        }

        public static IReadOnlyList<(string Classification, FloatVector Data)> Vectorise(this IReadOnlyList<(string Classification, WeightedIndexList WeightedIndex)> data)
        {
            var size = data.SelectMany(r => r.WeightedIndex.IndexList.Select(wi => wi.Index)).Max();
            FloatVector _Create(WeightedIndexList weightedIndexList)
            {
                var ret = new float[size];
                foreach(var item in weightedIndexList.IndexList)
                    ret[item.Index] = item.Weight;
                return new FloatVector {
                    Data = ret
                };
            }
            return data.Select(r => (r.Classification, _Create(r.WeightedIndex))).ToList();
        }
    }
}
