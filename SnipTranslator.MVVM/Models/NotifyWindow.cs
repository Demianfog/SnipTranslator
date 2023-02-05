using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform;

namespace SnipTranslator.MVVM.Models;

public class NotifyWindow
{
    private Window _window;
    private TextBlock _textBlock;
    public bool IsVisible => _window.IsVisible;
    
    public NotifyWindow()
    {
        _textBlock = new TextBlock
        {
            Text = "Hello World!",
            Background = Brushes.White,
            Opacity = 0.85
        };
        Border border = new Border
        {
            BorderBrush = Brushes.Black,
            BorderThickness = new Thickness(1),
            Child = _textBlock,
        };
        _window = new Window
        {
            Content = border,
            Background = Brushes.Transparent,
            WindowState = WindowState.Normal,
            Topmost = true,
            TransparencyLevelHint = WindowTransparencyLevel.Transparent,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            CanResize = false,
            ExtendClientAreaToDecorationsHint = true,
            ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.NoChrome,
            ExtendClientAreaTitleBarHeightHint = -1,
            MinHeight = 0,
            MinWidth = 0,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            VerticalContentAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            SizeToContent = SizeToContent.WidthAndHeight,
        };
        _window.LostFocus += (sender, args) => { _window.Hide(); };
    }
    
    public void Show(string text, Rect rectangle, float opacity = 0.5f, int duration = 2000)
    {
        
        _window.Show();
        _textBlock.Text = text;
        _window.Position = new PixelPoint((int)rectangle.X, (int)rectangle.Y);
        _window.Width = rectangle.Width;
        _window.Height = rectangle.Height;
        _window.Opacity = opacity;
    }
    
    public void Show(string text, Point point, float opacity = 0.5f, int duration = 2000)
    {
        _window.Show();
        _textBlock.Text = text;
        _window.Opacity = opacity;
        _window.Position = new PixelPoint((int)point.X, (int)point.Y);
    }
    
    public void Hide()
    {
        _window.Hide();
    }
    
    public void Close()
    {
        _window.Close();
    }
}