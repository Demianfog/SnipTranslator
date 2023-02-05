using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace SnipTranslator.MVVM.Extensions;

// public static class ScreenshotHelper
// {
//     public static Bitmap GetScreenshotFullScreen()
//     {
//         Rect screen = GetScreenSize();
//         return GetScreenshotOfWindow(screen);
//     }
//     
//     private static Bitmap GetScreenshotOfWindow(Rect rect)
//     {
//         Bitmap bmp = new Bitmap(rect.right - rect.left, rect.bottom - rect.top);
//         System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp);
//         g.CopyFromScreen(rect.left, rect.top, 0, 0, bmp.Size);
//         return bmp;
//     }
// }