using System;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SkiaSharp;
using Device = SharpDX.Direct3D11.Device;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using SnipTranslator.MVVM.Graphics;
using Tesseract;
using Bitmap = Avalonia.Media.Imaging.Bitmap;
using PixelFormat = Avalonia.Platform.PixelFormat;

namespace SnipTranslator.MVVM.Extensions;
public static class BitmapExtension
{
    // # of graphics card adapter
    const int numAdapter = 0;

    // # of output device (i.e. monitor)
    const int numOutput = 0;
    
    // Create DXGI Factory1
    private static Lazy<Adapter1> _adapter = new(() =>
    {
        var factory = new Factory1();
        return factory.GetAdapter1(numAdapter);
    });

    private static Lazy<Device> _device = new(() =>
    {
        return new Device(_adapter.Value);
    });

    private static Lazy<OutputDuplication> _outputDuplicate = new(() =>
    {
        // Get DXGI.Output
        var output = _adapter.Value.GetOutput(numOutput);
        return output.QueryInterface<Output1>().DuplicateOutput(_device.Value);
    });
    
    public static Pix ToPixImage(this Bitmap bitmap) => AvaloniaBitmapToPixConverter.Convert(bitmap);

    public static Bitmap? TakeScreenShot(Screen screen)
    {
        Bitmap? captured = null;

        var device = _device.Value;
        
        // Duplicate the output
        var duplicatedOutput = _outputDuplicate.Value;

        // Width/Height of desktop to capture
        int width = screen.Bounds.Width;
        int height = screen.Bounds.Height;

        // Create Staging texture CPU-accessible
        var textureDesc = new Texture2DDescription
        {
            CpuAccessFlags = CpuAccessFlags.Read,
            BindFlags = BindFlags.None,
            Format = Format.B8G8R8A8_UNorm,
            Width = width,
            Height = height,
            OptionFlags = ResourceOptionFlags.None,
            MipLevels = 1,
            ArraySize = 1,
            SampleDescription = { Count = 1, Quality = 0 },
            Usage = ResourceUsage.Staging
        };
        var screenTexture = new Texture2D(device, textureDesc);


        bool captureDone = false;
        for (int i = 0; !captureDone; i++)
        {
            try
            {
                SharpDX.DXGI.Resource screenResource;
                OutputDuplicateFrameInformation duplicateFrameInformation;

                // Try to get duplicated frame within given time
                duplicatedOutput.AcquireNextFrame(10000, out duplicateFrameInformation, out screenResource);

                if (i > 0)
                {
                    // copy resource into memory that can be accessed by the CPU
                    using (var screenTexture2D = screenResource.QueryInterface<Texture2D>()) 
                        device.ImmediateContext.CopyResource(screenTexture2D, screenTexture);

                    // Get the desktop capture texture
                    var mapSource = device.ImmediateContext.MapSubresource(screenTexture, 0, MapMode.Read, MapFlags.None);

                    // Create Drawing.Bitmap
                    var bitmap = new SKBitmap(width, height, SKColorType.Bgra8888, SKAlphaType.Opaque);
                    // Copy pixels from screen capture Texture to GDI bitmap
                    // var mapDest = bitmap.LockBits(boundsRect, ImageLockMode.WriteOnly, bitmap.PixelFormat);
                    var sourcePtr = mapSource.DataPointer;
                    var destPtr = bitmap.GetPixels();
                    for (int y = 0; y < height; y++)
                    {
                        // Copy a single line 
                        Utilities.CopyMemory(destPtr, sourcePtr, width * 4);

                        // Advance pointers
                        sourcePtr = IntPtr.Add(sourcePtr, mapSource.RowPitch);
                        destPtr = IntPtr.Add(destPtr, bitmap.RowBytes);
                    }

                    // Release source and dest locks
                    // bitmap.UnlockBits(mapDest);
                    device.ImmediateContext.UnmapSubresource(screenTexture, 0);

                    // Save the output
                    captured = bitmap.ToAvaloniaBitmap();
                    // Capture done
                    captureDone = true;
                }
                    

                screenResource.Dispose();
                duplicatedOutput.ReleaseFrame();

            }
            catch (SharpDXException e)
            {
                Debug.WriteLine($"Exception: {e.Message}");
                if (e.ResultCode.Code != SharpDX.DXGI.ResultCode.WaitTimeout.Result.Code)
                {
                    throw e;
                }
            }
        }
        return captured;
    }
    
    
    public static Bitmap ToAvaloniaBitmap(this SKBitmap skBitmap)
    {
        using (var stream = new MemoryStream())
        {
            skBitmap.Encode(stream, SKEncodedImageFormat.Png, 100);
            stream.Position = 0;
            return new Bitmap(stream);
        }
    }
}