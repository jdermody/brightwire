using BrightWire.Models;
using ProtoBuf;
using System.Collections.Generic;
using System.Linq;

namespace BrightWire.Source.Models.DataTable
{
    /// <summary>
    /// A data table vectorisation model - maps rows in a data table to vectors
    /// </summary>
    [ProtoContract]
    public class DataTableVectorisation
    {
        // lazy initialised variables
        Dictionary<int, Dictionary<int, string>> _reverseColumnMap = null;
        Dictionary<int, Dictionary<string, int>> _columnMap = null;

        /// <summary>
        /// A categorical column value
        /// </summary>
        [ProtoContract]
        public class CategoricalIndex
        {
            /// <summary>
            /// The classification label
            /// </summary>
            [ProtoMember(1)]
            public string Category { get; set; }

            /// <summary>
            /// The label's index
            /// </summary>
            [ProtoMember(2)]
            public int Index { get; set; }
        }

        /// <summary>
        /// Column information
        /// </summary>
        [ProtoContract]
        public class Column
        {
            /// <summary>
            /// The column index
            /// </summary>
            [ProtoMember(1)]
            public int ColumnIndex { get; set; }

            /// <summary>
            /// Column name
            /// </summary>
            [ProtoMember(2)]
            public string Name { get; set; }

            /// <summary>
            /// True if the column is the classification target
            /// </summary>
            [ProtoMember(3)]
            public bool IsTargetColumn { get; set; }

            /// <summary>
            /// True if the column has a continuous value
            /// </summary>
            [ProtoMember(4)]
            public bool IsContinuous { get; set; }

            /// <summary>
            /// The number slots this column will fill in the output vector
            /// </summary>
            [ProtoMember(5)]
            public int Size { get; set; }

            /// <summary>
            /// An array of categorial values
            /// </summary>
            [ProtoMember(6)]
            public CategoricalIndex[] Values { get; set; }
        }

        /// <summary>
        /// The columns in the table
        /// </summary>
        [ProtoMember(1)]
        public Column[] Columns { get; set; }

        /// <summary>
        /// The size of each input vector that will be created
        /// </summary>
        [ProtoMember(2)]
        public int InputSize { get; set; }

        /// <summary>
        /// The size of each output vector that will be created
        /// </summary>
        [ProtoMember(3)]
        public int OutputSize { get; set; }

        /// <summary>
        /// True if the vectoriser has a classification target column
        /// </summary>
        [ProtoMember(4)]
        public bool HasTarget { get; set; }

        /// <summary>
        /// True if the classification target column is continuous
        /// </summary>
        [ProtoMember(5)]
        public bool IsTargetContinuous { get; set; }

        /// <summary>
        /// The column index of the classifiation target column
        /// </summary>
        [ProtoMember(6)]
        public int ClassColumnIndex { get; set; }

        /// <summary>
        /// A dictionary of column to categorical value tables
        /// </summary>
        public Dictionary<int, Dictionary<string, int>> ColumnMap
        {
            get
            {
                if(_columnMap == null) {
                    lock(this) {
                        if(_columnMap == null) {
                            _columnMap = Columns
                                .Where(c => c.Values != null)
                                .Select(c => (c.ColumnIndex, c.Values.ToDictionary(cv => cv.Category, cv => cv.Index)))
                                .ToDictionary(d => d.Item1, d => d.Item2)
                            ;
                        }
                    }
                }
                return _columnMap;
            }
        }

        /// <summary>
        /// A dictionary of column to reversed categorical value tables
        /// </summary>
        public Dictionary<int, Dictionary<int, string>> ReverseColumnMap
        {
            get
            {
                if (_reverseColumnMap == null) {
                    lock (this) {
                        if (_reverseColumnMap == null) {
                            _reverseColumnMap = Columns
                                .Where(c => c.Values != null)
                                .Select(c => (c.ColumnIndex, c.Values.ToDictionary(cv => cv.Index, cv => cv.Category)))
                                .ToDictionary(d => d.Item1, d => d.Item2)
                            ;
                        }
                    }
                }
                return _reverseColumnMap;
            }
        }

        /// <summary>
        /// Returns the classification label for the corresponding column/vector indices
        /// </summary>
        /// <param name="columnIndex">The data table column index</param>
        /// <param name="vectorIndex">The one hot vector index</param>
        /// <returns></returns>
        public string GetOutputLabel(int columnIndex, int vectorIndex)
        {
            if (HasTarget) {
                if (ReverseColumnMap.TryGetValue(columnIndex, out Dictionary<int, string> map)) {
                    if (map.TryGetValue(vectorIndex, out string ret))
                        return ret;
                }
            }
            return null;
        }

        /// <summary>
        /// Vectorises the input columns of the specified row
        /// </summary>
        /// <param name="row">>The row to vectorise</param>
        public FloatVector GetInput(IRow row)
        {
            var ret = new float[InputSize];
            var index = 0;

            for (int i = 0, len = Columns.Length; i < len; i++) {
                var column = Columns[i];
                if (i == ClassColumnIndex)
                    continue;

                if (column.IsContinuous)
                    ret[index++] = row.GetField<float>(i);
                else {
                    var str = row.GetField<string>(i);
                    var offset = index + ColumnMap[i][str];
                    index += column.Size;
                    ret[offset] = 1f;
                }
            }
            return new FloatVector {
                Data = ret
            };
        }

        /// <summary>
        /// Vectorises the output column of the specified row
        /// </summary>
        /// <param name="row">The row to vectorise</param>
        public FloatVector GetOutput(IRow row)
        {
            if (HasTarget) {
                var ret = new float[OutputSize];
                if (IsTargetContinuous)
                    ret[0] = row.GetField<float>(ClassColumnIndex);
                else {
                    var str = row.GetField<string>(ClassColumnIndex);
                    var offset = ColumnMap[ClassColumnIndex][str];
                    ret[offset] = 1f;
                }
                return new FloatVector {
                    Data = ret
                };
            }
            return null;
        }
    }
}
