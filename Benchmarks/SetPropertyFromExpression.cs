using BenchmarkDotNet.Attributes;
using System.Linq.Expressions;
using System.Reflection;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace Benchmarks
{
    public class SetPropertyFromExpression
    {
        struct TestStruct
        {
            public int SomeField;

            public readonly override string ToString() => $"{SomeField}";
        }

        [Params(100, 1000, 10000)]
        public int Size { get; set; }

        TestStruct[] _structs;
        int[] _values;

        [GlobalSetup]
        public void SetupData()
        {
            _structs = new TestStruct[Size];
            _values = Enumerable.Range(0, Size).ToArray();
        }
        

        [Benchmark(Baseline = true)]
        public void Direct()
        {
            for(var i = 0; i < _structs.Length; i++)
                _structs[i].SomeField = _values[i];
        }

        [Benchmark]
        public void SetPropertyDirect()
        {
            SetPropertyDirect(_values, _structs, (ref TestStruct obj, int val) => obj.SomeField = val);
        }

        [Benchmark]
        public void ViaSetter()
        {
            SetPropertyViaSetter<TestStruct, int>(_values, _structs, x => x.SomeField);
        }

        [Benchmark]
        public void ViaReflection()
        {
            SetPropertyViaReflection<TestStruct, int>(_values, _structs, x => x.SomeField);
        }

        public delegate void SetPropertyDelegate<T, in PT>(ref T item, PT value);
        public static void SetPropertyViaSetter<T, P>(ReadOnlySpan<P> input, Span<T> output, Expression<Func<T, P>> property) where P : notnull
        {
            var prop = ((MemberExpression)property.Body).Member;
            var typeParam = Expression.Parameter(typeof(T).MakeByRefType());
            var valueParam = Expression.Parameter(typeof(P));
            var setter = Expression.Lambda<SetPropertyDelegate<T, P>>(Expression.Assign(Expression.MakeMemberAccess(typeParam, prop), valueParam), typeParam, valueParam).Compile();

            var index = 0;
            foreach (ref var item in output)
                setter(ref item, input[index++]);
        }

        public static void SetPropertyViaReflection<T, P>(ReadOnlySpan<P> input, Span<T> output, Expression<Func<T, P>> property) where P : notnull
        {
            var prop = (FieldInfo)((MemberExpression)property.Body).Member;

            var index = 0;
            foreach (ref var item in output) {
                var reference = __makeref(item);
                prop.SetValueDirect(reference, input[index++]);
            }
        }

        public static void SetPropertyDirect<T, P>(ReadOnlySpan<P> input, Span<T> output, SetPropertyDelegate<T, P> setProperty) where P : notnull
        {
            var index = 0;
            foreach (ref var item in output)
                setProperty(ref item, input[index++]);
        }
    }
}
