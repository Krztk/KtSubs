using System.Windows.Media;

namespace KtSubs.Wpf.Globals
{
    public static class Brushes
    {
        public static Brush WordNumber { get; } = new SolidColorBrush(Color.FromRgb(117, 117, 117));
        public static Brush NormalText { get; } = new SolidColorBrush(Color.FromRgb(0, 0, 0));
        public static Brush HighlightedText { get; } = new SolidColorBrush(Color.FromRgb(229, 107, 0));
        public static Brush LayerName { get; } = new SolidColorBrush(Color.FromRgb(160, 160, 160));
    }
}