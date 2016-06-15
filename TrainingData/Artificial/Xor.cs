using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.TrainingData.Artificial
{
    public static class XorData
    {
        public static IReadOnlyList<Tuple<float[], float[]>> Get()
        {
            return new[] {
                Tuple.Create(new [] { 0.0f, 0.0f }, new [] { 0.0f }),
                Tuple.Create(new [] { 1.0f, 0.0f }, new [] { 1.0f }),
                Tuple.Create(new [] { 0.0f, 1.0f }, new [] { 1.0f }),
                Tuple.Create(new [] { 1.0f, 1.0f }, new [] { 0.0f })
            };
        }
    }
}
