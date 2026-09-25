using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace System;

internal readonly struct Range : IEquatable<System.Range>
{
	private static class HashHelpers
	{
		public static int Combine(int h1, int h2)
		{
			return (((h1 << 5) | (h1 >>> 27)) + h1) ^ h2;
		}
	}

	private static class ThrowHelper
	{
		[System.Diagnostics.CodeAnalysis.DoesNotReturn]
		public static void ThrowArgumentOutOfRangeException()
		{
			throw new ArgumentOutOfRangeException("length");
		}
	}

	public System.Index Start { get; }

	public System.Index End { get; }

	public static System.Range All => System.Index.Start..System.Index.End;

	public Range(System.Index start, System.Index end)
	{
		Start = start;
		End = end;
	}

	public override bool Equals([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] object? value)
	{
		if (value is System.Range { Start: var start } range && start.Equals(Start))
		{
			return range.End.Equals(End);
		}
		return false;
	}

	public bool Equals(System.Range other)
	{
		if (other.Start.Equals(Start))
		{
			return other.End.Equals(End);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashHelpers.Combine(Start.GetHashCode(), End.GetHashCode());
	}

	public override string ToString()
	{
		return Start.ToString() + ".." + End;
	}

	public static System.Range StartAt(System.Index start)
	{
		return start..System.Index.End;
	}

	public static System.Range EndAt(System.Index end)
	{
		return System.Index.Start..end;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public (int Offset, int Length) GetOffsetAndLength(int length)
	{
		System.Index start = Start;
		int num = ((!start.IsFromEnd) ? start.Value : (length - start.Value));
		System.Index end = End;
		int num2 = ((!end.IsFromEnd) ? end.Value : (length - end.Value));
		if ((uint)num2 > (uint)length || (uint)num > (uint)num2)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException();
		}
		return (Offset: num, Length: num2 - num);
	}
}
