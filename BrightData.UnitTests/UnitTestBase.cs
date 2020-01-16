using System;
using System.Collections.Generic;
using System.Text;
using BrightData.Helper;

namespace BrightData.UnitTests
{
    public class UnitTestBase
    {
        private readonly BrightDataContext _context = new BrightDataContext();

        public Vector<float> CreateRandomVector(uint size = 32)
        {
            var rand = new Random();
            return _context.CreateVector(size, i => FloatMath.Next(rand));
        }
    }
}
