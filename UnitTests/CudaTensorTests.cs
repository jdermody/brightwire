using BrightWire;
using MathNet.Numerics.Distributions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitTests.Helper;

namespace UnitTests
{
    [TestClass]
    public class CudaTensorTests
    {
        static ILinearAlgebraProvider _cuda;
        static ILinearAlgebraProvider _cpu;

        [ClassInitialize]
        public static void Load(TestContext context)
        {
            _cuda = BrightWireGpuProvider.CreateLinearAlgebra(false);
            _cpu = BrightWireProvider.CreateLinearAlgebra(false);
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            _cuda.Dispose();
            _cpu.Dispose();
        }

        [TestMethod]
        public void TensorConvertToVector()
        {
            using (var cpuTensor = _cpu.CreateTensor(Enumerable.Range(0, 3).Select(i => _cpu.CreateMatrix(4, 4, (j, k) => (i + 1) * (j + 1) * (k + 1))).ToList()))
            using (var gpuTensor = _cuda.CreateTensor(cpuTensor.AsIndexable()))
            using (var cpuVector = cpuTensor.ConvertToVector())
            using (var gpuVector = gpuTensor.ConvertToVector())
                FloatingPointHelper.AssertEqual(cpuVector.AsIndexable(), gpuVector.AsIndexable());
        }

        [TestMethod]
        public void TensorConvertToMatrix()
        {
            using (var cpuTensor = _cpu.CreateTensor(Enumerable.Range(0, 3).Select(i => _cpu.CreateMatrix(4, 4, (j, k) => (i + 1) * (j + 1) * (k + 1))).ToList()))
            using (var gpuTensor = _cuda.CreateTensor(cpuTensor.AsIndexable()))
            using (var cpuMatrix = cpuTensor.ConvertToMatrix())
            using (var gpuMatrix = gpuTensor.ConvertToMatrix())
                FloatingPointHelper.AssertEqual(cpuMatrix.AsIndexable(), gpuMatrix.AsIndexable());
        }

        [TestMethod]
        public void TensorAddPadding()
        {
            using (var cpuTensor = _cpu.CreateTensor(Enumerable.Range(0, 3).Select(i => _cpu.CreateMatrix(4, 4, (j, k) => (i + 1) * (j + 1) * (k + 1))).ToList()))
            using (var gpuTensor = _cuda.CreateTensor(cpuTensor.AsIndexable()))
            using (var cpuPadding = cpuTensor.AddPadding(1))
            using (var gpuPadding = gpuTensor.AddPadding(1)) {
                FloatingPointHelper.AssertEqual(cpuPadding.AsIndexable(), gpuPadding.AsIndexable());
            }
        }

        [TestMethod]
        public void TensorAddPadding2()
        {
            using (var cpuTensor = _cpu.CreateTensor(Enumerable.Range(0, 3).Select(i => _cpu.CreateMatrix(4, 4, (j, k) => (i + 1) * (j + 1) * (k + 1))).ToList()))
            using (var gpuTensor = _cuda.CreateTensor(cpuTensor.AsIndexable()))
            using (var cpuPadding = cpuTensor.AddPadding(2))
            using (var gpuPadding = gpuTensor.AddPadding(2)) {
                FloatingPointHelper.AssertEqual(cpuPadding.AsIndexable(), gpuPadding.AsIndexable());
            }
        }

        [TestMethod]
        public void TensorIm2Col()
        {
            using (var cpuTensor = _cpu.CreateTensor(Enumerable.Range(0, 3).Select(i => _cpu.CreateMatrix(4, 4, (j, k) => (i + 1) * (j + 1) * (k + 1))).ToList()))
            using (var gpuTensor = _cuda.CreateTensor(cpuTensor.AsIndexable()))
            using (var cpuMatrix = cpuTensor.Im2Col(2, 2, 2))
            using (var gpuMatrix = gpuTensor.Im2Col(2, 2, 2)) {
                var cpu = cpuMatrix.AsIndexable();
                var gpu = gpuMatrix.AsIndexable();
                FloatingPointHelper.AssertEqual(cpu, gpu);
            }
        }

        [TestMethod]
        public void TensorIm2Col2()
        {
            var normalDistribution = new Normal(0, 1);
            using (var cpuTensor = _cpu.CreateTensor(Enumerable.Range(0, 1).Select(i => _cpu.CreateMatrix(8, 8, (j, k) => Convert.ToSingle(normalDistribution.Sample()))).ToList()))
            //using (var cpuTensor = _cpu.CreateTensor(Enumerable.Range(0, 3).Select(i => _cpu.Create(8, 8, (j, k) => (i + 1) * (j + 1) * (k + 1))).ToList()))
            using (var gpuTensor = _cuda.CreateTensor(cpuTensor.AsIndexable()))
            using (var cpuMatrix = cpuTensor.Im2Col(2, 2, 2))
            using (var gpuMatrix = gpuTensor.Im2Col(2, 2, 2)) {
                var cpu = cpuMatrix.AsIndexable();
                var gpu = gpuMatrix.AsIndexable();
                FloatingPointHelper.AssertEqual(cpu, gpu);
            }
        }

        [TestMethod]
        public void TensorRemovePadding()
        {
            // TODO...
        }
    }
}
