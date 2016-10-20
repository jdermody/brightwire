using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace BrightWire.Models
{
    [ProtoContract]
    public class Normalisation
    {
        [ProtoContract]
        public class Column
        {
            [ProtoMember(1)]
            public int ColumnIndex { get; set; }

            [ProtoMember(2)]
            public ColumnType DataType { get; set; }

            [ProtoMember(3)]
            public double Subtract { get; set; }

            [ProtoMember(4)]
            public double Divide { get; set; }

            public Column() { }
            public Column(int columnIndex, ColumnType dataType, double divide, double subtract = 0.0)
            {
                ColumnIndex = columnIndex;
                DataType = dataType;
                Divide = divide;
                Subtract = subtract;
            }

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
                        return Convert.ToByte(ret);
                    default:
                        return ret;
                }
            }
        }

        [ProtoMember(1)]
        public NormalisationType Type { get; set; }

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

        public IReadOnlyList<object> Normalise(IReadOnlyList<object> row)
        {
            Column norm;
            var columnTable = ColumnTable;
            var ret = new object[row.Count];

            for (var i = 0; i < row.Count; i++) {
                object obj = null;
                if (columnTable.TryGetValue(i, out norm)) {
                    double val;
                    switch (norm.DataType) {
                        case ColumnType.Byte:
                            val = (byte)row[i];
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
                }
                else
                    obj = row[i];
                ret[i] = obj;
            }
            return ret;
        }
    }
}
