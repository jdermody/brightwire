using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightWire;
using BrightWire.TrainingData.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
	[TestClass]
	public class MiscTests
	{
		//static ILinearAlgebraProvider _lap;

		//[ClassInitialize]
		//public static void Load(TestContext context)
		//{
		//	_lap = BrightWireProvider.CreateLinearAlgebra(false);
		//}

		//[ClassCleanup]
		//public static void Cleanup()
		//{
		//	_lap.Dispose();
		//}

		[TestMethod]
		public void TestFloatConverter()
		{
			var converter = BrightWireProvider.CreateTypeConverter(float.NaN);

			Assert.IsFalse(float.IsNaN((float)converter.ConvertValue("45.5").ConvertedValue));
			Assert.IsTrue(float.IsNaN((float)converter.ConvertValue("sdf").ConvertedValue));
		}
	}
}
