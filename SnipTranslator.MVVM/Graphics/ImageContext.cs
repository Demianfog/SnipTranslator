using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using SnipTranslator.MVVM.Extensions;
using Tesseract;

namespace SnipTranslator.MVVM.Graphics;

public class ImageContext : IDisposable
{
    public int Height => (int)_bitmap.Size.Height;
    public int Width => (int)_bitmap.Size.Width;
    private Bitmap _bitmap;
    private readonly TesseractEngine _engine;
    private string? _text;
    public ImageContext(string path, TesseractEngine engine)
    {
        _bitmap = new Bitmap(path);
        _engine = engine;
    }
    public ImageContext(Bitmap bitmap, TesseractEngine engine)
    {
        _bitmap = bitmap;
        _engine = engine;
    }

    public Bitmap GetBitmap()
    {
        return _bitmap;
    }
    
    public string GetText()
    {
        if (_text == null)
        {
            Pix img = _bitmap.ToPixImage();
            using Page? page = _engine.Process(img);
            _text = page.GetText();
        }

        return _text;
    }

    public void Dispose()
    {
        _bitmap.Dispose();
        _engine.Dispose();
    }
    
    
    public static implicit operator Bitmap(ImageContext imageContext)
    {
        return imageContext._bitmap;
    }
}