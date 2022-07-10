using System;
using System.Collections.Generic;

namespace BrightData.DataTable.Operations
{
    internal class ProjectionOperation : OperationBase<BrightDataTableBuilder?>
    {
        readonly BrightDataContext         _context;
        BrightDataTableBuilder?            _builder;
        readonly IEnumerator<object[]>     _rowIterator;
        readonly Func<object[], object[]?> _projector;

        public ProjectionOperation(uint rowCount, BrightDataContext context, IEnumerator<object[]> rowIterator, Func<object[], object[]?> projector) : base(rowCount, null)
        {
            _rowIterator = rowIterator;
            _projector = projector;
            _context = context;
        }

        protected override void NextStep(uint index)
        {
            _rowIterator.MoveNext();
            var result = _projector(_rowIterator.Current);
            if (result is not null) {
                if (_builder is null) {
                    _builder = new BrightDataTableBuilder(_context);
                    foreach (var item in result)
                        _builder.AddColumn(item.GetType().GetBrightDataType());
                }
                _builder.AddRow(result);
            }
        }

        protected override BrightDataTableBuilder? GetResult(bool wasCancelled)
        {
            return wasCancelled ? null : _builder;
        }
    }
}
