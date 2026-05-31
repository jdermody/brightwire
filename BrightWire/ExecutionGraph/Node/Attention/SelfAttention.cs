using System;
using System.IO;
using BrightData;
using BrightData.LinearAlgebra;

namespace BrightWire.ExecutionGraph.Node.Attention
{
    /// <summary>
    /// Single-head scaled dot-product self-attention layer (Transformer-style)
    /// https://arxiv.org/abs/1706.03762
    /// 
    /// Forward:
    ///   Q = X * Wq, K = X * Wk, V = X * Wv
    ///   scores = (Q * K^T) / sqrt(d_k)
    ///   weights = softmax(scores)
    ///   attention = weights * V
    ///   output = attention * Wo
    /// </summary>
    internal class SelfAttention : NodeBase
    {
        LinearAlgebraProvider<float> _lap;
        IMatrix<float> _Wq, _Wk, _Wv, _Wo;
        IGradientDescentOptimisation _updaterQ, _updaterK, _updaterV, _updaterO;
        uint _inputSize, _attentionSize, _outputSize;

        /// <summary>
        /// Backpropagation for self-attention
        /// </summary>
        class Backpropagation : SingleBackpropagationBase<SelfAttention>
        {
            readonly IMatrix<float> _Q;
            readonly IMatrix<float> _K;
            readonly IMatrix<float> _V;
            readonly IMatrix<float> _attentionWeights;
            readonly IMatrix<float> _attention;
            readonly float _scale;

            public Backpropagation(
                SelfAttention source,
                IMatrix<float> Q,
                IMatrix<float> K,
                IMatrix<float> V,
                IMatrix<float> attentionWeights,
                IMatrix<float> attention,
                float scale)
                : base(source)
            {
                _Q = Q;
                _K = K;
                _V = V;
                _attentionWeights = attentionWeights;
                _attention = attention;
                _scale = scale;
            }

            protected override void DisposeMemory(bool isDisposing)
            {
                if (isDisposing) {
                    _Q?.Dispose();
                    _K?.Dispose();
                    _V?.Dispose();
                    _attentionWeights?.Dispose();
                    _attention?.Dispose();
                }
            }

            protected override IGraphData Backpropagate(IGraphData errorSignal, IGraphContext context)
            {
                var lap = context.GetLinearAlgebraProvider();
                var learningContext = context.LearningContext!;

                // Do not wrap d_output in 'using' as it may be owned by the errorSignal graph data
                var d_output = errorSignal.GetMatrix();

                // d_attention = d_output * Wo^T
                using var wOT = _source._Wo.Transpose();
                using var d_attention = d_output.Multiply(wOT);

                // d_Wo = attention^T * d_output
                using var attentionT = _attention.Transpose();
                using var d_Wo = attentionT.Multiply(d_output);
                learningContext.AddError(NodeErrorType.SelfAttentionO, _source, d_Wo.Clone());

                // d_weights = d_attention * V^T
                using var vT = _V.Transpose();
                using var d_weights = d_attention.Multiply(vT);

                // d_V = weights^T * d_attention
                using var attentionWeightsT = _attentionWeights.Transpose();
                using var d_V = attentionWeightsT.Multiply(d_attention);
                learningContext.AddError(NodeErrorType.SelfAttentionV, _source, d_V.Clone());

                // Correct Softmax Derivative:
                // d_scores = weights * (d_weights - sum(weights * d_weights, axis=row))
                using var weightsDotD = _attentionWeights.PointwiseMultiply(d_weights);
                using var rowSums = weightsDotD.RowSums();

                // d_weights -= rowSums (broadcast row sums using vectorized SubtractRowVector)
                d_weights.SubtractRowVector(rowSums.Segment);

                // d_scores = attentionWeights * d_weights (element-wise), then scale
                var d_scores = _attentionWeights.PointwiseMultiply(d_weights);
                d_scores.MultiplyInPlace(_scale);

                // d_Q = d_scores * K
                using var d_Q = d_scores.Multiply(_K);
                learningContext.AddError(NodeErrorType.SelfAttentionQ, _source, d_Q.Clone());

                // d_K = d_scores^T * Q
                using var d_scoresT = d_scores.Transpose();
                using var d_K = d_scoresT.Multiply(_Q);
                learningContext.AddError(NodeErrorType.SelfAttentionK, _source, d_K.Clone());

                // Upstream error: d_input = d_Q * Wq^T + d_K * Wk^T + d_V * Wv^T
                using var wqT = _source._Wq.Transpose();
                using var wkT = _source._Wk.Transpose();
                using var wvT = _source._Wv.Transpose();

                using var d_input_Q = d_Q.Multiply(wqT);
                using var d_input_K = d_K.Multiply(wkT);
                using var d_input_V = d_V.Multiply(wvT);

                var d_input = d_input_Q.Clone();
                d_input.AddInPlace(d_input_K);
                d_input.AddInPlace(d_input_V);

                // Manually clean up d_scores as it was not in a using block
                d_scores.Dispose();

                return errorSignal.ReplaceWith(d_input);
            }
        }

        public SelfAttention(
            LinearAlgebraProvider<float> lap,
            uint inputSize,
            uint attentionSize,
            IWeightInitialisation weightInit,
            Func<IMatrix<float>, IGradientDescentOptimisation> updater,
            string? name,
            string? id = null)
            : base(name, id)
        {
            _lap = lap;
            _inputSize = inputSize;
            _attentionSize = attentionSize;
            _outputSize = attentionSize; // output same as attention size

            // Initialize weight matrices
            _Wq = weightInit.CreateWeight(attentionSize, inputSize);
            _Wk = weightInit.CreateWeight(attentionSize, inputSize);
            _Wv = weightInit.CreateWeight(attentionSize, inputSize);
            _Wo = weightInit.CreateWeight(_outputSize, attentionSize);

            _updaterQ = updater(_Wq);
            _updaterK = updater(_Wk);
            _updaterV = updater(_Wv);
            _updaterO = updater(_Wo);
        }

        public uint InputSize => _inputSize;
        public uint AttentionSize => _attentionSize;
        public uint OutputSize => _outputSize;

        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardSingleStep(
            IGraphData signal, uint channel, IGraphContext context, NodeBase? source)
        {
            var input = signal.GetMatrix();
            var scale = 1f / (float)Math.Sqrt(_attentionSize);

            // Project to Q, K, V
            // We DO NOT wrap these in 'using' because ownership will be transferred to the Backpropagation object
            var Q = input.Multiply(_Wq);
            var K = input.Multiply(_Wk);
            var V = input.Multiply(_Wv);

            // scores = Q * K^T / sqrt(d_k)
            // (Uses multiply with transpose to properly map Seq x Dim * Dim x Seq => Seq x Seq)
            using var kT = K.Transpose();
            using var scores = Q.Multiply(kT);
            scores.MultiplyInPlace(scale);

            // Softmax per row
            var softmaxSegments = scores.SoftmaxPerRow();

            // Convert softmax segments back to matrix
            var attentionWeights = _lap.CreateMatrix((uint)softmaxSegments.Length, softmaxSegments[0].Size, (i, j) => softmaxSegments[i][j]);

            // Clean up the intermediate softmax vectors
            foreach (var segment in softmaxSegments) {
                segment.Dispose();
            }

            // attention = weights * V
            var attention = attentionWeights.Multiply(V);

            // output = attention * Wo
            var output = attention.Multiply(_Wo);

            // Backprop object now assumes ownership of Q, K, V, attentionWeights, and attention
            return (this, signal.ReplaceWith(output), () => new Backpropagation(
                this,
                Q,
                K,
                V,
                attentionWeights,
                attention,
                scale)
            );
        }

        public override void ApplyError(NodeErrorType type, ITensor<float> delta, ILearningContext context)
        {
            var d = (IMatrix<float>)delta;
            switch (type) {
                case NodeErrorType.SelfAttentionQ:
                    _updaterQ.Update(_Wq, d, context);
                    break;
                case NodeErrorType.SelfAttentionK:
                    _updaterK.Update(_Wk, d, context);
                    break;
                case NodeErrorType.SelfAttentionV:
                    _updaterV.Update(_Wv, d, context);
                    break;
                case NodeErrorType.SelfAttentionO:
                    _updaterO.Update(_Wo, d, context);
                    break;
                default:
                    throw new NotImplementedException($"Unknown error type {type}");
            }
        }

        protected override void DisposeInternal(bool isDisposing)
        {
            if (isDisposing) {
                _Wq?.Dispose();
                _Wk?.Dispose();
                _Wv?.Dispose();
                _Wo?.Dispose();
            }
        }

        protected override (string Description, byte[] Data) GetInfo()
        {
            return ("SA", WriteData(WriteTo));
        }

        public override void WriteTo(BinaryWriter writer)
        {
            writer.Write((int)_inputSize);
            writer.Write((int)_attentionSize);
            writer.Write((int)_outputSize);
            _Wq.WriteTo(writer);
            _Wk.WriteTo(writer);
            _Wv.WriteTo(writer);
            _Wo.WriteTo(writer);
        }

        public override void ReadFrom(GraphFactory factory, BinaryReader reader)
        {
            _lap = factory.LinearAlgebraProvider;
            _inputSize = reader.ReadUInt32();
            _attentionSize = reader.ReadUInt32();
            _outputSize = reader.ReadUInt32();

            var wq = factory.Context.ReadMatrixFrom(reader);
            var wk = factory.Context.ReadMatrixFrom(reader);
            var wv = factory.Context.ReadMatrixFrom(reader);
            var wo = factory.Context.ReadMatrixFrom(reader);

            if (_Wq == null) _Wq = wq.Create(_lap); else wq.CopyTo(_Wq);
            if (_Wk == null) _Wk = wk.Create(_lap); else wk.CopyTo(_Wk);
            if (_Wv == null) _Wv = wv.Create(_lap); else wv.CopyTo(_Wv);
            if (_Wo == null) _Wo = wo.Create(_lap); else wo.CopyTo(_Wo);

            _updaterQ ??= factory.CreateWeightUpdater(_Wq);
            _updaterK ??= factory.CreateWeightUpdater(_Wk);
            _updaterV ??= factory.CreateWeightUpdater(_Wv);
            _updaterO ??= factory.CreateWeightUpdater(_Wo);
        }
    }
}