using System;
using BrightData.Helper;
using BrightData.UnitTests.Fixtures;
using BrightData.UnitTests.Helper;
using Xunit;
using FluentAssertions;

namespace BrightData.UnitTests
{
    public partial class TensorOperationTests : UnitTestBase
    {
        public void TestAdd() 
        {
            var a = CreateRandomVector();
            var b = CreateRandomVector();
        }
    }
}