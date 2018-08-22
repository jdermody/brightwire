using BrightWire.Models;
using BrightWire.TabularData;
using BrightWire.TabularData.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace BrightWire.Helper
{
    /// <summary>
    /// Builds data tables
    /// </summary>
    class DataTableBuilder : IHaveColumns
    {
        const int MAX_UNIQUE = 131072 * 4;

        internal class Column : IColumn
        {
	        internal bool? _isContinuous;
            internal ColumnType _type;
            readonly HashSet<string> _uniqueValues = new HashSet<string>();

	        public Column(int index, ColumnType type, string name, bool isTarget)
            {
                _type = type;
                Name = name;
				Index = index;
                IsTarget = isTarget;
                if (type == ColumnType.Double || type == ColumnType.Float)
                    _isContinuous = true;
            }

            public int NumDistinct => _uniqueValues.Count == MAX_UNIQUE ? 0 : _uniqueValues.Count;

	        bool _ValidateDimension(int size, int? dimension)
	        {
		        if (!dimension.HasValue)
			        return true;
		        return size == dimension.Value;
	        }

	        bool _Validate(object value)
	        {
		        switch (_type) {
					case ColumnType.Boolean:
						return value is Boolean;
					case ColumnType.Byte:
						return value is SByte;
					case ColumnType.Date:
						return value is DateTime;
					case ColumnType.Double:
						return value is Double;
					case ColumnType.Float:
						return value is float;
					case ColumnType.IndexList:
						return value is IndexList;
					case ColumnType.Int:
						return value is int;
					case ColumnType.Long:
						return value is long;
					case ColumnType.Matrix:
						return value is FloatMatrix matrix && _ValidateDimension(matrix.RowCount, DimensionY) && _ValidateDimension(matrix.ColumnCount, DimensionX);
					case ColumnType.String:
						return value is string;
					case ColumnType.Tensor:
						return value is FloatTensor tensor && _ValidateDimension(tensor.Depth, DimensionZ) && _ValidateDimension(tensor.RowCount, DimensionY) && _ValidateDimension(tensor.ColumnCount, DimensionX);
					case ColumnType.Vector:
						return value is FloatVector vector && _ValidateDimension(vector.Count, DimensionX);
					case ColumnType.WeightedIndexList:
						return value is WeightedIndexList;

					case ColumnType.Null:
					default:
						throw new NotImplementedException();
		        }
	        }

	        public void Add(object value, bool validate)
            {
                if (value != null) {
	                if (validate && !_Validate(value))
		                throw new ArgumentException($"Invalid type for column {Index} ({_type.ToString()}): {value.ToString()}");
                    if (_uniqueValues.Count < MAX_UNIQUE)
                        _uniqueValues.Add(value.ToString());
                }else if (validate)
	                throw new ArgumentException("null values are not allowed");
            }

			public int Index {get;}
            public ColumnType Type => _type;
	        public string Name { get; }
	        public bool IsTarget { get; }
	        public int? DimensionX { get; set; }
	        public int? DimensionY { get; set; }
	        public int? DimensionZ { get; set; }

	        public bool IsContinuous
            {
                get => _isContinuous.HasValue && _isContinuous.Value;
		        set => _isContinuous = value;
	        }
        }

	    readonly int _blockSize;
	    readonly bool _validateOnAdd;
        readonly List<Column> _column = new List<Column>();
        readonly List<IRow> _data = new List<IRow>();
        readonly RowConverter _rowConverter = new RowConverter();

        public DataTableBuilder(bool validateOnAdd = false, int blockSize = 1024)
        {
	        _blockSize = blockSize;
	        _validateOnAdd = validateOnAdd;
        }

        public DataTableBuilder(IEnumerable<IColumn> columns, bool validateOnAdd = true, int blockSize = 1024) 
	        : this(validateOnAdd, blockSize)
        {
            foreach (var column in columns) {
                var col = AddColumn(column.Type, column.Name, column.IsTarget);
                col.IsContinuous = column.IsContinuous;
            }
        }

		public static DataTableBuilder CreateTwoColumnMatrix()
		{
			var ret = new DataTableBuilder();
			ret.AddColumn(ColumnType.Matrix, "Input");
			ret.AddColumn(ColumnType.Matrix, "Output", true);
			return ret;
		}

		public IReadOnlyList<IColumn> Columns => _column;
	    internal IReadOnlyList<Column> Columns2 => _column;
	    public int RowCount => _data.Count;
	    public int ColumnCount => _column.Count;
	    public int BlockSize => _blockSize;

	    public IColumn AddColumn(ColumnType type, string name = "", bool isTarget = false)
	    {
		    if (type == ColumnType.Null)
			    throw new ArgumentException("null columns are not currently usable");
		    //else if (type == ColumnType.Vector || type == ColumnType.Matrix || type == ColumnType.Tensor)
			   // throw new InvalidOperationException("Please use the corresponding Add method for vectors, matrices and tensors");

            return _AddColumn(type, name, isTarget);
        }

	    Column _AddColumn(ColumnType type, string name = "", bool isTarget = false)
	    {
		    var ret = new Column(_column.Count, type, name, isTarget);
		    _column.Add(ret);
		    return ret;
	    }

	    public IColumn AddVectorColumn(int size, string name = "", bool isTarget = false)
	    {
		    var ret = _AddColumn(ColumnType.Vector, name, isTarget);
			ret.DimensionX = size;
		    return ret;
	    }

	    public IColumn AddMatrixColumn(int rows, int columns, string name = "", bool isTarget = false)
	    {
		    var ret = _AddColumn(ColumnType.Matrix, name, isTarget);
		    ret.DimensionX = columns;
		    ret.DimensionY = rows;
		    return ret;
	    }

	    public IColumn AddTensorColumn(int rows, int columns, int depth, string name = "", bool isTarget = false)
	    {
		    var ret = _AddColumn(ColumnType.Matrix, name, isTarget);
		    ret.DimensionX = columns;
		    ret.DimensionY = rows;
		    ret.DimensionZ = depth;
		    return ret;
	    }

        internal IRow AddRow(IRow row)
        {
            _data.Add(row);

            var data = row.Data;
            for (int j = 0, len = data.Count; j < len && j < _column.Count; j++)
                _column[j].Add(data[j], _validateOnAdd);
            return row;
        }

        public IRow Add(params object[] data)
        {
            return AddRow(CreateRow(data));
        }

        internal DataTableRow CreateRow(IReadOnlyList<object> data)
        {
            return new DataTableRow(this, data, _rowConverter);
        }

        internal void WriteMetadata(Stream stream)
        {
            var writer = new BinaryWriter(stream, Encoding.UTF8, true);
	        writer.Write(1); // format version
	        writer.Write(_blockSize); // write the block size
            writer.Write(_column.Count); // write the number of columns
            foreach (var column in _column) {
				var type = column.Type;
                writer.Write(column.Name);
                writer.Write((byte)type);
                writer.Write(column.IsTarget);
                writer.Write(column.NumDistinct);
                writer.Write(column._isContinuous.HasValue);
                if (column._isContinuous.HasValue)
                    writer.Write(column._isContinuous.Value);
	            if (type == ColumnType.Vector || type == ColumnType.Matrix || type == ColumnType.Tensor)
		            writer.Write(column.DimensionX ?? -1);
				if(type == ColumnType.Matrix || type == ColumnType.Tensor)
					writer.Write(column.DimensionY ?? -1);
	            if(type == ColumnType.Tensor)
		            writer.Write(column.DimensionZ ?? -1);
            }
        }

        void _WriteValueTo(BinaryWriter writer, Column column, object val)
        {
            var type = column.Type;

            if (type == ColumnType.Date)
                writer.Write(((DateTime)val).Ticks);
            else if (type == ColumnType.Boolean)
                writer.Write((bool)val);
            else if (type == ColumnType.Double)
                writer.Write((double)val);
            else if (type == ColumnType.Float)
                writer.Write((float)val);
            else if (type == ColumnType.Int)
                writer.Write((int)val);
            else if (type == ColumnType.Long)
                writer.Write((long)val);
            else if (type == ColumnType.String)
                writer.Write((string)val);
            else if (type == ColumnType.Byte)
                writer.Write((sbyte)val);
            else if (type == ColumnType.IndexList) {
                var data = (IndexList)val;
                data.WriteTo(writer);
            } else if (type == ColumnType.WeightedIndexList) {
                var data = (WeightedIndexList)val;
                data.WriteTo(writer);
            } else if (type == ColumnType.Vector) {
                var data = (FloatVector)val;
                data.WriteTo(writer);
            } else if (type == ColumnType.Matrix) {
                var data = (FloatMatrix)val;
                data.WriteTo(writer);
            } else if (type == ColumnType.Tensor) {
                var data = (FloatTensor)val;
                data.WriteTo(writer);
            }
        }

        internal int WriteData(Stream stream)
        {
            int ret = 0;
            var writer = new BinaryWriter(stream, Encoding.UTF8, true);
            foreach (var row in _data) {
                int index = 0;
                var rowData = row.Data;
                foreach (var column in _column) {
                    if (index < rowData.Count) {
                        var val = rowData[index++];
                        _WriteValueTo(writer, column, val);
                    } else {
                        // if the value is missing then write the column's default value instead
                        object val = null;
                        var ct = column.Type;
                        if (ct == ColumnType.String)
                            val = "";
                        else if (ct == ColumnType.Date)
                            val = DateTime.MinValue;
                        else if (ct != ColumnType.Null) {
                            var columnType = ct.GetColumnType();
                            if (columnType.GetTypeInfo().IsValueType)
                                val = Activator.CreateInstance(columnType);
                        }
                        _WriteValueTo(writer, column, val);
                    }
                }
                ++ret;
            }
            return ret;
        }

        internal void Process(IRowProcessor rowProcessor)
        {
            foreach (var item in _data) {
                if (!rowProcessor.Process(item))
                    break;
            }
        }

        public IDataTable Build(Stream output = null)
        {
            var writer = new DataTableWriter(Columns, output);
            Process(writer);
            return writer.GetDataTable();
        }

        public void ClearRows()
        {
            _data.Clear();
        }
    }
}
