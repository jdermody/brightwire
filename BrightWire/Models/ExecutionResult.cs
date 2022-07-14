using System;
using System.Collections.Generic;
using System.Linq;
using BrightData;
using BrightData.LinearAlgebra;

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
        public ExecutionResult(IMiniBatchSequence miniBatch, IMatrix output, bool wantInputInExecutionResults)
        {
            _miniBatch = miniBatch;

            Output = output.CopyAllRows();
            Target = _miniBatch.Target?.GetMatrix().CopyAllRows();
			if(wantInputInExecutionResults)
                Input = _miniBatch.Input?.GetMatrix().CopyAllRows();
        }

        /// <summary>
		/// The list of output rows
		/// </summary>
		public IVectorInfo[] Output { get; }

		/// <summary>
		/// The list of target rows
		/// </summary>
		public IVectorInfo[]? Target { get; }

		/// <summary>
		/// The list of input rows
		/// </summary>
		public IVectorInfo[]? Input { get; }

        /// <summary>
        /// Optional list of errors
        /// </summary>
        public IVectorInfo[]? Error { get; set; } = null;

		/// <summary>
		/// The mini batch
		/// </summary>
		public IMiniBatchSequence MiniBatchSequence => _miniBatch;

		/// <summary>
		/// Calculates the error of the output against the target
		/// </summary>
		/// <param name="errorMetric">The error metric to calculate with</param>
		/// <returns></returns>
		public float CalculateError(IErrorMetric errorMetric) => Output.Zip(Target!, errorMetric.Compute).Average();
    }
}
