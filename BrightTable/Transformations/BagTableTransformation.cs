using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightData;
using BrightTable.Builders;

namespace BrightTable.Transformations
{
    class BagTableTransformation : TableTransformationBase
    {
        readonly uint _sampleCount;
        readonly int? _randomSeed;
        readonly Dictionary<IDataTable, uint[]> _rowIndices = new Dictionary<IDataTable, uint[]>();

        public BagTableTransformation(uint sampleCount, int? randomSeed)
        {
            _sampleCount = sampleCount;
            _randomSeed = randomSeed;
        }

        protected override (uint ColumnCount, uint RowCount) CalculateNewSize(IDataTable dataTable)
        {
            _rowIndices[dataTable] = dataTable.RowIndices().ToList().Bag(_sampleCount, _randomSeed);
            return (dataTable.ColumnCount, _sampleCount);
        }

        protected override ISingleTypeTableSegment Transform(IColumnOrientedDataTable dataTable, uint index, ISingleTypeTableSegment column)
        {
            throw new NotImplementedException();
            //var metadata = new MetaData();
            //column.MetaData.CopyTo(metadata, Consts.StandardMetaData);
            //var ret = _CreateColumn(dataTable.Context, column.SingleType, metadata, _sampleCount);
            //var rows = _rowIndices[dataTable];

            //var columnData = new object[column.Size];
            //_Process(ret.Segment, (i, value) => columnData[i] = value);

            //for(uint i = 0; i < _sampleCount; i++)
            //    ret.Buffer.Set(i, columnData[rows[i]]);

            //ret.Buffer.Finalise();
            //return ret.Segment;
        }

        internal override void Transform(IRowOrientedDataTable dataTable, RowOrientedTableBuilder builder)
        {
            builder.AddColumnsFrom(dataTable);
            var rows = _rowIndices[dataTable];
            dataTable.ForEachRow(rows, builder.AddRow);
        }

        internal override IReadOnlyList<(long Position, long EndOfColumnOffset)> Transform(ColumnOrientedTableBuilder builder)
        {
            throw new NotImplementedException();
        }
    }
}
