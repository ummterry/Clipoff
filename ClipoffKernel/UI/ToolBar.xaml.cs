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
using System.Threading;
using Clipoff.Utility;

namespace Clipoff.UI
{
    /// <summary>
    /// Interaction logic for ToolBar.xaml
    /// </summary>
    public partial class ToolBar : UserControl
    {
        public ToolBar(ClipWindow clipWindow)
        {
            this.clipWindow = clipWindow;
            InitializeComponent();
            InitializeButtons();
        }

        ClipWindow clipWindow;
        PostBar postBar;
        int buttonCount = 2, selectedCount = 0;
        Dictionary<Button, bool> clickState = new Dictionary<Button, bool>();
        Dictionary<Button, Type> postDic = new Dictionary<Button, Type>();
        Button btnFacebook, btnTwitter, btnWeibo, btnRenren;
        bool hasPostBar = false;
        

        public void Post()
        {
            if (postBar == null)
            {
                this.SaveAs();
                return;
            }
            string message = postBar.Message;
            if (this.clipWindow.SaveCapture())
            {
                int count = 0;
                foreach (Button button in clickState.Keys)
                {
                    if (clickState[button])
                    {
                        count++;
                    }
                }
                App.Instance.TaskCount = count;
                this.clipWindow.Close();
                foreach (Button button in clickState.Keys)
                {
                    if (clickState[button])
                    {
                        ((PostBase)System.Activator.CreateInstance(postDic[button], this.clipWindow.SavedPicturePath, message)).Post();
                    }
                }
            }
            else
            {
                App.Instance.AddError(Config.SAVE_ERROR);
            }
            App.Instance.TryExit();
        }

        void InitializeButtons()
        {
            if (Config.UseFacebook)
            {
                this.buttonPanel.Children.Add(this.btnFacebook = InitializeButton("facebook", typeof(FacebookUtility)));
                this.clickState.Add(this.btnFacebook, false);
                buttonCount += 1;
            }
            if (Config.UseTwitter)
            {
                this.buttonPanel.Children.Add(this.btnTwitter = InitializeButton("twitter", typeof(TwitterUtility)));
                this.clickState.Add(this.btnTwitter, false);
                buttonCount += 1;
            }
            if (Config.UseWeibo)
            {
                this.buttonPanel.Children.Add(this.btnWeibo = InitializeButton("weibo", typeof(WeiboUtility)));
                this.clickState.Add(this.btnWeibo, false);
                buttonCount += 1;
            }
            if (Config.UseRenren)
            {
                this.buttonPanel.Children.Add(this.btnRenren = InitializeButton("renren", typeof(RenrenUtility)));
                this.clickState.Add(this.btnRenren, false);
                buttonCount += 1;
            }
        }

        Button InitializeButton(string imageName, Type utilityType)
        {
            Color c = Color.FromArgb(0, 0, 0, 0);
            Button button = new Button()
            {
                Width = 50,
                Height = 50,
                Margin = new Thickness(0, 0, 0, 0),
                Background = Brushes.Transparent,
                BorderBrush = Brushes.Transparent,
                BorderThickness = new Thickness(0, 0, 0, 0),
                Padding = new Thickness(-2),
                Content = new Image()
                {
                    Source = new BitmapImage(new Uri(String.Format("/Images/{0}.png", imageName), UriKind.Relative)),
                    Stretch = Stretch.Fill
                }
            };
            button.Click += this.Button_Click_Post;
            this.postDic.Add(button, utilityType);
            return button;
        }

        public new double Width
        {
            get
            {
                return 50 * buttonCount;
            }
        }
        public new double Height
        {
            get
            {
                return 50 + (hasPostBar ? 110 : 0);
            }
        }


        private void Button_Click_Save(object sender, RoutedEventArgs e)
        {
            this.SaveAs();
        }

        void SaveAs()
        {
            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            saveFileDialog.Title = "Save As";
            saveFileDialog.Filter = "JPeg Image (*.jpg)|*.jpg|Bitmap Image (*.bmp)|*.bmp|PNG Image (*.png)|*.png";
            saveFileDialog.FileName = "Clipoff Picture";
            if (saveFileDialog.ShowDialog() == true)
            {
                if (this.clipWindow.SaveCapture(saveFileDialog.FileName))
                {
                    //ClipWindow should do something else
                    this.clipWindow.Close();
                }
                else
                {
                    this.clipWindow.Close();
                    App.Instance.AddError(Config.SAVE_ERROR);
                }
            }
        }

        private void Button_Click_Cancel(object sender, RoutedEventArgs e)
        {
            this.clipWindow.Close();
        }

        private void Button_Click_Post(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            if (this.clickState[button])
            {
                this.clickState[button] = false;
                this.selectedCount--;
                button.Background = Brushes.Transparent;
                if (this.selectedCount == 0)
                {
                    this.contentGrid.Children.Remove(postBar);
                    this.hasPostBar = false;
                }
            }
            else
            {
                this.clickState[button] = true;
                this.selectedCount++;
                button.Background = Brushes.Magenta;
                if(this.selectedCount == 1)
                {
                    if (this.postBar == null)
                    {
                        this.postBar = new PostBar(this);
                        Grid.SetRow(postBar, 1);
                    }
                    this.contentGrid.Children.Add(postBar);
                    this.hasPostBar = true;
                    this.clipWindow.LocateToolBar();
                }
            }
        }

        //private void PostFacebook()
        //{
        //    if (this.clipWindow.SaveCapture())
        //    {
        //        this.clipWindow.Close();
        //        (new FacebookUtility(this.clipWindow.SavedPicturePath)).Post();
        //    }
        //    else
        //    {
        //        App.Instance.AddError(Config.SAVE_ERROR);
        //    }
        //}

        //private void PostTwitter()
        //{
        //    if (this.clipWindow.SaveCapture())
        //    {
        //        this.clipWindow.Close();
        //        (new TwitterUtility(this.clipWindow.SavedPicturePath)).Post();
        //    }
        //    else
        //    {
        //        this.clipWindow.Close();
        //        App.Instance.AddError(Config.SAVE_ERROR);
        //    }
        //}

        //private void PostWeibo()
        //{
        //    if (this.clipWindow.SaveCapture())
        //    {
        //        this.clipWindow.Close();
        //        (new WeiboUtility(this.clipWindow.SavedPicturePath)).Post();
        //    }
        //    else
        //    {
        //        App.Instance.AddError(Config.SAVE_ERROR);
        //    }
        //}

        //private void PostRenren()
        //{
        //    if (this.clipWindow.SaveCapture())
        //    {
        //        this.clipWindow.Close();
        //        (new RenrenUtility(this.clipWindow.SavedPicturePath)).Post();
        //    }
        //    else
        //    {
        //        App.Instance.AddError(Config.SAVE_ERROR);
        //    }
        //}

    }

}
