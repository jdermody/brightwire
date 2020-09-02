using BrightData;
using MathNet.Numerics.LinearAlgebra.Complex;
using System.Collections.Generic;
using System.Linq;

namespace BrightWire.Models
{
	/// <summary>
	/// The output from a mini batch
	/// </summary>
	public class ExecutionResult
	{
		readonly IMiniBatchSequence _miniBatch;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="miniBatch">The mini batch sequence</param>
		/// <param name="output">The mini batch output</param>
		/// <param name="index">Output index</param>
		public ExecutionResult(IMiniBatchSequence miniBatch, IReadOnlyList<Vector<float>> output, uint index)
		{
			Index = index;
			_miniBatch = miniBatch;
			Output = output;
			Target = _miniBatch.Target?.GetMatrix().Data.Rows.ToList();
			Input = _miniBatch.Input.Select(input => input.GetMatrix().Data.Rows.ToList()).ToList();
		}

		/// <summary>
		/// Output index
		/// </summary>
		public uint Index { get; }

		/// <summary>
		/// The list of output rows
		/// </summary>
		public IReadOnlyList<Vector<float>> Output { get; }

		/// <summary>
		/// The list of target rows
		/// </summary>
		public IReadOnlyList<Vector<float>> Target { get; }

		/// <summary>
		/// The list of input rows
		/// </summary>
		public IReadOnlyList<IReadOnlyList<Vector<float>>> Input { get; }

		/// <summary>
		/// The mini batch
		/// </summary>
		public IMiniBatchSequence MiniBatchSequence => _miniBatch;

		/// <summary>
		/// Calculates the error of the output against the target
		/// </summary>
		/// <param name="errorMetric">The error metric to calculate with</param>
		/// <returns></returns>
		public float CalculateError(IErrorMetric errorMetric) => Output.Zip(Target, (o, t) => errorMetric.Compute(o, t)).Average();
	}
}
