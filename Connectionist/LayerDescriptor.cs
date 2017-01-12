using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightWire.Models;

namespace BrightWire.Connectionist
{
    /// <summary>
    /// Neural network layer descriptor
    /// </summary>
    public class LayerDescriptor
    {
        /// <summary>
        /// The activation type
        /// </summary>
        public ActivationType Activation { get; set; }

        /// <summary>
        /// The weight initialisation strategy
        /// </summary>
        public WeightInitialisationType WeightInitialisation { get; set; }

        /// <summary>
        /// The regularisation type
        /// </summary>
        public RegularisationType Regularisation { get; set; }

        /// <summary>
        /// The gradient descent optimisation technique
        /// </summary>
        public WeightUpdateType WeightUpdate { get; set; }

        /// <summary>
        /// The layer trainer type
        /// </summary>
        public LayerTrainerType LayerTrainer { get; set; }

        /// <summary>
        /// Regularisation parameter
        /// </summary>
        public float Lambda { get; set; } = 0f;

        /// <summary>
        /// Momentum parameter
        /// </summary>
        public float Momentum { get; set; } = 0.9f;

        /// <summary>
        /// Adam and RMSprop parameter
        /// </summary>
        public float DecayRate { get; set; } = 0.9f;

        /// <summary>
        /// Adam parameter 2
        /// </summary>
        public float DecayRate2 { get; set; } = 0.99f;

        /// <summary>
        /// Dropout and drop connect percentage
        /// </summary>
        public float Dropout { get; set; } = 0.5f;

        /// <summary>
        /// Converts the descriptor to a protobuf network layer
        /// </summary>
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

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="lambda">Regularisation parameter</param>
        /// <param name="activation">The activation type</param>
        /// <param name="update">The gradient descent optimisation technique</param>
        /// <param name="trainer">The layer trainer type</param>
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

        /// <summary>
        /// Clones the current descriptor
        /// </summary>
        public LayerDescriptor Clone()
        {
            var ret = new LayerDescriptor();
            CopyTo(ret);
            return ret;
        }

        /// <summary>
        /// Copies attributes from this layer to the target layer
        /// </summary>
        /// <param name="layer">The target layer</param>
        public void CopyTo(LayerDescriptor layer)
        {
            layer.Activation = this.Activation;
            layer.WeightInitialisation = this.WeightInitialisation;
            layer.DecayRate = this.DecayRate;
            layer.LayerTrainer = this.LayerTrainer;
            layer.Dropout = this.Dropout;
            layer.Momentum = this.Momentum;
            layer.Regularisation = this.Regularisation;
            layer.WeightUpdate = this.WeightUpdate;
            layer.Lambda = this.Lambda;
            layer.DecayRate2 = this.DecayRate2;
        }

        /// <summary>
        /// Creates a descriptor from a network layer
        /// </summary>
        /// <param name="layer">The network layer to use as a source</param>
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
