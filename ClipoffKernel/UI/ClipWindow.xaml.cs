using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Clipoff.UI
{
    /// <summary>
    /// Interaction logic for ClipWindow.xaml
    /// </summary>
    public partial class ClipWindow : Window
    {
        public ClipWindow()
        {
            InitializeComponent();
            InitializeScreen();
        }

        enum CaptureState
        {
            UnCaptured,
            Capturing,
            Captured
        }
        private CaptureState captureState = CaptureState.UnCaptured;
        private Point startPoint, endPoint, upLeftPoint, downRightPoint;
        private Rectangle upRect, downRect, leftRect, rightRect, selectedRect;
        private ToolBar toolBar;
        private ScreenShot screenShot;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Topmost = true;
            this.InitializeBackground();
        }

        public String SavedPicturePath;
        public bool SaveCapture(String path = null)
        {
            path = (path == null) ? Config.ImagePath : path;
            if (this.downRightPoint.X > this.upLeftPoint.X && this.downRightPoint.Y > this.upLeftPoint.Y)
            {
                if (screenShot.SaveImage(
                    Convert.ToInt32(this.upLeftPoint.X),
                    Convert.ToInt32(this.upLeftPoint.Y),
                    Convert.ToInt32(this.downRightPoint.X),
                    Convert.ToInt32(this.downRightPoint.Y),
                    path))
                {
                    this.SavedPicturePath = path;
                    return true;
                }
            }
            return false;
        }

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);
        private void InitializeScreen()
        {
            screenShot = ScreenShot.CreateScreenShot();
            if (screenShot.ScreenImage != null)
            {
                IntPtr hBitmap = screenShot.ScreenImage.GetHbitmap();
                try
                {
                    ImageBrush brush = new System.Windows.Media.ImageBrush(
                        System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                            hBitmap,
                            IntPtr.Zero,
                            System.Windows.Int32Rect.Empty,
                            System.Windows.Media.Imaging.BitmapSizeOptions.FromWidthAndHeight(screenShot.ScreenImage.Width, screenShot.ScreenImage.Height)
                        )
                    );
                    if (brush != null)
                    {
                        this.Background = brush;
                    }
                }
                catch { }
                finally
                {
                    DeleteObject(hBitmap);
                }

                //this is better than setting the windows mode as maximized,
                //in the latter case the window size is ridiculously bigger than the screen size
                if (screenShot.Width > 0 && screenShot.Height > 0)
                {
                    this.Width = screenShot.Width;
                    this.Height = screenShot.Height;
                }
            }
        }

        private void InitializeBackground()
        {
            List<Rectangle> recList = new List<Rectangle>();
            for (int i = 0; i < 4; i++)
            {
                recList.Add(
                    new Rectangle
                    {
                        Width = this.ActualWidth,
                        Height = this.ActualHeight,
                        StrokeThickness = 0,
                        Fill = new SolidColorBrush(Color.FromArgb(100, 0, 0, 0))
                        {
                            Opacity = 0.5
                        }
                    }
                );
                Canvas.SetLeft(recList[i], i * recList[i].Width);
                Canvas.SetTop(recList[i], i * recList[i].Height);
            }
            this.canvas.Children.Add(upRect = recList[0]);
            this.canvas.Children.Add(downRect = recList[1]);
            this.canvas.Children.Add(leftRect = recList[2]);
            this.canvas.Children.Add(rightRect = recList[3]);
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.captureState == CaptureState.Capturing)
            {
                this.endPoint = Mouse.GetPosition(null);
                this.SetRects();   
            }
        }

        private void SetRects()
        {
            this.upLeftPoint = new Point(Math.Min(startPoint.X, endPoint.X), Math.Min(startPoint.Y, endPoint.Y));
            this.downRightPoint = new Point(Math.Max(startPoint.X, endPoint.X), Math.Max(startPoint.Y, endPoint.Y));
            this.SetRect(this.selectedRect, this.upLeftPoint.X, this.upLeftPoint.Y, 
                this.downRightPoint.X - this.upLeftPoint.X, this.downRightPoint.Y - this.upLeftPoint.Y);
            this.SetRect(this.upRect, 0, 0, this.ActualWidth, this.upLeftPoint.Y);
            this.SetRect(this.downRect, 0, this.downRightPoint.Y, this.ActualWidth, this.ActualHeight - this.downRightPoint.Y);
            this.SetRect(this.leftRect, 0, this.upLeftPoint.Y, this.upLeftPoint.X, this.downRightPoint.Y - this.upLeftPoint.Y);
            this.SetRect(this.rightRect, this.downRightPoint.X, this.upLeftPoint.Y,
                this.ActualWidth - this.downRightPoint.X, this.downRightPoint.Y - this.upLeftPoint.Y);
        }
        private void SetRect(Rectangle rect, double x, double y, double width, double height)
        {
            rect.Width = width;
            rect.Height = height;
            Canvas.SetLeft(rect, x);
            Canvas.SetTop(rect, y);
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this.captureState == CaptureState.UnCaptured
                ||this.captureState == CaptureState.Captured)
            {
                this.canvas.Children.Remove(this.selectedRect);
                this.canvas.Children.Remove(this.toolBar);
                this.startPoint = Mouse.GetPosition(null);
                this.selectedRect = new Rectangle
                {
                    Stroke = Brushes.PowderBlue,
                    StrokeThickness = 1,
                    Opacity = 1,
                    Fill = new SolidColorBrush()
                    {
                        Opacity = 0
                    }
                };
                Canvas.SetLeft(this.selectedRect, this.startPoint.X);
                Canvas.SetTop(this.selectedRect, this.startPoint.Y);
                this.canvas.Children.Add(this.selectedRect);
                this.captureState = CaptureState.Capturing;
            }
        }

        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (this.captureState == CaptureState.Capturing)
            {
                this.endPoint = Mouse.GetPosition(null);
                this.captureState = CaptureState.Captured;
                this.ShowToolBar();
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (this.captureState == CaptureState.Captured && e.Key == Key.Enter && this.toolBar != null)
            {
                this.toolBar.Post();
            }
            else if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }


        private void ShowToolBar()
        {
            this.toolBar = new ToolBar(this);

            this.LocateToolBar();

            this.canvas.Children.Add(toolBar);
        }

        public void LocateToolBar()
        {
            if (this.startPoint.X > this.endPoint.X && this.startPoint.Y > this.endPoint.Y)
            {
                //up left
                if (this.upLeftPoint.X > toolBar.Width)
                {
                    //left side
                    LocateToolBar(this.upLeftPoint.X - toolBar.Width, this.upLeftPoint.Y);
                }
                else if(this.upLeftPoint.Y > toolBar.Height)
                {
                    //top side
                    LocateToolBar(this.upLeftPoint.X, this.upLeftPoint.Y - toolBar.Height);
                }
                else if (CanFitToolBar)
                {
                    //inside
                    LocateToolBar(this.upLeftPoint.X, this.upLeftPoint.Y);
                }
                else
                {
                    LocateToolBarToRightBottom();
                }
            }
            else if (this.startPoint.X > this.endPoint.X && this.startPoint.Y < this.endPoint.Y)
            {
                //down left
                if (this.downRightPoint.Y + toolBar.Height < this.ActualHeight)
                {
                    //down side
                    LocateToolBar(this.upLeftPoint.X, this.downRightPoint.Y);
                }
                else if (this.upLeftPoint.X > toolBar.Width)
                {
                    //left side
                    LocateToolBar(this.upLeftPoint.X - toolBar.Width, this.downRightPoint.Y - toolBar.Height);
                }
                else if (CanFitToolBar)
                {
                    //inside
                    LocateToolBar(this.upLeftPoint.X, this.downRightPoint.Y - toolBar.Height);
                }
                else
                {
                    LocateToolBarToRightBottom();
                }
            }
            else if (this.startPoint.X < this.endPoint.X && this.startPoint.Y > this.endPoint.Y)
            {
                //up right
                if (this.downRightPoint.X + toolBar.Width < this.ActualWidth)
                {
                    //right side
                    LocateToolBar(this.downRightPoint.X, this.upLeftPoint.Y);
                }
                else if (this.upLeftPoint.Y > toolBar.Height)
                {
                    LocateToolBar(this.downRightPoint.X - toolBar.Width, this.upLeftPoint.Y - toolBar.Height);
                }
                else if (CanFitToolBar)
                {
                    LocateToolBar(this.downRightPoint.X - toolBar.Width, this.upLeftPoint.Y);
                }
                else
                {
                    LocateToolBarToRightBottom();
                }
            }
            else
            {
                //down right
                LocateToolBarToRightBottom();
            }
        }

        void LocateToolBarToRightBottom()
        {
            if (this.downRightPoint.Y + toolBar.Height < this.ActualHeight)
            {
                //down side
                LocateToolBar(this.downRightPoint.X - this.toolBar.Width, this.downRightPoint.Y);
            }
            else if (this.downRightPoint.X + toolBar.Width < this.ActualWidth)
            {
                //right side
                LocateToolBar(this.downRightPoint.X, this.downRightPoint.Y - this.toolBar.Height);
            }
            else if (CanFitToolBar)
            {
                //inside
                LocateToolBar(this.downRightPoint.X - this.toolBar.Width, this.downRightPoint.Y - this.toolBar.Height);
            }
            else
            {
                if (this.upLeftPoint.Y > this.toolBar.Height)
                {
                    //top of top right
                    LocateToolBar(this.downRightPoint.X - this.toolBar.Width, this.upLeftPoint.Y - this.toolBar.Height);
                }
                else if (this.upLeftPoint.X > this.toolBar.Width)
                {
                    //left of down left
                    LocateToolBar(this.upLeftPoint.X - this.toolBar.Width, this.downRightPoint.Y - this.toolBar.Height);
                }
                //ELSE (0,0)
            }
        }

        void LocateToolBar(double x, double y)
        {
            if (x < 0) x = 0;
            if (y < 0) y = 0;
            if (x + toolBar.Width > this.ActualWidth) x = this.ActualWidth - toolBar.Width;
            if (y + toolBar.Height > this.ActualHeight) y = this.ActualHeight - toolBar.Height;
            Canvas.SetLeft(toolBar, x);
            Canvas.SetTop(toolBar, y);
        }

        bool CanFitToolBar
        {
            get
            {
                return (this.selectedRect.ActualWidth > this.toolBar.Width
                    && this.selectedRect.ActualHeight > this.toolBar.Height);
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            screenShot.Dispose();
            screenShot = null;
            App.Instance.TryExit();
        }

    }

}
