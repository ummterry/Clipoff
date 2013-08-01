using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Diagnostics;

namespace Clipoff
{
    class ScreenShot : IDisposable
    {
        public ScreenShot(Bitmap image)
        {
            this.ScreenImage = image;
        }

        private Bitmap image;
        public Bitmap ScreenImage
        {
            get { return image; }
            private set
            {
                if (image != null)
                {
                    image.Dispose();
                }
                image = value;
                if (value != null)
                {
                    if (value.PixelFormat.Equals(PixelFormat.Format8bppIndexed) || value.PixelFormat.Equals(PixelFormat.Format1bppIndexed) || value.PixelFormat.Equals(PixelFormat.Format4bppIndexed))
                    {
                        try
                        {
                            // Default Bitmap PixelFormat is Format32bppArgb
                            this.image = new Bitmap(value);
                        }
                        finally
                        {
                            // Always dispose, even when a exception occured
                            value.Dispose();
                        }
                    }
                }
            }
        }
    
        public double Width
        {
            get
            {
                return image == null ? -1 : image.Size.Width;
            }
        }
        public double Height
        {
            get
            {
                return image == null ? -1 : image.Size.Height;
            }
        }

        public bool SaveImage(int x1, int y1, int x2, int y2, String filePath)
        {
            try
            {
                using (Bitmap bmp = image.Clone(new Rectangle(x1, y1, x2 - x1, y2 - y1), PixelFormat.Format32bppArgb))
                {
                    ImageFormat imageFormat = ImageFormat.Png;
                    if (filePath.EndsWith("*.jpg"))
                    {
                        imageFormat = ImageFormat.Jpeg;
                    }
                    else if (filePath.EndsWith("*.bmp"))
                    {
                        imageFormat = ImageFormat.Bmp;
                    }
                    bmp.Save(filePath, imageFormat);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        ~ScreenShot() {
			Dispose(false);
		}

		/// <summary>
		/// The public accessible Dispose
		/// Will call the GarbageCollector to SuppressFinalize, preventing being cleaned twice
		/// </summary>
		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// This Dispose is called from the Dispose and the Destructor.
		/// When disposing==true all non-managed resources should be freed too!
		/// </summary>
		/// <param name="disposing"></param>
		protected virtual void Dispose(bool disposing) {
			if (disposing) {
				if (image != null) {
					image.Dispose();
				}
			}
			image = null;
		}

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int BitBlt(IntPtr hDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, int dwRop);
        public static ScreenShot CreateScreenShot()
        {
            try
            {
                Bitmap bitmap = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, PixelFormat.Format32bppArgb);
                //using (Graphics g = Graphics.FromImage(bitmap))
                //{
                //    g.CopyFromScreen(Screen.PrimaryScreen.Bounds.X, Screen.PrimaryScreen.Bounds.Y, 0, 0,
                //        Screen.PrimaryScreen.Bounds.Size, CopyPixelOperation.SourceCopy);
                //}
                using (Graphics gdest = Graphics.FromImage(bitmap))
                {
                    using (Graphics gsrc = Graphics.FromHwnd(IntPtr.Zero))
                    {
                        IntPtr hSrcDC = gsrc.GetHdc();
                        IntPtr hDC = gdest.GetHdc();
                        int retval = BitBlt(hDC, 0, 0, bitmap.Width, bitmap.Height, hSrcDC, 0, 0, (int)CopyPixelOperation.SourceCopy);
                        gdest.ReleaseHdc();
                        gsrc.ReleaseHdc();
                    }
                }
                return new ScreenShot(bitmap);
            }
            catch { }
            return new ScreenShot(null);
        }


    }
}
