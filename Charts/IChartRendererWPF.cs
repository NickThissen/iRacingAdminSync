using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace Swordfish.NET.Charts {
  public interface IChartRendererWPF {
    void RenderFilledElements(DrawingContext ctx, Rect chartArea, Transform transform);
    void RenderUnfilledElements(DrawingContext ctx, Rect chartArea, Transform transform);
  }
}
