using BrightData.UnitTests.Helper;
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

        [Fact]
        public void HingeLoss_Same()
        {
            var lap = _context.LinearAlgebraProvider;
            var x1 = lap.CreateSegment(10, 1f);
            var x2 = lap.CreateSegment(10, 1f);
            var hingeloss = lap.CreateHingeLossCostFunction();
            var cost = hingeloss.Cost(x1, x2);
            cost.Should().Be(0f);
        }

        [Fact]
        public void HingeLoss_Different()
        {
            var lap = _context.LinearAlgebraProvider;
            var x1 = lap.CreateSegment(10, 1f);
            var x2 = lap.CreateSegment(10, -1f);
            var hingeloss = lap.CreateHingeLossCostFunction();
            var cost = hingeloss.Cost(x1, x2);
            // Margin = y * f(x) = 1 * (-1) = -1
            // Loss = max(0, 1 - margin) = max(0, 1-(-1)) = max(0, 2) = 2
            cost.Should().Be(2f);
        }

        [Fact]
        public void HingeLoss_MarginGreaterThanOne()
        {
            var lap = _context.LinearAlgebraProvider;
            var x1 = lap.CreateSegment(10, 2f); // prediction > 1 for class 1
            var x2 = lap.CreateSegment(10, 1f); // expected class 1
            var hingeloss = lap.CreateHingeLossCostFunction();
            var cost = hingeloss.Cost(x1, x2);
            // Margin = y * f(x) = 1 * 2 = 2 > 1
            // Loss = max(0, 1 - margin) = max(0, 1-2) = max(0, -1) = 0
            cost.Should().Be(0f);
        }

        [Fact]
        public void HuberLoss_SmallError()
        {
            var lap = _context.LinearAlgebraProvider;
            var x1 = lap.CreateSegment(10, 1.0f); // prediction
            var x2 = lap.CreateSegment(10, 1.2f); // expected (small error)
            var huberloss = lap.CreateHuberLossCostFunction();
            var cost = huberloss.Cost(x1, x2);
            // Error = 1.2 - 1.0 = 0.2
            // Since |error| = 0.2 < delta (default=1), use quadratic: 0.5 * 0.2^2 = 0.02
            cost.Should().BeApproximately(0.02f, 8);
        }

        [Fact]
        public void HuberLoss_LargeError()
        {
            var lap = _context.LinearAlgebraProvider;
            var x1 = lap.CreateSegment(10, 1.0f); // prediction
            var x2 = lap.CreateSegment(10, 3.0f); // expected (large error)
            var huberloss = lap.CreateHuberLossCostFunction();
            var cost = huberloss.Cost(x1, x2);
            // Error = 3.0 - 1.0 = 2.0
            // Since |error| = 2.0 > delta (default=1), use linear: 1 * (2.0 - 0.5*1) = 1.5
            cost.Should().Be(1.5f);
        }

        [Fact]
        public void HuberLoss_CustomDelta()
        {
            var lap = _context.LinearAlgebraProvider;
            var x1 = lap.CreateSegment(10, 1.0f); // prediction
            var x2 = lap.CreateSegment(10, 3.0f); // expected (large error)
            var huberloss = lap.CreateHuberLossCostFunction(0.5f);
            var cost = huberloss.Cost(x1, x2);
            // Error = 3.0 - 1.0 = 2.0
            // Since |error| = 2.0 > delta (0.5), use linear: 0.5 * (2.0 - 0.5*0.5) = 0.5 * 1.75 = 0.875
            cost.Should().Be(0.875f);
        }
    }
}
