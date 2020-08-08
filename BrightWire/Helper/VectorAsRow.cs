using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightWire.Models;

namespace BrightWire.Helper
{
	/// <summary>
	/// Vector to data table row adaptor
	/// </summary>
    class VectorAsRow : IRow
	{
		readonly FloatVector _vector;

		public VectorAsRow(FloatVector vector)
		{
			_vector = vector;
		}

		public int Index => 0;
		public IReadOnlyList<object> Data => _vector.Data.Cast<object>().ToList();
	    public T GetField<T>(int index)
	    {
		    return (T)(object)Convert.ToDouble(_vector.Data[index]);
	    }

	    public IReadOnlyList<T> GetFields<T>(IReadOnlyList<int> indices)
	    {
		    return indices.Select(GetField<T>).ToList();
	    }

		public IHaveColumns Table => throw new NotImplementedException();
	}
}
