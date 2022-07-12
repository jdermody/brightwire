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
	public class ExecutionResult : IDisposable
	{
		readonly IMiniBatchSequence _miniBatch;
        readonly IMatrix _output;
        readonly IMatrix? _target, _input;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="miniBatch">The mini batch sequence</param>
		/// <param name="output">The mini batch output</param>
        public ExecutionResult(IMiniBatchSequence miniBatch, IMatrix output)
        {
            _miniBatch = miniBatch;

            _output = output;
            _output.Segment.AddRef();

            _target = _miniBatch.Target?.GetMatrix();
            _target?.Segment.AddRef();

            _input = _miniBatch.Input?.GetMatrix();
            _input?.Segment.AddRef();
        }

        public void Dispose()
        {
            _output.Segment.Release();
            _target?.Segment.Release();
            _input?.Segment.Release();
        }

        /// <summary>
		/// The list of output rows
		/// </summary>
		public IMatrix Output { get; }

		/// <summary>
		/// The list of target rows
		/// </summary>
		public IMatrix? Target { get; }

		/// <summary>
		/// The list of input rows
		/// </summary>
		public IMatrix? Input { get; }

        /// <summary>
        /// Optional list of errors
        /// </summary>
        public IMatrix? Error { get; set; } = null;

		/// <summary>
		/// The mini batch
		/// </summary>
		public IMiniBatchSequence MiniBatchSequence => _miniBatch;

		/// <summary>
		/// Calculates the error of the output against the target
		/// </summary>
		/// <param name="errorMetric">The error metric to calculate with</param>
		/// <returns></returns>
		public float CalculateError(IErrorMetric errorMetric) => Output.AllRows().Zip(Target!.AllRows(), errorMetric.Compute).Average();
    }
}
