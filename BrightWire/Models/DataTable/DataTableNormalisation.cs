using System;
using System.Collections.Generic;
using System.Linq;
using BrightData;
using BrightTable;
using BrightWire.LinearAlgebra.Helper;

namespace BrightWire.Models.DataTable
{
    /// <summary>
    /// A data table normalisation model
    /// </summary>
    public class DataTableNormalisation
    {
        /// <summary>
        /// A column model
        /// </summary>
        public class Column
        {
            /// <summary>
            /// The column index
            /// </summary>
            public int ColumnIndex { get; set; }

            /// <summary>
            /// The type of data in the column
            /// </summary>
            public ColumnType DataType { get; set; }

            /// <summary>
            /// The value to subtract from the column
            /// </summary>
            public double Subtract { get; set; }

            /// <summary>
            /// The value to divide the column with (after subtraction)
            /// </summary>
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
                var ret = (Math.Abs(Divide) < BoundMath.ZERO_LIKE) ? val : (val - Subtract) / Divide;
	            return _Convert(ret);
            }

			/// <summary>
			/// Applies the normalisation in reverse
			/// </summary>
			/// <param name="val">Value to transform</param>
	        public object ReverseNormalise(double val)
	        {
		        var ret = val * Divide + Subtract;
		        return _Convert(ret);
	        }

	        object _Convert(double val)
	        {
		        switch (DataType) {
			        case ColumnType.Float:
				        return Convert.ToSingle(val);
			        case ColumnType.Long:
				        return Convert.ToInt64(val);
			        case ColumnType.Int:
				        return Convert.ToInt32(val);
			        case ColumnType.Byte:
				        return Convert.ToSByte(val);
			        default:
				        return val;
		        }
	        }
        }

		/// <summary>
		/// A vector based column to normalise
		/// </summary>
	    public class VectorColumn
	    {
		    /// <summary>
		    /// The column index
		    /// </summary>
		    public int ColumnIndex { get; set; }

		    /// <summary>
		    /// The normalisation data within the vector (each index within the vector becomes a "column")
		    /// </summary>
		    public Column[] VectorColumns { get; set; }

	    }

        /// <summary>
        /// The type of normalisation
        /// </summary>
        public NormalizationType Type { get; set; }

        /// <summary>
        /// The column normalisation data
        /// </summary>
        public Column[] ColumnNormalisation { get; set; }

		/// <summary>
		/// Vector columns normalisation data
		/// </summary>
		public VectorColumn[] VectorColumnNormalisation { get; set; }

        Dictionary<int, Column> _columnTable = null;
        Dictionary<int, Column> ColumnTable
        {
            get
            {
                return _columnTable ?? (_columnTable = ColumnNormalisation?.ToDictionary(c => c.ColumnIndex, c => c));
            }
        }

	    Dictionary<int, VectorColumn> _vectorColumnTable = null;
	    Dictionary<int, VectorColumn> VectorColumnTable
	    {
		    get
		    {
			    return _vectorColumnTable ?? (_vectorColumnTable = VectorColumnNormalisation?.ToDictionary(c => c.ColumnIndex, c => c));
		    }
	    }

        /// <summary>
        /// Normalises a row in the data table
        /// </summary>
        /// <param name="row">The row to normalise</param>
        public IReadOnlyList<object> Normalise(IReadOnlyList<object> row)
        {
	        var ret = new object[row.Count];

	        for (var i = 0; i < row.Count; i++)
		        ret[i] = _Normalise(i, row[i], true);

	        return ret;
        }

		/// <summary>
		/// Applies the normalisation in reverse
		/// </summary>
		/// <param name="columnIndex">Column index in original table</param>
		/// <param name="val">Value to normalise</param>
		/// <returns>Reverse normalised value</returns>
	    public object ReverseNormalise(int columnIndex, object val)
	    {
		    return _Normalise(columnIndex, val, false);
	    } 

	    object _Normalise(int columnIndex, object obj, bool normalise)
	    {
		    object ret = null;

		    if (ColumnTable.TryGetValue(columnIndex, out var norm)) {
			    double val;
			    switch (norm.DataType) {
				    case ColumnType.Byte:
					    val = (sbyte)obj;
					    break;
				    case ColumnType.Double:
					    val = (double)obj;
					    break;
				    case ColumnType.Float:
					    val = (float)obj;
					    break;
				    case ColumnType.Int:
					    val = (int)obj;
					    break;
				    case ColumnType.Long:
					    val = (long)obj;
					    break;
				    default:
					    throw new NotImplementedException();
			    }
			    ret = normalise ? norm.Normalise(val) : norm.ReverseNormalise(val);
		    }else if (VectorColumnTable != null && VectorColumnTable.TryGetValue(columnIndex, out var vectorNorm)) {
			    var vector = (FloatVector)obj;
			    var normalised = vector.Data.Values.Zip(vectorNorm.VectorColumns, (v, n) => (float)n.Normalise(Convert.ToDouble(v))).ToArray();
			    ret = FloatVector.Create(normalised);
		    } else
			    ret = obj;

			return ret;
	    }
    }
}
