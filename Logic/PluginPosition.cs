using System.Windows;

namespace HDTPlugin.Logic
{
    public static class PluginPosition
    {
        public const double DefaultLeft = 1770;
        public const double DefaultTop = 0;

        public static Point GetDefaultPosition() => new Point(DefaultLeft, DefaultTop);
    }
}