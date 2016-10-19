using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.SampleCode
{
    partial class Program
    {
        static void Main(string[] args)
        {
            IDataTable dataTable = null;
            dataTable = IrisClassification(dataTable);
            dataTable = MultinomialLogisticRegression(dataTable);
            dataTable = KNearestNeighbours(dataTable);

            //MarkovChains();
            //MNIST(@"D:\data\mnist\", @"D:\data\mnist\model.dat");
        }
    }
}
