using System;
using BrightData.Buffer;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using CommunityToolkit.HighPerformance;

namespace BrightData.Types
{
	/// <summary>
	/// A tuple of spans - each with the same type
	/// </summary>
	public static class SingleTypeSpanTuple
	{
		/// <summary>
		/// Creates a span of tuples (size 2)
		/// </summary>
		public static SingleTypeSpanTuple2<ST> Create<ST>(Span<ST> item1, Span<ST> item2) where ST: unmanaged
		{
			return new(
				item1,
				item2
			);
		}
		/// <summary>
		/// Creates a span of tuples (size 3)
		/// </summary>
		public static SingleTypeSpanTuple3<ST> Create<ST>(Span<ST> item1, Span<ST> item2, Span<ST> item3) where ST: unmanaged
		{
			return new(
				item1,
				item2,
				item3
			);
		}
		/// <summary>
		/// Creates a span of tuples (size 4)
		/// </summary>
		public static SingleTypeSpanTuple4<ST> Create<ST>(Span<ST> item1, Span<ST> item2, Span<ST> item3, Span<ST> item4) where ST: unmanaged
		{
			return new(
				item1,
				item2,
				item3,
				item4
			);
		}
		/// <summary>
		/// Creates a span of tuples (size 5)
		/// </summary>
		public static SingleTypeSpanTuple5<ST> Create<ST>(Span<ST> item1, Span<ST> item2, Span<ST> item3, Span<ST> item4, Span<ST> item5) where ST: unmanaged
		{
			return new(
				item1,
				item2,
				item3,
				item4,
				item5
			);
		}
		/// <summary>
		/// Creates a span of tuples (size 6)
		/// </summary>
		public static SingleTypeSpanTuple6<ST> Create<ST>(Span<ST> item1, Span<ST> item2, Span<ST> item3, Span<ST> item4, Span<ST> item5, Span<ST> item6) where ST: unmanaged
		{
			return new(
				item1,
				item2,
				item3,
				item4,
				item5,
				item6
			);
		}
		/// <summary>
		/// Creates a span of tuples (size 7)
		/// </summary>
		public static SingleTypeSpanTuple7<ST> Create<ST>(Span<ST> item1, Span<ST> item2, Span<ST> item3, Span<ST> item4, Span<ST> item5, Span<ST> item6, Span<ST> item7) where ST: unmanaged
		{
			return new(
				item1,
				item2,
				item3,
				item4,
				item5,
				item6,
				item7
			);
		}
		/// <summary>
		/// Creates a span of tuples (size 8)
		/// </summary>
		public static SingleTypeSpanTuple8<ST> Create<ST>(Span<ST> item1, Span<ST> item2, Span<ST> item3, Span<ST> item4, Span<ST> item5, Span<ST> item6, Span<ST> item7, Span<ST> item8) where ST: unmanaged
		{
			return new(
				item1,
				item2,
				item3,
				item4,
				item5,
				item6,
				item7,
				item8
			);
		}
		/// <summary>
		/// Creates a span of tuples (size 9)
		/// </summary>
		public static SingleTypeSpanTuple9<ST> Create<ST>(Span<ST> item1, Span<ST> item2, Span<ST> item3, Span<ST> item4, Span<ST> item5, Span<ST> item6, Span<ST> item7, Span<ST> item8, Span<ST> item9) where ST: unmanaged
		{
			return new(
				item1,
				item2,
				item3,
				item4,
				item5,
				item6,
				item7,
				item8,
				item9
			);
		}
		/// <summary>
		/// Creates a span of tuples (size 10)
		/// </summary>
		public static SingleTypeSpanTuple10<ST> Create<ST>(Span<ST> item1, Span<ST> item2, Span<ST> item3, Span<ST> item4, Span<ST> item5, Span<ST> item6, Span<ST> item7, Span<ST> item8, Span<ST> item9, Span<ST> item10) where ST: unmanaged
		{
			return new(
				item1,
				item2,
				item3,
				item4,
				item5,
				item6,
				item7,
				item8,
				item9,
				item10
			);
		}
		/// <summary>
		/// Creates a span of tuples (size 11)
		/// </summary>
		public static SingleTypeSpanTuple11<ST> Create<ST>(Span<ST> item1, Span<ST> item2, Span<ST> item3, Span<ST> item4, Span<ST> item5, Span<ST> item6, Span<ST> item7, Span<ST> item8, Span<ST> item9, Span<ST> item10, Span<ST> item11) where ST: unmanaged
		{
			return new(
				item1,
				item2,
				item3,
				item4,
				item5,
				item6,
				item7,
				item8,
				item9,
				item10,
				item11
			);
		}
		/// <summary>
		/// Creates a span of tuples (size 12)
		/// </summary>
		public static SingleTypeSpanTuple12<ST> Create<ST>(Span<ST> item1, Span<ST> item2, Span<ST> item3, Span<ST> item4, Span<ST> item5, Span<ST> item6, Span<ST> item7, Span<ST> item8, Span<ST> item9, Span<ST> item10, Span<ST> item11, Span<ST> item12) where ST: unmanaged
		{
			return new(
				item1,
				item2,
				item3,
				item4,
				item5,
				item6,
				item7,
				item8,
				item9,
				item10,
				item11,
				item12
			);
		}
	}

	/// <summary>
	/// A tuple (size 2) of spans - each with the same type
	/// </summary>
	public readonly ref struct SingleTypeSpanTuple2<ST> : IAmTupleOfSpans where ST: unmanaged
	{
		internal SingleTypeSpanTuple2(Span<ST> item1, Span<ST> item2)
		{
			Item1 = item1;
			Item2 = item2;
		}

		/// <summary>
		/// Item 1 in the tuple
		/// </summary>
		public Span<ST> Item1 { get; }

		/// <summary>
		/// Item 2 in the tuple
		/// </summary>
		public Span<ST> Item2 { get; }

		/// <inheritdoc />
		public int[] Sizes => [
			Item1.Length,
			Item2.Length,
		];

		/// <inheritdoc />
		public int[] ByteSizes => [
			Item1.Length * Unsafe.SizeOf<ST>(),
			Item2.Length * Unsafe.SizeOf<ST>(),
		];

		/// <inheritdoc />
		public void ForEach<T>(ForEachSpanCallback<T> callback) where T: unmanaged 
		{
			callback(MemoryMarshal.Cast<ST, T>(Item1), 0);
			callback(MemoryMarshal.Cast<ST, T>(Item2), 1);
		}
	}
	/// <summary>
	/// A tuple (size 3) of spans - each with the same type
	/// </summary>
	public readonly ref struct SingleTypeSpanTuple3<ST> : IAmTupleOfSpans where ST: unmanaged
	{
		internal SingleTypeSpanTuple3(Span<ST> item1, Span<ST> item2, Span<ST> item3)
		{
			Item1 = item1;
			Item2 = item2;
			Item3 = item3;
		}

		/// <summary>
		/// Item 1 in the tuple
		/// </summary>
		public Span<ST> Item1 { get; }

		/// <summary>
		/// Item 2 in the tuple
		/// </summary>
		public Span<ST> Item2 { get; }

		/// <summary>
		/// Item 3 in the tuple
		/// </summary>
		public Span<ST> Item3 { get; }

		/// <inheritdoc />
		public int[] Sizes => [
			Item1.Length,
			Item2.Length,
			Item3.Length,
		];

		/// <inheritdoc />
		public int[] ByteSizes => [
			Item1.Length * Unsafe.SizeOf<ST>(),
			Item2.Length * Unsafe.SizeOf<ST>(),
			Item3.Length * Unsafe.SizeOf<ST>(),
		];

		/// <inheritdoc />
		public void ForEach<T>(ForEachSpanCallback<T> callback) where T: unmanaged 
		{
			callback(MemoryMarshal.Cast<ST, T>(Item1), 0);
			callback(MemoryMarshal.Cast<ST, T>(Item2), 1);
			callback(MemoryMarshal.Cast<ST, T>(Item3), 2);
		}
	}
	/// <summary>
	/// A tuple (size 4) of spans - each with the same type
	/// </summary>
	public readonly ref struct SingleTypeSpanTuple4<ST> : IAmTupleOfSpans where ST: unmanaged
	{
		internal SingleTypeSpanTuple4(Span<ST> item1, Span<ST> item2, Span<ST> item3, Span<ST> item4)
		{
			Item1 = item1;
			Item2 = item2;
			Item3 = item3;
			Item4 = item4;
		}

		/// <summary>
		/// Item 1 in the tuple
		/// </summary>
		public Span<ST> Item1 { get; }

		/// <summary>
		/// Item 2 in the tuple
		/// </summary>
		public Span<ST> Item2 { get; }

		/// <summary>
		/// Item 3 in the tuple
		/// </summary>
		public Span<ST> Item3 { get; }

		/// <summary>
		/// Item 4 in the tuple
		/// </summary>
		public Span<ST> Item4 { get; }

		/// <inheritdoc />
		public int[] Sizes => [
			Item1.Length,
			Item2.Length,
			Item3.Length,
			Item4.Length,
		];

		/// <inheritdoc />
		public int[] ByteSizes => [
			Item1.Length * Unsafe.SizeOf<ST>(),
			Item2.Length * Unsafe.SizeOf<ST>(),
			Item3.Length * Unsafe.SizeOf<ST>(),
			Item4.Length * Unsafe.SizeOf<ST>(),
		];

		/// <inheritdoc />
		public void ForEach<T>(ForEachSpanCallback<T> callback) where T: unmanaged 
		{
			callback(MemoryMarshal.Cast<ST, T>(Item1), 0);
			callback(MemoryMarshal.Cast<ST, T>(Item2), 1);
			callback(MemoryMarshal.Cast<ST, T>(Item3), 2);
			callback(MemoryMarshal.Cast<ST, T>(Item4), 3);
		}
	}
	/// <summary>
	/// A tuple (size 5) of spans - each with the same type
	/// </summary>
	public readonly ref struct SingleTypeSpanTuple5<ST> : IAmTupleOfSpans where ST: unmanaged
	{
		internal SingleTypeSpanTuple5(Span<ST> item1, Span<ST> item2, Span<ST> item3, Span<ST> item4, Span<ST> item5)
		{
			Item1 = item1;
			Item2 = item2;
			Item3 = item3;
			Item4 = item4;
			Item5 = item5;
		}

		/// <summary>
		/// Item 1 in the tuple
		/// </summary>
		public Span<ST> Item1 { get; }

		/// <summary>
		/// Item 2 in the tuple
		/// </summary>
		public Span<ST> Item2 { get; }

		/// <summary>
		/// Item 3 in the tuple
		/// </summary>
		public Span<ST> Item3 { get; }

		/// <summary>
		/// Item 4 in the tuple
		/// </summary>
		public Span<ST> Item4 { get; }

		/// <summary>
		/// Item 5 in the tuple
		/// </summary>
		public Span<ST> Item5 { get; }

		/// <inheritdoc />
		public int[] Sizes => [
			Item1.Length,
			Item2.Length,
			Item3.Length,
			Item4.Length,
			Item5.Length,
		];

		/// <inheritdoc />
		public int[] ByteSizes => [
			Item1.Length * Unsafe.SizeOf<ST>(),
			Item2.Length * Unsafe.SizeOf<ST>(),
			Item3.Length * Unsafe.SizeOf<ST>(),
			Item4.Length * Unsafe.SizeOf<ST>(),
			Item5.Length * Unsafe.SizeOf<ST>(),
		];

		/// <inheritdoc />
		public void ForEach<T>(ForEachSpanCallback<T> callback) where T: unmanaged 
		{
			callback(MemoryMarshal.Cast<ST, T>(Item1), 0);
			callback(MemoryMarshal.Cast<ST, T>(Item2), 1);
			callback(MemoryMarshal.Cast<ST, T>(Item3), 2);
			callback(MemoryMarshal.Cast<ST, T>(Item4), 3);
			callback(MemoryMarshal.Cast<ST, T>(Item5), 4);
		}
	}
	/// <summary>
	/// A tuple (size 6) of spans - each with the same type
	/// </summary>
	public readonly ref struct SingleTypeSpanTuple6<ST> : IAmTupleOfSpans where ST: unmanaged
	{
		internal SingleTypeSpanTuple6(Span<ST> item1, Span<ST> item2, Span<ST> item3, Span<ST> item4, Span<ST> item5, Span<ST> item6)
		{
			Item1 = item1;
			Item2 = item2;
			Item3 = item3;
			Item4 = item4;
			Item5 = item5;
			Item6 = item6;
		}

		/// <summary>
		/// Item 1 in the tuple
		/// </summary>
		public Span<ST> Item1 { get; }

		/// <summary>
		/// Item 2 in the tuple
		/// </summary>
		public Span<ST> Item2 { get; }

		/// <summary>
		/// Item 3 in the tuple
		/// </summary>
		public Span<ST> Item3 { get; }

		/// <summary>
		/// Item 4 in the tuple
		/// </summary>
		public Span<ST> Item4 { get; }

		/// <summary>
		/// Item 5 in the tuple
		/// </summary>
		public Span<ST> Item5 { get; }

		/// <summary>
		/// Item 6 in the tuple
		/// </summary>
		public Span<ST> Item6 { get; }

		/// <inheritdoc />
		public int[] Sizes => [
			Item1.Length,
			Item2.Length,
			Item3.Length,
			Item4.Length,
			Item5.Length,
			Item6.Length,
		];

		/// <inheritdoc />
		public int[] ByteSizes => [
			Item1.Length * Unsafe.SizeOf<ST>(),
			Item2.Length * Unsafe.SizeOf<ST>(),
			Item3.Length * Unsafe.SizeOf<ST>(),
			Item4.Length * Unsafe.SizeOf<ST>(),
			Item5.Length * Unsafe.SizeOf<ST>(),
			Item6.Length * Unsafe.SizeOf<ST>(),
		];

		/// <inheritdoc />
		public void ForEach<T>(ForEachSpanCallback<T> callback) where T: unmanaged 
		{
			callback(MemoryMarshal.Cast<ST, T>(Item1), 0);
			callback(MemoryMarshal.Cast<ST, T>(Item2), 1);
			callback(MemoryMarshal.Cast<ST, T>(Item3), 2);
			callback(MemoryMarshal.Cast<ST, T>(Item4), 3);
			callback(MemoryMarshal.Cast<ST, T>(Item5), 4);
			callback(MemoryMarshal.Cast<ST, T>(Item6), 5);
		}
	}
	/// <summary>
	/// A tuple (size 7) of spans - each with the same type
	/// </summary>
	public readonly ref struct SingleTypeSpanTuple7<ST> : IAmTupleOfSpans where ST: unmanaged
	{
		internal SingleTypeSpanTuple7(Span<ST> item1, Span<ST> item2, Span<ST> item3, Span<ST> item4, Span<ST> item5, Span<ST> item6, Span<ST> item7)
		{
			Item1 = item1;
			Item2 = item2;
			Item3 = item3;
			Item4 = item4;
			Item5 = item5;
			Item6 = item6;
			Item7 = item7;
		}

		/// <summary>
		/// Item 1 in the tuple
		/// </summary>
		public Span<ST> Item1 { get; }

		/// <summary>
		/// Item 2 in the tuple
		/// </summary>
		public Span<ST> Item2 { get; }

		/// <summary>
		/// Item 3 in the tuple
		/// </summary>
		public Span<ST> Item3 { get; }

		/// <summary>
		/// Item 4 in the tuple
		/// </summary>
		public Span<ST> Item4 { get; }

		/// <summary>
		/// Item 5 in the tuple
		/// </summary>
		public Span<ST> Item5 { get; }

		/// <summary>
		/// Item 6 in the tuple
		/// </summary>
		public Span<ST> Item6 { get; }

		/// <summary>
		/// Item 7 in the tuple
		/// </summary>
		public Span<ST> Item7 { get; }

		/// <inheritdoc />
		public int[] Sizes => [
			Item1.Length,
			Item2.Length,
			Item3.Length,
			Item4.Length,
			Item5.Length,
			Item6.Length,
			Item7.Length,
		];

		/// <inheritdoc />
		public int[] ByteSizes => [
			Item1.Length * Unsafe.SizeOf<ST>(),
			Item2.Length * Unsafe.SizeOf<ST>(),
			Item3.Length * Unsafe.SizeOf<ST>(),
			Item4.Length * Unsafe.SizeOf<ST>(),
			Item5.Length * Unsafe.SizeOf<ST>(),
			Item6.Length * Unsafe.SizeOf<ST>(),
			Item7.Length * Unsafe.SizeOf<ST>(),
		];

		/// <inheritdoc />
		public void ForEach<T>(ForEachSpanCallback<T> callback) where T: unmanaged 
		{
			callback(MemoryMarshal.Cast<ST, T>(Item1), 0);
			callback(MemoryMarshal.Cast<ST, T>(Item2), 1);
			callback(MemoryMarshal.Cast<ST, T>(Item3), 2);
			callback(MemoryMarshal.Cast<ST, T>(Item4), 3);
			callback(MemoryMarshal.Cast<ST, T>(Item5), 4);
			callback(MemoryMarshal.Cast<ST, T>(Item6), 5);
			callback(MemoryMarshal.Cast<ST, T>(Item7), 6);
		}
	}
	/// <summary>
	/// A tuple (size 8) of spans - each with the same type
	/// </summary>
	public readonly ref struct SingleTypeSpanTuple8<ST> : IAmTupleOfSpans where ST: unmanaged
	{
		internal SingleTypeSpanTuple8(Span<ST> item1, Span<ST> item2, Span<ST> item3, Span<ST> item4, Span<ST> item5, Span<ST> item6, Span<ST> item7, Span<ST> item8)
		{
			Item1 = item1;
			Item2 = item2;
			Item3 = item3;
			Item4 = item4;
			Item5 = item5;
			Item6 = item6;
			Item7 = item7;
			Item8 = item8;
		}

		/// <summary>
		/// Item 1 in the tuple
		/// </summary>
		public Span<ST> Item1 { get; }

		/// <summary>
		/// Item 2 in the tuple
		/// </summary>
		public Span<ST> Item2 { get; }

		/// <summary>
		/// Item 3 in the tuple
		/// </summary>
		public Span<ST> Item3 { get; }

		/// <summary>
		/// Item 4 in the tuple
		/// </summary>
		public Span<ST> Item4 { get; }

		/// <summary>
		/// Item 5 in the tuple
		/// </summary>
		public Span<ST> Item5 { get; }

		/// <summary>
		/// Item 6 in the tuple
		/// </summary>
		public Span<ST> Item6 { get; }

		/// <summary>
		/// Item 7 in the tuple
		/// </summary>
		public Span<ST> Item7 { get; }

		/// <summary>
		/// Item 8 in the tuple
		/// </summary>
		public Span<ST> Item8 { get; }

		/// <inheritdoc />
		public int[] Sizes => [
			Item1.Length,
			Item2.Length,
			Item3.Length,
			Item4.Length,
			Item5.Length,
			Item6.Length,
			Item7.Length,
			Item8.Length,
		];

		/// <inheritdoc />
		public int[] ByteSizes => [
			Item1.Length * Unsafe.SizeOf<ST>(),
			Item2.Length * Unsafe.SizeOf<ST>(),
			Item3.Length * Unsafe.SizeOf<ST>(),
			Item4.Length * Unsafe.SizeOf<ST>(),
			Item5.Length * Unsafe.SizeOf<ST>(),
			Item6.Length * Unsafe.SizeOf<ST>(),
			Item7.Length * Unsafe.SizeOf<ST>(),
			Item8.Length * Unsafe.SizeOf<ST>(),
		];

		/// <inheritdoc />
		public void ForEach<T>(ForEachSpanCallback<T> callback) where T: unmanaged 
		{
			callback(MemoryMarshal.Cast<ST, T>(Item1), 0);
			callback(MemoryMarshal.Cast<ST, T>(Item2), 1);
			callback(MemoryMarshal.Cast<ST, T>(Item3), 2);
			callback(MemoryMarshal.Cast<ST, T>(Item4), 3);
			callback(MemoryMarshal.Cast<ST, T>(Item5), 4);
			callback(MemoryMarshal.Cast<ST, T>(Item6), 5);
			callback(MemoryMarshal.Cast<ST, T>(Item7), 6);
			callback(MemoryMarshal.Cast<ST, T>(Item8), 7);
		}
	}
	/// <summary>
	/// A tuple (size 9) of spans - each with the same type
	/// </summary>
	public readonly ref struct SingleTypeSpanTuple9<ST> : IAmTupleOfSpans where ST: unmanaged
	{
		internal SingleTypeSpanTuple9(Span<ST> item1, Span<ST> item2, Span<ST> item3, Span<ST> item4, Span<ST> item5, Span<ST> item6, Span<ST> item7, Span<ST> item8, Span<ST> item9)
		{
			Item1 = item1;
			Item2 = item2;
			Item3 = item3;
			Item4 = item4;
			Item5 = item5;
			Item6 = item6;
			Item7 = item7;
			Item8 = item8;
			Item9 = item9;
		}

		/// <summary>
		/// Item 1 in the tuple
		/// </summary>
		public Span<ST> Item1 { get; }

		/// <summary>
		/// Item 2 in the tuple
		/// </summary>
		public Span<ST> Item2 { get; }

		/// <summary>
		/// Item 3 in the tuple
		/// </summary>
		public Span<ST> Item3 { get; }

		/// <summary>
		/// Item 4 in the tuple
		/// </summary>
		public Span<ST> Item4 { get; }

		/// <summary>
		/// Item 5 in the tuple
		/// </summary>
		public Span<ST> Item5 { get; }

		/// <summary>
		/// Item 6 in the tuple
		/// </summary>
		public Span<ST> Item6 { get; }

		/// <summary>
		/// Item 7 in the tuple
		/// </summary>
		public Span<ST> Item7 { get; }

		/// <summary>
		/// Item 8 in the tuple
		/// </summary>
		public Span<ST> Item8 { get; }

		/// <summary>
		/// Item 9 in the tuple
		/// </summary>
		public Span<ST> Item9 { get; }

		/// <inheritdoc />
		public int[] Sizes => [
			Item1.Length,
			Item2.Length,
			Item3.Length,
			Item4.Length,
			Item5.Length,
			Item6.Length,
			Item7.Length,
			Item8.Length,
			Item9.Length,
		];

		/// <inheritdoc />
		public int[] ByteSizes => [
			Item1.Length * Unsafe.SizeOf<ST>(),
			Item2.Length * Unsafe.SizeOf<ST>(),
			Item3.Length * Unsafe.SizeOf<ST>(),
			Item4.Length * Unsafe.SizeOf<ST>(),
			Item5.Length * Unsafe.SizeOf<ST>(),
			Item6.Length * Unsafe.SizeOf<ST>(),
			Item7.Length * Unsafe.SizeOf<ST>(),
			Item8.Length * Unsafe.SizeOf<ST>(),
			Item9.Length * Unsafe.SizeOf<ST>(),
		];

		/// <inheritdoc />
		public void ForEach<T>(ForEachSpanCallback<T> callback) where T: unmanaged 
		{
			callback(MemoryMarshal.Cast<ST, T>(Item1), 0);
			callback(MemoryMarshal.Cast<ST, T>(Item2), 1);
			callback(MemoryMarshal.Cast<ST, T>(Item3), 2);
			callback(MemoryMarshal.Cast<ST, T>(Item4), 3);
			callback(MemoryMarshal.Cast<ST, T>(Item5), 4);
			callback(MemoryMarshal.Cast<ST, T>(Item6), 5);
			callback(MemoryMarshal.Cast<ST, T>(Item7), 6);
			callback(MemoryMarshal.Cast<ST, T>(Item8), 7);
			callback(MemoryMarshal.Cast<ST, T>(Item9), 8);
		}
	}
	/// <summary>
	/// A tuple (size 10) of spans - each with the same type
	/// </summary>
	public readonly ref struct SingleTypeSpanTuple10<ST> : IAmTupleOfSpans where ST: unmanaged
	{
		internal SingleTypeSpanTuple10(Span<ST> item1, Span<ST> item2, Span<ST> item3, Span<ST> item4, Span<ST> item5, Span<ST> item6, Span<ST> item7, Span<ST> item8, Span<ST> item9, Span<ST> item10)
		{
			Item1 = item1;
			Item2 = item2;
			Item3 = item3;
			Item4 = item4;
			Item5 = item5;
			Item6 = item6;
			Item7 = item7;
			Item8 = item8;
			Item9 = item9;
			Item10 = item10;
		}

		/// <summary>
		/// Item 1 in the tuple
		/// </summary>
		public Span<ST> Item1 { get; }

		/// <summary>
		/// Item 2 in the tuple
		/// </summary>
		public Span<ST> Item2 { get; }

		/// <summary>
		/// Item 3 in the tuple
		/// </summary>
		public Span<ST> Item3 { get; }

		/// <summary>
		/// Item 4 in the tuple
		/// </summary>
		public Span<ST> Item4 { get; }

		/// <summary>
		/// Item 5 in the tuple
		/// </summary>
		public Span<ST> Item5 { get; }

		/// <summary>
		/// Item 6 in the tuple
		/// </summary>
		public Span<ST> Item6 { get; }

		/// <summary>
		/// Item 7 in the tuple
		/// </summary>
		public Span<ST> Item7 { get; }

		/// <summary>
		/// Item 8 in the tuple
		/// </summary>
		public Span<ST> Item8 { get; }

		/// <summary>
		/// Item 9 in the tuple
		/// </summary>
		public Span<ST> Item9 { get; }

		/// <summary>
		/// Item 10 in the tuple
		/// </summary>
		public Span<ST> Item10 { get; }

		/// <inheritdoc />
		public int[] Sizes => [
			Item1.Length,
			Item2.Length,
			Item3.Length,
			Item4.Length,
			Item5.Length,
			Item6.Length,
			Item7.Length,
			Item8.Length,
			Item9.Length,
			Item10.Length,
		];

		/// <inheritdoc />
		public int[] ByteSizes => [
			Item1.Length * Unsafe.SizeOf<ST>(),
			Item2.Length * Unsafe.SizeOf<ST>(),
			Item3.Length * Unsafe.SizeOf<ST>(),
			Item4.Length * Unsafe.SizeOf<ST>(),
			Item5.Length * Unsafe.SizeOf<ST>(),
			Item6.Length * Unsafe.SizeOf<ST>(),
			Item7.Length * Unsafe.SizeOf<ST>(),
			Item8.Length * Unsafe.SizeOf<ST>(),
			Item9.Length * Unsafe.SizeOf<ST>(),
			Item10.Length * Unsafe.SizeOf<ST>(),
		];

		/// <inheritdoc />
		public void ForEach<T>(ForEachSpanCallback<T> callback) where T: unmanaged 
		{
			callback(MemoryMarshal.Cast<ST, T>(Item1), 0);
			callback(MemoryMarshal.Cast<ST, T>(Item2), 1);
			callback(MemoryMarshal.Cast<ST, T>(Item3), 2);
			callback(MemoryMarshal.Cast<ST, T>(Item4), 3);
			callback(MemoryMarshal.Cast<ST, T>(Item5), 4);
			callback(MemoryMarshal.Cast<ST, T>(Item6), 5);
			callback(MemoryMarshal.Cast<ST, T>(Item7), 6);
			callback(MemoryMarshal.Cast<ST, T>(Item8), 7);
			callback(MemoryMarshal.Cast<ST, T>(Item9), 8);
			callback(MemoryMarshal.Cast<ST, T>(Item10), 9);
		}
	}
	/// <summary>
	/// A tuple (size 11) of spans - each with the same type
	/// </summary>
	public readonly ref struct SingleTypeSpanTuple11<ST> : IAmTupleOfSpans where ST: unmanaged
	{
		internal SingleTypeSpanTuple11(Span<ST> item1, Span<ST> item2, Span<ST> item3, Span<ST> item4, Span<ST> item5, Span<ST> item6, Span<ST> item7, Span<ST> item8, Span<ST> item9, Span<ST> item10, Span<ST> item11)
		{
			Item1 = item1;
			Item2 = item2;
			Item3 = item3;
			Item4 = item4;
			Item5 = item5;
			Item6 = item6;
			Item7 = item7;
			Item8 = item8;
			Item9 = item9;
			Item10 = item10;
			Item11 = item11;
		}

		/// <summary>
		/// Item 1 in the tuple
		/// </summary>
		public Span<ST> Item1 { get; }

		/// <summary>
		/// Item 2 in the tuple
		/// </summary>
		public Span<ST> Item2 { get; }

		/// <summary>
		/// Item 3 in the tuple
		/// </summary>
		public Span<ST> Item3 { get; }

		/// <summary>
		/// Item 4 in the tuple
		/// </summary>
		public Span<ST> Item4 { get; }

		/// <summary>
		/// Item 5 in the tuple
		/// </summary>
		public Span<ST> Item5 { get; }

		/// <summary>
		/// Item 6 in the tuple
		/// </summary>
		public Span<ST> Item6 { get; }

		/// <summary>
		/// Item 7 in the tuple
		/// </summary>
		public Span<ST> Item7 { get; }

		/// <summary>
		/// Item 8 in the tuple
		/// </summary>
		public Span<ST> Item8 { get; }

		/// <summary>
		/// Item 9 in the tuple
		/// </summary>
		public Span<ST> Item9 { get; }

		/// <summary>
		/// Item 10 in the tuple
		/// </summary>
		public Span<ST> Item10 { get; }

		/// <summary>
		/// Item 11 in the tuple
		/// </summary>
		public Span<ST> Item11 { get; }

		/// <inheritdoc />
		public int[] Sizes => [
			Item1.Length,
			Item2.Length,
			Item3.Length,
			Item4.Length,
			Item5.Length,
			Item6.Length,
			Item7.Length,
			Item8.Length,
			Item9.Length,
			Item10.Length,
			Item11.Length,
		];

		/// <inheritdoc />
		public int[] ByteSizes => [
			Item1.Length * Unsafe.SizeOf<ST>(),
			Item2.Length * Unsafe.SizeOf<ST>(),
			Item3.Length * Unsafe.SizeOf<ST>(),
			Item4.Length * Unsafe.SizeOf<ST>(),
			Item5.Length * Unsafe.SizeOf<ST>(),
			Item6.Length * Unsafe.SizeOf<ST>(),
			Item7.Length * Unsafe.SizeOf<ST>(),
			Item8.Length * Unsafe.SizeOf<ST>(),
			Item9.Length * Unsafe.SizeOf<ST>(),
			Item10.Length * Unsafe.SizeOf<ST>(),
			Item11.Length * Unsafe.SizeOf<ST>(),
		];

		/// <inheritdoc />
		public void ForEach<T>(ForEachSpanCallback<T> callback) where T: unmanaged 
		{
			callback(MemoryMarshal.Cast<ST, T>(Item1), 0);
			callback(MemoryMarshal.Cast<ST, T>(Item2), 1);
			callback(MemoryMarshal.Cast<ST, T>(Item3), 2);
			callback(MemoryMarshal.Cast<ST, T>(Item4), 3);
			callback(MemoryMarshal.Cast<ST, T>(Item5), 4);
			callback(MemoryMarshal.Cast<ST, T>(Item6), 5);
			callback(MemoryMarshal.Cast<ST, T>(Item7), 6);
			callback(MemoryMarshal.Cast<ST, T>(Item8), 7);
			callback(MemoryMarshal.Cast<ST, T>(Item9), 8);
			callback(MemoryMarshal.Cast<ST, T>(Item10), 9);
			callback(MemoryMarshal.Cast<ST, T>(Item11), 10);
		}
	}
	/// <summary>
	/// A tuple (size 12) of spans - each with the same type
	/// </summary>
	public readonly ref struct SingleTypeSpanTuple12<ST> : IAmTupleOfSpans where ST: unmanaged
	{
		internal SingleTypeSpanTuple12(Span<ST> item1, Span<ST> item2, Span<ST> item3, Span<ST> item4, Span<ST> item5, Span<ST> item6, Span<ST> item7, Span<ST> item8, Span<ST> item9, Span<ST> item10, Span<ST> item11, Span<ST> item12)
		{
			Item1 = item1;
			Item2 = item2;
			Item3 = item3;
			Item4 = item4;
			Item5 = item5;
			Item6 = item6;
			Item7 = item7;
			Item8 = item8;
			Item9 = item9;
			Item10 = item10;
			Item11 = item11;
			Item12 = item12;
		}

		/// <summary>
		/// Item 1 in the tuple
		/// </summary>
		public Span<ST> Item1 { get; }

		/// <summary>
		/// Item 2 in the tuple
		/// </summary>
		public Span<ST> Item2 { get; }

		/// <summary>
		/// Item 3 in the tuple
		/// </summary>
		public Span<ST> Item3 { get; }

		/// <summary>
		/// Item 4 in the tuple
		/// </summary>
		public Span<ST> Item4 { get; }

		/// <summary>
		/// Item 5 in the tuple
		/// </summary>
		public Span<ST> Item5 { get; }

		/// <summary>
		/// Item 6 in the tuple
		/// </summary>
		public Span<ST> Item6 { get; }

		/// <summary>
		/// Item 7 in the tuple
		/// </summary>
		public Span<ST> Item7 { get; }

		/// <summary>
		/// Item 8 in the tuple
		/// </summary>
		public Span<ST> Item8 { get; }

		/// <summary>
		/// Item 9 in the tuple
		/// </summary>
		public Span<ST> Item9 { get; }

		/// <summary>
		/// Item 10 in the tuple
		/// </summary>
		public Span<ST> Item10 { get; }

		/// <summary>
		/// Item 11 in the tuple
		/// </summary>
		public Span<ST> Item11 { get; }

		/// <summary>
		/// Item 12 in the tuple
		/// </summary>
		public Span<ST> Item12 { get; }

		/// <inheritdoc />
		public int[] Sizes => [
			Item1.Length,
			Item2.Length,
			Item3.Length,
			Item4.Length,
			Item5.Length,
			Item6.Length,
			Item7.Length,
			Item8.Length,
			Item9.Length,
			Item10.Length,
			Item11.Length,
			Item12.Length,
		];

		/// <inheritdoc />
		public int[] ByteSizes => [
			Item1.Length * Unsafe.SizeOf<ST>(),
			Item2.Length * Unsafe.SizeOf<ST>(),
			Item3.Length * Unsafe.SizeOf<ST>(),
			Item4.Length * Unsafe.SizeOf<ST>(),
			Item5.Length * Unsafe.SizeOf<ST>(),
			Item6.Length * Unsafe.SizeOf<ST>(),
			Item7.Length * Unsafe.SizeOf<ST>(),
			Item8.Length * Unsafe.SizeOf<ST>(),
			Item9.Length * Unsafe.SizeOf<ST>(),
			Item10.Length * Unsafe.SizeOf<ST>(),
			Item11.Length * Unsafe.SizeOf<ST>(),
			Item12.Length * Unsafe.SizeOf<ST>(),
		];

		/// <inheritdoc />
		public void ForEach<T>(ForEachSpanCallback<T> callback) where T: unmanaged 
		{
			callback(MemoryMarshal.Cast<ST, T>(Item1), 0);
			callback(MemoryMarshal.Cast<ST, T>(Item2), 1);
			callback(MemoryMarshal.Cast<ST, T>(Item3), 2);
			callback(MemoryMarshal.Cast<ST, T>(Item4), 3);
			callback(MemoryMarshal.Cast<ST, T>(Item5), 4);
			callback(MemoryMarshal.Cast<ST, T>(Item6), 5);
			callback(MemoryMarshal.Cast<ST, T>(Item7), 6);
			callback(MemoryMarshal.Cast<ST, T>(Item8), 7);
			callback(MemoryMarshal.Cast<ST, T>(Item9), 8);
			callback(MemoryMarshal.Cast<ST, T>(Item10), 9);
			callback(MemoryMarshal.Cast<ST, T>(Item11), 10);
			callback(MemoryMarshal.Cast<ST, T>(Item12), 11);
		}
	}

	/// <summary>
	/// A tuple of read only spans - each with the same type
	/// </summary>
	public static class ReadOnlySingleTypeSpanTuple
	{
		/// <summary>
		/// Creates a read only span of tuples (size 2)
		/// </summary>
		public static ReadOnlySingleTypeSpanTuple2<ST> Create<ST>(ReadOnlySpan<ST> item1, ReadOnlySpan<ST> item2) where ST: unmanaged
		{
			return new(
				item1,
				item2
			);
		}
		/// <summary>
		/// Creates a read only span of tuples (size 3)
		/// </summary>
		public static ReadOnlySingleTypeSpanTuple3<ST> Create<ST>(ReadOnlySpan<ST> item1, ReadOnlySpan<ST> item2, ReadOnlySpan<ST> item3) where ST: unmanaged
		{
			return new(
				item1,
				item2,
				item3
			);
		}
		/// <summary>
		/// Creates a read only span of tuples (size 4)
		/// </summary>
		public static ReadOnlySingleTypeSpanTuple4<ST> Create<ST>(ReadOnlySpan<ST> item1, ReadOnlySpan<ST> item2, ReadOnlySpan<ST> item3, ReadOnlySpan<ST> item4) where ST: unmanaged
		{
			return new(
				item1,
				item2,
				item3,
				item4
			);
		}
		/// <summary>
		/// Creates a read only span of tuples (size 5)
		/// </summary>
		public static ReadOnlySingleTypeSpanTuple5<ST> Create<ST>(ReadOnlySpan<ST> item1, ReadOnlySpan<ST> item2, ReadOnlySpan<ST> item3, ReadOnlySpan<ST> item4, ReadOnlySpan<ST> item5) where ST: unmanaged
		{
			return new(
				item1,
				item2,
				item3,
				item4,
				item5
			);
		}
		/// <summary>
		/// Creates a read only span of tuples (size 6)
		/// </summary>
		public static ReadOnlySingleTypeSpanTuple6<ST> Create<ST>(ReadOnlySpan<ST> item1, ReadOnlySpan<ST> item2, ReadOnlySpan<ST> item3, ReadOnlySpan<ST> item4, ReadOnlySpan<ST> item5, ReadOnlySpan<ST> item6) where ST: unmanaged
		{
			return new(
				item1,
				item2,
				item3,
				item4,
				item5,
				item6
			);
		}
		/// <summary>
		/// Creates a read only span of tuples (size 7)
		/// </summary>
		public static ReadOnlySingleTypeSpanTuple7<ST> Create<ST>(ReadOnlySpan<ST> item1, ReadOnlySpan<ST> item2, ReadOnlySpan<ST> item3, ReadOnlySpan<ST> item4, ReadOnlySpan<ST> item5, ReadOnlySpan<ST> item6, ReadOnlySpan<ST> item7) where ST: unmanaged
		{
			return new(
				item1,
				item2,
				item3,
				item4,
				item5,
				item6,
				item7
			);
		}
		/// <summary>
		/// Creates a read only span of tuples (size 8)
		/// </summary>
		public static ReadOnlySingleTypeSpanTuple8<ST> Create<ST>(ReadOnlySpan<ST> item1, ReadOnlySpan<ST> item2, ReadOnlySpan<ST> item3, ReadOnlySpan<ST> item4, ReadOnlySpan<ST> item5, ReadOnlySpan<ST> item6, ReadOnlySpan<ST> item7, ReadOnlySpan<ST> item8) where ST: unmanaged
		{
			return new(
				item1,
				item2,
				item3,
				item4,
				item5,
				item6,
				item7,
				item8
			);
		}
		/// <summary>
		/// Creates a read only span of tuples (size 9)
		/// </summary>
		public static ReadOnlySingleTypeSpanTuple9<ST> Create<ST>(ReadOnlySpan<ST> item1, ReadOnlySpan<ST> item2, ReadOnlySpan<ST> item3, ReadOnlySpan<ST> item4, ReadOnlySpan<ST> item5, ReadOnlySpan<ST> item6, ReadOnlySpan<ST> item7, ReadOnlySpan<ST> item8, ReadOnlySpan<ST> item9) where ST: unmanaged
		{
			return new(
				item1,
				item2,
				item3,
				item4,
				item5,
				item6,
				item7,
				item8,
				item9
			);
		}
		/// <summary>
		/// Creates a read only span of tuples (size 10)
		/// </summary>
		public static ReadOnlySingleTypeSpanTuple10<ST> Create<ST>(ReadOnlySpan<ST> item1, ReadOnlySpan<ST> item2, ReadOnlySpan<ST> item3, ReadOnlySpan<ST> item4, ReadOnlySpan<ST> item5, ReadOnlySpan<ST> item6, ReadOnlySpan<ST> item7, ReadOnlySpan<ST> item8, ReadOnlySpan<ST> item9, ReadOnlySpan<ST> item10) where ST: unmanaged
		{
			return new(
				item1,
				item2,
				item3,
				item4,
				item5,
				item6,
				item7,
				item8,
				item9,
				item10
			);
		}
		/// <summary>
		/// Creates a read only span of tuples (size 11)
		/// </summary>
		public static ReadOnlySingleTypeSpanTuple11<ST> Create<ST>(ReadOnlySpan<ST> item1, ReadOnlySpan<ST> item2, ReadOnlySpan<ST> item3, ReadOnlySpan<ST> item4, ReadOnlySpan<ST> item5, ReadOnlySpan<ST> item6, ReadOnlySpan<ST> item7, ReadOnlySpan<ST> item8, ReadOnlySpan<ST> item9, ReadOnlySpan<ST> item10, ReadOnlySpan<ST> item11) where ST: unmanaged
		{
			return new(
				item1,
				item2,
				item3,
				item4,
				item5,
				item6,
				item7,
				item8,
				item9,
				item10,
				item11
			);
		}
		/// <summary>
		/// Creates a read only span of tuples (size 12)
		/// </summary>
		public static ReadOnlySingleTypeSpanTuple12<ST> Create<ST>(ReadOnlySpan<ST> item1, ReadOnlySpan<ST> item2, ReadOnlySpan<ST> item3, ReadOnlySpan<ST> item4, ReadOnlySpan<ST> item5, ReadOnlySpan<ST> item6, ReadOnlySpan<ST> item7, ReadOnlySpan<ST> item8, ReadOnlySpan<ST> item9, ReadOnlySpan<ST> item10, ReadOnlySpan<ST> item11, ReadOnlySpan<ST> item12) where ST: unmanaged
		{
			return new(
				item1,
				item2,
				item3,
				item4,
				item5,
				item6,
				item7,
				item8,
				item9,
				item10,
				item11,
				item12
			);
		}
	}

	/// <summary>
	/// A tuple (size 2) of read only spans - each with the same type
	/// </summary>
	public readonly ref struct ReadOnlySingleTypeSpanTuple2<ST> : IAmTupleOfSpans where ST: unmanaged
	{
		internal ReadOnlySingleTypeSpanTuple2(ReadOnlySpan<ST> item1, ReadOnlySpan<ST> item2)
		{
			Item1 = item1;
			Item2 = item2;
		}

		/// <summary>
		/// Item 1 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item1 { get; }

		/// <summary>
		/// Item 2 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item2 { get; }

		/// <inheritdoc />
		public int[] ByteSizes => [
			Item1.Length * Unsafe.SizeOf<ST>(),
			Item2.Length * Unsafe.SizeOf<ST>(),
		];

		/// <inheritdoc />
		public int[] Sizes => [
			Item1.Length,
			Item2.Length,
		];

		/// <inheritdoc />
		public void ForEach<T>(ForEachSpanCallback<T> callback) where T: unmanaged 
		{
			callback(MemoryMarshal.Cast<ST, T>(Item1), 0);
			callback(MemoryMarshal.Cast<ST, T>(Item2), 1);
		}
	}
	/// <summary>
	/// A tuple (size 3) of read only spans - each with the same type
	/// </summary>
	public readonly ref struct ReadOnlySingleTypeSpanTuple3<ST> : IAmTupleOfSpans where ST: unmanaged
	{
		internal ReadOnlySingleTypeSpanTuple3(ReadOnlySpan<ST> item1, ReadOnlySpan<ST> item2, ReadOnlySpan<ST> item3)
		{
			Item1 = item1;
			Item2 = item2;
			Item3 = item3;
		}

		/// <summary>
		/// Item 1 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item1 { get; }

		/// <summary>
		/// Item 2 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item2 { get; }

		/// <summary>
		/// Item 3 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item3 { get; }

		/// <inheritdoc />
		public int[] ByteSizes => [
			Item1.Length * Unsafe.SizeOf<ST>(),
			Item2.Length * Unsafe.SizeOf<ST>(),
			Item3.Length * Unsafe.SizeOf<ST>(),
		];

		/// <inheritdoc />
		public int[] Sizes => [
			Item1.Length,
			Item2.Length,
			Item3.Length,
		];

		/// <inheritdoc />
		public void ForEach<T>(ForEachSpanCallback<T> callback) where T: unmanaged 
		{
			callback(MemoryMarshal.Cast<ST, T>(Item1), 0);
			callback(MemoryMarshal.Cast<ST, T>(Item2), 1);
			callback(MemoryMarshal.Cast<ST, T>(Item3), 2);
		}
	}
	/// <summary>
	/// A tuple (size 4) of read only spans - each with the same type
	/// </summary>
	public readonly ref struct ReadOnlySingleTypeSpanTuple4<ST> : IAmTupleOfSpans where ST: unmanaged
	{
		internal ReadOnlySingleTypeSpanTuple4(ReadOnlySpan<ST> item1, ReadOnlySpan<ST> item2, ReadOnlySpan<ST> item3, ReadOnlySpan<ST> item4)
		{
			Item1 = item1;
			Item2 = item2;
			Item3 = item3;
			Item4 = item4;
		}

		/// <summary>
		/// Item 1 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item1 { get; }

		/// <summary>
		/// Item 2 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item2 { get; }

		/// <summary>
		/// Item 3 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item3 { get; }

		/// <summary>
		/// Item 4 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item4 { get; }

		/// <inheritdoc />
		public int[] ByteSizes => [
			Item1.Length * Unsafe.SizeOf<ST>(),
			Item2.Length * Unsafe.SizeOf<ST>(),
			Item3.Length * Unsafe.SizeOf<ST>(),
			Item4.Length * Unsafe.SizeOf<ST>(),
		];

		/// <inheritdoc />
		public int[] Sizes => [
			Item1.Length,
			Item2.Length,
			Item3.Length,
			Item4.Length,
		];

		/// <inheritdoc />
		public void ForEach<T>(ForEachSpanCallback<T> callback) where T: unmanaged 
		{
			callback(MemoryMarshal.Cast<ST, T>(Item1), 0);
			callback(MemoryMarshal.Cast<ST, T>(Item2), 1);
			callback(MemoryMarshal.Cast<ST, T>(Item3), 2);
			callback(MemoryMarshal.Cast<ST, T>(Item4), 3);
		}
	}
	/// <summary>
	/// A tuple (size 5) of read only spans - each with the same type
	/// </summary>
	public readonly ref struct ReadOnlySingleTypeSpanTuple5<ST> : IAmTupleOfSpans where ST: unmanaged
	{
		internal ReadOnlySingleTypeSpanTuple5(ReadOnlySpan<ST> item1, ReadOnlySpan<ST> item2, ReadOnlySpan<ST> item3, ReadOnlySpan<ST> item4, ReadOnlySpan<ST> item5)
		{
			Item1 = item1;
			Item2 = item2;
			Item3 = item3;
			Item4 = item4;
			Item5 = item5;
		}

		/// <summary>
		/// Item 1 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item1 { get; }

		/// <summary>
		/// Item 2 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item2 { get; }

		/// <summary>
		/// Item 3 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item3 { get; }

		/// <summary>
		/// Item 4 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item4 { get; }

		/// <summary>
		/// Item 5 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item5 { get; }

		/// <inheritdoc />
		public int[] ByteSizes => [
			Item1.Length * Unsafe.SizeOf<ST>(),
			Item2.Length * Unsafe.SizeOf<ST>(),
			Item3.Length * Unsafe.SizeOf<ST>(),
			Item4.Length * Unsafe.SizeOf<ST>(),
			Item5.Length * Unsafe.SizeOf<ST>(),
		];

		/// <inheritdoc />
		public int[] Sizes => [
			Item1.Length,
			Item2.Length,
			Item3.Length,
			Item4.Length,
			Item5.Length,
		];

		/// <inheritdoc />
		public void ForEach<T>(ForEachSpanCallback<T> callback) where T: unmanaged 
		{
			callback(MemoryMarshal.Cast<ST, T>(Item1), 0);
			callback(MemoryMarshal.Cast<ST, T>(Item2), 1);
			callback(MemoryMarshal.Cast<ST, T>(Item3), 2);
			callback(MemoryMarshal.Cast<ST, T>(Item4), 3);
			callback(MemoryMarshal.Cast<ST, T>(Item5), 4);
		}
	}
	/// <summary>
	/// A tuple (size 6) of read only spans - each with the same type
	/// </summary>
	public readonly ref struct ReadOnlySingleTypeSpanTuple6<ST> : IAmTupleOfSpans where ST: unmanaged
	{
		internal ReadOnlySingleTypeSpanTuple6(ReadOnlySpan<ST> item1, ReadOnlySpan<ST> item2, ReadOnlySpan<ST> item3, ReadOnlySpan<ST> item4, ReadOnlySpan<ST> item5, ReadOnlySpan<ST> item6)
		{
			Item1 = item1;
			Item2 = item2;
			Item3 = item3;
			Item4 = item4;
			Item5 = item5;
			Item6 = item6;
		}

		/// <summary>
		/// Item 1 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item1 { get; }

		/// <summary>
		/// Item 2 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item2 { get; }

		/// <summary>
		/// Item 3 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item3 { get; }

		/// <summary>
		/// Item 4 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item4 { get; }

		/// <summary>
		/// Item 5 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item5 { get; }

		/// <summary>
		/// Item 6 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item6 { get; }

		/// <inheritdoc />
		public int[] ByteSizes => [
			Item1.Length * Unsafe.SizeOf<ST>(),
			Item2.Length * Unsafe.SizeOf<ST>(),
			Item3.Length * Unsafe.SizeOf<ST>(),
			Item4.Length * Unsafe.SizeOf<ST>(),
			Item5.Length * Unsafe.SizeOf<ST>(),
			Item6.Length * Unsafe.SizeOf<ST>(),
		];

		/// <inheritdoc />
		public int[] Sizes => [
			Item1.Length,
			Item2.Length,
			Item3.Length,
			Item4.Length,
			Item5.Length,
			Item6.Length,
		];

		/// <inheritdoc />
		public void ForEach<T>(ForEachSpanCallback<T> callback) where T: unmanaged 
		{
			callback(MemoryMarshal.Cast<ST, T>(Item1), 0);
			callback(MemoryMarshal.Cast<ST, T>(Item2), 1);
			callback(MemoryMarshal.Cast<ST, T>(Item3), 2);
			callback(MemoryMarshal.Cast<ST, T>(Item4), 3);
			callback(MemoryMarshal.Cast<ST, T>(Item5), 4);
			callback(MemoryMarshal.Cast<ST, T>(Item6), 5);
		}
	}
	/// <summary>
	/// A tuple (size 7) of read only spans - each with the same type
	/// </summary>
	public readonly ref struct ReadOnlySingleTypeSpanTuple7<ST> : IAmTupleOfSpans where ST: unmanaged
	{
		internal ReadOnlySingleTypeSpanTuple7(ReadOnlySpan<ST> item1, ReadOnlySpan<ST> item2, ReadOnlySpan<ST> item3, ReadOnlySpan<ST> item4, ReadOnlySpan<ST> item5, ReadOnlySpan<ST> item6, ReadOnlySpan<ST> item7)
		{
			Item1 = item1;
			Item2 = item2;
			Item3 = item3;
			Item4 = item4;
			Item5 = item5;
			Item6 = item6;
			Item7 = item7;
		}

		/// <summary>
		/// Item 1 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item1 { get; }

		/// <summary>
		/// Item 2 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item2 { get; }

		/// <summary>
		/// Item 3 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item3 { get; }

		/// <summary>
		/// Item 4 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item4 { get; }

		/// <summary>
		/// Item 5 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item5 { get; }

		/// <summary>
		/// Item 6 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item6 { get; }

		/// <summary>
		/// Item 7 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item7 { get; }

		/// <inheritdoc />
		public int[] ByteSizes => [
			Item1.Length * Unsafe.SizeOf<ST>(),
			Item2.Length * Unsafe.SizeOf<ST>(),
			Item3.Length * Unsafe.SizeOf<ST>(),
			Item4.Length * Unsafe.SizeOf<ST>(),
			Item5.Length * Unsafe.SizeOf<ST>(),
			Item6.Length * Unsafe.SizeOf<ST>(),
			Item7.Length * Unsafe.SizeOf<ST>(),
		];

		/// <inheritdoc />
		public int[] Sizes => [
			Item1.Length,
			Item2.Length,
			Item3.Length,
			Item4.Length,
			Item5.Length,
			Item6.Length,
			Item7.Length,
		];

		/// <inheritdoc />
		public void ForEach<T>(ForEachSpanCallback<T> callback) where T: unmanaged 
		{
			callback(MemoryMarshal.Cast<ST, T>(Item1), 0);
			callback(MemoryMarshal.Cast<ST, T>(Item2), 1);
			callback(MemoryMarshal.Cast<ST, T>(Item3), 2);
			callback(MemoryMarshal.Cast<ST, T>(Item4), 3);
			callback(MemoryMarshal.Cast<ST, T>(Item5), 4);
			callback(MemoryMarshal.Cast<ST, T>(Item6), 5);
			callback(MemoryMarshal.Cast<ST, T>(Item7), 6);
		}
	}
	/// <summary>
	/// A tuple (size 8) of read only spans - each with the same type
	/// </summary>
	public readonly ref struct ReadOnlySingleTypeSpanTuple8<ST> : IAmTupleOfSpans where ST: unmanaged
	{
		internal ReadOnlySingleTypeSpanTuple8(ReadOnlySpan<ST> item1, ReadOnlySpan<ST> item2, ReadOnlySpan<ST> item3, ReadOnlySpan<ST> item4, ReadOnlySpan<ST> item5, ReadOnlySpan<ST> item6, ReadOnlySpan<ST> item7, ReadOnlySpan<ST> item8)
		{
			Item1 = item1;
			Item2 = item2;
			Item3 = item3;
			Item4 = item4;
			Item5 = item5;
			Item6 = item6;
			Item7 = item7;
			Item8 = item8;
		}

		/// <summary>
		/// Item 1 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item1 { get; }

		/// <summary>
		/// Item 2 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item2 { get; }

		/// <summary>
		/// Item 3 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item3 { get; }

		/// <summary>
		/// Item 4 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item4 { get; }

		/// <summary>
		/// Item 5 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item5 { get; }

		/// <summary>
		/// Item 6 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item6 { get; }

		/// <summary>
		/// Item 7 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item7 { get; }

		/// <summary>
		/// Item 8 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item8 { get; }

		/// <inheritdoc />
		public int[] ByteSizes => [
			Item1.Length * Unsafe.SizeOf<ST>(),
			Item2.Length * Unsafe.SizeOf<ST>(),
			Item3.Length * Unsafe.SizeOf<ST>(),
			Item4.Length * Unsafe.SizeOf<ST>(),
			Item5.Length * Unsafe.SizeOf<ST>(),
			Item6.Length * Unsafe.SizeOf<ST>(),
			Item7.Length * Unsafe.SizeOf<ST>(),
			Item8.Length * Unsafe.SizeOf<ST>(),
		];

		/// <inheritdoc />
		public int[] Sizes => [
			Item1.Length,
			Item2.Length,
			Item3.Length,
			Item4.Length,
			Item5.Length,
			Item6.Length,
			Item7.Length,
			Item8.Length,
		];

		/// <inheritdoc />
		public void ForEach<T>(ForEachSpanCallback<T> callback) where T: unmanaged 
		{
			callback(MemoryMarshal.Cast<ST, T>(Item1), 0);
			callback(MemoryMarshal.Cast<ST, T>(Item2), 1);
			callback(MemoryMarshal.Cast<ST, T>(Item3), 2);
			callback(MemoryMarshal.Cast<ST, T>(Item4), 3);
			callback(MemoryMarshal.Cast<ST, T>(Item5), 4);
			callback(MemoryMarshal.Cast<ST, T>(Item6), 5);
			callback(MemoryMarshal.Cast<ST, T>(Item7), 6);
			callback(MemoryMarshal.Cast<ST, T>(Item8), 7);
		}
	}
	/// <summary>
	/// A tuple (size 9) of read only spans - each with the same type
	/// </summary>
	public readonly ref struct ReadOnlySingleTypeSpanTuple9<ST> : IAmTupleOfSpans where ST: unmanaged
	{
		internal ReadOnlySingleTypeSpanTuple9(ReadOnlySpan<ST> item1, ReadOnlySpan<ST> item2, ReadOnlySpan<ST> item3, ReadOnlySpan<ST> item4, ReadOnlySpan<ST> item5, ReadOnlySpan<ST> item6, ReadOnlySpan<ST> item7, ReadOnlySpan<ST> item8, ReadOnlySpan<ST> item9)
		{
			Item1 = item1;
			Item2 = item2;
			Item3 = item3;
			Item4 = item4;
			Item5 = item5;
			Item6 = item6;
			Item7 = item7;
			Item8 = item8;
			Item9 = item9;
		}

		/// <summary>
		/// Item 1 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item1 { get; }

		/// <summary>
		/// Item 2 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item2 { get; }

		/// <summary>
		/// Item 3 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item3 { get; }

		/// <summary>
		/// Item 4 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item4 { get; }

		/// <summary>
		/// Item 5 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item5 { get; }

		/// <summary>
		/// Item 6 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item6 { get; }

		/// <summary>
		/// Item 7 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item7 { get; }

		/// <summary>
		/// Item 8 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item8 { get; }

		/// <summary>
		/// Item 9 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item9 { get; }

		/// <inheritdoc />
		public int[] ByteSizes => [
			Item1.Length * Unsafe.SizeOf<ST>(),
			Item2.Length * Unsafe.SizeOf<ST>(),
			Item3.Length * Unsafe.SizeOf<ST>(),
			Item4.Length * Unsafe.SizeOf<ST>(),
			Item5.Length * Unsafe.SizeOf<ST>(),
			Item6.Length * Unsafe.SizeOf<ST>(),
			Item7.Length * Unsafe.SizeOf<ST>(),
			Item8.Length * Unsafe.SizeOf<ST>(),
			Item9.Length * Unsafe.SizeOf<ST>(),
		];

		/// <inheritdoc />
		public int[] Sizes => [
			Item1.Length,
			Item2.Length,
			Item3.Length,
			Item4.Length,
			Item5.Length,
			Item6.Length,
			Item7.Length,
			Item8.Length,
			Item9.Length,
		];

		/// <inheritdoc />
		public void ForEach<T>(ForEachSpanCallback<T> callback) where T: unmanaged 
		{
			callback(MemoryMarshal.Cast<ST, T>(Item1), 0);
			callback(MemoryMarshal.Cast<ST, T>(Item2), 1);
			callback(MemoryMarshal.Cast<ST, T>(Item3), 2);
			callback(MemoryMarshal.Cast<ST, T>(Item4), 3);
			callback(MemoryMarshal.Cast<ST, T>(Item5), 4);
			callback(MemoryMarshal.Cast<ST, T>(Item6), 5);
			callback(MemoryMarshal.Cast<ST, T>(Item7), 6);
			callback(MemoryMarshal.Cast<ST, T>(Item8), 7);
			callback(MemoryMarshal.Cast<ST, T>(Item9), 8);
		}
	}
	/// <summary>
	/// A tuple (size 10) of read only spans - each with the same type
	/// </summary>
	public readonly ref struct ReadOnlySingleTypeSpanTuple10<ST> : IAmTupleOfSpans where ST: unmanaged
	{
		internal ReadOnlySingleTypeSpanTuple10(ReadOnlySpan<ST> item1, ReadOnlySpan<ST> item2, ReadOnlySpan<ST> item3, ReadOnlySpan<ST> item4, ReadOnlySpan<ST> item5, ReadOnlySpan<ST> item6, ReadOnlySpan<ST> item7, ReadOnlySpan<ST> item8, ReadOnlySpan<ST> item9, ReadOnlySpan<ST> item10)
		{
			Item1 = item1;
			Item2 = item2;
			Item3 = item3;
			Item4 = item4;
			Item5 = item5;
			Item6 = item6;
			Item7 = item7;
			Item8 = item8;
			Item9 = item9;
			Item10 = item10;
		}

		/// <summary>
		/// Item 1 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item1 { get; }

		/// <summary>
		/// Item 2 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item2 { get; }

		/// <summary>
		/// Item 3 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item3 { get; }

		/// <summary>
		/// Item 4 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item4 { get; }

		/// <summary>
		/// Item 5 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item5 { get; }

		/// <summary>
		/// Item 6 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item6 { get; }

		/// <summary>
		/// Item 7 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item7 { get; }

		/// <summary>
		/// Item 8 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item8 { get; }

		/// <summary>
		/// Item 9 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item9 { get; }

		/// <summary>
		/// Item 10 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item10 { get; }

		/// <inheritdoc />
		public int[] ByteSizes => [
			Item1.Length * Unsafe.SizeOf<ST>(),
			Item2.Length * Unsafe.SizeOf<ST>(),
			Item3.Length * Unsafe.SizeOf<ST>(),
			Item4.Length * Unsafe.SizeOf<ST>(),
			Item5.Length * Unsafe.SizeOf<ST>(),
			Item6.Length * Unsafe.SizeOf<ST>(),
			Item7.Length * Unsafe.SizeOf<ST>(),
			Item8.Length * Unsafe.SizeOf<ST>(),
			Item9.Length * Unsafe.SizeOf<ST>(),
			Item10.Length * Unsafe.SizeOf<ST>(),
		];

		/// <inheritdoc />
		public int[] Sizes => [
			Item1.Length,
			Item2.Length,
			Item3.Length,
			Item4.Length,
			Item5.Length,
			Item6.Length,
			Item7.Length,
			Item8.Length,
			Item9.Length,
			Item10.Length,
		];

		/// <inheritdoc />
		public void ForEach<T>(ForEachSpanCallback<T> callback) where T: unmanaged 
		{
			callback(MemoryMarshal.Cast<ST, T>(Item1), 0);
			callback(MemoryMarshal.Cast<ST, T>(Item2), 1);
			callback(MemoryMarshal.Cast<ST, T>(Item3), 2);
			callback(MemoryMarshal.Cast<ST, T>(Item4), 3);
			callback(MemoryMarshal.Cast<ST, T>(Item5), 4);
			callback(MemoryMarshal.Cast<ST, T>(Item6), 5);
			callback(MemoryMarshal.Cast<ST, T>(Item7), 6);
			callback(MemoryMarshal.Cast<ST, T>(Item8), 7);
			callback(MemoryMarshal.Cast<ST, T>(Item9), 8);
			callback(MemoryMarshal.Cast<ST, T>(Item10), 9);
		}
	}
	/// <summary>
	/// A tuple (size 11) of read only spans - each with the same type
	/// </summary>
	public readonly ref struct ReadOnlySingleTypeSpanTuple11<ST> : IAmTupleOfSpans where ST: unmanaged
	{
		internal ReadOnlySingleTypeSpanTuple11(ReadOnlySpan<ST> item1, ReadOnlySpan<ST> item2, ReadOnlySpan<ST> item3, ReadOnlySpan<ST> item4, ReadOnlySpan<ST> item5, ReadOnlySpan<ST> item6, ReadOnlySpan<ST> item7, ReadOnlySpan<ST> item8, ReadOnlySpan<ST> item9, ReadOnlySpan<ST> item10, ReadOnlySpan<ST> item11)
		{
			Item1 = item1;
			Item2 = item2;
			Item3 = item3;
			Item4 = item4;
			Item5 = item5;
			Item6 = item6;
			Item7 = item7;
			Item8 = item8;
			Item9 = item9;
			Item10 = item10;
			Item11 = item11;
		}

		/// <summary>
		/// Item 1 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item1 { get; }

		/// <summary>
		/// Item 2 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item2 { get; }

		/// <summary>
		/// Item 3 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item3 { get; }

		/// <summary>
		/// Item 4 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item4 { get; }

		/// <summary>
		/// Item 5 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item5 { get; }

		/// <summary>
		/// Item 6 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item6 { get; }

		/// <summary>
		/// Item 7 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item7 { get; }

		/// <summary>
		/// Item 8 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item8 { get; }

		/// <summary>
		/// Item 9 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item9 { get; }

		/// <summary>
		/// Item 10 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item10 { get; }

		/// <summary>
		/// Item 11 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item11 { get; }

		/// <inheritdoc />
		public int[] ByteSizes => [
			Item1.Length * Unsafe.SizeOf<ST>(),
			Item2.Length * Unsafe.SizeOf<ST>(),
			Item3.Length * Unsafe.SizeOf<ST>(),
			Item4.Length * Unsafe.SizeOf<ST>(),
			Item5.Length * Unsafe.SizeOf<ST>(),
			Item6.Length * Unsafe.SizeOf<ST>(),
			Item7.Length * Unsafe.SizeOf<ST>(),
			Item8.Length * Unsafe.SizeOf<ST>(),
			Item9.Length * Unsafe.SizeOf<ST>(),
			Item10.Length * Unsafe.SizeOf<ST>(),
			Item11.Length * Unsafe.SizeOf<ST>(),
		];

		/// <inheritdoc />
		public int[] Sizes => [
			Item1.Length,
			Item2.Length,
			Item3.Length,
			Item4.Length,
			Item5.Length,
			Item6.Length,
			Item7.Length,
			Item8.Length,
			Item9.Length,
			Item10.Length,
			Item11.Length,
		];

		/// <inheritdoc />
		public void ForEach<T>(ForEachSpanCallback<T> callback) where T: unmanaged 
		{
			callback(MemoryMarshal.Cast<ST, T>(Item1), 0);
			callback(MemoryMarshal.Cast<ST, T>(Item2), 1);
			callback(MemoryMarshal.Cast<ST, T>(Item3), 2);
			callback(MemoryMarshal.Cast<ST, T>(Item4), 3);
			callback(MemoryMarshal.Cast<ST, T>(Item5), 4);
			callback(MemoryMarshal.Cast<ST, T>(Item6), 5);
			callback(MemoryMarshal.Cast<ST, T>(Item7), 6);
			callback(MemoryMarshal.Cast<ST, T>(Item8), 7);
			callback(MemoryMarshal.Cast<ST, T>(Item9), 8);
			callback(MemoryMarshal.Cast<ST, T>(Item10), 9);
			callback(MemoryMarshal.Cast<ST, T>(Item11), 10);
		}
	}
	/// <summary>
	/// A tuple (size 12) of read only spans - each with the same type
	/// </summary>
	public readonly ref struct ReadOnlySingleTypeSpanTuple12<ST> : IAmTupleOfSpans where ST: unmanaged
	{
		internal ReadOnlySingleTypeSpanTuple12(ReadOnlySpan<ST> item1, ReadOnlySpan<ST> item2, ReadOnlySpan<ST> item3, ReadOnlySpan<ST> item4, ReadOnlySpan<ST> item5, ReadOnlySpan<ST> item6, ReadOnlySpan<ST> item7, ReadOnlySpan<ST> item8, ReadOnlySpan<ST> item9, ReadOnlySpan<ST> item10, ReadOnlySpan<ST> item11, ReadOnlySpan<ST> item12)
		{
			Item1 = item1;
			Item2 = item2;
			Item3 = item3;
			Item4 = item4;
			Item5 = item5;
			Item6 = item6;
			Item7 = item7;
			Item8 = item8;
			Item9 = item9;
			Item10 = item10;
			Item11 = item11;
			Item12 = item12;
		}

		/// <summary>
		/// Item 1 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item1 { get; }

		/// <summary>
		/// Item 2 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item2 { get; }

		/// <summary>
		/// Item 3 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item3 { get; }

		/// <summary>
		/// Item 4 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item4 { get; }

		/// <summary>
		/// Item 5 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item5 { get; }

		/// <summary>
		/// Item 6 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item6 { get; }

		/// <summary>
		/// Item 7 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item7 { get; }

		/// <summary>
		/// Item 8 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item8 { get; }

		/// <summary>
		/// Item 9 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item9 { get; }

		/// <summary>
		/// Item 10 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item10 { get; }

		/// <summary>
		/// Item 11 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item11 { get; }

		/// <summary>
		/// Item 12 in the tuple
		/// </summary>
		public ReadOnlySpan<ST> Item12 { get; }

		/// <inheritdoc />
		public int[] ByteSizes => [
			Item1.Length * Unsafe.SizeOf<ST>(),
			Item2.Length * Unsafe.SizeOf<ST>(),
			Item3.Length * Unsafe.SizeOf<ST>(),
			Item4.Length * Unsafe.SizeOf<ST>(),
			Item5.Length * Unsafe.SizeOf<ST>(),
			Item6.Length * Unsafe.SizeOf<ST>(),
			Item7.Length * Unsafe.SizeOf<ST>(),
			Item8.Length * Unsafe.SizeOf<ST>(),
			Item9.Length * Unsafe.SizeOf<ST>(),
			Item10.Length * Unsafe.SizeOf<ST>(),
			Item11.Length * Unsafe.SizeOf<ST>(),
			Item12.Length * Unsafe.SizeOf<ST>(),
		];

		/// <inheritdoc />
		public int[] Sizes => [
			Item1.Length,
			Item2.Length,
			Item3.Length,
			Item4.Length,
			Item5.Length,
			Item6.Length,
			Item7.Length,
			Item8.Length,
			Item9.Length,
			Item10.Length,
			Item11.Length,
			Item12.Length,
		];

		/// <inheritdoc />
		public void ForEach<T>(ForEachSpanCallback<T> callback) where T: unmanaged 
		{
			callback(MemoryMarshal.Cast<ST, T>(Item1), 0);
			callback(MemoryMarshal.Cast<ST, T>(Item2), 1);
			callback(MemoryMarshal.Cast<ST, T>(Item3), 2);
			callback(MemoryMarshal.Cast<ST, T>(Item4), 3);
			callback(MemoryMarshal.Cast<ST, T>(Item5), 4);
			callback(MemoryMarshal.Cast<ST, T>(Item6), 5);
			callback(MemoryMarshal.Cast<ST, T>(Item7), 6);
			callback(MemoryMarshal.Cast<ST, T>(Item8), 7);
			callback(MemoryMarshal.Cast<ST, T>(Item9), 8);
			callback(MemoryMarshal.Cast<ST, T>(Item10), 9);
			callback(MemoryMarshal.Cast<ST, T>(Item11), 10);
			callback(MemoryMarshal.Cast<ST, T>(Item12), 11);
		}
	}

	/// <summary>
	/// A tuple of spans, each with a different type
	/// </summary>
	public static class MultiTypeSpanTuple
	{
		/// <summary>
		/// Creates a multi type span tuple with 2 items
		/// </summary>
		public static MultiTypeSpanTuple2<T1, T2> Create<T1, T2>(Span<T1> item1, Span<T2> item2)
			where T1: unmanaged
			where T2: unmanaged
		{
			return new MultiTypeSpanTuple2<T1, T2>(
				item1,
				item2
			);
		}
		/// <summary>
		/// Creates a multi type span tuple with 3 items
		/// </summary>
		public static MultiTypeSpanTuple3<T1, T2, T3> Create<T1, T2, T3>(Span<T1> item1, Span<T2> item2, Span<T3> item3)
			where T1: unmanaged
			where T2: unmanaged
			where T3: unmanaged
		{
			return new MultiTypeSpanTuple3<T1, T2, T3>(
				item1,
				item2,
				item3
			);
		}
		/// <summary>
		/// Creates a multi type span tuple with 4 items
		/// </summary>
		public static MultiTypeSpanTuple4<T1, T2, T3, T4> Create<T1, T2, T3, T4>(Span<T1> item1, Span<T2> item2, Span<T3> item3, Span<T4> item4)
			where T1: unmanaged
			where T2: unmanaged
			where T3: unmanaged
			where T4: unmanaged
		{
			return new MultiTypeSpanTuple4<T1, T2, T3, T4>(
				item1,
				item2,
				item3,
				item4
			);
		}
		/// <summary>
		/// Creates a multi type span tuple with 5 items
		/// </summary>
		public static MultiTypeSpanTuple5<T1, T2, T3, T4, T5> Create<T1, T2, T3, T4, T5>(Span<T1> item1, Span<T2> item2, Span<T3> item3, Span<T4> item4, Span<T5> item5)
			where T1: unmanaged
			where T2: unmanaged
			where T3: unmanaged
			where T4: unmanaged
			where T5: unmanaged
		{
			return new MultiTypeSpanTuple5<T1, T2, T3, T4, T5>(
				item1,
				item2,
				item3,
				item4,
				item5
			);
		}
		/// <summary>
		/// Creates a multi type span tuple with 6 items
		/// </summary>
		public static MultiTypeSpanTuple6<T1, T2, T3, T4, T5, T6> Create<T1, T2, T3, T4, T5, T6>(Span<T1> item1, Span<T2> item2, Span<T3> item3, Span<T4> item4, Span<T5> item5, Span<T6> item6)
			where T1: unmanaged
			where T2: unmanaged
			where T3: unmanaged
			where T4: unmanaged
			where T5: unmanaged
			where T6: unmanaged
		{
			return new MultiTypeSpanTuple6<T1, T2, T3, T4, T5, T6>(
				item1,
				item2,
				item3,
				item4,
				item5,
				item6
			);
		}
		/// <summary>
		/// Creates a multi type span tuple with 7 items
		/// </summary>
		public static MultiTypeSpanTuple7<T1, T2, T3, T4, T5, T6, T7> Create<T1, T2, T3, T4, T5, T6, T7>(Span<T1> item1, Span<T2> item2, Span<T3> item3, Span<T4> item4, Span<T5> item5, Span<T6> item6, Span<T7> item7)
			where T1: unmanaged
			where T2: unmanaged
			where T3: unmanaged
			where T4: unmanaged
			where T5: unmanaged
			where T6: unmanaged
			where T7: unmanaged
		{
			return new MultiTypeSpanTuple7<T1, T2, T3, T4, T5, T6, T7>(
				item1,
				item2,
				item3,
				item4,
				item5,
				item6,
				item7
			);
		}
		/// <summary>
		/// Creates a multi type span tuple with 8 items
		/// </summary>
		public static MultiTypeSpanTuple8<T1, T2, T3, T4, T5, T6, T7, T8> Create<T1, T2, T3, T4, T5, T6, T7, T8>(Span<T1> item1, Span<T2> item2, Span<T3> item3, Span<T4> item4, Span<T5> item5, Span<T6> item6, Span<T7> item7, Span<T8> item8)
			where T1: unmanaged
			where T2: unmanaged
			where T3: unmanaged
			where T4: unmanaged
			where T5: unmanaged
			where T6: unmanaged
			where T7: unmanaged
			where T8: unmanaged
		{
			return new MultiTypeSpanTuple8<T1, T2, T3, T4, T5, T6, T7, T8>(
				item1,
				item2,
				item3,
				item4,
				item5,
				item6,
				item7,
				item8
			);
		}
		/// <summary>
		/// Creates a multi type span tuple with 9 items
		/// </summary>
		public static MultiTypeSpanTuple9<T1, T2, T3, T4, T5, T6, T7, T8, T9> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Span<T1> item1, Span<T2> item2, Span<T3> item3, Span<T4> item4, Span<T5> item5, Span<T6> item6, Span<T7> item7, Span<T8> item8, Span<T9> item9)
			where T1: unmanaged
			where T2: unmanaged
			where T3: unmanaged
			where T4: unmanaged
			where T5: unmanaged
			where T6: unmanaged
			where T7: unmanaged
			where T8: unmanaged
			where T9: unmanaged
		{
			return new MultiTypeSpanTuple9<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
				item1,
				item2,
				item3,
				item4,
				item5,
				item6,
				item7,
				item8,
				item9
			);
		}
		/// <summary>
		/// Creates a multi type span tuple with 10 items
		/// </summary>
		public static MultiTypeSpanTuple10<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Span<T1> item1, Span<T2> item2, Span<T3> item3, Span<T4> item4, Span<T5> item5, Span<T6> item6, Span<T7> item7, Span<T8> item8, Span<T9> item9, Span<T10> item10)
			where T1: unmanaged
			where T2: unmanaged
			where T3: unmanaged
			where T4: unmanaged
			where T5: unmanaged
			where T6: unmanaged
			where T7: unmanaged
			where T8: unmanaged
			where T9: unmanaged
			where T10: unmanaged
		{
			return new MultiTypeSpanTuple10<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
				item1,
				item2,
				item3,
				item4,
				item5,
				item6,
				item7,
				item8,
				item9,
				item10
			);
		}
		/// <summary>
		/// Creates a multi type span tuple with 11 items
		/// </summary>
		public static MultiTypeSpanTuple11<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(Span<T1> item1, Span<T2> item2, Span<T3> item3, Span<T4> item4, Span<T5> item5, Span<T6> item6, Span<T7> item7, Span<T8> item8, Span<T9> item9, Span<T10> item10, Span<T11> item11)
			where T1: unmanaged
			where T2: unmanaged
			where T3: unmanaged
			where T4: unmanaged
			where T5: unmanaged
			where T6: unmanaged
			where T7: unmanaged
			where T8: unmanaged
			where T9: unmanaged
			where T10: unmanaged
			where T11: unmanaged
		{
			return new MultiTypeSpanTuple11<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
				item1,
				item2,
				item3,
				item4,
				item5,
				item6,
				item7,
				item8,
				item9,
				item10,
				item11
			);
		}
		/// <summary>
		/// Creates a multi type span tuple with 12 items
		/// </summary>
		public static MultiTypeSpanTuple12<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(Span<T1> item1, Span<T2> item2, Span<T3> item3, Span<T4> item4, Span<T5> item5, Span<T6> item6, Span<T7> item7, Span<T8> item8, Span<T9> item9, Span<T10> item10, Span<T11> item11, Span<T12> item12)
			where T1: unmanaged
			where T2: unmanaged
			where T3: unmanaged
			where T4: unmanaged
			where T5: unmanaged
			where T6: unmanaged
			where T7: unmanaged
			where T8: unmanaged
			where T9: unmanaged
			where T10: unmanaged
			where T11: unmanaged
			where T12: unmanaged
		{
			return new MultiTypeSpanTuple12<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
				item1,
				item2,
				item3,
				item4,
				item5,
				item6,
				item7,
				item8,
				item9,
				item10,
				item11,
				item12
			);
		}
	}

	/// <summary>
	/// A tuple of read only spans, each with a different type
	/// </summary>
	public static class ReadOnlyMultiTypeSpanTuple
	{
		/// <summary>
		/// Creates a read only multi type span tuple with 2 items
		/// </summary>
		public static ReadOnlyMultiTypeSpanTuple2<T1, T2> Create<T1, T2>(ReadOnlySpan<T1> item1, ReadOnlySpan<T2> item2)
			where T1: unmanaged
			where T2: unmanaged
		{
			return new ReadOnlyMultiTypeSpanTuple2<T1, T2>(
				item1,
				item2
			);
		}
		/// <summary>
		/// Creates a read only multi type span tuple with 3 items
		/// </summary>
		public static ReadOnlyMultiTypeSpanTuple3<T1, T2, T3> Create<T1, T2, T3>(ReadOnlySpan<T1> item1, ReadOnlySpan<T2> item2, ReadOnlySpan<T3> item3)
			where T1: unmanaged
			where T2: unmanaged
			where T3: unmanaged
		{
			return new ReadOnlyMultiTypeSpanTuple3<T1, T2, T3>(
				item1,
				item2,
				item3
			);
		}
		/// <summary>
		/// Creates a read only multi type span tuple with 4 items
		/// </summary>
		public static ReadOnlyMultiTypeSpanTuple4<T1, T2, T3, T4> Create<T1, T2, T3, T4>(ReadOnlySpan<T1> item1, ReadOnlySpan<T2> item2, ReadOnlySpan<T3> item3, ReadOnlySpan<T4> item4)
			where T1: unmanaged
			where T2: unmanaged
			where T3: unmanaged
			where T4: unmanaged
		{
			return new ReadOnlyMultiTypeSpanTuple4<T1, T2, T3, T4>(
				item1,
				item2,
				item3,
				item4
			);
		}
		/// <summary>
		/// Creates a read only multi type span tuple with 5 items
		/// </summary>
		public static ReadOnlyMultiTypeSpanTuple5<T1, T2, T3, T4, T5> Create<T1, T2, T3, T4, T5>(ReadOnlySpan<T1> item1, ReadOnlySpan<T2> item2, ReadOnlySpan<T3> item3, ReadOnlySpan<T4> item4, ReadOnlySpan<T5> item5)
			where T1: unmanaged
			where T2: unmanaged
			where T3: unmanaged
			where T4: unmanaged
			where T5: unmanaged
		{
			return new ReadOnlyMultiTypeSpanTuple5<T1, T2, T3, T4, T5>(
				item1,
				item2,
				item3,
				item4,
				item5
			);
		}
		/// <summary>
		/// Creates a read only multi type span tuple with 6 items
		/// </summary>
		public static ReadOnlyMultiTypeSpanTuple6<T1, T2, T3, T4, T5, T6> Create<T1, T2, T3, T4, T5, T6>(ReadOnlySpan<T1> item1, ReadOnlySpan<T2> item2, ReadOnlySpan<T3> item3, ReadOnlySpan<T4> item4, ReadOnlySpan<T5> item5, ReadOnlySpan<T6> item6)
			where T1: unmanaged
			where T2: unmanaged
			where T3: unmanaged
			where T4: unmanaged
			where T5: unmanaged
			where T6: unmanaged
		{
			return new ReadOnlyMultiTypeSpanTuple6<T1, T2, T3, T4, T5, T6>(
				item1,
				item2,
				item3,
				item4,
				item5,
				item6
			);
		}
		/// <summary>
		/// Creates a read only multi type span tuple with 7 items
		/// </summary>
		public static ReadOnlyMultiTypeSpanTuple7<T1, T2, T3, T4, T5, T6, T7> Create<T1, T2, T3, T4, T5, T6, T7>(ReadOnlySpan<T1> item1, ReadOnlySpan<T2> item2, ReadOnlySpan<T3> item3, ReadOnlySpan<T4> item4, ReadOnlySpan<T5> item5, ReadOnlySpan<T6> item6, ReadOnlySpan<T7> item7)
			where T1: unmanaged
			where T2: unmanaged
			where T3: unmanaged
			where T4: unmanaged
			where T5: unmanaged
			where T6: unmanaged
			where T7: unmanaged
		{
			return new ReadOnlyMultiTypeSpanTuple7<T1, T2, T3, T4, T5, T6, T7>(
				item1,
				item2,
				item3,
				item4,
				item5,
				item6,
				item7
			);
		}
		/// <summary>
		/// Creates a read only multi type span tuple with 8 items
		/// </summary>
		public static ReadOnlyMultiTypeSpanTuple8<T1, T2, T3, T4, T5, T6, T7, T8> Create<T1, T2, T3, T4, T5, T6, T7, T8>(ReadOnlySpan<T1> item1, ReadOnlySpan<T2> item2, ReadOnlySpan<T3> item3, ReadOnlySpan<T4> item4, ReadOnlySpan<T5> item5, ReadOnlySpan<T6> item6, ReadOnlySpan<T7> item7, ReadOnlySpan<T8> item8)
			where T1: unmanaged
			where T2: unmanaged
			where T3: unmanaged
			where T4: unmanaged
			where T5: unmanaged
			where T6: unmanaged
			where T7: unmanaged
			where T8: unmanaged
		{
			return new ReadOnlyMultiTypeSpanTuple8<T1, T2, T3, T4, T5, T6, T7, T8>(
				item1,
				item2,
				item3,
				item4,
				item5,
				item6,
				item7,
				item8
			);
		}
		/// <summary>
		/// Creates a read only multi type span tuple with 9 items
		/// </summary>
		public static ReadOnlyMultiTypeSpanTuple9<T1, T2, T3, T4, T5, T6, T7, T8, T9> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9>(ReadOnlySpan<T1> item1, ReadOnlySpan<T2> item2, ReadOnlySpan<T3> item3, ReadOnlySpan<T4> item4, ReadOnlySpan<T5> item5, ReadOnlySpan<T6> item6, ReadOnlySpan<T7> item7, ReadOnlySpan<T8> item8, ReadOnlySpan<T9> item9)
			where T1: unmanaged
			where T2: unmanaged
			where T3: unmanaged
			where T4: unmanaged
			where T5: unmanaged
			where T6: unmanaged
			where T7: unmanaged
			where T8: unmanaged
			where T9: unmanaged
		{
			return new ReadOnlyMultiTypeSpanTuple9<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
				item1,
				item2,
				item3,
				item4,
				item5,
				item6,
				item7,
				item8,
				item9
			);
		}
		/// <summary>
		/// Creates a read only multi type span tuple with 10 items
		/// </summary>
		public static ReadOnlyMultiTypeSpanTuple10<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(ReadOnlySpan<T1> item1, ReadOnlySpan<T2> item2, ReadOnlySpan<T3> item3, ReadOnlySpan<T4> item4, ReadOnlySpan<T5> item5, ReadOnlySpan<T6> item6, ReadOnlySpan<T7> item7, ReadOnlySpan<T8> item8, ReadOnlySpan<T9> item9, ReadOnlySpan<T10> item10)
			where T1: unmanaged
			where T2: unmanaged
			where T3: unmanaged
			where T4: unmanaged
			where T5: unmanaged
			where T6: unmanaged
			where T7: unmanaged
			where T8: unmanaged
			where T9: unmanaged
			where T10: unmanaged
		{
			return new ReadOnlyMultiTypeSpanTuple10<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
				item1,
				item2,
				item3,
				item4,
				item5,
				item6,
				item7,
				item8,
				item9,
				item10
			);
		}
		/// <summary>
		/// Creates a read only multi type span tuple with 11 items
		/// </summary>
		public static ReadOnlyMultiTypeSpanTuple11<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(ReadOnlySpan<T1> item1, ReadOnlySpan<T2> item2, ReadOnlySpan<T3> item3, ReadOnlySpan<T4> item4, ReadOnlySpan<T5> item5, ReadOnlySpan<T6> item6, ReadOnlySpan<T7> item7, ReadOnlySpan<T8> item8, ReadOnlySpan<T9> item9, ReadOnlySpan<T10> item10, ReadOnlySpan<T11> item11)
			where T1: unmanaged
			where T2: unmanaged
			where T3: unmanaged
			where T4: unmanaged
			where T5: unmanaged
			where T6: unmanaged
			where T7: unmanaged
			where T8: unmanaged
			where T9: unmanaged
			where T10: unmanaged
			where T11: unmanaged
		{
			return new ReadOnlyMultiTypeSpanTuple11<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
				item1,
				item2,
				item3,
				item4,
				item5,
				item6,
				item7,
				item8,
				item9,
				item10,
				item11
			);
		}
		/// <summary>
		/// Creates a read only multi type span tuple with 12 items
		/// </summary>
		public static ReadOnlyMultiTypeSpanTuple12<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(ReadOnlySpan<T1> item1, ReadOnlySpan<T2> item2, ReadOnlySpan<T3> item3, ReadOnlySpan<T4> item4, ReadOnlySpan<T5> item5, ReadOnlySpan<T6> item6, ReadOnlySpan<T7> item7, ReadOnlySpan<T8> item8, ReadOnlySpan<T9> item9, ReadOnlySpan<T10> item10, ReadOnlySpan<T11> item11, ReadOnlySpan<T12> item12)
			where T1: unmanaged
			where T2: unmanaged
			where T3: unmanaged
			where T4: unmanaged
			where T5: unmanaged
			where T6: unmanaged
			where T7: unmanaged
			where T8: unmanaged
			where T9: unmanaged
			where T10: unmanaged
			where T11: unmanaged
			where T12: unmanaged
		{
			return new ReadOnlyMultiTypeSpanTuple12<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
				item1,
				item2,
				item3,
				item4,
				item5,
				item6,
				item7,
				item8,
				item9,
				item10,
				item11,
				item12
			);
		}
	}

	/// <summary>
	/// A tuple (size 2) of spans
	/// </summary>
	public readonly ref struct MultiTypeSpanTuple2<T1, T2> : IAmTupleOfSpans
		where T1: unmanaged
		where T2: unmanaged
	{
		internal MultiTypeSpanTuple2(Span<T1> item1, Span<T2> item2)
		{
			Item1 = item1;
			Item2 = item2;
		}

		/// <summary>
		/// Item 1 in the tuple
		/// </summary>
		public Span<T1> Item1 { get; }

		/// <summary>
		/// Item 2 in the tuple
		/// </summary>
		public Span<T2> Item2 { get; }

		/// <inheritdoc />
		public int[] Sizes => [
			Item1.Length,
			Item2.Length,
		];

		/// <inheritdoc />
		public int[] ByteSizes => [
			Item1.Length * Unsafe.SizeOf<T1>(),
			Item2.Length * Unsafe.SizeOf<T2>(),
		];

		/// <inheritdoc />
		public void ForEach<T>(ForEachSpanCallback<T> callback) where T: unmanaged 
		{
			callback(MemoryMarshal.Cast<T1, T>(Item1), 0);
			callback(MemoryMarshal.Cast<T2, T>(Item2), 1);
		}
	}

	/// <summary>
	/// A tuple (size 2) of read only spans, each item 
	/// </summary>
	public readonly ref struct ReadOnlyMultiTypeSpanTuple2<T1, T2> : IAmTupleOfSpans
		where T1: unmanaged
		where T2: unmanaged
	{
		internal ReadOnlyMultiTypeSpanTuple2(ReadOnlySpan<T1> item1, ReadOnlySpan<T2> item2)
		{
			Item1 = item1;
			Item2 = item2;
		}

		/// <summary>
		/// Item 1 in the tuple
		/// </summary>
		public ReadOnlySpan<T1> Item1 { get; }

		/// <summary>
		/// Item 2 in the tuple
		/// </summary>
		public ReadOnlySpan<T2> Item2 { get; }

		/// <inheritdoc />
		public int[] Sizes => [
			Item1.Length,
			Item2.Length,
		];

		/// <inheritdoc />
		public int[] ByteSizes => [
			Item1.Length * Unsafe.SizeOf<T1>(),
			Item2.Length * Unsafe.SizeOf<T2>(),
		];

		/// <summary>
		/// Casts each item to T and then invokes a callback on the new span
		/// </summary>
		/// <typeparam name="T">Type to cast to</typeparam>
		/// <param name="callback">Callback to invoke for each item</param>
		public void ForEach<T>(ForEachSpanCallback<T> callback) where T: unmanaged 
		{
			callback(MemoryMarshal.Cast<T1, T>(Item1), 0);
			callback(MemoryMarshal.Cast<T2, T>(Item2), 1);
		}
	}
	/// <summary>
	/// A tuple (size 3) of spans
	/// </summary>
	public readonly ref struct MultiTypeSpanTuple3<T1, T2, T3> : IAmTupleOfSpans
		where T1: unmanaged
		where T2: unmanaged
		where T3: unmanaged
	{
		internal MultiTypeSpanTuple3(Span<T1> item1, Span<T2> item2, Span<T3> item3)
		{
			Item1 = item1;
			Item2 = item2;
			Item3 = item3;
		}

		/// <summary>
		/// Item 1 in the tuple
		/// </summary>
		public Span<T1> Item1 { get; }

		/// <summary>
		/// Item 2 in the tuple
		/// </summary>
		public Span<T2> Item2 { get; }

		/// <summary>
		/// Item 3 in the tuple
		/// </summary>
		public Span<T3> Item3 { get; }

		/// <inheritdoc />
		public int[] Sizes => [
			Item1.Length,
			Item2.Length,
			Item3.Length,
		];

		/// <inheritdoc />
		public int[] ByteSizes => [
			Item1.Length * Unsafe.SizeOf<T1>(),
			Item2.Length * Unsafe.SizeOf<T2>(),
			Item3.Length * Unsafe.SizeOf<T3>(),
		];

		/// <inheritdoc />
		public void ForEach<T>(ForEachSpanCallback<T> callback) where T: unmanaged 
		{
			callback(MemoryMarshal.Cast<T1, T>(Item1), 0);
			callback(MemoryMarshal.Cast<T2, T>(Item2), 1);
			callback(MemoryMarshal.Cast<T3, T>(Item3), 2);
		}
	}

	/// <summary>
	/// A tuple (size 3) of read only spans, each item 
	/// </summary>
	public readonly ref struct ReadOnlyMultiTypeSpanTuple3<T1, T2, T3> : IAmTupleOfSpans
		where T1: unmanaged
		where T2: unmanaged
		where T3: unmanaged
	{
		internal ReadOnlyMultiTypeSpanTuple3(ReadOnlySpan<T1> item1, ReadOnlySpan<T2> item2, ReadOnlySpan<T3> item3)
		{
			Item1 = item1;
			Item2 = item2;
			Item3 = item3;
		}

		/// <summary>
		/// Item 1 in the tuple
		/// </summary>
		public ReadOnlySpan<T1> Item1 { get; }

		/// <summary>
		/// Item 2 in the tuple
		/// </summary>
		public ReadOnlySpan<T2> Item2 { get; }

		/// <summary>
		/// Item 3 in the tuple
		/// </summary>
		public ReadOnlySpan<T3> Item3 { get; }

		/// <inheritdoc />
		public int[] Sizes => [
			Item1.Length,
			Item2.Length,
			Item3.Length,
		];

		/// <inheritdoc />
		public int[] ByteSizes => [
			Item1.Length * Unsafe.SizeOf<T1>(),
			Item2.Length * Unsafe.SizeOf<T2>(),
			Item3.Length * Unsafe.SizeOf<T3>(),
		];

		/// <summary>
		/// Casts each item to T and then invokes a callback on the new span
		/// </summary>
		/// <typeparam name="T">Type to cast to</typeparam>
		/// <param name="callback">Callback to invoke for each item</param>
		public void ForEach<T>(ForEachSpanCallback<T> callback) where T: unmanaged 
		{
			callback(MemoryMarshal.Cast<T1, T>(Item1), 0);
			callback(MemoryMarshal.Cast<T2, T>(Item2), 1);
			callback(MemoryMarshal.Cast<T3, T>(Item3), 2);
		}
	}
	/// <summary>
	/// A tuple (size 4) of spans
	/// </summary>
	public readonly ref struct MultiTypeSpanTuple4<T1, T2, T3, T4> : IAmTupleOfSpans
		where T1: unmanaged
		where T2: unmanaged
		where T3: unmanaged
		where T4: unmanaged
	{
		internal MultiTypeSpanTuple4(Span<T1> item1, Span<T2> item2, Span<T3> item3, Span<T4> item4)
		{
			Item1 = item1;
			Item2 = item2;
			Item3 = item3;
			Item4 = item4;
		}

		/// <summary>
		/// Item 1 in the tuple
		/// </summary>
		public Span<T1> Item1 { get; }

		/// <summary>
		/// Item 2 in the tuple
		/// </summary>
		public Span<T2> Item2 { get; }

		/// <summary>
		/// Item 3 in the tuple
		/// </summary>
		public Span<T3> Item3 { get; }

		/// <summary>
		/// Item 4 in the tuple
		/// </summary>
		public Span<T4> Item4 { get; }

		/// <inheritdoc />
		public int[] Sizes => [
			Item1.Length,
			Item2.Length,
			Item3.Length,
			Item4.Length,
		];

		/// <inheritdoc />
		public int[] ByteSizes => [
			Item1.Length * Unsafe.SizeOf<T1>(),
			Item2.Length * Unsafe.SizeOf<T2>(),
			Item3.Length * Unsafe.SizeOf<T3>(),
			Item4.Length * Unsafe.SizeOf<T4>(),
		];

		/// <inheritdoc />
		public void ForEach<T>(ForEachSpanCallback<T> callback) where T: unmanaged 
		{
			callback(MemoryMarshal.Cast<T1, T>(Item1), 0);
			callback(MemoryMarshal.Cast<T2, T>(Item2), 1);
			callback(MemoryMarshal.Cast<T3, T>(Item3), 2);
			callback(MemoryMarshal.Cast<T4, T>(Item4), 3);
		}
	}

	/// <summary>
	/// A tuple (size 4) of read only spans, each item 
	/// </summary>
	public readonly ref struct ReadOnlyMultiTypeSpanTuple4<T1, T2, T3, T4> : IAmTupleOfSpans
		where T1: unmanaged
		where T2: unmanaged
		where T3: unmanaged
		where T4: unmanaged
	{
		internal ReadOnlyMultiTypeSpanTuple4(ReadOnlySpan<T1> item1, ReadOnlySpan<T2> item2, ReadOnlySpan<T3> item3, ReadOnlySpan<T4> item4)
		{
			Item1 = item1;
			Item2 = item2;
			Item3 = item3;
			Item4 = item4;
		}

		/// <summary>
		/// Item 1 in the tuple
		/// </summary>
		public ReadOnlySpan<T1> Item1 { get; }

		/// <summary>
		/// Item 2 in the tuple
		/// </summary>
		public ReadOnlySpan<T2> Item2 { get; }

		/// <summary>
		/// Item 3 in the tuple
		/// </summary>
		public ReadOnlySpan<T3> Item3 { get; }

		/// <summary>
		/// Item 4 in the tuple
		/// </summary>
		public ReadOnlySpan<T4> Item4 { get; }

		/// <inheritdoc />
		public int[] Sizes => [
			Item1.Length,
			Item2.Length,
			Item3.Length,
			Item4.Length,
		];

		/// <inheritdoc />
		public int[] ByteSizes => [
			Item1.Length * Unsafe.SizeOf<T1>(),
			Item2.Length * Unsafe.SizeOf<T2>(),
			Item3.Length * Unsafe.SizeOf<T3>(),
			Item4.Length * Unsafe.SizeOf<T4>(),
		];

		/// <summary>
		/// Casts each item to T and then invokes a callback on the new span
		/// </summary>
		/// <typeparam name="T">Type to cast to</typeparam>
		/// <param name="callback">Callback to invoke for each item</param>
		public void ForEach<T>(ForEachSpanCallback<T> callback) where T: unmanaged 
		{
			callback(MemoryMarshal.Cast<T1, T>(Item1), 0);
			callback(MemoryMarshal.Cast<T2, T>(Item2), 1);
			callback(MemoryMarshal.Cast<T3, T>(Item3), 2);
			callback(MemoryMarshal.Cast<T4, T>(Item4), 3);
		}
	}
	/// <summary>
	/// A tuple (size 5) of spans
	/// </summary>
	public readonly ref struct MultiTypeSpanTuple5<T1, T2, T3, T4, T5> : IAmTupleOfSpans
		where T1: unmanaged
		where T2: unmanaged
		where T3: unmanaged
		where T4: unmanaged
		where T5: unmanaged
	{
		internal MultiTypeSpanTuple5(Span<T1> item1, Span<T2> item2, Span<T3> item3, Span<T4> item4, Span<T5> item5)
		{
			Item1 = item1;
			Item2 = item2;
			Item3 = item3;
			Item4 = item4;
			Item5 = item5;
		}

		/// <summary>
		/// Item 1 in the tuple
		/// </summary>
		public Span<T1> Item1 { get; }

		/// <summary>
		/// Item 2 in the tuple
		/// </summary>
		public Span<T2> Item2 { get; }

		/// <summary>
		/// Item 3 in the tuple
		/// </summary>
		public Span<T3> Item3 { get; }

		/// <summary>
		/// Item 4 in the tuple
		/// </summary>
		public Span<T4> Item4 { get; }

		/// <summary>
		/// Item 5 in the tuple
		/// </summary>
		public Span<T5> Item5 { get; }

		/// <inheritdoc />
		public int[] Sizes => [
			Item1.Length,
			Item2.Length,
			Item3.Length,
			Item4.Length,
			Item5.Length,
		];

		/// <inheritdoc />
		public int[] ByteSizes => [
			Item1.Length * Unsafe.SizeOf<T1>(),
			Item2.Length * Unsafe.SizeOf<T2>(),
			Item3.Length * Unsafe.SizeOf<T3>(),
			Item4.Length * Unsafe.SizeOf<T4>(),
			Item5.Length * Unsafe.SizeOf<T5>(),
		];

		/// <inheritdoc />
		public void ForEach<T>(ForEachSpanCallback<T> callback) where T: unmanaged 
		{
			callback(MemoryMarshal.Cast<T1, T>(Item1), 0);
			callback(MemoryMarshal.Cast<T2, T>(Item2), 1);
			callback(MemoryMarshal.Cast<T3, T>(Item3), 2);
			callback(MemoryMarshal.Cast<T4, T>(Item4), 3);
			callback(MemoryMarshal.Cast<T5, T>(Item5), 4);
		}
	}

	/// <summary>
	/// A tuple (size 5) of read only spans, each item 
	/// </summary>
	public readonly ref struct ReadOnlyMultiTypeSpanTuple5<T1, T2, T3, T4, T5> : IAmTupleOfSpans
		where T1: unmanaged
		where T2: unmanaged
		where T3: unmanaged
		where T4: unmanaged
		where T5: unmanaged
	{
		internal ReadOnlyMultiTypeSpanTuple5(ReadOnlySpan<T1> item1, ReadOnlySpan<T2> item2, ReadOnlySpan<T3> item3, ReadOnlySpan<T4> item4, ReadOnlySpan<T5> item5)
		{
			Item1 = item1;
			Item2 = item2;
			Item3 = item3;
			Item4 = item4;
			Item5 = item5;
		}

		/// <summary>
		/// Item 1 in the tuple
		/// </summary>
		public ReadOnlySpan<T1> Item1 { get; }

		/// <summary>
		/// Item 2 in the tuple
		/// </summary>
		public ReadOnlySpan<T2> Item2 { get; }

		/// <summary>
		/// Item 3 in the tuple
		/// </summary>
		public ReadOnlySpan<T3> Item3 { get; }

		/// <summary>
		/// Item 4 in the tuple
		/// </summary>
		public ReadOnlySpan<T4> Item4 { get; }

		/// <summary>
		/// Item 5 in the tuple
		/// </summary>
		public ReadOnlySpan<T5> Item5 { get; }

		/// <inheritdoc />
		public int[] Sizes => [
			Item1.Length,
			Item2.Length,
			Item3.Length,
			Item4.Length,
			Item5.Length,
		];

		/// <inheritdoc />
		public int[] ByteSizes => [
			Item1.Length * Unsafe.SizeOf<T1>(),
			Item2.Length * Unsafe.SizeOf<T2>(),
			Item3.Length * Unsafe.SizeOf<T3>(),
			Item4.Length * Unsafe.SizeOf<T4>(),
			Item5.Length * Unsafe.SizeOf<T5>(),
		];

		/// <summary>
		/// Casts each item to T and then invokes a callback on the new span
		/// </summary>
		/// <typeparam name="T">Type to cast to</typeparam>
		/// <param name="callback">Callback to invoke for each item</param>
		public void ForEach<T>(ForEachSpanCallback<T> callback) where T: unmanaged 
		{
			callback(MemoryMarshal.Cast<T1, T>(Item1), 0);
			callback(MemoryMarshal.Cast<T2, T>(Item2), 1);
			callback(MemoryMarshal.Cast<T3, T>(Item3), 2);
			callback(MemoryMarshal.Cast<T4, T>(Item4), 3);
			callback(MemoryMarshal.Cast<T5, T>(Item5), 4);
		}
	}
	/// <summary>
	/// A tuple (size 6) of spans
	/// </summary>
	public readonly ref struct MultiTypeSpanTuple6<T1, T2, T3, T4, T5, T6> : IAmTupleOfSpans
		where T1: unmanaged
		where T2: unmanaged
		where T3: unmanaged
		where T4: unmanaged
		where T5: unmanaged
		where T6: unmanaged
	{
		internal MultiTypeSpanTuple6(Span<T1> item1, Span<T2> item2, Span<T3> item3, Span<T4> item4, Span<T5> item5, Span<T6> item6)
		{
			Item1 = item1;
			Item2 = item2;
			Item3 = item3;
			Item4 = item4;
			Item5 = item5;
			Item6 = item6;
		}

		/// <summary>
		/// Item 1 in the tuple
		/// </summary>
		public Span<T1> Item1 { get; }

		/// <summary>
		/// Item 2 in the tuple
		/// </summary>
		public Span<T2> Item2 { get; }

		/// <summary>
		/// Item 3 in the tuple
		/// </summary>
		public Span<T3> Item3 { get; }

		/// <summary>
		/// Item 4 in the tuple
		/// </summary>
		public Span<T4> Item4 { get; }

		/// <summary>
		/// Item 5 in the tuple
		/// </summary>
		public Span<T5> Item5 { get; }

		/// <summary>
		/// Item 6 in the tuple
		/// </summary>
		public Span<T6> Item6 { get; }

		/// <inheritdoc />
		public int[] Sizes => [
			Item1.Length,
			Item2.Length,
			Item3.Length,
			Item4.Length,
			Item5.Length,
			Item6.Length,
		];

		/// <inheritdoc />
		public int[] ByteSizes => [
			Item1.Length * Unsafe.SizeOf<T1>(),
			Item2.Length * Unsafe.SizeOf<T2>(),
			Item3.Length * Unsafe.SizeOf<T3>(),
			Item4.Length * Unsafe.SizeOf<T4>(),
			Item5.Length * Unsafe.SizeOf<T5>(),
			Item6.Length * Unsafe.SizeOf<T6>(),
		];

		/// <inheritdoc />
		public void ForEach<T>(ForEachSpanCallback<T> callback) where T: unmanaged 
		{
			callback(MemoryMarshal.Cast<T1, T>(Item1), 0);
			callback(MemoryMarshal.Cast<T2, T>(Item2), 1);
			callback(MemoryMarshal.Cast<T3, T>(Item3), 2);
			callback(MemoryMarshal.Cast<T4, T>(Item4), 3);
			callback(MemoryMarshal.Cast<T5, T>(Item5), 4);
			callback(MemoryMarshal.Cast<T6, T>(Item6), 5);
		}
	}

	/// <summary>
	/// A tuple (size 6) of read only spans, each item 
	/// </summary>
	public readonly ref struct ReadOnlyMultiTypeSpanTuple6<T1, T2, T3, T4, T5, T6> : IAmTupleOfSpans
		where T1: unmanaged
		where T2: unmanaged
		where T3: unmanaged
		where T4: unmanaged
		where T5: unmanaged
		where T6: unmanaged
	{
		internal ReadOnlyMultiTypeSpanTuple6(ReadOnlySpan<T1> item1, ReadOnlySpan<T2> item2, ReadOnlySpan<T3> item3, ReadOnlySpan<T4> item4, ReadOnlySpan<T5> item5, ReadOnlySpan<T6> item6)
		{
			Item1 = item1;
			Item2 = item2;
			Item3 = item3;
			Item4 = item4;
			Item5 = item5;
			Item6 = item6;
		}

		/// <summary>
		/// Item 1 in the tuple
		/// </summary>
		public ReadOnlySpan<T1> Item1 { get; }

		/// <summary>
		/// Item 2 in the tuple
		/// </summary>
		public ReadOnlySpan<T2> Item2 { get; }

		/// <summary>
		/// Item 3 in the tuple
		/// </summary>
		public ReadOnlySpan<T3> Item3 { get; }

		/// <summary>
		/// Item 4 in the tuple
		/// </summary>
		public ReadOnlySpan<T4> Item4 { get; }

		/// <summary>
		/// Item 5 in the tuple
		/// </summary>
		public ReadOnlySpan<T5> Item5 { get; }

		/// <summary>
		/// Item 6 in the tuple
		/// </summary>
		public ReadOnlySpan<T6> Item6 { get; }

		/// <inheritdoc />
		public int[] Sizes => [
			Item1.Length,
			Item2.Length,
			Item3.Length,
			Item4.Length,
			Item5.Length,
			Item6.Length,
		];

		/// <inheritdoc />
		public int[] ByteSizes => [
			Item1.Length * Unsafe.SizeOf<T1>(),
			Item2.Length * Unsafe.SizeOf<T2>(),
			Item3.Length * Unsafe.SizeOf<T3>(),
			Item4.Length * Unsafe.SizeOf<T4>(),
			Item5.Length * Unsafe.SizeOf<T5>(),
			Item6.Length * Unsafe.SizeOf<T6>(),
		];

		/// <summary>
		/// Casts each item to T and then invokes a callback on the new span
		/// </summary>
		/// <typeparam name="T">Type to cast to</typeparam>
		/// <param name="callback">Callback to invoke for each item</param>
		public void ForEach<T>(ForEachSpanCallback<T> callback) where T: unmanaged 
		{
			callback(MemoryMarshal.Cast<T1, T>(Item1), 0);
			callback(MemoryMarshal.Cast<T2, T>(Item2), 1);
			callback(MemoryMarshal.Cast<T3, T>(Item3), 2);
			callback(MemoryMarshal.Cast<T4, T>(Item4), 3);
			callback(MemoryMarshal.Cast<T5, T>(Item5), 4);
			callback(MemoryMarshal.Cast<T6, T>(Item6), 5);
		}
	}
	/// <summary>
	/// A tuple (size 7) of spans
	/// </summary>
	public readonly ref struct MultiTypeSpanTuple7<T1, T2, T3, T4, T5, T6, T7> : IAmTupleOfSpans
		where T1: unmanaged
		where T2: unmanaged
		where T3: unmanaged
		where T4: unmanaged
		where T5: unmanaged
		where T6: unmanaged
		where T7: unmanaged
	{
		internal MultiTypeSpanTuple7(Span<T1> item1, Span<T2> item2, Span<T3> item3, Span<T4> item4, Span<T5> item5, Span<T6> item6, Span<T7> item7)
		{
			Item1 = item1;
			Item2 = item2;
			Item3 = item3;
			Item4 = item4;
			Item5 = item5;
			Item6 = item6;
			Item7 = item7;
		}

		/// <summary>
		/// Item 1 in the tuple
		/// </summary>
		public Span<T1> Item1 { get; }

		/// <summary>
		/// Item 2 in the tuple
		/// </summary>
		public Span<T2> Item2 { get; }

		/// <summary>
		/// Item 3 in the tuple
		/// </summary>
		public Span<T3> Item3 { get; }

		/// <summary>
		/// Item 4 in the tuple
		/// </summary>
		public Span<T4> Item4 { get; }

		/// <summary>
		/// Item 5 in the tuple
		/// </summary>
		public Span<T5> Item5 { get; }

		/// <summary>
		/// Item 6 in the tuple
		/// </summary>
		public Span<T6> Item6 { get; }

		/// <summary>
		/// Item 7 in the tuple
		/// </summary>
		public Span<T7> Item7 { get; }

		/// <inheritdoc />
		public int[] Sizes => [
			Item1.Length,
			Item2.Length,
			Item3.Length,
			Item4.Length,
			Item5.Length,
			Item6.Length,
			Item7.Length,
		];

		/// <inheritdoc />
		public int[] ByteSizes => [
			Item1.Length * Unsafe.SizeOf<T1>(),
			Item2.Length * Unsafe.SizeOf<T2>(),
			Item3.Length * Unsafe.SizeOf<T3>(),
			Item4.Length * Unsafe.SizeOf<T4>(),
			Item5.Length * Unsafe.SizeOf<T5>(),
			Item6.Length * Unsafe.SizeOf<T6>(),
			Item7.Length * Unsafe.SizeOf<T7>(),
		];

		/// <inheritdoc />
		public void ForEach<T>(ForEachSpanCallback<T> callback) where T: unmanaged 
		{
			callback(MemoryMarshal.Cast<T1, T>(Item1), 0);
			callback(MemoryMarshal.Cast<T2, T>(Item2), 1);
			callback(MemoryMarshal.Cast<T3, T>(Item3), 2);
			callback(MemoryMarshal.Cast<T4, T>(Item4), 3);
			callback(MemoryMarshal.Cast<T5, T>(Item5), 4);
			callback(MemoryMarshal.Cast<T6, T>(Item6), 5);
			callback(MemoryMarshal.Cast<T7, T>(Item7), 6);
		}
	}

	/// <summary>
	/// A tuple (size 7) of read only spans, each item 
	/// </summary>
	public readonly ref struct ReadOnlyMultiTypeSpanTuple7<T1, T2, T3, T4, T5, T6, T7> : IAmTupleOfSpans
		where T1: unmanaged
		where T2: unmanaged
		where T3: unmanaged
		where T4: unmanaged
		where T5: unmanaged
		where T6: unmanaged
		where T7: unmanaged
	{
		internal ReadOnlyMultiTypeSpanTuple7(ReadOnlySpan<T1> item1, ReadOnlySpan<T2> item2, ReadOnlySpan<T3> item3, ReadOnlySpan<T4> item4, ReadOnlySpan<T5> item5, ReadOnlySpan<T6> item6, ReadOnlySpan<T7> item7)
		{
			Item1 = item1;
			Item2 = item2;
			Item3 = item3;
			Item4 = item4;
			Item5 = item5;
			Item6 = item6;
			Item7 = item7;
		}

		/// <summary>
		/// Item 1 in the tuple
		/// </summary>
		public ReadOnlySpan<T1> Item1 { get; }

		/// <summary>
		/// Item 2 in the tuple
		/// </summary>
		public ReadOnlySpan<T2> Item2 { get; }

		/// <summary>
		/// Item 3 in the tuple
		/// </summary>
		public ReadOnlySpan<T3> Item3 { get; }

		/// <summary>
		/// Item 4 in the tuple
		/// </summary>
		public ReadOnlySpan<T4> Item4 { get; }

		/// <summary>
		/// Item 5 in the tuple
		/// </summary>
		public ReadOnlySpan<T5> Item5 { get; }

		/// <summary>
		/// Item 6 in the tuple
		/// </summary>
		public ReadOnlySpan<T6> Item6 { get; }

		/// <summary>
		/// Item 7 in the tuple
		/// </summary>
		public ReadOnlySpan<T7> Item7 { get; }

		/// <inheritdoc />
		public int[] Sizes => [
			Item1.Length,
			Item2.Length,
			Item3.Length,
			Item4.Length,
			Item5.Length,
			Item6.Length,
			Item7.Length,
		];

		/// <inheritdoc />
		public int[] ByteSizes => [
			Item1.Length * Unsafe.SizeOf<T1>(),
			Item2.Length * Unsafe.SizeOf<T2>(),
			Item3.Length * Unsafe.SizeOf<T3>(),
			Item4.Length * Unsafe.SizeOf<T4>(),
			Item5.Length * Unsafe.SizeOf<T5>(),
			Item6.Length * Unsafe.SizeOf<T6>(),
			Item7.Length * Unsafe.SizeOf<T7>(),
		];

		/// <summary>
		/// Casts each item to T and then invokes a callback on the new span
		/// </summary>
		/// <typeparam name="T">Type to cast to</typeparam>
		/// <param name="callback">Callback to invoke for each item</param>
		public void ForEach<T>(ForEachSpanCallback<T> callback) where T: unmanaged 
		{
			callback(MemoryMarshal.Cast<T1, T>(Item1), 0);
			callback(MemoryMarshal.Cast<T2, T>(Item2), 1);
			callback(MemoryMarshal.Cast<T3, T>(Item3), 2);
			callback(MemoryMarshal.Cast<T4, T>(Item4), 3);
			callback(MemoryMarshal.Cast<T5, T>(Item5), 4);
			callback(MemoryMarshal.Cast<T6, T>(Item6), 5);
			callback(MemoryMarshal.Cast<T7, T>(Item7), 6);
		}
	}
	/// <summary>
	/// A tuple (size 8) of spans
	/// </summary>
	public readonly ref struct MultiTypeSpanTuple8<T1, T2, T3, T4, T5, T6, T7, T8> : IAmTupleOfSpans
		where T1: unmanaged
		where T2: unmanaged
		where T3: unmanaged
		where T4: unmanaged
		where T5: unmanaged
		where T6: unmanaged
		where T7: unmanaged
		where T8: unmanaged
	{
		internal MultiTypeSpanTuple8(Span<T1> item1, Span<T2> item2, Span<T3> item3, Span<T4> item4, Span<T5> item5, Span<T6> item6, Span<T7> item7, Span<T8> item8)
		{
			Item1 = item1;
			Item2 = item2;
			Item3 = item3;
			Item4 = item4;
			Item5 = item5;
			Item6 = item6;
			Item7 = item7;
			Item8 = item8;
		}

		/// <summary>
		/// Item 1 in the tuple
		/// </summary>
		public Span<T1> Item1 { get; }

		/// <summary>
		/// Item 2 in the tuple
		/// </summary>
		public Span<T2> Item2 { get; }

		/// <summary>
		/// Item 3 in the tuple
		/// </summary>
		public Span<T3> Item3 { get; }

		/// <summary>
		/// Item 4 in the tuple
		/// </summary>
		public Span<T4> Item4 { get; }

		/// <summary>
		/// Item 5 in the tuple
		/// </summary>
		public Span<T5> Item5 { get; }

		/// <summary>
		/// Item 6 in the tuple
		/// </summary>
		public Span<T6> Item6 { get; }

		/// <summary>
		/// Item 7 in the tuple
		/// </summary>
		public Span<T7> Item7 { get; }

		/// <summary>
		/// Item 8 in the tuple
		/// </summary>
		public Span<T8> Item8 { get; }

		/// <inheritdoc />
		public int[] Sizes => [
			Item1.Length,
			Item2.Length,
			Item3.Length,
			Item4.Length,
			Item5.Length,
			Item6.Length,
			Item7.Length,
			Item8.Length,
		];

		/// <inheritdoc />
		public int[] ByteSizes => [
			Item1.Length * Unsafe.SizeOf<T1>(),
			Item2.Length * Unsafe.SizeOf<T2>(),
			Item3.Length * Unsafe.SizeOf<T3>(),
			Item4.Length * Unsafe.SizeOf<T4>(),
			Item5.Length * Unsafe.SizeOf<T5>(),
			Item6.Length * Unsafe.SizeOf<T6>(),
			Item7.Length * Unsafe.SizeOf<T7>(),
			Item8.Length * Unsafe.SizeOf<T8>(),
		];

		/// <inheritdoc />
		public void ForEach<T>(ForEachSpanCallback<T> callback) where T: unmanaged 
		{
			callback(MemoryMarshal.Cast<T1, T>(Item1), 0);
			callback(MemoryMarshal.Cast<T2, T>(Item2), 1);
			callback(MemoryMarshal.Cast<T3, T>(Item3), 2);
			callback(MemoryMarshal.Cast<T4, T>(Item4), 3);
			callback(MemoryMarshal.Cast<T5, T>(Item5), 4);
			callback(MemoryMarshal.Cast<T6, T>(Item6), 5);
			callback(MemoryMarshal.Cast<T7, T>(Item7), 6);
			callback(MemoryMarshal.Cast<T8, T>(Item8), 7);
		}
	}

	/// <summary>
	/// A tuple (size 8) of read only spans, each item 
	/// </summary>
	public readonly ref struct ReadOnlyMultiTypeSpanTuple8<T1, T2, T3, T4, T5, T6, T7, T8> : IAmTupleOfSpans
		where T1: unmanaged
		where T2: unmanaged
		where T3: unmanaged
		where T4: unmanaged
		where T5: unmanaged
		where T6: unmanaged
		where T7: unmanaged
		where T8: unmanaged
	{
		internal ReadOnlyMultiTypeSpanTuple8(ReadOnlySpan<T1> item1, ReadOnlySpan<T2> item2, ReadOnlySpan<T3> item3, ReadOnlySpan<T4> item4, ReadOnlySpan<T5> item5, ReadOnlySpan<T6> item6, ReadOnlySpan<T7> item7, ReadOnlySpan<T8> item8)
		{
			Item1 = item1;
			Item2 = item2;
			Item3 = item3;
			Item4 = item4;
			Item5 = item5;
			Item6 = item6;
			Item7 = item7;
			Item8 = item8;
		}

		/// <summary>
		/// Item 1 in the tuple
		/// </summary>
		public ReadOnlySpan<T1> Item1 { get; }

		/// <summary>
		/// Item 2 in the tuple
		/// </summary>
		public ReadOnlySpan<T2> Item2 { get; }

		/// <summary>
		/// Item 3 in the tuple
		/// </summary>
		public ReadOnlySpan<T3> Item3 { get; }

		/// <summary>
		/// Item 4 in the tuple
		/// </summary>
		public ReadOnlySpan<T4> Item4 { get; }

		/// <summary>
		/// Item 5 in the tuple
		/// </summary>
		public ReadOnlySpan<T5> Item5 { get; }

		/// <summary>
		/// Item 6 in the tuple
		/// </summary>
		public ReadOnlySpan<T6> Item6 { get; }

		/// <summary>
		/// Item 7 in the tuple
		/// </summary>
		public ReadOnlySpan<T7> Item7 { get; }

		/// <summary>
		/// Item 8 in the tuple
		/// </summary>
		public ReadOnlySpan<T8> Item8 { get; }

		/// <inheritdoc />
		public int[] Sizes => [
			Item1.Length,
			Item2.Length,
			Item3.Length,
			Item4.Length,
			Item5.Length,
			Item6.Length,
			Item7.Length,
			Item8.Length,
		];

		/// <inheritdoc />
		public int[] ByteSizes => [
			Item1.Length * Unsafe.SizeOf<T1>(),
			Item2.Length * Unsafe.SizeOf<T2>(),
			Item3.Length * Unsafe.SizeOf<T3>(),
			Item4.Length * Unsafe.SizeOf<T4>(),
			Item5.Length * Unsafe.SizeOf<T5>(),
			Item6.Length * Unsafe.SizeOf<T6>(),
			Item7.Length * Unsafe.SizeOf<T7>(),
			Item8.Length * Unsafe.SizeOf<T8>(),
		];

		/// <summary>
		/// Casts each item to T and then invokes a callback on the new span
		/// </summary>
		/// <typeparam name="T">Type to cast to</typeparam>
		/// <param name="callback">Callback to invoke for each item</param>
		public void ForEach<T>(ForEachSpanCallback<T> callback) where T: unmanaged 
		{
			callback(MemoryMarshal.Cast<T1, T>(Item1), 0);
			callback(MemoryMarshal.Cast<T2, T>(Item2), 1);
			callback(MemoryMarshal.Cast<T3, T>(Item3), 2);
			callback(MemoryMarshal.Cast<T4, T>(Item4), 3);
			callback(MemoryMarshal.Cast<T5, T>(Item5), 4);
			callback(MemoryMarshal.Cast<T6, T>(Item6), 5);
			callback(MemoryMarshal.Cast<T7, T>(Item7), 6);
			callback(MemoryMarshal.Cast<T8, T>(Item8), 7);
		}
	}
	/// <summary>
	/// A tuple (size 9) of spans
	/// </summary>
	public readonly ref struct MultiTypeSpanTuple9<T1, T2, T3, T4, T5, T6, T7, T8, T9> : IAmTupleOfSpans
		where T1: unmanaged
		where T2: unmanaged
		where T3: unmanaged
		where T4: unmanaged
		where T5: unmanaged
		where T6: unmanaged
		where T7: unmanaged
		where T8: unmanaged
		where T9: unmanaged
	{
		internal MultiTypeSpanTuple9(Span<T1> item1, Span<T2> item2, Span<T3> item3, Span<T4> item4, Span<T5> item5, Span<T6> item6, Span<T7> item7, Span<T8> item8, Span<T9> item9)
		{
			Item1 = item1;
			Item2 = item2;
			Item3 = item3;
			Item4 = item4;
			Item5 = item5;
			Item6 = item6;
			Item7 = item7;
			Item8 = item8;
			Item9 = item9;
		}

		/// <summary>
		/// Item 1 in the tuple
		/// </summary>
		public Span<T1> Item1 { get; }

		/// <summary>
		/// Item 2 in the tuple
		/// </summary>
		public Span<T2> Item2 { get; }

		/// <summary>
		/// Item 3 in the tuple
		/// </summary>
		public Span<T3> Item3 { get; }

		/// <summary>
		/// Item 4 in the tuple
		/// </summary>
		public Span<T4> Item4 { get; }

		/// <summary>
		/// Item 5 in the tuple
		/// </summary>
		public Span<T5> Item5 { get; }

		/// <summary>
		/// Item 6 in the tuple
		/// </summary>
		public Span<T6> Item6 { get; }

		/// <summary>
		/// Item 7 in the tuple
		/// </summary>
		public Span<T7> Item7 { get; }

		/// <summary>
		/// Item 8 in the tuple
		/// </summary>
		public Span<T8> Item8 { get; }

		/// <summary>
		/// Item 9 in the tuple
		/// </summary>
		public Span<T9> Item9 { get; }

		/// <inheritdoc />
		public int[] Sizes => [
			Item1.Length,
			Item2.Length,
			Item3.Length,
			Item4.Length,
			Item5.Length,
			Item6.Length,
			Item7.Length,
			Item8.Length,
			Item9.Length,
		];

		/// <inheritdoc />
		public int[] ByteSizes => [
			Item1.Length * Unsafe.SizeOf<T1>(),
			Item2.Length * Unsafe.SizeOf<T2>(),
			Item3.Length * Unsafe.SizeOf<T3>(),
			Item4.Length * Unsafe.SizeOf<T4>(),
			Item5.Length * Unsafe.SizeOf<T5>(),
			Item6.Length * Unsafe.SizeOf<T6>(),
			Item7.Length * Unsafe.SizeOf<T7>(),
			Item8.Length * Unsafe.SizeOf<T8>(),
			Item9.Length * Unsafe.SizeOf<T9>(),
		];

		/// <inheritdoc />
		public void ForEach<T>(ForEachSpanCallback<T> callback) where T: unmanaged 
		{
			callback(MemoryMarshal.Cast<T1, T>(Item1), 0);
			callback(MemoryMarshal.Cast<T2, T>(Item2), 1);
			callback(MemoryMarshal.Cast<T3, T>(Item3), 2);
			callback(MemoryMarshal.Cast<T4, T>(Item4), 3);
			callback(MemoryMarshal.Cast<T5, T>(Item5), 4);
			callback(MemoryMarshal.Cast<T6, T>(Item6), 5);
			callback(MemoryMarshal.Cast<T7, T>(Item7), 6);
			callback(MemoryMarshal.Cast<T8, T>(Item8), 7);
			callback(MemoryMarshal.Cast<T9, T>(Item9), 8);
		}
	}

	/// <summary>
	/// A tuple (size 9) of read only spans, each item 
	/// </summary>
	public readonly ref struct ReadOnlyMultiTypeSpanTuple9<T1, T2, T3, T4, T5, T6, T7, T8, T9> : IAmTupleOfSpans
		where T1: unmanaged
		where T2: unmanaged
		where T3: unmanaged
		where T4: unmanaged
		where T5: unmanaged
		where T6: unmanaged
		where T7: unmanaged
		where T8: unmanaged
		where T9: unmanaged
	{
		internal ReadOnlyMultiTypeSpanTuple9(ReadOnlySpan<T1> item1, ReadOnlySpan<T2> item2, ReadOnlySpan<T3> item3, ReadOnlySpan<T4> item4, ReadOnlySpan<T5> item5, ReadOnlySpan<T6> item6, ReadOnlySpan<T7> item7, ReadOnlySpan<T8> item8, ReadOnlySpan<T9> item9)
		{
			Item1 = item1;
			Item2 = item2;
			Item3 = item3;
			Item4 = item4;
			Item5 = item5;
			Item6 = item6;
			Item7 = item7;
			Item8 = item8;
			Item9 = item9;
		}

		/// <summary>
		/// Item 1 in the tuple
		/// </summary>
		public ReadOnlySpan<T1> Item1 { get; }

		/// <summary>
		/// Item 2 in the tuple
		/// </summary>
		public ReadOnlySpan<T2> Item2 { get; }

		/// <summary>
		/// Item 3 in the tuple
		/// </summary>
		public ReadOnlySpan<T3> Item3 { get; }

		/// <summary>
		/// Item 4 in the tuple
		/// </summary>
		public ReadOnlySpan<T4> Item4 { get; }

		/// <summary>
		/// Item 5 in the tuple
		/// </summary>
		public ReadOnlySpan<T5> Item5 { get; }

		/// <summary>
		/// Item 6 in the tuple
		/// </summary>
		public ReadOnlySpan<T6> Item6 { get; }

		/// <summary>
		/// Item 7 in the tuple
		/// </summary>
		public ReadOnlySpan<T7> Item7 { get; }

		/// <summary>
		/// Item 8 in the tuple
		/// </summary>
		public ReadOnlySpan<T8> Item8 { get; }

		/// <summary>
		/// Item 9 in the tuple
		/// </summary>
		public ReadOnlySpan<T9> Item9 { get; }

		/// <inheritdoc />
		public int[] Sizes => [
			Item1.Length,
			Item2.Length,
			Item3.Length,
			Item4.Length,
			Item5.Length,
			Item6.Length,
			Item7.Length,
			Item8.Length,
			Item9.Length,
		];

		/// <inheritdoc />
		public int[] ByteSizes => [
			Item1.Length * Unsafe.SizeOf<T1>(),
			Item2.Length * Unsafe.SizeOf<T2>(),
			Item3.Length * Unsafe.SizeOf<T3>(),
			Item4.Length * Unsafe.SizeOf<T4>(),
			Item5.Length * Unsafe.SizeOf<T5>(),
			Item6.Length * Unsafe.SizeOf<T6>(),
			Item7.Length * Unsafe.SizeOf<T7>(),
			Item8.Length * Unsafe.SizeOf<T8>(),
			Item9.Length * Unsafe.SizeOf<T9>(),
		];

		/// <summary>
		/// Casts each item to T and then invokes a callback on the new span
		/// </summary>
		/// <typeparam name="T">Type to cast to</typeparam>
		/// <param name="callback">Callback to invoke for each item</param>
		public void ForEach<T>(ForEachSpanCallback<T> callback) where T: unmanaged 
		{
			callback(MemoryMarshal.Cast<T1, T>(Item1), 0);
			callback(MemoryMarshal.Cast<T2, T>(Item2), 1);
			callback(MemoryMarshal.Cast<T3, T>(Item3), 2);
			callback(MemoryMarshal.Cast<T4, T>(Item4), 3);
			callback(MemoryMarshal.Cast<T5, T>(Item5), 4);
			callback(MemoryMarshal.Cast<T6, T>(Item6), 5);
			callback(MemoryMarshal.Cast<T7, T>(Item7), 6);
			callback(MemoryMarshal.Cast<T8, T>(Item8), 7);
			callback(MemoryMarshal.Cast<T9, T>(Item9), 8);
		}
	}
	/// <summary>
	/// A tuple (size 10) of spans
	/// </summary>
	public readonly ref struct MultiTypeSpanTuple10<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : IAmTupleOfSpans
		where T1: unmanaged
		where T2: unmanaged
		where T3: unmanaged
		where T4: unmanaged
		where T5: unmanaged
		where T6: unmanaged
		where T7: unmanaged
		where T8: unmanaged
		where T9: unmanaged
		where T10: unmanaged
	{
		internal MultiTypeSpanTuple10(Span<T1> item1, Span<T2> item2, Span<T3> item3, Span<T4> item4, Span<T5> item5, Span<T6> item6, Span<T7> item7, Span<T8> item8, Span<T9> item9, Span<T10> item10)
		{
			Item1 = item1;
			Item2 = item2;
			Item3 = item3;
			Item4 = item4;
			Item5 = item5;
			Item6 = item6;
			Item7 = item7;
			Item8 = item8;
			Item9 = item9;
			Item10 = item10;
		}

		/// <summary>
		/// Item 1 in the tuple
		/// </summary>
		public Span<T1> Item1 { get; }

		/// <summary>
		/// Item 2 in the tuple
		/// </summary>
		public Span<T2> Item2 { get; }

		/// <summary>
		/// Item 3 in the tuple
		/// </summary>
		public Span<T3> Item3 { get; }

		/// <summary>
		/// Item 4 in the tuple
		/// </summary>
		public Span<T4> Item4 { get; }

		/// <summary>
		/// Item 5 in the tuple
		/// </summary>
		public Span<T5> Item5 { get; }

		/// <summary>
		/// Item 6 in the tuple
		/// </summary>
		public Span<T6> Item6 { get; }

		/// <summary>
		/// Item 7 in the tuple
		/// </summary>
		public Span<T7> Item7 { get; }

		/// <summary>
		/// Item 8 in the tuple
		/// </summary>
		public Span<T8> Item8 { get; }

		/// <summary>
		/// Item 9 in the tuple
		/// </summary>
		public Span<T9> Item9 { get; }

		/// <summary>
		/// Item 10 in the tuple
		/// </summary>
		public Span<T10> Item10 { get; }

		/// <inheritdoc />
		public int[] Sizes => [
			Item1.Length,
			Item2.Length,
			Item3.Length,
			Item4.Length,
			Item5.Length,
			Item6.Length,
			Item7.Length,
			Item8.Length,
			Item9.Length,
			Item10.Length,
		];

		/// <inheritdoc />
		public int[] ByteSizes => [
			Item1.Length * Unsafe.SizeOf<T1>(),
			Item2.Length * Unsafe.SizeOf<T2>(),
			Item3.Length * Unsafe.SizeOf<T3>(),
			Item4.Length * Unsafe.SizeOf<T4>(),
			Item5.Length * Unsafe.SizeOf<T5>(),
			Item6.Length * Unsafe.SizeOf<T6>(),
			Item7.Length * Unsafe.SizeOf<T7>(),
			Item8.Length * Unsafe.SizeOf<T8>(),
			Item9.Length * Unsafe.SizeOf<T9>(),
			Item10.Length * Unsafe.SizeOf<T10>(),
		];

		/// <inheritdoc />
		public void ForEach<T>(ForEachSpanCallback<T> callback) where T: unmanaged 
		{
			callback(MemoryMarshal.Cast<T1, T>(Item1), 0);
			callback(MemoryMarshal.Cast<T2, T>(Item2), 1);
			callback(MemoryMarshal.Cast<T3, T>(Item3), 2);
			callback(MemoryMarshal.Cast<T4, T>(Item4), 3);
			callback(MemoryMarshal.Cast<T5, T>(Item5), 4);
			callback(MemoryMarshal.Cast<T6, T>(Item6), 5);
			callback(MemoryMarshal.Cast<T7, T>(Item7), 6);
			callback(MemoryMarshal.Cast<T8, T>(Item8), 7);
			callback(MemoryMarshal.Cast<T9, T>(Item9), 8);
			callback(MemoryMarshal.Cast<T10, T>(Item10), 9);
		}
	}

	/// <summary>
	/// A tuple (size 10) of read only spans, each item 
	/// </summary>
	public readonly ref struct ReadOnlyMultiTypeSpanTuple10<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : IAmTupleOfSpans
		where T1: unmanaged
		where T2: unmanaged
		where T3: unmanaged
		where T4: unmanaged
		where T5: unmanaged
		where T6: unmanaged
		where T7: unmanaged
		where T8: unmanaged
		where T9: unmanaged
		where T10: unmanaged
	{
		internal ReadOnlyMultiTypeSpanTuple10(ReadOnlySpan<T1> item1, ReadOnlySpan<T2> item2, ReadOnlySpan<T3> item3, ReadOnlySpan<T4> item4, ReadOnlySpan<T5> item5, ReadOnlySpan<T6> item6, ReadOnlySpan<T7> item7, ReadOnlySpan<T8> item8, ReadOnlySpan<T9> item9, ReadOnlySpan<T10> item10)
		{
			Item1 = item1;
			Item2 = item2;
			Item3 = item3;
			Item4 = item4;
			Item5 = item5;
			Item6 = item6;
			Item7 = item7;
			Item8 = item8;
			Item9 = item9;
			Item10 = item10;
		}

		/// <summary>
		/// Item 1 in the tuple
		/// </summary>
		public ReadOnlySpan<T1> Item1 { get; }

		/// <summary>
		/// Item 2 in the tuple
		/// </summary>
		public ReadOnlySpan<T2> Item2 { get; }

		/// <summary>
		/// Item 3 in the tuple
		/// </summary>
		public ReadOnlySpan<T3> Item3 { get; }

		/// <summary>
		/// Item 4 in the tuple
		/// </summary>
		public ReadOnlySpan<T4> Item4 { get; }

		/// <summary>
		/// Item 5 in the tuple
		/// </summary>
		public ReadOnlySpan<T5> Item5 { get; }

		/// <summary>
		/// Item 6 in the tuple
		/// </summary>
		public ReadOnlySpan<T6> Item6 { get; }

		/// <summary>
		/// Item 7 in the tuple
		/// </summary>
		public ReadOnlySpan<T7> Item7 { get; }

		/// <summary>
		/// Item 8 in the tuple
		/// </summary>
		public ReadOnlySpan<T8> Item8 { get; }

		/// <summary>
		/// Item 9 in the tuple
		/// </summary>
		public ReadOnlySpan<T9> Item9 { get; }

		/// <summary>
		/// Item 10 in the tuple
		/// </summary>
		public ReadOnlySpan<T10> Item10 { get; }

		/// <inheritdoc />
		public int[] Sizes => [
			Item1.Length,
			Item2.Length,
			Item3.Length,
			Item4.Length,
			Item5.Length,
			Item6.Length,
			Item7.Length,
			Item8.Length,
			Item9.Length,
			Item10.Length,
		];

		/// <inheritdoc />
		public int[] ByteSizes => [
			Item1.Length * Unsafe.SizeOf<T1>(),
			Item2.Length * Unsafe.SizeOf<T2>(),
			Item3.Length * Unsafe.SizeOf<T3>(),
			Item4.Length * Unsafe.SizeOf<T4>(),
			Item5.Length * Unsafe.SizeOf<T5>(),
			Item6.Length * Unsafe.SizeOf<T6>(),
			Item7.Length * Unsafe.SizeOf<T7>(),
			Item8.Length * Unsafe.SizeOf<T8>(),
			Item9.Length * Unsafe.SizeOf<T9>(),
			Item10.Length * Unsafe.SizeOf<T10>(),
		];

		/// <summary>
		/// Casts each item to T and then invokes a callback on the new span
		/// </summary>
		/// <typeparam name="T">Type to cast to</typeparam>
		/// <param name="callback">Callback to invoke for each item</param>
		public void ForEach<T>(ForEachSpanCallback<T> callback) where T: unmanaged 
		{
			callback(MemoryMarshal.Cast<T1, T>(Item1), 0);
			callback(MemoryMarshal.Cast<T2, T>(Item2), 1);
			callback(MemoryMarshal.Cast<T3, T>(Item3), 2);
			callback(MemoryMarshal.Cast<T4, T>(Item4), 3);
			callback(MemoryMarshal.Cast<T5, T>(Item5), 4);
			callback(MemoryMarshal.Cast<T6, T>(Item6), 5);
			callback(MemoryMarshal.Cast<T7, T>(Item7), 6);
			callback(MemoryMarshal.Cast<T8, T>(Item8), 7);
			callback(MemoryMarshal.Cast<T9, T>(Item9), 8);
			callback(MemoryMarshal.Cast<T10, T>(Item10), 9);
		}
	}
	/// <summary>
	/// A tuple (size 11) of spans
	/// </summary>
	public readonly ref struct MultiTypeSpanTuple11<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> : IAmTupleOfSpans
		where T1: unmanaged
		where T2: unmanaged
		where T3: unmanaged
		where T4: unmanaged
		where T5: unmanaged
		where T6: unmanaged
		where T7: unmanaged
		where T8: unmanaged
		where T9: unmanaged
		where T10: unmanaged
		where T11: unmanaged
	{
		internal MultiTypeSpanTuple11(Span<T1> item1, Span<T2> item2, Span<T3> item3, Span<T4> item4, Span<T5> item5, Span<T6> item6, Span<T7> item7, Span<T8> item8, Span<T9> item9, Span<T10> item10, Span<T11> item11)
		{
			Item1 = item1;
			Item2 = item2;
			Item3 = item3;
			Item4 = item4;
			Item5 = item5;
			Item6 = item6;
			Item7 = item7;
			Item8 = item8;
			Item9 = item9;
			Item10 = item10;
			Item11 = item11;
		}

		/// <summary>
		/// Item 1 in the tuple
		/// </summary>
		public Span<T1> Item1 { get; }

		/// <summary>
		/// Item 2 in the tuple
		/// </summary>
		public Span<T2> Item2 { get; }

		/// <summary>
		/// Item 3 in the tuple
		/// </summary>
		public Span<T3> Item3 { get; }

		/// <summary>
		/// Item 4 in the tuple
		/// </summary>
		public Span<T4> Item4 { get; }

		/// <summary>
		/// Item 5 in the tuple
		/// </summary>
		public Span<T5> Item5 { get; }

		/// <summary>
		/// Item 6 in the tuple
		/// </summary>
		public Span<T6> Item6 { get; }

		/// <summary>
		/// Item 7 in the tuple
		/// </summary>
		public Span<T7> Item7 { get; }

		/// <summary>
		/// Item 8 in the tuple
		/// </summary>
		public Span<T8> Item8 { get; }

		/// <summary>
		/// Item 9 in the tuple
		/// </summary>
		public Span<T9> Item9 { get; }

		/// <summary>
		/// Item 10 in the tuple
		/// </summary>
		public Span<T10> Item10 { get; }

		/// <summary>
		/// Item 11 in the tuple
		/// </summary>
		public Span<T11> Item11 { get; }

		/// <inheritdoc />
		public int[] Sizes => [
			Item1.Length,
			Item2.Length,
			Item3.Length,
			Item4.Length,
			Item5.Length,
			Item6.Length,
			Item7.Length,
			Item8.Length,
			Item9.Length,
			Item10.Length,
			Item11.Length,
		];

		/// <inheritdoc />
		public int[] ByteSizes => [
			Item1.Length * Unsafe.SizeOf<T1>(),
			Item2.Length * Unsafe.SizeOf<T2>(),
			Item3.Length * Unsafe.SizeOf<T3>(),
			Item4.Length * Unsafe.SizeOf<T4>(),
			Item5.Length * Unsafe.SizeOf<T5>(),
			Item6.Length * Unsafe.SizeOf<T6>(),
			Item7.Length * Unsafe.SizeOf<T7>(),
			Item8.Length * Unsafe.SizeOf<T8>(),
			Item9.Length * Unsafe.SizeOf<T9>(),
			Item10.Length * Unsafe.SizeOf<T10>(),
			Item11.Length * Unsafe.SizeOf<T11>(),
		];

		/// <inheritdoc />
		public void ForEach<T>(ForEachSpanCallback<T> callback) where T: unmanaged 
		{
			callback(MemoryMarshal.Cast<T1, T>(Item1), 0);
			callback(MemoryMarshal.Cast<T2, T>(Item2), 1);
			callback(MemoryMarshal.Cast<T3, T>(Item3), 2);
			callback(MemoryMarshal.Cast<T4, T>(Item4), 3);
			callback(MemoryMarshal.Cast<T5, T>(Item5), 4);
			callback(MemoryMarshal.Cast<T6, T>(Item6), 5);
			callback(MemoryMarshal.Cast<T7, T>(Item7), 6);
			callback(MemoryMarshal.Cast<T8, T>(Item8), 7);
			callback(MemoryMarshal.Cast<T9, T>(Item9), 8);
			callback(MemoryMarshal.Cast<T10, T>(Item10), 9);
			callback(MemoryMarshal.Cast<T11, T>(Item11), 10);
		}
	}

	/// <summary>
	/// A tuple (size 11) of read only spans, each item 
	/// </summary>
	public readonly ref struct ReadOnlyMultiTypeSpanTuple11<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> : IAmTupleOfSpans
		where T1: unmanaged
		where T2: unmanaged
		where T3: unmanaged
		where T4: unmanaged
		where T5: unmanaged
		where T6: unmanaged
		where T7: unmanaged
		where T8: unmanaged
		where T9: unmanaged
		where T10: unmanaged
		where T11: unmanaged
	{
		internal ReadOnlyMultiTypeSpanTuple11(ReadOnlySpan<T1> item1, ReadOnlySpan<T2> item2, ReadOnlySpan<T3> item3, ReadOnlySpan<T4> item4, ReadOnlySpan<T5> item5, ReadOnlySpan<T6> item6, ReadOnlySpan<T7> item7, ReadOnlySpan<T8> item8, ReadOnlySpan<T9> item9, ReadOnlySpan<T10> item10, ReadOnlySpan<T11> item11)
		{
			Item1 = item1;
			Item2 = item2;
			Item3 = item3;
			Item4 = item4;
			Item5 = item5;
			Item6 = item6;
			Item7 = item7;
			Item8 = item8;
			Item9 = item9;
			Item10 = item10;
			Item11 = item11;
		}

		/// <summary>
		/// Item 1 in the tuple
		/// </summary>
		public ReadOnlySpan<T1> Item1 { get; }

		/// <summary>
		/// Item 2 in the tuple
		/// </summary>
		public ReadOnlySpan<T2> Item2 { get; }

		/// <summary>
		/// Item 3 in the tuple
		/// </summary>
		public ReadOnlySpan<T3> Item3 { get; }

		/// <summary>
		/// Item 4 in the tuple
		/// </summary>
		public ReadOnlySpan<T4> Item4 { get; }

		/// <summary>
		/// Item 5 in the tuple
		/// </summary>
		public ReadOnlySpan<T5> Item5 { get; }

		/// <summary>
		/// Item 6 in the tuple
		/// </summary>
		public ReadOnlySpan<T6> Item6 { get; }

		/// <summary>
		/// Item 7 in the tuple
		/// </summary>
		public ReadOnlySpan<T7> Item7 { get; }

		/// <summary>
		/// Item 8 in the tuple
		/// </summary>
		public ReadOnlySpan<T8> Item8 { get; }

		/// <summary>
		/// Item 9 in the tuple
		/// </summary>
		public ReadOnlySpan<T9> Item9 { get; }

		/// <summary>
		/// Item 10 in the tuple
		/// </summary>
		public ReadOnlySpan<T10> Item10 { get; }

		/// <summary>
		/// Item 11 in the tuple
		/// </summary>
		public ReadOnlySpan<T11> Item11 { get; }

		/// <inheritdoc />
		public int[] Sizes => [
			Item1.Length,
			Item2.Length,
			Item3.Length,
			Item4.Length,
			Item5.Length,
			Item6.Length,
			Item7.Length,
			Item8.Length,
			Item9.Length,
			Item10.Length,
			Item11.Length,
		];

		/// <inheritdoc />
		public int[] ByteSizes => [
			Item1.Length * Unsafe.SizeOf<T1>(),
			Item2.Length * Unsafe.SizeOf<T2>(),
			Item3.Length * Unsafe.SizeOf<T3>(),
			Item4.Length * Unsafe.SizeOf<T4>(),
			Item5.Length * Unsafe.SizeOf<T5>(),
			Item6.Length * Unsafe.SizeOf<T6>(),
			Item7.Length * Unsafe.SizeOf<T7>(),
			Item8.Length * Unsafe.SizeOf<T8>(),
			Item9.Length * Unsafe.SizeOf<T9>(),
			Item10.Length * Unsafe.SizeOf<T10>(),
			Item11.Length * Unsafe.SizeOf<T11>(),
		];

		/// <summary>
		/// Casts each item to T and then invokes a callback on the new span
		/// </summary>
		/// <typeparam name="T">Type to cast to</typeparam>
		/// <param name="callback">Callback to invoke for each item</param>
		public void ForEach<T>(ForEachSpanCallback<T> callback) where T: unmanaged 
		{
			callback(MemoryMarshal.Cast<T1, T>(Item1), 0);
			callback(MemoryMarshal.Cast<T2, T>(Item2), 1);
			callback(MemoryMarshal.Cast<T3, T>(Item3), 2);
			callback(MemoryMarshal.Cast<T4, T>(Item4), 3);
			callback(MemoryMarshal.Cast<T5, T>(Item5), 4);
			callback(MemoryMarshal.Cast<T6, T>(Item6), 5);
			callback(MemoryMarshal.Cast<T7, T>(Item7), 6);
			callback(MemoryMarshal.Cast<T8, T>(Item8), 7);
			callback(MemoryMarshal.Cast<T9, T>(Item9), 8);
			callback(MemoryMarshal.Cast<T10, T>(Item10), 9);
			callback(MemoryMarshal.Cast<T11, T>(Item11), 10);
		}
	}
	/// <summary>
	/// A tuple (size 12) of spans
	/// </summary>
	public readonly ref struct MultiTypeSpanTuple12<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> : IAmTupleOfSpans
		where T1: unmanaged
		where T2: unmanaged
		where T3: unmanaged
		where T4: unmanaged
		where T5: unmanaged
		where T6: unmanaged
		where T7: unmanaged
		where T8: unmanaged
		where T9: unmanaged
		where T10: unmanaged
		where T11: unmanaged
		where T12: unmanaged
	{
		internal MultiTypeSpanTuple12(Span<T1> item1, Span<T2> item2, Span<T3> item3, Span<T4> item4, Span<T5> item5, Span<T6> item6, Span<T7> item7, Span<T8> item8, Span<T9> item9, Span<T10> item10, Span<T11> item11, Span<T12> item12)
		{
			Item1 = item1;
			Item2 = item2;
			Item3 = item3;
			Item4 = item4;
			Item5 = item5;
			Item6 = item6;
			Item7 = item7;
			Item8 = item8;
			Item9 = item9;
			Item10 = item10;
			Item11 = item11;
			Item12 = item12;
		}

		/// <summary>
		/// Item 1 in the tuple
		/// </summary>
		public Span<T1> Item1 { get; }

		/// <summary>
		/// Item 2 in the tuple
		/// </summary>
		public Span<T2> Item2 { get; }

		/// <summary>
		/// Item 3 in the tuple
		/// </summary>
		public Span<T3> Item3 { get; }

		/// <summary>
		/// Item 4 in the tuple
		/// </summary>
		public Span<T4> Item4 { get; }

		/// <summary>
		/// Item 5 in the tuple
		/// </summary>
		public Span<T5> Item5 { get; }

		/// <summary>
		/// Item 6 in the tuple
		/// </summary>
		public Span<T6> Item6 { get; }

		/// <summary>
		/// Item 7 in the tuple
		/// </summary>
		public Span<T7> Item7 { get; }

		/// <summary>
		/// Item 8 in the tuple
		/// </summary>
		public Span<T8> Item8 { get; }

		/// <summary>
		/// Item 9 in the tuple
		/// </summary>
		public Span<T9> Item9 { get; }

		/// <summary>
		/// Item 10 in the tuple
		/// </summary>
		public Span<T10> Item10 { get; }

		/// <summary>
		/// Item 11 in the tuple
		/// </summary>
		public Span<T11> Item11 { get; }

		/// <summary>
		/// Item 12 in the tuple
		/// </summary>
		public Span<T12> Item12 { get; }

		/// <inheritdoc />
		public int[] Sizes => [
			Item1.Length,
			Item2.Length,
			Item3.Length,
			Item4.Length,
			Item5.Length,
			Item6.Length,
			Item7.Length,
			Item8.Length,
			Item9.Length,
			Item10.Length,
			Item11.Length,
			Item12.Length,
		];

		/// <inheritdoc />
		public int[] ByteSizes => [
			Item1.Length * Unsafe.SizeOf<T1>(),
			Item2.Length * Unsafe.SizeOf<T2>(),
			Item3.Length * Unsafe.SizeOf<T3>(),
			Item4.Length * Unsafe.SizeOf<T4>(),
			Item5.Length * Unsafe.SizeOf<T5>(),
			Item6.Length * Unsafe.SizeOf<T6>(),
			Item7.Length * Unsafe.SizeOf<T7>(),
			Item8.Length * Unsafe.SizeOf<T8>(),
			Item9.Length * Unsafe.SizeOf<T9>(),
			Item10.Length * Unsafe.SizeOf<T10>(),
			Item11.Length * Unsafe.SizeOf<T11>(),
			Item12.Length * Unsafe.SizeOf<T12>(),
		];

		/// <inheritdoc />
		public void ForEach<T>(ForEachSpanCallback<T> callback) where T: unmanaged 
		{
			callback(MemoryMarshal.Cast<T1, T>(Item1), 0);
			callback(MemoryMarshal.Cast<T2, T>(Item2), 1);
			callback(MemoryMarshal.Cast<T3, T>(Item3), 2);
			callback(MemoryMarshal.Cast<T4, T>(Item4), 3);
			callback(MemoryMarshal.Cast<T5, T>(Item5), 4);
			callback(MemoryMarshal.Cast<T6, T>(Item6), 5);
			callback(MemoryMarshal.Cast<T7, T>(Item7), 6);
			callback(MemoryMarshal.Cast<T8, T>(Item8), 7);
			callback(MemoryMarshal.Cast<T9, T>(Item9), 8);
			callback(MemoryMarshal.Cast<T10, T>(Item10), 9);
			callback(MemoryMarshal.Cast<T11, T>(Item11), 10);
			callback(MemoryMarshal.Cast<T12, T>(Item12), 11);
		}
	}

	/// <summary>
	/// A tuple (size 12) of read only spans, each item 
	/// </summary>
	public readonly ref struct ReadOnlyMultiTypeSpanTuple12<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> : IAmTupleOfSpans
		where T1: unmanaged
		where T2: unmanaged
		where T3: unmanaged
		where T4: unmanaged
		where T5: unmanaged
		where T6: unmanaged
		where T7: unmanaged
		where T8: unmanaged
		where T9: unmanaged
		where T10: unmanaged
		where T11: unmanaged
		where T12: unmanaged
	{
		internal ReadOnlyMultiTypeSpanTuple12(ReadOnlySpan<T1> item1, ReadOnlySpan<T2> item2, ReadOnlySpan<T3> item3, ReadOnlySpan<T4> item4, ReadOnlySpan<T5> item5, ReadOnlySpan<T6> item6, ReadOnlySpan<T7> item7, ReadOnlySpan<T8> item8, ReadOnlySpan<T9> item9, ReadOnlySpan<T10> item10, ReadOnlySpan<T11> item11, ReadOnlySpan<T12> item12)
		{
			Item1 = item1;
			Item2 = item2;
			Item3 = item3;
			Item4 = item4;
			Item5 = item5;
			Item6 = item6;
			Item7 = item7;
			Item8 = item8;
			Item9 = item9;
			Item10 = item10;
			Item11 = item11;
			Item12 = item12;
		}

		/// <summary>
		/// Item 1 in the tuple
		/// </summary>
		public ReadOnlySpan<T1> Item1 { get; }

		/// <summary>
		/// Item 2 in the tuple
		/// </summary>
		public ReadOnlySpan<T2> Item2 { get; }

		/// <summary>
		/// Item 3 in the tuple
		/// </summary>
		public ReadOnlySpan<T3> Item3 { get; }

		/// <summary>
		/// Item 4 in the tuple
		/// </summary>
		public ReadOnlySpan<T4> Item4 { get; }

		/// <summary>
		/// Item 5 in the tuple
		/// </summary>
		public ReadOnlySpan<T5> Item5 { get; }

		/// <summary>
		/// Item 6 in the tuple
		/// </summary>
		public ReadOnlySpan<T6> Item6 { get; }

		/// <summary>
		/// Item 7 in the tuple
		/// </summary>
		public ReadOnlySpan<T7> Item7 { get; }

		/// <summary>
		/// Item 8 in the tuple
		/// </summary>
		public ReadOnlySpan<T8> Item8 { get; }

		/// <summary>
		/// Item 9 in the tuple
		/// </summary>
		public ReadOnlySpan<T9> Item9 { get; }

		/// <summary>
		/// Item 10 in the tuple
		/// </summary>
		public ReadOnlySpan<T10> Item10 { get; }

		/// <summary>
		/// Item 11 in the tuple
		/// </summary>
		public ReadOnlySpan<T11> Item11 { get; }

		/// <summary>
		/// Item 12 in the tuple
		/// </summary>
		public ReadOnlySpan<T12> Item12 { get; }

		/// <inheritdoc />
		public int[] Sizes => [
			Item1.Length,
			Item2.Length,
			Item3.Length,
			Item4.Length,
			Item5.Length,
			Item6.Length,
			Item7.Length,
			Item8.Length,
			Item9.Length,
			Item10.Length,
			Item11.Length,
			Item12.Length,
		];

		/// <inheritdoc />
		public int[] ByteSizes => [
			Item1.Length * Unsafe.SizeOf<T1>(),
			Item2.Length * Unsafe.SizeOf<T2>(),
			Item3.Length * Unsafe.SizeOf<T3>(),
			Item4.Length * Unsafe.SizeOf<T4>(),
			Item5.Length * Unsafe.SizeOf<T5>(),
			Item6.Length * Unsafe.SizeOf<T6>(),
			Item7.Length * Unsafe.SizeOf<T7>(),
			Item8.Length * Unsafe.SizeOf<T8>(),
			Item9.Length * Unsafe.SizeOf<T9>(),
			Item10.Length * Unsafe.SizeOf<T10>(),
			Item11.Length * Unsafe.SizeOf<T11>(),
			Item12.Length * Unsafe.SizeOf<T12>(),
		];

		/// <summary>
		/// Casts each item to T and then invokes a callback on the new span
		/// </summary>
		/// <typeparam name="T">Type to cast to</typeparam>
		/// <param name="callback">Callback to invoke for each item</param>
		public void ForEach<T>(ForEachSpanCallback<T> callback) where T: unmanaged 
		{
			callback(MemoryMarshal.Cast<T1, T>(Item1), 0);
			callback(MemoryMarshal.Cast<T2, T>(Item2), 1);
			callback(MemoryMarshal.Cast<T3, T>(Item3), 2);
			callback(MemoryMarshal.Cast<T4, T>(Item4), 3);
			callback(MemoryMarshal.Cast<T5, T>(Item5), 4);
			callback(MemoryMarshal.Cast<T6, T>(Item6), 5);
			callback(MemoryMarshal.Cast<T7, T>(Item7), 6);
			callback(MemoryMarshal.Cast<T8, T>(Item8), 7);
			callback(MemoryMarshal.Cast<T9, T>(Item9), 8);
			callback(MemoryMarshal.Cast<T10, T>(Item10), 9);
			callback(MemoryMarshal.Cast<T11, T>(Item11), 10);
			callback(MemoryMarshal.Cast<T12, T>(Item12), 11);
		}
	}
}

namespace BrightData
{
	public static partial class ExtensionMethods
	{
		/// <summary>
		/// Reads the data block that has a header of <see cref="BlockHeader" /> and returns a tuple of read only memory corresponding to each data block
		/// </summary>
		public static (ReadOnlyMemory<T1> Item1, ReadOnlyMemory<T2> Item2) GetTupleFromBlockHeader<T1, T2>(this ReadOnlyMemory<byte> data)
			where T1: unmanaged
			where T2: unmanaged
		{
			var span = data.Span;
            var header = span[..(2 * BlockHeader.StructSize)].Cast<byte, BlockHeader>();
			return (
				header[0].Get(data).Cast<byte, T1>(),
				header[1].Get(data).Cast<byte, T2>()
			);
		}
		/// <summary>
		/// Reads the data block that has a header of <see cref="BlockHeader" /> and returns a tuple of read only memory corresponding to each data block
		/// </summary>
		public static (ReadOnlyMemory<T1> Item1, ReadOnlyMemory<T2> Item2, ReadOnlyMemory<T3> Item3) GetTupleFromBlockHeader<T1, T2, T3>(this ReadOnlyMemory<byte> data)
			where T1: unmanaged
			where T2: unmanaged
			where T3: unmanaged
		{
			var span = data.Span;
            var header = span[..(3 * BlockHeader.StructSize)].Cast<byte, BlockHeader>();
			return (
				header[0].Get(data).Cast<byte, T1>(),
				header[1].Get(data).Cast<byte, T2>(),
				header[2].Get(data).Cast<byte, T3>()
			);
		}
		/// <summary>
		/// Reads the data block that has a header of <see cref="BlockHeader" /> and returns a tuple of read only memory corresponding to each data block
		/// </summary>
		public static (ReadOnlyMemory<T1> Item1, ReadOnlyMemory<T2> Item2, ReadOnlyMemory<T3> Item3, ReadOnlyMemory<T4> Item4) GetTupleFromBlockHeader<T1, T2, T3, T4>(this ReadOnlyMemory<byte> data)
			where T1: unmanaged
			where T2: unmanaged
			where T3: unmanaged
			where T4: unmanaged
		{
			var span = data.Span;
            var header = span[..(4 * BlockHeader.StructSize)].Cast<byte, BlockHeader>();
			return (
				header[0].Get(data).Cast<byte, T1>(),
				header[1].Get(data).Cast<byte, T2>(),
				header[2].Get(data).Cast<byte, T3>(),
				header[3].Get(data).Cast<byte, T4>()
			);
		}
		/// <summary>
		/// Reads the data block that has a header of <see cref="BlockHeader" /> and returns a tuple of read only memory corresponding to each data block
		/// </summary>
		public static (ReadOnlyMemory<T1> Item1, ReadOnlyMemory<T2> Item2, ReadOnlyMemory<T3> Item3, ReadOnlyMemory<T4> Item4, ReadOnlyMemory<T5> Item5) GetTupleFromBlockHeader<T1, T2, T3, T4, T5>(this ReadOnlyMemory<byte> data)
			where T1: unmanaged
			where T2: unmanaged
			where T3: unmanaged
			where T4: unmanaged
			where T5: unmanaged
		{
			var span = data.Span;
            var header = span[..(5 * BlockHeader.StructSize)].Cast<byte, BlockHeader>();
			return (
				header[0].Get(data).Cast<byte, T1>(),
				header[1].Get(data).Cast<byte, T2>(),
				header[2].Get(data).Cast<byte, T3>(),
				header[3].Get(data).Cast<byte, T4>(),
				header[4].Get(data).Cast<byte, T5>()
			);
		}
		/// <summary>
		/// Reads the data block that has a header of <see cref="BlockHeader" /> and returns a tuple of read only memory corresponding to each data block
		/// </summary>
		public static (ReadOnlyMemory<T1> Item1, ReadOnlyMemory<T2> Item2, ReadOnlyMemory<T3> Item3, ReadOnlyMemory<T4> Item4, ReadOnlyMemory<T5> Item5, ReadOnlyMemory<T6> Item6) GetTupleFromBlockHeader<T1, T2, T3, T4, T5, T6>(this ReadOnlyMemory<byte> data)
			where T1: unmanaged
			where T2: unmanaged
			where T3: unmanaged
			where T4: unmanaged
			where T5: unmanaged
			where T6: unmanaged
		{
			var span = data.Span;
            var header = span[..(6 * BlockHeader.StructSize)].Cast<byte, BlockHeader>();
			return (
				header[0].Get(data).Cast<byte, T1>(),
				header[1].Get(data).Cast<byte, T2>(),
				header[2].Get(data).Cast<byte, T3>(),
				header[3].Get(data).Cast<byte, T4>(),
				header[4].Get(data).Cast<byte, T5>(),
				header[5].Get(data).Cast<byte, T6>()
			);
		}
		/// <summary>
		/// Reads the data block that has a header of <see cref="BlockHeader" /> and returns a tuple of read only memory corresponding to each data block
		/// </summary>
		public static (ReadOnlyMemory<T1> Item1, ReadOnlyMemory<T2> Item2, ReadOnlyMemory<T3> Item3, ReadOnlyMemory<T4> Item4, ReadOnlyMemory<T5> Item5, ReadOnlyMemory<T6> Item6, ReadOnlyMemory<T7> Item7) GetTupleFromBlockHeader<T1, T2, T3, T4, T5, T6, T7>(this ReadOnlyMemory<byte> data)
			where T1: unmanaged
			where T2: unmanaged
			where T3: unmanaged
			where T4: unmanaged
			where T5: unmanaged
			where T6: unmanaged
			where T7: unmanaged
		{
			var span = data.Span;
            var header = span[..(7 * BlockHeader.StructSize)].Cast<byte, BlockHeader>();
			return (
				header[0].Get(data).Cast<byte, T1>(),
				header[1].Get(data).Cast<byte, T2>(),
				header[2].Get(data).Cast<byte, T3>(),
				header[3].Get(data).Cast<byte, T4>(),
				header[4].Get(data).Cast<byte, T5>(),
				header[5].Get(data).Cast<byte, T6>(),
				header[6].Get(data).Cast<byte, T7>()
			);
		}
		/// <summary>
		/// Reads the data block that has a header of <see cref="BlockHeader" /> and returns a tuple of read only memory corresponding to each data block
		/// </summary>
		public static (ReadOnlyMemory<T1> Item1, ReadOnlyMemory<T2> Item2, ReadOnlyMemory<T3> Item3, ReadOnlyMemory<T4> Item4, ReadOnlyMemory<T5> Item5, ReadOnlyMemory<T6> Item6, ReadOnlyMemory<T7> Item7, ReadOnlyMemory<T8> Item8) GetTupleFromBlockHeader<T1, T2, T3, T4, T5, T6, T7, T8>(this ReadOnlyMemory<byte> data)
			where T1: unmanaged
			where T2: unmanaged
			where T3: unmanaged
			where T4: unmanaged
			where T5: unmanaged
			where T6: unmanaged
			where T7: unmanaged
			where T8: unmanaged
		{
			var span = data.Span;
            var header = span[..(8 * BlockHeader.StructSize)].Cast<byte, BlockHeader>();
			return (
				header[0].Get(data).Cast<byte, T1>(),
				header[1].Get(data).Cast<byte, T2>(),
				header[2].Get(data).Cast<byte, T3>(),
				header[3].Get(data).Cast<byte, T4>(),
				header[4].Get(data).Cast<byte, T5>(),
				header[5].Get(data).Cast<byte, T6>(),
				header[6].Get(data).Cast<byte, T7>(),
				header[7].Get(data).Cast<byte, T8>()
			);
		}
		/// <summary>
		/// Reads the data block that has a header of <see cref="BlockHeader" /> and returns a tuple of read only memory corresponding to each data block
		/// </summary>
		public static (ReadOnlyMemory<T1> Item1, ReadOnlyMemory<T2> Item2, ReadOnlyMemory<T3> Item3, ReadOnlyMemory<T4> Item4, ReadOnlyMemory<T5> Item5, ReadOnlyMemory<T6> Item6, ReadOnlyMemory<T7> Item7, ReadOnlyMemory<T8> Item8, ReadOnlyMemory<T9> Item9) GetTupleFromBlockHeader<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this ReadOnlyMemory<byte> data)
			where T1: unmanaged
			where T2: unmanaged
			where T3: unmanaged
			where T4: unmanaged
			where T5: unmanaged
			where T6: unmanaged
			where T7: unmanaged
			where T8: unmanaged
			where T9: unmanaged
		{
			var span = data.Span;
            var header = span[..(9 * BlockHeader.StructSize)].Cast<byte, BlockHeader>();
			return (
				header[0].Get(data).Cast<byte, T1>(),
				header[1].Get(data).Cast<byte, T2>(),
				header[2].Get(data).Cast<byte, T3>(),
				header[3].Get(data).Cast<byte, T4>(),
				header[4].Get(data).Cast<byte, T5>(),
				header[5].Get(data).Cast<byte, T6>(),
				header[6].Get(data).Cast<byte, T7>(),
				header[7].Get(data).Cast<byte, T8>(),
				header[8].Get(data).Cast<byte, T9>()
			);
		}
		/// <summary>
		/// Reads the data block that has a header of <see cref="BlockHeader" /> and returns a tuple of read only memory corresponding to each data block
		/// </summary>
		public static (ReadOnlyMemory<T1> Item1, ReadOnlyMemory<T2> Item2, ReadOnlyMemory<T3> Item3, ReadOnlyMemory<T4> Item4, ReadOnlyMemory<T5> Item5, ReadOnlyMemory<T6> Item6, ReadOnlyMemory<T7> Item7, ReadOnlyMemory<T8> Item8, ReadOnlyMemory<T9> Item9, ReadOnlyMemory<T10> Item10) GetTupleFromBlockHeader<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this ReadOnlyMemory<byte> data)
			where T1: unmanaged
			where T2: unmanaged
			where T3: unmanaged
			where T4: unmanaged
			where T5: unmanaged
			where T6: unmanaged
			where T7: unmanaged
			where T8: unmanaged
			where T9: unmanaged
			where T10: unmanaged
		{
			var span = data.Span;
            var header = span[..(10 * BlockHeader.StructSize)].Cast<byte, BlockHeader>();
			return (
				header[0].Get(data).Cast<byte, T1>(),
				header[1].Get(data).Cast<byte, T2>(),
				header[2].Get(data).Cast<byte, T3>(),
				header[3].Get(data).Cast<byte, T4>(),
				header[4].Get(data).Cast<byte, T5>(),
				header[5].Get(data).Cast<byte, T6>(),
				header[6].Get(data).Cast<byte, T7>(),
				header[7].Get(data).Cast<byte, T8>(),
				header[8].Get(data).Cast<byte, T9>(),
				header[9].Get(data).Cast<byte, T10>()
			);
		}
		/// <summary>
		/// Reads the data block that has a header of <see cref="BlockHeader" /> and returns a tuple of read only memory corresponding to each data block
		/// </summary>
		public static (ReadOnlyMemory<T1> Item1, ReadOnlyMemory<T2> Item2, ReadOnlyMemory<T3> Item3, ReadOnlyMemory<T4> Item4, ReadOnlyMemory<T5> Item5, ReadOnlyMemory<T6> Item6, ReadOnlyMemory<T7> Item7, ReadOnlyMemory<T8> Item8, ReadOnlyMemory<T9> Item9, ReadOnlyMemory<T10> Item10, ReadOnlyMemory<T11> Item11) GetTupleFromBlockHeader<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(this ReadOnlyMemory<byte> data)
			where T1: unmanaged
			where T2: unmanaged
			where T3: unmanaged
			where T4: unmanaged
			where T5: unmanaged
			where T6: unmanaged
			where T7: unmanaged
			where T8: unmanaged
			where T9: unmanaged
			where T10: unmanaged
			where T11: unmanaged
		{
			var span = data.Span;
            var header = span[..(11 * BlockHeader.StructSize)].Cast<byte, BlockHeader>();
			return (
				header[0].Get(data).Cast<byte, T1>(),
				header[1].Get(data).Cast<byte, T2>(),
				header[2].Get(data).Cast<byte, T3>(),
				header[3].Get(data).Cast<byte, T4>(),
				header[4].Get(data).Cast<byte, T5>(),
				header[5].Get(data).Cast<byte, T6>(),
				header[6].Get(data).Cast<byte, T7>(),
				header[7].Get(data).Cast<byte, T8>(),
				header[8].Get(data).Cast<byte, T9>(),
				header[9].Get(data).Cast<byte, T10>(),
				header[10].Get(data).Cast<byte, T11>()
			);
		}
		/// <summary>
		/// Reads the data block that has a header of <see cref="BlockHeader" /> and returns a tuple of read only memory corresponding to each data block
		/// </summary>
		public static (ReadOnlyMemory<T1> Item1, ReadOnlyMemory<T2> Item2, ReadOnlyMemory<T3> Item3, ReadOnlyMemory<T4> Item4, ReadOnlyMemory<T5> Item5, ReadOnlyMemory<T6> Item6, ReadOnlyMemory<T7> Item7, ReadOnlyMemory<T8> Item8, ReadOnlyMemory<T9> Item9, ReadOnlyMemory<T10> Item10, ReadOnlyMemory<T11> Item11, ReadOnlyMemory<T12> Item12) GetTupleFromBlockHeader<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(this ReadOnlyMemory<byte> data)
			where T1: unmanaged
			where T2: unmanaged
			where T3: unmanaged
			where T4: unmanaged
			where T5: unmanaged
			where T6: unmanaged
			where T7: unmanaged
			where T8: unmanaged
			where T9: unmanaged
			where T10: unmanaged
			where T11: unmanaged
			where T12: unmanaged
		{
			var span = data.Span;
            var header = span[..(12 * BlockHeader.StructSize)].Cast<byte, BlockHeader>();
			return (
				header[0].Get(data).Cast<byte, T1>(),
				header[1].Get(data).Cast<byte, T2>(),
				header[2].Get(data).Cast<byte, T3>(),
				header[3].Get(data).Cast<byte, T4>(),
				header[4].Get(data).Cast<byte, T5>(),
				header[5].Get(data).Cast<byte, T6>(),
				header[6].Get(data).Cast<byte, T7>(),
				header[7].Get(data).Cast<byte, T8>(),
				header[8].Get(data).Cast<byte, T9>(),
				header[9].Get(data).Cast<byte, T10>(),
				header[10].Get(data).Cast<byte, T11>(),
				header[11].Get(data).Cast<byte, T12>()
			);
		}
	}
}

