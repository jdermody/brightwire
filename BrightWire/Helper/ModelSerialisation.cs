using System.IO;
using BrightData;
using BrightData.LinearAlgebra;
using BrightWire.Models;
using BrightWire.Models.Bayesian;
using BrightWire.Models.InstanceBased;
using BrightWire.Models.TreeBased;
using BrightData.Serialisation;

namespace BrightWire.Helper
{
    internal static class ModelSerialisation
    {
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
            model.Graph = context.Create<ExecutionGraphModel>(reader);
            if (reader.ReadBoolean())
                model.DataSource = context.Create<DataSourceModel>(reader);
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
            model.Graph = context.Create<ExecutionGraphModel>(reader);
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
            model.InputNode = context.Create<ExecutionGraphModel.Node>(reader);
            model.OtherNodes = reader.ReadArray<ExecutionGraphModel.Node>(context);
            model.Wires = reader.ReadArray<ExecutionGraphModel.Wire>(context);
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

        public static void ReadFrom(IBrightDataContext _, BinaryReader reader, ExecutionGraphModel.Node model)
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

        public static void ReadFrom(IBrightDataContext _, BinaryReader reader, ExecutionGraphModel.Wire model)
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
            model.ClassData = reader.ReadArray<BernoulliNaiveBayes.Class>(context);
            model.Vocabulary = reader.ReadStructArray<uint>();
        }

        public static void WriteTo(BernoulliNaiveBayes.StringIndexProbability model, BinaryWriter writer)
        {
            model.StringIndex.WriteTo(writer);
            model.ConditionalProbability.WriteTo(writer);
            model.InverseProbability.WriteTo(writer);
        }

        public static void ReadFrom(IBrightDataContext _, BinaryReader reader, BernoulliNaiveBayes.StringIndexProbability model)
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
            model.Index = reader.ReadArray<BernoulliNaiveBayes.StringIndexProbability>(context);
            model.InverseMissingProbability = reader.ReadDouble();
        }

        public static void WriteTo(MultinomialNaiveBayes model, BinaryWriter writer)
        {
            model.ClassData.WriteTo(writer);
        }

        public static void ReadFrom(IBrightDataContext context, BinaryReader reader, MultinomialNaiveBayes model)
        {
            model.ClassData = reader.ReadArray<MultinomialNaiveBayes.Class>(context);
        }

        public static void WriteTo(MultinomialNaiveBayes.StringIndexProbability model, BinaryWriter writer)
        {
            model.StringIndex.WriteTo(writer);
            model.ConditionalProbability.WriteTo(writer);
        }

        public static void ReadFrom(IBrightDataContext _, BinaryReader reader, MultinomialNaiveBayes.StringIndexProbability model)
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
            model.Index = reader.ReadArray<MultinomialNaiveBayes.StringIndexProbability>(context);
        }

        public static void WriteTo(NaiveBayes model, BinaryWriter writer)
        {
            model.Class.WriteTo(writer);
        }

        public static void ReadFrom(IBrightDataContext context, BinaryReader reader, NaiveBayes model)
        {
            model.Class = reader.ReadArray<NaiveBayes.ClassSummary>(context);
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
            model.Probability = reader.ReadArray<NaiveBayes.CategorialProbability>(context);
        }

        public static void WriteTo(NaiveBayes.CategorialProbability model, BinaryWriter writer)
        {
            model.Category.WriteTo(writer);
            model.LogProbability.WriteTo(writer);
            model.Probability.WriteTo(writer);
        }

        public static void ReadFrom(IBrightDataContext _, BinaryReader reader, NaiveBayes.CategorialProbability model)
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
            model.ColumnSummary = reader.ReadArray<NaiveBayes.Column>(context);
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
            model.Instance = reader.ReadArrayOfArrays<float>();
            model.Classification = reader.ReadStringArray();
            model.DataColumns = reader.ReadStructArray<uint>();
            model.TargetColumn = reader.ReadUInt32();
        }

        //public static void WriteTo(MultinomialLogisticRegression model, BinaryWriter writer)
        //{
        //    model.Model.WriteTo(writer);
        //    model.Classification.WriteTo(writer);
        //    model.FeatureColumn.WriteTo(writer);
        //}

        //public static void ReadFrom(IBrightDataContext context, BinaryReader reader, MultinomialLogisticRegression model)
        //{
        //    model.Model = reader.ReadArray<LogisticRegression>(context);
        //    model.Classification = reader.ReadStringArray();
        //    model.FeatureColumn = reader.ReadStructArray<uint>();
        //}

        public static void WriteTo(DecisionTree model, BinaryWriter writer)
        {
            model.ClassColumnIndex.WriteTo(writer);
            model.Root.WriteTo(writer);
        }

        public static void ReadFrom(IBrightDataContext context, BinaryReader reader, DecisionTree model)
        {
            model.ClassColumnIndex = reader.ReadUInt32();
            model.Root = context.Create<DecisionTree.Node>(reader);
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
            model.Children = reader.ReadArray<DecisionTree.Node>(context);
            model.ColumnIndex = reader.ReadNullable(reader.ReadUInt32);
            model.MatchLabel = reader.ReadString();
            model.Split = reader.ReadNullable(reader.ReadDouble);
            model.Classification = reader.ReadString();
        }

        public static void WriteTo(RandomForest model, BinaryWriter writer)
        {
            model.Forest.WriteTo(writer);
        }

        public static void ReadFrom(IBrightDataContext context, BinaryReader reader, RandomForest model)
        {
            model.Forest = reader.ReadArray<DecisionTree>(context);
        }
    }
}
