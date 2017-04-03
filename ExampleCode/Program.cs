using BrightWire;
using System;

namespace ExampleCode
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var lap = Provider.CreateLinearAlgebra()) {
                var matrix = lap.Create(3, 3, (i, j) => (i+1) * (j+1));
                var rot180 = matrix.Rotate180();

                var xml = matrix.AsIndexable().AsXml;
                var xml2 = rot180.AsIndexable().AsXml;
            }
        }
    }
}