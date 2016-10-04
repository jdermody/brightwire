using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightWire.Models;

namespace BrightWire.Connectionist.Training.Layer.Recurrent
{
    // http://arunmallya.github.io/writeups/nn/lstm/index.html
    public class Lstm : RecurrentLayerBase, INeuralNetworkRecurrentLayer
    {
        readonly ILinearAlgebraProvider _lap;
        readonly IActivationFunction _activation;
        readonly INeuralNetworkLayerUpdater _uc, _wc, _ui, _wi, _uf, _wf, _uo, _wo;

        class Backpropagation : INeuralNetworkRecurrentBackpropagation
        {
            readonly IActivationFunction _activation;
            readonly IMatrix _c, _ca, _pc, _o, _a, _i, _f, _ones, _input;
            readonly INeuralNetworkLayerUpdater _uc, _wc, _ui, _wi, _uf, _wf, _uo, _wo;

            public Backpropagation(IActivationFunction activation, IMatrix ones, IMatrix c, IMatrix ca, IMatrix pc, IMatrix o, IMatrix a, IMatrix i, IMatrix f, IMatrix input,
                INeuralNetworkLayerUpdater uc, INeuralNetworkLayerUpdater wc,
                INeuralNetworkLayerUpdater ui, INeuralNetworkLayerUpdater wi,
                INeuralNetworkLayerUpdater uf, INeuralNetworkLayerUpdater wf,
                INeuralNetworkLayerUpdater uo, INeuralNetworkLayerUpdater wo
            )
            {
                _activation = activation;
                _ones = ones;
                _c = c;
                _ca = ca;
                _pc = pc;
                _o = o;
                _a = a;
                _i = i;
                _f = f;
                _input = input;
                _uc = uc; _wc = wc;
                _ui = ui; _wi = wi;
                _uf = uf; _wf = wf;
                _uo = uo; _wo = wo;
            }

            void _Cleanup()
            {
                _ones.Dispose();
                //_c.Dispose();
                _ca.Dispose();
                //_pc.Dispose();
                _o.Dispose();
                _a.Dispose();
                _i.Dispose();
                _f.Dispose();
                //_input.Dispose();
            }

            public IMatrix Execute(IMatrix error, ITrainingContext context, bool calculateOutput, INeuralNetworkUpdateAccumulator updateAccumulator)
            {
                const float NEG_MAX = -1f, POS_MAX = 1f;

                using (var cd = _activation.Derivative(_c, null))
                using (var ad = _activation.Derivative(_a, null)) {

                    using (var i2 = _ones.Subtract(_i))
                    using (var f2 = _ones.Subtract(_f))
                    using (var o2 = _ones.Subtract(_o))

                    using (var errorO = error.PointwiseMultiply(_o))
                    using (var dO = error.PointwiseMultiply(_ca))
                    using (var dC = errorO.PointwiseMultiply(cd)) {
                        // clip the gradients
                        dO.Constrain(NEG_MAX, POS_MAX);
                        dC.Constrain(NEG_MAX, POS_MAX);

                        using (var dI = dC.PointwiseMultiply(_a))
                        using (var dF = dC.PointwiseMultiply(_pc))
                        using (var dA = dC.PointwiseMultiply(_i))
                        using (var dCp = dC.PointwiseMultiply(_f))

                        using (var dIi = dI.PointwiseMultiply(_i))
                        using (var dFf = dF.PointwiseMultiply(_f))
                        using (var dOo = dO.PointwiseMultiply(_o)) {
                            var dA2 = dA.PointwiseMultiply(ad);
                            var dI2 = dIi.PointwiseMultiply(i2);
                            var dF2 = dFf.PointwiseMultiply(f2);
                            var dO2 = dOo.PointwiseMultiply(o2);

                            using (var ui = dI2.TransposeAndMultiply(_ui.Layer.Weight))
                            using (var uf = dF2.TransposeAndMultiply(_uf.Layer.Weight))
                            using (var uo = dO2.TransposeAndMultiply(_uo.Layer.Weight)) {
                                var uc = dA2.TransposeAndMultiply(_uc.Layer.Weight);

                                uc.AddInPlace(ui, 1f, 1f);
                                uc.AddInPlace(uf, 1f, 1f);
                                uc.AddInPlace(uo, 1f, 1f);

                                _Update(dA2, _wc, _uc, updateAccumulator);
                                _Update(dI2, _wi, _ui, updateAccumulator);
                                _Update(dF2, _wf, _uf, updateAccumulator);
                                _Update(dO2, _wo, _uo, updateAccumulator);

                                _Cleanup();
                                return uc;
                            }
                        }
                    }
                }
            }

            void _Update(IMatrix error, INeuralNetworkLayerUpdater w, INeuralNetworkLayerUpdater u, INeuralNetworkUpdateAccumulator updateAccumulator)
            {
                var deltaW = _input.TransposeThisAndMultiply(error);
                var deltaU = _pc.TransposeThisAndMultiply(error);
                updateAccumulator.Record(w, error, deltaW);
                updateAccumulator.Record(u, error.Clone(), deltaU);
            }
        }

        public Lstm(int inputSize, int hiddenSize, INeuralNetworkFactory factory, INeuralNetworkLayerDescriptor template)
        {
            _lap = factory.LinearAlgebraProvider;
            _activation = factory.GetActivation(template.Activation);

            _wc = CreateLayer(inputSize, hiddenSize, factory, template);
            _wi = CreateLayer(inputSize, hiddenSize, factory, template);
            _wf = CreateLayer(inputSize, hiddenSize, factory, template);
            _wo = CreateLayer(inputSize, hiddenSize, factory, template);

            _uc = CreateLayer(hiddenSize, hiddenSize, factory, template);
            _ui = CreateLayer(hiddenSize, hiddenSize, factory, template);
            _uf = CreateLayer(hiddenSize, hiddenSize, factory, template);
            _uo = CreateLayer(hiddenSize, hiddenSize, factory, template);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(disposing) {
                _uc.Dispose();
                _wc.Dispose();
                _ui.Dispose();
                _wi.Dispose();
                _uf.Dispose();
                _wf.Dispose();
                _uo.Dispose();
                _wo.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public bool IsRecurrent { get { return true; } }

        public RecurrentLayer LayerInfo
        {
            get
            {
                return new RecurrentLayer {
                    Activation = _activation.Type,
                    Type = RecurrentLayerType.Lstm,
                    Layer = new[] {
                        _wc.Layer.LayerInfo,
                        _uc.Layer.LayerInfo,

                        _wi.Layer.LayerInfo,
                        _ui.Layer.LayerInfo,

                        _wf.Layer.LayerInfo,
                        _uf.Layer.LayerInfo,

                        _wo.Layer.LayerInfo,
                        _uo.Layer.LayerInfo,
                    }
                };
            }

            set
            {
                _wc.Layer.LayerInfo = value.Layer[0];
                _uc.Layer.LayerInfo = value.Layer[1];

                _wi.Layer.LayerInfo = value.Layer[2];
                _ui.Layer.LayerInfo = value.Layer[3];

                _wf.Layer.LayerInfo = value.Layer[4];
                _uf.Layer.LayerInfo = value.Layer[5];

                _wo.Layer.LayerInfo = value.Layer[6];
                _uo.Layer.LayerInfo = value.Layer[7];
            }
        }

        public INeuralNetworkRecurrentBackpropagation Execute(List<IMatrix> curr, bool backpropagate)
        {
            var input = curr[0];
            var memory = curr[1];

            var a = Combine(input, memory, _wc.Layer, _uc.Layer, m => _activation.Calculate(m));
            var i = Combine(input, memory, _wi.Layer, _ui.Layer, m => m.SigmoidActivation());
            var f = Combine(input, memory, _wf.Layer, _uf.Layer, m => m.SigmoidActivation());
            var o = Combine(input, memory, _wo.Layer, _uo.Layer, m => m.SigmoidActivation());
            using (var f2 = f.PointwiseMultiply(memory)) {
                var ct = a.PointwiseMultiply(i);
                ct.AddInPlace(f2);
                var cta = _activation.Calculate(ct);

                curr[0] = o.PointwiseMultiply(cta);
                curr[1] = ct;

                if (backpropagate) {
                    var ones = _lap.Create(memory.RowCount, memory.ColumnCount, (x, y) => 1f);
                    return new Backpropagation(_activation, ones, ct, cta, memory, o, a, i, f, input, _uc, _wc, _ui, _wi, _uf, _wf, _uo, _wo);
                }
                //memory.Dispose();
                //input.Dispose();
                a.Dispose();
                i.Dispose();
                f.Dispose();
                o.Dispose();
                cta.Dispose();
                return null;
            }
        }
    }
}
