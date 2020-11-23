using System;
using System.Collections.Generic;
using System.Linq;
using BrightWire.Models;
using BrightWire.TabularData;
using BrightWire.TabularData.Helper;

namespace BrightWire.Source.TabularData.Manipulation
{
    class AverageSummariser: ISummariseRows
    {
	    readonly IHaveColumns _table;
	    readonly RowConverter _converter;
	    readonly List<SummariserBase> _columnSummary;

	    public AverageSummariser(IHaveColumns table, RowConverter converter)
	    {
		    _table = table;
		    _converter = converter;

		    _columnSummary = table.Columns
			    .Select((c, index) => _GetSummariserFor(index, c.Type))
			    .ToList()
			;
	    }

	    SummariserBase _GetSummariserFor(int index, ColumnType columnType)
	    {
		    switch (columnType)
		    {
			    case ColumnType.Byte:
			    case ColumnType.Double:
			    case ColumnType.Float:
			    case ColumnType.Int:
			    case ColumnType.Long:
				    return new NumericSummariser(index);
			    case ColumnType.Date:
				    return new DateSummariser(index);
			    case ColumnType.Boolean:
				    return new FrequencyBasedSummariser<bool>(index);
			    case ColumnType.String:
				    return new FrequencyBasedSummariser<string>(index);
				case ColumnType.IndexList:
					return new IndexListSummariser(index);
			    case ColumnType.WeightedIndexList:
				    return new WeightedIndexListSummariser(index);
			    case ColumnType.Vector:
				    return new VectorSummariser(index);
				case ColumnType.Matrix:
					return new MatrixSummariser(index);
			    case ColumnType.Tensor:
				    return new TensorSummariser(index);
				case ColumnType.Null:
					return new NopSummariser(index);
				default:
					throw new NotImplementedException();
		    }
	    }

	    abstract class SummariserBase
	    {
		    protected readonly int Index;

		    protected SummariserBase(int index)
		    {
			    Index = index;
		    }

		    public abstract void Add(IRow row);
			public abstract object Result { get; }
	    }

		class NopSummariser : SummariserBase
		{
			public NopSummariser(int index) : base(index)
			{
			}

			public override void Add(IRow row)
			{
				// nop
			}

			public override object Result => null;
		}

	    class FrequencyBasedSummariser<T> : SummariserBase
	    {
		    readonly Dictionary<T, int> _frequencyTable = new Dictionary<T, int>();

		    public FrequencyBasedSummariser(int index) : base(index)
		    {
		    }

		    public override void Add(IRow row)
		    {
			    var str = row.GetField<T>(Index);
			    if (_frequencyTable.TryGetValue(str, out var count))
				    _frequencyTable[str] = count + 1;
			    else
				    _frequencyTable.Add(str, 1);
		    }
		    public override object Result => _frequencyTable
			    .OrderByDescending(kv => kv.Value)
			    .Select(kv => kv.Key)
			    .FirstOrDefault()
		    ;
	    }

	    class DateSummariser : SummariserBase
	    {
		    readonly NumericSummariser _numericSummary;

		    public DateSummariser(int index) : base(index)
		    {
			    _numericSummary = new NumericSummariser(-1);
		    }

		    public override void Add(IRow row)
		    {
			    var date = row.GetField<DateTime>(Index);
			    _numericSummary.Add(date.Ticks);
		    }

		    public override object Result
		    {
			    get
			    {
				    var roundedTicks = Convert.ToInt64(Math.Round(_numericSummary.CurrentAverage));
				    return new DateTime(roundedTicks);
			    }
		    }
	    }

	    class NumericSummariser : SummariserBase
	    {
		    double _mean = 0;
		    ulong _total = 0;

		    public NumericSummariser(int index) : base(index)
		    {
		    }

		    public override void Add(IRow row)
		    {
			    Add(row.GetField<double>(Index));
		    }

		    public void Add(double val)
		    {
			    ++_total;
			    _mean += (val - _mean) / _total;
		    }
			public override object Result => _mean;
			public double CurrentAverage => _mean;
	    }

	    class IndexListSummariser : SummariserBase
	    {
		    readonly HashSet<uint> _curr = new HashSet<uint>();

		    public IndexListSummariser(int index) : base(index)
		    {
		    }

		    public override void Add(IRow row)
		    {
			    var indexList = row.GetField<IndexList>(Index);
			    if (indexList?.Index != null)
			    {
				    foreach (var index in indexList.Index)
					    _curr.Add(index);
			    }
		    }

		    public override object Result => IndexList.Create(_curr.OrderBy(v => v).ToArray());
	    }

	    class WeightedIndexListSummariser : SummariserBase
	    {
		    readonly Dictionary<uint, NumericSummariser> _curr = new Dictionary<uint, NumericSummariser>();

		    public WeightedIndexListSummariser(int index) : base(index)
		    {
		    }

		    public override void Add(IRow row)
		    {
			    var weightedIndexList = row.GetField<WeightedIndexList>(Index);
			    if (weightedIndexList?.IndexList != null)
			    {
				    foreach (var item in weightedIndexList.IndexList)
				    {
					    if (_curr.TryGetValue(item.Index, out var numericSummariser))
						    numericSummariser.Add(item.Weight);
					    else
					    {
						    var val = new NumericSummariser(-1);
						    val.Add(item.Weight);
						    _curr.Add(item.Index, val);
					    }
				    }
			    }
		    }

		    public override object Result => WeightedIndexList.Create(_curr.Select(kv => new WeightedIndexList.WeightedIndex
		    {
			    Index = kv.Key,
			    Weight = Convert.ToSingle(kv.Value.CurrentAverage)
		    }).ToArray());
	    }

	    class VectorSummariser : SummariserBase
	    {
		    readonly List<NumericSummariser> _summarisers = new List<NumericSummariser>();

		    public VectorSummariser(int index) : base(index)
		    {
		    }

		    public override void Add(IRow row)
		    {
			    Add(row.GetField<FloatVector>(Index));
		    }

		    public void Add(FloatVector vector)
		    {
			    if (vector.Data == null) 
				    return;

			    if (!_summarisers.Any())
				    _summarisers.AddRange(Enumerable.Range(0, vector.Size).Select(i => new NumericSummariser(i)));
			    var data = vector.Data;
			    for (int i = 0, len = vector.Size; i < len; i++)
				    _summarisers[i].Add(data[i]);
		    }

		    public override object Result
		    {
			    get { return FloatVector.Create(_summarisers.Select(s => Convert.ToSingle(s.CurrentAverage)).ToArray()); }
		    }
	    }

	    class MatrixSummariser : SummariserBase
	    {
		    readonly List<VectorSummariser> _summarisers = new List<VectorSummariser>();

		    public MatrixSummariser(int index) : base(index)
		    {
		    }

		    public override void Add(IRow row)
		    {
			    Add(row.GetField<FloatMatrix>(Index));
		    }

		    public void Add(FloatMatrix matrix)
		    {
			    if (!_summarisers.Any())
				    _summarisers.AddRange(Enumerable.Range(0, matrix.RowCount).Select(i => new VectorSummariser(i)));
			    var rows = matrix.Row;
			    for (int i = 0, len = matrix.RowCount; i < len; i++)
				    _summarisers[i].Add(rows[i]);
		    }

		    public override object Result
		    {
			    get { return FloatMatrix.Create(_summarisers.Select(r => (FloatVector) r.Result).ToArray()); }
		    }
	    }

	    class TensorSummariser : SummariserBase
	    {
		    readonly List<MatrixSummariser> _summarisers = new List<MatrixSummariser>();

		    public TensorSummariser(int index) : base(index)
		    {
		    }

		    public override void Add(IRow row)
		    {
			    var tensor = row.GetField<FloatTensor>(Index);
			    if (!_summarisers.Any())
				    _summarisers.AddRange(Enumerable.Range(0, tensor.Depth).Select(i => new MatrixSummariser(i)));
			    var matrices = tensor.Matrix;
			    for (int i = 0, len = matrices.Length; i < len; i++)
				    _summarisers[i].Add(matrices[i]);
		    }

		    public override object Result
		    {
			    get { return FloatTensor.Create(_summarisers.Select(r => (FloatMatrix) r.Result).ToArray()); }
		    }
	    }

	    public IRow Summarise(IReadOnlyList<IRow> rows)
	    {
		    foreach (var row in rows)
		    {
			    foreach (var summariser in _columnSummary)
				    summariser.Add(row);
		    }

		    return new DataTableRow(_table, _columnSummary.Select(cs => cs.Result).ToList(), _converter);
	    }
    }
}
