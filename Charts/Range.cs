using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;
using System.Text;

namespace Swordfish.NET.Charts {
  public class Range<T> {
    public readonly T Start;
    public readonly T End;
    public Range(T start, T end) {
      Start = start;
      End = end;
    }
  }

  public class RangeAndColor<T> : Range<T> {
    public readonly Color Color;
    public RangeAndColor(T start, T end, Color color) : base(start, end) {
      Color = color;
    }
  }

  public static class RangeSpecializations {
    public static bool Contains(this Range<double> range, double value) {
      double min = Math.Min(range.Start, range.End);
      double max = Math.Max(range.Start, range.End);
      return min <= value && value <= max;
    }
    public static bool Contains(this Range<int> range, double value) {
      int min = Math.Min(range.Start, range.End);
      int max = Math.Max(range.Start, range.End);
      return min <= value && value <= max;
    }
    public static bool Intersects(this Range<double> range, Range<double> value) {
      double min1 = Math.Min(range.Start, range.End);
      double max1 = Math.Max(range.Start, range.End);
      double min2 = Math.Min(value.Start, value.End);
      double max2 = Math.Max(value.Start, value.End);
      return max1 > min2 && min1 < max2;
    }
    public static bool Intersects(this Range<int> range, Range<int> value) {
      int min1 = Math.Min(range.Start, range.End);
      int max1 = Math.Max(range.Start, range.End);
      int min2 = Math.Min(value.Start, value.End);
      int max2 = Math.Max(value.Start, value.End);
      return max1 > min2 && min1 < max2;
    }
  }
}
