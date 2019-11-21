using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightData;
using BrightTable.Builders;
using BrightTable.Helper;

namespace BrightTable.Transformations
{
    class OneHotEncodeCategoricalColumnsTransformation : TableTransformationBase
    {
        readonly bool _writeToMetadata;
        readonly Dictionary<uint, Dictionary<string, int>> _categorial;

        public OneHotEncodeCategoricalColumnsTransformation(bool writeToMetadata, params uint[] columnIndices)
        {
            _writeToMetadata = writeToMetadata;
            _categorial = columnIndices.ToDictionary(i => i, i => new Dictionary<string, int>());
        }

        public IReadOnlyList<OneHotEncodings> ColumnCategories => _categorial
            .OrderBy(kv => kv.Key)
            .Select(kv => new OneHotEncodings(kv.Key, kv.Value.OrderBy(d => d.Value).Select(d => d.Key).ToArray()))
            .ToList();

        protected override ISingleTypeTableSegment Transform(IColumnOrientedDataTable dataTable, uint index, ISingleTypeTableSegment column)
        {
            if (_categorial.TryGetValue(index, out var categoryTable)) {
                var metadata = new MetaData();
                column.MetaData.CopyTo(metadata, Consts.StandardMetaData);

                var (segment, buffer) = _CreateColumn(dataTable.Context, ColumnType.Int, metadata, column.Size);
                uint ind = 0;
                foreach (var item in column.Enumerate()) {
                    var str = item.ToString();
                    if (!categoryTable.TryGetValue(str, out var categoryIndex))
                        categoryTable.Add(str, categoryIndex = categoryTable.Count);
                    buffer.Set(ind++, categoryIndex);
                }

                buffer.Finalise();
                return segment;
            }
            return column;
        }

        internal override void Transform(IRowOrientedDataTable dataTable, RowOrientedTableBuilder builder)
        {
            uint ind = 0;
            var columnMetadata = new List<IMetaData>();
            foreach (var column in dataTable.ColumnTypes.Zip(dataTable.AllMetaData(), (ct, md) => (Type: ct, MetaData: md))) {
                if(_categorial.ContainsKey(ind))
                    columnMetadata.Add(builder.AddColumn(ColumnType.Int, new MetaData(column.MetaData, Consts.Name, Consts.Index, Consts.IsTarget)));
                else
                    columnMetadata.Add(builder.AddColumn(column.Type, column.MetaData));
                ++ind;
            }

            dataTable.ForEachRow((row, index) => {
                foreach (var item in _categorial) {
                    var str = row[item.Key].ToString();
                    if (!item.Value.TryGetValue(str, out var categoryIndex))
                        item.Value.Add(str, categoryIndex = item.Value.Count);
                    row[item.Key] = categoryIndex;
                }

                builder.AddRow(row);
            });

            if (_writeToMetadata) {
                foreach (var item in ColumnCategories) {
                    var metadata = columnMetadata[(int)item.ColumnIndex];
                    uint index = 0;
                    foreach (var category in item.Categories)
                        metadata.Set("category" + index++, category);
                }
            }
        }

        internal override IReadOnlyList<(long Position, long EndOfColumnOffset)> Transform(ColumnOrientedTableBuilder builder)
        {
            throw new System.NotImplementedException();
        }
    }
}
