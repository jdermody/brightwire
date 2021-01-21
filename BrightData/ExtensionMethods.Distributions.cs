namespace BrightData
{
    public partial class ExtensionMethods
    {
        /// <summary>
        /// Returns a randomly initialized float
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static float NextRandomFloat(this IBrightDataContext context) => (float)context.Random.NextDouble();

        /// <summary>
        /// Returns a randomly initialized positive number
        /// </summary>
        /// <param name="context"></param>
        /// <param name="length">Exclusive upper bound</param>
        /// <returns></returns>
        public static uint RandomIndex(this IBrightDataContext context, int length) => (uint)context.Random.Next(length);

        /// <summary>
        /// Returns a randomly initialized positive number
        /// </summary>
        /// <param name="context"></param>
        /// <param name="length">Exclusive upper bound</param>
        /// <returns></returns>
        public static uint RandomIndex(this IBrightDataContext context, uint length) => (uint)context.Random.Next((int)length);
    }
}
