using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Icbld.BrightWire.Models;

namespace Icbld.BrightWire.Connectionist
{
    public class LayerDescriptor : INeuralNetworkLayerDescriptor
    {
        public ActivationType Activation { get; set; }
        public WeightInitialisationType WeightInitialisation { get; set; }
        public RegularisationType Regularisation { get; set; }
        public WeightUpdateType WeightUpdate { get; set; }
        public LayerTrainerType LayerTrainer { get; set; }
        public float Lambda { get; set; } = 0f;
        public float Momentum { get; set; } = 0.9f;
        public float DecayRate { get; set; } = 0.9f;
        public float DecayRate2 { get; set; } = 0.99f;
        public float Dropout { get; set; } = 0.5f;

        public NetworkLayer AsLayer
        {
            get
            {
                return new NetworkLayer {
                    Activation = Activation,
                    WeightInitialisation = WeightInitialisation,
                    Regularisation = Regularisation,
                    WeightUpdate = WeightUpdate,
                    LayerTrainer = LayerTrainer,
                    Lambda = Lambda,
                    Momentum = Momentum,
                    DecayRate = DecayRate,
                    DecayRate2 = DecayRate2,
                    Dropout = Dropout
                };
            }
        }

        public LayerDescriptor(float lambda, ActivationType activation = ActivationType.LeakyRelu, WeightUpdateType update = WeightUpdateType.Adam, LayerTrainerType trainer = LayerTrainerType.Standard)
        {
            Activation = activation;
            WeightInitialisation = WeightInitialisationType.Xavier;
            Regularisation = RegularisationType.L2;
            WeightUpdate = update;
            LayerTrainer = trainer;
            Lambda = lambda;
        }

        private LayerDescriptor() { }

        public INeuralNetworkLayerDescriptor Clone()
        {
            return new LayerDescriptor() {
                Activation = this.Activation,
                WeightInitialisation = this.WeightInitialisation,
                DecayRate = this.DecayRate,
                LayerTrainer = this.LayerTrainer,
                Dropout = this.Dropout,
                Momentum = this.Momentum,
                Regularisation = this.Regularisation,
                WeightUpdate = this.WeightUpdate,
                Lambda = this.Lambda,
                DecayRate2 = this.DecayRate2
            };
        }

        public static LayerDescriptor CreateFrom(NetworkLayer layer)
        {
            return new LayerDescriptor {
                Activation = layer.Activation,
                WeightInitialisation = layer.WeightInitialisation,
                Regularisation = layer.Regularisation,
                WeightUpdate = layer.WeightUpdate,
                LayerTrainer = layer.LayerTrainer,
                Lambda = layer.Lambda,
                Momentum = layer.Momentum,
                DecayRate = layer.DecayRate,
                DecayRate2 = layer.DecayRate2,
                Dropout = layer.Dropout
            };
        }
    }
}
