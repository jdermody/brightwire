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
        protected readonly ISequentialTrainingDataProvider _testData;

        internal RecurrentManagerBase(ISequentialTrainingDataProvider testData)
        {
            _testData = testData;
        }

        protected double _GetScore(ISequentialTrainingDataProvider data, INeuralNetworkRecurrentBatchTrainer network, float[] memory, IRecurrentTrainingContext context)
        {
            return network.Execute(data, memory, context).SelectMany(d => d).Select(d => context.TrainingContext.ErrorMetric.Compute(d.Output, d.Target)).Average();
        }

        protected double _GetScore(ISequentialTrainingDataProvider data, INeuralNetworkBidirectionalBatchTrainer network, float[] forwardMemory, float[] backwardMemory, IRecurrentTrainingContext context)
        {
            return network.Execute(data, forwardMemory, backwardMemory, context).SelectMany(d => d).Select(d => context.TrainingContext.ErrorMetric.Compute(d.Output, d.Target)).Average();
        }

        protected bool _CalculateTestScore(ITrainingContext context, float[] memory, ISequentialTrainingDataProvider data, INeuralNetworkRecurrentBatchTrainer network, IRecurrentTrainingContext recurrentContext, ref double bestScore, ref RecurrentNetwork output)
        {
            bool flag = false;
            var score = _GetScore(data, network, memory, recurrentContext);
            var errorMetric = recurrentContext.TrainingContext.ErrorMetric;

            if ((errorMetric.HigherIsBetter && score > bestScore) || (!errorMetric.HigherIsBetter && score < bestScore)) {
                bestScore = score;
                output = network.NetworkInfo;
                output.Memory = new FloatArray {
                    Data = memory
                };
                flag = true;
            }
            context.WriteScore(score, errorMetric.DisplayAsPercentage, flag);
            return flag;
        }

        protected bool _CalculateTestScore(ITrainingContext context, float[] forwardMemory, float[] backwardMemory, ISequentialTrainingDataProvider data, INeuralNetworkBidirectionalBatchTrainer network, IRecurrentTrainingContext recurrentContext, ref double bestScore, ref BidirectionalNetwork output)
        {
            bool flag = false;
            var score = _GetScore(data, network, forwardMemory, backwardMemory, recurrentContext);
            var errorMetric = recurrentContext.TrainingContext.ErrorMetric;

            if ((errorMetric.HigherIsBetter && score > bestScore) || (!errorMetric.HigherIsBetter && score < bestScore)) {
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
            context.WriteScore(score, errorMetric.DisplayAsPercentage, flag);
            return flag;
        }

        protected float[] _Load(INeuralNetworkRecurrentBatchTrainer network, string file, int hiddenLayerSize)
        {
            var ret = Enumerable.Range(0, hiddenLayerSize).Select(i => 0f).ToArray();

            if (File.Exists(file)) {
                using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read)) {
                    var networkInfo = Serializer.Deserialize<RecurrentNetwork>(stream);
                    network.NetworkInfo = networkInfo;
                    Array.Copy(networkInfo.Memory.Data, ret, Math.Min(networkInfo.Memory.Data.Length, ret.Length));
                }
            }
            return ret;
        }

        protected Tuple<float[], float[]> _Load(INeuralNetworkBidirectionalBatchTrainer network, string file, int hiddenLayerSize)
        {
            var forwardMemory = Enumerable.Range(0, hiddenLayerSize).Select(i => 0f).ToArray();
            var backwardMemory = Enumerable.Range(0, hiddenLayerSize).Select(i => 0f).ToArray();

            if (File.Exists(file)) {
                using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read)) {
                    var networkInfo = Serializer.Deserialize<BidirectionalNetwork>(stream);
                    network.NetworkInfo = networkInfo;
                    Array.Copy(networkInfo.ForwardMemory.Data, forwardMemory, Math.Min(networkInfo.ForwardMemory.Data.Length, forwardMemory.Length));
                    Array.Copy(networkInfo.BackwardMemory.Data, backwardMemory, Math.Min(networkInfo.BackwardMemory.Data.Length, backwardMemory.Length));
                }
            }
            return Tuple.Create(forwardMemory, backwardMemory);
        }
    }
}
