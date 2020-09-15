using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using BrightData;
using MathNet.Numerics.Distributions;

namespace BrightWire.Source.Helper
{
	/// <summary>
	/// Calculates the distance between a list of vectors and a list of vectors to compare against
	/// </summary>
    public class VectorDistanceHelper : IDisposable
	{
		readonly ILinearAlgebraProvider _lap;
		readonly DistanceMetric _distanceMetric;
		readonly List<IFloatVector> _comparison = new List<IFloatVector>();
		readonly IFloatVector[] _data;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="lap">Linear algebra provider</param>
		/// <param name="data">List of vectors to compare</param>
		/// <param name="distanceMetric">Distance metric for comparison</param>
	    public VectorDistanceHelper(IFloatVector[] data, DistanceMetric distanceMetric = DistanceMetric.Euclidean)
	    {
			_lap = data[0].LinearAlgebraProvider;
		    _distanceMetric = distanceMetric;
		    _data = data;
	    }

		void IDisposable.Dispose()
		{
			_comparison.ForEach(c => c.Dispose());
			_comparison.Clear();
		}

		/// <summary>
		/// The list of vectors to compare against
		/// </summary>
		public IReadOnlyList<IFloatVector> CompareTo => _comparison;

		/// <summary>
		/// Distance metric
		/// </summary>
		public DistanceMetric Metric => _distanceMetric;

		/// <summary>
		/// Adds a comparison vector (will be owned and disposed by the helper class)
		/// </summary>
		/// <param name="comparison">Vector to compare against</param>
		/// <returns>Index of the comparison vector</returns>
		public int AddComparison(IFloatVector comparison)
		{
			var ret = _comparison.Count;
			_comparison.Add(comparison);
			return ret;
		}

		/// <summary>
		/// Updates the comparison vector at this index (disposes the old vector)
		/// </summary>
		/// <param name="index">Index to update</param>
		/// <param name="newVector">Vector to replace with</param>
		public void UpdateComparisonVector(int index, IFloatVector newVector)
		{
			_comparison[index].Dispose();
			_comparison[index] = newVector;
		}

		/// <summary>
		/// Updates the entire list of comparison vectors
		/// </summary>
		/// <param name="comparisonVectors">List of vectors to compare against</param>
		public void SetComparisonVectors(IEnumerable<IFloatVector> comparisonVectors)
		{
			_comparison.ForEach(c => c.Dispose());
			_comparison.Clear();
			_comparison.AddRange(comparisonVectors);
		}

		/// <summary>
		/// Returns the index of the closest comparison vector for each vector
		/// </summary>
		public uint[] GetClosest()
        {
            using var distance = _lap.CalculateDistances(_data, _comparison, _distanceMetric);
            return _data.Length.AsRange()
                .Select(i => _GetMinimum(distance, i).Index)
                .ToArray();
        }

		/// <summary>
		/// Returns a vector averaged from the data vectors
		/// </summary>
		/// <param name="indices">Indices of the data vectors to use in the averaged vector</param>
		public IFloatVector GetAverageFromData(int[] indices)
        {
            using var data = _lap.CreateMatrixFromColumns(indices.Select(i => _data[i]).ToArray());
            var result = data.RowSums();
            result.Multiply(1f / indices.Length);
            return result;
        }

		(uint Index, float Value) _GetMinimum(IFloatMatrix data, uint columnIndex)
		{
            var len = _comparison.Count;

			if (_lap.IsGpu) {
				if (len == 1)
					return (0, data.GetAt(0, columnIndex));

				var column = data.Column(columnIndex);
				var index = column.MinimumIndex();
				return (index, column.GetAt(index));
			}

            var matrix = data.AsIndexable();
				
            if (len == 1)
                return (0, matrix[0, columnIndex]);
            else if(len == 0)
                throw new Exception("Cannot find minimum with zero length");

            uint bestIndex = uint.MaxValue;
            var min = float.MaxValue;
            for (uint j = 0; j < len; j++) {
                var val = matrix[j, columnIndex];
                if (val < min) {
                    bestIndex = j;
                    min = val;
                }
            }
            return (bestIndex, min);
        }
	}
}
