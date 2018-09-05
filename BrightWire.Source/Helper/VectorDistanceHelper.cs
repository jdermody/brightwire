using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using MathNet.Numerics.Distributions;

namespace BrightWire.Source.Helper
{
	/// <summary>
	/// 
	/// </summary>
    public class VectorDistanceHelper : IDisposable
	{
		readonly ILinearAlgebraProvider _lap;
		readonly DistanceMetric _distanceMetric;
		readonly List<IVector> _comparison = new List<IVector>();
		readonly IMatrix _data;
		IMatrix _compareTo = null;
		bool _isDirty = false;

	    public VectorDistanceHelper(ILinearAlgebraProvider lap, IReadOnlyList<IVector> data, DistanceMetric distanceMetric = DistanceMetric.Euclidean)
	    {
		    _lap = lap;
		    _distanceMetric = distanceMetric;
		    _data = lap.CreateMatrixFromColumns(data);
	    }

		public void Dispose()
		{
			_data.Dispose();
		}

		public IReadOnlyList<IVector> CompareTo => _comparison;

		public int AddForComparison(IVector comparison)
		{
			// TODO: append to existing matrix if possible
			var ret = _comparison.Count;
			_comparison.Add(comparison);
			_isDirty = true;
			return ret;
		}

		public void UpdateComparisonVector(int index, IVector newVector)
		{
			_comparison[index] = newVector;
			_isDirty = true;
		}

		public void UpdateComparisonVectors(IReadOnlyList<IVector> comparisonVectors)
		{
			Debug.Assert(comparisonVectors.Count == _comparison.Count);
			for (var i = 0; i < _comparison.Count; i++)
				_comparison[i] = comparisonVectors[i];
			_isDirty = true;
		}

		public IReadOnlyList<int> GetClosest()
		{
			var distance = _data.CalculateDistance(_GetComparisonMatrix(), _distanceMetric);
			return Enumerable.Range(0, distance.ColumnCount)
				.Select(i => distance.Column(i).MinimumIndex())
				.ToList()
			;
		}

		public IReadOnlyList<int> GetFurthest()
		{
			var distance = _data.CalculateDistance(_GetComparisonMatrix(), _distanceMetric);
			return Enumerable.Range(0, distance.ColumnCount)
				.Select(i => distance.Column(i).MaximumIndex())
				.ToList()
			;
		}

		public IDiscreteDistribution GetCategoricalDistribution()
		{
			var probabilityList = new List<double>();

			var distance = _data.CalculateDistance(_GetComparisonMatrix(), _distanceMetric);
			for (var i = 0; i < distance.ColumnCount; i++) {
				var column = distance.Column(i);
				probabilityList.Add(column.GetAt(column.MinimumIndex()));
			}
			return new Categorical(probabilityList.ToArray());
		}

		public IVector GetAverageFromData(IReadOnlyList<int> indices)
		{
			var data = _data.GetNewMatrixFromColumns(indices);
			var result = data.RowSums();
			result.Multiply(1f / indices.Count);
			return result;
		}

		IMatrix _GetComparisonMatrix()
		{
			if (_isDirty) {
				_isDirty = false;
				_compareTo?.Dispose();
				_compareTo = _lap.CreateMatrixFromColumns(_comparison);
			}

			return _compareTo;
		}
	}
}
