using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.Engine.Helper
{
    class SingleRowDataSource : IDataSource
    {
        readonly float[] _data;
        class Sequence : IMiniBatchSequence
        {
            readonly IMatrix _data;
            readonly IMiniBatch _miniBatch;

            public Sequence(IMatrix data, IMiniBatch miniBatch) { _data = data; _miniBatch = miniBatch; }
            public IMiniBatch MiniBatch => _miniBatch;
            public int SequenceIndex => 0;
            public MiniBatchType Type => MiniBatchType.Standard;
            public IMatrix Input => _data;
            public IMatrix Target => null;
        }
        class MiniBatch : IMiniBatch
        {
            readonly IMiniBatchSequence _sequence;
            readonly IDataSource _dataSource;

            public MiniBatch(IDataSource dataSource, IMatrix data)
            {
                _sequence = new Sequence(data, this);
                _dataSource = dataSource;
            }

            public IReadOnlyList<int> Rows => new[] { 0 };
            public IDataSource DataSource => _dataSource;
            public bool IsSequential => false;
            public int BatchSize => 1;
            public IMiniBatchSequence CurrentSequence => _sequence;
            public bool HasNextSequence => false;
            public int SequenceCount => 0;
            public IMiniBatchSequence GetNextSequence()
            {
                throw new NotImplementedException();
            }
            public IMiniBatchSequence GetSequenceAtIndex(int index)
            {
                throw new NotImplementedException();
            }
        }

        public SingleRowDataSource(float[] data)
        {
            _data = data;
        }

        public bool IsSequential => false;
        public int InputSize => _data.Length;
        public int OutputSize => throw new NotImplementedException();
        public int RowCount => 1;

        public IDataSource CloneWith(IDataTable dataTable)
        {
            throw new NotImplementedException();
        }

        public IMiniBatch Get(IExecutionContext executionContext, IReadOnlyList<int> rows)
        {
            var data = executionContext.LinearAlgebraProvider.CreateVector(_data);
            return new MiniBatch(this, data.ToRowMatrix());
        }

        public IReadOnlyList<IReadOnlyList<int>> GetBuckets()
        {
            return new[] {
                    new [] { 1 }
                };
        }

        public void OnBatchProcessed(IContext context)
        {
            throw new NotImplementedException();
        }
    }
}
