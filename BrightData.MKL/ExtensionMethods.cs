namespace BrightData.MKL
{
    /// <summary>
    /// MKL Extension methods
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Configures the bright data context to use MKL linear algebra provider
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static MklLinearAlgebraProvider UseMkl(this BrightDataContext context)
        {
            var ret = new MklLinearAlgebraProvider(context);
            var setLinearAlgebraProvider = (ISetLinearAlgebraProvider)context;
            setLinearAlgebraProvider.LinearAlgebraProvider = ret;
            setLinearAlgebraProvider.LinearAlgebraProviderFactory = () => new MklLinearAlgebraProvider(context);
            return ret;
        }
    }
}
