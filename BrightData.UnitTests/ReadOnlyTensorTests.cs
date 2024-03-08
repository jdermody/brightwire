using BrightData.UnitTests.Helper;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace BrightData.UnitTests
{
    public class ReadOnlyTensorTests : UnitTestBase
    {
        IReadOnlyVector<float> CreateVector() => _context.CreateReadOnlyVector(4, i => i + 1);
        IReadOnlyMatrix<float> CreateMatrix() => _context.CreateReadOnlyMatrixFromRows(CreateVector(), CreateVector(), CreateVector(), CreateVector());
        IReadOnlyTensor3D<float> CreateTensor3D() => _context.CreateReadOnlyTensor3D(CreateMatrix(), CreateMatrix(), CreateMatrix());
        IReadOnlyTensor4D<float> CreateTensor4D() => _context.CreateReadOnlyTensor4D(CreateTensor3D(), CreateTensor3D());

        [Fact]
        public void TestVectorValueSemantics()
        {
            var v1 = CreateVector();
            var v2 = CreateVector();

            v1.Should().Be(v2);
            var set = new HashSet<IReadOnlyVector<float>> {
                v1, v2
            };
            set.Count.Should().Be(1);
        }

        [Fact]
        public void TestMatrixValueSemantics()
        {
            var m1 = CreateMatrix();
            var m2 = CreateMatrix();

            m1.Should().Be(m2);
            var set = new HashSet<IReadOnlyMatrix<float>> {
                m1, m2
            };
            set.Count.Should().Be(1);
        }

        [Fact]
        public void TestTensor3DValueSemantics()
        {
            var t1 = CreateTensor3D();
            var t2 = CreateTensor3D();

            t1.Should().Be(t2);
            var set = new HashSet<IReadOnlyTensor3D<float>> {
                t1, t2
            };
            set.Count.Should().Be(1);
        }

        [Fact]
        public void TestTensor4DValueSemantics()
        {
            var t1 = CreateTensor4D();
            var t2 = CreateTensor4D();

            t1.Should().Be(t2);
            var set = new HashSet<IReadOnlyTensor4D<float>> {
                t1, t2
            };
            set.Count.Should().Be(1);
        }
    }
}
