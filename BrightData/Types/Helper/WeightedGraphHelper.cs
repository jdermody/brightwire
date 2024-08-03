using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BrightData.Types.Helper
{
    internal static class WeightedGraphHelper
    {
        public delegate ReadOnlySpan<uint> GetNeighboursDelegate(uint nodeIndex);

        public static RAT SearchFixedSize<W, RAT, CAT>(uint q, uint entryPoint, ICalculateNodeWeights<W> distanceCalculator, GetNeighboursDelegate getNeighbours)
            where W : unmanaged, INumber<W>, IMinMaxValue<W>
            where RAT : struct, IFixedSizeSortedArray<uint, W>
            where CAT : struct, IFixedSizeSortedArray<uint, W>
        {
            var visited = new HashSet<uint> { entryPoint };
            var candidates = new CAT();
            var distanceEQ = distanceCalculator.GetWeight(q, entryPoint);
            candidates.TryAdd(entryPoint, distanceEQ);
            var ret = new RAT();
            ret.TryAdd(entryPoint, distanceEQ);

            while (candidates.Size > 0) {
                var c = candidates.RemoveAt(0);
                var f = ret.MaxValue;
                if (distanceCalculator.GetWeight(c, q) > distanceCalculator.GetWeight(f, q))
                    break;

                foreach (var neighbour in getNeighbours(c)) {
                    if(!visited.Add(neighbour))
                        continue;

                    f = ret.MaxValue;
                    if ((distanceEQ = distanceCalculator.GetWeight(neighbour, q)) < distanceCalculator.GetWeight(f, q)) {
                        candidates.TryAdd(neighbour, distanceEQ);
                        ret.TryAdd(neighbour, distanceEQ);
                    }
                }
            }
            return ret;
        }
    }
}
