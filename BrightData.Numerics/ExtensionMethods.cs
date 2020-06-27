using System;
using System.Collections.Generic;
using System.Text;

namespace BrightData.Numerics
{
    public static class ExtensionMethods
    {
        public static void UseNumericsComputation(this IBrightDataContext context)
        {
            //context.ComputableFactory = new NumericsComputableFactory(context);
        }

        public static IComputableVector AsComputable(this Vector<float> vector, IComputableFactory factory)
        {
            return factory.Create(vector);
        }

        public static IComputableVector CreateComputable(this IBrightDataContext context, IComputableFactory factory, int size, Func<int, float> initializer)
        {
            return factory.Create(size, initializer);
        }

        public static IComputableMatrix AsComputable(this Matrix<float> matrix, IComputableFactory factory)
        {
            return factory.Create(matrix);
        }

        public static IComputableMatrix CreateComputable(this IBrightDataContext context, IComputableFactory factory, int rows, int columns, Func<int, int, float> initializer)
        {
            return factory.Create(rows, columns, initializer);
        }

        public static IComputable3DTensor AsComputable(this Tensor3D<float> tensor, IComputableFactory factory)
        {
            return factory.Create(tensor);
        }

        public static IComputable4DTensor AsComputable(this Tensor4D<float> tensor, IComputableFactory factory)
        {
            return factory.Create(tensor);
        }
    }
}
