using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Visuals.Media.Imaging;
using DynamicData;
using SnipTranslator.MVVM.Graphics;

namespace SnipTranslator.MVVM.Models;
public struct ImageCroppedEventArgs
{
    public Bitmap CroppedBitmap { get; set; }
    public Point StartPoint { get; set; }
}
internal class FullScreenImage
{
    private readonly ImageContext _imgContext;
    private Window window;
    private Canvas canvas;
    private Point pointerPressedPoint;
    private Point pointerReleasedPoint;
    private Rectangle selectorRectangle;
    private Image _imageWindow;
    private bool isDragging;
    private TaskCompletionSource<Bitmap> _tcs;
    
    public event EventHandler<ImageCroppedEventArgs> OnImageCropped; 
    
    public FullScreenImage(ImageContext imgContext)
    {
        _imgContext = imgContext;

        canvas = new Canvas();
        window = new Window
        {
            Content = canvas,
            WindowState = WindowState.FullScreen,
            Topmost = true,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            CanResize = false,
            ExtendClientAreaToDecorationsHint = true,
            ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.NoChrome,
            ExtendClientAreaTitleBarHeightHint = -1,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            VerticalContentAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
        };
        
        window.KeyDown += Window_OnKeyDown;
    }

    private async void OnCanvasOnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if(!IsSelectionRectangleValid()) return;
        
        if(!e.GetCurrentPoint(canvas).Properties.IsLeftButtonPressed)
        {
            BeginCutImageAsync();
            OnImageCropped?.Invoke(this, new ImageCroppedEventArgs
            {
                CroppedBitmap = await EndCutImageAsync(),
                StartPoint = pointerPressedPoint
            });
            Close();
        }
    }

    private Task<Bitmap> EndCutImageAsync()
    {
        return _tcs.Task;
    }

    private void DrawImageOnCanvas()
    {
        Bitmap bitmap = _imgContext;
        _imageWindow = new Image
        {
            Source = bitmap,
            Stretch = Stretch.Uniform,
            StretchDirection = StretchDirection.Both,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Width = window.Bounds.Width,
            // Height = window.Bounds.Height,
        };
        canvas.Children.Add(_imageWindow);
        //Subscribe to events
        _imageWindow.PointerPressed += OnCanvasOnPointerPressed;
        _imageWindow.PointerMoved += OnCanvasOnPointerMoved;
        _imageWindow.PointerReleased += OnCanvasOnPointerReleased;
    }

    public void Show()
    {
        window.Show();
        
        window.InvalidateArrange();
        window.InvalidateVisual();
        window.InvalidateMeasure();
        
        DrawImageOnCanvas();
        DrawRectangleOnCanvas();
    }

    private void OnCanvasOnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (e.GetCurrentPoint(canvas).Properties.IsLeftButtonPressed)
        { 
            pointerReleasedPoint = e.GetPosition(canvas);
            UpdateRectangleOnCanvas();
        }
    }

    private void OnCanvasOnPointerPressed(object sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(canvas).Properties.IsLeftButtonPressed)
        {
            pointerPressedPoint = e.GetPosition(canvas);
        }
    }

    private void Window_OnKeyDown(Object sender, KeyEventArgs args)
    {
        if (args.Key == Avalonia.Input.Key.Escape)
        {
            Close();
        }
    }

    private void DrawRectangleOnCanvas()
    {
        selectorRectangle = new Rectangle
        {
            Stroke = Brushes.Red,
            StrokeThickness = 2,
            Width = Math.Abs(pointerReleasedPoint.X - pointerPressedPoint.X),
            Height = Math.Abs(pointerReleasedPoint.Y - pointerPressedPoint.Y),
            Margin = new Thickness(Math.Min(pointerReleasedPoint.X, pointerPressedPoint.X),
                Math.Min(pointerReleasedPoint.Y, pointerPressedPoint.Y), 0, 0)
        };
        canvas.Children.Add(selectorRectangle);
    }

    private void UpdateRectangleOnCanvas()
    {
        selectorRectangle.Width = Math.Abs(pointerReleasedPoint.X - pointerPressedPoint.X);
        selectorRectangle.Height = Math.Abs(pointerReleasedPoint.Y - pointerPressedPoint.Y);
        selectorRectangle.Margin = new Thickness(Math.Min(pointerReleasedPoint.X, pointerPressedPoint.X),
            Math.Min(pointerReleasedPoint.Y, pointerPressedPoint.Y), 0, 0);
    }
    
    private bool IsSelectionRectangleValid()
    {
        return Math.Abs(pointerReleasedPoint.X - pointerPressedPoint.X) > 0.0 && Math.Abs(pointerReleasedPoint.Y - pointerPressedPoint.Y) > 0.0;
    }
    
    private void BeginCutImageAsync()
    {
        int x = (int)selectorRectangle.Margin.Left;
        int y = (int)selectorRectangle.Margin.Top;
        int width = (int)selectorRectangle.Width;
        int height = (int)selectorRectangle.Height;
        //Render the image to a bitmap
        var rawImage = new RenderTargetBitmap(new PixelSize((int)_imageWindow.Bounds.Width, (int)_imageWindow.Bounds.Height));
        rawImage.Render(_imageWindow);
        //Crop the bitmap
        CroppedBitmap  croppedBitmap = new CroppedBitmap(rawImage, new PixelRect(x, y, width, height));
        var image = new Image()
        {
            Source = croppedBitmap
        };
        canvas.Children.Add(image);
        
        _tcs = new TaskCompletionSource<Bitmap>();
        //Wait for the image to be rendered
        window.LayoutUpdated += ImageCut_WindowOnLayoutUpdated;
    }

    private void ImageCut_WindowOnLayoutUpdated(object? sender, EventArgs e)
    {
        //Remove the event handler
        window.LayoutUpdated -= ImageCut_WindowOnLayoutUpdated;
        //Get the size of the image
        int width = (int)selectorRectangle.Width;
        int height = (int)selectorRectangle.Height;
        //Create a new target bitmap
        var target = new RenderTargetBitmap(new PixelSize(width, height));
        //Finally render the image to the target
        target.Render(canvas.Children.Last());
        _tcs.SetResult(target);
    }

    private void Close()
    {
        window.Close();
    }
}