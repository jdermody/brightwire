using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightTable;
using BrightWire.Models;

namespace BrightWire.Helper
{
	/// <summary>
	/// Vector to data table row adaptor
	/// </summary>
    class VectorAsRow : IDataTableSegment
	{
		readonly FloatVector _vector;

		public VectorAsRow(FloatVector vector)
		{
			_vector = vector;
		}

		public uint Size => (uint)_vector.Count;

		public ColumnType[] Types => throw new NotImplementedException();

		public object this[uint index] => _vector.Data[index];
	}
}
