using BrightWire.Models;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.Connectionist.Training.Manager
{
    internal class RecurrentManagerBase
    {
        protected readonly IReadOnlyList<Tuple<float[], float[]>[]> _testData;
        protected IErrorMetric _errorMetric;

        internal RecurrentManagerBase(IReadOnlyList<Tuple<float[], float[]>[]> testData, IErrorMetric errorMetric)
        {
            _testData = testData;
            _errorMetric = errorMetric;
        }

        protected double _GetScore(IReadOnlyList<Tuple<float[], float[]>[]> data, INeuralNetworkRecurrentBatchTrainer network, float[] memory, IRecurrentTrainingContext context)
        {
            return network.Execute(data, memory, context).SelectMany(d => d).Select(d => _errorMetric.Compute(d.Output, d.Target)).Average();
        }

        protected double _GetScore(IReadOnlyList<Tuple<float[], float[]>[]> data, INeuralNetworkBidirectionalBatchTrainer network, float[] forwardMemory, float[] backwardMemory, IRecurrentTrainingContext context)
        {
            return network.Execute(data, forwardMemory, backwardMemory, context).SelectMany(d => d).Select(d => _errorMetric.Compute(d.Output, d.Target)).Average();
        }

        protected void _AfterEpoch(ITrainingContext context, float[] memory, IReadOnlyList<Tuple<float[], float[]>[]> data, INeuralNetworkRecurrentBatchTrainer network, IRecurrentTrainingContext recurrentContext, ref double bestScore, ref RecurrentNetwork output)
        {
            bool flag = false;
            var score = _GetScore(data, network, memory, recurrentContext);
            if ((_errorMetric.HigherIsBetter && score > bestScore) || (!_errorMetric.HigherIsBetter && score < bestScore)) {
                bestScore = score;
                output = network.NetworkInfo;
                output.Memory = new FloatArray {
                    Data = memory
                };
                flag = true;
            }
            context.WriteScore(score, _errorMetric.DisplayAsPercentage, flag);
        }

        protected void _AfterEpoch(ITrainingContext context, float[] forwardMemory, float[] backwardMemory, IReadOnlyList<Tuple<float[], float[]>[]> data, INeuralNetworkBidirectionalBatchTrainer network, IRecurrentTrainingContext recurrentContext, ref double bestScore, ref BidirectionalNetwork output)
        {
            bool flag = false;
            var score = _GetScore(data, network, forwardMemory, backwardMemory, recurrentContext);
            if ((_errorMetric.HigherIsBetter && score > bestScore) || (!_errorMetric.HigherIsBetter && score < bestScore)) {
                bestScore = score;
                output = network.NetworkInfo;
                output.ForwardMemory = new FloatArray {
                    Data = forwardMemory
                };
                output.BackwardMemory = new FloatArray {
                    Data = backwardMemory
                };
                flag = true;
            }
            context.WriteScore(score, _errorMetric.DisplayAsPercentage, flag);
        }

        protected float[] _Load(INeuralNetworkRecurrentBatchTrainer network, string file, int hiddenLayerSize)
        {
            float[] ret;
            if (File.Exists(file)) {
                using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read)) {
                    var networkInfo = Serializer.Deserialize<RecurrentNetwork>(stream);
                    network.NetworkInfo = networkInfo;
                    return networkInfo.Memory.Data;
                }
            }
            else
                ret = Enumerable.Range(0, hiddenLayerSize).Select(i => 0f).ToArray();
            return ret;
        }

        protected Tuple<float[], float[]> _Load(INeuralNetworkBidirectionalBatchTrainer network, string file, int hiddenLayerSize)
        {
            Tuple<float[], float[]> ret;
            if (File.Exists(file)) {
                using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read)) {
                    var networkInfo = Serializer.Deserialize<BidirectionalNetwork>(stream);
                    network.NetworkInfo = networkInfo;
                    return Tuple.Create(networkInfo.ForwardMemory.Data, networkInfo.BackwardMemory.Data);
                }
            }
            else
                ret = Tuple.Create(Enumerable.Range(0, hiddenLayerSize).Select(i => 0f).ToArray(), Enumerable.Range(0, hiddenLayerSize).Select(i => 0f).ToArray());
            return ret;
        }
    }
}
