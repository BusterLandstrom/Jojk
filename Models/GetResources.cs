using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;

namespace Jojk.Models
{
    public class GetResources
    {
        public static SolidColorBrush GetBrush(string key)
        {
            if (Application.Current!.Resources.TryGetResource(key, ThemeVariant.Default, out var res) && res is SolidColorBrush brush)
                return brush;
            return new(Colors.White);
        }
    }
}
