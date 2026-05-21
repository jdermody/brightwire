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

        // ========== MSE Gradient Tests ==========

        [Fact]
        public void MSE_Gradient_SameValues()
        {
            var lap = _context.LinearAlgebraProvider;
            var predicted = lap.CreateSegment(0.5f, 0.5f, 0.5f);
            var expected = lap.CreateSegment(0.5f, 0.5f, 0.5f);
            var mse = lap.CreateMeanSquaredErrorCostFunction();
            var gradient = mse.Gradient(predicted, expected);
            // Gradient = (expected - predicted) / N = [0, 0, 0] / 3 = [0, 0, 0]
            ((float?)gradient[0]).Should().BeApproximately(0f, 1e-5f);
            ((float?)gradient[1]).Should().BeApproximately(0f, 1e-5f);
            ((float?)gradient[2]).Should().BeApproximately(0f, 1e-5f);
        }

        [Fact]
        public void MSE_Gradient_ConstantDifference()
        {
            var lap = _context.LinearAlgebraProvider;
            var predicted = lap.CreateSegment(0f, 0f, 0f);
            var expected = lap.CreateSegment(1f, 1f, 1f);
            var mse = lap.CreateMeanSquaredErrorCostFunction();
            var gradient = mse.Gradient(predicted, expected);
            // Gradient = (1 - 0) / 3 = 1/3 for each element
            var expectedGrad = 1f / 3f;
            ((float?)gradient[0]).Should().BeApproximately(expectedGrad, 1e-5f);
            ((float?)gradient[1]).Should().BeApproximately(expectedGrad, 1e-5f);
            ((float?)gradient[2]).Should().BeApproximately(expectedGrad, 1e-5f);
        }

        [Fact]
        public void MSE_Gradient_MixedValues()
        {
            var lap = _context.LinearAlgebraProvider;
            var predicted = lap.CreateSegment(1f, 2f, 3f);
            var expected = lap.CreateSegment(4f, 5f, 6f);
            var mse = lap.CreateMeanSquaredErrorCostFunction();
            var gradient = mse.Gradient(predicted, expected);
            // Gradient = (expected - predicted) / N = [3, 3, 3] / 3 = [1, 1, 1]
            ((float?)gradient[0]).Should().BeApproximately(1f, 1e-5f);
            ((float?)gradient[1]).Should().BeApproximately(1f, 1e-5f);
            ((float?)gradient[2]).Should().BeApproximately(1f, 1e-5f);
        }

        [Fact]
        public void MSE_Gradient_NegativeDifference()
        {
            var lap = _context.LinearAlgebraProvider;
            var predicted = lap.CreateSegment(3f, 2f, 1f);
            var expected = lap.CreateSegment(1f, 2f, 3f);
            var mse = lap.CreateMeanSquaredErrorCostFunction();
            var gradient = mse.Gradient(predicted, expected);
            // Gradient = (expected - predicted) / N = [-2, 0, 2] / 3
            ((float?)gradient[0]).Should().BeApproximately(-2f / 3f, 1e-5f);
            ((float?)gradient[1]).Should().BeApproximately(0f, 1e-5f);
            ((float?)gradient[2]).Should().BeApproximately(2f / 3f, 1e-5f);
        }

        // ========== CrossEntropy Gradient Tests ==========

        [Fact]
        public void CrossEntropy_Gradient_SameValues()
        {
            var lap = _context.LinearAlgebraProvider;
            var predicted = lap.CreateSegment(1f, 1f, 1f);
            var expected = lap.CreateSegment(1f, 1f, 1f);
            var ce = lap.CreateCrossEntropyCostFunction();
            var gradient = ce.Gradient(predicted, expected);
            // Gradient = expected - predicted = [0, 0, 0]
            ((float?)gradient[0]).Should().BeApproximately(0f, 1e-5f);
            ((float?)gradient[1]).Should().BeApproximately(0f, 1e-5f);
            ((float?)gradient[2]).Should().BeApproximately(0f, 1e-5f);
        }

        [Fact]
        public void CrossEntropy_Gradient_AllZero()
        {
            var lap = _context.LinearAlgebraProvider;
            var predicted = lap.CreateSegment(0f, 0f, 0f);
            var expected = lap.CreateSegment(0f, 0f, 0f);
            var ce = lap.CreateCrossEntropyCostFunction();
            var gradient = ce.Gradient(predicted, expected);
            // Gradient = expected - predicted = [0, 0, 0]
            ((float?)gradient[0]).Should().BeApproximately(0f, 1e-5f);
            ((float?)gradient[1]).Should().BeApproximately(0f, 1e-5f);
            ((float?)gradient[2]).Should().BeApproximately(0f, 1e-5f);
        }

        [Fact]
        public void CrossEntropy_Gradient_MixedBinary()
        {
            var lap = _context.LinearAlgebraProvider;
            var predicted = lap.CreateSegment(0.8f, 0.3f, 0.6f);
            var expected = lap.CreateSegment(1f, 0f, 1f);
            var ce = lap.CreateCrossEntropyCostFunction();
            var gradient = ce.Gradient(predicted, expected);
            // Gradient = expected - predicted = [0.2, -0.3, 0.4]
            ((float?)gradient[0]).Should().BeApproximately(0.2f, 1e-5f);
            ((float?)gradient[1]).Should().BeApproximately(-0.3f, 1e-5f);
            ((float?)gradient[2]).Should().BeApproximately(0.4f, 1e-5f);
        }

        [Fact]
        public void CrossEntropy_Gradient_Probabilities()
        {
            var lap = _context.LinearAlgebraProvider;
            var predicted = lap.CreateSegment(0.9f, 0.1f, 0.5f);
            var expected = lap.CreateSegment(1f, 0f, 0f);
            var ce = lap.CreateCrossEntropyCostFunction();
            var gradient = ce.Gradient(predicted, expected);
            // Gradient = expected - predicted = [0.1, -0.1, -0.5]
            ((float?)gradient[0]).Should().BeApproximately(0.1f, 1e-5f);
            ((float?)gradient[1]).Should().BeApproximately(-0.1f, 1e-5f);
            ((float?)gradient[2]).Should().BeApproximately(-0.5f, 1e-5f);
        }

        // ========== HingeLoss Gradient Tests ==========

        [Fact]
        public void HingeLoss_Gradient_CorrectClassification()
        {
            var lap = _context.LinearAlgebraProvider;
            var predicted = lap.CreateSegment(2f, 3f, 1.5f);
            var expected = lap.CreateSegment(1f, 1f, 1f);
            var hl = lap.CreateHingeLossCostFunction();
            var gradient = hl.Gradient(predicted, expected);
            // Margin = y * f(x) = [2, 3, 1.5], all >= 1, so gradient = [0, 0, 0]
            ((float?)gradient[0]).Should().BeApproximately(0f, 1e-5f);
            ((float?)gradient[1]).Should().BeApproximately(0f, 1e-5f);
            ((float?)gradient[2]).Should().BeApproximately(0f, 1e-5f);
        }

        [Fact]
        public void HingeLoss_Gradient_Misclassification()
        {
            var lap = _context.LinearAlgebraProvider;
            var predicted = lap.CreateSegment(-1f, -1f, -1f);
            var expected = lap.CreateSegment(1f, 1f, 1f);
            var hl = lap.CreateHingeLossCostFunction();
            var gradient = hl.Gradient(predicted, expected);
            // Margin = y * f(x) = [-1, -1, -1], all < 1, so gradient = -y = [-1, -1, -1]
            ((float?)gradient[0]).Should().BeApproximately(-1f, 1e-5f);
            ((float?)gradient[1]).Should().BeApproximately(-1f, 1e-5f);
            ((float?)gradient[2]).Should().BeApproximately(-1f, 1e-5f);
        }

        [Fact]
        public void HingeLoss_Gradient_MixedMargin()
        {
            var lap = _context.LinearAlgebraProvider;
            var predicted = lap.CreateSegment(2f, 0.5f, -0.5f);
            var expected = lap.CreateSegment(1f, 1f, 1f);
            var hl = lap.CreateHingeLossCostFunction();
            var gradient = hl.Gradient(predicted, expected);
            // Margin = [2, 0.5, -0.5]: first >= 1 (grad=0), rest < 1 (grad=-1)
            ((float?)gradient[0]).Should().BeApproximately(0f, 1e-5f);
            ((float?)gradient[1]).Should().BeApproximately(-1f, 1e-5f);
            ((float?)gradient[2]).Should().BeApproximately(-1f, 1e-5f);
        }

        [Fact]
        public void HingeLoss_Gradient_NegativeLabels()
        {
            var lap = _context.LinearAlgebraProvider;
            var predicted = lap.CreateSegment(1f, -2f);
            var expected = lap.CreateSegment(-1f, -1f);
            var hl = lap.CreateHingeLossCostFunction();
            var gradient = hl.Gradient(predicted, expected);
            // Margin = y * f(x) = [-1, 2]: first < 1 (grad = -y = 1), second >= 1 (grad = 0)
            ((float?)gradient[0]).Should().BeApproximately(1f, 1e-5f);
            ((float?)gradient[1]).Should().BeApproximately(0f, 1e-5f);
        }

        // ========== HuberLoss Gradient Tests ==========

        [Fact]
        public void HuberLoss_Gradient_SmallError()
        {
            var lap = _context.LinearAlgebraProvider;
            var predicted = lap.CreateSegment(1f, 1f, 1f);
            var expected = lap.CreateSegment(1.2f, 1.1f, 1.3f);
            var hl = lap.CreateHuberLossCostFunction();
            var gradient = hl.Gradient(predicted, expected);
            // diff = expected - predicted = [0.2, 0.1, 0.3], |diff| <= delta(1.0)
            // Gradient = -diff = [-0.2, -0.1, -0.3]
            ((float?)gradient[0]).Should().BeApproximately(-0.2f, 1e-5f);
            ((float?)gradient[1]).Should().BeApproximately(-0.1f, 1e-5f);
            ((float?)gradient[2]).Should().BeApproximately(-0.3f, 1e-5f);
        }

        [Fact]
        public void HuberLoss_Gradient_LargeError()
        {
            var lap = _context.LinearAlgebraProvider;
            var predicted = lap.CreateSegment(1f, 1f, 1f);
            var expected = lap.CreateSegment(5f, -3f, 10f);
            var hl = lap.CreateHuberLossCostFunction();
            var gradient = hl.Gradient(predicted, expected);
            // diff = [4, -4, 9], |diff| > delta(1.0) for all
            // Gradient = -sign(diff) * delta = [-1, 1, -1]
            ((float?)gradient[0]).Should().BeApproximately(-1f, 1e-5f);
            ((float?)gradient[1]).Should().BeApproximately(1f, 1e-5f);
            ((float?)gradient[2]).Should().BeApproximately(-1f, 1e-5f);
        }

        [Fact]
        public void HuberLoss_Gradient_MixedError()
        {
            var lap = _context.LinearAlgebraProvider;
            var predicted = lap.CreateSegment(1f, 2f, 3f);
            var expected = lap.CreateSegment(1.5f, 5f, 2.5f);
            var hl = lap.CreateHuberLossCostFunction();
            var gradient = hl.Gradient(predicted, expected);
            // diff = [0.5, 3, -0.5]: first |0.5|<=1 (grad=-0.5), second |3|>1 (grad=-1), third |-0.5|<=1 (grad=0.5)
            ((float?)gradient[0]).Should().BeApproximately(-0.5f, 1e-5f);
            ((float?)gradient[1]).Should().BeApproximately(-1f, 1e-5f);
            ((float?)gradient[2]).Should().BeApproximately(0.5f, 1e-5f);
        }

        [Fact]
        public void HuberLoss_Gradient_CustomDelta()
        {
            var lap = _context.LinearAlgebraProvider;
            var predicted = lap.CreateSegment(1f, 1f);
            var expected = lap.CreateSegment(2f, 5f);
            var hl = lap.CreateHuberLossCostFunction(2.0f);
            var gradient = hl.Gradient(predicted, expected);
            // diff = [1, 4]: |1|<=2 (grad=-1), |4|>2 (grad=-2)
            ((float?)gradient[0]).Should().BeApproximately(-1f, 1e-5f);
            ((float?)gradient[1]).Should().BeApproximately(-2f, 1e-5f);
        }

        [Fact]
        public void HuberLoss_Gradient_ZeroError()
        {
            var lap = _context.LinearAlgebraProvider;
            var predicted = lap.CreateSegment(1f, 2f, 3f);
            var expected = lap.CreateSegment(1f, 2f, 3f);
            var hl = lap.CreateHuberLossCostFunction();
            var gradient = hl.Gradient(predicted, expected);
            // diff = [0, 0, 0], gradient = [0, 0, 0]
            ((float?)gradient[0]).Should().BeApproximately(0f, 1e-5f);
            ((float?)gradient[1]).Should().BeApproximately(0f, 1e-5f);
            ((float?)gradient[2]).Should().BeApproximately(0f, 1e-5f);
        }

    }
}
