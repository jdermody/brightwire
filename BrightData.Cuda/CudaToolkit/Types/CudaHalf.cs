using System.Runtime.InteropServices;

namespace BrightData.Cuda.CudaToolkit.Types
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct CudaHalf
    {
        ushort x;
        public CudaHalf(float f)
        {
            x = __float2half(f).x;
        }
        public CudaHalf(double d)
        {
            x = __double2half(d).x;
        }
        public CudaHalf(CudaHalf h16)
        {
            x = h16.x;
        }

        static ushort __internal_float2half(float f, out uint remainder)
        {
            var temp = new[] { f };
            var x = new uint[1];
            uint result;
            System.Buffer.BlockCopy(temp, 0, x, 0, sizeof(float));

            var u = (x[0] & 0x7fffffffU);
            var sign = ((x[0] >> 16) & 0x8000U);
            if (u >= 0x7f800000U) {
                remainder = 0U;
                result = ((u == 0x7f800000U) ? (sign | 0x7c00U) : 0x7fffU);
            }
            else if (u > 0x477fefffU) {
                remainder = 0x80000000U;
                result = (sign | 0x7bffU);
            }
            else if (u >= 0x38800000U) {
                remainder = u << 19;
                u -= 0x38000000U;
                result = (sign | (u >> 13));
            }
            else if (u < 0x33000001U) {
                remainder = u;
                result = sign;
            }
            else {
                var exponent = u >> 23;
                var shift = 0x7eU - exponent;
                var mantissa = (u & 0x7fffffU);
                mantissa |= 0x800000U;
                remainder = mantissa << (32 - (int)shift);
                result = (sign | (mantissa >> (int)shift));
            }

            return (ushort)(result);
        }

        static CudaHalf __double2half(double x)
        {
            var ux = new ulong[1];
            var xa = new[] { x };
            System.Buffer.BlockCopy(xa, 0, ux, 0, sizeof(double));

            var absX = (ux[0] & 0x7fffffffffffffffUL);
            if (absX is >= 0x40f0000000000000UL or <= 0x3e60000000000000UL) {
                return __float2half((float)x);
            }
            var shifterBits = ux[0] & 0x7ff0000000000000UL;
            if (absX >= 0x3f10000000000000UL) {
                shifterBits += 42ul << 52;
            }

            else {
                shifterBits = ((42ul - 14 + 1023) << 52);
            }
            shifterBits |= 1ul << 51;
            var shifterBitsArr = new[] { shifterBits };
            var shifter = new double[1];

            System.Buffer.BlockCopy(shifterBitsArr, 0, shifter, 0, sizeof(double));

            var xShiftRound = x + shifter[0];
            var xShiftRoundArr = new[] { xShiftRound };
            var xShiftRoundBits = new ulong[1];

            System.Buffer.BlockCopy(xShiftRoundArr, 0, xShiftRoundBits, 0, sizeof(double));
            xShiftRoundBits[0] &= 0x7ffffffffffffffful;

            System.Buffer.BlockCopy(xShiftRoundBits, 0, xShiftRoundArr, 0, sizeof(double));

            var xRounded = xShiftRound - shifter[0];
            var xRndFlt = (float)xRounded;
            var res = __float2half(xRndFlt);
            return res;
        }

        static CudaHalf __float2half(float a)
        {
            var r = new CudaHalf {
                x = __internal_float2half(a, out var remainder)
            };
            if ((remainder > 0x80000000U) || ((remainder == 0x80000000U) && ((r.x & 0x1U) != 0U))) {
                r.x++;
            }

            return r;
        }
        public override string ToString()
        {
            return x.ToString();
        }
    }
}
