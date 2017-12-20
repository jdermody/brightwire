using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BrightWire.Models.DataTable
{
    /// <summary>
    /// A data table normalisation model
    /// </summary>
    [ProtoContract]
    public class DataTableNormalisation
    {
        /// <summary>
        /// A column model
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
            /// The type of data in the column
            /// </summary>
            [ProtoMember(2)]
            public ColumnType DataType { get; set; }

            /// <summary>
            /// The value to subtract from the column
            /// </summary>
            [ProtoMember(3)]
            public double Subtract { get; set; }

            /// <summary>
            /// The value to divide the column with (after subtraction)
            /// </summary>
            [ProtoMember(4)]
            public double Divide { get; set; }

            /// <summary>
            /// Default constructor
            /// </summary>
            public Column() { }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="columnIndex"></param>
            /// <param name="dataType"></param>
            /// <param name="divide"></param>
            /// <param name="subtract"></param>
            public Column(int columnIndex, ColumnType dataType, double divide, double subtract = 0.0)
            {
                ColumnIndex = columnIndex;
                DataType = dataType;
                Divide = divide;
                Subtract = subtract;
            }

            /// <summary>
            /// Perform the normalisation step
            /// </summary>
            /// <param name="val">The input value</param>
            /// <returns>The normalused input value</returns>
            public object Normalise(double val)
            {
                var ret = (val - Subtract) / Divide;

                switch (DataType) {
                    case ColumnType.Float:
                        return Convert.ToSingle(ret);
                    case ColumnType.Long:
                        return Convert.ToInt64(ret);
                    case ColumnType.Int:
                        return Convert.ToInt32(ret);
                    case ColumnType.Byte:
                        return Convert.ToSByte(ret);
                    default:
                        return ret;
                }
            }
        }

        /// <summary>
        /// The type of normalisation
        /// </summary>
        [ProtoMember(1)]
        public NormalisationType Type { get; set; }

        /// <summary>
        /// The column normalisation data
        /// </summary>
        [ProtoMember(2)]
        public Column[] ColumnNormalisation { get; set; }

        Dictionary<int, Column> _columnTable = null;
        Dictionary<int, Column> ColumnTable
        {
            get
            {
                return _columnTable ?? (_columnTable = ColumnNormalisation.ToDictionary(c => c.ColumnIndex, c => c));
            }
        }

        /// <summary>
        /// Normalises a row in the data table
        /// </summary>
        /// <param name="row">The row to normalise</param>
        public IReadOnlyList<object> Normalise(IReadOnlyList<object> row)
        {
            var columnTable = ColumnTable;
            var ret = new object[row.Count];

            for (var i = 0; i < row.Count; i++) {
                object obj = null;
                if (columnTable.TryGetValue(i, out Column norm)) {
                    double val;
                    switch (norm.DataType) {
                        case ColumnType.Byte:
                            val = (sbyte)row[i];
                            break;
                        case ColumnType.Double:
                            val = (double)row[i];
                            break;
                        case ColumnType.Float:
                            val = (float)row[i];
                            break;
                        case ColumnType.Int:
                            val = (int)row[i];
                            break;
                        case ColumnType.Long:
                            val = (long)row[i];
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                    obj = norm.Normalise(val);
                } else
                    obj = row[i];
                ret[i] = obj;
            }
            return ret;
        }
    }
}
