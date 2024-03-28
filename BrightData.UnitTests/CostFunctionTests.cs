using BrightData.UnitTests.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace BrightData.UnitTests
{
    public class CostFunctionTests : UnitTestBase
    {
        [Fact]
        public void MSE_Same()
        {
            var lap = _context.LinearAlgebraProvider;
            var x1 = lap.CreateSegment(10, 0.5f);
            var x2 = lap.CreateSegment(10, 0.5f);
            var mse = lap.CreateMeanSquaredErrorCostFunction();
            var cost = mse.Cost(x1, x2);
            cost.Should().Be(0f);
        }

        [Fact]
        public void MSE_Different()
        {
            var lap = _context.LinearAlgebraProvider;
            var x1 = lap.CreateSegment(10, 0.6f);
            var x2 = lap.CreateSegment(10, 0.5f);
            var x3 = lap.CreateSegment(10, 0.4f);
            var mse = lap.CreateMeanSquaredErrorCostFunction();
            var cost1 = mse.Cost(x1, x2);
            cost1.Should().NotBe(0f);

            var cost2 = mse.Cost(x1, x3);
            cost2.Should().NotBe(0f);

            cost2.Should().BeGreaterThan(cost1);
        }

        [Fact]
        public void CrossEntropy_True()
        {
            var lap = _context.LinearAlgebraProvider;
            var x1 = lap.CreateSegment(10, 1f);
            var x2 = lap.CreateSegment(10, 1f);
            var mse = lap.CreateCrossEntropyCostFunction();
            var cost = mse.Cost(x1, x2);
            cost.Should().Be(0f);
        }

        [Fact]
        public void CrossEntropy_False()
        {
            var lap = _context.LinearAlgebraProvider;
            var x1 = lap.CreateSegment(10, 0f);
            var x2 = lap.CreateSegment(10, 0f);
            var mse = lap.CreateCrossEntropyCostFunction();
            var cost = mse.Cost(x1, x2);
            cost.Should().Be(0f);
        }

        [Fact]
        public void CrossEntropy_Different()
        {
            var lap = _context.LinearAlgebraProvider;
            var x1 = lap.CreateSegment(10, 0.6f);
            var x2 = lap.CreateSegment(10, 0.5f);
            var x3 = lap.CreateSegment(10, 0.4f);
            var mse = lap.CreateCrossEntropyCostFunction();
            var cost1 = mse.Cost(x1, x2);
            cost1.Should().NotBe(0f);

            var cost2 = mse.Cost(x1, x3);
            cost2.Should().NotBe(0f);

            cost2.Should().BeGreaterThan(cost1);
        }
    }
}
