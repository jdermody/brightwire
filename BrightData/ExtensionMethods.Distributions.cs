using System;
using System.Collections.Generic;
using System.Text;
using BrightData.Distributions;

namespace BrightData
{
    public partial class ExtensionMethods
    {
        public static float NextFloat(this IBrightDataContext context) => (float)context.Random.NextDouble();
        public static uint RandomIndex(this IBrightDataContext context, int length) => (uint)context.Random.Next(length);
        public static uint RandomIndex(this IBrightDataContext context, uint length) => (uint)context.Random.Next((int)length);
    }
}
