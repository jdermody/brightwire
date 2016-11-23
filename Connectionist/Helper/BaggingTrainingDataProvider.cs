using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using MathNet.Numerics.Distributions;

namespace BrightWire.Connectionist.Helper
{
    internal class BaggingTrainingDataProvider : ITrainingDataProvider
    {
        readonly ITrainingDataProvider _dataProvider;
        readonly Func<float[]> _weightFunc;
        readonly Dictionary<int, int> _rowMap = new Dictionary<int, int>();

        public BaggingTrainingDataProvider(ITrainingDataProvider dataProvider, Func<float[]> weightFunc)
        {
            _dataProvider = dataProvider;
            _weightFunc = weightFunc;
        }

        public int Count
        {
            get
            {
                return _dataProvider.Count;
            }
        }

        public int InputSize
        {
            get
            {
                return _dataProvider.InputSize;
            }
        }

        public int OutputSize
        {
            get
            {
                return _dataProvider.OutputSize;
            }
        }

        public IMiniBatch GetTrainingData(IReadOnlyList<int> rows)
        {
            return _dataProvider.GetTrainingData(rows.Select(r => _rowMap[r]).ToList());
        }

        public void StartEpoch()
        {
            var distribution = new Categorical(_weightFunc().Select(d => Convert.ToDouble(d)).ToArray());

            _rowMap.Clear();
            for (int i = 0, len = _dataProvider.Count; i < len; i++)
                _rowMap[i] = distribution.Sample();
        }
    }
}
