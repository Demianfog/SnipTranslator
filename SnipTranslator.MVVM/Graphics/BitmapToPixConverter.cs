using System;
using System.IO;
using Avalonia.Media.Imaging;
using SkiaSharp;
using Tesseract;

namespace SnipTranslator.MVVM.Graphics;
  public static class AvaloniaBitmapToPixConverter
  {
    public static Pix Convert(Bitmap img)
    {
      SKBitmap skBitmap;
      using (var stream = new MemoryStream())
      {
        img.Save(stream);
        stream.Seek(0, SeekOrigin.Begin);
        skBitmap = SKBitmap.Decode(stream);
      }

      int pixDepth = 32;
      Pix pix = Pix.Create((int)img.Size.Width, (int)img.Size.Height, pixDepth);
      pix.XRes = (int) Math.Round(img.Dpi.X);
      pix.YRes = (int) Math.Round(img.Dpi.Y);
      try
      {
        PixData data = pix.GetData();
        TransferDataFormat32bppArgb(skBitmap, data);
        return pix;
      }
      catch (Exception ex)
      {
        pix.Dispose();
        throw;
      }
      finally
      {
        skBitmap.Dispose();
      }
    }
    
    private static unsafe void TransferDataFormat32bppArgb(SKBitmap imgData, PixData pixData)
    {
      int height = imgData.Height;
      int width = imgData.Width;
      for (int index1 = 0; index1 < height; ++index1)
      {
        byte* numPtr1 = (byte*) ((IntPtr) (void*) imgData.GetPixels() + index1 * imgData.RowBytes);
        uint* data = (uint*) ((IntPtr) (void*) pixData.Data + (IntPtr) (index1 * pixData.WordsPerLine) * 4);
        for (int index2 = 0; index2 < width; ++index2)
        {
          byte* numPtr2 = numPtr1 + (index2 << 2);
          byte blue = *numPtr2;
          byte green = numPtr2[1];
          byte red = numPtr2[2];
          byte alpha = numPtr2[3];
          PixData.SetDataFourByte(data, index2, BitmapHelper.EncodeAsRGBA(red, green, blue, alpha));
        }
      }
    }
  }
