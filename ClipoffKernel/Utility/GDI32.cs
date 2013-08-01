﻿using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security;
using Microsoft.Win32.SafeHandles;

namespace Clipoff
{
    public static class GDIExtensions
    {
        public static bool AreRectangleCornersVisisble(this Region region, Rectangle rectangle)
        {
            Point topLeft = new Point(rectangle.X, rectangle.Y);
            Point topRight = new Point(rectangle.X + rectangle.Width, rectangle.Y);
            Point bottomLeft = new Point(rectangle.X, rectangle.Y + rectangle.Height);
            Point bottomRight = new Point(rectangle.X + rectangle.Width, rectangle.Y + rectangle.Height);
            bool topLeftVisible = region.IsVisible(topLeft);
            bool topRightVisible = region.IsVisible(topRight);
            bool bottomLeftVisible = region.IsVisible(bottomLeft);
            bool bottomRightVisible = region.IsVisible(bottomRight);

            return topLeftVisible && topRightVisible && bottomLeftVisible && bottomRightVisible;
        }
    }

    /// <summary>
    /// A hbitmap SafeHandle implementation
    /// </summary>
    public class SafeHBitmapHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        [SecurityCritical]
        private SafeHBitmapHandle()
            : base(true)
        {
        }

        [SecurityCritical]
        public SafeHBitmapHandle(IntPtr preexistingHandle)
            : base(true)
        {
            SetHandle(preexistingHandle);
        }

        protected override bool ReleaseHandle()
        {
            return GDI32.DeleteObject(handle);
        }
    }

    /// <summary>
    /// GDI32 Helpers
    /// </summary>
    public static class GDI32
    {
        [DllImport("gdi32", SetLastError = true)]
        public static extern bool BitBlt(IntPtr hObject, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hObjectSource, int nXSrc, int nYSrc, CopyPixelOperation dwRop);
        [DllImport("gdi32", SetLastError = true)]
        public static extern bool StretchBlt(IntPtr hdcDest, int nXOriginDest, int nYOriginDest, int nWidthDest, int nHeightDest, IntPtr hdcSrc, int nXOriginSrc, int nYOriginSrc, int nWidthSrc, int nHeightSrc, CopyPixelOperation dwRop);
        [DllImport("gdi32", SetLastError = true)]
        public static extern IntPtr CreateCompatibleDC(IntPtr hDC);
        [DllImport("gdi32", SetLastError = true)]
        public static extern bool DeleteDC(IntPtr hDC);
        [DllImport("gdi32", SetLastError = true)]
        public static extern bool DeleteObject(IntPtr hObject);
        [DllImport("gdi32", SetLastError = true)]
        public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);
        [DllImport("gdi32", SetLastError = true)]
        public static extern IntPtr CreateDIBSection(IntPtr hdc, ref BitmapInfoHeader bmi, uint Usage, out IntPtr bits, IntPtr hSection, uint dwOffset);
        [DllImport("gdi32", SetLastError = true)]
        public static extern IntPtr CreateRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect);
        [DllImport("gdi32", SetLastError = true)]
        public static extern int GetClipBox(IntPtr hdc, out RECT lprc);
        [DllImport("gdi32", SetLastError = true)]
        public static extern uint GetPixel(IntPtr hdc, int nXPos, int nYPos);
        [DllImport("gdi32", SetLastError = true)]
        public static extern IntPtr CreateRoundRectRgn(int x1, int y1, int x2, int y2, int cx, int cy);
        [DllImport("gdi32", SetLastError = true)]
        public static extern int GetDeviceCaps(IntPtr hdc, DeviceCaps nIndex);

        /// <summary>
        /// Doesn't work?
        /// </summary>
        /// <param name="target"></param>
        /// <param name="source"></param>
        public static void StretchBlt(this Graphics target, Bitmap sourceBitmap, Rectangle source, Rectangle destination)
        {
            IntPtr hDCSrc = IntPtr.Zero;
            IntPtr hDCDest = IntPtr.Zero;
            try
            {
                hDCDest = target.GetHdc();
                hDCSrc = CreateCompatibleDC(hDCDest);
                using (SafeHBitmapHandle hBitmapHandle = new SafeHBitmapHandle(sourceBitmap.GetHbitmap()))
                {
                    IntPtr pOrig = SelectObject(hDCSrc, hBitmapHandle.DangerousGetHandle());
                    StretchBlt(hDCDest, destination.X, destination.Y, destination.Width, destination.Height, hDCSrc, source.Left, source.Top, source.Width, source.Height, CopyPixelOperation.SourceCopy);
                    IntPtr pNew = SelectObject(hDCDest, pOrig);
                }
            }
            finally
            {
                if (hDCSrc != IntPtr.Zero)
                {
                    DeleteDC(hDCSrc);
                }
                if (hDCDest != IntPtr.Zero)
                {
                    target.ReleaseHdc(hDCDest);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="source"></param>
        public static void BitBlt(this Graphics target, Bitmap sourceBitmap, Rectangle source, Point destination)
        {
            IntPtr hDCSrc = IntPtr.Zero;
            IntPtr hDCDest = IntPtr.Zero;
            try
            {
                hDCDest = target.GetHdc();
                hDCSrc = CreateCompatibleDC(hDCDest);
                using (SafeHBitmapHandle hBitmapHandle = new SafeHBitmapHandle(sourceBitmap.GetHbitmap()))
                {
                    IntPtr pOrig = SelectObject(hDCSrc, hBitmapHandle.DangerousGetHandle());
                    BitBlt(hDCDest, destination.X, destination.Y, source.Width, source.Height, hDCSrc, source.Left, source.Top, CopyPixelOperation.SourceCopy);
                    IntPtr pNew = SelectObject(hDCDest, pOrig);
                }
            }
            finally
            {
                if (hDCSrc != IntPtr.Zero)
                {
                    DeleteDC(hDCSrc);
                }
                if (hDCDest != IntPtr.Zero)
                {
                    target.ReleaseHdc(hDCDest);
                }
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public struct BitmapFileHeader
    {
        public static readonly short BM = 0x4d42; // BM
        public short bfType;
        public int bfSize;
        public short bfReserved1;
        public short bfReserved2;
        public int bfOffBits;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct BitmapInfoHeader
    {
        public uint biSize;
        public int biWidth;
        public int biHeight;
        public short biPlanes;
        public short biBitCount;
        public uint biCompression;
        public uint biSizeImage;
        public int biXPelsPerMeter;
        public int biYPelsPerMeter;
        public uint biClrUsed;
        public int biClrImportant;

        private const int BI_RGB = 0;	//Das Bitmap ist nicht komprimiert
        private const int BI_RLE8 = 1; //Das Bitmap ist komprimiert (Für 8-Bit Bitmaps)
        private const int BI_RLE4 = 2; //Das Bitmap ist komprimiert (Für 4-Bit Bitmaps)
        private const int BI_BITFIELDS = 3; //Das Bitmap ist nicht komprimiert. Die Farbtabelle enthält 
        public const int DIB_RGB_COLORS = 0;

        public BitmapInfoHeader(int width, int height, short bpp)
        {
            biSize = (uint)Marshal.SizeOf(typeof(BitmapInfoHeader));	// BITMAPINFOHEADER is 40 bytes
            biPlanes = 1;	// Should allways be 1
            biCompression = BI_RGB;
            biWidth = width;
            biHeight = height;
            biBitCount = bpp;
            biSizeImage = 0;
            biXPelsPerMeter = 0;
            biYPelsPerMeter = 0;
            biClrUsed = 0;
            biClrImportant = 0;
        }
    }
}
