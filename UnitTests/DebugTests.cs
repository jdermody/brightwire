using System;
using BrightWire;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
	/// <summary>
	/// These tests use information that is only set during Debug builds
	/// </summary>
	[TestClass]
	public class DebugTests
	{
		[TestMethod]
		public void MemoryLayerTest()
		{
			#if DEBUG
			using (var context = BrightWireGpuProvider.CreateLinearAlgebra())
			{
				var matrix = context.CreateZeroMatrix(10, 10);
				context.PushLayer();
				var matrix2 = context.CreateZeroMatrix(10, 10);
				context.PopLayer();
				Assert.IsFalse(matrix2.IsValid);
				Assert.IsTrue(matrix.IsValid);
				matrix.Dispose();
				Assert.IsFalse(matrix.IsValid);
			}
			#endif
		}
	}
}
