using System;
using System.Collections.Generic;
using System.Text;
using BrightData.Helper;
using BrightData.LinearAlgebra;

namespace BrightData.UnitTests
{
    public class UnitTestBase
    {
        protected readonly BrightDataContext _context = new BrightDataContext(0);

        public Vector<float> CreateRandomVector(uint size = 32)
        {
            var rand = new Random();
            return _context.CreateVector(size, i => FloatMath.Next(rand));
        }
    }
}
