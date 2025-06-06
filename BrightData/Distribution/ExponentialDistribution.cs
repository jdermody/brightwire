﻿using System.Numerics;

namespace BrightData.Distribution
{
    internal class ExponentialDistribution<T>(BrightDataContext context, T? lambda) : IContinuousDistribution<T>
        where T : unmanaged, INumber<T>, IBinaryFloatingPointIeee754<T>
    {
        public T Lambda { get; } = lambda ?? T.One;

        public T Sample()
        {
            T r;
            do {
                r = context.NextRandom<T>();
            } while (r == T.Zero);
            return -T.Log(r) * Lambda;
        }
    }
}
