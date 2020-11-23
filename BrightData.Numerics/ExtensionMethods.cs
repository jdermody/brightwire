namespace BrightData.Numerics
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Creates a linear algebra provider that runs on the CPU
        /// </summary>
        /// <param name="context"></param>
        public static ILinearAlgebraProvider UseNumericsLinearAlgebra(this IBrightDataContext context)
        {
            var ret = new NumericsProvider(context);
            ((ISetLinearAlgebraProvider) context).LinearAlgebraProvider = ret;
            return ret;
        }
    }
}
