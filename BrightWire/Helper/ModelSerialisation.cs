using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using BrightData;
using BrightWire.Models;
using BrightWire.Models.Bayesian;
using BrightWire.Models.InstanceBased;
using BrightWire.Models.Linear;
using BrightWire.Models.TreeBased;

namespace BrightWire.Helper
{
    internal static class ModelSerialisation
    {
        static void WriteTo(this String str, BinaryWriter writer) => writer.Write(str ?? "");
        static void WriteTo(this int val, BinaryWriter writer) => writer.Write(val);
        static void WriteTo(this uint val, BinaryWriter writer) => writer.Write((int)val);
        static void WriteTo(this double val, BinaryWriter writer) => writer.Write(val);

        static void WriteTo<T>(this T? val, BinaryWriter writer, Action<T> onWrite) where T: struct
        {
            writer.Write(val.HasValue);
            if(val.HasValue)
                onWrite(val.Value);
        }

        static T? ReadNullable<T>(BinaryReader reader, Func<T> onRead) where T : struct
        {
            if (reader.ReadBoolean())
                return onRead();
            return null;
        }

        static void WriteTo(this IReadOnlyCollection<ICanWriteToBinaryWriter> list, BinaryWriter writer)
        {
            writer.Write(list?.Count ?? 0);
            if (list?.Count > 0) {
                foreach (var item in list)
                    item.WriteTo(writer);
            }
        }
        static void WriteTo<T>(this T[] array, BinaryWriter writer) where T : struct
        {
            writer.Write(array?.Length ?? 0);
            if (array?.Length > 0) {
                var bytes = MemoryMarshal.Cast<T, byte>(array);
                writer.Flush();
                writer.BaseStream.Write(bytes);
            }
        }

        static void WriteTo(this string[] array, BinaryWriter writer)
        {
            writer.Write(array?.Length ?? 0);
            if (array?.Length > 0) {
                foreach(var str in array)
                    writer.Write(str);
            }
        }

        public static T Create<T>(IBrightDataContext context, BinaryReader reader)
            where T : ICanInitializeFromBinaryReader
        {
            var ret = (T)FormatterServices.GetUninitializedObject(typeof(T));
            ret.Initialize(context, reader);
            return ret;
        }

        static T[] CreateArray<T>(IBrightDataContext context, BinaryReader reader)
            where T : ICanInitializeFromBinaryReader
        {
            var len = reader.ReadInt32();
            var ret = new T[len];
            for (var i = 0; i < len; i++)
                ret[i] = Create<T>(context, reader);
            return ret;
        }

        static T[] CreateStructArray<T>(IBrightDataContext context, BinaryReader reader)
            where T : struct
        {
            var len = reader.ReadInt32();
            var ret = new T[len];
            var bytes = MemoryMarshal.Cast<T, byte>(ret);
            reader.BaseStream.Read(bytes);
            return ret;
        }

        static string[] CreateStringArray(IBrightDataContext context, BinaryReader reader)
        {
            var len = reader.ReadInt32();
            var ret = new string[len];
            for (var i = 0; i < len; i++)
                ret[i] = reader.ReadString();
            return ret;
        }

        public static void WriteTo(GraphModel model, BinaryWriter writer)
        {
            model.Version.WriteTo(writer);
            model.Name.WriteTo(writer);
            model.Graph.WriteTo(writer);
            writer.Write(model.DataSource != null);
            model.DataSource?.WriteTo(writer);
        }

        public static void ReadFrom(IBrightDataContext context, BinaryReader reader, GraphModel model)
        {
            model.Version = reader.ReadString();
            model.Name = reader.ReadString();
            model.Graph = Create<ExecutionGraphModel>(context, reader);
            if (reader.ReadBoolean())
                model.DataSource = Create<DataSourceModel>(context, reader);
        }

        public static void WriteTo(DataSourceModel model, BinaryWriter writer)
        {
            model.Version.WriteTo(writer);
            model.Name.WriteTo(writer);
            model.InputSize.WriteTo(writer);
            model.OutputSize.WriteTo(writer);
            model.Graph.WriteTo(writer);
        }

        public static void ReadFrom(IBrightDataContext context, BinaryReader reader, DataSourceModel model)
        {
            model.Version = reader.ReadString();
            model.Name = reader.ReadString();
            model.InputSize = (uint)reader.ReadInt32();
            model.OutputSize = (uint)reader.ReadInt32();
            model.Graph = Create<ExecutionGraphModel>(context, reader);
        }

        public static void WriteTo(ExecutionGraphModel model, BinaryWriter writer)
        {
            model.Version.WriteTo(writer);
            model.Name.WriteTo(writer);
            model.InputNode.WriteTo(writer);
            model.OtherNodes.WriteTo(writer);
            model.Wires.WriteTo(writer);
        }

        public static void ReadFrom(IBrightDataContext context, BinaryReader reader, ExecutionGraphModel model)
        {
            model.Version = reader.ReadString();
            model.Name = reader.ReadString();
            model.InputNode = Create<ExecutionGraphModel.Node>(context, reader);
            model.OtherNodes = CreateArray<ExecutionGraphModel.Node>(context, reader);
            model.Wires = CreateArray<ExecutionGraphModel.Wire>(context, reader);
        }

        public static void WriteTo(ExecutionGraphModel.Node model, BinaryWriter writer)
        {
            model.TypeName.WriteTo(writer);
            model.Id.WriteTo(writer);
            model.Name.WriteTo(writer);
            model.Description.WriteTo(writer);
            writer.Write(model.Data?.Length ?? 0);
            if (model.Data?.Length > 0)
                writer.Write(model.Data);
        }

        public static void ReadFrom(IBrightDataContext context, BinaryReader reader, ExecutionGraphModel.Node model)
        {
            model.TypeName = reader.ReadString();
            model.Id = reader.ReadString();
            model.Name = reader.ReadString();
            model.Description = reader.ReadString();
            var len = reader.ReadInt32();
            model.Data = reader.ReadBytes(len);
        }

        public static void WriteTo(ExecutionGraphModel.Wire model, BinaryWriter writer)
        {
            model.FromId.WriteTo(writer);
            model.ToId.WriteTo(writer);
            model.InputChannel.WriteTo(writer);
        }

        public static void ReadFrom(IBrightDataContext context, BinaryReader reader, ExecutionGraphModel.Wire model)
        {
            model.FromId = reader.ReadString();
            model.ToId = reader.ReadString();
            model.InputChannel = (uint)reader.ReadInt32();
        }

        public static void WriteTo(BernoulliNaiveBayes model, BinaryWriter writer)
        {
            model.ClassData.WriteTo(writer);
            model.Vocabulary.WriteTo(writer);
        }

        public static void ReadFrom(IBrightDataContext context, BinaryReader reader, BernoulliNaiveBayes model)
        {
            model.ClassData = CreateArray<BernoulliNaiveBayes.Class>(context, reader);
            model.Vocabulary = CreateStructArray<uint>(context, reader);
        }

        public static void WriteTo(BernoulliNaiveBayes.StringIndexProbability model, BinaryWriter writer)
        {
            model.StringIndex.WriteTo(writer);
            model.ConditionalProbability.WriteTo(writer);
            model.InverseProbability.WriteTo(writer);
        }

        public static void ReadFrom(IBrightDataContext context, BinaryReader reader, BernoulliNaiveBayes.StringIndexProbability model)
        {
            model.StringIndex = reader.ReadUInt32();
            model.ConditionalProbability = reader.ReadDouble();
            model.InverseProbability = reader.ReadDouble();
        }

        public static void WriteTo(BernoulliNaiveBayes.Class model, BinaryWriter writer)
        {
            model.Label.WriteTo(writer);
            model.Prior.WriteTo(writer);
            model.MissingProbability.WriteTo(writer);
            model.Index.WriteTo(writer);
            model.InverseMissingProbability.WriteTo(writer);
        }

        public static void ReadFrom(IBrightDataContext context, BinaryReader reader, BernoulliNaiveBayes.Class model)
        {
            model.Label = reader.ReadString();
            model.Prior = reader.ReadDouble();
            model.MissingProbability = reader.ReadDouble();
            model.Index = CreateArray<BernoulliNaiveBayes.StringIndexProbability>(context, reader);
            model.InverseMissingProbability = reader.ReadDouble();
        }

        public static void WriteTo(MultinomialNaiveBayes model, BinaryWriter writer)
        {
            model.ClassData.WriteTo(writer);
        }

        public static void ReadFrom(IBrightDataContext context, BinaryReader reader, MultinomialNaiveBayes model)
        {
            model.ClassData = CreateArray<MultinomialNaiveBayes.Class>(context, reader);
        }

        public static void WriteTo(MultinomialNaiveBayes.StringIndexProbability model, BinaryWriter writer)
        {
            model.StringIndex.WriteTo(writer);
            model.ConditionalProbability.WriteTo(writer);
        }

        public static void ReadFrom(IBrightDataContext context, BinaryReader reader, MultinomialNaiveBayes.StringIndexProbability model)
        {
            model.StringIndex = reader.ReadUInt32();
            model.ConditionalProbability = reader.ReadDouble();
        }

        public static void WriteTo(MultinomialNaiveBayes.Class model, BinaryWriter writer)
        {
            model.Label.WriteTo(writer);
            model.Prior.WriteTo(writer);
            model.MissingProbability.WriteTo(writer);
            model.Index.WriteTo(writer);
        }

        public static void ReadFrom(IBrightDataContext context, BinaryReader reader, MultinomialNaiveBayes.Class model)
        {
            model.Label = reader.ReadString();
            model.Prior = reader.ReadDouble();
            model.MissingProbability = reader.ReadDouble();
            model.Index = CreateArray<MultinomialNaiveBayes.StringIndexProbability>(context, reader);
        }

        public static void WriteTo(NaiveBayes model, BinaryWriter writer)
        {
            model.Class.WriteTo(writer);
        }

        public static void ReadFrom(IBrightDataContext context, BinaryReader reader, NaiveBayes model)
        {
            model.Class = CreateArray<NaiveBayes.ClassSummary>(context, reader);
        }

        public static void WriteTo(NaiveBayes.Column model, BinaryWriter writer)
        {
            model.ColumnIndex.WriteTo(writer);
            writer.Write((byte)model.Type);
            model.Variance.WriteTo(writer);
            model.Mean.WriteTo(writer);
            model.Probability.WriteTo(writer);
        }

        public static void ReadFrom(IBrightDataContext context, BinaryReader reader, NaiveBayes.Column model)
        {
            model.ColumnIndex = reader.ReadUInt32();
            model.Type = (NaiveBayes.ColumnType)reader.ReadByte();
            model.Variance = reader.ReadDouble();
            model.Mean = reader.ReadDouble();
            model.Probability = CreateArray<NaiveBayes.CategorialProbability>(context, reader);
        }

        public static void WriteTo(NaiveBayes.CategorialProbability model, BinaryWriter writer)
        {
            model.Category.WriteTo(writer);
            model.LogProbability.WriteTo(writer);
            model.Probability.WriteTo(writer);
        }

        public static void ReadFrom(IBrightDataContext context, BinaryReader reader, NaiveBayes.CategorialProbability model)
        {
            model.Category = reader.ReadString();
            model.LogProbability = reader.ReadDouble();
            model.Probability = reader.ReadDouble();
        }

        public static void WriteTo(NaiveBayes.ClassSummary model, BinaryWriter writer)
        {
            model.Label.WriteTo(writer);
            model.LogPrior.WriteTo(writer);
            model.ColumnSummary.WriteTo(writer);
            model.Prior.WriteTo(writer);
        }

        public static void ReadFrom(IBrightDataContext context, BinaryReader reader, NaiveBayes.ClassSummary model)
        {
            model.Label = reader.ReadString();
            model.LogPrior = reader.ReadDouble();
            model.ColumnSummary = CreateArray<NaiveBayes.Column>(context, reader);
            model.Prior = reader.ReadDouble();
        }

        public static void WriteTo(KNearestNeighbours model, BinaryWriter writer)
        {
            model.Instance.WriteTo(writer);
            model.Classification.WriteTo(writer);
            model.DataColumns.WriteTo(writer);
            model.TargetColumn.WriteTo(writer);
        }

        public static void ReadFrom(IBrightDataContext context, BinaryReader reader, KNearestNeighbours model)
        {
            model.Instance = CreateArray<Vector<float>>(context, reader);
            model.Classification = CreateStringArray(context, reader);
            model.DataColumns = CreateStructArray<uint>(context, reader);
            model.TargetColumn = reader.ReadUInt32();
        }

        public static void WriteTo(MultinomialLogisticRegression model, BinaryWriter writer)
        {
            model.Model.WriteTo(writer);
            model.Classification.WriteTo(writer);
            model.FeatureColumn.WriteTo(writer);
        }

        public static void ReadFrom(IBrightDataContext context, BinaryReader reader, MultinomialLogisticRegression model)
        {
            model.Model = CreateArray<LogisticRegression>(context, reader);
            model.Classification = CreateStringArray(context, reader);
            model.FeatureColumn = CreateStructArray<uint>(context, reader);
        }

        public static void WriteTo(DecisionTree model, BinaryWriter writer)
        {
            model.ClassColumnIndex.WriteTo(writer);
            model.Root.WriteTo(writer);
        }

        public static void ReadFrom(IBrightDataContext context, BinaryReader reader, DecisionTree model)
        {
            model.ClassColumnIndex = reader.ReadUInt32();
            model.Root = Create<DecisionTree.Node>(context, reader);
        }

        public static void WriteTo(DecisionTree.Node model, BinaryWriter writer)
        {
            model.Children.WriteTo(writer);
            model.ColumnIndex.WriteTo(writer, writer.Write);
            model.MatchLabel.WriteTo(writer);
            model.Split.WriteTo(writer, writer.Write);
            model.Classification.WriteTo(writer);
        }

        public static void ReadFrom(IBrightDataContext context, BinaryReader reader, DecisionTree.Node model)
        {
            model.Children = CreateArray<DecisionTree.Node>(context, reader);
            model.ColumnIndex = ReadNullable(reader, reader.ReadUInt32);
            model.MatchLabel = reader.ReadString();
            model.Split = ReadNullable(reader, reader.ReadDouble);
            model.Classification = reader.ReadString();
        }

        public static void WriteTo(RandomForest model, BinaryWriter writer)
        {
            model.Forest.WriteTo(writer);
        }

        public static void ReadFrom(IBrightDataContext context, BinaryReader reader, RandomForest model)
        {
            model.Forest = CreateArray<DecisionTree>(context, reader);
        }
    }
}
